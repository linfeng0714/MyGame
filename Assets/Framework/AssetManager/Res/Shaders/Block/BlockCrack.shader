// Upgrade NOTE: upgraded instancing buffer 'Properties' to new syntax.

Shader "Block/BlockCrack"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CubeWidth("CubeWidth", Float) = 1
		_BaseAlpha("BaseAlpha", Range(0,1)) = 0.3

		_Transparency("Transparency", Range(0, 1)) = 1
		_Progress("Progress",Range(0,1)) = 1
		_BlockPos("BlockPos", Vector) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		ZWrite Off
		Offset -2,-2

		Pass
		{
			CGPROGRAM
			// 排除一些不用的Unity内置变量
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants LIGHTPROBE_SH
			#define LIGHTPROBE_SH 1

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			// Gpu Instancing
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			#define SIN_90 1
			#define CON_90 0

			#define SIN_180 0
			#define CON_180 -1

			#define SIN_270 -1
			#define CON_270 0

			#define SIN_360 0
			#define CON_360 1

			#define SIN_45 0.7071
			#define CON_45 0.7071

			#define SIN_135 0.7071
			#define CON_135 -0.7071

			#define SIN_225 -0.7071
			#define CON_225 -0.7071

			#define SIN_315 -0.7071
			#define CON_315 0.7071

			static const fixed ONE_DIV_SIN_45 = 1 / SIN_45;

			// uv旋转常量
			static const float2 ROTATE_UV_X[8] = 
			{
				float2(CON_90, -SIN_90),
				float2(CON_180, -SIN_180),
				float2(CON_270, -SIN_270),
				float2(CON_360, -SIN_360),
				float2(CON_45, -SIN_45),
				float2(CON_135, -SIN_135),
				float2(CON_225, -SIN_225),
				float2(CON_315, -SIN_315),
			};

			static const float2 ROTATE_UV_Y[8] =
			{
				float2(SIN_90, CON_90),
				float2(SIN_180, CON_180),
				float2(SIN_270, CON_270),
				float2(SIN_360, CON_360),
				float2(SIN_45, CON_45),
				float2(SIN_135, CON_135),
				float2(SIN_225, CON_225),
				float2(SIN_315, CON_315),
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				UNITY_POSITION(pos);
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform half _CubeWidth;
			uniform fixed _BaseAlpha;
			//uniform fixed _Progress;
			//uniform fixed _Transparency;
			//uniform float4 _BlockPos;

			UNITY_INSTANCING_BUFFER_START(Properties)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _Progress)
				UNITY_DEFINE_INSTANCED_PROP(fixed, _Transparency)
				UNITY_DEFINE_INSTANCED_PROP(float4, _BlockPos)
			UNITY_INSTANCING_BUFFER_END(Properties)
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// 旋转
				float4 block_pos = UNITY_ACCESS_INSTANCED_PROP(Properties, _BlockPos);
				float index = floor(fmod(abs(block_pos.x + block_pos.y + block_pos.z), 8));
				float2 temp_uv = o.uv;
				o.uv.x = dot(temp_uv.xy - 0.5, ROTATE_UV_X[index].xy) + 0.5;
				o.uv.y = dot(temp_uv.xy - 0.5, ROTATE_UV_Y[index].xy) + 0.5;

				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				//距离中心点位置
				fixed dist = distance(i.uv, fixed2(0.5, 0.5));

				
				// 进度
				fixed progress = UNITY_ACCESS_INSTANCED_PROP(Properties, _Progress);
				fixed width = _CubeWidth * 0.5;
				// 三角函数计算斜边
				fixed farthest_dist = lerp(width, width * ONE_DIV_SIN_45, progress);
				// 分界
				fixed sep_dist = (farthest_dist * progress);
				// 分成两部分
				//fixed near_dist = sep_dist * 0.5;
				//fixed far_dist = sep_dist;
				fixed near_dist = sep_dist;
				fixed far_dist = farthest_dist;
	
				// dist < sep_dist == 1
				//fixed _part_near = step(dist, near_dist);
				//fixed _part_far = (1.0 - _part_near) * (1.0 - saturate((dist - near_dist) / (far_dist - near_dist)));
				//fixed dist_alpha = _part_near + _part_far;
				
				fixed dist_alpha = 1.0 - saturate((dist - near_dist) / (far_dist - near_dist));

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a *= dist_alpha * lerp(_BaseAlpha, 1, progress);
				col.a *= UNITY_ACCESS_INSTANCED_PROP(Properties, _Transparency);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
