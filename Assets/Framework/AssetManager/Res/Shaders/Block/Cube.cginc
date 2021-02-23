#ifndef __Cube__
#define __Cube__

#include "UnityCG.cginc"
#include "HLSLSupport.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= ����Shaderʹ�õĿ��ع��ܺ� =========================
// Shader��ʹ��Define����:
// DISABLE_BLOCK_BUMP - ����ʹ�÷���ߵķ��ߣ�ʹ�ú�ɴﵽ�Ż�Ŀ��
// DISABLE_BLOCK_BLEND - ����ʹ�÷����ϣ�ʹ�ú�ɴﵽ�Ż�Ŀ��
// DISABLE_BLOCK_AO - ����ʹ��AO��ʹ�ú�ɴﵽ�Ż�Ŀ��
// DISABLE_MC_LIGHT - ����ʹ��MC���գ�ʹ�ú�ɴﵽ�Ż�Ŀ�ģ���������ʹ��
// USE_FORCE_TEX_ATLAS - ǿ��ʹ��ͼ����ʽ��ʹ�ú�ɴﵽ�Ż�Ŀ��


// ========================= ���ض��� =========================
// ��Ŀ�ķ�����:
// 1.Ч������
// 2.�Ż����أ��Ż����ؾ�������һ��Ŀ����������ﵽ�Ż�Ŀ�ġ�
// �����÷�ʽ������:
// 1.��̬���ã���Shader��ֱ��ʹ��#define������(���ֲ����������)
// 2.��̬���ã���Shader��ʹ��multi_compile��shader_feature������(���ֻ��������)
// ΪʲôҪ��USE_XXX��USING_XXX�أ���Ϊǰ���Ǹ��û��趨�ģ�����Ϊ����ʱʹ�õģ����ߵ������п��ܲ�һ����

// �Ƿ���ʹ��TextureArray
#define USEING_TEXTURE_ARRAY (SHADER_TARGET >= 35) && !USE_FORCE_TEX_ATLAS

// dither ͸��
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// MC����
#define USING_MC_LIGHT !defined(DISABLE_MC_LIGHT) && defined(UNITY_PASS_FORWARDBASE)

// �Ƿ�ʹ��unity�� sh ����
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// �Ƿ�ʹ��Alpha cutoff
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// ��ͨ��ͼ͸��
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// ��ͨ͸��
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// ���Ƿ���
#define USING_BLOCK_BUMP (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_BUMP))

//����
#define USING_BLOCK_NORMAL_TEX (defined(UNITY_PASS_FORWARDBASE) && defined(ENABLE_BLOCK_NORMAL_TEX) && defined(USEING_TEXTURE_ARRAY))

// ���
#define USING_BLOCK_BLEND (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_BLEND) && defined(USEING_TEXTURE_ARRAY))

// AO
#define USING_BLOCK_AO (defined(UNITY_PASS_FORWARDBASE) && !defined(DISABLE_BLOCK_AO))

// ========================= ���� - ��ʼ =========================

#if USEING_TEXTURE_ARRAY
// (texture)����ͼ
UNITY_DECLARE_TEX2DARRAY(_MainTex);				// ����ͼ��
// (texture)�����ͼ
UNITY_DECLARE_TEX2DARRAY(_BlendTex);			// �����ͼ��
// (texture)������ͼ
UNITY_DECLARE_TEX2DARRAY(_NormalTex);			// ������ͼ��
#else
// (texture)����ͼ
uniform sampler2D _MainTexAtlas;
#endif

uniform float _TJunctionOffset;

uniform fixed _ZWrite;


#if USING_MC_LIGHT
uniform sampler2D _SkyLightTex;					// ȫ�������ɫͼ
uniform sampler2D _BlockLightTex;				// ȫ�ֻ�ѹ���ɫͼ
#endif

#if USING_BLOCK_BUMP
uniform sampler2D _BumpMap;								// ���Ƿ�����ͼ
//uniform fixed _BumpScale;								// �������ŷ���ֵ
//uniform fixed _BumpIntensity;							// ���Ǳ����ջ�����ǿ��
#endif

#if USING_BLOCK_BLEND
uniform sampler2D _BlockBlendMaskTex;					// ���mask
float4 _BlockBlendMaskTex_TexelSize;
#endif

// AO Effect
#if USING_BLOCK_AO
uniform fixed _AO_Blend;
uniform fixed4 _AO_Color;
uniform fixed _AO_Size;
#endif


// ��ͨ͸��
#if USING_TRANSPARENCY
uniform fixed _Transparency;
#endif

// ========================= �������� - ��ʼ =========================

// ��ǰ�ƹⷽ��
inline fixed3 LightDirection(float3 posWorld)
{
#ifndef USING_DIRECTIONAL_LIGHT
	return normalize(UnityWorldSpaceLightDir(posWorld));
#else
	return _WorldSpaceLightPos0.xyz;
#endif
}



// ========================= ���鳣������ - ��ʼ =========================


//uniform fixed3 CUBE_FACE_NORMALS[6];
//uniform fixed2 CUBE_QUAD_UVS[4];

// ���T-junction�������������(GreedyMesh�ϲ�quad�����), ������΢�����ص�һ��
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

// 6����ķ��߳���, ��������ֻ���뷨������
static const fixed3 CUBE_FACE_NORMALS[8] = {
	VECTOR3_DOWN,			// down
	VECTOR3_UP,			// up
	VECTOR3_FORWARD,			// north
	VECTOR3_BACK,			// south
	VECTOR3_LEFT,			// west
	VECTOR3_RIGHT, 			// east

	// ��Ϊshader�ı������Ż���������ݴ��ң�����һ���������ݣ���ֹ�Ż�
	// hack for shader complier optimizing
	fixed3(1, 0.1, 0), 			// east
	fixed3(1, 0.1, 0), 			// east

	//// Down y-
	//// ��ˮƽ��ֱ  xΪˮƽ��zΪ��ֱ
	//// ����0
	//// -x
	//normalize(VECTOR3_DOWN + VECTOR3_LEFT),
	//// -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK),
	//// -x & -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK + VECTOR3_LEFT),
	//// ����1
	//// +x
	//normalize(VECTOR3_DOWN + VECTOR3_RIGHT),
	//// -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK),
	//// +x & -z
	//normalize(VECTOR3_DOWN + VECTOR3_BACK + VECTOR3_RIGHT),
	//// ����2
	//// +x
	//normalize(VECTOR3_DOWN + VECTOR3_RIGHT),
	//// +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD),
	//// +x & +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// ����3
	//// -x
	//normalize(VECTOR3_DOWN + VECTOR3_LEFT),
	//// +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD),
	//// -x & +z
	//normalize(VECTOR3_DOWN + VECTOR3_FORWARD + VECTOR3_LEFT),

	//// Up y+
	//// ��ˮƽ��ֱ  xΪˮƽ��zΪ��ֱ
	//// ����0
	//// -x
	//normalize(VECTOR3_UP + VECTOR3_LEFT),
	//// +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD),
	//// -x & +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD + VECTOR3_LEFT),
	//// ����1
	//// +x
	//normalize(VECTOR3_UP + VECTOR3_RIGHT),
	//// +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD),
	//// +x & +z
	//normalize(VECTOR3_UP + VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// ����2
	//// +x
	//normalize(VECTOR3_UP + VECTOR3_RIGHT),
	//// -z
	//normalize(VECTOR3_UP + VECTOR3_BACK),
	//// +x & -z
	//normalize(VECTOR3_UP + VECTOR3_BACK + VECTOR3_RIGHT),
	//// ����3
	//// -x
	//normalize(VECTOR3_UP + VECTOR3_LEFT),
	//// -z
	//normalize(VECTOR3_UP + VECTOR3_BACK),
	//// -x & -z
	//normalize(VECTOR3_UP + VECTOR3_BACK + VECTOR3_LEFT),

	//// North z+
	//// ��ˮƽ��ֱ  xΪˮƽ��yΪ��ֱ
	//// ����0
	//// +x
	//normalize(VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP + VECTOR3_RIGHT),
	//// ����1
	//// -x
	//normalize(VECTOR3_FORWARD + VECTOR3_LEFT),
	//// +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP),
	//// -x & +y
	//normalize(VECTOR3_FORWARD + VECTOR3_UP + VECTOR3_LEFT),
	//// ����2
	//// -x
	//normalize(VECTOR3_FORWARD + VECTOR3_LEFT),
	//// -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN),
	//// -x & -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN + VECTOR3_LEFT),
	//// ����3
	//// +x
	//normalize(VECTOR3_FORWARD + VECTOR3_RIGHT),
	//// -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN),
	//// +x & -y
	//normalize(VECTOR3_FORWARD + VECTOR3_DOWN + VECTOR3_RIGHT),

	//// South z-
	//// ��ˮƽ��ֱ  xΪˮƽ��yΪ��ֱ
	//// ����0
	//// -x
	//normalize(VECTOR3_BACK + VECTOR3_LEFT),
	//// +y
	//normalize(VECTOR3_BACK + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_BACK + VECTOR3_UP + VECTOR3_LEFT),
	//// ����1
	//// +x
	//normalize(VECTOR3_BACK + VECTOR3_RIGHT),
	//// +y
	//normalize(VECTOR3_BACK + VECTOR3_UP),
	//// +x & +y
	//normalize(VECTOR3_BACK + VECTOR3_UP + VECTOR3_RIGHT),
	//// ����2
	//// +x
	//normalize(VECTOR3_BACK + VECTOR3_RIGHT),
	//// -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN),
	//// +x & -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN + VECTOR3_RIGHT),
	//// ����3
	//// -x
	//normalize(VECTOR3_BACK + VECTOR3_LEFT),
	//// -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN),
	//// -x & -y
	//normalize(VECTOR3_BACK + VECTOR3_DOWN + VECTOR3_LEFT),

	//// West x-
	//// ��ˮƽ��ֱ zΪˮƽ��yΪ��ֱ
	//// ����0
	//// +z
	//normalize(VECTOR3_LEFT + VECTOR3_FORWARD),
	//// +y
	//normalize(VECTOR3_LEFT + VECTOR3_UP),
	//// +y & +z
	//normalize(VECTOR3_LEFT + VECTOR3_UP + VECTOR3_FORWARD),
	//// ����1
	//// -z
	//normalize(VECTOR3_LEFT + VECTOR3_BACK),
	//// +y
	//normalize(VECTOR3_LEFT + VECTOR3_UP),
	//// +y & -z
	//normalize(VECTOR3_LEFT + VECTOR3_UP + VECTOR3_BACK),
	//// ����2
	//// -z
	//normalize(VECTOR3_LEFT + VECTOR3_BACK),
	//// -y
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN),
	//// -y & -z
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN + VECTOR3_BACK),
	//// ����3
	//// +z
	//normalize(VECTOR3_LEFT + VECTOR3_FORWARD),
	//// -y
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN),
	//// -y & +z
	//normalize(VECTOR3_LEFT + VECTOR3_DOWN + VECTOR3_FORWARD),

	//// East x+
	//// ��ˮƽ��ֱ zΪˮƽ��yΪ��ֱ
	//// ����0
	//// -z
	//normalize(VECTOR3_RIGHT + VECTOR3_BACK),
	//// +y
	//normalize(VECTOR3_RIGHT + VECTOR3_UP),
	//// +y & -z
	//normalize(VECTOR3_RIGHT + VECTOR3_UP + VECTOR3_BACK),
	//// ����1
	//// +z
	//normalize(VECTOR3_RIGHT + VECTOR3_FORWARD),
	//// +y
	//normalize(VECTOR3_RIGHT + VECTOR3_UP),
	//// +y & +z
	//normalize(VECTOR3_RIGHT + VECTOR3_UP + VECTOR3_FORWARD),
	//// ����2
	//// +z
	//normalize(VECTOR3_RIGHT + VECTOR3_FORWARD),
	//// -y
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN),
	//// -y & +z
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN + VECTOR3_FORWARD),
	//// ����3
	//// -z
	//normalize(VECTOR3_RIGHT + VECTOR3_BACK),
	//// -y
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN),
	//// -y & -z
	//normalize(VECTOR3_RIGHT + VECTOR3_DOWN + VECTOR3_BACK),

	//// ��Ϊshader�ı������Ż���������ݴ��ң�����һ���������ݣ���ֹ�Ż�
	//// �����������ʹ��Metal��Mac��
	//// hack for shader complier optimizing
	//fixed3(1.0, 0.1, 0.0),
	//fixed3(1.0, 0.1, 0.0),
};

// 4������ı�׼uv, ��������ֻ����uv����
static const fixed2 CUBE_QUAD_UVS[8] = {
	fixed2(0.0, 1.0),
	fixed2(1.0, 1.0),
	fixed2(1.0, 0.0),
	fixed2(0.0, 0.0),

	// ��Ϊshader�ı������Ż���������ݴ��ң�����һ���������ݣ���ֹ�Ż�
	// hack for shader complier optimizing
	fixed2(0.1, 0.2),
	fixed2(0.3, 0.4),
	fixed2(0.5, 0.6),
	fixed2(0.7, 0.8),
};

#if USING_BLOCK_BUMP

//uniform fixed4 CUBE_FACE_TANGENTS[6];

//// 6��������߳���, ��������ֻ������������
static const fixed4 CUBE_FACE_TANGENTS[8] = {
	fixed4(1.0, 0.0, 0.0, -1.0),			// down y-
	fixed4(1.0, 0.0, 0.0, -1.0),			// up y+
	fixed4(-1.0, 0.0, 0.0, -1.0),			// north z+
	fixed4(1.0, 0.0, 0.0, -1.0),			// south z-
	fixed4(0.0, 0.0, -1.0, -1.0),			// west x-
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east x+

	// ��Ϊshader�ı������Ż���������ݴ��ң�����һ���������ݣ���ֹ�Ż�
	// hack for shader complier optimizing
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east
	fixed4(0.0, 0.0, 1.0, -1.0), 			// east
	
	//// Down y-
	//// ����0
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),
	//// ����1
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// ����2
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),
	//// ����3
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),

	//// Up y+
	//// ����0
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// ����1
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),
	//// ����2
	//fixed4(0.7071067,-0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),
	//// ����3
	//fixed4(0.7071067,0.7071069,0,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),

	//// North z+
	//// ����0
	//fixed4(-0.7071067,0,0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,0.4082484,0.4082484,-1),
	//// ����1
	//fixed4(-0.7071067,0,-0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,-0.4082484,-0.4082484,-1),
	//// ����2
	//fixed4(-0.7071067,0,-0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,0.4082484,-0.4082484,-1),
	//// ����3
	//fixed4(-0.7071067,0,0.7071069,-1),
	//fixed4(-1,0,0,-1),
	//fixed4(-0.8164965,-0.4082484,0.4082484,-1),

	//// South z-
	//// ����0
	//fixed4(0.7071067,0,-0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,-0.4082484,-1),
	//// ����1
	//fixed4(0.7071067,0,0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,0.4082484,-1),
	//// ����2
	//fixed4(0.7071067,0,0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,0.4082484,0.4082484,-1),
	//// ����3
	//fixed4(0.7071067,0,-0.7071069,-1),
	//fixed4(1,0,0,-1),
	//fixed4(0.8164965,-0.4082484,-0.4082484,-1),

	//// West x-
	//// ����0
	//fixed4(-0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(-0.4082484,0.4082484,-0.8164965,-1),
	//// ����1
	//fixed4(0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(0.4082484,-0.4082484,-0.8164965,-1),
	//// ����2
	//fixed4(0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(0.4082484,0.4082484,-0.8164965,-1),
	//// ����3
	//fixed4(-0.7071069,0,-0.7071067,-1),
	//fixed4(0,0,-1,-1),
	//fixed4(-0.4082484,-0.4082484,-0.8164965,-1),

	//// East x+
	//// ����0
	//fixed4(0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(0.4082484,0.4082484,0.8164965,-1),
	//// ����1
	//fixed4(-0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(-0.4082484,-0.4082484,0.8164965,-1),
	//// ����2
	//fixed4(-0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(-0.4082484,0.4082484,0.8164965,-1),
	//// ����3
	//fixed4(0.7071069,0,0.7071067,-1),
	//fixed4(0, 0, 1, -1),
	//fixed4(0.4082484,-0.4082484,0.8164965,-1),

	//// ��Ϊshader�ı������Ż���������ݴ��ң�����һ���������ݣ���ֹ�Ż�
	//// �����������ʹ��Metal��Mac��
	//// hack for shader complier optimizing
	//fixed4(0.1, 0.1, 1.0, -1.0),
	//fixed4(0.1, 0.1, 1.0, -1.0),
};

// 16�����Ƿ�����ͼ(��һ��)��uv, ��������ֻ���뵹�Ƿ�����ͼ������, ����ֵ��ΧΪ0~63, ����16~63���Զ�����uv.y��ƫ��
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

// ��һ��float�н�����3��ֵ��ΧΪ0~255��int
// ref:https://www.gamedev.net/forums/topic/556757-unpack-4-bytes-from-an-int-in-glsl/
half3 UnpackValues(float packedVal)
{
	float3 unpackedVals = float3(1.0, 256.0, 65536.0);
	unpackedVals = frac(unpackedVals * packedVal);

	return floor(unpackedVals * 256.0);
}

// ��1��8λ��int��, ������tilesUV������ֵ, ��4λΪtilesU, ��4λΪtilesV
inline half2 UnpackTilesUV(float packTilesUV)
{
	return half2(floor(packTilesUV / 16) + 1, fmod(packTilesUV, 16) + 1);
}



// ========================= Forward���� =========================

#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)



struct appdata
{
	float4 vertex : POSITION;
	float3 packData : TEXCOORD0;		// ���ΰ�����9��int(0 ~ 256)����:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	float4 color : COLOR;				// (color, r:aoֵ, g:skylightֵ, b:blocklightֵ)
};

struct v2f
{
	UNITY_POSITION(pos);
	fixed4 color : COLOR;						// (color, r:aoֵ, g:skylightֵ, b:blocklightֵ)
	float4 main_bump_uv : TEXCOORD0;			// (xy:mainTexUV, zw:bumpTexUV)
#if USING_BLOCK_BLEND
	half4 blend_tex_indices : TEXCOORD1;		// �����ͼ��TextureArray�е�����(xyzw��Ӧ�����϶� ֵ:0-255)
#endif
#if USING_BLOCK_BUMP
	float4 tSpace0 : TEXCOORD2;					// ���߿ռ�(x:Tangent.x, y:Binormal.x, z:Normal.x, w:posWorld.x), �����������ʹ��float
	float4 tSpace1 : TEXCOORD3;					// ���߿ռ�(x:Tangent.y, y:Binormal.y, z:Normal.y, w:posWorld.y), �����������ʹ��float
	float4 tSpace2 : TEXCOORD4;					// ���߿ռ�(x:Tangent.z, y:Binormal.z, z:Normal.z, w:posWorld.z), �����������ʹ��float
#else
	fixed3 normalWorld : TEXCOORD2;				// û����ʱ��ҲҪ������
	float3 posWorld : TEXCOORD3;				// û����ʱ��ҲҪ������λ��
#endif

	SHADOW_COORDS(5)
	UNITY_FOG_COORDS_PACKED(6, float2)			// x: fogCoord.x, y: ����ͼ����      fogֻ��һ��half���������ã�y��������ͼ��TextureArray����(0-255)

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

	// ��packData.xyz�н�����9��int����:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	half3 tex_uv_bumpUV = UnpackValues(v.packData.x);
	half3 normal_tilesuv_northBlend = UnpackValues(v.packData.y);
	half3 westBlend_southBlend_eastBlend = UnpackValues(v.packData.z);

	float4 offset_vertex = v.vertex;
	//offset_vertex.xyz += TJUNCTION_OFFSET[normal_tilesuv_northBlend.x * 4 + tex_uv_bumpUV.y] * _TJunctionOffset;
	o.pos = UnityObjectToClipPos(offset_vertex);
	
	
#if USING_BLOCK_BUMP
	// ����bump uv
	int mod_bump_uv_index = tex_uv_bumpUV.z % 16;
	int bump_uv_row_index = floor(tex_uv_bumpUV.z / 16);

	fixed2 bump_uv = BUMP_UVS[mod_bump_uv_index];
	bump_uv.y -= bump_uv_row_index * 0.25;

	o.main_bump_uv.zw = bump_uv.xy;
#endif

	// v2f:����ͼ����
	o.fogCoord.y = tex_uv_bumpUV.x;

	// ����ͼuv
	float2 uv = CUBE_QUAD_UVS[tex_uv_bumpUV.y];
#if USEING_TEXTURE_ARRAY
	// ƽ�̣����ڲ����ˣ���Ϊû�кϲ�mesh
	//half2 tilesUV = UnpackTilesUV(normal_tilesuv_northBlend.y);
	//uv.x *= tilesUV.x;
	//uv.y *= tilesUV.y;
	// �����ǵ�����ͼuv
	o.main_bump_uv.xy = uv.xy;
#else
	// 1 / 16 = 0.0625
	// �õ���ͼ��
	o.main_bump_uv.x = (tex_uv_bumpUV.x % 16) * 0.0625 + (uv.x * 0.0625);
	o.main_bump_uv.y = (1 - ((floor(tex_uv_bumpUV.x / 16) + 1) * 0.0625)) + (uv.y * 0.0625);
	
#endif

#if USING_BLOCK_BLEND
	// v2f:�ı߻����ͼ����
	o.blend_tex_indices = half4(normal_tilesuv_northBlend.z, westBlend_southBlend_eastBlend.xyz);
#endif

	// v2f:color
	o.color = v.color;

	// ����ռ䶥��λ��
	float3 posWorld = mul(unity_ObjectToWorld, offset_vertex).xyz;
#if CALC_DEPTH
	//���������д��color.a 
	GSTORE_TRANSFER_DEPTH(o.color.a, offset_vertex);
#endif
	// ����
	fixed3 normal = CUBE_FACE_NORMALS[normal_tilesuv_northBlend.x];
	// ����ռ䷨��
	fixed3 normalWorld = UnityObjectToWorldNormal(normal);

#if USING_BLOCK_BUMP
	// ����
	fixed4 tangent = CUBE_FACE_TANGENTS[normal_tilesuv_northBlend.x];
	// ����ռ�Tangent
	fixed3 worldTangent = UnityObjectToWorldDir(tangent.xyz);

	// ����ռ�Binormal
	fixed tangentSign = tangent.w * unity_WorldTransformParams.w;
	fixed3 worldBinormal = cross(normalWorld, worldTangent) * tangentSign;

	// Transfer to frag
	o.tSpace0 = float4(worldTangent.x, worldBinormal.x, normalWorld.x, posWorld.x);
	o.tSpace1 = float4(worldTangent.y, worldBinormal.y, normalWorld.y, posWorld.y);
	o.tSpace2 = float4(worldTangent.z, worldBinormal.z, normalWorld.z, posWorld.z);
#else
	// û����ʱ��ҲҪ�����ߺ���������
	o.normalWorld = normalWorld;
	o.posWorld = posWorld;
#endif

	// vertex�����еĹ�����ɫ
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
	// ע�⣺ShadeSH9���ֿɷŵ�frag���Bump��ϣ���Ч�ʿ��ǵĻ������Ƿ���vert���������bumpӰ��
	// ��������˻�����
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

	// ��ͼ��ɫ
#if USEING_TEXTURE_ARRAY
	fixed4 tex_diffuse = UNITY_SAMPLE_TEX2DARRAY(_MainTex, fixed3(i.main_bump_uv.x, i.main_bump_uv.y, i.fogCoord.y));
#else
	fixed4 tex_diffuse = tex2D(_MainTexAtlas, i.main_bump_uv.xy);
#endif

	// -------------- ������ͼ ------------------
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

	// -------------- ���鵹�� ------------------
	// ������ͼ(���пռ�)
	#if USING_BLOCK_BUMP
		// ���編��
		//fixed3 bump_noarmal = UnpackScaleNormal(tex2D(_BumpMap, i.main_bump_uv.zw), _BumpScale);
		fixed3 bump_noarmal = UnpackNormal(tex2D(_BumpMap, i.main_bump_uv.zw));
		fixed3 normalWorld;
		normalWorld.x = dot(i.tSpace0.xyz, bump_noarmal);
		normalWorld.y = dot(i.tSpace1.xyz, bump_noarmal);
		normalWorld.z = dot(i.tSpace2.xyz, bump_noarmal);

		// �����������������
		float3 posWorld = float3(i.tSpace0.w, i.tSpace1.w, i.tSpace2.w);

		// ���ջ�����ǿ��
		fixed bump_intensity = 1 - max(0.0, (dot(fixed3(i.tSpace0.z, i.tSpace1.z, i.tSpace2.z), normalWorld)));
		//bump_intensity *= _BumpIntensity;
		bump_intensity *= 2.0;

		//
		//normalWorld += texture_world_normal;
	#else
		fixed3 normalWorld = i.normalWorld;	// ��������عⲻ���㷨����ͼ
		float3 posWorld = i.posWorld;
		fixed bump_intensity = 0;
	#endif

		// ע�⣺����ʹ��
//#if defined(UNITY_PASS_FORWARDBASE)
//		// ����1
//		return fixed4(normalize(normalWorld) * 0.5 + 0.5, 1.0);
//		// ����2
//		//return fixed4(normalize(cross(ddy(posWorld), ddx(posWorld))) * 0.5 + 0.5, 1.0);
//		// uv
//		//return fixed4(frac(i.main_bump_uv.x), frac(i.main_bump_uv.y), 0.0, 1.0);
//		// ����λ��
//		//return fixed4(frac(posWorld.x), frac(posWorld.y), frac(posWorld.z), 1.0);
//		// �ֲ�λ��
//		//return fixed4(frac(i.pos.x), frac(i.pos.y), frac(i.pos.z), 1.0);
//#else
//		return 0;
//#endif


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
	// �������߼�ǿ����Ӱ��
	//lambert_term *= lambert_term;
	//lambert_term = pow((lambert_term - 1), 3) + 1;
	//lambert_term = 2 * lambert_term - (lambert_term * lambert_term);
	//lambert_term = 1 - pow((lambert_term - 1), 4);

	// ��ʽ����
	// ��ʽ���壺ʹ��ֱ��ⶨ��������ɫ
	half3 direct_diffuse = _LightColor0.rgb * lambert_term;
	half3 indirect_diffuse = half3(0, 0, 0);
#if USING_UNITY_SH
	// Should SH (light probe / ambient) calculations be performed?
	// additive, shadowcaster etc. ��������SH��Unity�ڲ��Ѷ����
	// ���������������ı�Ȩ��, ��SH����Ҳ���뵽���������
	// ��ʽ����: ʹ�û����ⶨ��Ϊ������ɫ
	indirect_diffuse += i.sh * (1 - (1 - bump_intensity) * lambert_term);
	//indirect_diffuse += i.sh * (1 - lambert_term);
	//indirect_diffuse += i.sh * (1 - atten);
#endif

	// ���㷽�����
	half3 diffuse_part = (direct_diffuse + indirect_diffuse);
#if USING_MC_LIGHT
	// ֻ���ڿ���MC_LIGHT, �����ǵ�һ������pass��ʱ��, �ż��㷽�����(�������ع�pass������)
	diffuse_part = CombineMCLighting_Cube(diffuse_part, atten, i.color.g, i.color.b, _SkyLightTex, _BlockLightTex);
#else
	diffuse_part *= atten;
#endif
	
	// ����ͼ���
	#if USING_BLOCK_BLEND
		// �����ͼ��r:�� g:�� b:�� a:��
		fixed2 blend_uv = frac(i.main_bump_uv.xy);
		// ��Сһ����
		blend_uv = lerp(_BlockBlendMaskTex_TexelSize * 2, 1 - _BlockBlendMaskTex_TexelSize * 2, blend_uv);
		fixed4 blend_factor = tex2D(_BlockBlendMaskTex, blend_uv);
		fixed3 blend_color;

		// ��
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

		// ��
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

		// ��
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

		// ��
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

	// AO���
	// AO�ȼ� 1=û��AO (��������عⲻ����AO)
	fixed ao_level = 1;
	#if USING_BLOCK_AO
		ao_level = saturate(ao_smoothstep(i.color.r, _AO_Size));
		//ao_level = lerp(0, 1, ((1 - ao_level) * -_AO_Blend + 1));
		tex_diffuse.rgb = lerp(tex_diffuse.rgb * _AO_Color.rgb, tex_diffuse.rgb, ((1 - ao_level) * -_AO_Blend + 1));
	#endif

	// (ֱ�������� + ��������� + ����) * ��������ɫ
	half3 diffuse = diffuse_part * tex_diffuse * ao_level;

	fixed final_alpha = 1.0;
#if USING_TEX_TRANSPARENCY
	// -------------- ��ͼAlpha���� ------------------
	final_alpha *= tex_diffuse.a;
#endif
#if USING_TRANSPARENCY
	// -------------- Alpha���� ------------------
	final_alpha *= _Transparency;
#endif

#if CALC_DEPTH
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.color.a, _ZWrite, final_alpha);
#endif
	//return fixed4(1, 1, 1, 1);
	// ������ɫ
	fixed4 final_color = fixed4(diffuse, final_alpha);

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, final_color);


	// discard һ��Ҫ�ŵ���󣬷������iphone����ʾ����ȷ
	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);
	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);
	return final_color;
}

#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

// ========================= shadow caster ���� =========================

#if defined(UNITY_PASS_SHADOWCASTER)

struct appdata_shadowcaster
{
	float4 vertex : POSITION;
	float3 packData : TEXCOORD0;		// ���ΰ�����9��int(0 ~ 256)����:texIndex, uvIndex, bumpUVIndex, normalIndex, packTileUV, n/w/e/s blendTexIndex
	float3 normal : NORMAL;
};

struct v2f_shadowcaster
{
	V2F_SHADOW_CASTER;
	// һ��Ҫ��TEXCOORD1��ʼ,V2F_SHADOW_CASTER�п���ռ��TEXCOORD0
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
	// ���ߣ���Unity����ʵ��TRANSFER_SHADOW_CASTER_NORMALOFFSETʱ��Ҫ����
	fixed3 normal = CUBE_FACE_NORMALS[normal_tilesuv_northBlend.x];
	v.normal = normal;
#endif

#if GSTORE_USING_ALPHA_CUTOFF
	half3 tex_uv_bumpUV = UnpackValues(v.packData.x);
	// ����ͼuv
	float2 uv = CUBE_QUAD_UVS[tex_uv_bumpUV.y];

#if USEING_TEXTURE_ARRAY
	// ƽ�̣����ڲ����ˣ���Ϊû�кϲ�mesh
	//half2 tilesUV = UnpackTilesUV(normal_tilesuv_northBlend.y);
	//uv.x *= tilesUV.x;
	//uv.y *= tilesUV.y;
	// v2f:����ͼuv
	o.uv = uv.xy;
#else
	// 1 / 16 = 0.0625
	// �õ���ͼ��
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
