Shader "Custom/GlassBlur" {
	Properties{
		_blurSizeXY("BlurSizeXY", Range(0, 1)) = 0
	}
		SubShader{

		// 不透明オブジェクトを描画した後に実行する
		Tags{ "Queue" = "Transparent+1" }

		// オブジェクトの背景テクスチャーを _BackgroundTexture に取得する。
		GrabPass{ "_BackgroundTexture" }

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag 
		#include "UnityCG.cginc"

		sampler2D _BackgroundTexture;
	float _blurSizeXY;

	struct v2f {
		float4 position : POSITION;
		float4 screenPos : TEXCOORD0;
	};

	v2f vert(appdata_base i) {
		v2f o;
		o.position = UnityObjectToClipPos(i.vertex);
		o.screenPos = ComputeGrabScreenPos(o.position);
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		float2 screenPos = i.screenPos.xy / i.screenPos.w;
		float depth = _blurSizeXY * 0.01;

		half4 sum = half4(0.0h, 0.0h, 0.0h, 0.0h);
		float count = 0.0;

		for (float x = -1.0; x <= 1.0; x += 0.25) {
			for (float y = -1.0; y <= 1.0; y += 0.25) {
				sum += tex2D(_BackgroundTexture, float2(screenPos.x + x * depth, screenPos.y + y * depth));
				count += 1.0;
			}
		}

		return sum / count;
	}
		ENDCG
	}
	}
}