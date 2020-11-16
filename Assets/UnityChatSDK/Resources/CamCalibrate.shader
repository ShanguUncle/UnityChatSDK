// 
//   NatCorder
//   Copyright (c) 2019 Yusuf Olokoba
//

Shader "UnityChatSDK/CamCalibrate" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Rotation ("Rotation", float) = 0
		_Scale ("Scale", float) = 1
	}
	SubShader {

		Tags {
			"Queue"="Transparent"
			"RenderType"="Transparent" 
			"IgnoreProjector"="True"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off
		Fog { Mode off }
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			uniform fixed _Rotation, _Scale;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// Rotate and mirror UV
				o.uv = v.uv - float2(0.5, 0.5);
				float s, c;
				sincos(_Rotation, s, c);
				float2x2 transform = mul(float2x2(
					float2(c, -s),
					float2(s, c)
				), float2x2(
					float2(_Scale, 0.0),
					float2(0.0, 1.0)
				));
				o.uv = mul(transform, o.uv) + float2(0.5, 0.5);
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target {
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
