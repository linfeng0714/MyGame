
Shader "Block/Block_Water"
{
	Properties
	{
		[NoScaleOffset] _ReflectiveColor("(RGB通道控制反射的颜色)(A通道控制菲涅耳)", 2D) = "" {}
		_HorizonColor("水平面颜色(alpha为透明度)", COLOR) = (.172, .463, .435, 1)
		
		[NoScaleOffset]_BumpMap("法线贴图 ", 2D) = "bump" {}

		_BumpTiling("法线平铺大小(1[xy], 2[zw])", Vector) = (1.0 ,1.0, -2.0, 3.0)
		_BumpDirection("法线方向与速度(1[xy], 2[zw])", Vector) = (1.0 ,1.0, -1.0, 1.0)

		_ReflectionDistort("反射 扭曲", Range(0,1.5)) = 0.44
		_ReflectionCubemap("Reflection Cubemap", Cube) = "_Skybox" { }

		[Toggle(USE_CUBEMAP)] _UseCubemap("Use Cubemap", Float) = 0

		// 抖动透明(可程序控制)
		[Header(Dither Transparency)]
		[Toggle(USE_DITHER_TRANSPARENCY)] _UseDitherTransparency("Use DitherTransparency", Float) = 0
		_DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{

			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull [_G_UnderWaterCullMode]
			Offset -1, -1			

			// 写入模版缓存, 用来区分水面
			/*Stencil 
			{
		        Ref 2
		        WriteMask 2
		        Comp Always
		        Pass Replace
			}*/

			Tags{ "LightMode" = "ForwardBase" }

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

			// Dither Transparency
			#pragma multi_compile __ USE_DITHER_TRANSPARENCY

			// 是否使用unity的 sh 光照
			#define USING_UNITY_SH (UNITY_SHOULD_SAMPLE_SH) && !defined(LIGHTMAP_ON)
			
			// 使用cube map
			#pragma shader_feature USE_CUBEMAP

			// 水下效果
			#pragma multi_compile __ UNDER_WATER_ON

			// ----------------- 开关定义 -----------------

			// dither 透明
			#define GSTORE_USEING_DITHER_TRANSPARENCY (USE_DITHER_TRANSPARENCY)
			#include "../Lib/DitherTransparency.cginc"

			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				UNITY_POSITION(pos);
				fixed2 uv : TEXCOORD0;
				float4 bumpCoords : TEXCOORD1;
				GSTORE_DITHER_TRANSPARENCY_COORDS(2)
				float3 worldViewDir : TEXCOORD3;
				fixed3 normal : TEXCOORD4;
				float4 worldPos : TEXCOORD5;
				UNITY_FOG_COORDS(6)
				float3 I : TEXCOORD7;
#if USING_UNITY_SH
				half3 sh : TEXCOORD8;	// SH
#endif
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// 全局水下属性(由C#代码设置)
			uniform float _G_UnderWaterCullMode;


			// 贴图
			sampler2D _BumpMap;
			sampler2D _ReflectiveColor;
			samplerCUBE _ReflectionCubemap;

			// Bump
			float4 _BumpTiling;
			float4 _BumpDirection;

			// 反射
			uniform float _ReflectionDistort;

			// 水平面颜色(alpha为透明度)
			uniform float4 _HorizonColor;

#if UNDER_WATER_ON
			// (bool)是否处于水下
			uniform fixed4 _G_UnderWaterFogColor;
			uniform half _G_UnderWaterFogDensity;

			half ComputeUnderWaterFog(float z)
			{
				half fog = 0.0;

				fog = exp2(-_G_UnderWaterFogDensity * z);

				//fog = _FogDensity * z;
				//fog = exp2(-fog * fog);

				return saturate(fog);
			}
#endif // UNDER_WATER_ON

			// 与FadeToSkybox关联
			fixed _G_IsFadeToSkybox;
			float _G_F2SB_Start;
			float _G_F2SB_End;

			half ComputeFade(float z)
			{
				half fade = 0.0;

				fade = (_G_F2SB_End - z) / max(_G_F2SB_End - _G_F2SB_Start, 1.0e-4);

				return saturate(fade);
			}

			// 对法线贴图进行每像素法线采样，得到世界空间的法线
			inline half3 PerPixelNormal(sampler2D bumpMap, half4 coords)
			{
				half3 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)).rgb + UnpackNormal(tex2D(bumpMap, coords.zw)).rgb) * 0.5;
				return normalize(bump);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				// 世界空间坐标
				float3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldPos.xyz = worldVertexPos;

				// 与镜头的距离
				o.worldPos.w = -(UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w);
				//o.worldPos.w = -(UnityObjectToViewPos(v.vertex).z);
				//o.worldPos.w = length(mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceCameraPos.xyz);

				// Bump的两组uv(分别储存在xy,zw)
				// 使用顶点的世界空间xz定位
				o.bumpCoords.xyzw = (worldVertexPos.xzxz + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

				// 镜头方向(由顶点指向镜头，非法线向量)
				o.worldViewDir.xzy = UnityWorldSpaceViewDir(worldVertexPos);

				// 世界空间法线
				fixed3 normalWorld = UnityObjectToWorldNormal(v.normal);
				o.normal = normalWorld;

				o.I = reflect(-o.worldViewDir.xzy, normalWorld);
				// vertex程序中的光照颜色
				// SH/ambient and vertex lights
#if USING_UNITY_SH
				o.sh = 0;
				// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
				o.sh += Shade4PointLights(
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, o.worldPos.xyz, normalWorld);
#endif
				// 注意：ShadeSH9部分可放到frag里和Bump结合，但效率考虑的话，还是放在vert里，但不会受bump影响
				o.sh += max(half3(0, 0, 0), ShadeSH9(half4(normalWorld, 1.0)));
#endif

				//TRANSFER_SHADOW(o); 			// pass shadow coordinates to pixel shader
				GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, o.pos);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
			
			fixed4 frag (v2f i, float facing : VFACE) : SV_Target
			{
				float faceSign = (facing >= 0 ? 1 : -1);

				UNITY_SETUP_INSTANCE_ID(i);

				
				//return fixed4(normalize(i.normal) *0.5 + 0.5, 1);
				// 顶点法线
				half3 vertexWorldNormal = normalize(i.normal);
				//half3 vertexWorldNormal = normalize(float3(0, 1, 0));
				// 根据法线贴图获得法线，顶点法线固定(0,1,0)
				half3 bumpNormal = PerPixelNormal(_BumpMap, i.bumpCoords);
				// 顶点指向镜头
				half3 viewVector = normalize(i.worldViewDir);
				// fresnel factor 菲涅耳反射因子
				half fresnelFac = dot(viewVector, bumpNormal);

				// 灯光方向
				fixed3 lightDirection = UnityWorldSpaceLightDir(i.worldPos.xyz);

				// 计算衰减 compute lighting & shadowing factor	
				//UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz)

				// -------------- 漫反射部分 ------------------
				// 兰伯
				half3 lambert_term = max(0.0, (0.5 * dot(vertexWorldNormal, lightDirection) + 0.5));
				//half3 lambert_term = max(0.0, (dot(vertexWorldNormal, lightDirection)));


				// 公式意义：使用直射光定义亮部颜色
				half3 direct_diffuse = _LightColor0.rgb * lambert_term;
				half3 indirect_diffuse = half3(0, 0, 0);
#if USING_UNITY_SH
				// 公式意义: 使用环境光定义为暗部颜色
				indirect_diffuse += i.sh /** (1 - lambert_term)*/;
#endif
				half3 diffuse_part = (direct_diffuse + indirect_diffuse);

				// 水颜色
				fixed4 water_col = tex2D(_ReflectiveColor, float2(fresnelFac, fresnelFac));
				fixed4 final_color;
				fixed final_alpha = _HorizonColor.a;

#ifdef USE_CUBEMAP
				i.I += bumpNormal * _ReflectionDistort;
				fixed4 reflcol = texCUBE(_ReflectionCubemap, i.I);
				final_color.rgb = lerp(water_col.rgb, reflcol.rgb, water_col.a);
#else
				final_color.rgb = lerp(water_col.rgb, _HorizonColor.rgb, water_col.a);
#endif
				final_color.rgb *= diffuse_part;

				// 距离
				float dist = i.worldPos.w;

#if UNDER_WATER_ON
				// 水下效果

				fixed fogFactor = 1 - ComputeUnderWaterFog(dist);
				fogFactor *= _G_UnderWaterFogColor.a;

				final_color.rgb = lerp(final_color.rgb, _G_UnderWaterFogColor.rgb, fogFactor);
				final_alpha = lerp(final_alpha, 1, fogFactor);
#endif

				// Fade To Skybox
				final_alpha = final_alpha * (ComputeFade((dist * _ProjectionParams.z) - _ProjectionParams.y) * _G_IsFadeToSkybox + (1 - _G_IsFadeToSkybox));
				final_color.a = final_alpha;
				// 系统Fog
				UNITY_APPLY_FOG(i.fogCoord, final_color);

				// 半透明实现
				GSTORE_DITHER_TRANSPARENCY_CUTOFF(i);

				return final_color;

				
			}
			ENDCG
		}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert_shadowcaster
			#pragma fragment frag_shadowcaster
			#pragma multi_compile_shadowcaster
		
			#define UNITY_PASS_SHADOWCASTER

			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert_shadowcaster(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag_shadowcaster(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}
	Fallback "Legacy Shaders/Transparent/Diffuse"
}
