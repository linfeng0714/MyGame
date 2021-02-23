#ifndef __FastDepthMap__
#define __FastDepthMap__

// ע�⣡����ע�⣡����ע�⣡����ע�⣡����ע�⣡����
// ������ļ�������include���ã��޸Ĵ��ļ��󣬱���Reimport�������ô��ļ���shader������û��Ч��


// ��ȼ����

// ��Ⱦ�������崦��
// 1.����ͷ�ļ���
// #include "����ļ�"
// 2.�ڶ��㴦����������Ⱦ���:
// GSTORE_TRANSFER_DEPTH
// 3.�����ش������alpha���������
// GSTORE_CALC_DEPTH_TO_ALPHA



#include "UnityCG.cginc"


// ��8bit�����ڣ��Ժ��ʵľ���ȡ���һ����������ת��Ϊ�󲿷ֺ����õ����ͼ����һ���֣�������д��8bitʱʹ�ã�
inline fixed Distance2DepthPow0dot3_InLog_Step1(float distance) 
{
	return max(min((log(distance)) * 0.1, 1),0);
}

// ��8bit�����ڣ��Ժ��ʵľ���ȡ���һ����������ת��Ϊ�󲿷ֺ����õ����ͼ���ڶ����֣������ڶ�ȡ��΢����ʱʹ�á������Ҫת����unity������䣬��POW(X, 3)����һ�Σ�
inline fixed Distance2DepthPow0dot3_InLog_Step2(float depth)
{
	return min(depth + 0.02, 1);
}

// ����ͬ��Distance2DepthPow0dot3_InLog��������Զ����Ч���Բ���log�����Ǽ��������٣������Ҫת����unity������䣬��POW(X, 3)����һ�Σ�
inline fixed Distance2DepthPow0dot3(float distance)
{
	return pow(distance * _ProjectionParams.w, 0.3);
}

// ������ת����Unity���
inline fixed Distance2Depth(float distance)
{
	return distance * _ProjectionParams.w;
}

// ��Unity���ת���ɾ���
inline fixed Depth2Distance(float depth)
{
	return depth * _ProjectionParams.z;
}





// ȫ�ֱ���
uniform fixed _WriteDepth;

// ����alphaֵ
inline fixed CalcDepthToAlpha(fixed depth, fixed keep_alpha)
{
	return _WriteDepth * depth + (1 - _WriteDepth) * keep_alpha;
}
inline fixed CalcDepthZWriteToAlpha(fixed depth, fixed zwrite, fixed keep_alpha)
{
	return zwrite * _WriteDepth * depth + min((2 - (zwrite + _WriteDepth)), 1) * keep_alpha;
}

// ---------------------- ʹ���� - �궨�� ----------------------

// �������ֵ(0-1)
// out_depth : ����Ĳ���
// world_pos : ģ�Ϳռ�����
//#define GSTORE_TRANSFER_DEPTH(out_depth, world_pos) out_depth = Distance2DepthPow0dot3(length(world_pos.xyz - _WorldSpaceCameraPos.xyz))
//#define GSTORE_TRANSFER_DEPTH(out_depth, world_pos) out_depth = Distance2Depth(length(world_pos.xyz - _WorldSpaceCameraPos.xyz))
// �����ͬ��ʹ�� COMPUTE_DEPTH_01
#define GSTORE_TRANSFER_DEPTH(out_depth, local_pos) out_depth = -(UnityObjectToViewPos(local_pos.xyz).z * _ProjectionParams.w)


// ����ȱ�ΪAlphaֵ
#define GSTORE_CALC_DEPTH_TO_ALPHA(alpha_value, depth, keep_alpha) alpha_value = CalcDepthToAlpha(depth, keep_alpha)

#define GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(alpha_value, depth, zwrite, keep_alpha) alpha_value = CalcDepthZWriteToAlpha(depth, zwrite, keep_alpha)

#endif // __FastDepthMap__