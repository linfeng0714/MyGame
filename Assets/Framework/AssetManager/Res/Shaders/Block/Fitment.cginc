// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

#ifndef __Fitment__
#define __Fitment__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= ����Shaderʹ�õĿ��ع��ܺ� =========================
// Shader��ʹ��Define����:

// USE_MAIN_COLOR - ʹ������ɫ������ͬʱû��ʹ������ͼ(û��_MainTex)

// USE_BUMP_MAP - ʹ�÷�����ͼ(ֵΪ0ʱ���Ż�����������)

// �Է���Ч��
// USE_SELF_ILLUMIN
// USE_SELF_ILLUMIN_MAIN_TEX_ALPHA USE_SELF_ILLUMIN_TEX

// ʹ�òݶ��㶯��
// USE_GRASS_VA

// ʹ����ͼ͸��ͨ���ü�
// USE_ALPHA_CUTOFF

// ʹ����ͼ͸��
// USE_TEX_TRANSPARENCY

// ʹ�÷���������
// USE_FRESNEL_REFLECT

// �������Dither Transparency
// USE_DITHER_TRANSPARENCY 

// �������͸���ȣ�����Ϊ_Transparency
// USE_TRANSPARENCY

// ��������ʹ�ã�ǿ��ȥ��MC���ռ���(������ʱû���õ�)
// DISABLE_MC_LIGHT



// ========================= ���ض��� =========================
// ��Ŀ�ķ�����:
// 1.Ч������
// 2.�Ż����أ��Ż����ؾ�������һ��Ŀ����������ﵽ�Ż�Ŀ�ġ�
// �����÷�ʽ������:
// 1.��̬���ã���Shader��ֱ��ʹ��#define������(���ֲ����������)
// 2.��̬���ã���Shader��ʹ��multi_compile��shader_feature������(���ֻ��������)
// ΪʲôҪ��USE_XXX��USING_XXX�أ���Ϊǰ���Ǹ��û��趨�ģ�����Ϊ����ʱʹ�õģ����ߵ������п��ܲ�һ����

// �������ɫ
#define USING_MAIN_COLOR (USE_MAIN_COLOR)

// ������ͼ
#define USING_BUMP_MAP (USE_BUMP_MAP) && defined(UNITY_PASS_FORWARDBASE)

// �Է���Ч��
#define USING_SELF_ILLUMIN (USE_SELF_ILLUMIN) && defined(UNITY_PASS_FORWARDBASE)
#define USING_SELF_ILLUMIN_TEX (USE_SELF_ILLUMIN_TEX)

// dither ͸��
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// MC����
#define USING_MC_LIGHT !defined(DISABLE_MC_LIGHT) && defined(UNITY_PASS_FORWARDBASE)

// �Ƿ�ʹ��unity�� sh ����
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// �Ƿ�ʹ�òݵĶ��㶯��
#define USING_GRASS_VA (USE_GRASS_VA)

// �Ƿ�ʹ��Alpha cutoff
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// ��ͨ��ͼ͸��
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// ��ͨ͸��
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// ����������
#define USING_FRESNEL_REFLECT (USE_FRESNEL_REFLECT)

// �Ƿ�����˫��ķ���
#define USING_FIXED_DOUBLE_SIZE_NORMAL (SHADER_TARGET >= 30)

// ========================= ���� - ��ʼ =========================
// ע��:
// �Ż� - ����ʹ��_XXXXXX_ST�ɼ�������GPUָ��˲����ͼӲ�������ϸ��TRANSFORM_TEX���ú�����

#if !USING_MAIN_COLOR
// (texture)����ͼ
uniform sampler2D _MainTex;
#endif

#if USING_MAIN_COLOR
// (color)������ɫ
uniform fixed4 _Color;
#endif

#if USING_BUMP_MAP
// (texture)������ͼ
uniform sampler2D _BumpMapTex;								
#endif

// �Է���
#if USING_SELF_ILLUMIN
// (bool)�Ƿ�ʹ���Է����ܻ�������Ȩ��
uniform fixed _BaseIlluminWeighted;
// (0-1)Self Illumin ǿ��
uniform fixed _SelfIlluminIntensity;
#if USING_SELF_ILLUMIN_TEX
// (texture)�Է�����ͼ
// ��ͼ��������������Ҫ����ĵط����Ϸ������ɫ����������Ҫ����ĵط���ɫ�Ϳ�����
uniform sampler2D _SelfIlluminTex;
#endif
#endif

// �ݶ��㶯��
#if USING_GRASS_VA
// (float)Grass ���Կ�ʼ X
uniform half _GrassVAStartX;
// (0,1)Grass �Ƿ�ʹ������ X
uniform half _GrassVALerp2X;
// (-5,5)Grass ǿ���뷽�� X
uniform half _GrassVAIntensityDirX;
// (0, 2)Grass VA Scale X
uniform half _GrassVAScaleX;
// (float)Grass ���Կ�ʼ Y
uniform half _GrassVAStartY;
// (0,1)Grass �Ƿ�ʹ������ Y
uniform half _GrassVALerp2Y;
// (-5,5)Grass ǿ���뷽�� Y
uniform half _GrassVAIntensityDirY;
// (0, 2)Grass VA Scale Y
uniform half _GrassVAScaleY;
// (flaot)Grass ����ǿ��
uniform half _GrassVAIntensity;
// (flaot)Grass VA Ƶ��
uniform half _GrassVATimeScale;
// (0, 1)Grass VA ͳһ��
uniform fixed _GrassVAUnified;
#endif

// Texture Transparent
#if USING_TEX_TRANSPARENCY
// (0, 1) ��ͼ͸����
uniform half _TexTransparency;
#endif

// ��ͨ͸��
#if USING_TRANSPARENCY
// (0, 1) �������͸����
uniform fixed _Transparency;
#endif

// (bool) �Ƿ�д��z
uniform fixed _ZWrite;


// �����޸ĵ�MC��������
#if USING_MC_LIGHT
// (texture)�������������ͼ
uniform sampler2D _SkyLightTex;
// (texture)�������û�ѹ���ͼ
uniform sampler2D _BlockLightTex;
// (bool)�Ƿ�ʹ�÷�����ΪȨ��
uniform fixed _NormalWeightedLight;

// ---------------------------------------------
// ����1��
// ������˳��Ҫע�⣬һ����ռ�õ�ֵ�ļĴ�����С����
// ���������������iOS��metal�ı༭���оͻ����bug
// ---------------------------------------------
// ����2��
// Unity2017.4.1�汾��Androidƽ̨�£�API OpenGL ES 3.2 V@145.0 (GIT@I07a8b16b27)
// ���Buffer���ݽṹ����Ҫ���ƣ�
// ������ڵ���24��float
// ����С��28��float�����ֻ��8������
// ---------------------------------------------
UNITY_INSTANCING_BUFFER_START(Fitment_Properties)
	UNITY_DEFINE_INSTANCED_PROP(float4, _OriginParam0)
	UNITY_DEFINE_INSTANCED_PROP(float4, _OriginParam1)
	UNITY_DEFINE_INSTANCED_PROP(float4, _OriginParam2)
	UNITY_DEFINE_INSTANCED_PROP(float4, _ColorEncode)
	UNITY_DEFINE_INSTANCED_PROP(half3, _BlockSize)
	UNITY_DEFINE_INSTANCED_PROP(fixed3, _Dummy_1)
	UNITY_DEFINE_INSTANCED_PROP(fixed, _EnableBlockLight)
	UNITY_DEFINE_INSTANCED_PROP(fixed, _Dummy_2)
UNITY_INSTANCING_BUFFER_END(Fitment_Properties)

#define _OriginParam0_Inst		UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _OriginParam0)
#define _OriginParam1_Inst		UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _OriginParam1)
#define _OriginParam2_Inst		UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _OriginParam2)
#define _ColorEncode_Inst		UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _ColorEncode)
#define _EnableBlockLight_Inst	UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _EnableBlockLight)
#define _BlockSize_Inst			UNITY_ACCESS_INSTANCED_PROP(Fitment_Properties, _BlockSize)



#endif


#if USING_FRESNEL_REFLECT
uniform samplerCUBE _Cubemap;
uniform fixed _Glossiness;
uniform fixed _FresnelThickness;
uniform fixed4 _EmissiveColor;
#endif


// ========================= �������� - ��ʼ =========================

// �Է��⺯��
inline fixed3 SelfIlluminEmission(fixed4 tex_diffuse, float2 uv)
{
#if USING_SELF_ILLUMIN


#if USE_SELF_ILLUMIN_MAIN_TEX_ALPHA
	return (_SelfIlluminIntensity * tex_diffuse.rgb) * tex_diffuse.a;
#elif USE_SELF_ILLUMIN_TEX
	return (_SelfIlluminIntensity * tex2D(_SelfIlluminTex, uv).rgb);
#else 
	// full
	return (_SelfIlluminIntensity * tex_diffuse.rgb);
#endif


#else
	return 0;
#endif
}

// ��ǰ�ƹⷽ��
inline fixed3 LightDirection(float3 posWorld)
{
#ifndef USING_DIRECTIONAL_LIGHT
	return normalize(UnityWorldSpaceLightDir(posWorld));
#else
	return _WorldSpaceLightPos0.xyz;
#endif
}

// Calculate a 4 fast sine-cosine pairs
// val:     the 4 input values - each must be in the range (0 to 1)
void FastSin(half4 val, out half4 s)
{
	val = val * 6.408849 - 3.1415927;
	half4 r5 = val * val;
	half4 r1 = r5 * val;
	half4 r2 = r1 * r5;
	half4 r3 = r2 * r5;
	half4 sin7 = { 1, -0.16161616, 0.0083333, -0.00019841 };
	s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
}

// �ݶ��㶯������ - �ɺ���
//inline void WavingGrassVert(inout float4 vertex)
//{
//#if USING_GRASS_VA
//	float time = _Time.y + (sin(mul(unity_ObjectToWorld, vertex).x)) * _GrassVATimeScale;
//	float vertexOffset = (_GrassVAIntensity * cos(time)) *  (_GrassVAIntensity * sin(time));
//	vertex.xyz += (max(vertex.y - _GrassVAStartY, 0) * float3(vertexOffset, 0.0, vertexOffset));
//	// half y_delta = max(vertex.y - _GrassVAStartY, 0);
//	// vertex.xyz += (sin(y_delta * _GrassVAEffectY) * float3(vertexOffset * sin(time), 0.0, vertexOffset * cos(time)));
//#endif
//}
// �ݶ��㶯������ - �º���
inline void WavingGrassVert(inout float4 vertex)
{
#if USING_GRASS_VA
	
	// ȡ����ռ��xyz������ͬxyz��λ�õĶ�������ͬ�Ĳ��컯��ʹ�����Ķ��㶯����ɲ�һ�µı���
	//float3 posWorldOffset = mul(unity_ObjectToWorld, vertex).xyz;
	float3 posWorldOffset = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	half posOffset = dot(posWorldOffset, posWorldOffset) * 3.1415926;

	// wind speed, wave size, wind amount, max sqr distance
	const half4 waveSpeed = half4 (1.2, 2, 1.6, 4.8);
	const half4 _waveXmove = half4(0.024, 0.04, -0.12, 0.096);
	const half4 _waveZmove = half4 (0.006, 0.02, -0.02, 0.1);
	const half4 _waveYmove = (_waveXmove + _waveZmove) * 0.5;

	const half4 _waveXSize = half4(0.012, 0.02, 0.06, 0.024);
	const half4 _waveZSize = half4 (0.006, 0.02, 0.02, 0.05);
	
	half4 waves = (vertex.x * _waveXSize) + (vertex.z * _waveZSize) + (_Time.x * _GrassVATimeScale * waveSpeed) + ( (posOffset) *  (1.0 - _GrassVAUnified));

	half4 s;
	waves = frac(waves);
	FastSin(waves, s);
	s = s * s;
	s = s * s;  //��������s��4�η�����ʹ�ò�ͬ��y���µı仯���ȸ�С���ڶ�����ʵ

	// ȡ�ֲ�Y��ɿ���Y���ϱ仯����(С��_GrassVAStartY�Ķ���仯��Ϊ0��Խ��Խ�仯��)
	half4 waveAmount = lerp(1, max((vertex.x - _GrassVAStartX) * _GrassVAIntensityDirX, 0), _GrassVALerp2X) * lerp(1, max((vertex.y - _GrassVAStartY) * _GrassVAIntensityDirY, 0), _GrassVALerp2Y) * _GrassVAIntensity;
	s *= waveAmount; //�������
	
	half3 waveMove = half3 (0, 0, 0);
	waveMove.x = dot(s, _waveXmove) * _GrassVAScaleX;
	waveMove.z = dot(s, _waveZmove);
	waveMove.y = dot(s, _waveYmove) * _GrassVAScaleY;
	//vertex.xz -= mul((float3x3)unity_WorldToObject, waveMove).xz;
	vertex.xyz -= waveMove.xyz;
#endif
}
#if USING_GRASS_VA
#define APPLY_GRASS_VA(vertex) WavingGrassVert(vertex)
#else
#define APPLY_GRASS_VA(vertex)
#endif



// ����Ҿ߶���Ļ�ѹ���
//inline fixed2 CalcVertBlockLight(float3 pos_world, float3 normal, fixed normal_weighted, fixed3 block_size, fixed3 origin_offset, fixed4 sky_light_0123, fixed4 sky_light_4567, fixed4 block_light_0123, fixed4 block_light_4567)
//{
//	// �ȼ���8���������������λ��
//	float3 pos_0 = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz + origin_offset;
//	float3 pos_1 = pos_0 + float3(block_size.x, 0, 0);
//	float3 pos_2 = pos_0 + float3(block_size.x, 0, block_size.z);
//	float3 pos_3 = pos_0 + float3(0, 0, block_size.z);
//	float3 pos_4 = pos_0 + float3(0, block_size.y, 0);
//	float3 pos_5 = pos_1 + float3(0, block_size.y, 0);
//	float3 pos_6 = pos_2 + float3(0, block_size.y, 0);
//	float3 pos_7 = pos_3 + float3(0, block_size.y, 0);
//
////	// block_size������ļ���֮��Ҫת����������ϵ
////	block_size = mul(unity_ObjectToWorld, float4(block_size.x, block_size.y, block_size.z, 0)).xyz;
//
//	// ����8������ľ���
//	float4 dist_0123 = float4(length(pos_0 - pos_world),
//	length(pos_1 - pos_world),
//	length(pos_2 - pos_world),
//	length(pos_3 - pos_world));
//
//	float4 dist_4567 = float4(length(pos_4 - pos_world),
//	length(pos_5 - pos_world),
//	length(pos_6 - pos_world),
//	length(pos_7 - pos_world));
//
//	dist_0123 = max(float4(0.01, 0.01, 0.01, 0.01), dist_0123);
//	dist_4567 = max(float4(0.01, 0.01, 0.01, 0.01), dist_4567);
//
//	// Ȩ��
//	float4 weight_0123 = 1 / dist_0123;
//	float4 weight_4567 = 1 / dist_4567;
//
//	// Ȩ������(�պ��ڱ߽�Ķ���ֻ��Ҫ���Ǳ߽�Ķ���ֵ)
//	fixed3 factor = sign(abs(abs(pos_world - pos_0) - abs(block_size)) - 0.05) + 1;	// ��ͬ��if (abs(abs(pos_world - pos_0) - abs(block_size)) < 0.01) { factor.xyz = 0 } else { factor.xyz = 2 }
//	weight_0123.x *= saturate(factor.x * factor.y * factor.z); 		// �Թ�������ͬ��/��Ķ���
//
//	factor = sign(abs(abs(pos_world - pos_1) - abs(block_size)) - 0.05) + 1;
//	weight_0123.y *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_2) - abs(block_size)) - 0.05) + 1;
//	weight_0123.z *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_3) - abs(block_size)) - 0.05) + 1;
//	weight_0123.w *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_4) - abs(block_size)) - 0.05) + 1;
//	weight_4567.x *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_5) - abs(block_size)) - 0.05) + 1;
//	weight_4567.y *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_6) - abs(block_size)) - 0.05) + 1;
//	weight_4567.z *= saturate(factor.x * factor.y * factor.z);	
//
//	factor = sign(abs(abs(pos_world - pos_7) - abs(block_size)) - 0.05) + 1;
//	weight_4567.w *= saturate(factor.x * factor.y * factor.z);	
//
//	// ���Ϸ��߳����Ȩ���޸�	
//	if (normal_weighted > 0)
//	{	
//		float3 dot_eun = float3(max(0, dot(normal, fixed3(1, 0, 0))),
//		max(0, dot(normal, fixed3(0, 1, 0))),
//		max(0, dot(normal, fixed3(0, 0, 1))));
//
//		float3 dot_wds = float3(max(0, dot(normal, fixed3(-1, 0, 0))),
//		max(0, dot(normal, fixed3(0, -1, 0))),
//		max(0, dot(normal, fixed3(0, 0, -1))));
//
//		weight_0123.x *= (dot_wds.x + dot_wds.y + dot_wds.z);
//		weight_0123.y *= (dot_eun.x + dot_wds.y + dot_wds.z);	
//		weight_0123.z *= (dot_eun.x + dot_wds.y + dot_eun.z);	
//		weight_0123.w *= (dot_wds.x + dot_wds.y + dot_eun.z);	
//		weight_4567.x *= (dot_wds.x + dot_eun.y + dot_wds.z);	
//		weight_4567.y *= (dot_eun.x + dot_eun.y + dot_wds.z);	
//		weight_4567.z *= (dot_eun.x + dot_eun.y + dot_eun.z);	
//		weight_4567.w *= (dot_wds.x + dot_eun.y + dot_eun.z);	
//	}
//
//	// ����Ȩ��
//	float total_weight = weight_0123.x + weight_0123.y + weight_0123.z + weight_0123.w + 
//	weight_4567.x + weight_4567.y + weight_4567.z + weight_4567.w;
//
//	weight_0123 /= total_weight;
//	weight_4567 /= total_weight;
//
//	// ���չ���ֵ
//	fixed sky_light = sky_light_0123.x * weight_0123.x +
//	sky_light_0123.y * weight_0123.y +
//	sky_light_0123.z * weight_0123.z +
//	sky_light_0123.w * weight_0123.w +
//	sky_light_4567.x * weight_4567.x +
//	sky_light_4567.y * weight_4567.y +
//	sky_light_4567.z * weight_4567.z +
//	sky_light_4567.w * weight_4567.w;
//
//	fixed block_light = block_light_0123.x * weight_0123.x +
//	block_light_0123.y * weight_0123.y +
//	block_light_0123.z * weight_0123.z +
//	block_light_0123.w * weight_0123.w +
//	block_light_4567.x * weight_4567.x +
//	block_light_4567.y * weight_4567.y +
//	block_light_4567.z * weight_4567.z +
//	block_light_4567.w * weight_4567.w;
//
//	return fixed2(sky_light, block_light);
//}

// ���ߺ������ҳ�x,y,z��������Сֵ
inline half Max3(half3 x)
{
	return max(x.x, max(x.y, x.z));
}

inline half Min3(half3 x)
{
	return min(x.x, min(x.y, x.z));
}

// һ����С��ֵ
#define EPSILON         1.0e-4

// ����Ҿ߶���Ļ�ѹ���
inline fixed2 CalcVertBlockLight(half3 pos_local, fixed3 normal_local, fixed normal_weighted, fixed3 block_size, half4x4 localOffsetMatrix, fixed4 sky_light_0123, fixed4 sky_light_4567, fixed4 block_light_0123, fixed4 block_light_4567)
{
	// �Ѿֲ�����תΪ����ռ�λ��
	pos_local = mul(localOffsetMatrix, half4(pos_local, 1));
	normal_local = normalize(mul((half3x3)localOffsetMatrix, normal_local));

	// �ȼ���8������ķ���ռ�λ��
	half3 pos_0 = half3(0, 0, 0);
	half3 pos_1 = pos_0 + half3(block_size.x, 0, 0);
	half3 pos_2 = pos_0 + half3(block_size.x, 0, block_size.z);
	half3 pos_3 = pos_0 + half3(0, 0, block_size.z);
	half3 pos_4 = pos_0 + half3(0, block_size.y, 0);
	half3 pos_5 = pos_1 + half3(0, block_size.y, 0);
	half3 pos_6 = pos_2 + half3(0, block_size.y, 0);
	half3 pos_7 = pos_3 + half3(0, block_size.y, 0);

	// ����8������ľ���
	//half4 dist_0123 = half4(length(pos_0 - pos_local),
	//	length(pos_1 - pos_local),
	//	length(pos_2 - pos_local),
	//	length(pos_3 - pos_local));

	//half4 dist_4567 = half4(length(pos_4 - pos_local),
	//	length(pos_5 - pos_local),
	//	length(pos_6 - pos_local),
	//	length(pos_7 - pos_local));

	// �Ѿ��뻯ΪȨ��
	float4 weight_0123;// = dist_0123;
	float4 weight_4567;// = dist_4567;
	float3 one_over_block_size = 1.0 / block_size;

	// Ȩ���ص���������Խ�ӽ��Ҿ�Bound�Ķ��㣬��Զ���Ķ������Ӱ��ԽС����ֹ������ͬ���ͼҾ߱��ϵĵ����Ա߼Ҿߵĵ������ɫ��һ��
	half3 offset;
	offset = pos_local - pos_0;
	{
		weight_0123.x = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_1;
	{
		offset.x = -offset.x;
		weight_0123.y = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_2;
	{
		offset.xz = -offset.xz;
		weight_0123.z = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_3;
	{
		offset.z = -offset.z;
		weight_0123.w = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_4;
	{
		offset.y = -offset.y;
		weight_4567.x = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_5;
	{
		offset.xy = -offset.xy;
		weight_4567.y = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_6;
	{
		offset = -offset;
		weight_4567.z = Min3(1 - saturate(offset * one_over_block_size));
	}

	offset = pos_local - pos_7;
	{
		offset.zy = -offset.zy;
		weight_4567.w = Min3(1 - saturate(offset * one_over_block_size));
	}


	// ���Ϸ��߳����Ȩ���޸�	
	if (normal_weighted > 0)
	{
		// east up north : x y z
		fixed3 dot_eun = max(0, 
			fixed3
			(
				dot(normal_local, fixed3(1, 0, 0)),
				dot(normal_local, fixed3(0, 1, 0)),
				dot(normal_local, fixed3(0, 0, 1))
			)
		);

		// west down south : x y z
		fixed3 dot_wds = max(0, 
			fixed3
			(
				dot(normal_local, fixed3(-1, 0, 0)),
				dot(normal_local, fixed3(0, -1, 0)),
				dot(normal_local, fixed3(0, 0, -1))
			)
		);

		weight_0123.x *= (dot_wds.x + dot_wds.y + dot_wds.z);
		weight_0123.y *= (dot_eun.x + dot_wds.y + dot_wds.z);
		weight_0123.z *= (dot_eun.x + dot_wds.y + dot_eun.z);
		weight_0123.w *= (dot_wds.x + dot_wds.y + dot_eun.z);
		weight_4567.x *= (dot_wds.x + dot_eun.y + dot_wds.z);
		weight_4567.y *= (dot_eun.x + dot_eun.y + dot_wds.z);
		weight_4567.z *= (dot_eun.x + dot_eun.y + dot_eun.z);
		weight_4567.w *= (dot_wds.x + dot_eun.y + dot_eun.z);
	}

	// ����Ȩ��
	half total_weight = weight_0123.x + weight_0123.y + weight_0123.z + weight_0123.w +
		weight_4567.x + weight_4567.y + weight_4567.z + weight_4567.w;

	total_weight = max(total_weight, EPSILON);
	weight_0123 /= total_weight;
	weight_4567 /= total_weight;

	// ���չ���ֵ
	/*fixed sky_light = sky_light_0123.x * weight_0123.x +
	sky_light_0123.y * weight_0123.y +
	sky_light_0123.z * weight_0123.z +
	sky_light_0123.w * weight_0123.w +
	sky_light_4567.x * weight_4567.x +
	sky_light_4567.y * weight_4567.y +
	sky_light_4567.z * weight_4567.z +
	sky_light_4567.w * weight_4567.w;*/
	fixed sky_light = dot(sky_light_0123, weight_0123) + dot(sky_light_4567, weight_4567);

	/*fixed block_light = block_light_0123.x * weight_0123.x +
	block_light_0123.y * weight_0123.y +
	block_light_0123.z * weight_0123.z +
	block_light_0123.w * weight_0123.w +
	block_light_4567.x * weight_4567.x +
	block_light_4567.y * weight_4567.y +
	block_light_4567.z * weight_4567.z +
	block_light_4567.w * weight_4567.w;*/
	fixed block_light = dot(block_light_0123, weight_0123) + dot(block_light_4567, weight_4567);

	return fixed2(sky_light, block_light);
}

// ��һ��float�н�����3��ֵ��ΧΪ0~255��int
// ref:https://www.gamedev.net/forums/topic/556757-unpack-4-bytes-from-an-int-in-glsl/
float3 UnpackValues(float packedVal)
{
	float3 unpackedVals = float3(1.0, 256.0, 65536.0);
	unpackedVals = frac(unpackedVals * packedVal);
	return floor(unpackedVals * 256.0);
}

float2 DecodeByteValue(float value)
{
	return float2(fmod(value, 16), floor(value * 0.0625));
}

void DecodeLightColor(float3x3 color_encode, out fixed4 block_light_down, out fixed4 block_light_up, out fixed4 sky_light_down, out fixed4 sky_light_up)
{
	float4 mulValue = float4(0.066666666667, 0.066666666667, 0.066666666667, 0.066666666667);
	block_light_down = float4(DecodeByteValue(color_encode[0].x), DecodeByteValue(color_encode[0].y)) * mulValue;
	block_light_up = float4(DecodeByteValue(color_encode[0].z), DecodeByteValue(color_encode[1].x)) * mulValue;
	sky_light_down = float4(DecodeByteValue(color_encode[1].y), DecodeByteValue(color_encode[1].z)) * mulValue;
	sky_light_up = float4(DecodeByteValue(color_encode[2].x), DecodeByteValue(color_encode[2].y)) * mulValue;
}

// ========================= Forward���� =========================

#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
#if USING_BUMP_MAP
	float4 tangent : TANGENT;
#endif

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	UNITY_POSITION(pos);
#if CALC_DEPTH
	float3 uv : TEXCOORD0;					// ����ͼuv��д�����ʱuv.z������¼���ֵ
#else
	float2 uv : TEXCOORD0;
#endif

#if USING_BUMP_MAP
	float4 tSpace0 : TEXCOORD1;					// ���߿ռ�(x:Tangent.x, y:Binormal.x, z:Normal.x, w:posWorld.x), �����������ʹ��float
	float4 tSpace1 : TEXCOORD2;					// ���߿ռ�(x:Tangent.y, y:Binormal.y, z:Normal.y, w:posWorld.y), �����������ʹ��float
	float4 tSpace2 : TEXCOORD3;					// ���߿ռ�(x:Tangent.z, y:Binormal.z, z:Normal.z, w:posWorld.z), �����������ʹ��float
#else
	fixed3 normalWorld : TEXCOORD1;			// ���編��
	float3 posWorld : TEXCOORD2;			// ��������
#endif

	SHADOW_COORDS(4)
	UNITY_FOG_COORDS(5)						// unity��ֻ�õ�һ��float
	fixed4 sh : TEXCOORD6;				// �����⼰�������Դ��alphaͨ���������blockLight

#if USING_MC_LIGHT
	fixed2 mcLight : TEXCOORD7;				// skylight, blocklightֵ
#endif

	UNITY_VERTEX_OUTPUT_STEREO
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	// Ӧ�ö��㶯��
	APPLY_GRASS_VA(v.vertex);

	o.pos = UnityObjectToClipPos(v.vertex);
	// uv
	o.uv.xy = v.texcoord;

	// ����ռ䶥��λ��
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;

#if CALC_DEPTH
	//���������д��uv.z
	GSTORE_TRANSFER_DEPTH(o.uv.z, v.vertex);
#endif

	// ����ռ䷨��
	fixed3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if USING_BUMP_MAP
	// ����ռ�Tangent
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	// ����ռ�Binormal
	fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(normalWorld, worldTangent) * tangentSign;

	// Transfer to frag
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, normalWorld.x, posWorld.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, normalWorld.y, posWorld.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, normalWorld.z, posWorld.z);
#else
	// ������ߺ�����ռ䶥��λ��
	o.posWorld = posWorld;
	o.normalWorld = normalWorld;
#endif
	
	o.sh = 0;
	// vertex�����еĹ�����ɫ�ͻ�����
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
	o.sh.rgb += Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	// ע�⣺ShadeSH9���ֿɷŵ�frag���Bump��ϣ���Ч�ʿ��ǵĻ������Ƿ���vert���������bumpӰ��
	// ��������˻�����
	//o.sh.rgb += max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
	o.sh.rgb += CaleCameraSH(normalWorld);
#endif

#if USING_MC_LIGHT
	//o.mcLight = CalcVertBlockLight(posWorld, normalWorld, _NormalWeightedLight, _SkyLight_Inst, _OriginOffset_Inst,
	//	_SkyLightDown_Inst, _SkyLightUp_Inst,
	//	_BlockLightDown_Inst, _BlockLightUp_Inst);

	//o.mcLight = CalcVertBlockLight(v.vertex, v.normal, _NormalWeightedLight, _BlockSize_Inst, _OriginMatrix_Inst,
	//	_SkyLightDown_Inst, _SkyLightUp_Inst,
	//	_BlockLightDown_Inst, _BlockLightUp_Inst);

	float4x4 origin_matrix;
	origin_matrix[0] = float4(_OriginParam0_Inst.xyz, 0.0);
	origin_matrix[1] = float4(_OriginParam1_Inst.xyz, 0.0);
	origin_matrix[2] = float4(_OriginParam2_Inst.xyz, 0.0);
	origin_matrix[3] = float4(_OriginParam0_Inst.w, _OriginParam1_Inst.w, _OriginParam2_Inst.w, 1.0);

	fixed4 sky_light_down, sky_light_up, block_light_down, block_light_up;
	float3x3 color_encode;
	color_encode[0] = UnpackValues(_ColorEncode_Inst.x);
	color_encode[1] = UnpackValues(_ColorEncode_Inst.y);
	color_encode[2] = UnpackValues(_ColorEncode_Inst.z);
	DecodeLightColor(color_encode, block_light_down, block_light_up, sky_light_down, sky_light_up);

	o.mcLight = CalcVertBlockLight(v.vertex, v.normal, _NormalWeightedLight, _BlockSize_Inst, origin_matrix,
		sky_light_down, sky_light_up, block_light_down, block_light_up);
#endif

	TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);// pass fog coordinates to pixel shader
	return o;
}

fixed4 frag(
	v2f i
#if SHADER_TARGET >= 30
	,fixed facing : VFACE
#endif
) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);

	// ��ͼ��ɫ
#if USING_MAIN_COLOR
	fixed4 tex_diffuse = _Color;
#else
	fixed4 tex_diffuse = tex2D(_MainTex, i.uv.xy);
#endif

	// ������ͼ(���пռ�)
#if USING_BUMP_MAP
	// ���編��
	fixed3 bump_noarmal = UnpackNormal(tex2D(_BumpMapTex, i.uv.xy));
	fixed3 normalWorld;
	normalWorld.x = dot(i.tSpace0.xyz, bump_noarmal);
	normalWorld.y = dot(i.tSpace1.xyz, bump_noarmal);
	normalWorld.z = dot(i.tSpace2.xyz, bump_noarmal);

	// �����������������
	float3 posWorld = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
#else
	fixed3 normalWorld = i.normalWorld;	// ��������عⲻ���㷨����ͼ
	float3 posWorld = i.posWorld;
#endif

	// �ӵ㵥λ����
#if USING_FIXED_DOUBLE_SIZE_NORMAL && USING_FRESNEL_REFLECT
	fixed3 viewDir = normalize(UnityWorldSpaceViewDir(posWorld));
#endif

	// �����淴ת����
#if USING_FIXED_DOUBLE_SIZE_NORMAL
	fixed facing_sign = facing >= 0 ? 1 : -1;
	normalWorld *= facing_sign;
#endif

	// ����˥�� compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, i, posWorld)
	// �ƹⷽ��
	fixed3 lightDirection = LightDirection(posWorld);
	// ���߷���
	fixed3 normalDirection = normalize(normalWorld);

	// -------------- �����䲿�� ------------------
	// ����
	//half3 lambert_term = max(0.0, (0.5 * dot(normalDirection, lightDirection) + 0.5));
	half3 lambert_term = max(0.0, (dot(normalDirection, lightDirection)));

	// ��ʽ����
	// ��ʽ���壺ʹ��ֱ��ⶨ��������ɫ
	half3 direct_diffuse = _LightColor0.rgb * lambert_term;
	half3 indirect_diffuse = half3(0, 0, 0);
#if USING_UNITY_SH
	// ���������������ı�Ȩ��, ��SH����Ҳ���뵽���������
	// ��ʽ����: ʹ�û����ⶨ��Ϊ������ɫ
	indirect_diffuse += i.sh.rgb * (1 - lambert_term);
	//indirect_diffuse += i.sh.rgb * (1 - atten);
#endif

	
	// ���㷽�����
	half3 diffuse_part = (direct_diffuse + indirect_diffuse);
#if ENABLE_FITMENT_ALONE
	// ����˥��
	diffuse_part *= atten;

	// ��������ǿ��
	half main_light_luminosity = saturate(Luminance(_LightColor0.rgb));

	// ������ǿ�ȡ���Ҫ��ȥ�����⣬��ȡ���ص��ֵ
	half3 point_lights = i.sh.rgb;
#if USING_UNITY_SH
	point_lights -= max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
#endif
	half point_lights_luminosity = saturate(Luminance(point_lights));

	// �����������ɫ
	// 1 - main_light_luminosity��	��ֹ�����ǿʱ��ɫ�ع�
	// point_lights_luminosity��	��ֹ�������ǿʱ��ɫ�ع�
	diffuse_part += (1 - main_light_luminosity) * point_lights_luminosity *
		CombineMCLighting_Fitment(diffuse_part, atten, main_light_luminosity, Luminance(i.sh.rgb), _SkyLightTex, _BlockLightTex) * 1.8;
#elif USING_MC_LIGHT
	diffuse_part = CombineMCLighting_Fitment(diffuse_part, atten, saturate(i.mcLight.x), _EnableBlockLight_Inst * saturate(i.mcLight.y), _SkyLightTex, _BlockLightTex);
#else
	diffuse_part *= atten;
#endif

//#if USING_MC_LIGHT
//	diffuse_part = CombineMCLighting_Fitment(diffuse_part, atten, saturate(i.mcLight.x), _EnableBlockLight_Inst * saturate(i.mcLight.y), _SkyLightTex, _BlockLightTex);
//#else
//	diffuse_part *= atten;
//#endif

	half3 emissive = half3(0, 0, 0);
#if USING_SELF_ILLUMIN
	//half diffuse_luminosity = (1 - _BaseIlluminWeighted) + _BaseIlluminWeighted * (1 - saturate(Luminance(diffuse_part)));
	// ����������ȼ�
	half diffuse_luminosity = 1 - (_BaseIlluminWeighted * saturate(Luminance(diffuse_part)));
	emissive += SelfIlluminEmission(tex_diffuse, i.uv.xy) * diffuse_luminosity;
#endif

#if USING_FRESNEL_REFLECT
	// �ӵ㵥λ����
	//fixed3 viewDir = normalize(UnityWorldSpaceViewDir(posWorld));
	// �����淨�߲�������
#if SHADER_TARGET < 30
	normalWorld = lerp(-normalWorld, normalWorld, ((dot(viewDir, normalWorld) + 1) / 2)); // hack�汾
//#else
//	normalWorld = normalWorld * facing; // ��ȷ�汾��Ҫ��shader model 3.0
#endif
	// �ӵ㵥λ������������
	fixed3 viewReflectDir = reflect(-viewDir, normalWorld);
	// ����cubemap
	fixed4 cubemap_var = texCUBE(_Cubemap, viewReflectDir);
	// ������ǿ�ȼ��㣬hack��
	fixed fresnel_var = pow(1 - max(0, dot(normalWorld, viewDir)), _FresnelThickness);
	// ��ȡ������ɫǿ�ȣ���Сֵ0.2
	fixed3 lightColorStr = max(saturate(1 - max(diffuse_part.r, max(diffuse_part.g, diffuse_part.b))), 0.2);
	// ��Ϸ�������ֵ���㷴��ȣ�����������ɫǿ��
	float Glossiness = max(fresnel_var, _Glossiness) * lightColorStr;
	//float Glossiness = _Glossiness * lightColorStr;
	// �������Է��⣬������Χ������ɫ��Cubemap��ɫ
	emissive += min(fresnel_var, _EmissiveColor.a) * _EmissiveColor.rgb * diffuse_part * lerp(fixed3(1,1,1), cubemap_var, Glossiness);
	diffuse_part = lerp(diffuse_part, cubemap_var, Glossiness);
#endif 


	// (ֱ�������� + ��������� + ����) * ��������ɫ
	half3 diffuse = diffuse_part * tex_diffuse;


	fixed final_alpha = 1.0;
#if USING_TEX_TRANSPARENCY
	// -------------- ��ͼAlpha���� ------------------
	#if USING_FRESNEL_REFLECT
	final_alpha *= max(fresnel_var, tex_diffuse.a * _TexTransparency);
	#else
	final_alpha *= tex_diffuse.a * _TexTransparency;
	#endif
#endif
#if USING_TRANSPARENCY
	// -------------- Alpha���� ----------------------
	// �������
	final_alpha *= _Transparency;
#endif
#if CALC_DEPTH
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.uv.z, _ZWrite, final_alpha);
#endif
	// ������ɫ
	fixed4 final_color = fixed4(diffuse + emissive, final_alpha);

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);

	// discard һ��Ҫ�ŵ���󣬷������iphone����ʾ����ȷ
	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);
	// lod crossfade
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);
	return final_color;
}


#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// ========================= shadow caster ���� =========================

#if defined(UNITY_PASS_SHADOWCASTER)

struct v2f_shadowcaster
{
	V2F_SHADOW_CASTER;
	// һ��Ҫ��TEXCOORD1��ʼ,V2F_SHADOW_CASTER�п���ռ��TEXCOORD0

#if GSTORE_USING_ALPHA_CUTOFF
	float2 uv : TEXCOORD1;		// for alpha test
#endif
	
	UNITY_VERTEX_OUTPUT_STEREO

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f_shadowcaster vert(appdata_base v)
{
	v2f_shadowcaster o;
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	// Ӧ�ö��㶯��
	APPLY_GRASS_VA(v.vertex);

#if GSTORE_USING_ALPHA_CUTOFF
	o.uv = v.texcoord.xy;
#endif


	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	return o;
}

float4 frag(v2f_shadowcaster i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);

#if !USING_MAIN_COLOR
	GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(_MainTex, i.uv);
#endif

	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);

	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	SHADOW_CASTER_FRAGMENT(i)
}



#endif // defined(UNITY_PASS_SHADOWCASTER)


#endif // __Fitment__
