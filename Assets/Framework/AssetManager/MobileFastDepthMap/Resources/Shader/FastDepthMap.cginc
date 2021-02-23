#ifndef __FastDepthMap__
#define __FastDepthMap__

// 注意！！！注意！！！注意！！！注意！！！注意！！！
// 如果本文件被多重include引用，修改此文件后，必需Reimport所有引用此文件的shader，否则没有效果


// 深度计算库

// 渲染场景物体处理：
// 1.加入头文件：
// #include "这个文件"
// 2.在顶点处理加入计算深度距离:
// GSTORE_TRANSFER_DEPTH
// 3.在像素处理加入alpha计算输出：
// GSTORE_CALC_DEPTH_TO_ALPHA



#include "UnityCG.cginc"


// 在8bit精度内，以合适的精度取舍归一化，将距离转换为大部分后处理够用的深度图（第一部分，建议在写入8bit时使用）
inline fixed Distance2DepthPow0dot3_InLog_Step1(float distance) 
{
	return max(min((log(distance)) * 0.1, 1),0);
}

// 在8bit精度内，以合适的精度取舍归一化，将距离转换为大部分后处理够用的深度图（第二部分，建议在读取后微处理时使用。如果需要转换到unity深度区间，用POW(X, 3)处理一次）
inline fixed Distance2DepthPow0dot3_InLog_Step2(float depth)
{
	return min(depth + 0.02, 1);
}

// 作用同“Distance2DepthPow0dot3_InLog”方法，远距离效果略差于log，但是计算量更少（如果需要转换到unity深度区间，用POW(X, 3)处理一次）
inline fixed Distance2DepthPow0dot3(float distance)
{
	return pow(distance * _ProjectionParams.w, 0.3);
}

// 将距离转换成Unity深度
inline fixed Distance2Depth(float distance)
{
	return distance * _ProjectionParams.w;
}

// 将Unity深度转换成距离
inline fixed Depth2Distance(float depth)
{
	return depth * _ProjectionParams.z;
}





// 全局变量
uniform fixed _WriteDepth;

// 计算alpha值
inline fixed CalcDepthToAlpha(fixed depth, fixed keep_alpha)
{
	return _WriteDepth * depth + (1 - _WriteDepth) * keep_alpha;
}
inline fixed CalcDepthZWriteToAlpha(fixed depth, fixed zwrite, fixed keep_alpha)
{
	return zwrite * _WriteDepth * depth + min((2 - (zwrite + _WriteDepth)), 1) * keep_alpha;
}

// ---------------------- 使用者 - 宏定义 ----------------------

// 计算深度值(0-1)
// out_depth : 输出的参数
// world_pos : 模型空间坐标
//#define GSTORE_TRANSFER_DEPTH(out_depth, world_pos) out_depth = Distance2DepthPow0dot3(length(world_pos.xyz - _WorldSpaceCameraPos.xyz))
//#define GSTORE_TRANSFER_DEPTH(out_depth, world_pos) out_depth = Distance2Depth(length(world_pos.xyz - _WorldSpaceCameraPos.xyz))
// 下面等同于使用 COMPUTE_DEPTH_01
#define GSTORE_TRANSFER_DEPTH(out_depth, local_pos) out_depth = -(UnityObjectToViewPos(local_pos.xyz).z * _ProjectionParams.w)


// 把深度变为Alpha值
#define GSTORE_CALC_DEPTH_TO_ALPHA(alpha_value, depth, keep_alpha) alpha_value = CalcDepthToAlpha(depth, keep_alpha)

#define GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(alpha_value, depth, zwrite, keep_alpha) alpha_value = CalcDepthZWriteToAlpha(depth, zwrite, keep_alpha)

#endif // __FastDepthMap__