using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSyncMgr : SingletonMono<FrameSyncMgr>
{
    public bool bEnaleUDP = false;
    //帧数据接收类
    public FrameWindow frameWindow { get; private set; }

    public FrameReconnection reconnectAgent { get; private set; }

    public FrameReplay replayAgent { get; private set; }
    //可执行帧数据队列
    public Queue<FrameCommand> frameQueue = new Queue<FrameCommand>();

    public int SvrFrameDelta = 66;// 服务器一帧的时长(单位：毫秒)
    public int SvrEnableFrameSeg = 0;               // 启用帧刻度逻辑
    public const int FixUpdateFrameDelta = 33;      //客户端逻辑帧时长(单位：毫秒)
    public int frameSegIdx = 0;//每SvrFrameDelta中, FixUpdateFrameDelta分段的刻度
    public uint frameSegDelay = 0;
    public bool IsActive;//当前是否为帧同步执行方式
    /// <summary>
    /// 帧间隔时间
    /// </summary>
    public int FrameDeltaTime
    {
        get
        {
            return (int)(FixUpdateFrameDelta * _frameSpeed);
        }
    }
    /// <summary>
    /// 帧间隔时间
    /// </summary>
    public int UnscaledFrameDeltaTime
    {
        get
        {
            return FixUpdateFrameDelta;
        }
    }

    /// <summary>
    /// 当前帧同步是否是活动中
    /// </summary>
    public bool IsRunning = false;
    /// <summary>
    /// 平均帧延时（同时也是追赶算法里，帧缓冲区大小）
    /// </summary>
    public int AvgFrameDelay = 0;
    /// <summary>
    /// 帧开始执行的时间（单位是相对于Time.realtimeSinceStartup）
    /// 在UpdateFrame里使用
    /// </summary>
    public float StartFrameTime = 0;
    /// <summary>
    /// 当前是否正在追赶
    /// </summary>
    public bool IsChasingFrame = false;
    /// <summary>
    /// UDP是否已经连接上
    /// </summary>
    public bool IsConnectingUDP = false;
    /// <summary>
    /// 服务器返回结束帧
    /// </summary>
    public int EndFrameIndex = 0;

    private float nMultiFrameDelta = 0;
    private int nJitterDelay = 0;

    public const float MAX_SPEED = 8f;
    public const float MIN_SPEED = 0f;

    //--------------需要同步的字段
    private float _frameSpeed = 1;
    private int logicFrameMillisRemainTime = 0;
    /// <summary>
    /// 当前连续的可执行服务器帧位置
    /// </summary>
    public int SvrFrameIndex = 0;
    /// <summary>
    /// 当前已经执行的服务器帧位置
    /// </summary>
    public int CurFrameIndex = 0;
    /// <summary>
    /// 已经执行了多少服务器完整帧时间（单位：毫秒）
    /// </summary>
    public long LogicFrameTick = 0;
    /// <summary>
    /// 当前逻辑帧位置
    /// </summary>
    public int FixUpdateFrameIndex = 0;
    /// <summary>
    /// 当前帧执行的逻辑帧更新次数（统计追帧次数）
    /// </summary>
    public int LogicFrameUpdateTimes;
    //---------------

    /// <summary>
    /// 战斗的播放速度
    /// </summary>
    public float FrameSpeed
    {
        get
        {
            return this._frameSpeed;
        }
        set
        {
            if (_frameSpeed != value)
            {
                _frameSpeed = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED);
                Time.timeScale = _frameSpeed;
                nMultiFrameDelta *= _frameSpeed;
            }
        }
    }

    protected override void _OnAwake()
    {
        base._OnAwake();
        DontDestroyOnLoad(gameObject);
        frameWindow = new FrameWindow(this);
        reconnectAgent = new FrameReconnection(this);
        replayAgent = new FrameReplay(this);
    }

    protected override void _OnDestroy()
    {
        base._OnDestroy();
        //DisConnectUDP();//关闭udp连接
    }




}
