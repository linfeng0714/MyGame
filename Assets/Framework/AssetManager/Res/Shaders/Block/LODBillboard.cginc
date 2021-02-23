#ifndef __LODBillboard__
#define __LODBillboard__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "SpeedTreeBillboardCommon.cginc"
#if CALC_DEPTH 
#include "../Lib/FastDepthMap.cginc"
#endif
// ----------------- ���ض��� -----------------

// dither ͸��
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// -------- shader ���� --------

// SpeedTreeBillboardCommon.cginc�Ѿ������
//sampler2D _MainTex;
//fixed _Cutoff;


// �Ƿ�ʹ��unity�� sh ����
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)


// -------------------- �������� - ��ʼ --------------------

// ��ǰ�ƹⷽ��
inline fixed3 LightDirection(float3 posWorld)
{
#ifndef USING_DIRECTIONAL_LIGHT
	return normalize(UnityWorldSpaceLightDir(posWorld));
#else
	return _WorldSpaceLightPos0.xyz;
#endif
}

// -------------------- �������� - ���� --------------------

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

	Input data : TEXCOORD6; // LOD crossfade ��Ϊ��������������Ҫ�����

	UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(SpeedTreeBillboardData v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	SpeedTreeBillboardVert(v, o.data);
	// ����λ��
	o.pos = UnityObjectToClipPos(v.vertex);
	// uv��SpeedTreeBillboardVert���Ѿ����

	// ����ռ䶥��λ��
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
	// ����ռ䷨��
	fixed3 normalWorld = UnityObjectToWorldNormal(v.normal);
	

#if CALC_DEPTH
	//���������д��o.posWorld.w
	GSTORE_TRANSFER_DEPTH(o.posWorld.w, v.vertex);
#endif
	// ������ߺ�����ռ䶥��λ��
	o.posWorld.xyz = posWorld;
	o.normalWorld = normalWorld;

	// vertex�����еĹ�����ɫ�ͻ�����
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
	// ע�⣺ShadeSH9���ֿɷŵ�frag���Bump��ϣ���Ч�ʿ��ǵĻ������Ƿ���vert���������bumpӰ��
	// ��������˻�����
	o.sh.rgb += max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
#endif

	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);	// �����Ļ�ռ�λ��
	TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);// pass fog coordinates to pixel shader
	return o;
}


fixed4 frag(v2f IN) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(IN);

	// ��ͼ��ɫ
	fixed4 tex_color = tex2D(_MainTex, IN.data.mainTexUV);
	clip(tex_color.a - _Cutoff);
	// ����˥�� compute lighting & shadowing factor
	UNITY_LIGHT_ATTENUATION(atten, IN, IN.posWorld)
	// �ƹⷽ��
	fixed3 lightDirection = LightDirection(IN.posWorld);
	// ���߷���
	fixed3 normalDirection = normalize(IN.normalWorld);


	// ��������ɫ
	float lambert = max(0, dot(normalDirection, lerp( lightDirection, normalDirection, 0.5)));
	//fixed lambert = max(0, 0.5 * dot(normalDirection, -normalDirection) + 0.5);
	//lambert = 0.3 + 0.7 * lambert;	// ��������
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

	// ������ɫ
	fixed4 final_color = fixed4(tex_color * diff_color, final_alpha);
	//return fixed4(IN.sh, 1);
	// apply fog
	UNITY_APPLY_FOG(IN.fogCoord, final_color);

	// discard һ��Ҫ�ŵ���󣬷������iphone����ʾ����ȷ
	// alpha test
	clip(tex_color.a - _Cutoff);
	// ��͸��ʵ��
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
	// һ��Ҫ��TEXCOORD1��ʼ,V2F_SHADOW_CASTER�п���ռ��TEXCOORD0
	Input data : TEXCOORD1; // LOD crossfade ��Ϊ��������������Ҫ�����

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
	// uv��SpeedTreeBillboardVert��������
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	fixed4 texcol = tex2D(_MainTex, i.data.mainTexUV);


	// discard һ��Ҫ�ŵ���󣬷������iphone����ʾ����ȷ
	// Alpha cutoff
	clip(texcol.a - _Cutoff);
	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);
	// LOD
	UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

	SHADOW_CASTER_FRAGMENT(i)
}

#endif // defined(UNITY_PASS_SHADOWCASTER)

#endif // __LODBillboard__
