Shader "Actor/Base" {
    Properties {
        _Color ("Diffuse Color", Color) = (1,1,1,1)
		[NoScaleOffset]_diffuse ("Diffuse (RGBA)", 2D) = "white" {}
		[NoScaleOffset]_sge ("SGE (RGB) / Mask (A)", 2D) = "black" {}
        _spec5 ("Spec Color", Color) = (0.5,0.5,0.5,1)
        _spec ("Spec Intensity", Range(0, 5)) = 5
        _gloss ("Gloss Intensity", Range(0, 1)) = 1
        _em3 ("Emission Color", Color) = (0.5,0.5,0.5,1)
        _em2 ("Emission Intensity", Range(0, 3)) = 1.292027
        _m2lerp ("Material 2 Lerp", Range(0, 1)) = 1
		[NoScaleOffset]_MatCapTex ("MatCap Tex", 2D) = "white" {}
        _MatCapColor ("MatCap Color", Color) = (0.5,0.5,0.5,1)
        _MatCapIntensity ("MatCap Intensity", Range(0, 10)) = 1

		// 透明通道裁剪(只在面板设置用)
		[Header(Alpha Cutoff)]
		[Toggle(USE_ALPHA_CUTOFF)] _UseAlphaCutoff("Use Alpha Cutoff", Float) = 0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0

		// 普通贴图透明(只在面板设置用)
		[Header(Texture Transparency)]
		[Toggle(USE_TEX_TRANSPARENCY)] _UseTexTransparency("Use Texture Transparency", Float) = 0

		[Space(100)]
		[Header(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)]
		// ------------ 下面为程序使用 ------------

		// 溶解效果
		[Header(Dissolve Effect)]
		[Toggle(USE_DISSOLVE_EFFECT)] _UseDissolveEffect("Use Dissolve Effect", Float) = 0
		[NoScaleOffset]_DissTex("Dissolve Texture", 2D) = "white" {}
		_DissStartColor("Dissolve Start Color", Color) = (1, 1, 0, 1)	// 溶解颜色渐变区的开始颜色
		_DissEndColor("Dissolve End Color", Color) = (1, 0, 0, 1)
		_DissProgress("Dissolve Progress", Range(0, 1)) = 0			// 溶解消失的进度, 随时间改变这个来实现消失的动画
		_DissSize("Dissolve Size", Range(0, 0.5)) = 0.1				// 溶解颜色渐变区的大小范围

		// 石化效果
		[Header(Stone Effect)]
		[Toggle(USE_STONE_EFFECT)] _UseStoneEffect("Use Stone Effect", Float) = 0
		[NoScaleOffset]_StoneTex ("Stone Texture (RGBA)", 2D) = "white" {}
		_StoneIntensity("Stone Intensity", Range(0, 1)) = 0

		// 隐身效果
		[Header(Invisible Effect)]
		[Toggle(USE_INVISIBLE_EFFECT)] _UseInvisibleEffect("Use Invisible Effect", Float) = 0
		_InvisibleVisibility("Invisible Visibility", Range(0, 1)) = 0.5
		_InvisibleRimPower("Invisible RimPower", Range(0.5, 20.0)) = 0.8

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
    }
    SubShader {
        Tags {
			"ObjectType"="Actor"			// 给程序里识别使用的类型(类型请看ShaderDefine)
            "RenderType"="Opaque"
            "Queue" = "Geometry+456"		// 这个顺序在AlphaTest之后, 因为角色武器用的也是这个shader, 所以这个渲染顺序在seethrough之后, 确保武器不会遮挡角色
        }
        LOD 200
        Pass {
            Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

            CGPROGRAM
			// 排除一些不用的Unity内置变量
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants LIGHTPROBE_SH
			#define LIGHTPROBE_SH 1

			#pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv novertexlights
			// make fog work
			#pragma multi_compile_fog

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用Alpha cutoff效果 / 贴图透明
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// 关闭使用 MATCAP
			#pragma multi_compile __ NOT_USE_MATCAP
			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			// 使用石化效果 / 使用溶解效果 / 使用隐身效果
			#pragma multi_compile __ USE_STONE_EFFECT USE_DISSOLVE_EFFECT USE_INVISIBLE_EFFECT

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE

			#include "Character.cginc"
            ENDCG
        }
   //     Pass {
   //         Name "FORWARD_DELTA"
			//Tags{ "LightMode" = "ForwardAdd" }
			//Blend[_SrcBlend] One
			//Fog{ Color(0,0,0,0) } // in additive pass fog should be black
			//ZWrite Off
			//ZTest LEqual

   //         CGPROGRAM
			//// 排除一些不用的Unity内置变量
			//#pragma skip_variants FOG_EXP FOG_EXP2
			//// Add Pass一些用不到的光照
			//#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

			//#pragma target 3.0
   //         #pragma vertex vert
   //         #pragma fragment frag
			//// Apparently need to add this declaration 
			//#pragma multi_compile_fwdadd nodynlightmap nodirlightmap nolightmap noshadowmask nolppv novertexlights
			//// make fog work
			//#pragma multi_compile_fog

			//// -------------- 自定义变体 --------------
			//// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			//// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			//// 使用Alpha cutoff效果 / 贴图透明
			////#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			//#pragma shader_feature __ USE_ALPHA_CUTOFF

			//// 关闭使用 MATCAP
			//#pragma multi_compile __ NOT_USE_MATCAP
			//// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			////#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			//// 使用石化效果 / 使用溶解效果 / 使用隐身效果
			//#pragma multi_compile __ USE_STONE_EFFECT USE_DISSOLVE_EFFECT USE_INVISIBLE_EFFECT

			//// 优化(减少变体数量，把计算小的变体默认就加上)：
			//#define USE_TEX_TRANSPARENCY 1
			//#define USE_TRANSPARENCY 1

			//// SH光照
			//#define UNITY_PASS_FORWARDADD
   //         
			//#include "Character.cginc"
   //         ENDCG
   //     }

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

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
	CustomEditor "ActorBaseShaderGUI"
}
