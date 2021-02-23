using UnityEngine;
using System.Collections.Generic;
using System;

namespace GStore
{
    public class Scheduler : SingletonMono<Scheduler>
    {
        public class ScheduleData
        {
            public Action<float> func;
            public float interval;
            public float eclipsed;
            public uint repeat;
            public uint count;
            public uint handle;
            public bool paused;
            public bool removed;
            public string info;
        }

        private Dictionary<uint, ScheduleData> handles = new Dictionary<uint, ScheduleData>();
        private Dictionary<uint, ScheduleData> delayJoins = new Dictionary<uint, ScheduleData>();
        private HashSet<uint> removed = new HashSet<uint>();

        private uint count = 0;
        private bool isRuning = false;
        private bool bClear = false;

        public void delayCallOnce(Action<float> f, float interval)
        {
            Action<float> func = (float delta) =>
            {
                f(delta);
            };

            registerSchedule(func, interval,1); 
        }

        public uint scheduleUpdate(Action<float> func, float interval, uint repeat = 0,string info = null)
        {
            return registerSchedule(func, interval, repeat, info);
        }

        public uint registerSchedule(Action<float> func, float interval, uint repeat = 0, string info = null)
        {
            count++;
            ScheduleData data = new ScheduleData();

            data.func = func;
            data.interval = interval;
            data.eclipsed = 0.0f;
            data.repeat = repeat;
            data.count = 0;
            data.handle = count;
            data.paused = false;
            data.removed = false;
            data.info = info;

            if (isRuning)
            {
                delayJoins.Add(count, data);
            }
            else
            {
                handles.Add(count, data);
            }

            return count;
        }

        public void removeSchedule(uint handle)
        {
            if (isRuning)
            {
                ScheduleData data;
                if (handles.TryGetValue(handle, out data))
                {
                    data.removed = true;
                }
                else if (delayJoins.TryGetValue(handle, out data))
                {
                    data.removed = true;
                }
            }
            else
            {
                handles.Remove(handle);
                delayJoins.Remove(handle);
            }
        }


        public void resume()
        {
            enabled = true;
        }

        public void pause()
        {
            enabled = false;
        }


        public void pauseHandle(uint handle)
        {
            ScheduleData data = null;

            if (!handles.TryGetValue(handle, out data))
                delayJoins.TryGetValue(handle, out data);

            if (data != null)
                data.paused = true;

        }

        public void resumeHandle(uint handle)
        {
            ScheduleData data = null;

            if (!handles.TryGetValue(handle, out data))
                delayJoins.TryGetValue(handle, out data);

            if (data != null)
                data.paused = false;
        }

        void Update()
        {

            if (bClear)
            {
                handles.Clear();
                delayJoins.Clear();
                bClear = false;
            }

            isRuning = true;

            foreach (KeyValuePair<uint, ScheduleData> pair in handles)
            {

                if (pair.Value.removed || pair.Value.paused)
                    continue;

                pair.Value.eclipsed += Time.deltaTime;

                if (pair.Value.eclipsed > pair.Value.interval)
                {
                    pair.Value.eclipsed -= pair.Value.interval;
                    pair.Value.count++;

                    pair.Value.func.Invoke(pair.Value.interval);
                }

                if (pair.Value.repeat > 0 && pair.Value.count >= pair.Value.repeat)
                    pair.Value.removed = true;
            }

            var handlesEnum = handles.GetEnumerator();

            while (handlesEnum.MoveNext())
            {
                var pair = handlesEnum.Current;

                if (pair.Value.removed)
                    removed.Add(pair.Key);
            }

            var removedEnum = removed.GetEnumerator();

            while (removedEnum.MoveNext())
            {
                var key = removedEnum.Current;

                handles.Remove(key);
            }

            var delayJoinsEnum = delayJoins.GetEnumerator();

            while (delayJoinsEnum.MoveNext())
            {
                var pair = delayJoinsEnum.Current;

                handles.Add(pair.Key, pair.Value);
            }

            removed.Clear();
            delayJoins.Clear();

            isRuning = false;
        }

        public void Clear()
        {
            bClear = true;
        }
    }
}
