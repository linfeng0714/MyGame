Shader "Role/RampRim"
{
    Properties
    {
        _MainTex("主贴图", 2D) = "white" {}    // These properties must have the same names as in standard Unity 
      	_NormalTex ("法线贴图", 2D) = "bump" {}
        
		_RampTex ("明暗过渡图", 2D) = "gray" {}
        _SColor ("暗面颜色", Color) = (0.0,0.0,0.0,1)
		_LColor ("亮面颜色", Color) = (0.5,0.5,0.5,1)
		
        _SpecCol("高光颜色", Color) = (0.0, 0.0, 0.0, 1.0)
        _SpecPower("高光锐度", Range(0.5, 128.0)) = 3.0
		_SpecTex("高光贴图", 2D) = "white" {}
		
        _RimColor("边缘光颜色", Color) = (0.26,0.19,0.16,0.0)
        _RimPower("边缘光锐度", Range(0.5, 20.0)) = 3.0
        
        _DiffIntensity ("基本明暗强度", Range(0, 5.0)) = 1.0
        _SpecIntensity ("高光强度", Range(0, 5.0)) = 1.0
        _RimIntensity ("边缘光强度", Range(0, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }    // This allows Unity to intelligently substitute this shader when needed.
        LOD 200
 
        CGPROGRAM
        #include "UnityCG.cginc"
        #pragma surface surf NPR

        #define MIN_AMBIENT_LUMIN 0.5

        float ColorLuminosity(fixed3 col)
		{
			float luminosity = 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
			
			return luminosity;
		}
 
        // These match the shader properties
        sampler2D _MainTex;
        sampler2D _NormalTex;
        
        sampler2D _RampTex;
		float4 _LColor;
		float4 _SColor;
        
        sampler2D _SpecTex;
        float4 _SpecCol;
        float _SpecPower;
        
        float4 _RimColor;
        float _RimPower;
        
        float _DiffIntensity;
        float _SpecIntensity;
        float _RimIntensity;
        
        // version3 (viewdirAdjust & inverseLight blend)
        half4 LightingNPR(SurfaceOutput o, half3 lightdir, half3 viewdir, half atten)
        {
            float lambert = max(0, dot(o.Normal, lightdir));
			//lambert = (lambert + 0.5) * 0.5;
			
			half3 ramp = tex2D(_RampTex, float2(lambert, lambert)).rgb;
			ramp = lerp(_SColor,_LColor,ramp);
			half4 diffuse = half4(ramp * _LightColor0.rgb * o.Albedo.rgb, 1.0);
            diffuse *= _DiffIntensity;
 
            // Rim lighting
            half3 vd = viewdir;
            
          	float rim_term = 1.0 - saturate(abs(dot(normalize(vd), o.Normal)));
            rim_term = pow(rim_term, _RimPower);
            half4 rim = half4(_RimColor.rgb * rim_term, 1.0); 
            rim *= _RimIntensity;  
            rim.rgb *= _LightColor0.rgb * o.Albedo.rgb;
 
            // Phong's specular term
            half3 r = reflect(-lightdir, o.Normal);
            float phong = pow(saturate(dot(r, viewdir)), _SpecPower);
            half4 specular = half4(phong * _SpecCol);
            specular *= o.Gloss * _SpecIntensity;
 
            return diffuse + rim + specular;
        }
 
        struct Input
        {
            float2 uv_MainTex;
        	float2 uv_NormalTex;
        };
 
        void surf(Input IN, inout SurfaceOutput o)
        {
            // This is where we prepare the surcace for lighting by propagating a SurfaceOutput structure
            half4 c = tex2D(_MainTex, IN.uv_MainTex);    // Sample the texture
            o.Albedo = c;                    // Modulate by main colour
            o.Alpha = 1.0;                                // No alpha in this shader
            o.Gloss = tex2D(_SpecTex, IN.uv_MainTex).r;
        	o.Normal = UnpackNormal (tex2D (_NormalTex, IN.uv_NormalTex));

        	// 根据当前环境光, 决定角色自发光亮度
        	half4 ambientCol = UNITY_LIGHTMODEL_AMBIENT;
        	float ambientLumin = ColorLuminosity(ambientCol.rgb);

        	float extraLumin = saturate(MIN_AMBIENT_LUMIN - ambientLumin);
        	o.Emission = (extraLumin * UNITY_LIGHTMODEL_AMBIENT / ambientLumin) * c;
        }
        ENDCG
    }
    FallBack "Diffuse"    // Shader to use if the user's hardware cannot incorporate this one
}