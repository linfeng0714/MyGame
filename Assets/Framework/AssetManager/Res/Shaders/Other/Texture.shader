Shader "Unlit/Texture_WriteDepth" {
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		Lighting Off
		//ZWrite Off

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "../Lib/FastDepthMap.cginc"

	uniform sampler2D _MainTex;

	struct vertexInput {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct vertexOutput {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		fixed depth : TEXCOORD1;
	};

	vertexOutput vert(vertexInput v) {
		vertexOutput o;
		o.pos = UnityObjectToClipPos(v.vertex);

		GSTORE_TRANSFER_DEPTH(o.depth, v.vertex);

		o.uv = v.texcoord;
		return o;
	}

	fixed4 frag(vertexOutput i) : COLOR{
		fixed3 col = tex2D(_MainTex, i.uv);
		fixed alpha = 1.0;

		GSTORE_CALC_DEPTH_TO_ALPHA(alpha, i.depth, alpha);
		return fixed4(col.rgb, alpha);
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
}

