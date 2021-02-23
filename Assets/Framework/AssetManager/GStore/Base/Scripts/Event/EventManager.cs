using System.Collections.Generic;
using UnityEngine;
/**
 * 全局事件库
 * 
 * 注意：注册了全局事件，需要相应的安全处理事件释放
 * 
 * 这个类注册的都是delegate，在lua中，相应用function实现就可以了
 * 
 * lua里注册func的写法：
 * 
 * --args参数列表(是lua的table类型，下标从1开始访问)
 * function XXX(args)

	local param1 = args[1]
    local param2 = args[2]
    --...

   end

  注意：以上如果注册在Lua文件里，则所有uiHandler调用的func是同一个引用； 
  如果想独立func引用，请把func定义在init内部，如：
  function OnInit( uiHandler )
       function XXX(args)

            local param1 = args[1]
            local param2 = args[2]
            --...

       end

        uiHandler.AddEventListener(eventId,xxx);

      //--这种方式不用考虑eventRemove，因为ui离开会自动remove所注册的event
      //当然你也可以手动在生命周期内调用uiHander.RemoveEventListener(eventId)
   end

   2017-1-18
   现在LuaDelegate是WeakReference，随时可能被回收，
   这会导致EventManager.Instance:AddEventListener在ui.OnInit & ui.OnClose时，2个方法地址不同了，从而event无法回收！
   所以，
   =>Lua文件里，尽量用uiHandler.AddEventListener(eventId,xxx);
     不要用EventManager.Instance:AddEventListener(eventId,xxx);
*/
namespace GStore
{
    public class EventManager : Singleton<EventManager>
    {

        public delegate void EventReceiver(int id, params object[] param);

        public EventReceiver eventReceiver = null;
        /// <summary>
        /// 事件实体
        /// </summary>
        public class EventEntity
        {
            public EventManager.EventDelegate eventDelegate;

            public object uimanager;

            public EventEntity(EventManager.EventDelegate _func, object uimanager)
            {
                this.eventDelegate = _func;
                this.uimanager = uimanager;
            }
        }

        /// <summary>
        /// 委托
        /// </summary>
        /// <param name="args"></param>
        public delegate void EventDelegate(params object[] args);

        /// <summary>
        /// 事件集合
        /// </summary>
        private Dictionary<int, List<EventEntity>> event_dic = new Dictionary<int, List<EventEntity>>();


        #region 对外接口
        /// <summary>
        /// 添加一个事件监听
        /// </summary>
        /// <param name="_event_id">eventId, see@EventDefine.cs</param>
        /// <param name="_func">Event delegate / lua event func </param>
        public void AddEventListener(int _event_id, EventDelegate _func)
        {
            AddEventListenerImpl(_event_id, _func, null);
        }

        /// <summary>
        /// 通过指定一个func名称删除一个事件监听
        /// </summary>
        /// <param name="_event_id">eventId, see@EventDefine.cs</param>
        /// <param name="_func">Event delegate / lua event func </param>
        public void RemoveEventListener(int _event_id, EventDelegate _func)
        {
            RemoveEventListenerImpl(_event_id, _func, null);
        }

        public bool IsHasEventListener(int _event_id)
        {
            if (event_dic.ContainsKey(_event_id))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 通过指定一个事件id删除所有监听
        /// </summary>
        /// <param name="_event_id"></param>
        public void RemoveAllEventListener(int _event_id)
        {
            if (event_dic.ContainsKey(_event_id) == false)
            {
                return;
            }

            List<EventEntity> _eventlist = event_dic[_event_id];

            if (_eventlist == null)
            {
                return;
            }
            _eventlist.Clear();
        }

        /// <summary>
        /// 事件通知
        /// </summary>
        /// <param name="_event_id">eventId,see@EventDefine.cs</param>
        /// <param name="_args">事件的参数</param>
        public void NotifyEvent(int _event_id, params object[] _args)
        {
            if (event_dic.ContainsKey(_event_id))
            {
                List<EventEntity> _eventlist = event_dic[_event_id];
                if (_eventlist == null)
                {
                    return;
                }

                for (int i = _eventlist.Count - 1; i >= 0; i--)
                {
                    EventEntity eventEntity = _eventlist[i];
                    if (eventEntity == null)
                    {
                        continue;
                    }

                    EventDelegate _delegate = eventEntity.eventDelegate;
                    if (_delegate != null)
                    {
                        _delegate(_args);
                    }
                }
            }

            //         if (LuaEventHandler.IsEventRegister(_event_id))
            //         {
            //             LuaEventHandler.HandlerEvent(_event_id, _args);
            //         }

            if (eventReceiver != null)
                eventReceiver(_event_id, _args);

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_event_id"></param>
        /// <param name="_event_func"></param>
        /// <param name="uiHandler"></param>
        private void AddEventListenerImpl(int _event_id, EventDelegate _event_func, object uiHandler)
        {
            List<EventEntity> _event_list = null;

            if (event_dic.ContainsKey(_event_id))
            {
                _event_list = event_dic[_event_id];
            }
            else
            {
                _event_list = new List<EventEntity>();
                event_dic[_event_id] = _event_list;
            }

            if (_event_list == null)
            {
                return;
            }

            if (IsHasEventDelegate(_event_list, _event_func, uiHandler))
            {
                Debug.LogError("重复添加事件监听 eventId=" + _event_id + ",eventFunc=" + _event_func + ",uiHandler=" + uiHandler);
                return;
            }

            _event_list.Add(new EventEntity(_event_func, uiHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventFunc"></param>
        /// <param name="uiHandler"></param>
        private void RemoveEventListenerImpl(int eventId, EventDelegate eventFunc, object uiHandler)
        {
            if (event_dic.ContainsKey(eventId) == false)
            {
                return;
            }

            List<EventEntity> _eventlist = event_dic[eventId];

            if (_eventlist == null)
            {
                return;
            }

            RemoveEventDelegate(eventId, _eventlist, eventFunc, uiHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventlist"></param>
        /// <param name="eventFunc"></param>
        /// <param name="uiHandler"></param>
        /// <returns></returns>
        private bool IsHasEventDelegate(List<EventEntity> eventlist, EventDelegate eventDelegate, object uimanager)
        {
            if (eventlist == null)
            {
                return false;
            }

            for (int i = 0; i < eventlist.Count; i++)
            {
                EventEntity eventEntity = eventlist[i];
                if (eventEntity == null)
                {
                    continue;
                }

                //如果传入uiHander不为null，则需要比对uiHandler
                if (uimanager != null)
                {
                    if (eventEntity.uimanager != uimanager)
                    {
                        continue;
                    }
                }

                if (eventEntity.eventDelegate == eventDelegate)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventlist"></param>
        /// <param name="eventFunc"></param>
        /// <param name="uiHandler"></param>
        /// <returns></returns>
        private bool RemoveEventDelegate(int eventId, List<EventEntity> eventlist, EventDelegate eventFunc, object uiHandler)
        {
            if (eventlist == null)
            {
                return false;
            }

            for (int i = (eventlist.Count - 1); i >= 0; i--)
            {
                EventEntity eventEntity = eventlist[i];
                if (eventEntity == null)
                {
                    continue;
                }

                //如果传入uiHander不为null，则需要比对uiHandler
                if (uiHandler != null)
                {
                    if (eventEntity.uimanager != uiHandler)
                    {
                        continue;
                    }
                }

                if (eventEntity.eventDelegate == eventFunc)
                {
                    eventlist.RemoveAt(i);
                    return true;
                }
            }

            Debug.LogError("要删除的事件监听不存在eventId= " + eventId + ",eventFunc=" + ((eventFunc == null) ? "null" : "" + eventFunc.GetHashCode()) +
                                                ",_eventlist.count=" + eventlist.Count + ",uiHandler=" + uiHandler);
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 打印相关
        /// </summary>
        public void LogRegisterEvents()
        {
            Dictionary<int, List<EventEntity>>.Enumerator enumerator = event_dic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, List<EventEntity>> keyPair = enumerator.Current;
                List<EventEntity> _eventlist = keyPair.Value;
                int eventId = keyPair.Key;

                if (_eventlist == null)
                {
                    continue;
                }
                for (int i = _eventlist.Count - 1; i >= 0; i--)
                {
                    EventEntity eventEntity = _eventlist[i];
                    if (eventEntity == null)
                    {
                        continue;
                    }
                    EventDelegate _dele = eventEntity.eventDelegate;
                    if (_dele != null)
                    {
                        Debug.LogWarning("[事件注册]-" + eventId + ":" + _dele.GetHashCode());
                    }
                }
            }

        }
#endif
    }
}