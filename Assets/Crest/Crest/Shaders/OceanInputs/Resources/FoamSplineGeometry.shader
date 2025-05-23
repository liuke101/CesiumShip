﻿// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

Shader "Hidden/Crest/Inputs/Foam/Spline Geometry"
{
	SubShader
	{
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			CBUFFER_START(CrestPerOceanInput)
			half _FeatherWidth;
			float3 _DisplacementAtInputPosition;
			float _Weight;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
				float invNormDistToShoreline : TEXCOORD1;
				float weight : TEXCOORD2;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 invNormDistToShoreline_weight : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;

				float3 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
				// Correct for displacement
				worldPos.xz -= _DisplacementAtInputPosition.xz;
				o.positionCS = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

				o.invNormDistToShoreline_weight.x = input.invNormDistToShoreline;

				o.invNormDistToShoreline_weight.y = input.weight;

				return o;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				float wt = input.invNormDistToShoreline_weight.y;

				// Feather at front/back
				if (_FeatherWidth > 0.0)
				{
					if( input.invNormDistToShoreline_weight.x > 0.5 )
					{
						input.invNormDistToShoreline_weight.x = 1.0 - input.invNormDistToShoreline_weight.x;
					}
					wt *= min( input.invNormDistToShoreline_weight.x / _FeatherWidth, 1.0 );
				}

				return 0.02 * wt;
			}
			ENDCG
		}
	}
}
