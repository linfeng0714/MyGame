using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ThreadManager : Singleton<ThreadManager>
{
    Thread thread;
    //Action excute = null;
    private static bool isRuning = false;

    static Queue<Action> events = new Queue<Action>();

    static readonly object m_lockObj = new object();
    public void Start()
    {
        UnityEngine.Debug.Log("start Thread");
        if(thread == null)
        {
            isRuning = true;
            thread = new Thread(DoAction);
            thread.Start();
            thread.IsBackground = true;
        }
    }

    public void CloseThread()
    {
        try
        {
            lock (m_lockObj)
            {
                isRuning = false;
            }

            if (thread != null)
            {
                thread.Join();
                thread.Abort();
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("thread error CloseThread " + ex.Message);
        }
        finally
        {
            thread = null;
        }
    }

    public void AddEvent(Action addExcute)
    {
        lock (m_lockObj)
        {
            events.Enqueue(addExcute);
        }
    }

    private void DoAction(object obj)
    {
        try
        {
            while(isRuning)
            {
                Thread.Sleep(20);
                lock(m_lockObj)
                {
                    if (events.Count > 0)
                    {
                        Action action = events.Dequeue();
                        if (action != null)
                        {
                            action();
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("thread error OnUpdate " + ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
