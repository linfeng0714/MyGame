#ifndef __Character__
#define __Character__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#if CALC_DEPTH
#include "../Lib/FastDepthMap.cginc"
#endif
#include "../Lib/IlluminationFormula.cginc"

// MatCapȫ��MaterailCapture���������ǹ�����Ϣ��ͨ�����ߵ�xy����ȥ����matcap���õ��ڸ÷����ߵĹ�����Ϣ
//matcap�����ʵ�ǣ�������xy������Ϊuv����Ӧ�Ĺ�����ɫ��Ϣ������xyȥ�����ͺã�ע��xy�����ǹ�һ�������еķ�������z��û��Ҫ


// �߹���ͼ�Ĳ�ͬͨ�����Ʋ�ͬЧ��:
// �� : ���Ƹ߹���ɫ��Ӱ�죬���뵽�߹���ɫ��
// �� : ���Ƹ߹�ǿ��
// �� : �Է����ǿ��(diffuse��ͼ�Ĺⰵmask����˼)����(_Color * ����ͼ)

// ----------------- ���ض��� -----------------
// �Ƿ�ʹ��unity�� sh ����
#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)

// ��ͨ͸��
#define USING_TRANSPARENCY (USE_TRANSPARENCY)

// dither ͸��
#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
#include "../Lib/DitherTransparency.cginc"

// cutoff 
#define GSTORE_USING_ALPHA_CUTOFF (USE_ALPHA_CUTOFF)
#include "../Lib/AlphaCutoff.cginc"

// ��ͨ��ͼ͸��
#define USING_TEX_TRANSPARENCY (USE_TEX_TRANSPARENCY)

// �Է��ⲿ��(Add pass��ʹ��)
#define USING_EMISSIVE (!defined(NOT_USE_EMISSIVE) && defined(UNITY_PASS_FORWARDBASE))

// matcap����(Add pass��ʹ��)
#define USING_MATCAP (!defined(NOT_USE_MATCAP)) && (defined(UNITY_PASS_FORWARDBASE))

// sge����
#define USING_SGE_TEX !NOT_USE_SGE_TEX

// ʯ������
#define USING_STONE_EFFECT (USE_STONE_EFFECT)

// �߹⿪��
#define USING_SPECULAR (!defined(NOT_USE_SPECULAR))

// �ܽ⿪��
#define USING_DISSOLVE_EFFECT (USE_DISSOLVE_EFFECT)

// ������
#define USING_INVISIBLE_EFFECT (USE_INVISIBLE_EFFECT)

// ��ͨ��������ͼ
uniform sampler2D _diffuse;

#if USING_SGE_TEX
// SGE��ͼ
uniform sampler2D _sge;
#endif

// ��ͨ������ĵ�����ɫ
uniform fixed4 _Color;
// �߹��ǿ��
uniform fixed _spec;
// �߹����ɫ
uniform fixed4 _spec5;

uniform fixed _em2;
uniform fixed4 _em3;

// �����
uniform fixed _gloss;
// ��������ͼ�ı���
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
// ʯ����ͼ
uniform sampler2D _StoneTex;
// ʯ��ǿ��
uniform fixed _StoneIntensity;
#endif

#if USE_INVISIBLE_EFFECT
// ����̶�
uniform fixed _InvisibleVisibility;
// ��Եǿ��
uniform half _InvisibleRimPower;
#endif



// -------------------- �������� - ���� --------------------

// ForwardBase/ForwardAdd pass
#if defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)


// ----------------- ���ڿ��ƻ����� -----------------------
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



// ���붥����Ϣ
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
	float3 uv : TEXCOORD0;					// ����ͼuv��д�����ʱuv.z������¼���ֵ
#else
	float2 uv : TEXCOORD0;
#endif

	float3 posWorld : TEXCOORD1;					// ����ռ�λ��
	half3 normalWorld : TEXCOORD2;				// ���߷���
	SHADOW_COORDS(3)						// ������Ӱ�͵����(ADDPASS)	
	UNITY_FOG_COORDS(4)					// fogֻ��һ��half
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

	// ����
	o.pos = UnityObjectToClipPos(v.vertex);
	// UV
	o.uv.xy = v.texcoord;
	// ����
	half3 normalWorld = UnityObjectToWorldNormal(v.normal);
	o.normalWorld = normalWorld;
	// ����ռ䶥��λ��
	float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.posWorld = posWorld;

#if CALC_DEPTH
	//���������д��uv.z
	GSTORE_TRANSFER_DEPTH(o.uv.z, v.vertex);
	//o.uv.z = min(max(length(posWorld - _WorldSpaceCameraPos.xyz) - 1, 0) * 0.03, 1);
#endif
	// vertex�����еĹ�����ɫ�ͻ�����
	// SH/ambient and vertex lights
#if USING_UNITY_SH
	o.sh = 0;
	// Approximated illumination from non-important point lights
	// ������Ǳ�Ҫ�ģ���Ϊunity����ĵ��Դ�õ�cull mask���ǻῪ��
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

	GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);	// �����Ļ�ռ�λ��

	TRANSFER_SHADOW(o); 			// pass shadow coordinates to pixel shader
	UNITY_TRANSFER_FOG(o, o.pos);	// pass fog coordinates to pixel shader
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);

	// ����ͼ��ɫ
	fixed4 tex_diffuse = tex2D(_diffuse, i.uv);

	// Alpha cutoff
	GSTORE_APPLY_ALPHA_CUTOFF(tex_diffuse.a);

	// ������ͼ��ɫ
#if USING_SGE_TEX
	fixed4 tex_sge = tex2D(_sge, i.uv);
#else
	fixed4 tex_sge = fixed4(0, 0, 0, 0);
#endif
	
	// ����˥�� compute lighting & shadowing factor(i�������pos�������)
	UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld)

	// �ƹⷽ�� lightDirection(����ָ�����)
#ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDirection = normalize(UnityWorldSpaceLightDir(i.posWorld));
#else
	fixed3 lightDirection = _WorldSpaceLightPos0.xyz;
#endif

	// ���߷���
	float3 normalDirection = normalize(i.normalWorld);
	// �ɶ���ָ��ͷ
	float3 viewDirection = normalize(UnityWorldSpaceViewDir(i.posWorld));
	// �ɾ�ͷָ��̫��
	float3 halfDirection = normalize(viewDirection + lightDirection);

	// ���matcapӦ�õĲ��֣���ͼ����Ϊ(1 - mc_mask)
#if USING_SGE_TEX
	fixed mc_mask = (tex_sge.g * _m2lerp);
#else
	fixed mc_mask = 0;
#endif

	// ��������ɫ(����mask)
#if USING_STONE_EFFECT
	// ʯ����ͼ
	fixed4 tex_stone = tex2D(_StoneTex, i.uv);
	fixed3 diffuse_map = lerp((_Color.rgb * tex_diffuse.rgb) * (1.0 - mc_mask), tex_stone.rgb, _StoneIntensity);
	tex_diffuse.rgb = lerp(tex_diffuse.rgb, tex_stone.rgb, _StoneIntensity);
#else
	fixed3 diffuse_map = ((_Color.rgb * tex_diffuse.rgb) * (1.0 - mc_mask));
#endif

	// -------------- Unity�ⲿ�� ------------------
	// Unity������ɫ + Ӱ��˥��
	half3 atten_light_color = _LightColor0.rgb * atten;
	
	half3 specular = half3(0, 0, 0);
#if USING_SPECULAR
	// -------------- �߹ⲿ�� ------------------
	// ��ͼ���Ӹ߹���ɫ
	half3 specular_color = (diffuse_map * ((_spec5.rgb * _spec) * tex_sge.r));
	// ��ø߹��������ɫ(Gloss�������_gloss��������)
	half3 direct_specular = atten_light_color * pow(max(0, dot(halfDirection, normalDirection)), exp2(_gloss * 10.0 + 1.0)) * specular_color;
	specular += direct_specular;

	
#if USING_STONE_EFFECT
	// ʯ�������ϵ��
	specular *= 1.0 - _StoneIntensity;
#endif // USING_STONE_EFFECT

#endif

	// -------------- �����䲿�� ------------------
	// ����
	//half3 lambert_term = max(0.0, (0.5 * dot(normalDirection, lightDirection) + 0.5));
	half3 lambert_term = max(0.0, (dot(normalDirection, lightDirection)));

	// ��ʽ����
	// ��ʽ���壺ʹ��ֱ��ⶨ��������ɫ
	half3 direct_diffuse = lambert_term * atten_light_color;

	// ���������
	half3 indirect_diffuse = half3(0, 0, 0);

#if USING_UNITY_SH
	// ���������������ı�Ȩ��, ��SH����Ҳ���뵽���������
	//indirect_diffuse += i.sh.rgb * (1 - lambert_term);
	indirect_diffuse += WeightedAmbient(i.sh.rgb, direct_diffuse);
#endif

	half3 extra_diffuse = half3(0, 0, 0);
#if (USING_EMISSIVE && defined(UNITY_PASS_FORWARDBASE))
	// ���Ȳ���
	float diffuse_lumin = ColorLuminosity(direct_diffuse + indirect_diffuse);
	float emissive_lumin = ColorLuminosity(_em3.rgb);
	float extraLumin = saturate(MIN_LUMIN - diffuse_lumin);
	extra_diffuse += (extraLumin * _em3.rgb / emissive_lumin);
#endif
	// (ֱ�������� + ��������� + ����) * ��������ɫ
	half3 diffuse = saturate(direct_diffuse + indirect_diffuse + extra_diffuse) * diffuse_map;

	half3 emissive = half3(0, 0, 0);
#if USING_EMISSIVE || USING_MATCAP
	// ��diffuse�ı��Ͷ�,������ͼ(�Է����MatCapʹ��)
	fixed3 desaturate_diffuse = lerp(tex_diffuse.rgb, dot(tex_diffuse.rgb, float3(0.3, 0.59, 0.11)), mc_mask);
#endif

#if USING_EMISSIVE
	// -------------- �Է��ⲿ�� ------------------
	fixed emission_factor = 1.0;
#if USING_STONE_EFFECT
	// ʯ�������ϵ��
	emission_factor *= 1.0 - _StoneIntensity;
#endif  // USING_STONE_EFFECT

	// ����diffuse��ͼ���Ͷ�
	emissive += (tex_sge.b * desaturate_diffuse * (_em3.rgb * _em2)) * emission_factor;
#endif // USING_EMISSIVE

#if USING_MATCAP
	// -------------- MatCap���� ------------------
	fixed matcap_factor = 1.0;
#if USING_STONE_EFFECT
	// ʯ�������ϵ��
	matcap_factor *= 1.0 - _StoneIntensity;
#endif // USING_STONE_EFFECT

	// MaterailCapture��ͼ��ɫ
	fixed4 tex_MatCapTex = tex2D(_MatCapTex, (mul(UNITY_MATRIX_V, float4(normalDirection, 0)).xy * 0.5 + 0.5));
	emissive += (mc_mask * ((desaturate_diffuse * tex_MatCapTex.rgb * _MatCapColor.rgb) + pow(max(max(tex_MatCapTex.r, tex_MatCapTex.g), tex_MatCapTex.b), 3.0)) * _MatCapIntensity) * matcap_factor;

#endif // USING_MATCAP


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
	//final_alpha = i.uv.z;
	GSTORE_CALC_DEPTH_ZWRITE_TO_ALPHA(final_alpha, i.uv.z, _ZWrite, final_alpha);
#endif
	// -------------- �������ɫ ------------------
	fixed4 final_color = fixed4(diffuse + specular + emissive, final_alpha);


#if USING_DISSOLVE_EFFECT
	// -------------- �ܽ� ------------------
	fixed tex_dissAlpha = tex2D(_DissTex, i.uv).r;
	fixed alphaDist = tex_dissAlpha - _DissProgress + 0.01;
	fixed remainPart = max(0, sign(alphaDist));		// 0��ʾӦ����ȫ͸��
	clip(remainPart - 0.1);
#if defined(UNITY_PASS_FORWARDBASE)
	fixed dissFactor = saturate(alphaDist / _DissSize);
	fixed4 dissColor = lerp(_DissStartColor, _DissEndColor, dissFactor);
	fixed dissPart = max(0, sign(_DissSize - alphaDist));	// 1��ʾ�����ܽ⽥����, 0��ʾ�����ܽ⽥����
	final_color.rgb = dissColor.rgb * dissPart + (1.0 - dissPart) * final_color.rgb;
#endif
#endif // USING_DISSOLVE_EFFECT

#if USE_INVISIBLE_EFFECT
	// -------------- ���� ------------------
	// ��ɫ�ɼ�
	fixed col_visibility = saturate((_InvisibleVisibility - 0.5) * 2);
	// ��Ե��ɼ�
	fixed rim_visibility = saturate(_InvisibleVisibility * 2);
	// ��Ե��Ĳ���
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


// -------- ��ɫ���ڵ���� see through --------
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

// ���붥����Ϣ
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

	// һ��Ҫ��TEXCOORD1��ʼ,V2F_SHADOW_CASTER�п���ռ��TEXCOORD0
#if GSTORE_USING_ALPHA_CUTOFF
	float2 uv : TEXCOORD1;					// ����ͼ��UV
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

	// ��͸��ʵ��
	GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(i.pos.xy);

	SHADOW_CASTER_FRAGMENT(i)
}

#endif // defined(UNITY_PASS_SHADOWCASTER)

#endif // __Character__
