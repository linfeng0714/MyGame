#ifndef __CubeOpaqueExploder__
#define __CubeOpaqueExploder__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= 可在Shader使用的开关功能宏 =========================
// Shader中使用Define定义:
// USE_FORCE_TEX_ATLAS - 强制使用图集方式，使用后可达到优化目的


// ========================= 开关定义 =========================
// 按目的分两类:
// 1.效果开关
// 2.优化开关，优化开关就是牺牲一点的可设置性来达到优化目的。
// 按设置方式分两类:
// 1.静态设置，在Shader里直接使用#define来设置(这种不会产生变体)
// 2.动态设置，在Shader里使用multi_compile或shader_feature来设置(这种会产生变体)
// 为什么要分USE_XXX和USING_XXX呢？因为前者是给用户设定的，后者为编码时使用的，两者的意义有可能不一样。

// 是否能使用TextureArray
#define USEING_TEXTURE_ARRAY (SHADER_TARGET >= 35) && !USE_FORCE_TEX_ATLAS

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;     // xy:_MainTex z:tex_index
};

struct v2f
{
	float4 pos : SV_POSITION;
	fixed4 pack_uv : TEXCOORD0;			// TextureArray( xy:_MainTex z:tex_index)     Atlas(xy:图集uv)
	fixed3 worldNormal : TEXCOORD1;
#if CALC_DEPTH
	float4 worldPos : TEXCOORD2;					// worldPos，写入深度时worldPos.w用来记录深度值
#else
	float3 worldPos : TEXCOORD2;
#endif
	SHADOW_COORDS(3)
	UNITY_FOG_COORDS(4)
#if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD5; // SH
#endif
	
};

#if USEING_TEXTURE_ARRAY
// (texture)主贴图
UNITY_DECLARE_TEX2DARRAY(_MainTex);
#else
// (texture)主贴图
uniform sampler2D _MainTexAtlas;
#endif


// 颜色
fixed4 _Color;

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	o.pos = UnityObjectToClipPos(v.vertex);

	// uv
#if USEING_TEXTURE_ARRAY
	o.pack_uv = v.texcoord;
#else
	// 1 / 16 = 0.0625
	// 用的是图集
	o.pack_uv.x = (v.texcoord.z % 16) * 0.0625 + (v.texcoord.x * 0.0625);
	o.pack_uv.y = (1 - ((floor(v.texcoord.z / 16) + 1) * 0.0625)) + (v.texcoord.y * 0.0625);
#endif

	// 世界空间顶点位置
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	// 世界空间法线
	fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);

	o.worldPos.xyz = worldPos;

#if CALC_DEPTH
	//将顶点深度写入posWorld.w
	GSTORE_TRANSFER_DEPTH(o.worldPos.w, v.vertex);
#endif

	o.worldNormal = worldNormal;

	// vertex程序中的光照颜色
	// SH/ambient and vertex lights
#ifndef LIGHTMAP_ON
#if UNITY_SHOULD_SAMPLE_SH
	o.sh = 0;
	// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
	o.sh += Shade4PointLights(
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, worldPos.xyz, worldNormal);
#endif
	//o.sh += max(half3(0, 0, 0), ShadeSH9(half4(worldNormal, 1.0)));
	o.sh += CaleCameraSH(worldNormal);
#endif
#endif // !LIGHTMAP_ON

	TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);// pass fog coordinates to pixel shader
	return o;
}



fixed4 frag(v2f i) : SV_Target
{
	float3 posWorld = i.worldPos.xyz;
	fixed3 normalWorld = i.worldNormal;

	// 计算衰减 compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, i, posWorld)
	// 灯光方向
	fixed3 lightDirection = UnityWorldSpaceLightDir(posWorld);
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
#if UNITY_SHOULD_SAMPLE_SH
	// 公式意义: 使用环境光定义为暗部颜色
	indirect_diffuse += i.sh * (1 - lambert_term);
#endif

	half3 diffuse_part = (direct_diffuse + indirect_diffuse) * atten;

	// 贴图颜色+ 叠加颜色
#if USEING_TEXTURE_ARRAY
	fixed4 tex_color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.pack_uv.x, i.pack_uv.y, i.pack_uv.z)) * _Color;
#else
	fixed4 tex_color = tex2D(_MainTexAtlas, i.pack_uv.xy) * _Color;
#endif

	// 最后的颜色
	fixed4 final_color = fixed4(tex_color * diffuse_part, tex_color.a);
#if CALC_DEPTH
	GSTORE_CALC_DEPTH_TO_ALPHA(final_color.a, i.worldPos.w, final_color.a);
#endif
//#ifdef UNITY_PASS_FORWARDADD
//	final_color.a = 1.0;
//#endif

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);
	return final_color;
}

#endif // __CubeOpaqueExploder__
