// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GD/Particles/UniversalPremultiplitedFlipbookDistortion"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Flipbook("Flipbook", 2D) = "white" {}
		_Tex1("Tex1", 2D) = "white" {}
		_Add_Blend("Add_Blend", Range( 0 , 4)) = 0
		_SpeedTex1("Speed Tex1", Vector) = (0.21,0.12,-0.06,-0.36)
		_Distortion("Distortion", Float) = -0.13
		_Wave("Wave", Float) = 1.53
		_Colums("Colums", Float) = 0
		_Rows("Rows", Float) = 0
		_StartFrame("StartFrame", Float) = 0
		_Time1("Time1", Float) = 0
		_Speed("Speed", Float) = 0
		_EmissionKek("EmissionKek", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend One OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform sampler2D _Flipbook;
				uniform float _Colums;
				uniform float _Rows;
				uniform float _Speed;
				uniform float _StartFrame;
				uniform float _Time1;
				uniform sampler2D _Tex1;
				uniform float4 _SpeedTex1;
				uniform float _Wave;
				uniform float _Distortion;
				uniform float _EmissionKek;
				uniform float _Add_Blend;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv0123 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					// *** BEGIN Flipbook UV Animation vars ***
					// Total tiles of Flipbook Texture
					float fbtotaltiles147 = _Colums * _Rows;
					// Offsets for cols and rows of Flipbook Texture
					float fbcolsoffset147 = 1.0f / _Colums;
					float fbrowsoffset147 = 1.0f / _Rows;
					// Speed of animation
					float fbspeed147 = _Time1 * _Speed;
					// UV Tiling (col and row offset)
					float2 fbtiling147 = float2(fbcolsoffset147, fbrowsoffset147);
					// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
					// Calculate current tile linear index
					float fbcurrenttileindex147 = round( fmod( fbspeed147 + _StartFrame, fbtotaltiles147) );
					fbcurrenttileindex147 += ( fbcurrenttileindex147 < 0) ? fbtotaltiles147 : 0;
					// Obtain Offset X coordinate from current tile linear index
					float fblinearindextox147 = round ( fmod ( fbcurrenttileindex147, _Colums ) );
					// Multiply Offset X by coloffset
					float fboffsetx147 = fblinearindextox147 * fbcolsoffset147;
					// Obtain Offset Y coordinate from current tile linear index
					float fblinearindextoy147 = round( fmod( ( fbcurrenttileindex147 - fblinearindextox147 ) / _Colums, _Rows ) );
					// Reverse Y to get tiles from Top to Bottom
					fblinearindextoy147 = (int)(_Rows-1) - fblinearindextoy147;
					// Multiply Offset Y by rowoffset
					float fboffsety147 = fblinearindextoy147 * fbrowsoffset147;
					// UV Offset
					float2 fboffset147 = float2(fboffsetx147, fboffsety147);
					// Flipbook UV
					half2 fbuv147 = uv0123 * fbtiling147 + fboffset147;
					// *** END Flipbook UV Animation vars ***
					float2 appendResult114 = (float2(_SpeedTex1.x , _SpeedTex1.y));
					float2 uv0113 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 panner117 = ( 1.0 * _Time.y * appendResult114 + uv0113);
					float4 tex2DNode120 = tex2D( _Tex1, panner117 );
					float2 appendResult112 = (float2(_SpeedTex1.z , _SpeedTex1.w));
					float2 panner116 = ( 1.0 * _Time.y * appendResult112 + uv0113);
					float4 tex2DNode121 = tex2D( _Tex1, ( panner116 * _Wave ) );
					float4 tex2DNode127 = tex2D( _Flipbook, ( float4( fbuv147, 0.0 , 0.0 ) + ( ( tex2DNode120.a * tex2DNode121.a * tex2DNode121 * tex2DNode120 ) * _Distortion ) ).rg );
					float4 temp_output_143_0 = ( i.color * tex2DNode127 * _EmissionKek * i.color.a );
					float4 appendResult169 = (float4(temp_output_143_0.rgb , ( temp_output_143_0 * ( i.color.a * ( tex2DNode127.a * _Add_Blend ) ) ).r));
					

					fixed4 col = saturate( appendResult169 );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18100
60;84;1920;780;60.93081;-735.1726;1.596035;True;True
Node;AmplifyShaderEditor.Vector4Node;111;-2080.665,1937.716;Float;False;Property;_SpeedTex1;Speed Tex1;3;0;Create;True;0;0;False;0;False;0.21,0.12,-0.06,-0.36;0.21,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;113;-1764.634,1814.734;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;112;-1685.384,2051.633;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;116;-1509.447,1984.139;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;114;-1687.172,1929.026;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-1438.879,2125.844;Inherit;False;Property;_Wave;Wave;6;0;Create;True;0;0;False;0;False;1.53;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;117;-1517.057,1848.452;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;118;-1606.005,1587.677;Float;True;Property;_Tex1;Tex1;1;0;Create;True;0;0;False;0;False;None;4e3951c538fc8a647a4a10a99b480987;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;-1284.879,2052.843;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;121;-1197.502,1882.3;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;120;-1156.58,1654.856;Inherit;True;Property;_Noise18;Noise18;0;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;-765.4503,1693.714;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;149;-1214.612,1436.4;Inherit;False;Property;_Rows;Rows;8;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;148;-1213.092,1335.607;Inherit;False;Property;_Colums;Colums;7;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;126;-697.2817,2069.157;Inherit;False;Property;_Distortion;Distortion;5;0;Create;True;0;0;False;0;False;-0.13;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-987.6121,1586.4;Inherit;False;Property;_StartFrame;StartFrame;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;123;-1267.856,1128.097;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;152;-796.6121,1582.4;Inherit;False;Property;_Time1;Time1;10;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-1129.612,1520.4;Inherit;False;Property;_Speed;Speed;11;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;147;-831.3478,1311.375;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-295.4736,1978.73;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;125;-480.4841,1462.776;Inherit;False;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;127;-85.37042,1441.391;Inherit;True;Property;_Flipbook;Flipbook;0;0;Create;True;0;0;False;0;False;-1;None;0994522556391e44fbb16fd985f5c474;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;139;74.90289,1814.865;Inherit;False;Property;_Add_Blend;Add_Blend;2;0;Create;True;0;0;False;0;False;0;3;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;157;128,1088;Inherit;False;Property;_EmissionKek;EmissionKek;12;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;443.8858,1703.367;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;145;61.2211,1227.149;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;611.0779,1501.595;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;676.0965,1178.501;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;825.7682,1479.025;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;169;1039.31,1362.614;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;141;-296.8367,1006.397;Inherit;False;Property;_Color0;Color 0;4;0;Create;True;0;0;False;0;False;0.4339623,0.4073514,0.4073514,1;0.5566038,0.5566038,0.5566038,0.4980392;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;171;1121.218,1553.17;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;166;1400.179,1343.508;Float;False;True;-1;2;ASEMaterialInspector;0;7;GD/Particles/UniversalPremultiplitedFlipbookDistortion;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;3;1;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;0
WireConnection;112;0;111;3
WireConnection;112;1;111;4
WireConnection;116;0;113;0
WireConnection;116;2;112;0
WireConnection;114;0;111;1
WireConnection;114;1;111;2
WireConnection;117;0;113;0
WireConnection;117;2;114;0
WireConnection;119;0;116;0
WireConnection;119;1;115;0
WireConnection;121;0;118;0
WireConnection;121;1;119;0
WireConnection;120;0;118;0
WireConnection;120;1;117;0
WireConnection;122;0;120;4
WireConnection;122;1;121;4
WireConnection;122;2;121;0
WireConnection;122;3;120;0
WireConnection;147;0;123;0
WireConnection;147;1;148;0
WireConnection;147;2;149;0
WireConnection;147;3;150;0
WireConnection;147;4;151;0
WireConnection;147;5;152;0
WireConnection;124;0;122;0
WireConnection;124;1;126;0
WireConnection;125;0;147;0
WireConnection;125;1;124;0
WireConnection;127;1;125;0
WireConnection;142;0;127;4
WireConnection;142;1;139;0
WireConnection;144;0;145;4
WireConnection;144;1;142;0
WireConnection;143;0;145;0
WireConnection;143;1;127;0
WireConnection;143;2;157;0
WireConnection;143;3;145;4
WireConnection;170;0;143;0
WireConnection;170;1;144;0
WireConnection;169;0;143;0
WireConnection;169;3;170;0
WireConnection;171;0;169;0
WireConnection;166;0;171;0
ASEEND*/
//CHKSM=1011EDB4EC6E1ECB58DAA9BDE821AAD3034CEAE3