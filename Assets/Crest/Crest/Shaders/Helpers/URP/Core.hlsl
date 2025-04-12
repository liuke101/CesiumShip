// Crest Ocean System

#ifndef CREST_URP_CORE_INCLUDED
#define CREST_URP_CORE_INCLUDED

#define CREST_URP 1

// Not set and _ScreenParams.zw is "1.0 + 1.0 / _ScreenParams.xy"
#define _ScreenSize float4(_ScreenParams.xy, float2(1.0, 1.0) / _ScreenParams.xy)

#if UNITY_VERSION < 60000000
#define FoveatedRemapLinearToNonUniform(uv) uv
#endif

#if UNITY_VERSION < 202130
#define m_InitializeInputDataFog(x, y) y
#else
#define m_InitializeInputDataFog InitializeInputDataFog
#endif

#endif // CREST_URP_CORE_INCLUDED
