using Framework.Base;
using pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FrameCommand : PooledClass
{
    public enum FrameCommandType
    {
        None = 0,

        Move,
        KeyState,
        UseSkill,
        Talk,
        Custom,

        OffLine = 15,  //服务器指令

    }
    public uint PlayerNum;

    public uint Param;
    public uint SeqIdx;

    public uint sendTick;
    public uint recvTick;
    public uint runTick;
    public int FrameId;

    public abstract FrameCommandType CmdType{ get; }

    protected abstract void OnExecute(string message);

    public void ExecCommand()
    {
        ExecFrameCommand();
        Release();
    }

    private void ExecFrameCommand()
    {
#if FRAME_LOG
            FrameLogger.LogFilter("FrameCommand", " time", FrameSyncMgr.Instance.LogicFrameTick, PlayerNum/*, Seq*/, CmdType, Param);
#endif
        //游戏结束
        //if (BattleMgr.Instance.HasSendEnd)
        //    return;
        //Hero hero = GetHero();
        //自己的角色
        //if (UnitHelper.SelfHero != null && hero == UnitHelper.SelfHero)
        //{
        //    this.runTick = (uint)FrameSyncMgr.Instance.GetFrameSegOffset();
        //    FrameSyncMgr.Instance.frameSegDelay = runTick - sendTick;
        //}
        //GLogger.Log("[toby] FrameCommand sendTick:" + sendTick+"("+(sendTick%3)+")" + ", recvTick:" + recvTick + ", runTick:" + runTick + "(" + (runTick % 3) + ")" + ",diff_r-s:"+(recvTick-sendTick)+", diff all:"+ (runTick-sendTick) );
#if FRAME_LOG
            FrameLogger.Log("FrameCommand", hero == null ? "NULL" : hero.UserId.ToString());
#endif
        OnExecute(string.Empty);
    }

    public void Send()
    {
        SendCmd();
    }

    private void SendCmd()
    {
        //if (!CanSend()) return;
        //if (UnitHelper.SelfHero == null) return;
        //PlayerNum = UnitHelper.SelfHero.Number;
        ////sendTick = (uint)FrameSyncMgr.Instance.GetFrameSegOffset();

            SendNetCmd();
            Release();
        //}
        //else
        //{
        //   FrameSyncMgr.Instance.AddFrameData(this);
        //}
    }
    private void SendNetCmd()
    {
        //if (!FrameSyncMgr.Instance.IsActive) return;
        FrameId = Time.frameCount;
        //发送消息
        //NetSendCmd<C2SFrameCommand_2000> cmd = NetSendCmdPool<C2SFrameCommand_2000>.Get(NetOpcode.FRAME_COMMAND);
        //cmd.SendData.CmdType = (uint)CmdType;
        //cmd.SendData.PlayerNum = PlayerNum;
        //cmd.SendData.Param = Param;
        //cmd.SendData.FrameIdx = ((sendTick & 0xffff) << 16);

        //cmd.Send();


#if FRAME_LOG
            FrameLogger.LogFilter("SendCommand", " time", FrameSyncMgr.Instance.LogicFrameTick, PlayerNum/*, cmd.SendData.Seq*/, CmdType, Param);
#endif
    }

    public static FrameCommand CreateCommand(int frameId , C2SFrameCommand_2000 cmdData)
    {
        if (cmdData == null)
        {
            Debug.LogError("cmdData is null");
        }
        FrameCommand cmd = null;
        uint CmdType = cmdData.CmdType;
        //switch ((FrameCommandType)CmdType)
        //{
        //    case FrameCommandType.Move:
        //        cmd = ClassPool<MoveDirectionCommand>.Get();
        //        break;
        //    case FrameCommandType.KeyState:
        //        cmd = ClassPool<KeyStateCommand>.Get();
        //        break;
        //    case FrameCommandType.Talk:
        //        cmd = ClassPool<TalkCommand>.Get();
        //        break;
        //    case FrameCommandType.UseSkill:
        //        cmd = ClassPool<UseSkillCommand>.Get();
        //        break;
        //    case FrameCommandType.Custom:
        //        cmd = ClassPool<CustomCommand>.Get();
        //        break;
        //    case FrameCommandType.OffLine:
        //        cmd = ClassPool<OffLineCommand>.Get();
        //        break;
        //    default:
        //        cmd = ClassPool<NullCommand>.Get();
        //        break;
        //}
        cmd.FrameId = frameId;
        cmd.PlayerNum = cmdData.PlayerNum;
        cmd.Param = cmdData.Param;

        cmd.SeqIdx = (cmdData.FrameIdx & 0x0000ffff);
        cmd.sendTick = (cmdData.FrameIdx & 0xffff0000) >> 16;
        //cmd.recvTick = (uint)FrameSyncMgr.Instance.GetFrameSegOffset();
        return cmd;
    }

    public class NullCommand : FrameCommand
    {
        public override FrameCommandType CmdType
        {
            get
            {
                return FrameCommandType.None;
            }
        }

        protected override void OnExecute(string message)
        {

        }
    }
}
