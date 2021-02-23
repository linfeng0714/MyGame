using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 关于深度图优化方案的说明：
/// 该方案不适合大量透明shader的场景
/// 该方案无法与移动设备HDR共存
/// 所有需要获取深度的非透明物体shader都必须进行一次深度计算并填充到alpha通道
/// 如果有其他处在AfterForwardOpaque之中或之前的CommandBuffer，可能会与该方案冲突
/// </summary>

/// <summary>
/// 注册深度图索取接口，用来判断当前是否需要生成深度图
/// 需要使用优化方案深度图的类需要实现该接口
/// </summary>
public interface IDepthMapContacter
{
    bool IsDepthReqOn();
}

/// <summary>
/// 静态的全局开关集合委托
/// 需要控制开关的类将控制方法挂在静态委托上即可
/// </summary>
public static class DepthWriteGlobal
{
    //public delegate bool getDepthWriteSwitch();
    private static System.Func<bool> s_global_switch;

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="_switch"></param>
    public static void RegisterSwitch(System.Func<bool> _switch)
    {
        s_global_switch += _switch;
    }

    /// <summary>
    /// 反注册
    /// </summary>
    /// <param name="_switch"></param>
    public static void UnregisterSwitch(System.Func<bool> _switch)
    {
        s_global_switch -= _switch;
    }

    /// <summary>
    /// 是否使用深度度
    /// 返回false就没法使用优化深度贴图
    /// </summary>
    /// <returns></returns>
    public static bool IsNeedDepth()
    {
        if (s_global_switch == null)
        {
            Debug.LogError("DepthWriteGlobal.s_global_switch！组件全局开关未初始化！");
            return false;
        }
        bool _flag = true;
        var _all_call = s_global_switch.GetInvocationList();
        if (_all_call != null)
        {
            for (int i = 0; i < _all_call.Length; i++)
            {
                System.Func<bool> _call = _all_call[i] as System.Func<bool>;
                if (_call != null)
                {
                    _flag = _call.Invoke();
                    if (!_flag)
                    {
                        break;
                    }
                }
            }
        }
        return _flag;
    }
}

/// <summary>
/// 镜头深度图获取
/// </summary>
public class CameraDepthCatcher : MonoBehaviour
{
    static class Uniforms
    {
        internal static readonly int _WriteDepth = Shader.PropertyToID("_WriteDepth");
        internal static readonly int _DepthTexture = Shader.PropertyToID("_DepthTexture");
        //internal static readonly RenderTargetIdentifier _DepthTextureRTI = _DepthTexture;
    }


    /// <summary>
    /// 开启CameraDepthCatcher的Shader Key
    /// </summary>
    public const string DEPTH_CATCH_KEY = "DepthWrite_ON";

    /// <summary>
    /// 写入深度所在的CameraEvent
    /// </summary>
    private const CameraEvent ENABLE_DEPTH_WRITE_EVENT = CameraEvent.BeforeForwardOpaque;

    /// <summary>
    /// 获取深度图所在的CameraEvent
    /// </summary>
    private const CameraEvent CATCH_DEPTH_EVENT = CameraEvent.AfterForwardOpaque;

    /// <summary>
    /// 是否有任意组件需求DepthMap
    /// </summary>
    public bool isContacted;

    /// <summary>
    /// 镜头
    /// </summary>
    private Camera target_camera;

    /// <summary>
    /// 需求Depthmap的组件
    /// </summary>
    private List<IDepthMapContacter> contacters = new List<IDepthMapContacter>();

    /// <summary>
    /// 是否开始Update
    /// </summary>
    private bool is_started;

    /// <summary>
    /// 开始写入深度所用的CommandBuffer
    /// </summary>
    private CommandBuffer enable_depth_buffer;

    /// <summary>
    /// 获取深度图所用的CommandBuffer
    /// </summary>
    private CommandBuffer depth_buffer;

    /// <summary>
    /// 抽取出Alpha通道的material
    /// </summary>
    private Material read_alpha_mat;

    /// <summary>
    /// 深度贴图
    /// </summary>
    private RenderTexture depth_texture;

    /// <summary>
    /// 深度贴图描述
    /// </summary>
    private RenderTextureDescriptor depth_texture_desc;


#if UNITY_EDITOR
    /// <summary>
    /// debug用材质和标记变量
    /// </summary>
    public bool debug = false;
    Material debug_mat;
#endif

    /// <summary>
    /// 开始获取深度
    /// </summary>
    public void StartWork()
    {
        is_started = true;
    }

    /// <summary>
    /// 深度获取是否已经初始化
    /// </summary>
    /// <returns></returns>
    public bool IsInited()
    {
        return target_camera != null;
    }

    /// <summary>
    /// 是否开启深度写入
    /// 返回false就没法使用优化深度贴图
    /// </summary>
    /// <returns></returns>
    public bool IsCustomDepth()
    {
        return CheckSwitchList();
    }

    /// <summary>
    /// 设置开关
    /// 返回false就没法使用优化深度贴图
    /// </summary>
    /// <returns></returns>
    private bool CheckSwitchList()
    {
        if(target_camera == null)
        {
            return false;
        }
        // HDR模式下无法开启优化深度贴图
        return !target_camera.allowHDR || DepthWriteGlobal.IsNeedDepth();
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public void Destroy()
    {
        StopCatchDepth();
        contacters.Clear();
        is_started = false;
        isContacted = false;
        target_camera = null;
        UnityEngine.Object.Destroy(read_alpha_mat);
        read_alpha_mat = null;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_camera"></param
    public void Init(Camera _camera)
    {
        target_camera = _camera;
        isContacted = false;
        read_alpha_mat = new Material(Shader.Find("MobileFastDepthMap/ReadFromAlpha"));

        // 初始化desc
        GetRTDescriptor(ref depth_texture_desc);

        //开启深度写入
        contacters.Clear();
    }

    /// <summary>
    /// 关联深度处理
    /// </summary>
    /// <param name="_contacter"></param>
    public void ContactDepthMap(IDepthMapContacter _contacter)
    {
        if (!contacters.Contains(_contacter))
        {
            contacters.Add(_contacter);
        }
    }

    /// <summary>
    /// 取消关联深度处理
    /// </summary>
    /// <param name="_contacter"></param>
    public void DisContactDepthMap(IDepthMapContacter _contacter)
    {
        contacters.Remove(_contacter);
    }


    #region RenderTexture

    /// <summary>
    /// 获得RenderTexture的Descriptor
    /// 暂时不用这个方法，因为这个方法创建的Texture会上下翻转
    /// </summary>
    /// <param name="depthBufferBits"></param>
    /// <param name="colorFormat"></param>
    /// <param name="readWrite"></param>
    /// <returns></returns>
    private RenderTextureDescriptor GetRTDescriptor(ref RenderTextureDescriptor _des)
    {
        _des = new RenderTextureDescriptor();
        _des.bindMS = false;
        _des.autoGenerateMips = false;
        _des.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        _des.enableRandomWrite = false;
        _des.memoryless = RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA;
        _des.msaaSamples = 1;
        _des.shadowSamplingMode = UnityEngine.Rendering.ShadowSamplingMode.None;
        _des.sRGB = true;
        _des.useMipMap = false;
        _des.volumeDepth = 1;
        _des.vrUsage = VRTextureUsage.None;
        //_des.colorFormat = _format;
        //_des.depthBufferBits = _depth;
        //_des.width = _width;
        //_des.height = _height;
        return _des;
    }

    #endregion // RenderTexture

    #region Enable Depth Write CommandBuffer


    /// <summary>
    /// 创建或获得Enable Depth Write的CommandBuffer
    /// </summary>
    /// <returns></returns>
    private CommandBuffer CreateOrGetEnableDepthWriteCommandBuffer()
    {
        if (target_camera == null)
        {
            return null;
        }
        if (enable_depth_buffer == null)
        {
            enable_depth_buffer = new CommandBuffer { name = "EnableDepthWrite" };
            target_camera.AddCommandBuffer(ENABLE_DEPTH_WRITE_EVENT, enable_depth_buffer);
        }
        return enable_depth_buffer;
    }

    /// <summary>
    /// 删除Enable Depth Write的CommandBuffer
    /// </summary>
    private void RemoveEnableDepthWriteCommandBuffer()
    {
        if (target_camera != null)
        {
            if (enable_depth_buffer != null)
            {
                target_camera.RemoveCommandBuffer(ENABLE_DEPTH_WRITE_EVENT, enable_depth_buffer);
                enable_depth_buffer.Dispose();
                enable_depth_buffer = null;
            }
        }
    }

    /// <summary>
    /// 清空Enable Depth Write的CommandBuffer
    /// </summary>
    private void ClearEnableDepthWriteCommandBuffer()
    {
        if (enable_depth_buffer != null)
        {
            enable_depth_buffer.Clear();
        }
    }

    /// <summary>
    /// 生成Enable Depth Write的CommandBuffer
    /// </summary>
    /// <param name="_cb"></param>
    private void PopulateEnableDepthWriteCommandBuffer(CommandBuffer _cb)
    {
        if (target_camera == null)
        {
            return;
        }

        _cb.SetGlobalFloat(Uniforms._WriteDepth, 1);
    }

    #endregion // Enable Depth Write CommandBuffer

    #region Depth CommandBuffer

    /// <summary>
    /// 创建或获得Depth的CommandBuffer
    /// </summary>
    /// <returns></returns>
    private CommandBuffer CreateOrGetDepthCommandBuffer()
    {
        if(target_camera == null)
        {
            return null;
        }
        if (depth_buffer == null)
        {
            depth_buffer = new CommandBuffer { name = "GetDepthTex" };
            target_camera.AddCommandBuffer(CATCH_DEPTH_EVENT, depth_buffer);
        }
        return depth_buffer;
    }

    /// <summary>
    /// 删除Enable Depth Write的CommandBuffer
    /// </summary>
    private void RemoveDepthCommandBuffer()
    {
        if (target_camera != null)
        {
            if (depth_buffer != null)
            {
                target_camera.RemoveCommandBuffer(CATCH_DEPTH_EVENT, depth_buffer);
                depth_buffer.Dispose();
                depth_buffer = null;
            }
        }
    }

    /// <summary>
    /// 清空Depth的CommandBuffer
    /// </summary>
    private void ClearDepthCommandBuffer()
    {
        if(depth_buffer != null)
        {
            depth_buffer.Clear();
        }
    }

    /// <summary>
    /// 生成Depth的CommandBuffer
    /// </summary>
    /// <param name="_cb"></param>
    private void PopulateDepthCommandBuffer(CommandBuffer _cb)
    {
        if(target_camera == null)
        {
            return;
        }
        //depth_texture_desc.colorFormat = RenderTextureFormat.R8;
        //depth_texture_desc.width = target_camera.pixelWidth;
        //depth_texture_desc.height = target_camera.pixelHeight;
        //depth_texture_desc.depthBufferBits = 0;

        // 每次不一样的RT，防止分辨率改变
        if(depth_texture != null)
        {
            RenderTexture.ReleaseTemporary(depth_texture);
            depth_texture = null;
        }
        //depth_texture = RenderTexture.GetTemporary(depth_texture_desc);
        depth_texture = RenderTexture.GetTemporary(target_camera.pixelWidth, target_camera.pixelHeight, 0, RenderTextureFormat.R8);

        // 取出深度
        _cb.Blit(BuiltinRenderTextureType.CameraTarget, depth_texture, read_alpha_mat);
        _cb.SetGlobalTexture(Uniforms._DepthTexture, depth_texture);
        // 取消标志
        _cb.SetGlobalFloat(Uniforms._WriteDepth, 0);
    }

    #endregion // Depth CommandBuffer

    #region Mono

    void OnDisable()
    {
        StopCatchDepth();
    }

    /// <summary>
    /// 准备渲染
    /// </summary>
    void OnPreRender()
    {
        if(!is_started)
        {
            return;
        }

        // 检测是否开启
        DepthUpdate();

        if (depth_buffer != null)
        {
            // 先清空获取depth CommandBuffer
            depth_buffer.Clear();
            // 创建cb
            PopulateDepthCommandBuffer(depth_buffer);
        }
    }

    /// <summary>
    /// Unity Update()
    /// </summary>
    //private void Update()
    //{
    //    if (is_started)
    //    {
    //        DepthUpdate();
    //    }
    //}

    #endregion // Mono

    /// <summary>
    /// 更新深度
    /// </summary>
    private void DepthUpdate()
    {
        if (target_camera != null)
        {
            isContacted = CheckIfContacted();
            if (isContacted && CheckSwitchList())
            {
                //Debug.Log("StartCatch");
                StartCatchDepth();
            }
            else
            {
                //isRun = false;
                StopCatchDepth();
            }
        }
    }

    /// <summary>
    /// 判断当前是否存在关联组件需要深度
    /// </summary>
    /// <returns></returns>
    private bool CheckIfContacted()
    {
        for(int i = 0; i < contacters.Count; i++)
        {
            if (contacters[i].IsDepthReqOn())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断Buffer是否存在
    /// </summary>
    /// <returns></returns>
    //private bool IsBufferExist(string _targetBufferName, CameraEvent _cameraEvent)
    //{
    //    CommandBuffer[] _buffers = target_camera.GetCommandBuffers(_cameraEvent);
    //    bool _flag = false;
    //    for (int i = 0; i < _buffers.Length; i++)
    //    {
    //        if (_buffers[i].name == _targetBufferName)
    //        {
    //            _flag = true;
    //            break;
    //        }
    //    }
    //    return _flag;
    //}

    /// <summary>
    /// 往target_camera加入CommandBuffer
    /// 检查是否存在，不存在就添加，否则跳过
    /// </summary>
    /// <param name="_cameraEvent"><param name="_commandBuffer"></param> 
    //private void AddBuffer(CameraEvent _cameraEvent, CommandBuffer _commandBuffer)
    //{
    //    if (!IsBufferExist(_commandBuffer.name, _cameraEvent))
    //    {
    //        target_camera.AddCommandBuffer(_cameraEvent, _commandBuffer);
    //    }
    //}

    /// <summary>
    /// 开始获取深度
    /// </summary>
    private void StartCatchDepth()
    {
        target_camera.depthTextureMode = DepthTextureMode.None;

        // 使用者Keyword
        Shader.EnableKeyword(DEPTH_CATCH_KEY);
        // 创建并加入Camera
        var _cb = CreateOrGetEnableDepthWriteCommandBuffer();
        if (_cb != null)
        {
            _cb.Clear();
            PopulateEnableDepthWriteCommandBuffer(_cb);
        }
        // 创建并加入Camera
        CreateOrGetDepthCommandBuffer();
        // Depth需要每帧创建
    }

    /// <summary>
    /// 停止获取深度
    /// </summary>
    private void StopCatchDepth()
    {
        // 使用者Keyword
        Shader.DisableKeyword(DEPTH_CATCH_KEY);
        // 从Camera删除
        RemoveEnableDepthWriteCommandBuffer();
        // 从Camera删除
        RemoveDepthCommandBuffer();
        // 确保已关闭
        Shader.SetGlobalFloat(Uniforms._WriteDepth, 0);

        // 释放RT
        if(depth_texture != null)
        {
            RenderTexture.ReleaseTemporary(depth_texture);
            depth_texture = null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Debug用方法，通过public的debug来控制是否将深度图直接渲染到摄像机
    /// </summary>
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (debug)
        {
            if (debug_mat == null)
            {
                debug_mat = new Material(Shader.Find("MobileFastDepthMap/DebugDepth"));
            }
            Graphics.Blit(source, destination, debug_mat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
#endif
}
