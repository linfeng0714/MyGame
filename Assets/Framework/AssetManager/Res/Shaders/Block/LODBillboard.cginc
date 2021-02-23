#ifndef __LODBillboard__
#define __LODBillboard__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "SpeedTreeBillboardCommon.cginc"
#if CALC_DEPTH 
#include "../Lib/FastDepthMap.cginc"
#endif
// ----------------- 开关定义 -----------------

// dither 透明
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// -------- shader 变量 --------

// SpeedTreeBillboardCommon.cginc已经定义过
//sampler2D _MainTex;
//fixed _Cutoff;


// 是否使用unity的 sh 光照
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)


// -------------------- 函数集合 - 开始 --------------------

// 当前灯光方向
inline fixed3 LightDirection(float3 posWorld)
{
#ifndef USING_DIRECTIONAL_LIGHT
	return normalize(UnityWorldSpaceLightDir(posWorld));
#else
	return _WorldSpaceLightPos0.xyz;
#endif
}

// -------------------- 函数集合 - 结束 --------------------

// -------- forward --------
#define AMBIENT_LUMIN_MIN 0
#define AMBIENT_LUMIN_MAX 1

half ColorLuminosity(fixed3 col)
{
	return 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
}

fixed3 WeightedAmbient(fixed3 ambient_col, fixed3 col)
{
	fixed3 weighted_ambient_col = ambient_col;

	half lumin = ColorLuminosity(col);
	lumin = clamp(lumin, AMBIENT_LUMIN_MIN, AMBIENT_LUMIN_MAX);
	half ambient_factor = (lumin - AMBIENT_LUMIN_MIN) / (AMBIENT_LUMIN_MAX - AMBIENT_LUMIN_MIN);
	weighted_ambient_col = lerp(weighted_ambient_col, 0, ambient_factor);

	return weighted_ambient_col;
}

#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

struct v2f
{
	UNITY_POSITION(pos);
	fixed3 normalWorld : TEXCOORD0;
#if CALC_DEPTH
	float4 posWorld : TEXCOORD1;
#else
	float3 posWorld : TEXCOORD1;
#endif
	SHADOW_COORDS(2)
	UNITY_FOG_COORDS(3)
#if USING_UNITY_SH
	half3 sh : TEXCOORD4; // SH
#endif
	GSTORE_DITHER_TRANSPARENCY_COORDS(5)	// Dither transparency

	Input data : TEXCOORD6; // LOD crossfade 因为不定个数，所以要放最后

	UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(SpeedTreeBillboardData v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	SpeedTreeBillboardVert(v, o.data);
	// 顶点位置
	o.pos = UnityObjectToClipPos(v.vertex);
	// uv在SpeedTreeBillboardVert里已经设好

	// 世界空间顶点位置
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
	// 世界空间法线
	fixed3 normalWorld = UnityObjectToWorldNormal(v.normal);
	

#if CALC_DEPTH
	//将顶点深度写入o.posWorld.w
	GSTORE_TRANSFER_DEPTH(o.posWorld.w, v.vertex);
#endif
	// 输出法线和世界空间顶点位置
	o.posWorld.xyz = posWorld;
	o.normalWorld = normalWorld;

	// vertex程序中的光照颜色和环境光
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	o.sh = 0;
	// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
	o.sh.rgb += Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, posWorld, normalWorld);
#endif
	// 注意：ShadeSH9部分可放到frag里和Bump结合，但效率考虑的话，还是放在vert里，但不会受bump影响
	// 这里包含了环境光
	o.sh.rgb += max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
#endif

	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);	// 获得屏幕空间位置
	TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);// pass fog coordinates to pixel shader
	return o;
}


fixed4 frag(v2f IN) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(IN);

	// 贴图颜色
	fixed4 tex_color = tex2D(_MainTex, IN.data.mainTexUV);
	clip(tex_color.a - _Cutoff);
	// 计算衰减 compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, IN, IN.posWorld)
	// 灯光方向
	fixed3 lightDirection = LightDirection(IN.posWorld);
	// 法线方向
	fixed3 normalDirection = normalize(IN.normalWorld);


	// 漫反射颜色
	float lambert = max(0, dot(normalDirection, lerp( lightDirection, normalDirection, 0.5)));
	//fixed lambert = max(0, 0.5 * dot(normalDirection, -normalDirection) + 0.5);
	//lambert = 0.3 + 0.7 * lambert;	// 整体提亮
	fixed3 diff_color = _LightColor0.rgb * atten * lambert;

#if USING_UNITY_SH
	fixed3 shCol = WeightedAmbient(IN.sh, diff_color);
	diff_color += shCol;
	//diff_color += IN.sh;
#endif

	float final_alpha = 1.0;
#if CALC_DEPTH
	GSTORE_CALC_DEPTH_TO_ALPHA(final_alpha, IN.posWorld.w, final_alpha);
#endif

	// 最后的颜色
	fixed4 final_color = fixed4(tex_color * diff_color, final_alpha);
	//return fixed4(IN.sh, 1);
	// apply fog
	UNITY_APPLY_FOG(IN.fogCoord, final_color);

	// discard 一定要放到最后，否则造成iphone上显示不正确
	// alpha test
	clip(tex_color.a - _Cutoff);
	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF(IN);
	// LOD corssfade
	UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
	return final_color ;
}

#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// -------- shadow caster --------

#if defined(UNITY_PASS_SHADOWCASTER)


struct v2f
{
	V2F_SHADOW_CASTER;
	// 一定要从TEXCOORD1开始,V2F_SHADOW_CASTER有可能占用TEXCOORD0
	Input data : TEXCOORD1; // LOD crossfade 因为不定个数，所以要放最后

	GSTORE_DITHER_TRANSPARENCY_COORDS(2)

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(SpeedTreeBillboardData v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	SpeedTreeBillboardVert(v, o.data);
	// uv在SpeedTreeBillboardVert里计算好了
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 texcol = tex2D(_MainTex, i.data.mainTexUV);


	// discard 一定要放到最后，否则造成iphone上显示不正确
	// Alpha cutoff
	clip(texcol.a - _Cutoff);
	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);
	// LOD
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

	SHADOW_CASTER_FRAGMENT(i)
}

#endif // defined(UNITY_PASS_SHADOWCASTER)

#endif // __LODBillboard__
