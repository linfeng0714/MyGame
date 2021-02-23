Shader "Actor/Base_SeeThrough" {
    Properties {
        _Color ("Diffuse Color", Color) = (1,1,1,1)
		[NoScaleOffset]_diffuse ("Diffuse (RGBA)", 2D) = "white" {}
		[NoScaleOffset]_sge ("SGE (RGB) / Mask (A)", 2D) = "white" {}
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

		// 描边
		[Header(See Through)]
		_SeeThroughOutlineColor("See through Outline Color", Color) = (1, 0.6, 0, 1)
		_SeeThroughOutline("See through Outline", Range(0,1)) = 0
		_SeeThroughRimCutoff("See through RimCutoff", Range(0,1)) = 0


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
		[NoScaleOffset]_StoneTex("Stone Texture (RGBA)", 2D) = "white" {}
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
			"ObjectType" = "Actor"			// 给程序里识别使用的类型(类型请看ShaderDefine)
            "RenderType"="Opaque"
            "Queue" = "Geometry+455"		// 这个顺序在AlphaTest之后, AlphaTest的树也会遮挡角色产生透视
        }
        LOD 200

//		Pass {
//			Name "SEETHROUGH"
//			Tags{ "LightMode" = "Always" }
//			ZTest Greater
//			ZWrite Off
//			Blend SrcAlpha OneMinusSrcAlpha
//			Cull Front
//
//			CGPROGRAM
//			#pragma target 2.0
//			#pragma vertex vert_seethrough
//			#pragma fragment frag_seethrough
//
//
//			#define USE_SEE_THROUGH
//
//			#include "Character.cginc"
//
//			ENDCG
//		}
		UsePass "Actor/Base/FORWARD"

		//UsePass "Actor/Base/FORWARD_DELTA"

		UsePass "Actor/Base/SHADOWCASTER"
    }
	CustomEditor "ActorBaseShaderGUI"
}
