// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:32727,y:32704,varname:node_4013,prsc:2|emission-5801-OUT,clip-3327-OUT;n:type:ShaderForge.SFN_Color,id:1304,x:31548,y:32828,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_1304,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:6764,x:32006,y:32629,ptovrint:False,ptlb:dif_tex,ptin:_MainTex,varname:node_6764,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:36f361063cc20d94c945195771e02a55,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9264,x:32297,y:32771,varname:node_9264,prsc:2|A-6764-RGB,B-1304-RGB;n:type:ShaderForge.SFN_Fresnel,id:5902,x:32181,y:32507,varname:node_5902,prsc:2|EXP-9781-OUT;n:type:ShaderForge.SFN_Add,id:5801,x:32548,y:32721,varname:node_5801,prsc:2|A-5469-OUT,B-9264-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9781,x:31890,y:32384,ptovrint:False,ptlb:rim_exp,ptin:_RimPower,varname:node_9781,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Color,id:4701,x:32071,y:32220,ptovrint:False,ptlb:node_4701,ptin:_RimColor,varname:node_4701,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.3970588,c3:0.3970588,c4:1;n:type:ShaderForge.SFN_Multiply,id:5469,x:32505,y:32355,varname:node_5469,prsc:2|A-4701-RGB,B-5902-OUT,C-4754-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4754,x:32297,y:32693,ptovrint:False,ptlb:rim_v,ptin:_RimIntensity,varname:node_4754,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_Tex2d,id:1622,x:31822,y:32971,ptovrint:False,ptlb:Mask,ptin:_MaskTex,varname:node_1622,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:9c66ada5e480f4f49addaffb71b0acec,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:5774,x:31665,y:33202,ptovrint:False,ptlb:dis_v,ptin:_DissolveValue,varname:node_5774,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0.3511034,max:1;n:type:ShaderForge.SFN_Desaturate,id:3327,x:32412,y:33040,varname:node_3327,prsc:2|COL-8989-OUT;n:type:ShaderForge.SFN_Add,id:8989,x:32177,y:33025,varname:node_8989,prsc:2|A-1622-RGB,B-5774-OUT;proporder:1304-6764-9781-4701-4754-1622-5774;pass:END;sub:END;*/

Shader "VFX/RimDissolve" 
{
    Properties 
	{
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex("Base(RGB)", 2D) = "white" {}
        _RimColor ("Rim Color(RGB)", Color) = (1,0.3970588,0.3970588,1)
		_RimPower("Rim Power", Float) = 1
        _RimIntensity("Rim Intensity", Float ) = 3
        _MaskTex("Mask(RGB)", 2D) = "white" {}
        _DissolveValue ("Dissolve Value", Range(0, 1)) = 1
    }
    SubShader
	{
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Pass {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
  
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
			#include "../Lib/FastDepthMap.cginc"

            #pragma multi_compile_fwdbase
            #pragma target 2.0

            uniform fixed4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform half _RimPower;
            uniform half4 _RimColor;
            uniform half _RimIntensity;
            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform fixed _DissolveValue;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float3 uv0 : TEXCOORD0;
                fixed3 viewDir : TEXCOORD1;
                fixed3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0.xy = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos( v.vertex );

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.viewDir = UnityWorldSpaceViewDir(worldPos);

				GSTORE_TRANSFER_DEPTH(o.uv0.z, v.vertex);
                return o;
            }

            float4 frag(VertexOutput i) : SV_Target 
			{
                fixed3 normalDirection = normalize(i.normalDir);
				fixed3 viewDirection = normalize(i.viewDir);

                fixed3 mask_color = tex2D(_MaskTex,TRANSFORM_TEX(i.uv0, _MaskTex));

                clip(Luminance(mask_color.rgb + _DissolveValue) - 1);

                fixed3 main_color = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
				fixed3 finalColor = main_color.rgb * _Color.rgb;
                fixed3 emissive = _RimColor.rgb * pow(1.0 - max(0, dot(normalDirection, viewDirection)), _RimPower) * _RimIntensity;

				fixed final_alpha = 1;
				GSTORE_CALC_DEPTH_TO_ALPHA(final_alpha, i.uv0.z, final_alpha);
                return fixed4(finalColor + emissive, final_alpha);
            }
            ENDCG
        }
        Pass 
		{
            Name "ShadowCaster"
            Tags 
			{
                "LightMode"="ShadowCaster"
            }

            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
           
            #pragma target 2.0

            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform fixed _DissolveValue;

            struct VertexInput 
			{
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput 
			{
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };

            VertexOutput vert(VertexInput v)
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }

            float4 frag(VertexOutput i) : COLOR 
			{
                float4 mask_color = tex2D(_MaskTex,TRANSFORM_TEX(i.uv0, _MaskTex));
				clip(Luminance(mask_color.rgb + _DissolveValue) - 1);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
