Shader "Custom/TransShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Fade("Fade", Range(0, 1)) = 0.5
		_Light("Light", Range(0, 1)) = 0.1
		_Wave("Wave", Range(0, 10)) = 0.1
		_AnimSpeed("AnimSpeed", Range(0, 10)) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Fade;
			float _Light;
			float _Wave;
			float _AnimSpeed;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = float3(_Light, _Light, _Light) * 0.5 + col.rgb * (1 - _Fade);
				col.rgb *= float3(_Light, _Light, _Light) + (sin(i.uv.y * _Wave + _Time.y * _AnimSpeed) + 1.0) * 0.5;
				return col;
			}
			ENDCG
		}
	}
}
