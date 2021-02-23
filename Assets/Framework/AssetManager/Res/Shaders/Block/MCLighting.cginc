#ifndef __MCLighting__
#define __MCLighting__

//#define	MIN_LUMINANCE 0.2	// �����������(skylightΪ0ʱ��skylightTex�е�λ��)
//#define AMBIENT_LUMIN_MIN 0
//#define AMBIENT_LUMIN_MAX 1

#include "UnityCG.cginc"

//half ColorLuminosity(fixed3 col)
//{
//	return 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
//}
//
//fixed3 WeightedAmbient(fixed3 ambient_col, fixed3 col)
//{
//	fixed3 weighted_ambient_col = ambient_col;
//
//	half lumin = Luminance(col);
//	lumin = clamp(lumin, AMBIENT_LUMIN_MIN, AMBIENT_LUMIN_MAX);
//	half ambient_factor = (lumin - AMBIENT_LUMIN_MIN) / (AMBIENT_LUMIN_MAX - AMBIENT_LUMIN_MIN);
//	weighted_ambient_col = lerp(weighted_ambient_col, 0, ambient_factor);
//
//	return weighted_ambient_col;
//}
//
//// ������ռ���, ��ǰ���ռ�����������ͻ�ѹ�(û����ͼ��ɫ)
//fixed3 CombineMCLighting(fixed3 col, half skyLightVal, half blockLightVal, sampler2D skyLightTex, sampler2D blockLightTex, half blockLightIntensity)
//{
//	// skyLightVal 0 - 1
//	// blockLightVal 0 - 1
//	UNITY_BRANCH
//	if (skyLightVal > 0.99 && blockLightVal < 0.01)
//	{
//		return col;
//	}
//	
//	// MC����
//	fixed3 mcLightCol = 0;
//
//	fixed3 skyLightCol = 1;
//	UNITY_BRANCH
//	if (skyLightVal < 0.99)
//	{
//		skyLightCol = tex2Dlod(skyLightTex, float4(0.5, 1 - skyLightVal, 0, 0)).rgb;
//	}
//	
//	skyLightCol *= col;
//	half lumin = Luminance(skyLightCol);
//	half adjusted_lumin = clamp(lumin, MIN_LUMINANCE, 10);
//	
//	// ��ֹ����
//	//lumin = max(0.01, lumin);
//	// brigtness����ֱ�ӳ���һ��ϵ����Ҳ����RGB�������ţ���������
//	skyLightCol *= adjusted_lumin / lumin;
//	UNITY_BRANCH
//	if (blockLightVal < 0.01)
//	{
//		return skyLightCol;
//	}
//
//	fixed3 blockLightCol = tex2Dlod(blockLightTex, float4(0.5, 1 - blockLightVal, 0, 0)).rgb;
//
//	half skyLightLuminosity = Luminance(skyLightCol);
//	half blockLightLuminosity = Luminance(blockLightCol);
//
//	half skyLightWeight = skyLightLuminosity / (skyLightLuminosity + blockLightLuminosity);
//	UNITY_BRANCH
//	if (skyLightWeight < 0.5)
//	{
//		skyLightWeight = 8 * skyLightWeight * skyLightWeight * skyLightWeight * skyLightWeight;
//	}
//	else
//	{
//		half k = skyLightWeight - 1;
//		skyLightWeight = -8 * k * k * k * k + 1;
//	}
//
//	half extraBlockLight = (0.5 + 0.5 * skyLightWeight) * blockLightIntensity - 1;	// ����ʹ��һ����ʽ������ѹ���ǿ��(��Ѩ��Ļ������ĸ���)
//
//	mcLightCol.rgb = skyLightWeight * skyLightCol + (1 - skyLightWeight) * blockLightCol * (1 + extraBlockLight);	
//
//	return mcLightCol;
//}
//
//fixed3 CombineMCLightingFitment(fixed3 col, float skyLightVal, float blockLightVal, sampler2D skyLightTex, sampler2D blockLightTex, float blockLightIntensity)
//{
//	return CombineMCLighting(col, skyLightVal, blockLightVal, skyLightTex, blockLightTex, 2.5);
//}

// ===============================================================================================================

// ����ͼ���������� - ��ѹ�
static const fixed3 BLOCK_LIGHT[16] = {
	{ 0.04313726, 0.01568628, 0.003921569 },
	{ 0.06300648, 0.03529412, 0.01176471 },
	{ 0.08235294, 0.05882353, 0.01960784 },
	{ 0.1019608, 0.08235294, 0.02745098 },
	{ 0.1343791, 0.1019608, 0.03921569 },
	{ 0.1895425, 0.1333333, 0.05490196 },
	{ 0.2603921, 0.1898039, 0.08784312 },
	{ 0.3722875, 0.2703268, 0.1176471 },
	{ 0.5139869, 0.370719, 0.1589542 },
	{ 0.6831372, 0.4870588, 0.210196 },
	{ 0.8169935, 0.5895425, 0.2627451 },
	{ 0.9126797, 0.6802614, 0.3166013 },
	{ 0.9764706, 0.7788235, 0.3788235 },
	{ 0.9994771, 0.8661438, 0.4739869 },
	{ 1, 0.9213072, 0.5647059 },
	{ 1, 0.972549, 0.627451 }
};

// ����ͼ���������� - ���
static const fixed3 SKY_LIGHT[16] = {
	{ 0.3333333, 0.3333333, 0.3333333 },
	{ 0.3529412, 0.3529412, 0.3529412 },
	{ 0.3686275, 0.3686275, 0.3686275 },
	{ 0.3882353, 0.3882353, 0.3882353 },
	{ 0.4078431, 0.4078431, 0.4078431 },
	{ 0.4235294, 0.4235294, 0.4235294 },
	{ 0.4462745, 0.4462745, 0.4462745 },
	{ 0.4784314, 0.4784314, 0.4784314 },
	{ 0.5589542, 0.5589542, 0.5589542 },
	{ 0.6494118, 0.6494118, 0.6494118 },
	{ 0.7424837, 0.7424837, 0.7424837 },
	{ 0.8175163, 0.8175163, 0.8175163 },
	{ 0.8854902, 0.8854902, 0.8854902 },
	{ 0.9440523, 0.9440523, 0.9440523 },
	{ 0.9921569, 0.9921569, 0.9921569 },
	{ 1, 1, 1 }
};


// ������ռ���, ��ǰ���ռ�����������ͻ�ѹ�(û����ͼ��ɫ)
inline fixed3 New_CombineMCLighting(fixed3 diffuse, fixed atten, fixed skyLightVal, fixed blockLightVal, sampler2D skyLightTex, sampler2D blockLightTex, half blockLightIntensity)
{
	// skyLightVal 0 - 1
	// blockLightVal 0 - 1

	// ��ͼ�����е����ģ���Ҫ��Ϊconst buffer
	//fixed3 skyLightCol = tex2Dlod(skyLightTex, fixed4(0.5, 1 - skyLightVal, 0, 0)).rgb;
	//fixed3 blockLightCol = tex2Dlod(blockLightTex, fixed4(0.5, 1 - blockLightVal, 0, 0)).rgb;
	// ����汾
	half2 sky_n_block_index = half2(skyLightVal, blockLightVal) * 15;
	half2 sky_n_block_index_up = ceil(sky_n_block_index);
	half2 sky_n_block_index_down = floor(sky_n_block_index);
	fixed3 skyLightCol = lerp(SKY_LIGHT[sky_n_block_index_down.x], SKY_LIGHT[sky_n_block_index_up.x], frac(sky_n_block_index.x));
	fixed3 blockLightCol = lerp(BLOCK_LIGHT[sky_n_block_index_down.y], BLOCK_LIGHT[sky_n_block_index_up.y], frac(sky_n_block_index.y));


	// ����������
	diffuse = skyLightCol * diffuse;
	// ��õ�ǰ�׶�����
	fixed lumin = saturate(Luminance(diffuse));
	// �ϲ���Ӱ�ͻ�ѹ�
	// ��ѹ�����ȵ͵ĵط�����
	return (diffuse * atten) + ((1 - lumin) * blockLightCol * blockLightIntensity);
}

// �������
inline fixed3 CombineMCLighting_Cube(fixed3 diffuse, fixed atten, fixed skyLightVal, fixed blockLightVal, sampler2D skyLightTex, sampler2D blockLightTex)
{
	return New_CombineMCLighting(diffuse, atten, skyLightVal, blockLightVal, skyLightTex, blockLightTex, 2);
}

// �Ҿߵ���
inline fixed3 CombineMCLighting_Fitment(fixed3 diffuse, fixed atten, fixed skyLightVal, fixed blockLightVal, sampler2D skyLightTex, sampler2D blockLightTex)
{
	return New_CombineMCLighting(diffuse, atten, skyLightVal, blockLightVal, skyLightTex, blockLightTex, 2);
}

#endif
