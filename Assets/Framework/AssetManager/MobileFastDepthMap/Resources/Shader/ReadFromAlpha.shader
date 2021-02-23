Shader "MobileFastDepthMap/ReadFromAlpha" {
	Properties{
		_MainTex ("Base (RGBA)", 2D) = "" {}
	}
	SubShader{
		Lighting Off

		Pass{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);

			struct vertexInput 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			vertexOutput vert(vertexInput v) 
			{
				vertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag(vertexOutput i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).a;
				return fixed4(depth, 0, 0, 1);
			}
			ENDCG
		}
	}
}
