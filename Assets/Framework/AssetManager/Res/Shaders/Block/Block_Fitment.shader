// 家具(通用)
Shader "Block/Block_Fitment"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("主贴图(RGB)", 2D) = "white" {}
		[MaterialEnum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Int) = 2

		// 法线贴图(只在面板设置用)
		[Header(Bump Map)]
		[Normal][NoScaleOffset]_BumpMapTex("Bump Map 贴图", 2D) = "white" {}

		// 自发光效果(只在面板设置用)
		[Header(Self Illumin)]
		//[Toggle(USE_SELF_ILLUMIN)] _UseSelfIllumin("Use Self Illumin", Float) = 0
		[Toggle] _BaseIlluminWeighted("Base Illumin Weighted", Float) = 1
		[Enum(SelfIlluminChannel)] _SelfIlluminChannel("Self Illumin 依据", Float) = 0
		_SelfIlluminIntensity("Self Illumin 强度",  Range(0, 1)) = 0
		[NoScaleOffset]_SelfIlluminTex("Self Illumin 贴图", 2D) = "white" {}

		// 透明通道裁剪(只在面板设置用)
		[Header(Alpha cutoff)]
		[Toggle(USE_ALPHA_CUTOFF)] _UseAlphaCutoff("Use Alpha Off", Float) = 0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5

		// 普通贴图透明(只在面板设置用)
		[Header(Texture Transparency)]
		[Toggle(USE_TEX_TRANSPARENCY)] _UseTexTransparency("Use Texture Transparency", Float) = 0
		_TexTransparency("Texture Transparency", Range(0, 1)) = 1

		// 草类顶点动画(只在面板设置用)
		[Header(Grass Vertex Animation)]
		[Toggle(USE_GRASS_VA)] _UseGrassVA("Use Grass Vertex Animation", Float) = 0
		_GrassVAStartX("Grass 线性开始 X", Float) = 0.0
		_GrassVALerp2X("Grass 是否使用线性 X", Range(0,1)) = 0.0
		_GrassVAIntensityDirX("Grass 强度与方向 X", Range(-5,5)) = 1.0
		_GrassVAScaleX("Grass VA Scale X", Range(0, 2)) = 1.0
		_GrassVAStartY("Grass 线性开始 Y", Float) = 0.0
		_GrassVALerp2Y("Grass 是否使用线性 Y", Range(0,1)) = 1.0
		_GrassVAIntensityDirY("Grass 强度与方向 Y", Range(-5,5)) = 1.0
		_GrassVAScaleY("Grass VA Scale Y", Range(0, 2)) = 0.0
		_GrassVAIntensity("Grass 整体强度", Float) = 0.3
		_GrassVATimeScale("Grass VA 频率",Float) = 1
		_GrassVAUnified("Grass VA 统一性", Range(0, 1)) = 0.0

		// 方块光照(只在面板设置用)
		[Header(Block Light)]
		[Toggle] _NormalWeightedLight("Normal Weighted Light", Float) = 1		// 方块光照是否考虑面的法线

		[Space(100)]
		[Header(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)]
		// ------------ 下面为程序使用 ------------

		// 抖动透明(可程序控制)
		[Header(Dither Transparency    Art Dont Use)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

		// Alpha控制透明(可程序控制)
		[Header(Transparency    Art Dont Use)]
		[Toggle(USE_TRANSPARENCY)] _UseTransparency("Use Transparency", Float) = 0
		_Transparency("Transparency", Range(0, 1)) = 1

		// 脚本MaterialPropertyBlock动态数据
		[Header(MaterialPropertyBlock Light)]
		//_SkyLight("Sky Light", Range(0,1)) = 1
		//_BlockLightUp ("BlockLightUp", Color) = (0, 0, 0, 0)
		//_BlockLightDown ("BlockLightDown", Color) = (0, 0, 0, 0)
		//_SkyLightUp ("SkyLightUp", Color) = (1, 1, 1, 1)
		//_SkyLightDown ("SkyLightDown", Color) = (1, 1, 1, 1)
		_ColorEncode("ColorEncode", Vector) = (0, 0, 0, 0)
		_BlockSize ("BlockSize", Vector) = (1, 1, 1, 0)
		//_OriginOffset ("OriginOffset", Vector) = (0, 0, 0, 0)
		// 是否可被火把光照亮
		[Toggle] _EnableBlockLight("Enable Block Light", Float) = 1

		// Blending state 内部使用
		[Header(Internal Blending State)]
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Source", Int) = 1
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dest", Int) = 0
		[MaterialEnum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1

		// 关闭深度写入alpha
		[MaterialEnum(Off,0,On,1)] _NoDepthWrite("No Depth Write", Int) = 0
	}

	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.5
	// 全功能开放
	// GPU Instancing
	SubShader
	{
		Tags
		{
			"ObjectType" = "Fitment"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			"DisableBatching" = "LODFading"
			"IgnoreProjector" = "True"
		}
		LOD 100


		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			Cull[_Cull]
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
			// 排除一些不用的Unity内置变量
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants LIGHTPROBE_SH
			#define LIGHTPROBE_SH 1

			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog
			// LOD
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			// Gpu Instancing
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling lodfade

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 法线(暂时没有家具使用)
			//#pragma multi_compile __ USE_BUMP_MAP
			// 自发光效果
			#pragma multi_compile __ USE_SELF_ILLUMIN_MAIN_TEX_ALPHA USE_SELF_ILLUMIN_TEX
			// 使用草顶点动画
			#pragma multi_compile __ USE_GRASS_VA
			// 使用透明通道裁剪 / 贴图透明
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			#pragma multi_compile __ ENABLE_FITMENT_ALONE
			
			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 0
			#define USE_SELF_ILLUMIN 1
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE

			#include "Fitment.cginc"
			ENDCG
		}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			Cull[_Cull]

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// LOD
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			// Gpu Instancing
            #pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling lodfade

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间
			// 使用草顶点动画
			#pragma multi_compile __ USE_GRASS_VA
			// 使用透明通道裁剪
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			#define UNITY_PASS_SHADOWCASTER
			
			#include "Fitment.cginc"
			ENDCG
		}
	}


	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.0
	// 全功能开放
	// Instancing需要SM3.5
	// 没有instancing，需要去掉草的变体
	SubShader
	{
		Tags
		{
			"ObjectType" = "Fitment"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			"DisableBatching" = "LODFading"
			"IgnoreProjector" = "True"
		}
		LOD 100


		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			Cull[_Cull]
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

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
			#pragma multi_compile __ LOD_FADE_CROSSFADE

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 法线(暂时没有家具使用)
			//#pragma multi_compile __ USE_BUMP_MAP
			// 自发光效果
			#pragma multi_compile __ USE_SELF_ILLUMIN_MAIN_TEX_ALPHA USE_SELF_ILLUMIN_TEX
			// 使用透明通道裁剪 / 贴图透明
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			#pragma multi_compile __ ENABLE_FITMENT_ALONE

			// 没有Instancing有可能是动态Batch，所以不能做顶点动画
			#define USE_GRASS_VA 0

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 0
			#define USE_SELF_ILLUMIN 1
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE

			#include "Fitment.cginc"
			ENDCG
		}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			Cull[_Cull]

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// LOD
			#pragma multi_compile __ LOD_FADE_CROSSFADE

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY


			// 没有Instancing有可能是动态Batch，所以不能做顶点动画
			#define USE_GRASS_VA 0

			#define UNITY_PASS_SHADOWCASTER
			
			#include "Fitment.cginc"
			ENDCG
		}
	}

	// VFace 只能在SM 3.0或以上
	SubShader
	{
		Tags
		{
			"ObjectType" = "Fitment"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			"DisableBatching" = "LODFading"
			"IgnoreProjector" = "True"
		}
		LOD 100


		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			Cull[_Cull]
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
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog
			// LOD
			#pragma multi_compile __ LOD_FADE_CROSSFADE

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 法线
			#pragma multi_compile __ USE_BUMP_MAP
			// 自发光效果
			//#pragma multi_compile __ USE_SELF_ILLUMIN
			#pragma multi_compile __ USE_SELF_ILLUMIN_MAIN_TEX_ALPHA USE_SELF_ILLUMIN_TEX
			// 使用透明通道裁剪 / 贴图透明
			//#pragma multi_compile __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			#pragma multi_compile __ ENABLE_FITMENT_ALONE

			// 没有Instancing有可能是动态Batch，所以不能做顶点动画
			#define USE_GRASS_VA 0

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 0
			#define USE_SELF_ILLUMIN 1
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE

			#include "Fitment.cginc"
			ENDCG
		}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			Cull[_Cull]

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// LOD
			#pragma multi_compile __ LOD_FADE_CROSSFADE

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma multi_compile __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY


			// 没有Instancing有可能是动态Batch，所以不能做顶点动画
			#define USE_GRASS_VA 0

			#define UNITY_PASS_SHADOWCASTER
			
			#include "Fitment.cginc"
			ENDCG
		}
	}
	CustomEditor "FitmentShaderGUI"
}
