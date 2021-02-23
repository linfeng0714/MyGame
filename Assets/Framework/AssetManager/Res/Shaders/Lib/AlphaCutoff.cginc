#ifndef __AlphaCutoff__
#define __AlphaCutoff__


// Alpha Cutoff��
// ʹ�÷���:
// 1.ʹ��ǰ��shader�ļ�����Ҫ����GSTORE_USING_ALPHA_CUTOFF��ֵ
// 2.�ڶ���GSTORE_USING_ALPHA_CUTOFF��������������ļ�

// ���ӣ�
// #define GSTORE_USING_ALPHA_CUTOFF 1
// #include "../Lib/AlphaCutoff.cginc"

// [shader������ñ���]
// _Cutoff("Alpha cutoff", Range(0,1)) = 0.5

// [fragment shader����ú�]
// ������ʹ�õĺ꣬������Ҫ��ѡһ�֣����Ҫ���м�;��
// GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(sampler2D, UV) - ������ͼ����ӦUV����Cutoff
// GSTORE_APPLY_ALPHA_CUTOFF(alpha_value) - ����Alphaֵ����Cutoff



// ���
#ifndef GSTORE_USING_ALPHA_CUTOFF
#error "need to define GSTORE_USING_ALPHA_CUTOFF"
#endif


#if GSTORE_USING_ALPHA_CUTOFF

// ---------------------- Shader������ ----------------------
// һ��unity����shaderʹ��Ҳ��_Cutoff����������������Ҳͳһʹ�����
uniform fixed _Cutoff;

// ---------------------- �ڲ����� ----------------------
// Alpha cutoff
inline void ApplyAlphaCutoff(fixed texcol_alpha)
{
	clip(texcol_alpha - _Cutoff);
}
inline void ApplyTextureAlphaCutoff(sampler2D tex, fixed2 uv)
{
	ApplyAlphaCutoff(tex2D(tex, uv).a);
}

#endif


// ---------------------- ʹ���� - �궨�� ----------------------
#if GSTORE_USING_ALPHA_CUTOFF
#define GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(tex, uv) ApplyTextureAlphaCutoff(tex, uv)
#define GSTORE_APPLY_ALPHA_CUTOFF(texcol_alpha) ApplyAlphaCutoff(texcol_alpha)
#else
#define GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(tex, uv)
#define GSTORE_APPLY_ALPHA_CUTOFF(texcol_alpha)
#endif


#endif // __AlphaCutoff__
