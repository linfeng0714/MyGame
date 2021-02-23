#ifndef __Cube__
#define __Cube__

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= 可在Shader使用的开关功能宏 =========================
// Shader中使用Define定义:
// DISABLE_BLOCK_BUMP - 屏蔽使用方块边的法线，使用后可达到优化目的
// DISABLE_BLOCK_BLEND - 屏蔽使用方块混合，使用后可达到优化目的
// DISABLE_BLOCK_AO - 屏蔽使用AO，使用后可达到优化目的
// DISABLE_MC_LIGHT - 屏蔽使用MC光照，使用后可达到优化目的，但不建议使用
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

// dither 透明
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// MC光照
#define USING_MC_LIGHT !defined(DISABLE_MC_LIGHT) && defined(UNITY_PASS_FORWARDBASE)

// 是否使用unity的 sh 光照
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// 是否使用Alpha cutoff
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// 普通贴图透明
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// 普通透明
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// 倒角法线
#define USING_BLOCK_BUMP (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_BUMP))

//法线
#define USING_BLOCK_NORMAL_TEX (defined(UNITY_PASS_FORWARDBASE) && defined(ENABLE_BLOCK_NORMAL_TEX) && defined(USEING_TEXTURE_ARRAY))

// 混合
#define USING_BLOCK_BLEND (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_BLEND) && defined(USEING_TEXTURE_ARRAY))

// AO
#define USING_BLOCK_AO (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_AO))

// ========================= 属性 - 开始 =========================

#if USEING_TEXTURE_ARRAY
// (texture)主贴图
UNITY_DECLARE_TEX2DARRAY(_MainTex);				// 主贴图集
// (texture)混合贴图
UNITY_DECLARE_TEX2DARRAY(_BlendTex);			// 混合贴图集
// (texture)法线贴图
UNITY_DECLARE_TEX2DARRAY(_NormalTex);			// 法线贴图集
#else
// (texture)主贴图
uniform sampler2D _MainTexAtlas;
#endif

uniform float _TJunctionOffset;

uniform fixed _ZWrite;


#if USING_MC_LIGHT
uniform sampler2D _SkyLightTex;					// 全局天光颜色图
uniform sampler2D _BlockLightTex;				// 全局火把光颜色图
#endif

#if USING_BLOCK_BUMP
uniform sampler2D _BumpMap;								// 倒角法线贴图
//uniform fixed _BumpScale;								// 倒角缩放法线值
//uniform fixed _BumpIntensity;							// 倒角边吸收环境光强度
#endif

#if USING_BLOCK_BLEND
uniform sampler2D _BlockBlendMaskTex;					// 混合mask
float4 _BlockBlendMaskTex_TexelSize;
#endif

// AO Effect
#if USING_BLOCK_AO
uniform fixed _AO_Blend;
uniform fixed4 _AO_Color;
uniform fixed _AO_Size;
#endif


// 普通透明
#if USING_TRANSPARENCY
uniform fixed _Transparency;
#endif

// ========================= 函数集合 - 开始 =========================

// 当前灯光方向
inline fixed3 LightDirection(float3 posWorld)
{
#ifndef USING_DIRECTIONAL_LIGHT
	return normalize(UnityWorldSpaceLightDir(posWorld));
#else
	return _WorldSpaceLightPos0.xyz;
#endif
}



// ========================= 方块常量集合 - 开始 =========================


//uniform fixed3 CUBE_FACE_NORMALS[6];
//uniform fixed2 CUBE_QUAD_UVS[4];

// 解决T-junction的像素误差问题(GreedyMesh合并quad引起的), 顶点稍微外扩重叠一点
// ref:https://www.reddit.com/r/VoxelGameDev/comments/3xsp09/are_there_any_good_solutions_for_tjunction/
static const float3 TJUNCTION_OFFSET[24] = {
	// down
	float3(-1, 0, -1),
	float3(1, 0, -1),
	float3(1, 0, 1),
	float3(-1, 0, 1),

	// up
	float3(-1, 0, 1),
	float3(1, 0, 1),
	float3(1, 0, -1),
	float3(-1, 0, -1),

	// north
	float3(1, 1, 0),
	float3(-1, 1, 0),
	float3(-1, -1, 0),
	float3(1, -1, 0),

	// south
	float3(-1, 1, 0),
	float3(1, 1, 0),
	float3(1, -1, 0),
	float3(-1, -1, 0),

	// west
	float3(0, 1, 1),
	float3(0, 1, -1),
	float3(0, -1, -1),
	float3(0, -1, 1),

	// east
	float3(0, 1, -1),
	float3(0, 1, 1),
	float3(0, -1, 1),
	float3(0, -1, -1),
};

#define VECTOR3_DOWN fixed3(0.0, -1.0, 0.0)
#define VECTOR3_UP fixed3(0.0, 1.0, 0.0)
#define VECTOR3_FORWARD fixed3(0.0, 0.0, 1.0)
#define VECTOR3_BACK fixed3(0.0, 0.0, -1.0)
#define VECTOR3_LEFT fixed3(-1.0, 0.0, 0.0)
#define VECTOR3_RIGHT fixed3(1.0, 0.0, 0.0)

// 6个面的法线常量, 顶点里面只传入法线索引
static const fixed3 CUBE_FACE_NORMALS[8] = {
	VECTOR3_DOWN,			// down
	VECTOR3_UP,			// up
	VECTOR3_FORWARD,			// north
	VECTOR3_BACK,			// south
	VECTOR3_LEFT,			// west
	VECTOR3_RIGHT, 			// east

	// 因为shader的编译器优化，会把数据打乱，增加一点冗余数据，防止优化
	// hack for shader complier optimizing
	fixed3(1, 0.1, 0), 			// east
	fixed3(1, 0.1, 0), 			// east

	//// Down y-
	//// 先水平后垂直  x为水平，z为垂直
	//// 索引0
	//// -x
	//normalize(VECTOR3_DOWN + VECTOR3_LEFT),
	//// -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK),
	//// -x & -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK + VECTOR3_LEFT),
	//// 索引1
	//// +x
	//normalize(VECTOR3_DOWN + VECTOR3_RIGHT),
	//// -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK),
	//// +x & -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK + VECTOR3_RIGHT),
	//// 索引2
	//// +x
	//normalize(VECTOR3_DOWN + VECTOR3_RIGHT),
	//// +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD),
	//// +x & +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// 索引3
	//// -x
	//normalize(VECTOR3_DOWN + VECTOR3_LEFT),
	//// +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD),
	//// -x & +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD + VECTOR3_LEFT),

	//// Up y+
	//// 先水平后垂直  x为水平，z为垂直
	//// 索引0
	//// -x
	//normalize(VECTOR3_UP + VECTOR3_LEFT),
	//// +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD),
	//// -x & +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD + VECTOR3_LEFT),
	//// 索引1
	//// +x
	//normalize(VECTOR3_UP + VECTOR3_RIGHT),
	//// +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD),
	//// +x & +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// 索引2
	//// +x
	//normalize(VECTOR3_UP + VECTOR3_RIGHT),
	//// -z
	//normalize(VECTOR3_UP + VECTOR3_BACK),
	//// +x & -z
	//normalize(VECTOR3_UP + VECTOR3_BACK + VECTOR3_RIGHT),
	//// 索引3
	//// -x
	//normalize(VECTOR3_UP + VECTOR3_LEFT),
	//// -z
	//normalize(VECTOR3_UP + VECTOR3_BACK),
	//// -x & -z
	//normalize(VECTOR3_UP + VECTOR3_BACK + VECTOR3_LEFT),

	//// North z+
	//// 先水平后垂直  x为水平，y为垂直
	//// 索引0
	//// +x
	//normalize(VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP + VECTOR3_RIGHT),
	//// 索引1
	//// -x
	//normalize(VECTOR3_FORWARD + VECTOR3_LEFT),
	//// +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP),
	//// -x & +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP + VECTOR3_LEFT),
	//// 索引2
	//// -x
	//normalize(VECTOR3_FORWARD + VECTOR3_LEFT),
	//// -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN),
	//// -x & -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN + VECTOR3_LEFT),
	//// 索引3
	//// +x
	//normalize(VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN),
	//// +x & -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN + VECTOR3_RIGHT),

	//// South z-
	//// 先水平后垂直  x为水平，y为垂直
	//// 索引0
	//// -x
	//normalize(VECTOR3_BACK + VECTOR3_LEFT),
	//// +y
	//normalize(VECTOR3_BACK + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_BACK + VECTOR3_UP + VECTOR3_LEFT),
	//// 索引1
	//// +x
	//normalize(VECTOR3_BACK + VECTOR3_RIGHT),
	//// +y
	//normalize(VECTOR3_BACK + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_BACK + VECTOR3_UP + VECTOR3_RIGHT),
	//// 索引2
	//// +x
	//normalize(VECTOR3_BACK + VECTOR3_RIGHT),
	//// -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN),
	//// +x & -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN + VECTOR3_RIGHT),
	//// 索引3
	//// -x
	//normalize(VECTOR3_BACK + VECTOR3_LEFT),
	//// -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN),
	//// -x & -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN + VECTOR3_LEFT),

	//// West x-
	//// 先水平后垂直 z为水平，y为垂直
	//// 索引0
	//// +z
	//normalize(VECTOR3_LEFT + VECTOR3_FORWARD),
	//// +y
	//normalize(VECTOR3_LEFT + VECTOR3_UP),
	//// +y & +z
	//normalize(VECTOR3_LEFT + VECTOR3_UP + VECTOR3_FORWARD),
	//// 索引1
	//// -z
	//normalize(VECTOR3_LEFT + VECTOR3_BACK),
	//// +y
	//normalize(VECTOR3_LEFT + VECTOR3_UP),
	//// +y & -z
	//normalize(VECTOR3_LEFT + VECTOR3_UP + VECTOR3_BACK),
	//// 索引2
	//// -z
	//normalize(VECTOR3_LEFT + VECTOR3_BACK),
	//// -y
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN),
	//// -y & -z
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN + VECTOR3_BACK),
	//// 索引3
	//// +z
	//normalize(VECTOR3_LEFT + VECTOR3_FORWARD),
	//// -y
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN),
	//// -y & +z
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN + VECTOR3_FORWARD),

	//// East x+
	//// 先水平后垂直 z为水平，y为垂直
	//// 索引0
	//// -z
	//normalize(VECTOR3_RIGHT + VECTOR3_BACK),
	//// +y
	//normalize(VECTOR3_RIGHT + VECTOR3_UP),
	//// +y & -z
	//normalize(VECTOR3_RIGHT + VECTOR3_UP + VECTOR3_BACK),
	//// 索引1
	//// +z
	//normalize(VECTOR3_RIGHT + VECTOR3_FORWARD),
	//// +y
	//normalize(VECTOR3_RIGHT + VECTOR3_UP),
	//// +y & +z
	//normalize(VECTOR3_RIGHT + VECTOR3_UP + VECTOR3_FORWARD),
	//// 索引2
	//// +z
	//normalize(VECTOR3_RIGHT + VECTOR3_FORWARD),
	//// -y
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN),
	//// -y & +z
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN + VECTOR3_FORWARD),
	//// 索引3
	//// -z
	//normalize(VECTOR3_RIGHT + VECTOR3_BACK),
	//// -y
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN),
	//// -y & -z
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN + VECTOR3_BACK),

	//// 因为shader的编译器优化，会把数据打乱，增加一点冗余数据，防止优化
	//// 这个现象发现在使用Metal的Mac上
	//// hack for shader complier optimizing
	//fixed3(1.0, 0.1, 0.0),
	//fixed3(1.0, 0.1, 0.0),
};

// 4个顶点的标准uv, 顶点里面只传入uv索引
static const fixed2 CUBE_QUAD_UVS[8] = {
	fixed2(0.0, 1.0),
	fixed2(1.0, 1.0),
	fixed2(1.0, 0.0),
	fixed2(0.0, 0.0),

	// 因为shader的编译器优化，会把数据打乱，增加一点冗余数据，防止优化
	// hack for shader complier optimizing
	fixed2(0.1, 0.2),
	fixed2(0.3, 0.4),
	fixed2(0.5, 0.6),
	fixed2(0.7, 0.8),
};

#if USING_BLOCK_BUMP

//uniform fixed4 CUBE_FACE_TANGENTS[6];

//// 6个面的切线常量, 顶点里面只传入切线索引
static const fixed4 CUBE_FACE_TANGENTS[8] = {
	fixed4(1.0, 0.0, 0.0, -1.0),			// down y-
	fixed4(1.0, 0.0, 0.0, -1.0),			// up y+
	fixed4(-1.0, 0.0, 0.0, -1.0),			// north z+
	fixed4(1.0, 0.0, 0.0, -1.0),			// south z-
	fixed4(0.0, 0.0, -1.0, -1.0),			// west x-
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east x+

	// 因为shader的编译器优化，会把数据打乱，增加一点冗余数据，防止优化
	// hack for shader complier optimizing
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east
	
	//// Down y-
	//// 索引0
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),
	//// 索引1
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// 索引2
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),
	//// 索引3
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),

	//// Up y+
	//// 索引0
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// 索引1
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),
	//// 索引2
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),
	//// 索引3
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),

	//// North z+
	//// 索引0
	//fixed4(-0.7071067,0,0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,0.4082484,0.4082484,-1),
	//// 索引1
	//fixed4(-0.7071067,0,-0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,-0.4082484,-0.4082484,-1),
	//// 索引2
	//fixed4(-0.7071067,0,-0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,0.4082484,-0.4082484,-1),
	//// 索引3
	//fixed4(-0.7071067,0,0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,-0.4082484,0.4082484,-1),

	//// South z-
	//// 索引0
	//fixed4(0.7071067,0,-0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),
	//// 索引1
	//fixed4(0.7071067,0,0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),
	//// 索引2
	//fixed4(0.7071067,0,0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// 索引3
	//fixed4(0.7071067,0,-0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),

	//// West x-
	//// 索引0
	//fixed4(-0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(-0.4082484,0.4082484,-0.8164965,-1),
	//// 索引1
	//fixed4(0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(0.4082484,-0.4082484,-0.8164965,-1),
	//// 索引2
	//fixed4(0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(0.4082484,0.4082484,-0.8164965,-1),
	//// 索引3
	//fixed4(-0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(-0.4082484,-0.4082484,-0.8164965,-1),

	//// East x+
	//// 索引0
	//fixed4(0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(0.4082484,0.4082484,0.8164965,-1),
	//// 索引1
	//fixed4(-0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(-0.4082484,-0.4082484,0.8164965,-1),
	//// 索引2
	//fixed4(-0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(-0.4082484,0.4082484,0.8164965,-1),
	//// 索引3
	//fixed4(0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(0.4082484,-0.4082484,0.8164965,-1),

	//// 因为shader的编译器优化，会把数据打乱，增加一点冗余数据，防止优化
	//// 这个现象发现在使用Metal的Mac上
	//// hack for shader complier optimizing
	//fixed4(0.1, 0.1, 1.0, -1.0),
	//fixed4(0.1, 0.1, 1.0, -1.0),
};

// 16个倒角法线贴图(第一行)的uv, 顶点里面只传入倒角法线贴图的索引, 索引值范围为0~63, 索引16~63的自动计算uv.y的偏移
static const fixed2 BUMP_UVS[16] = {
	fixed2(0.0625, 0.9375),
	fixed2(0.1875, 0.9375),
	fixed2(0.1875, 0.8125),
	fixed2(0.0625, 0.8125),

	fixed2(0.3125, 0.9375),
	fixed2(0.4375, 0.9375),
	fixed2(0.4375, 0.8125),
	fixed2(0.3125, 0.8125),

	fixed2(0.5625, 0.9375),
	fixed2(0.6875, 0.9375),
	fixed2(0.6875, 0.8125),
	fixed2(0.5625, 0.8125),

	fixed2(0.8125, 0.9375),
	fixed2(0.9375, 0.9375),
	fixed2(0.9375, 0.8125),
	fixed2(0.8125, 0.8125)
};

#endif

inline fixed ao_smoothstep(fixed t, fixed size)
{
	return (t / size);
}

// 从一个float中解析出3个值范围为0~255的int
// ref:https://www.gamedev.net/forums/topic/556757-unpack-4-bytes-from-an-int-in-glsl/
half3 UnpackValues(float packedVal)
{
	float3 unpackedVals = float3(1.0, 256.0, 65536.0);
	unpackedVals = frac(unpackedVals * packedVal);

	return floor(unpackedVals * 256.0);
}

// 从1个8位的int中, 解析出tilesUV的两个值, 高4位为tilesU, 低4位为tilesV
inline half2 UnpackTilesUV(float packTilesUV)
{
	return half2(floor(packTilesUV / 16) + 1, fmod(packTilesUV, 16) + 1);
}



// ========================= Forward主体 =========================

#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)



struct appdata
{
	float4 vertex : POSITION;
	float3 packData : TEXCOORD0;		// 依次包含了9个int(0 ~ 256)数据:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	float4 color : COLOR;				// (color, r:ao值, g:skylight值, b:blocklight值)
};

struct v2f
{
	UNITY_POSITION(pos);
	fixed4 color : COLOR;						// (color, r:ao值, g:skylight值, b:blocklight值)
	float4 main_bump_uv : TEXCOORD0;			// (xy:mainTexUV, zw:bumpTexUV)
#if USING_BLOCK_BLEND
	half4 blend_tex_indices : TEXCOORD1;		// 混合贴图在TextureArray中的索引(xyzw对应北西南东 值:0-255)
#endif
#if USING_BLOCK_BUMP
	float4 tSpace0 : TEXCOORD2;					// 切线空间(x:Tangent.x, y:Binormal.x, z:Normal.x, w:posWorld.x), 有世界坐标必使用float
	float4 tSpace1 : TEXCOORD3;					// 切线空间(x:Tangent.y, y:Binormal.y, z:Normal.y, w:posWorld.y), 有世界坐标必使用float
	float4 tSpace2 : TEXCOORD4;					// 切线空间(x:Tangent.z, y:Binormal.z, z:Normal.z, w:posWorld.z), 有世界坐标必使用float
#else
	fixed3 normalWorld : TEXCOORD2;				// 没法线时，也要传法线
	float3 posWorld : TEXCOORD3;				// 没法线时，也要传世界位置
#endif

	SHADOW_COORDS(5)
	UNITY_FOG_COORDS_PACKED(6, float2)			// x: fogCoord.x, y: 主贴图索引      fog只用一个half，合理利用，y用了主贴图的TextureArray索引(0-255)

#if USING_UNITY_SH
	half3 sh : TEXCOORD7;						// SH
#endif


	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata v)
{
	v2f o;  
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	// 从packData.xyz中解析出9个int数据:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	half3 tex_uv_bumpUV = UnpackValues(v.packData.x);
	half3 normal_tilesuv_northBlend = UnpackValues(v.packData.y);
	half3 westBlend_southBlend_eastBlend = UnpackValues(v.packData.z);

	float4 offset_vertex = v.vertex;
	//offset_vertex.xyz += TJUNCTION_OFFSET[normal_tilesuv_northBlend.x * 4 + tex_uv_bumpUV.y] * _TJunctionOffset;
	o.pos = UnityObjectToClipPos(offset_vertex);
	
	
#if USING_BLOCK_BUMP
	// 倒角bump uv
	int mod_bump_uv_index = tex_uv_bumpUV.z % 16;
	int bump_uv_row_index = floor(tex_uv_bumpUV.z / 16);

	fixed2 bump_uv = BUMP_UVS[mod_bump_uv_index];
	bump_uv.y -= bump_uv_row_index * 0.25;

	o.main_bump_uv.zw = bump_uv.xy;
#endif

	// v2f:主贴图索引
	o.fogCoord.y = tex_uv_bumpUV.x;

	// 主贴图uv
	float2 uv = CUBE_QUAD_UVS[tex_uv_bumpUV.y];
#if USEING_TEXTURE_ARRAY
	// 平铺，现在不用了，因为没有合并mesh
	//half2 tilesUV = UnpackTilesUV(normal_tilesuv_northBlend.y);
	//uv.x *= tilesUV.x;
	//uv.y *= tilesUV.y;
	// 传的是单张贴图uv
	o.main_bump_uv.xy = uv.xy;
#else
	// 1 / 16 = 0.0625
	// 用的是图集
	o.main_bump_uv.x = (tex_uv_bumpUV.x % 16) * 0.0625 + (uv.x * 0.0625);
	o.main_bump_uv.y = (1 - ((floor(tex_uv_bumpUV.x / 16) + 1) * 0.0625)) + (uv.y * 0.0625);
	
#endif

#if USING_BLOCK_BLEND
	// v2f:四边混合贴图索引
	o.blend_tex_indices = half4(normal_tilesuv_northBlend.z, westBlend_southBlend_eastBlend.xyz);
#endif

	// v2f:color
	o.color = v.color;

	// 世界空间顶点位置
	float3 posWorld = mul(unity_ObjectToWorld, offset_vertex).xyz;
#if CALC_DEPTH
	//将顶点深度写入color.a 
	GSTORE_TRANSFER_DEPTH(o.color.a, offset_vertex);
#endif
	// 法线
	fixed3 normal = CUBE_FACE_NORMALS[normal_tilesuv_northBlend.x];
	// 世界空间法线
	fixed3 normalWorld = UnityObjectToWorldNormal(normal);

#if USING_BLOCK_BUMP
	// 切线
	fixed4 tangent = CUBE_FACE_TANGENTS[normal_tilesuv_northBlend.x];
	// 世界空间Tangent
	fixed3 worldTangent = UnityObjectToWorldDir(tangent.xyz);

	// 世界空间Binormal
	fixed tangentSign = tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(normalWorld, worldTangent) * tangentSign;

	// Transfer to frag
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, normalWorld.x, posWorld.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, normalWorld.y, posWorld.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, normalWorld.z, posWorld.z);
#else
	// 没法线时，也要传法线和世界坐标
	o.normalWorld = normalWorld;
	o.posWorld = posWorld;
#endif

	// vertex程序中的光照颜色
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	o.sh = 0;
	// Approximated illumination from non-important point lights
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

	TRANSFER_SHADOW(o); 			// pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);	// pass fog coordinates to pixel shader
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);

	// 贴图颜色
#if USEING_TEXTURE_ARRAY
	fixed4 tex_diffuse = UNITY_SAMPLE_TEX2DARRAY(_MainTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.fogCoord.y));
#else
	fixed4 tex_diffuse = tex2D(_MainTexAtlas, i.main_bump_uv.xy);
#endif

	// -------------- 法线贴图 ------------------
//#if USING_BLOCK_NORMAL_TEX
//#if USEING_TEXTURE_ARRAY
//	fixed4 texture_normal = UNITY_SAMPLE_TEX2DARRAY(_NormalTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.fogCoord.y));
//	texture_normal.xyz = 2 * texture_normal.xyz - 1;
//
//	fixed3 texture_world_normal;
//	texture_world_normal.x = dot(i.tSpace0.xyz, texture_normal.xyz);
//	texture_world_normal.y = dot(i.tSpace1.xyz, texture_normal.xyz);
//	texture_world_normal.z = dot(i.tSpace2.xyz, texture_normal.xyz);
//#else
//	fixed4 texture_world_normal = fixed4(0, 0, 0, 1);
//#endif
//#else
//	fixed4 texture_world_normal = fixed4(0, 0, 0, 1);
//#endif

	// -------------- 方块倒角 ------------------
	// 法线贴图(正切空间)
	#if USING_BLOCK_BUMP
		// 世界法线
		//fixed3 bump_noarmal = UnpackScaleNormal(tex2D(_BumpMap, i.main_bump_uv.zw), _BumpScale);
		fixed3 bump_noarmal = UnpackNormal(tex2D(_BumpMap, i.main_bump_uv.zw));
		fixed3 normalWorld;
		normalWorld.x = dot(i.tSpace0.xyz, bump_noarmal);
		normalWorld.y = dot(i.tSpace1.xyz, bump_noarmal);
		normalWorld.z = dot(i.tSpace2.xyz, bump_noarmal);

		// 把世界坐标组合起来
		float3 posWorld = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);

		// 吸收环境光强度
		fixed bump_intensity = 1 - max(0.0, (dot(fixed3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z), normalWorld)));
		//bump_intensity *= _BumpIntensity;
		bump_intensity *= 2.0;

		//
		//normalWorld += texture_world_normal;
	#else
		fixed3 normalWorld = i.normalWorld;	// 额外的像素光不计算法线贴图
		float3 posWorld = i.posWorld;
		fixed bump_intensity = 0;
	#endif

		// 注意：调试使用
//#if defined(UNITY_PASS_FORWARDBASE)
//		// 法线1
//		return fixed4(normalize(normalWorld) * 0.5 + 0.5, 1.0);
//		// 法线2
//		//return fixed4(normalize(cross(ddy(posWorld), ddx(posWorld))) * 0.5 + 0.5, 1.0);
//		// uv
//		//return fixed4(frac(i.main_bump_uv.x), frac(i.main_bump_uv.y), 0.0, 1.0);
//		// 世界位置
//		//return fixed4(frac(posWorld.x), frac(posWorld.y), frac(posWorld.z), 1.0);
//		// 局部位置
//		//return fixed4(frac(i.pos.x), frac(i.pos.y), frac(i.pos.z), 1.0);
//#else
//		return 0;
//#endif


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
	// 各种曲线加强法线影响
	//lambert_term *= lambert_term;
	//lambert_term = pow((lambert_term - 1), 3) + 1;
	//lambert_term = 2 * lambert_term - (lambert_term * lambert_term);
	//lambert_term = 1 - pow((lambert_term - 1), 4);

	// 公式计算
	// 公式意义：使用直射光定义亮部颜色
	half3 direct_diffuse = _LightColor0.rgb * lambert_term;
	half3 indirect_diffuse = half3(0, 0, 0);
#if USING_UNITY_SH
	// Should SH (light probe / ambient) calculations be performed?
	// additive, shadowcaster etc. 都不会有SH，Unity内部已定义好
	// 环境光随漫反射光改变权重, 把SH光照也加入到间接漫反射
	// 公式意义: 使用环境光定义为暗部颜色
	indirect_diffuse += i.sh * (1 - (1 - bump_intensity) * lambert_term);
	//indirect_diffuse += i.sh * (1 - lambert_term);
	//indirect_diffuse += i.sh * (1 - atten);
#endif

	// 计算方块光照
	half3 diffuse_part = (direct_diffuse + indirect_diffuse);
#if USING_MC_LIGHT
	// 只有在开启MC_LIGHT, 并且是第一个光照pass的时候, 才计算方块光照(额外像素光pass不计算)
	diffuse_part = CombineMCLighting_Cube(diffuse_part, atten, i.color.g, i.color.b, _SkyLightTex, _BlockLightTex);
#else
	diffuse_part *= atten;
#endif
	
	// 主贴图混合
	#if USING_BLOCK_BLEND
		// 混合贴图：r:北 g:西 b:南 a:东
		fixed2 blend_uv = frac(i.main_bump_uv.xy);
		// 缩小一纹素
		blend_uv = lerp(_BlockBlendMaskTex_TexelSize * 2, 1 - _BlockBlendMaskTex_TexelSize * 2, blend_uv);
		fixed4 blend_factor = tex2D(_BlockBlendMaskTex, blend_uv);
		fixed3 blend_color;

		// 北
		UNITY_BRANCH
		if (blend_factor.r * (abs(i.blend_tex_indices.x - i.fogCoord.y) - 0.5) > 0)
		{
#if USEING_TEXTURE_ARRAY
			blend_color = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.blend_tex_indices.x), 0);
#else
			blend_color = tex_diffuse;
#endif
			tex_diffuse.rgb = lerp(tex_diffuse.rgb, blend_color, blend_factor.r);
		}

		// 西
		UNITY_BRANCH
		if (blend_factor.g * (abs(i.blend_tex_indices.y - i.fogCoord.y) - 0.5) > 0)
		{
#if USEING_TEXTURE_ARRAY
			blend_color = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.blend_tex_indices.y), 0);
#else
			blend_color = tex_diffuse;
#endif
			tex_diffuse.rgb = lerp(tex_diffuse.rgb, blend_color, blend_factor.g);
		}

		// 南
		UNITY_BRANCH
		if (blend_factor.b * (abs(i.blend_tex_indices.z - i.fogCoord.y) - 0.5) > 0)
		{
#if USEING_TEXTURE_ARRAY
			blend_color = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.blend_tex_indices.z), 0);
#else
			blend_color = tex_diffuse;
#endif
			tex_diffuse.rgb = lerp(tex_diffuse.rgb, blend_color, blend_factor.b);
		}

		// 东
		UNITY_BRANCH
		if (blend_factor.a * (abs(i.blend_tex_indices.w - i.fogCoord.y) - 0.5) > 0)
		{
#if USEING_TEXTURE_ARRAY
			blend_color = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.blend_tex_indices.w), 0);
#else
			blend_color = tex_diffuse;
#endif
			tex_diffuse.rgb = lerp(tex_diffuse.rgb, blend_color, blend_factor.a);
		}
	#endif

	// AO混合
	// AO等级 1=没有AO (额外的像素光不计算AO)
	fixed ao_level = 1;
	#if USING_BLOCK_AO
		ao_level = saturate(ao_smoothstep(i.color.r, _AO_Size));
		//ao_level = lerp(0, 1, ((1 - ao_level) * -_AO_Blend + 1));
		tex_diffuse.rgb = lerp(tex_diffuse.rgb * _AO_Color.rgb, tex_diffuse.rgb, ((1 - ao_level) * -_AO_Blend + 1));
	#endif

	// (直接漫反射 + 间接漫反射 + 补偿) * 漫反射颜色
	half3 diffuse = diffuse_part * tex_diffuse * ao_level;

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
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.color.a, _ZWrite, final_alpha);
#endif
	//return fixed4(1, 1, 1, 1);
	// 最后的颜色
	fixed4 final_color = fixed4(diffuse, final_alpha);

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);


	// discard 一定要放到最后，否则造成iphone上显示不正确
	// 半透明实现
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);
	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);
	return final_color;
}

#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

// ========================= shadow caster 主体 =========================

#if defined(UNITY_PASS_SHADOWCASTER)

struct appdata_shadowcaster
{
	float4 vertex : POSITION;
	float3 packData : TEXCOORD0;		// 依次包含了9个int(0 ~ 256)数据:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	float3 normal : NORMAL;
};

struct v2f_shadowcaster
{
	V2F_SHADOW_CASTER;
	// 一定要从TEXCOORD1开始,V2F_SHADOW_CASTER有可能占用TEXCOORD0
#if GSTORE_USING_ALPHA_CUTOFF
	float2 uv : TEXCOORD1;		// for alpha test
	float index : TEXCOORD2;
#endif

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f_shadowcaster vert(appdata_shadowcaster v)
{
	v2f_shadowcaster o;
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

#if defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || GSTORE_USING_ALPHA_CUTOFF
	fixed3 normal_tilesuv_northBlend = UnpackValues(v.packData.y);
#endif
	

#if defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY)
	// 法线，在Unity内置实现TRANSFER_SHADOW_CASTER_NORMALOFFSET时需要法线
	fixed3 normal = CUBE_FACE_NORMALS[normal_tilesuv_northBlend.x];
	v.normal = normal;
#endif

#if GSTORE_USING_ALPHA_CUTOFF
	half3 tex_uv_bumpUV = UnpackValues(v.packData.x);
	// 主贴图uv
	float2 uv = CUBE_QUAD_UVS[tex_uv_bumpUV.y];

#if USEING_TEXTURE_ARRAY
	// 平铺，现在不用了，因为没有合并mesh
	//half2 tilesUV = UnpackTilesUV(normal_tilesuv_northBlend.y);
	//uv.x *= tilesUV.x;
	//uv.y *= tilesUV.y;
	// v2f:主贴图uv
	o.uv = uv.xy;
#else
	// 1 / 16 = 0.0625
	// 用的是图集
	o.uv.x = (tex_uv_bumpUV.x % 16) * 0.0625 + (uv.x * 0.0625);
	o.uv.y = (1 - ((floor(tex_uv_bumpUV.x / 16) + 1) * 0.0625)) + (uv.y * 0.0625);

#endif

	o.index = tex_uv_bumpUV.x;
#endif


	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	return o;
}

float4 frag(v2f_shadowcaster i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);

#if GSTORE_USING_ALPHA_CUTOFF
#if USEING_TEXTURE_ARRAY
	fixed4 tex_diffuse = UNITY_SAMPLE_TEX2DARRAY(_MainTex, fixed3(i.uv.x, i.uv.y, i.index));
#else
	fixed4 tex_diffuse = tex2D(_MainTexAtlas, i.uv.xy);
#endif
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);
#endif

	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);

	SHADOW_CASTER_FRAGMENT(i)
}

#endif // defined(UNITY_PASS_SHADOWCASTER)

#endif // __Cube__
