#ifndef __Character__
#define __Character__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#if CALC_DEPTH
#include "../Lib/FastDepthMap.cginc"
#endif
#include "../Lib/IlluminationFormula.cginc"

// MatCap全称MaterailCapture，里面存的是光照信息，通过法线的xy分量去采样matcap，得到在该方向法线的光照信息
//matcap存的其实是：法线中xy分量作为uv，对应的光照颜色信息，即用xy去采样就好，注：xy必须是归一化法线中的分量，故z才没必要


// 高光贴图的不同通道控制不同效果:
// 红 : 控制高光颜色的影响，参与到高光颜色中
// 绿 : 控制高光强度
// 蓝 : 自发光的强度(diffuse贴图的光暗mask的意思)控制(_Color * 主贴图)

// ----------------- 开关定义 -----------------
// 是否使用unity的 sh 光照
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// 普通透明
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// dither 透明
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// cutoff 
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// 普通贴图透明
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// 自发光部分(Add pass不使用)
#define USING_EMISSIVE (!defined(NOT_USE_EMISSIVE) && defined(UNITY_PASS_FORWARDBASE))

// matcap开关(Add pass不使用)
#define USING_MATCAP (!defined(NOT_USE_MATCAP)) && (defined(UNITY_PASS_FORWARDBASE))

// sge开关
#define USING_SGE_TEX !NOT_USE_SGE_TEX

// 石化开关
#define USING_STONE_EFFECT (USE_STONE_EFFECT)

// 高光开关
#define USING_SPECULAR (!defined(NOT_USE_SPECULAR))

// 溶解开关
#define USING_DISSOLVE_EFFECT (USE_DISSOLVE_EFFECT)

// 隐身开关
#define USING_INVISIBLE_EFFECT (USE_INVISIBLE_EFFECT)

// 普通漫反射贴图
uniform sampler2D _diffuse;

#if USING_SGE_TEX
// SGE贴图
uniform sampler2D _sge;
#endif

// 普通漫反射的叠加颜色
uniform fixed4 _Color;
// 高光的强度
uniform fixed _spec;
// 高光的颜色
uniform fixed4 _spec5;

uniform fixed _em2;
uniform fixed4 _em3;

// 光泽度
uniform fixed _gloss;
// 漫反射贴图的比例
uniform fixed _m2lerp;

uniform fixed _ZWrite;


#if USING_TRANSPARENCY
uniform fixed _Transparency;
#endif


#if USING_MATCAP
uniform sampler2D _MatCapTex;
uniform fixed _MatCapIntensity;
uniform fixed4 _MatCapColor;
#endif

#if USING_DISSOLVE_EFFECT
uniform sampler2D _DissTex;
uniform fixed4 _DissStartColor;
uniform fixed4 _DissEndColor;
uniform fixed _DissProgress;
uniform fixed _DissSize;
#endif

#if USING_STONE_EFFECT
// 石化贴图
uniform sampler2D _StoneTex;
// 石化强度
uniform fixed _StoneIntensity;
#endif

#if USE_INVISIBLE_EFFECT
// 隐身程度
uniform fixed _InvisibleVisibility;
// 边缘强度
uniform half _InvisibleRimPower;
#endif



// -------------------- 函数集合 - 结束 --------------------

// ForwardBase/ForwardAdd pass
#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// ----------------- 用于控制环境光 -----------------------
#define MIN_LUMIN 0.6
#define AMBIENT_LUMIN_MIN 0
#define AMBIENT_LUMIN_MAX 1

inline half ColorLuminosity(fixed3 col)
{
	return 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
}

inline fixed3 WeightedAmbient(fixed3 ambient_col, fixed3 col)
{
	fixed3 weighted_ambient_col = ambient_col;

	half lumin = ColorLuminosity(col);
	lumin = clamp(lumin, AMBIENT_LUMIN_MIN, AMBIENT_LUMIN_MAX);
	half ambient_factor = (lumin - AMBIENT_LUMIN_MIN) / (AMBIENT_LUMIN_MAX - AMBIENT_LUMIN_MIN);
	weighted_ambient_col = lerp(weighted_ambient_col, 0, ambient_factor);

	return weighted_ambient_col;
}



// 输入顶点信息
struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
};

struct v2f
{
	UNITY_POSITION(pos);

#if CALC_DEPTH
	float3 uv : TEXCOORD0;					// 主贴图uv，写入深度时uv.z用来记录深度值
#else
	float2 uv : TEXCOORD0;
#endif

	float3 posWorld : TEXCOORD1;					// 世界空间位置
	half3 normalWorld : TEXCOORD2;				// 法线方向
	SHADOW_COORDS(3)						// 接受阴影和点光照(ADDPASS)	
	UNITY_FOG_COORDS(4)					// fog只用一个half
	GSTORE_DITHER_TRANSPARENCY_COORDS(5)	// Dither transparency
#if USING_UNITY_SH
	half3 sh : TEXCOORD6;						// SH
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	// 顶点
	o.pos = UnityObjectToClipPos(v.vertex);
	// UV
	o.uv.xy = v.texcoord;
	// 法线
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);
	o.normalWorld = normalWorld;
	// 世界空间顶点位置
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.posWorld = posWorld;

#if CALC_DEPTH
	//将顶点深度写入uv.z
	GSTORE_TRANSFER_DEPTH(o.uv.z, v.vertex);
	//o.uv.z = min(max(length(posWorld - _WorldSpaceCameraPos.xyz) - 1, 0) * 0.03, 1);
#endif
	// vertex程序中的光照颜色和环境光
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	o.sh = 0;
	// Approximated illumination from non-important point lights
	// 这个宏是必要的，因为unity引擎的点光源用的cull mask就是会开启
#ifdef VERTEXLIGHT_ON
	o.sh += Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	// 注意：ShadeSH9部分可放到frag里和Bump结合，但效率考虑的话，还是放在vert里，但不会受bump影响
	// 这里包含了环境光
	//o.sh += max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
	o.sh += CaleCameraSH(normalWorld);
#endif

	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);	// 获得屏幕空间位置

	TRANSFER_SHADOW(o); 			// pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);	// pass fog coordinates to pixel shader
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);

	// 主贴图颜色
	fixed4 tex_diffuse = tex2D(_diffuse, i.uv);

	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);

	// 控制贴图颜色
#if USING_SGE_TEX
	fixed4 tex_sge = tex2D(_sge, i.uv);
#else
	fixed4 tex_sge = fixed4(0, 0, 0, 0);
#endif
	
	// 计算衰减 compute lighting & shadowing factor(i里必需有pos这个属性)
	UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld)

	// 灯光方向 lightDirection(顶点指向光照)
#ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDirection = normalize(UnityWorldSpaceLightDir(i.posWorld));
#else
	fixed3 lightDirection = _WorldSpaceLightPos0.xyz;
#endif

	// 法线方向
	float3 normalDirection = normalize(i.normalWorld);
	// 由顶点指向镜头
	float3 viewDirection = normalize(UnityWorldSpaceViewDir(i.posWorld));
	// 由镜头指向太阳
	float3 halfDirection = normalize(viewDirection + lightDirection);

	// 获得matcap应用的部分，贴图部分为(1 - mc_mask)
#if USING_SGE_TEX
	fixed mc_mask = (tex_sge.g * _m2lerp);
#else
	fixed mc_mask = 0;
#endif

	// 漫反射颜色(经过mask)
#if USING_STONE_EFFECT
	// 石化贴图
	fixed4 tex_stone = tex2D(_StoneTex, i.uv);
	fixed3 diffuse_map = lerp((_Color.rgb * tex_diffuse.rgb) * (1.0 - mc_mask), tex_stone.rgb, _StoneIntensity);
	tex_diffuse.rgb = lerp(tex_diffuse.rgb, tex_stone.rgb, _StoneIntensity);
#else
	fixed3 diffuse_map = ((_Color.rgb * tex_diffuse.rgb) * (1.0 - mc_mask));
#endif

	// -------------- Unity光部分 ------------------
	// Unity光照颜色 + 影子衰减
	half3 atten_light_color = _LightColor0.rgb * atten;
	
	half3 specular = half3(0, 0, 0);
#if USING_SPECULAR
	// -------------- 高光部分 ------------------
	// 贴图叠加高光颜色
	half3 specular_color = (diffuse_map * ((_spec5.rgb * _spec) * tex_sge.r));
	// 获得高光的真正颜色(Gloss光泽度由_gloss计算所得)
	half3 direct_specular = atten_light_color * pow(max(0, dot(halfDirection, normalDirection)), exp2(_gloss * 10.0 + 1.0)) * specular_color;
	specular += direct_specular;

	
#if USING_STONE_EFFECT
	// 石化后控制系数
	specular *= 1.0 - _StoneIntensity;
#endif // USING_STONE_EFFECT

#endif

	// -------------- 漫反射部分 ------------------
	// 兰伯
	//half3 lambert_term = max(0.0, (0.5 * dot(normalDirection, lightDirection) + 0.5));
	half3 lambert_term = max(0.0, (dot(normalDirection, lightDirection)));

	// 公式计算
	// 公式意义：使用直射光定义亮部颜色
	half3 direct_diffuse = lambert_term * atten_light_color;

	// 间接漫反射
	half3 indirect_diffuse = half3(0, 0, 0);

#if USING_UNITY_SH
	// 环境光随漫反射光改变权重, 把SH光照也加入到间接漫反射
	//indirect_diffuse += i.sh.rgb * (1 - lambert_term);
	indirect_diffuse += WeightedAmbient(i.sh.rgb, direct_diffuse);
#endif

	half3 extra_diffuse = half3(0, 0, 0);
#if (USING_EMISSIVE && defined(UNITY_PASS_FORWARDBASE))
	// 亮度补偿
	float diffuse_lumin = ColorLuminosity(direct_diffuse + indirect_diffuse);
	float emissive_lumin = ColorLuminosity(_em3.rgb);
	float extraLumin = saturate(MIN_LUMIN - diffuse_lumin);
	extra_diffuse += (extraLumin * _em3.rgb / emissive_lumin);
#endif
	// (直接漫反射 + 间接漫反射 + 补偿) * 漫反射颜色
	half3 diffuse = saturate(direct_diffuse + indirect_diffuse + extra_diffuse) * diffuse_map;

	half3 emissive = half3(0, 0, 0);
#if USING_EMISSIVE || USING_MATCAP
	// 降diffuse的饱和度,辅助贴图(自发光和MatCap使用)
	fixed3 desaturate_diffuse = lerp(tex_diffuse.rgb, dot(tex_diffuse.rgb, float3(0.3, 0.59, 0.11)), mc_mask);
#endif

#if USING_EMISSIVE
	// -------------- 自发光部分 ------------------
	fixed emission_factor = 1.0;
#if USING_STONE_EFFECT
	// 石化后控制系数
	emission_factor *= 1.0 - _StoneIntensity;
#endif  // USING_STONE_EFFECT

	// 降低diffuse贴图饱和度
	emissive += (tex_sge.b * desaturate_diffuse * (_em3.rgb * _em2)) * emission_factor;
#endif // USING_EMISSIVE

#if USING_MATCAP
	// -------------- MatCap部分 ------------------
	fixed matcap_factor = 1.0;
#if USING_STONE_EFFECT
	// 石化后控制系数
	matcap_factor *= 1.0 - _StoneIntensity;
#endif // USING_STONE_EFFECT

	// MaterailCapture贴图颜色
	fixed4 tex_MatCapTex = tex2D(_MatCapTex, (mul(UNITY_MATRIX_V, float4(normalDirection, 0)).xy * 0.5 + 0.5));
	emissive += (mc_mask * ((desaturate_diffuse * tex_MatCapTex.rgb * _MatCapColor.rgb) + pow(max(max(tex_MatCapTex.r, tex_MatCapTex.g), tex_MatCapTex.b), 3.0)) * _MatCapIntensity) * matcap_factor;

#endif // USING_MATCAP


	fixed final_alpha = 1.0;
#if USING_TEX_TRANSPARENCY
	// -------------- 贴图Alpha部分 ------------------
	final_alpha *= tex_diffuse.a;
#endif
#if USING_TRANSPARENCY
	// -------------- Alpha部分 ------------------
	final_alpha *= _Transparency;
#endif
#if CALC_DEPTH
	//final_alpha = i.uv.z;
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.uv.z, _ZWrite, final_alpha);
#endif
	// -------------- 组合最终色 ------------------
	fixed4 final_color = fixed4(diffuse + specular + emissive, final_alpha);


#if USING_DISSOLVE_EFFECT
	// -------------- 溶解 ------------------
	fixed tex_dissAlpha = tex2D(_DissTex, i.uv).r;
	fixed alphaDist = tex_dissAlpha - _DissProgress + 0.01;
	fixed remainPart = max(0, sign(alphaDist));		// 0表示应该完全透明
	clip(remainPart - 0.1);
#if defined(UNITY_PASS_FORWARDBASE)
	fixed dissFactor = saturate(alphaDist / _DissSize);
	fixed4 dissColor = lerp(_DissStartColor, _DissEndColor, dissFactor);
	fixed dissPart = max(0, sign(_DissSize - alphaDist));	// 1表示处于溶解渐变区, 0表示超出溶解渐变区
	final_color.rgb = dissColor.rgb * dissPart + (1.0 - dissPart) * final_color.rgb;
#endif
#endif // USING_DISSOLVE_EFFECT

#if USE_INVISIBLE_EFFECT
	// -------------- 隐身 ------------------
	// 颜色可见
	fixed col_visibility = saturate((_InvisibleVisibility - 0.5) * 2);
	// 边缘光可见
	fixed rim_visibility = saturate(_InvisibleVisibility * 2);
	// 边缘光的部分
	fixed rim_term = 1.0 - saturate(abs(dot(viewDirection, normalDirection)));
	rim_term = pow(rim_term, _InvisibleRimPower);
	fixed4 rim_color = rim_term * rim_visibility;
	rim_color.a *= 0.8;
#if defined(UNITY_PASS_FORWARDBASE)
	final_color.rgba = lerp(rim_color, final_color.rgba, col_visibility);
#else
	final_color.rgba = lerp(0, final_color.rgba, col_visibility);
#endif
#endif // USE_INVISIBLE_EFFECT

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);
	return final_color;
}

#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// -------- 角色被遮挡描边 see through --------
#if defined(USE_SEE_THROUGH)

/*
ZTest Greater
ZWrite Off
Blend SrcAlpha OneMinusSrcAlpha
Cull Front
*/
// "Queue" = "Geometry+455"


uniform float _SeeThroughOutline;
uniform float _SeeThroughRimCutoff;
uniform fixed4 _SeeThroughOutlineColor;

// 输入顶点信息
struct appdata_seethrough
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
};

struct v2f_seethrough
{
	UNITY_POSITION(pos);
	float2 uv : TEXCOORD0;
	half3 worldNormal : TEXCOORD1;
	half3 worldViewDir: TEXCOORD2;

	UNITY_VERTEX_OUTPUT_STEREO
};

v2f_seethrough vert_seethrough(appdata_seethrough v)
{
	v2f_seethrough o;
	UNITY_INITIALIZE_OUTPUT(v2f_seethrough, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord;
	o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
	o.worldViewDir = mul((float3x3)unity_ObjectToWorld, ObjSpaceViewDir(v.vertex));

	TRANSFER_VERTEX_TO_FRAGMENT(o);

	return o;
}

fixed4 frag_seethrough(v2f_seethrough i) : SV_Target
{
	float rim_term = 1.0 - saturate(abs(dot(normalize(i.worldViewDir), normalize(i.worldNormal))));
	rim_term = pow(rim_term, 10 * _SeeThroughOutline);

	rim_term = rim_term < _SeeThroughRimCutoff ? 0 : rim_term;

	fixed4 fragColor;
	fragColor.rgb = _SeeThroughOutlineColor.rgb;
	fragColor.a = rim_term;

	return fragColor;
}

#endif // defined(USE_SEE_THROUGH)

// -------- shadow caster --------

#if defined(UNITY_PASS_SHADOWCASTER)


struct v2f_shadowcaster
{
	V2F_SHADOW_CASTER;

	// 一定要从TEXCOORD1开始,V2F_SHADOW_CASTER有可能占用TEXCOORD0
#if GSTORE_USING_ALPHA_CUTOFF
	float2 uv : TEXCOORD1;					// 主贴图的UV
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

v2f_shadowcaster vert(appdata_base v)
{
	v2f_shadowcaster o;
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if GSTORE_USING_ALPHA_CUTOFF
		o.uv = v.texcoord.xy;
#endif

	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	return o;
}

float4 frag(v2f_shadowcaster i) : SV_Target
{
	GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(_diffuse, i.uv);

	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);

	SHADOW_CASTER_FRAGMENT(i)
}

#endif // defined(UNITY_PASS_SHADOWCASTER)

#endif // __Character__
