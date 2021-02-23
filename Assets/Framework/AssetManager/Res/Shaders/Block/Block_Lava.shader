
Shader "Block/Block_Lava"
{
	Properties
	{ 
		[NoScaleOffset]_MainTex("主贴图(RGB)", 2D) = "white" {}										// 熔岩贴图
		_Color("主贴图叠加颜色(RGB)", Color) = (1,1,1,1)												// 主贴图颜色
		_Tiling("主贴图平铺", Float) = 1				
		[NoScaleOffset]_FlowTex("流光贴图", 2D) = "black" {}
		_FlowColor("流光贴图叠加颜色(RGB)", Color) = (1,1,1,1)
		_FlowSpeed("流光速度", Float) = 0
		_EdgeColor("边颜色(RGB)", Color) = (1,1,1,1)

		// 抖动透明(可程序控制)
		[Header(Dither Transparency)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0
	}

	SubShader
	{
		//Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100
		//ColorMask RGB

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			// 因贴图采用不透明，所以不用合用透明特性
			//Blend SrcAlpha OneMinusSrcAlpha
			//ZTest LEqual
			//ZWrite On
			Cull Off

			CGPROGRAM
			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			// -------------- 自定义变体 --------------
			// 程序需要控制的使用multi_compile，shader面板设置使用 shader_feature

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY


			// ----------------- 开关定义 -----------------

			// dither 透明
			#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
			#include "../Lib/DitherTransparency.cginc"
			#include "../Lib/FastDepthMap.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;			// xy:主贴图uv1(x方向)  zw:流光uv
				float3 normalDir : TEXCOORD1;	// 世界法线
				float4 main_uv : TEXCOORD2;		// xy:主贴图uv2(y方向) zw:主贴图uv3 (z方向)
				UNITY_FOG_COORDS(3)
				GSTORE_DITHER_TRANSPARENCY_COORDS(4)
				fixed depth : TEXCOORD5;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// 贴图
			sampler2D _MainTex;
			sampler2D _FlowTex;

			// 颜色
			fixed4 _Color;
			fixed4 _FlowColor;
			fixed4 _EdgeColor;

			// 其它参数
			half _Tiling;
			float _FlowSpeed;


			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
		
				o.pos = UnityObjectToClipPos(v.vertex);
				// 主uv
				o.uv.xy = v.uv.xy;
				// 流光uv
				o.uv.zw = v.uv.xy + _Time.y * _FlowSpeed;
				// 世界normal
				o.normalDir = UnityObjectToWorldNormal(v.normal);

				// 计算世界空间uv
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

				half2 uv_x = (half2(worldPos.y, worldPos.z) * _Tiling);
				half2 uv_y = (half2(worldPos.x, worldPos.z) * _Tiling);
				half2 uv_z = (half2(worldPos.x, worldPos.y) * _Tiling);
				o.uv.xy = uv_x;
				o.main_uv.xy = uv_y;
				o.main_uv.zw = uv_z;
				// 计算深度
				GSTORE_TRANSFER_DEPTH(o.depth, v.vertex);

				GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);

				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				// 半透明实现
				GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);

				// 法线
				i.normalDir = normalize(i.normalDir);

				// 水深计算
				half depth_blend = 1.0;
				// 边颜色计算(透明->不透明(边颜色)->透明)
				half edge_blend = (1.0 - (distance(depth_blend, 0.5) * 2.0));
				fixed3 edge_base_color = _EdgeColor.rgb * edge_blend;

				// 流光贴图颜色
				fixed3 flow_base_color = tex2D(_FlowTex, i.uv.zw);
				// 叠加流光基础颜色
				flow_base_color.rgb *= _FlowColor.rgb;
				
				// 主贴图世界空间贴图mapping
				half3 world_dir_mask = abs(i.normalDir);
				// 主贴图颜色
				fixed3 base_color = (tex2D(_MainTex, i.uv.xy) * world_dir_mask.x) + (tex2D(_MainTex, i.main_uv.xy) * world_dir_mask.y) + (tex2D(_MainTex, i.main_uv.zw) * world_dir_mask.z);
				// 主贴图去色，形成流光遮罩
				half flow_blend = pow(dot(base_color, float3(0.3, 0.59, 0.11)), 3.0);
				// 叠加主基础颜色
				base_color.rgb *= _Color.rgb;
				// 流光应用遮罩
				flow_base_color.rgb *= flow_blend;
				float alpha = depth_blend + edge_blend;
				GSTORE_CALC_DEPTH_TO_ALPHA(alpha, i.depth, alpha);
				fixed4 final_color = fixed4(base_color.rgb + flow_base_color.rgb + edge_base_color.rgb, alpha);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, final_color);
				return final_color;
			}
			ENDCG
		}
	}
	Fallback "Legacy Shaders/Diffuse"
}
