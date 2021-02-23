Shader "VFX/SceneTipsBox"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_AmbientColor("AmbientColor", Color) = (0,0,0,0)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent-100" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "Always" }

			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off

			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform fixed4 _AmbientColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 c = tex2D(_MainTex, i.uv).rgb * _AmbientColor.rgb;
				return fixed4(c, _AmbientColor.a);
			}
			ENDCG
		}
	}
}
