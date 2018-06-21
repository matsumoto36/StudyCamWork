Shader "Custom/LineShader" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Fill("Fill", float) = 1
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+1" }

		//ColorMask 0

		/*Stencil{
			Ref 1
			Comp equal
		}*/

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		half3 _Color;
		float _Fill;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = fixed3(0, 0, 0);

		if (IN.uv_MainTex.x > 1.0f - _Fill) {
			o.Alpha = c.a;
			o.Emission = _Color;
		}
		else {
			clip(-1);
			o.Alpha = 1;
			o.Emission = half3(0, 0, 0);
		}
	}
	ENDCG
	}
		FallBack "Diffuse"
}
