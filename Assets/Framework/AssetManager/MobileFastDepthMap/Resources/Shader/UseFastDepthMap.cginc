#ifndef __UseFastDepthMap__
#define __UseFastDepthMap__


// ע�⣡����ע�⣡����ע�⣡����ע�⣡����ע�⣡����
// ������ļ�������include���ã��޸Ĵ��ļ��󣬱���Reimport�������ô��ļ���shader������û��Ч��


// ʹ����ȼ����


// ʹ�����ͼʱ����:
// 1.���ӱ��붨��:
// #pragma multi_compile __ DepthWrite_ON
// 2.ʹ�ú������������ͼ
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
	// ���ͼ������[0,1]��Χ�ķ����Էֲ������ֵ����Щ���ֵ����NDC���ꡣ
	fixed depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
	// ����͸��ͶӰ�任ʱ��Zֵ�ļ��㷽ʽ���£�
	// Z' = f * n / [(n-f)*z] + f/(f-n);
	// ���Լ򵥵����Ϊ��zֵ�ĵ������в�ֵ���㣬ӳ�䵽[0, 1]��d3d���ռ��С�
	// ����ʽ����Linear01Depth�У��ɵ���������Ϊz/f
	// ����˵���ǽ�[0, f]ӳ�䵽[0, 1]�ռ䣬��Ҫӳ��[n, f]��[0, 1]����(z-n)/(f-n)
	depth = Linear01Depth(depth);
	return depth;
}
#endif


// ---------------------- ʹ���� - �궨�� ----------------------

// ȡ�����ֵfixed(0-1)
// uv Ϊ��Ļuv
#define GSTORE_SAMPLE_DEPTH(uv) SampleDepth(uv)


#endif // __UseFastDepthMap__