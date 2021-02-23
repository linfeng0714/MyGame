#ifndef __ILLUMINATION_FORMULA__
#define __ILLUMINATION_FORMULA__

// 全局变量
uniform fixed _DisableMainCamera;

// 计算环境光 
// 在游戏运行时，UI中的人物或地型是不受环境光的影响
inline half3 CaleCameraSH(in half3 worldNormal)
{
	return (1 - _DisableMainCamera) * max(half3(0, 0, 0), ShadeSH9(half4(worldNormal, 1.0))) + _DisableMainCamera * half3(1, 1, 1);
}

#endif
