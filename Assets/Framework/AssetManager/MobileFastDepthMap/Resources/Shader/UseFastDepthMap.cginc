#ifndef __UseFastDepthMap__
#define __UseFastDepthMap__


// 注意！！！注意！！！注意！！！注意！！！注意！！！
// 如果本文件被多重include引用，修改此文件后，必需Reimport所有引用此文件的shader，否则没有效果


// 使用深度计算库


// 使用深度图时处理:
// 1.增加编译定义:
// #pragma multi_compile __ DepthWrite_ON
// 2.使用宏来采样深度贴图
// GSTORE_SAMPLE_DEPTH


#include "UnityCG.cginc"



#if DepthWrite_ON
sampler2D _DepthTexture;
#else
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#endif


// Linear01Depth
// 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);


// Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
// x = 1-far/near
// y = far/near
// z = x/far
// w = y/far
// or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
// x = -1+far/near
// y = 1
// z = x/far
// w = 1/far
//float4 _ZBufferParams;

/*
Some math I did ... (not 100% verified, but looks like it works)

n = near
f = far
z = depth buffer Z-value
EZ  = eye Z value
LZ  = depth buffer Z-value remapped to a linear [0..1] range (near plane to far plane)
LZ2 = depth buffer Z-value remapped to a linear [0..1] range (eye to far plane)


DX:
EZ  = (n * f) / (f - z * (f - n))
LZ  = (eyeZ - n) / (f - n) = z / (f - z * (f - n))
LZ2 = eyeZ / f = n / (f - z * (f - n))


GL:
EZ  = (2 * n * f) / (f + n - z * (f - n))
LZ  = (eyeZ - n) / (f - n) = n * (z + 1.0) / (f + n - z * (f - n))
LZ2 = eyeZ / f = (2 * n) / (f + n - z * (f - n))



LZ2 in two instructions:
LZ2 = 1.0 / (c0 * z + c1)

DX:
c1 = f / n
c0 = 1.0 - c1

GL:
c0 = (1 - f / n) / 2
c1 = (1 + f / n) / 2


-------------------
http://www.humus.ca
*/

#if DepthWrite_ON
inline fixed SampleDepth(float2 uv)
{
	fixed depth = tex2D(_DepthTexture, uv).r;
	return depth;
}
#else

inline fixed SampleDepth(float2 uv)
{
	// 深度图里存放了[0,1]范围的非线性分布的深度值，这些深度值来自NDC坐标。
	fixed depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
	// 经过透视投影变换时，Z值的计算方式如下：
	// Z' = f * n / [(n-f)*z] + f/(f-n);
	// 可以简单的理解为将z值的倒数进行插值计算，映射到[0, 1]（d3d）空间中。
	// 将上式带入Linear01Depth中，可得最后计算结果为z/f
	// 就是说，是将[0, f]映射到[0, 1]空间，若要映射[n, f]到[0, 1]则是(z-n)/(f-n)
	depth = Linear01Depth(depth);
	return depth;
}
#endif


// ---------------------- 使用者 - 宏定义 ----------------------

// 取出深度值fixed(0-1)
// uv 为屏幕uv
#define GSTORE_SAMPLE_DEPTH(uv) SampleDepth(uv)


#endif // __UseFastDepthMap__