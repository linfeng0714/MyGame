﻿
Shader "VFX/SceneProjector"
{
	 Properties{
		_Color("Color", COLOR) = (1, 1, 1, 1)
        _ShadowTex("Cookie", 2D) = "gray" {}
        //_FalloffTex("FallOff", 2D) = "white" {}
    }
    Subshader{
        Tags{ "Queue" = "Transparent" }
        Pass{
        ZWrite Off
		//Ztest Equal
        ColorMask RGB
        //Blend DstColor Zero
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -1

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile_fog
        #include "UnityCG.cginc"

        struct v2f {
            float4 uvShadow : TEXCOORD0;
            float4 uvFalloff : TEXCOORD1;
            UNITY_FOG_COORDS(2)
            float4 pos : SV_POSITION;
        };

		fixed4 _Color;
        float4x4 unity_Projector;
        float4x4 unity_ProjectorClip;

        v2f vert(float4 vertex : POSITION)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(vertex);
            o.uvShadow = mul(unity_Projector, vertex);
            o.uvFalloff = mul(unity_ProjectorClip, vertex);
            UNITY_TRANSFER_FOG(o,o.pos);
            return o;
        }

        sampler2D _ShadowTex;
        //sampler2D _FalloffTex;

        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
            //texS.a = 1.0-texS.a;

            //fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
            //lerp(fixed4(1, 1, 1, 0), texS, texF.a);
            fixed4 res = texS * _Color;

			UNITY_APPLY_FOG(i.fogCoord, res);
            return res;
        }
            ENDCG
        }
    }
}
