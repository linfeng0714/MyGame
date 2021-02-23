#ifndef __DitherTransparency__
#define __DitherTransparency__


// Dither Transparency��
// ʹ�÷���:
// 1.ʹ��ǰ��shader�ļ�����Ҫ����GSTORE_USEING_DITHER_TRANSPARENCY��ֵ
// 2.�ڶ���GSTORE_USEING_DITHER_TRANSPARENCY��������������ļ�

// ���ӣ�
// #define GSTORE_USEING_DITHER_TRANSPARENCY 1
// #include "../Lib/DitherTransparency.cginc"

// [shader������ñ���]
// _DitherTransparency("Dither Transparency", Range(0,1)) = 1.0

// ��ʽ1:
// ��V2F�Ľṹ�ﶨ����Ļ����:
// GSTORE_DITHER_TRANSPARENCY_COORDS
// ��Vertex Shader�е��û����Ļ����:
// GSTORE_TRANSFER_DITHER_TRANSPARENCY
// ��Fragment Shader�н������زü�(��ͬʱʹ��GSTORE_DITHER_TRANSPARENCY_COORDS �� GSTORE_TRANSFER_DITHER_TRANSPARENCY):
// GSTORE_DITHER_TRANSPARENCY_CUTOFF

// ��ʽ2:
// �ѻ����Ļ��������£���Fragment Shader�н������زü�
// GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS �� GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS


#include "UnityCG.cginc"

// ���
#ifndef GSTORE_USEING_DITHER_TRANSPARENCY
#error "need to define GSTORE_USEING_DITHER_TRANSPARENCY"
#endif

#if GSTORE_USEING_DITHER_TRANSPARENCY

// ---------------------- Shader������ ----------------------
uniform fixed _DitherTransparency;


// -------- �������� --------
// Screen-door transparency: Discard pixel if below threshold.
static const fixed4x4 GSTORE_DITHER_TRANSPARENCY_THRESHOLD_MATRIX =
{
	1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
	13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
	4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
	16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
};

//static const fixed4x4 GSTORE_DITHER_TRANSPARENCY_ROW_ACCESS = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };

// ����
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


// ---------------------- ʹ���� - �궨�� ----------------------
#if GSTORE_USEING_DITHER_TRANSPARENCY

// ��V2F�Ľṹ�ﶨ����Ļ����(�����;��)
#define GSTORE_DITHER_TRANSPARENCY_COORDS(idx1) V2F_SCREEN_TYPE _ScreenPos : TEXCOORD##idx1;

// ��Vertex Shader�е��û����Ļ����
#define GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, v_clipPos) o._ScreenPos = ComputeScreenPos(v_clipPos)

// ��Fragment Shader�н������زü�(��ͬʱʹ��GSTORE_DITHER_TRANSPARENCY_COORDS��GSTORE_TRANSFER_DITHER_TRANSPARENCY)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF(i) ApplyDitherTransparencyCutoff(i._ScreenPos, _DitherTransparency)

// ��Fragment Shader�н������زü�(����ʹ�ã��ṩ��Ļ����)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS(screenPos)  ApplyDitherTransparencyCutoff(screenPos, _DitherTransparency)

// ��Fragment Shader�н������زü�(����ʹ�ã��ṩVPOS����)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(vpos)  ApplyDitherTransparencyCutoff_ByVPOS(vpos, _DitherTransparency)


#else // GSTORE_USEING_DITHER_TRANSPARENCY


#define GSTORE_DITHER_TRANSPARENCY_COORDS(idx1)
#define GSTORE_TRANSFER_DITHER_TRANSPARENCY(o, v_clipPos)

#define GSTORE_DITHER_TRANSPARENCY_CUTOFF(i)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_SCREENPOS(screenPos)
#define GSTORE_DITHER_TRANSPARENCY_CUTOFF_VPOS(vpos)


#endif // GSTORE_USEING_DITHER_TRANSPARENCY



#endif // __DitherTransparency__
