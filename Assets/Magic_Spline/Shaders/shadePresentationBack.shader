// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33832,y:32756,varname:node_3138,prsc:2|emission-6473-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32767,y:32486,ptovrint:False,ptlb:Color Checker 01,ptin:_ColorChecker01,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:3438,x:32578,y:32177,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_3438,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:55d29bb645851824684d646c32793426,ntxv:0,isnm:False;n:type:ShaderForge.SFN_FragmentPosition,id:6916,x:32494,y:33025,varname:node_6916,prsc:2;n:type:ShaderForge.SFN_Color,id:406,x:32740,y:32896,ptovrint:False,ptlb:Color Fog,ptin:_ColorFog,varname:node_406,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:2072,x:32914,y:33136,varname:node_2072,prsc:2|A-9459-OUT,B-830-OUT;n:type:ShaderForge.SFN_Lerp,id:9829,x:33345,y:32839,varname:node_9829,prsc:2|A-2557-OUT,B-406-RGB,T-8109-OUT;n:type:ShaderForge.SFN_Slider,id:830,x:32757,y:33333,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:node_830,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:9459,x:32684,y:33136,varname:node_9459,prsc:2|A-6916-Y,B-8048-OUT;n:type:ShaderForge.SFN_Slider,id:8048,x:32402,y:33339,ptovrint:False,ptlb:Offset Fog,ptin:_OffsetFog,varname:node_8048,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:50;n:type:ShaderForge.SFN_Clamp01,id:8109,x:33115,y:33136,varname:node_8109,prsc:2|IN-2072-OUT;n:type:ShaderForge.SFN_Color,id:6229,x:32641,y:32634,ptovrint:False,ptlb:Color Checker 02,ptin:_ColorChecker02,varname:_ColorChecker02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Color,id:7447,x:32507,y:32768,ptovrint:False,ptlb:Color Grid,ptin:_ColorGrid,varname:node_7447,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_ComponentMask,id:8564,x:32917,y:32312,varname:node_8564,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-3438-RGB;n:type:ShaderForge.SFN_ChannelBlend,id:2557,x:33196,y:32574,varname:node_2557,prsc:2,chbt:1|M-8564-OUT,R-7241-RGB,G-6229-RGB,BTM-7447-RGB;n:type:ShaderForge.SFN_LightAttenuation,id:7052,x:33351,y:33163,varname:node_7052,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1583,x:33541,y:33141,varname:node_1583,prsc:2|A-7052-OUT,B-4186-A;n:type:ShaderForge.SFN_LightColor,id:4186,x:33351,y:33319,varname:node_4186,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6473,x:33608,y:32917,varname:node_6473,prsc:2|A-9829-OUT,B-1583-OUT;proporder:7447-7241-6229-3438-406-830-8048;pass:END;sub:END;*/

Shader "Lessons RealTime VFX/remapTexAlphablend" {
    Properties {
        _ColorGrid ("Color Grid", Color) = (1,1,1,1)
        _ColorChecker01 ("Color Checker 01", Color) = (1,1,1,1)
        _ColorChecker02 ("Color Checker 02", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _ColorFog ("Color Fog", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 1)) = 0
        _OffsetFog ("Offset Fog", Range(0, 50)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 2.0
            uniform float4 _ColorChecker01;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _ColorFog;
            uniform float _Intensity;
            uniform float _OffsetFog;
            uniform float4 _ColorChecker02;
            uniform float4 _ColorGrid;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float2 node_8564 = _MainTex_var.rgb.rg;
                float3 emissive = (lerp((lerp( lerp( _ColorGrid.rgb, _ColorChecker01.rgb, node_8564.r ), _ColorChecker02.rgb, node_8564.g )),_ColorFog.rgb,saturate(((i.posWorld.g+_OffsetFog)*_Intensity)))*(attenuation*_LightColor0.a));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 2.0
            uniform float4 _ColorChecker01;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _ColorFog;
            uniform float _Intensity;
            uniform float _OffsetFog;
            uniform float4 _ColorChecker02;
            uniform float4 _ColorGrid;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 finalColor = 0;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
