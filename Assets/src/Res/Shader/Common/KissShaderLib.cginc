#ifndef KISSSHADER_LIB_INCLUDED
#define KISSSHADER_LIB_INCLUDED

#include "UVScroll.cginc"
#include "ShinyEffect.cginc"

// 可选其他模块

inline float2 Kiss_ApplyUV(float2 uv)
{
    #if defined(KISS_USE_UVSCROLL)
    uv = Kiss_UVScroll(uv);
    #endif
    return uv;
}

inline fixed4 Kiss_ApplyColor(fixed4 col, float2 uv)
{
    #if defined(KISS_USE_SHINY)
    col.rgb += Kiss_ShinyEffect(uv);
    #endif
    return col;
}

#endif
