// 家具(通用)
Shader "Block/Block_Fitment_Glass"
{
	Properties
	{
		[MaterialEnum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Int) = 2

		[Header(Color)]
		_Color("Main Color", Color) = (1,1,1,1)

		// 普通贴图透明(只在面板设置用)
		[Header(Texture Transparency)]
		[Toggle(USE_TEX_TRANSPARENCY)] _UseTexTransparency("Use Texture Transparency", Float) = 0
		_TexTransparency("Texture Transparency", Range(0, 1)) = 1

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
		_SkyLight("Sky Light", Range(0,1)) = 1
		_BlockLightUp ("BlockLightUp", Color) = (0, 0, 0, 0)
		_BlockLightDown ("BlockLightDown", Color) = (0, 0, 0, 0)
		_SkyLightUp ("SkyLightUp", Color) = (1, 1, 1, 1)
		_SkyLightDown ("SkyLightDown", Color) = (1, 1, 1, 1)
		_BlockSize ("BlockSize", Vector) = (1, 1, 1, 0)
		_OriginOffset ("OriginOffset", Vector) = (0, 0, 0, 0)
		// 是否可被火把光照亮
		[Toggle] _EnableBlockLight("Enable Block Light", Float) = 1

		// Blending state 内部使用
		[Header(Internal Blending State)]
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Source", Int) = 1
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dest", Int) = 0
		[MaterialEnum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1
	}

	// 可选屏蔽效果项:
	// DISABLE_MC_LIGHT

	SubShader
	{
		Tags
		{
			"ObjectType" = "Fitment"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			"DisableBatching" = "LODFading"
			"IgnoreProjector" = "False"
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

			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 1
			#define USE_BUMP_MAP 0
			#define USE_SELF_ILLUMIN 0
			#define USE_SELF_ILLUMIN_MAIN_TEX_ALPHA 0
			#define USE_SELF_ILLUMIN_TEX 0
			#define USE_GRASS_VA 0
			#define USE_ALPHA_CUTOFF 0
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1

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

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 1
			#define USE_BUMP_MAP 0
			#define USE_SELF_ILLUMIN 0
			#define USE_SELF_ILLUMIN_MAIN_TEX_ALPHA 0
			#define USE_SELF_ILLUMIN_TEX 0
			#define USE_GRASS_VA 0
			#define USE_ALPHA_CUTOFF 0
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1

			#define UNITY_PASS_SHADOWCASTER
			
			#include "Fitment.cginc"
			ENDCG
		}
	}

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
			"IgnoreProjector" = "False"
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

			// Dither Transparency / 使用Transparency效果 (2选1)(程序控制)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 1
			#define USE_BUMP_MAP 0
			#define USE_SELF_ILLUMIN 0
			#define USE_SELF_ILLUMIN_MAIN_TEX_ALPHA 0
			#define USE_SELF_ILLUMIN_TEX 0
			#define USE_GRASS_VA 0
			#define USE_ALPHA_CUTOFF 0
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1

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

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY


			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_MAIN_COLOR 1
			#define USE_BUMP_MAP 0
			#define USE_SELF_ILLUMIN 0
			#define USE_SELF_ILLUMIN_MAIN_TEX_ALPHA 0
			#define USE_SELF_ILLUMIN_TEX 0
			#define USE_GRASS_VA 0
			#define USE_ALPHA_CUTOFF 0
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1

			#define UNITY_PASS_SHADOWCASTER
			
			#include "Fitment.cginc"
			ENDCG
		}
	}
	CustomEditor "FitmentShaderGUI"
}
