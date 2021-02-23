Shader "Actor/Animal"
{
	Properties
	{
		_Color("Diffuse Color", Color) = (1,1,1,1)
		[NoScaleOffset]_diffuse("Diffuse (RGBA)", 2D) = "white" {}
		//[NoScaleOffset]_sge("SGE (RGB) / Mask (A)", 2D) = "white" {}
		//_spec5("Spec Color", Color) = (0.5,0.5,0.5,1)
		//_spec("Spec Intensity", Range(0, 5)) = 5
		//_gloss("Gloss Intensity", Range(0, 1)) = 1
		//_em3("Emission Color", Color) = (0.5,0.5,0.5,1)
		//_em2("Emission Intensity", Range(0, 3)) = 1.292027
		//_m2lerp("Material 2 Lerp", Range(0, 1)) = 1
		//[NoScaleOffset]_MatCapTex("MatCap Tex", 2D) = "white" {}
		//_MatCapColor("MatCap Color", Color) = (0.5,0.5,0.5,1)
		//_MatCapIntensity("MatCap Intensity", Range(0, 10)) = 1

		[Space(100)]
		[Header(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)]
		// ------------ 下面为程序使用 ------------

		// 抖动透明(可程序控制)
		[Header(Dither Transparency    Art Dont Use)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

		// 普通透明(可程序控制)
		[Header(Transparency Art DO NOT Use)]
		[Toggle(USE_TRANSPARENCY)] _UseTransparency("Use Transparency", Float) = 0
		_Transparency("Transparency", Range(0, 1)) = 1

		// Blending state 内部使用
		[Header(Internal Blending State)]
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Source", Int) = 1
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dest", Int) = 0
		[MaterialEnum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1

		// 关闭深度写入alpha
		[MaterialEnum(Off,0,On,1)] _NoDepthWrite("No Depth Write", Int) = 0

		
		[Space(100)]
		[Header(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)]

		// 隐身效果
		[Header(Invisible Effect)]
		[Toggle(USE_INVISIBLE_EFFECT)] _UseInvisibleEffect("Use Invisible Effect", Float) = 0
		_InvisibleVisibility("Invisible Visibility", Range(0, 1)) = 0.5
		_InvisibleRimPower("Invisible RimPower", Range(0.5, 20.0)) = 0.8
	}
	SubShader
	{
		Tags
		{
			"ObjectType"="Actor"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}
		LOD 200

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
			// 排除一些不用的Unity内置变量
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants LIGHTPROBE_SH
			#define LIGHTPROBE_SH 1

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv novertexlights
			#pragma multi_compile_fog

			// 使用Alpha cutoff效果 / 贴图透明
			#pragma multi_compile __ USE_ALPHA_CUTOFF

				// 使用石化效果 / 使用溶解效果 / 使用隐身效果
			#pragma multi_compile __ USE_INVISIBLE_EFFECT

			// 关闭使用 MATCAP
			//#pragma multi_compile __ NOT_USE_MATCAP

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define NOT_USE_SPECULAR 1
			#define NOT_USE_EMISSIVE 1
			#define NOT_USE_MATCAP 1
			#define NOT_USE_SGE_TEX 0
			#define CALC_DEPTH 1
			
			// 关闭SH光照
			#define UNITY_PASS_FORWARDBASE
			
			#include "Character.cginc"
			ENDCG
		}
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv novertexlights

			// 使用Alpha cutoff效果
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			#define UNITY_PASS_SHADOWCASTER

			#include "Character.cginc"
			ENDCG
		}
	}
}
