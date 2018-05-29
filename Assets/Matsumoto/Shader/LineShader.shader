Shader "Custom/ZTest" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }


		Stencil{
			Ref 1
			Comp equal
		}

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		half3 _Color;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = fixed3(0, 0, 0);
		o.Alpha = c.a;
		//o.Emission = c.rgb;
		o.Emission = _Color;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
