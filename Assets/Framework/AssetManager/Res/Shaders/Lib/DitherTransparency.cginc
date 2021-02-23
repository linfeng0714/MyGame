#ifndef __DitherTransparency__
#define __DitherTransparency__


// Dither Transparency库
// 使用方法:
// 1.使用前在shader文件中需要定义GSTORE_USEING_DITHER_TRANSPARENCY的值
// 2.在定义GSTORE_USEING_DITHER_TRANSPARENCY后，再引用这个库文件

// 例子：
// #define GSTORE_USEING_DITHER_TRANSPARENCY 1
// #include "../Lib/DitherTransparency.cginc"

// [shader面板设置变量]
// _DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

// 方式1:
// 在V2F的结构里定义屏幕变量:
// GSTORE_DITHER_TRANSPARENCY_COORDS
// 在Vertex Shader中调用获得屏幕坐标:
// GSTORE_TRANSFER_DITHER_TRANSPARENCY
// 在Fragment Shader中进行像素裁剪(已同时使用GSTORE_DITHER_TRANSPARENCY_COORDS 和 GSTORE_TRANSFER_DITHER_TRANSPARENCY):
// GSTORE_DITHER_TRANSPARENCY_CUTOFF

// 方式2:
// 已获得屏幕坐标情况下，在Fragment Shader中进行像素裁剪
// GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS 或 GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS


#include "UnityCG.cginc"

// 检查
#ifndef GSTORE_USEING_DITHER_TRANSPARENCY
#error "need to define GSTORE_USEING_DITHER_TRANSPARENCY"
#endif

#if GSTORE_USEING_DITHER_TRANSPARENCY

// ---------------------- Shader面板变量 ----------------------
uniform fixed _DitherTransparency;


// -------- 公共常量 --------
// Screen-door transparency: Discard pixel if below threshold.
static const fixed4x4 GSTORE_DITHER_TRANSPARENCY_THRESHOLD_MATRIX =
{
	1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
	13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
	4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
	16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
};

//static const fixed4x4 GSTORE_DITHER_TRANSPARENCY_ROW_ACCESS = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };

// 函数
inline void ApplyDitherTransparencyCutoff(V2F_SCREEN_TYPE screenPos, fixed transparency)
{
	float2 pixel_pos = screenPos.xy / screenPos.w;
	pixel_pos *= _ScreenParams.xy; // pixel position
	pixel_pos = fmod(pixel_pos, 4);
	//clip(transparency - GSTORE_DITHER_TRANSPARENCY_THRESHOLD_MATRIX[fmod(pixel_pos.x, 4)] * GSTORE_DITHER_TRANSPARENCY_ROW_ACCESS[fmod(pixel_pos.y, 4)]);
	clip(transparency - GSTORE_DITHER_TRANSPARENCY_THRESHOLD_MATRIX[pixel_pos.x][pixel_pos.y]);
}

inline void ApplyDitherTransparencyCutoff_ByVPOS(float2 vpos, fixed transparency)
{
	float2 pixel_pos = fmod(vpos.xy, 4);
	//pixel_pos *= _ScreenParams.xy; // pixel position
	clip(transparency - GSTORE_DITHER_TRANSPARENCY_THRESHOLD_MATRIX[pixel_pos.x][pixel_pos.y]);
}


#endif // GSTORE_USEING_DITHER_TRANSPARENCY


// ---------------------- 使用者 - 宏定义 ----------------------
#if GSTORE_USEING_DITHER_TRANSPARENCY

// 在V2F的结构里定义屏幕变量(最后不用;号)
#define GSTORE_DITHER_TRANSPARENCY_COORDS(idx1) V2F_SCREEN_TYPE _ScreenPos : TEXCOORD##idx1;

// 在Vertex Shader中调用获得屏幕坐标
#define GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, v_clipPos) o._ScreenPos = ComputeScreenPos(v_clipPos)

// 在Fragment Shader中进行像素裁剪(已同时使用GSTORE_DITHER_TRANSPARENCY_COORDS和GSTORE_TRANSFER_DITHER_TRANSPARENCY)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF(i) ApplyDitherTransparencyCutoff(i._ScreenPos, _DitherTransparency)

// 在Fragment Shader中进行像素裁剪(单独使用，提供屏幕坐标)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS(screenPos)  ApplyDitherTransparencyCutoff(screenPos, _DitherTransparency)

// 在Fragment Shader中进行像素裁剪(单独使用，提供VPOS坐标)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(vpos)  ApplyDitherTransparencyCutoff_ByVPOS(vpos, _DitherTransparency)


#else // GSTORE_USEING_DITHER_TRANSPARENCY


#define GSTORE_DITHER_TRANSPARENCY_COORDS(idx1)
#define GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, v_clipPos)

#define GSTORE_DITHER_TRANSPARENCY_CUTOFF(i)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS(screenPos)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(vpos)


#endif // GSTORE_USEING_DITHER_TRANSPARENCY



#endif // __DitherTransparency__
