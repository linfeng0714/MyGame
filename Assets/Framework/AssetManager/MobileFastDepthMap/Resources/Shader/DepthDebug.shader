Shader "MobileFastDepthMap/DebugDepth" {
	Properties{
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
#include "UseFastDepthMap.cginc"
#pragma multi_compile __ DepthWrite_ON


	struct vertexInput {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct vertexOutput {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};	

	vertexOutput vert(vertexInput v) {
		vertexOutput o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		return o;
	}

	fixed4 frag(vertexOutput i) : COLOR{
		float depth = GSTORE_SAMPLE_DEPTH(i.uv);

		return fixed4(depth, depth, depth,1);
	}
		ENDCG
	}
	}
}

