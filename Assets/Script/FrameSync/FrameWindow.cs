using pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameWindow
{
    FrameSyncMgr frameManager;
    public FrameWindow(FrameSyncMgr frameManager)
    {
        this.frameManager = frameManager;
        this.Reset();
    }

    private const int FRAME_WIN_LEN = 900;              // 帧接收数组的长度
    private const int MAX_TIMEOUT_TIMES = 5;            // 最大超时请求次数
    private const int MAX_REQUEST_COUNT = 300;          // 最大允许请求个数
    private const int BEGIN_FRAME_INDEX = 0;            // 服务器帧数据开始帧id（世界3从0开始，王者荣耀从1开始）
    private const int TIMEOUT_FRAME_OFFSET = 1;
    private const int TIMEOUT_FRAME_STEP = 3;//曲线系数（坡度）

    private S2CServerFrameUpdate_2001[] _receiveWindow;
    private int _baseFrameId;//_receiveWindow数组的起点帧id
    private int _beginFrameId;//当前已经提交到执行的连续帧id
    private int _maxFrameId;//当前收到服务器的最大帧id
    //重发相关变量
    private int _repairBeginFrameId;
    private float _repairCounter;
    private int _repairTimes;

    //超时相关变量
    private int _aliveFrameCount;
    private float _timeoutCounter;
    private int _timeoutTimes;
    private float _deltaTime;

    public void Reset()
    {
        this._receiveWindow = new S2CServerFrameUpdate_2001[FRAME_WIN_LEN];
        this._baseFrameId = 0;

        this._beginFrameId = BEGIN_FRAME_INDEX;
        this._maxFrameId = BEGIN_FRAME_INDEX - 1;

        this._repairCounter = 0;
        this._repairTimes = 0;

        this._timeoutCounter = 0;
        this._timeoutTimes = 0;

        this._aliveFrameCount = 0;

        this._deltaTime = 0;
    }

    // 根据frameId拿到该frame在数组里的位置
    private int FrqNoToWinIdx(int theFrameId)
    {
        return ((theFrameId - this._baseFrameId) % FRAME_WIN_LEN);
    }
}
