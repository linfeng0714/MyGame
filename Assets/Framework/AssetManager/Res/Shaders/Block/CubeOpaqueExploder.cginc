#ifndef __CubeOpaqueExploder__
#define __CubeOpaqueExploder__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "MCLighting.cginc"
#include "../Lib/FastDepthMap.cginc"
#include "../Lib/IlluminationFormula.cginc"

// ========================= ����Shaderʹ�õĿ��ع��ܺ� =========================
// Shader��ʹ��Define����:
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

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;     // xy:_MainTex z:tex_index
};

struct v2f
{
	float4 pos : SV_POSITION;
	fixed4 pack_uv : TEXCOORD0;			// TextureArray( xy:_MainTex z:tex_index)     Atlas(xy:ͼ��uv)
	fixed3 worldNormal : TEXCOORD1;
#if CALC_DEPTH
	float4 worldPos : TEXCOORD2;					// worldPos��д�����ʱworldPos.w������¼���ֵ
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
// (texture)����ͼ
UNITY_DECLARE_TEX2DARRAY(_MainTex);
#else
// (texture)����ͼ
uniform sampler2D _MainTexAtlas;
#endif


// ��ɫ
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
	// �õ���ͼ��
	o.pack_uv.x = (v.texcoord.z % 16) * 0.0625 + (v.texcoord.x * 0.0625);
	o.pack_uv.y = (1 - ((floor(v.texcoord.z / 16) + 1) * 0.0625)) + (v.texcoord.y * 0.0625);
#endif

	// ����ռ䶥��λ��
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	// ����ռ䷨��
	fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);

	o.worldPos.xyz = worldPos;

#if CALC_DEPTH
	//���������д��posWorld.w
	GSTORE_TRANSFER_DEPTH(o.worldPos.w, v.vertex);
#endif

	o.worldNormal = worldNormal;

	// vertex�����еĹ�����ɫ
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

	// ����˥�� compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, i, posWorld)
	// �ƹⷽ��
	fixed3 lightDirection = UnityWorldSpaceLightDir(posWorld);
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
#if UNITY_SHOULD_SAMPLE_SH
	// ��ʽ����: ʹ�û����ⶨ��Ϊ������ɫ
	indirect_diffuse += i.sh * (1 - lambert_term);
#endif

	half3 diffuse_part = (direct_diffuse + indirect_diffuse) * atten;

	// ��ͼ��ɫ+ ������ɫ
#if USEING_TEXTURE_ARRAY
	fixed4 tex_color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.pack_uv.x, i.pack_uv.y, i.pack_uv.z)) * _Color;
#else
	fixed4 tex_color = tex2D(_MainTexAtlas, i.pack_uv.xy) * _Color;
#endif

	// ������ɫ
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
