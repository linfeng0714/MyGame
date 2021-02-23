// 方块通用
Shader "Block/Block_Cube"
{
	Properties
	{
		[NoScaleOffset]_MainTex("Texture", 2DArray) = "" {}
		[NoScaleOffset]_MainTexAtlas ("Atlas Texture", 2D) = "" {}
		[NoScaleOffset]_BumpMap("Normalmap", 2D) = "bump" {}
		//_BumpScale ("Bump Scale", Float) = 1.0
		//_BumpIntensity("Bump Intensity", Float) = 1.0
		[NoScaleOffset]_BlendTex("Blend Texture", 2DArray) = "" {}
		[NoScaleOffset]_BlockBlendMaskTex("Block Blend Mask Texture", 2D) = "white" {}

		//[NoScaleOffset]_NormalTex("Normal Texture", 2DArray) = "" {}

		// AO 效果
		[Header(AO Effect)]
		_AO_Blend ("AO Blend", Range(0, 10)) = 1.0
		_AO_Color ("AO Color", Color) = (1,1,1,1)
		_AO_Size ("AO Size", Range(0,1)) = 1.0

		// 透明通道裁剪(只在面板设置用)
		[Header(Alpha cutoff)]
		[Toggle(USE_ALPHA_CUTOFF)] _UseAlphaCutoff("Use Alpha Off", Float) = 0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5

		// 普通贴图透明(只在面板设置用)
		[Header(Texture Transparency)]
		[Toggle(USE_TEX_TRANSPARENCY)] _UseTexTransparency("Use Texture Transparency", Float) = 0

		[Space(100)]
		[Header(xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)]
		// ------------ 下面为程序使用 ------------

		// Alpha控制透明(可程序控制)
		[Header(Transparency Art Dont Use)]
		[Toggle(USE_TRANSPARENCY)] _UseTransparency("Use Transparency", Float) = 0
		_Transparency("Transparency", Range(0, 1)) = 1

		// 抖动透明(可程序控制)
		[Header(Dither Transparency)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

		// Blending state 内部使用
		[Header(Internal Blending State)]
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Source", Int) = 1
		[MaterialEnum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dest", Int) = 0
		[MaterialEnum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1

		// 关闭深度写入alpha
		[MaterialEnum(Off,0,On,1)] _NoDepthWrite("No Depth Write", Int) = 0
	}

	// 可选屏蔽效果项:
	// DISABLE_BLOCK_BUMP
	// DISABLE_BLOCK_BLEND
	// DISABLE_BLOCK_AO
	// DISABLE_MC_LIGHT
			
	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.5 LOD=400
	// 使用贴图数组，全功能开放
	SubShader
	{
		Tags
		{
			"ObjectType" = "Cube"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		LOD 400

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

			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪 / 贴图透明 (界面使用)
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			
			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1 
			#define CALC_DEPTH 1 

			// 屏蔽效果
			#define DISABLE_BLOCK_BLEND

			// SH光照
			#define UNITY_PASS_FORWARDBASE  
			#include "Cube.cginc"
			
			ENDCG
		}
		
		//Pass
		//{
		//	Name "FORWARD_DELTA"
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ZWrite Off
		//	ZTest LEqual
		//	Blend [_SrcBlend] One
		//	Fog{ Color(0,0,0,0) } // in additive pass fog should be black

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	#pragma skip_variants LIGHTPROBE_SH
		//	#define LIGHTPROBE_SH 1
		//	// Add Pass一些用不到的光照
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 3.5
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog
		//	
		//	// -------------- 自定义变体 --------------
		//	// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
		//	// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

		//	// 使用透明通道裁剪 / 贴图透明 (界面使用)
		//	//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
		//	#pragma shader_feature __ USE_ALPHA_CUTOFF

		//	// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
		//	//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY

		//	// 优化(减少变体数量，把计算小的变体默认就加上)：
		//	#define USE_TEX_TRANSPARENCY 1
		//	#define USE_TRANSPARENCY 1

		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD

		//	#include "Cube.cginc"

		//	ENDCG
		//}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 屏蔽效果
			#define DISABLE_BLOCK_BLEND

			#define UNITY_PASS_SHADOWCASTER
			#include "Cube.cginc"
			ENDCG

		}
	}
	
	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.5 LOD=200
	// 使用贴图数组，屏蔽 Bump Blend
	SubShader
	{
		Tags
		{
			"ObjectType" = "Cube"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
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

			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪 / 贴图透明 (界面使用)
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1 
			#define CALC_DEPTH 1 

			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND

			// SH光照
			#define UNITY_PASS_FORWARDBASE
			#include "Cube.cginc"

			ENDCG
		}

		//Pass
		//{
		//	Name "FORWARD_DELTA"
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ZWrite Off
		//	ZTest LEqual
		//	Blend[_SrcBlend] One
		//	Fog{ Color(0,0,0,0) } // in additive pass fog should be black

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	#pragma skip_variants LIGHTPROBE_SH
		//	#define LIGHTPROBE_SH 1
		//	// Add Pass一些用不到的光照
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 3.5
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog

		//	// -------------- 自定义变体 --------------
		//	// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
		//	// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

		//	// 使用透明通道裁剪 / 贴图透明 (界面使用)
		//	//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
		//	#pragma shader_feature __ USE_ALPHA_CUTOFF

		//	// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
		//	//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY

		//	// 优化(减少变体数量，把计算小的变体默认就加上)：
		//	#define USE_TEX_TRANSPARENCY 1
		//	#define USE_TRANSPARENCY 1
		//	

		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD

		//	// 屏蔽效果
		//	#define DISABLE_BLOCK_BUMP
		//	#define DISABLE_BLOCK_BLEND

		//	#include "Cube.cginc"

		//	ENDCG
		//}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND

			#define UNITY_PASS_SHADOWCASTER
			#include "Cube.cginc"
			ENDCG

		}
	}

	
	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.5 LOD=50
	// 强制使用图集，屏蔽 Bump Blend
	SubShader
	{
		Tags
		{
			"ObjectType" = "Cube"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		LOD 50
		// 屏蔽 Bump Blend 强制使用图集

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

			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪 / 贴图透明 (界面使用)
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1 
			#define CALC_DEPTH 1 

			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND
			#define USE_FORCE_TEX_ATLAS 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE
			#include "Cube.cginc"

			ENDCG
		}

		//Pass
		//{
		//	Name "FORWARD_DELTA"
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ZWrite Off
		//	ZTest LEqual
		//	Blend[_SrcBlend] One
		//	Fog{ Color(0,0,0,0) } // in additive pass fog should be black

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	#pragma skip_variants LIGHTPROBE_SH
		//	#define LIGHTPROBE_SH 1
		//	// Add Pass一些用不到的光照
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 3.5
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog

		//	// -------------- 自定义变体 --------------
		//	// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
		//	// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

		//	// 使用透明通道裁剪 / 贴图透明 (界面使用)
		//	//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
		//	#pragma shader_feature __ USE_ALPHA_CUTOFF

		//	// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
		//	//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY

		//	// 优化(减少变体数量，把计算小的变体默认就加上)：
		//	#define USE_TEX_TRANSPARENCY 1
		//	#define USE_TRANSPARENCY 1

		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD

		//	// 屏蔽效果
		//	#define DISABLE_BLOCK_BUMP
		//	#define DISABLE_BLOCK_BLEND
		//	#define USE_FORCE_TEX_ATLAS 1

		//	#include "Cube.cginc"

		//	ENDCG
		//}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND
			#define USE_FORCE_TEX_ATLAS 1

			#define UNITY_PASS_SHADOWCASTER
			#include "Cube.cginc"
			ENDCG

		}
	}
	
	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=2.0 LOD=400
	// 强制使用图集，全功能开放
	SubShader
	{
		Tags
		{
			"ObjectType" = "Cube"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		LOD 400

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

			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪 / 贴图透明 (界面使用)
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			
			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1 

			// 屏蔽效果
			#define DISABLE_BLOCK_BLEND 
			#define USE_FORCE_TEX_ATLAS 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE
			#include "Cube.cginc"
			
			ENDCG
		}
		
		//Pass
		//{
		//	Name "FORWARD_DELTA"
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ZWrite Off
		//	ZTest LEqual
		//	Blend[_SrcBlend] One
		//	Fog{ Color(0,0,0,0) } // in additive pass fog should be black

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	#pragma skip_variants LIGHTPROBE_SH
		//	#define LIGHTPROBE_SH 1
		//	// Add Pass一些用不到的光照
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 2.0
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog
		//	
		//	// -------------- 自定义变体 --------------
		//	// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
		//	// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

		//	// 使用透明通道裁剪 / 贴图透明 (界面使用)
		//	//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
		//	#pragma shader_feature __ USE_ALPHA_CUTOFF

		//	// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
		//	//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY

		//	// 优化(减少变体数量，把计算小的变体默认就加上)：
		//	#define USE_TEX_TRANSPARENCY 1
		//	#define USE_TRANSPARENCY 1

		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD

		//	#include "Cube.cginc"

		//	ENDCG
		//}

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
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 屏蔽效果
			#define DISABLE_BLOCK_BLEND 
			#define USE_FORCE_TEX_ATLAS 1

			#define UNITY_PASS_SHADOWCASTER
			#include "Cube.cginc"
			ENDCG

		}
	}
	
	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=2.0 LOD=50
	// 强制使用图集，屏蔽 Bump Blend
	SubShader
	{
		Tags
		{
			"ObjectType" = "Cube"			// 给程序里识别使用的类型(类型请看ShaderDefine)
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		LOD 50

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

			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			// Apparently need to add this declaration 
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
			// make fog work
			#pragma multi_compile_fog


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪 / 贴图透明 (界面使用)
			//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
			//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY
			
			// 优化(减少变体数量，把计算小的变体默认就加上)：
			#define USE_TEX_TRANSPARENCY 1
			#define USE_TRANSPARENCY 1
			#define CALC_DEPTH 1 
			
			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND
			#define USE_FORCE_TEX_ATLAS 1

			// SH光照
			#define UNITY_PASS_FORWARDBASE
			#include "Cube.cginc"

			ENDCG
		}

		//Pass
		//{
		//	Name "FORWARD_DELTA"
		//	Tags{ "LightMode" = "ForwardAdd" }
		//	ZWrite Off
		//	ZTest LEqual
		//	Blend[_SrcBlend] One
		//	Fog{ Color(0,0,0,0) } // in additive pass fog should be black

		//	CGPROGRAM
		//	// 排除一些不用的Unity内置变量
		//	#pragma skip_variants FOG_EXP FOG_EXP2
		//	// Add Pass一些用不到的光照
		//	#pragma skip_variants DIRECTIONAL DIRECTIONAL_COOKIE POINT_COOKIE SPOT

		//	#pragma target 2.0
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	// Apparently need to add this declaration 
		//	#pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 
		//	// make fog work
		//	#pragma multi_compile_fog

		//	// -------------- 自定义变体 --------------
		//	// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
		//	// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

		//	// 使用透明通道裁剪 / 贴图透明 (界面使用)
		//	//#pragma shader_feature __ USE_ALPHA_CUTOFF USE_TEX_TRANSPARENCY
		//	#pragma shader_feature __ USE_ALPHA_CUTOFF

		//	// Dither Transparency / 使用Transparency效果 (2选1) (程序使用)
		//	//#pragma multi_compile __ USE_DITHER_TRANSPARENCY USE_TRANSPARENCY
		//	#pragma multi_compile __ USE_DITHER_TRANSPARENCY

		//	// 优化(减少变体数量，把计算小的变体默认就加上)：
		//	#define USE_TEX_TRANSPARENCY 1
		//	#define USE_TRANSPARENCY 1
		//	

		//	// SH光照
		//	#define UNITY_PASS_FORWARDADD

		//	// 屏蔽效果
		//	#define DISABLE_BLOCK_BUMP
		//	#define DISABLE_BLOCK_BLEND

		//	#include "Cube.cginc"

		//	ENDCG
		//}

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
			#pragma multi_compile_shadowcaster nolightmap nodirlightmap nodynlightmap noshadowmask nolppv 


			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature
			// 注意：不要使用超过4个变体，变体多会影响内存和Build时间

			// 使用透明通道裁剪
			#pragma shader_feature __ USE_ALPHA_CUTOFF

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 屏蔽效果
			#define DISABLE_BLOCK_BUMP
			#define DISABLE_BLOCK_BLEND
			#define USE_FORCE_TEX_ATLAS 1

			#define UNITY_PASS_SHADOWCASTER
			#include "Cube.cginc"
			ENDCG

		}
	}

	CustomEditor "CubeShaderGUI"
}
