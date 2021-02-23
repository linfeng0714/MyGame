using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Framework.ThreadManager
{
    public sealed class ThreadManager : Singleton<ThreadManager>
    {
        private readonly Queue<ITask> mTasks = new Queue<ITask>();
        private readonly List<Worker> mWorkers = new List<Worker>();

       

        public ThreadManager()
        {
            Initializer();
        }

        public void ReleaseData()
        {
            Uninitialze();
        }

        private void Initializer()
        {
            if(mWorkers.Count == 0)
            {
                int proCount = UnityEngine.SystemInfo.processorCount;
                proCount = Math.Max(1, proCount);
                mWorkers.Capacity = proCount;
                for(int i=0;i<proCount;i++)
                {
                    Worker worker = new Worker();
                    worker.Init(this);
                    mWorkers.Add(worker);
                }
            }
        }

        private void Uninitialze()
        {
            if(mWorkers.Count > 0)
            {
                for (int i = 0; i < mWorkers.Count; ++i)
                {
                    mWorkers[i].Uninit();
                }
                mWorkers.Clear();
            }
            lock(mTasks)
            {
                mTasks.Clear();
            }
        }

        private class Worker
        {
            private bool mBusy = false;

            public bool IsBusy { get { return mBusy; } }
            private Thread mThread = null;
            private ManualResetEvent mStopEvent = null;
            private ManualResetEvent mPauseEvent = null;
            private ThreadManager mThreadManager = null;

            public void Init(ThreadManager manager)
            {
                if (mThread == null)
                {
                    mBusy = false;
                    mThreadManager = manager;
                    mStopEvent = new ManualResetEvent(false);
                    mPauseEvent = new ManualResetEvent(true);
                    mThread = new Thread(new ThreadStart(this.Proc));
                    mThread.IsBackground = true;
                    mThread.Start();
                }
            }

            public void Uninit()
            {
                if (mThread == null)
                    return;
                mStopEvent.Set();
                mPauseEvent.Set();
                mThread.Join();
                mThread.Abort();
                mThread = null;
                mBusy = false;
            }

            public void Awake()
            {
                mPauseEvent.Set();
            }

            private void OnPause()
            {
                mPauseEvent.Reset();
            }

            private void Run(ITask task)
            {
                try
                {
                    if(task != null)
                    {
                        mBusy = true;
                        task.Execute();
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(string.Format("message:{0},stack trace:{1}", e.Message, e.StackTrace));
                    task.OnException(e);
                }
                finally
                {
                    mBusy = false;
                }
            }

            private void Proc()
            {
                while (true)
                {
                    mPauseEvent.WaitOne();

                    if (mStopEvent.WaitOne(0))
                        break;

                    ITask task = null;
                    //if(mThreadManager.tr)
                }
            }
        }
    }   
}
