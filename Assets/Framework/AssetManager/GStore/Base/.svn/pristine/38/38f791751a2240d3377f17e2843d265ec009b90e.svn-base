using System;
using UnityEngine;
using UnityEngine.Events;
using GStore;

public class EventUtil
{
    public enum PointerEvent
    {
        //
        // ժҪ:
        //     Intercepts a IPointerEnterHandler.OnPointerEnter.
        PointerEnter,
        //
        // ժҪ:
        //     Intercepts a IPointerExitHandler.OnPointerExit.
        PointerExit,
        //
        // ժҪ:
        //     Intercepts a IPointerDownHandler.OnPointerDown.
        PointerDown,
        //
        // ժҪ:
        //     Intercepts a IPointerUpHandler.OnPointerUp.
        PointerUp,
        //
        // ժҪ:
        //     Intercepts a IPointerClickHandler.OnPointerClick.
        PointerClick,
        //
        // ժҪ:
        //     Intercepts a IDragHandler.OnDrag.
        Drag,
        //
        // ժҪ:
        //     Intercepts a IDropHandler.OnDrop.
        Drop,
        //
        // ժҪ:
        //     Intercepts a IScrollHandler.OnScroll.
        Scroll,

        //
        // ժҪ:
        //     Intercepts a IDeselectHandler.OnDeselect.
        //Deselect = "Deselect",

        //
        // ժҪ:
        //     Intercepts a IMoveHandler.OnMove.
        //Move = "Move",

        //
        // ժҪ:
        //     Intercepts IInitializePotentialDrag.InitializePotentialDrag.
        InitializePotentialDrag,
        //
        // ժҪ:
        //     Intercepts IBeginDragHandler.OnBeginDrag.
        BeginDrag,
        //
        // ժҪ:
        //     Intercepts IEndDragHandler.OnEndDrag.
        EndDrag,

        //
        // ժҪ:
        //     Intercepts ICancelHandler.OnCancel.
        Cancel,
    }


    static Type t1 = typeof(UnityEvent);
    static Type t2 = typeof(UnityEvent<bool>);
    static Type t3 = typeof(UnityEvent<int>);
    static Type t4 = typeof(UnityEvent<Vector2>);
    static Type t5 = typeof(UnityEvent<float>);

    public delegate void UnityEventDelegate(params object[] aa);

    public static Action ListenUnityEvent(UnityEvent e, Action fun)
    {
        UnityAction act = () =>
        {
            fun();
        };

        e.AddListener(act);

        return () => { e.RemoveListener(act); };
    }


    public static Action ListenUnityEvent<T1>(UnityEvent<T1> e,Action<T1> fun)
    {
        UnityAction<T1> act = (p) =>
        {
            fun(p);
        };

        e.AddListener(act);

        return () => { e.RemoveListener(act); };
    }

    static public Action ListenMessage<T1>(int _event_id, Action<T1> _func)
    {
        EventManager.EventDelegate fnDelegate = (object[] objects) =>
        {
            _func((T1)objects[0]);
        };

        EventManager.Instance.AddEventListener(_event_id, fnDelegate);

        return () =>
        {
            EventManager.Instance.RemoveEventListener(_event_id, fnDelegate);
        };
    }

    static public Action ListenMessage<T1,T2>(int _event_id, Action<T1, T2> _func)
    {
        EventManager.EventDelegate fnDelegate = (object[] objects) =>
        {
            _func((T1)objects[0], (T2)objects[1]);
        };

        EventManager.Instance.AddEventListener(_event_id, fnDelegate);

        return () =>
        {
            EventManager.Instance.RemoveEventListener(_event_id, fnDelegate);
        };
    }

    static public Action ListenMessage<T1,T2,T3>(int _event_id, Action<T1, T2,T3> _func)
    {
        EventManager.EventDelegate fnDelegate = (object[] objects) =>
        {
            _func((T1)objects[0], (T2)objects[1], (T3)objects[2]);
        };

        EventManager.Instance.AddEventListener(_event_id, fnDelegate);

        return () =>
        {
            EventManager.Instance.RemoveEventListener(_event_id, fnDelegate);
        };
    }

    //static public void ListenNetEvent(int _event_id, OnHandleOneMessage _handler)
    //{
        //Service.Instance.LogicHandleMessage.RegisterMessage(_event_id, _handler);
    //}

    static public Action ListenPointerEvent(GameObject go, PointerEvent eventType,Action<GameObject,UnityEngine.EventSystems.BaseEventData> callback)
    {
        UIEventListener l = go.GetComponent<UIEventListener>();

        if (l != null)
        {
            UIEventListener.PointerEventDelegate fn = (_go, data) =>
            {
                callback(_go, data);
            };

            if (eventType == PointerEvent.PointerEnter)
            {
                if(l.onPointerEnter == null) 
                    l.onPointerEnter = fn;
                else
                    l.onPointerEnter += fn;

                return () => { l.onPointerEnter -= fn; };
            }
            else if(eventType == PointerEvent.PointerExit)
            {
                if (l.onPointerExit == null)
                    l.onPointerExit = fn;
                else
                    l.onPointerExit += fn;

                return () => { l.onPointerExit -= fn; };
            }
            else if (eventType == PointerEvent.PointerDown)
            {
                if (l.onPointerDown == null)
                    l.onPointerDown = fn;
                else
                    l.onPointerDown += fn;

                return () => { l.onPointerDown -= fn; };
            }
            else if (eventType == PointerEvent.PointerUp)
            {
                if (l.onPointerUp == null)
                    l.onPointerUp = fn;
                else
                    l.onPointerUp += fn;

                return () => { l.onPointerUp -= fn; };
            }
            else if (eventType == PointerEvent.PointerClick)
            {
                if (l.onPointerClick == null)
                    l.onPointerClick = fn;
                else
                    l.onPointerClick += fn;

                return () => { l.onPointerClick -= fn; };
            }
            else if (eventType == PointerEvent.Drag)
            {
                if (l.onDrag == null)
                    l.onDrag = fn;
                else
                    l.onDrag += fn;

                return () => { l.onDrag -= fn; };
            }
            else if (eventType == PointerEvent.Scroll)
            {
                if (l.onScroll == null)
                    l.onScroll = fn;
                else
                    l.onScroll += fn;

                return () => { l.onScroll -= fn; };
            }
            else if (eventType == PointerEvent.InitializePotentialDrag)
            {
                if (l.onInitializePotentialDrag == null)
                    l.onInitializePotentialDrag = fn;
                else
                    l.onInitializePotentialDrag += fn;

                return () => { l.onInitializePotentialDrag -= fn; };
            }
            else if (eventType == PointerEvent.BeginDrag)
            {
                if (l.onBeginDrag == null)
                    l.onBeginDrag = fn;
                else
                    l.onBeginDrag += fn;

                return () => { l.onBeginDrag -= fn; };
            }
            else if (eventType == PointerEvent.EndDrag)
            {
                if (l.onEndDrag == null)
                    l.onEndDrag = fn;
                else
                    l.onEndDrag += fn;

                return () => { l.onEndDrag -= fn; };
            }
            //else if (eventType == PointerEvent.Cancel)
            //{
            //    l.onCancel = fn;

            //    if (l.onCancel == null)
            //        l.onCancel = fn;
            //    else
            //        l.onCancel += fn;

            //    return () => { l.onCancel -= fn; };
            //} 
        }

        return null;
    }

    public delegate void EventArgs(params object[] param);

    static public void ListenDelegate<T>(ref T del, T del2)
    {
        object o = del;
        Delegate delo = (Delegate)o;

        object actiono = del2;
        Delegate action = (Delegate)actiono;

        Delegate d = Delegate.CreateDelegate(typeof(T), action.Target, action.Method);

        Delegate nd = Delegate.Combine(delo, d);

        o = nd;
        del = (T)o;
    }

//     static public void ListenDelegate<T>( ref T del, Delegate action)
//     {
//         ListenDelegate(ref del, action);
//     }

}