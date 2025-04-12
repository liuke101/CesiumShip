// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hologram_Premultiplited"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Tex_Noise("Tex_Noise", 2D) = "white" {}
		_SpeedTex1("Speed Tex1", Vector) = (0.21,0.12,-0.06,-0.36)
		_Distortion("Distortion", Float) = -0.13
		_Wave("Wave", Float) = 1.53
		_Colums("Colums", Float) = 0
		_Rows("Rows", Float) = 0
		_StartFrame("StartFrame", Float) = 0
		_Time1("Time1", Float) = 0
		_Speed("Speed", Float) = 0
		_EmissionKek("EmissionKek", Float) = 0
		_SpeedHolo("SpeedHolo", Float) = 1
		_T_Hologram("T_Hologram", 2D) = "white" {}
		_SizeHologram("SizeHologram", Vector) = (1,1,0,0)
		[Toggle(_HOLOGRAMA_ON)] _Holograma("Holograma", Float) = 0

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
				#pragma shader_feature_local _HOLOGRAMA_ON


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
				uniform sampler2D _TextureSample0;
				uniform float _Colums;
				uniform float _Rows;
				uniform float _Speed;
				uniform float _StartFrame;
				uniform float _Time1;
				uniform sampler2D _Tex_Noise;
				uniform float4 _SpeedTex1;
				uniform float _Wave;
				uniform float _Distortion;
				uniform float _EmissionKek;
				uniform sampler2D _T_Hologram;
				uniform float2 _SizeHologram;
				uniform float _SpeedHolo;


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

					float2 texCoord27 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					// *** BEGIN Flipbook UV Animation vars ***
					// Total tiles of Flipbook Texture
					float fbtotaltiles32 = _Colums * _Rows;
					// Offsets for cols and rows of Flipbook Texture
					float fbcolsoffset32 = 1.0f / _Colums;
					float fbrowsoffset32 = 1.0f / _Rows;
					// Speed of animation
					float fbspeed32 = _Time1 * _Speed;
					// UV Tiling (col and row offset)
					float2 fbtiling32 = float2(fbcolsoffset32, fbrowsoffset32);
					// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
					// Calculate current tile linear index
					float fbcurrenttileindex32 = round( fmod( fbspeed32 + _StartFrame, fbtotaltiles32) );
					fbcurrenttileindex32 += ( fbcurrenttileindex32 < 0) ? fbtotaltiles32 : 0;
					// Obtain Offset X coordinate from current tile linear index
					float fblinearindextox32 = round ( fmod ( fbcurrenttileindex32, _Colums ) );
					// Multiply Offset X by coloffset
					float fboffsetx32 = fblinearindextox32 * fbcolsoffset32;
					// Obtain Offset Y coordinate from current tile linear index
					float fblinearindextoy32 = round( fmod( ( fbcurrenttileindex32 - fblinearindextox32 ) / _Colums, _Rows ) );
					// Reverse Y to get tiles from Top to Bottom
					fblinearindextoy32 = (int)(_Rows-1) - fblinearindextoy32;
					// Multiply Offset Y by rowoffset
					float fboffsety32 = fblinearindextoy32 * fbrowsoffset32;
					// UV Offset
					float2 fboffset32 = float2(fboffsetx32, fboffsety32);
					// Flipbook UV
					half2 fbuv32 = texCoord27 * fbtiling32 + fboffset32;
					// *** END Flipbook UV Animation vars ***
					float2 appendResult17 = (float2(_SpeedTex1.x , _SpeedTex1.y));
					float2 texCoord14 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 panner19 = ( 1.0 * _Time.y * appendResult17 + texCoord14);
					float4 tex2DNode23 = tex2D( _Tex_Noise, panner19 );
					float2 appendResult15 = (float2(_SpeedTex1.z , _SpeedTex1.w));
					float2 panner16 = ( 1.0 * _Time.y * appendResult15 + texCoord14);
					float4 tex2DNode22 = tex2D( _Tex_Noise, ( panner16 * _Wave ) );
					float4 temp_output_36_0 = ( ( i.color * tex2D( _TextureSample0, ( float4( fbuv32, 0.0 , 0.0 ) + ( ( tex2DNode23.a * tex2DNode22.a * tex2DNode22 * tex2DNode23 ) * _Distortion ) ).rg ) * i.color.a ) * _EmissionKek );
					float2 temp_cast_4 = (_SpeedHolo).xx;
					float2 panner41 = ( 1.0 * _Time.y * temp_cast_4 + float2( 0,0 ));
					float2 texCoord45 = i.texcoord.xy * _SizeHologram + panner41;
					#ifdef _HOLOGRAMA_ON
					float4 staticSwitch53 = ( temp_output_36_0 - tex2D( _T_Hologram, texCoord45 ) );
					#else
					float4 staticSwitch53 = temp_output_36_0;
					#endif
					

					fixed4 col = saturate( staticSwitch53 );
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
Version=18800
-1920;35;1920;762;2586.563;1124.416;2.213431;True;True
Node;AmplifyShaderEditor.Vector4Node;13;-2687.175,-105.8522;Float;False;Property;_SpeedTex1;Speed Tex1;2;0;Create;True;0;0;0;False;0;False;0.21,0.12,-0.06,-0.36;0.21,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-2371.145,-228.8341;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;15;-2291.895,8.064968;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2045.389,82.27591;Inherit;False;Property;_Wave;Wave;5;0;Create;True;0;0;0;False;0;False;1.53;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-2293.682,-114.5421;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;16;-2115.958,-59.42906;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;19;-2123.567,-195.1161;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;20;-2212.515,-455.8911;Float;True;Property;_Tex_Noise;Tex_Noise;1;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1891.389,9.274929;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;23;-1763.09,-388.7121;Inherit;True;Property;_Noise18;Noise18;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-1804.012,-161.2681;Inherit;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1371.961,-349.8541;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1594.123,-457.1681;Inherit;False;Property;_StartFrame;StartFrame;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1821.122,-607.168;Inherit;False;Property;_Rows;Rows;7;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-1874.366,-915.4709;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-1303.792,25.58889;Inherit;False;Property;_Distortion;Distortion;4;0;Create;True;0;0;0;False;0;False;-0.13;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1736.122,-523.168;Inherit;False;Property;_Speed;Speed;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-1819.602,-707.961;Inherit;False;Property;_Colums;Colums;6;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1403.123,-461.1681;Inherit;False;Property;_Time1;Time1;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;32;-1437.858,-732.1931;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-901.9839,-64.83812;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-730.6514,489.6797;Inherit;False;Property;_SpeedHolo;SpeedHolo;12;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-1086.995,-580.7921;Inherit;False;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-506.6658,-751.9291;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;95dc0bc4e76e5cc49b0dc7d80ee3aa85;95dc0bc4e76e5cc49b0dc7d80ee3aa85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;3;43.98151,-1120.443;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;44;-193.6514,150.6797;Inherit;True;Property;_SizeHologram;SizeHologram;14;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;41;-260.4933,443.5351;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;35;103.2599,-452.9688;Inherit;False;Property;_EmissionKek;EmissionKek;11;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;192.0123,-737.9941;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;91.34856,273.6797;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;46;387.1427,256.2806;Inherit;True;Property;_T_Hologram;T_Hologram;13;0;Create;True;0;0;0;False;0;False;-1;8521fffcc0cdafe4099d25841604670b;8521fffcc0cdafe4099d25841604670b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;317.8015,-636.4407;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;51;787.313,-77.37459;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;53;790.723,-313.6277;Inherit;False;Property;_Holograma;Holograma;15;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;54;210.1985,-155.017;Inherit;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;37;978.0134,-710.2602;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;1429.801,-251.4668;Inherit;False;myVarName;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-198.5914,-362.6123;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;42;-496.6514,429.6797;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-633.4877,-363.582;Inherit;False;Property;_Add_Blend;Add_Blend;3;0;Create;True;0;0;0;False;0;False;0;0.63;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;52;-493.5016,258.5123;Inherit;False;Constant;_Vector1;Vector 1;14;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;730.1164,-639.4314;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;1872.977,-223.016;Inherit;False;47;myVarName;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;392.2054,-350.2331;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1562.096,-784.2411;Float;False;True;-1;2;ASEMaterialInspector;0;7;Hologram_Premultiplited;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;3;1;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;15;0;13;3
WireConnection;15;1;13;4
WireConnection;17;0;13;1
WireConnection;17;1;13;2
WireConnection;16;0;14;0
WireConnection;16;2;15;0
WireConnection;19;0;14;0
WireConnection;19;2;17;0
WireConnection;21;0;16;0
WireConnection;21;1;18;0
WireConnection;23;0;20;0
WireConnection;23;1;19;0
WireConnection;22;0;20;0
WireConnection;22;1;21;0
WireConnection;30;0;23;4
WireConnection;30;1;22;4
WireConnection;30;2;22;0
WireConnection;30;3;23;0
WireConnection;32;0;27;0
WireConnection;32;1;28;0
WireConnection;32;2;29;0
WireConnection;32;3;24;0
WireConnection;32;4;26;0
WireConnection;32;5;25;0
WireConnection;33;0;30;0
WireConnection;33;1;31;0
WireConnection;34;0;32;0
WireConnection;34;1;33;0
WireConnection;1;1;34;0
WireConnection;41;2;43;0
WireConnection;10;0;3;0
WireConnection;10;1;1;0
WireConnection;10;2;3;4
WireConnection;45;0;44;0
WireConnection;45;1;41;0
WireConnection;46;1;45;0
WireConnection;36;0;10;0
WireConnection;36;1;35;0
WireConnection;51;0;36;0
WireConnection;51;1;46;0
WireConnection;53;1;36;0
WireConnection;53;0;51;0
WireConnection;37;0;53;0
WireConnection;39;0;3;4
WireConnection;39;1;38;0
WireConnection;49;0;36;0
WireConnection;40;0;36;0
WireConnection;40;1;39;0
WireConnection;0;0;37;0
ASEEND*/
//CHKSM=0F88DB88687D550A65BF98D824F6508DAA25E66F