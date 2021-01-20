using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameReplay
{
    FrameSyncMgr frameManager = null;
    public FrameReplay(FrameSyncMgr _frameManager)
    {
        this.frameManager = _frameManager;
    }
}
