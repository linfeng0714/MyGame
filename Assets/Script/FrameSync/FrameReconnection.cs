using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameReconnection 
{
    protected FrameSyncMgr frameManager;

    public FrameReconnection(FrameSyncMgr _manager)
    {
        this.frameManager = _manager;
    }
}
