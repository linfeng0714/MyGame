

Shader "Block/Block_Cube_Alone"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Texture", 2DArray) = "white" {}
		[NoScaleOffset]_MainTexAtlas("Atlas Texture", 2D) = "" {}
		[MaterialEnum(Off,0,On,1)] _NoDepthWrite("No Depth Write", Int) = 0
		_Color("颜色(RGBA)", Color) = (1,1,1,1)
	}

	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=3.5 LOD=100
	// 使用贴图数组
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			ZTest LEqual
			ZWrite On

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
			// SH光照
			#define UNITY_PASS_FORWARDBASE

			// 获取深度信息
			#define CALC_DEPTH 1 

			#include "CubeOpaqueExploder.cginc"
			ENDCG
		}
	}

	// --------------------------------------------------------------------------------------------------------------------------------------------
	// target=2.0 LOD=50
	// 强制使用图集
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
		LOD 50

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			ZTest LEqual
			ZWrite On

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
			// SH光照
			#define UNITY_PASS_FORWARDBASE

			// 强制使用图集
			#define USE_FORCE_TEX_ATLAS 1

			// 获取深度信息
			#define CALC_DEPTH 1 

			#include "CubeOpaqueExploder.cginc"
			ENDCG
		}
	}

	// 利用ShadowCaster pass
	// 产生的影子如果需要与物体的Alpha相关联，必需使用Shader Model 3.0
	FallBack "VertexLit"
}
