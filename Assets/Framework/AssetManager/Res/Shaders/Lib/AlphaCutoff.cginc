#ifndef __AlphaCutoff__
#define __AlphaCutoff__


// Alpha Cutoff库
// 使用方法:
// 1.使用前在shader文件中需要定义GSTORE_USING_ALPHA_CUTOFF的值
// 2.在定义GSTORE_USING_ALPHA_CUTOFF后，再引用这个库文件

// 例子：
// #define GSTORE_USING_ALPHA_CUTOFF 1
// #include "../Lib/AlphaCutoff.cginc"

// [shader面板设置变量]
// _Cutoff("Alpha cutoff", Range(0,1)) = 0.5

// [fragment shader里调用宏]
// 两个可使用的宏，根据需要任选一种，最后要自行加;号
// GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(sampler2D, UV) - 根据贴图及对应UV进行Cutoff
// GSTORE_APPLY_ALPHA_CUTOFF(alpha_value) - 根据Alpha值进行Cutoff



// 检查
#ifndef GSTORE_USING_ALPHA_CUTOFF
#error "need to define GSTORE_USING_ALPHA_CUTOFF"
#endif


#if GSTORE_USING_ALPHA_CUTOFF

// ---------------------- Shader面板变量 ----------------------
// 一般unity内置shader使用也是_Cutoff变量名，所以这里也统一使用这个
uniform fixed _Cutoff;

// ---------------------- 内部函数 ----------------------
// Alpha cutoff
inline void ApplyAlphaCutoff(fixed texcol_alpha)
{
	clip(texcol_alpha - _Cutoff);
}
inline void ApplyTextureAlphaCutoff(sampler2D tex, fixed2 uv)
{
	ApplyAlphaCutoff(tex2D(tex, uv).a);
}

#endif


// ---------------------- 使用者 - 宏定义 ----------------------
#if GSTORE_USING_ALPHA_CUTOFF
#define GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(tex, uv) ApplyTextureAlphaCutoff(tex, uv)
#define GSTORE_APPLY_ALPHA_CUTOFF(texcol_alpha) ApplyAlphaCutoff(texcol_alpha)
#else
#define GSTORE_APPLY_TEXTURE_ALPHA_CUTOFF(tex, uv)
#define GSTORE_APPLY_ALPHA_CUTOFF(texcol_alpha)
#endif


#endif // __AlphaCutoff__
