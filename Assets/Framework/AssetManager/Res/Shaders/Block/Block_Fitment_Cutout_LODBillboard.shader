// 家具LOD专用billboard(没有半透明的)(透明通道Cutout)
// 单面
// 贴图带透明通道，透明通道作为cutout依据
// Y轴旋转的billboard


Shader "Block/Block_Fitment_Cutout_LODBillboard"
{
	Properties
	{
		[Header(Alpha channel is use for Cutout)]
		[NoScaleOffset]_MainTex ("主贴图(RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5

		// 抖动透明(可程序控制)
		[Header(Dither Transparency)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0
	}
	SubShader
	{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		//Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			//ColorMask RGB

			CGPROGRAM
			// 排除一些不用的Unity内置变量
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants LIGHTPROBE_SH
			#define LIGHTPROBE_SH 1

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog
			// LOD
			#pragma multi_compile_vertex __ BILLBOARD_FACE_CAMERA_POS
			#pragma multi_compile_fragment __ LOD_FADE_CROSSFADE
			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			
			// SH光照
			#define UNITY_PASS_FORWARDBASE

			#define CALC_DEPTH 1

			#include "LODBillboard.cginc"
			ENDCG
		}

		//Pass
		//{
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ColorMask RGB
		//	ZWrite Off
		//	Blend One One

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 3.0
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nodynlightmap nodirlightmap nolightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog
		//	// LOD
		//	#pragma multi_compile_vertex __ BILLBOARD_FACE_CAMERA_POS
		//	#pragma multi_compile_fragment __ LOD_FADE_CROSSFADE
		//	// Dither Transparency
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY
		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD
		//	
		//	#include "LODBillboard.cginc"
		//	
		//	ENDCG
		//}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "Caster"
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			// LOD
			#pragma multi_compile_vertex __ BILLBOARD_FACE_CAMERA_POS
			#pragma multi_compile_fragment __ LOD_FADE_CROSSFADE
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 

			#define UNITY_PASS_SHADOWCASTER

			#include "LODBillboard.cginc"

			ENDCG
		}
	}
	
	FallBack "Transparent/Cutout/VertexLit"
}
