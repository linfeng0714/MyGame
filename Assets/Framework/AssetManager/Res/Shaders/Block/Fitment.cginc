// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

#ifndef __Fitment__
#define __Fitment__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= 可在Shader使用的开关功能宏 =========================
// Shader中使用Define定义:

// USE_MAIN_COLOR - 使用主颜色，但是同时没有使用主贴图(没有_MainTex)

// USE_BUMP_MAP - 使用法线贴图(值为0时可优化，减少设置)

// 自发光效果
// USE_SELF_ILLUMIN
// USE_SELF_ILLUMIN_MAIN_TEX_ALPHA USE_SELF_ILLUMIN_TEX

// 使用草顶点动画
// USE_GRASS_VA

// 使用贴图透明通道裁剪
// USE_ALPHA_CUTOFF

// 使用贴图透明
// USE_TEX_TRANSPARENCY

// 使用菲涅尔反射
// USE_FRESNEL_REFLECT

// 程序控制Dither Transparency
// USE_DITHER_TRANSPARENCY 

// 程序控制透明度，变量为_Transparency
// USE_TRANSPARENCY

// 程序设置使用，强制去掉MC光照计算(现在暂时没有用到)
// DISABLE_MC_LIGHT



// ========================= 开关定义 =========================
// 按目的分两类:
// 1.效果开关
// 2.优化开关，优化开关就是牺牲一点的可设置性来达到优化目的。
// 按设置方式分两类:
// 1.静态设置，在Shader里直接使用#define来设置(这种不会产生变体)
// 2.动态设置，在Shader里使用multi_compile或shader_feature来设置(这种会产生变体)
// 为什么要分USE_XXX和USING_XXX呢？因为前者是给用户设定的，后者为编码时使用的，两者的意义有可能不一样。

// 额外的颜色
#define USING_MAIN_COLOR (USE_MAIN_COLOR)

// 法线贴图
#define USING_BUMP_MAP (USE_BUMP_MAP) && defined(UNITY_PASS_FORWARDBASE)

// 自发光效果
#define USING_SELF_ILLUMIN (USE_SELF_ILLUMIN) && defined(UNITY_PASS_FORWARDBASE)
#define USING_SELF_ILLUMIN_TEX (USE_SELF_ILLUMIN_TEX)

// dither 透明
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// MC光照
#define USING_MC_LIGHT !defined(DISABLE_MC_LIGHT) && defined(UNITY_PASS_FORWARDBASE)

// 是否使用unity的 sh 光照
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// 是否使用草的顶点动画
#define USING_GRASS_VA (USE_GRASS_VA)

// 是否使用Alpha cutoff
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// 普通贴图透明
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// 普通透明
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// 菲涅尔反射
#define USING_FRESNEL_REFLECT (USE_FRESNEL_REFLECT)

// 是否修正双面的法线
#define USING_FIXED_DOUBLE_SIZE_NORMAL (SHADER_TARGET >= 30)

// ========================= 属性 - 开始 =========================
// 注意:
// 优化 - 纹理不使用_XXXXXX_ST可减少两条GPU指令（乘操作和加操作，详细见TRANSFORM_TEX内置函数）

#if !USING_MAIN_COLOR
// (texture)主贴图
uniform sampler2D _MainTex;
#endif

#if USING_MAIN_COLOR
// (color)额外颜色
uniform fixed4 _Color;
#endif

#if USING_BUMP_MAP
// (texture)法线贴图
uniform sampler2D _BumpMapTex;								
#endif

// 自发光
#if USING_SELF_ILLUMIN
// (bool)是否使用自发光受基本光照权重
uniform fixed _BaseIlluminWeighted;
// (0-1)Self Illumin 强度
uniform fixed _SelfIlluminIntensity;
#if USING_SELF_ILLUMIN_TEX
// (texture)自发光贴图
// 贴图制作方法：把需要发光的地方填上发光的颜色，其它不需要发光的地方黑色就可以了
uniform sampler2D _SelfIlluminTex;
#endif
#endif

// 草顶点动画
#if USING_GRASS_VA
// (float)Grass 线性开始 X
uniform half _GrassVAStartX;
// (0,1)Grass 是否使用线性 X
uniform half _GrassVALerp2X;
// (-5,5)Grass 强度与方向 X
uniform half _GrassVAIntensityDirX;
// (0, 2)Grass VA Scale X
uniform half _GrassVAScaleX;
// (float)Grass 线性开始 Y
uniform half _GrassVAStartY;
// (0,1)Grass 是否使用线性 Y
uniform half _GrassVALerp2Y;
// (-5,5)Grass 强度与方向 Y
uniform half _GrassVAIntensityDirY;
// (0, 2)Grass VA Scale Y
uniform half _GrassVAScaleY;
// (flaot)Grass 整体强度
uniform half _GrassVAIntensity;
// (flaot)Grass VA 频率
uniform half _GrassVATimeScale;
// (0, 1)Grass VA 统一性
uniform fixed _GrassVAUnified;
#endif

// Texture Transparent
#if USING_TEX_TRANSPARENCY
// (0, 1) 贴图透明度
uniform half _TexTransparency;
#endif

// 普通透明
#if USING_TRANSPARENCY
// (0, 1) 程序控制透明度
uniform fixed _Transparency;
#endif

// (bool) 是否写入z
uniform fixed _ZWrite;


// 程序修改的MC光照数据
#if USING_MC_LIGHT
// (texture)程序设置天光贴图
uniform sampler2D _SkyLightTex;
// (texture)程序设置火把光贴图
uniform sampler2D _BlockLightTex;
// (bool)是否使用法线做为权重
uniform fixed _NormalWeightedLight;

// ---------------------------------------------
// 问题1：
// 声明的顺序要注意，一定按占用的值的寄存器大小排序
// 如果不按规则排序，iOS在metal的编辑器中就会出现bug
// ---------------------------------------------
// 问题2：
// Unity2017.4.1版本在Android平台下，API OpenGL ES 3.2 V@145.0 (GIT@I07a8b16b27)
// 如果Buffer数据结构体需要限制：
// 必需大于等于24个float
// 必需小于28个float但最多只能8个变量
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


// ========================= 函数集合 - 开始 =========================

// 自发光函数
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

// 当前灯光方向
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

// 草顶点动画函数 - 旧函数
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
// 草顶点动画函数 - 新函数
inline void WavingGrassVert(inout float4 vertex)
{
#if USING_GRASS_VA
	
	// 取世界空间的xyz，给不同xyz的位置的东西做不同的差异化，使附近的顶点动画造成不一致的表现
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
	s = s * s;  //这里做了s的4次方处理，使得不同的y导致的变化幅度更小，摆动更真实

	// 取局部Y轴可控制Y轴上变化的量(小于_GrassVAStartY的顶点变化量为0，越高越变化多)
	half4 waveAmount = lerp(1, max((vertex.x - _GrassVAStartX) * _GrassVAIntensityDirX, 0), _GrassVALerp2X) * lerp(1, max((vertex.y - _GrassVAStartY) * _GrassVAIntensityDirY, 0), _GrassVALerp2Y) * _GrassVAIntensity;
	s *= waveAmount; //计算振幅
	
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



// 计算家具顶点的火把光照
//inline fixed2 CalcVertBlockLight(float3 pos_world, float3 normal, fixed normal_weighted, fixed3 block_size, fixed3 origin_offset, fixed4 sky_light_0123, fixed4 sky_light_4567, fixed4 block_light_0123, fixed4 block_light_4567)
//{
//	// 先计算8个顶点的世界坐标位置
//	float3 pos_0 = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz + origin_offset;
//	float3 pos_1 = pos_0 + float3(block_size.x, 0, 0);
//	float3 pos_2 = pos_0 + float3(block_size.x, 0, block_size.z);
//	float3 pos_3 = pos_0 + float3(0, 0, block_size.z);
//	float3 pos_4 = pos_0 + float3(0, block_size.y, 0);
//	float3 pos_5 = pos_1 + float3(0, block_size.y, 0);
//	float3 pos_6 = pos_2 + float3(0, block_size.y, 0);
//	float3 pos_7 = pos_3 + float3(0, block_size.y, 0);
//
////	// block_size在上面的计算之后要转到世界坐标系
////	block_size = mul(unity_ObjectToWorld, float4(block_size.x, block_size.y, block_size.z, 0)).xyz;
//
//	// 计算8个顶点的距离
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
//	// 权重
//	float4 weight_0123 = 1 / dist_0123;
//	float4 weight_4567 = 1 / dist_4567;
//
//	// 权重修正(刚好在边界的顶点只需要考虑边界的顶点值)
//	fixed3 factor = sign(abs(abs(pos_world - pos_0) - abs(block_size)) - 0.05) + 1;	// 等同于if (abs(abs(pos_world - pos_0) - abs(block_size)) < 0.01) { factor.xyz = 0 } else { factor.xyz = 2 }
//	weight_0123.x *= saturate(factor.x * factor.y * factor.z); 		// 略过不在相同边/面的顶点
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
//	// 加上法线朝向的权重修改	
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
//	// 最终权重
//	float total_weight = weight_0123.x + weight_0123.y + weight_0123.z + weight_0123.w + 
//	weight_4567.x + weight_4567.y + weight_4567.z + weight_4567.w;
//
//	weight_0123 /= total_weight;
//	weight_4567 /= total_weight;
//
//	// 最终光照值
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

// 工具函数，找出x,y,z中最大或最小值
inline half Max3(half3 x)
{
	return max(x.x, max(x.y, x.z));
}

inline half Min3(half3 x)
{
	return min(x.x, min(x.y, x.z));
}

// 一个很小的值
#define EPSILON         1.0e-4

// 计算家具顶点的火把光照
inline fixed2 CalcVertBlockLight(half3 pos_local, fixed3 normal_local, fixed normal_weighted, fixed3 block_size, half4x4 localOffsetMatrix, fixed4 sky_light_0123, fixed4 sky_light_4567, fixed4 block_light_0123, fixed4 block_light_4567)
{
	// 把局部顶点转为方块空间位置
	pos_local = mul(localOffsetMatrix, half4(pos_local, 1));
	normal_local = normalize(mul((half3x3)localOffsetMatrix, normal_local));

	// 先计算8个顶点的方块空间位置
	half3 pos_0 = half3(0, 0, 0);
	half3 pos_1 = pos_0 + half3(block_size.x, 0, 0);
	half3 pos_2 = pos_0 + half3(block_size.x, 0, block_size.z);
	half3 pos_3 = pos_0 + half3(0, 0, block_size.z);
	half3 pos_4 = pos_0 + half3(0, block_size.y, 0);
	half3 pos_5 = pos_1 + half3(0, block_size.y, 0);
	half3 pos_6 = pos_2 + half3(0, block_size.y, 0);
	half3 pos_7 = pos_3 + half3(0, block_size.y, 0);

	// 计算8个顶点的距离
	//half4 dist_0123 = half4(length(pos_0 - pos_local),
	//	length(pos_1 - pos_local),
	//	length(pos_2 - pos_local),
	//	length(pos_3 - pos_local));

	//half4 dist_4567 = half4(length(pos_4 - pos_local),
	//	length(pos_5 - pos_local),
	//	length(pos_6 - pos_local),
	//	length(pos_7 - pos_local));

	// 把距离化为权重
	float4 weight_0123;// = dist_0123;
	float4 weight_4567;// = dist_4567;
	float3 one_over_block_size = 1.0 / block_size;

	// 权重重调比例，把越接近家具Bound的顶点，受远处的顶点光照影响越小，防止两个相同类型家具边上的点与旁边家具的点光照颜色不一样
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


	// 加上法线朝向的权重修改	
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

	// 最终权重
	half total_weight = weight_0123.x + weight_0123.y + weight_0123.z + weight_0123.w +
		weight_4567.x + weight_4567.y + weight_4567.z + weight_4567.w;

	total_weight = max(total_weight, EPSILON);
	weight_0123 /= total_weight;
	weight_4567 /= total_weight;

	// 最终光照值
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

// 从一个float中解析出3个值范围为0~255的int
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

// ========================= Forward主体 =========================

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
	float3 uv : TEXCOORD0;					// 主贴图uv，写入深度时uv.z用来记录深度值
#else
	float2 uv : TEXCOORD0;
#endif

#if USING_BUMP_MAP
	float4 tSpace0 : TEXCOORD1;					// 切线空间(x:Tangent.x, y:Binormal.x, z:Normal.x, w:posWorld.x), 有世界坐标必使用float
	float4 tSpace1 : TEXCOORD2;					// 切线空间(x:Tangent.y, y:Binormal.y, z:Normal.y, w:posWorld.y), 有世界坐标必使用float
	float4 tSpace2 : TEXCOORD3;					// 切线空间(x:Tangent.z, y:Binormal.z, z:Normal.z, w:posWorld.z), 有世界坐标必使用float
#else
	fixed3 normalWorld : TEXCOORD1;			// 世界法线
	float3 posWorld : TEXCOORD2;			// 世界坐标
#endif

	SHADOW_COORDS(4)
	UNITY_FOG_COORDS(5)						// unity雾，只用到一个float
	fixed4 sh : TEXCOORD6;				// 环境光及其它点光源，alpha通道用来存放blockLight

#if USING_MC_LIGHT
	fixed2 mcLight : TEXCOORD7;				// skylight, blocklight值
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

	// 应用顶点动画
	APPLY_GRASS_VA(v.vertex);

	o.pos = UnityObjectToClipPos(v.vertex);
	// uv
	o.uv.xy = v.texcoord;

	// 世界空间顶点位置
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;

#if CALC_DEPTH
	//将顶点深度写入uv.z
	GSTORE_TRANSFER_DEPTH(o.uv.z, v.vertex);
#endif

	// 世界空间法线
	fixed3 normalWorld = UnityObjectToWorldNormal(v.normal);

#if USING_BUMP_MAP
	// 世界空间Tangent
	fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
	// 世界空间Binormal
	fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(normalWorld, worldTangent) * tangentSign;

	// Transfer to frag
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, normalWorld.x, posWorld.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, normalWorld.y, posWorld.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, normalWorld.z, posWorld.z);
#else
	// 输出法线和世界空间顶点位置
	o.posWorld = posWorld;
	o.normalWorld = normalWorld;
#endif
	
	o.sh = 0;
	// vertex程序中的光照颜色和环境光
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
	o.sh.rgb += Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	// 注意：ShadeSH9部分可放到frag里和Bump结合，但效率考虑的话，还是放在vert里，但不会受bump影响
	// 这里包含了环境光
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

	// 贴图颜色
#if USING_MAIN_COLOR
	fixed4 tex_diffuse = _Color;
#else
	fixed4 tex_diffuse = tex2D(_MainTex, i.uv.xy);
#endif

	// 法线贴图(正切空间)
#if USING_BUMP_MAP
	// 世界法线
	fixed3 bump_noarmal = UnpackNormal(tex2D(_BumpMapTex, i.uv.xy));
	fixed3 normalWorld;
	normalWorld.x = dot(i.tSpace0.xyz, bump_noarmal);
	normalWorld.y = dot(i.tSpace1.xyz, bump_noarmal);
	normalWorld.z = dot(i.tSpace2.xyz, bump_noarmal);

	// 把世界坐标组合起来
	float3 posWorld = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);
#else
	fixed3 normalWorld = i.normalWorld;	// 额外的像素光不计算法线贴图
	float3 posWorld = i.posWorld;
#endif

	// 视点单位向量
#if USING_FIXED_DOUBLE_SIZE_NORMAL && USING_FRESNEL_REFLECT
	fixed3 viewDir = normalize(UnityWorldSpaceViewDir(posWorld));
#endif

	// 根据面反转法线
#if USING_FIXED_DOUBLE_SIZE_NORMAL
	fixed facing_sign = facing >= 0 ? 1 : -1;
	normalWorld *= facing_sign;
#endif

	// 计算衰减 compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, i, posWorld)
	// 灯光方向
	fixed3 lightDirection = LightDirection(posWorld);
	// 法线方向
	fixed3 normalDirection = normalize(normalWorld);

	// -------------- 漫反射部分 ------------------
	// 兰伯
	//half3 lambert_term = max(0.0, (0.5 * dot(normalDirection, lightDirection) + 0.5));
	half3 lambert_term = max(0.0, (dot(normalDirection, lightDirection)));

	// 公式计算
	// 公式意义：使用直射光定义亮部颜色
	half3 direct_diffuse = _LightColor0.rgb * lambert_term;
	half3 indirect_diffuse = half3(0, 0, 0);
#if USING_UNITY_SH
	// 环境光随漫反射光改变权重, 把SH光照也加入到间接漫反射
	// 公式意义: 使用环境光定义为暗部颜色
	indirect_diffuse += i.sh.rgb * (1 - lambert_term);
	//indirect_diffuse += i.sh.rgb * (1 - atten);
#endif

	
	// 计算方块光照
	half3 diffuse_part = (direct_diffuse + indirect_diffuse);
#if ENABLE_FITMENT_ALONE
	// 计算衰减
	diffuse_part *= atten;

	// 计算主光强度
	half main_light_luminosity = saturate(Luminance(_LightColor0.rgb));

	// 计算点光强度。需要减去环境光，获取像素点光值
	half3 point_lights = i.sh.rgb;
#if USING_UNITY_SH
	point_lights -= max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
#endif
	half point_lights_luminosity = saturate(Luminance(point_lights));

	// 计算点光光照颜色
	// 1 - main_light_luminosity：	防止主光较强时颜色曝光
	// point_lights_luminosity：	防止环境光较强时颜色曝光
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
	// 下面与上面等价
	half diffuse_luminosity = 1 - (_BaseIlluminWeighted * saturate(Luminance(diffuse_part)));
	emissive += SelfIlluminEmission(tex_diffuse, i.uv.xy) * diffuse_luminosity;
#endif

#if USING_FRESNEL_REFLECT
	// 视点单位向量
	//fixed3 viewDir = normalize(UnityWorldSpaceViewDir(posWorld));
	// 处理单面法线采样问题
#if SHADER_TARGET < 30
	normalWorld = lerp(-normalWorld, normalWorld, ((dot(viewDir, normalWorld) + 1) / 2)); // hack版本
//#else
//	normalWorld = normalWorld * facing; // 正确版本，要求shader model 3.0
#endif
	// 视点单位向量反射向量
	fixed3 viewReflectDir = reflect(-viewDir, normalWorld);
	// 采样cubemap
	fixed4 cubemap_var = texCUBE(_Cubemap, viewReflectDir);
	// 菲涅尔强度计算，hack简化
	fixed fresnel_var = pow(1 - max(0, dot(normalWorld, viewDir)), _FresnelThickness);
	// 获取光照颜色强度，最小值0.2
	fixed3 lightColorStr = max(saturate(1 - max(diffuse_part.r, max(diffuse_part.g, diffuse_part.b))), 0.2);
	// 混合菲涅尔数值计算反射度，关联光照颜色强度
	float Glossiness = max(fresnel_var, _Glossiness) * lightColorStr;
	//float Glossiness = _Glossiness * lightColorStr;
	// 菲涅尔自发光，关联周围光照颜色和Cubemap颜色
	emissive += min(fresnel_var, _EmissiveColor.a) * _EmissiveColor.rgb * diffuse_part * lerp(fixed3(1,1,1), cubemap_var, Glossiness);
	diffuse_part = lerp(diffuse_part, cubemap_var, Glossiness);
#endif 


	// (直接漫反射 + 间接漫反射 + 补偿) * 漫反射颜色
	half3 diffuse = diffuse_part * tex_diffuse;


	fixed final_alpha = 1.0;
#if USING_TEX_TRANSPARENCY
	// -------------- 贴图Alpha部分 ------------------
	#if USING_FRESNEL_REFLECT
	final_alpha *= max(fresnel_var, tex_diffuse.a * _TexTransparency);
	#else
	final_alpha *= tex_diffuse.a * _TexTransparency;
	#endif
#endif
#if USING_TRANSPARENCY
	// -------------- Alpha部分 ----------------------
	// 程序控制
	final_alpha *= _Transparency;
#endif
#if CALC_DEPTH
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.uv.z, _ZWrite, final_alpha);
#endif
	// 最后的颜色
	fixed4 final_color = fixed4(diffuse + emissive, final_alpha);

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);

	// discard 一定要放到最后，否则造成iphone上显示不正确
	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);
	// lod crossfade
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);
	return final_color;
}


#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// ========================= shadow caster 主体 =========================

#if defined(UNITY_PASS_SHADOWCASTER)

struct v2f_shadowcaster
{
	V2F_SHADOW_CASTER;
	// 一定要从TEXCOORD1开始,V2F_SHADOW_CASTER有可能占用TEXCOORD0

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

	// 应用顶点动画
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

	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);

	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
	SHADOW_CASTER_FRAGMENT(i)
}



#endif // defined(UNITY_PASS_SHADOWCASTER)


#endif // __Fitment__
