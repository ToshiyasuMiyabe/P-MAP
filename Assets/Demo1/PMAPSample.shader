
Shader "PMAP/PMAP Sample" {
	Properties
	{
		_Tex0 ("Texture0", 2D) = "white" {}
		_Tex1 ("Texture1", 2D) = "white" {}
		_Tex2 ("Texture2", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
//		Blend One OneMinusSrcAlpha
		Blend One SrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
//				fixed4 color    : COLOR;
				half2 texcoord		: TEXCOORD0;
				half2 texcoordA0	: TEXCOORD1;
				half2 texcoordA1	: TEXCOORD2;
				half2 texcoordB		: TEXCOORD3;
			};
			sampler2D _Tex0;
			sampler2D _Tex1;
			sampler2D _Tex2;
			float4 _TexA_ST;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				fixed Scroll = _TexA_ST.z;
				OUT.texcoord = IN.texcoord;
				OUT.texcoordA0 = IN.texcoord + fixed2(-Scroll,0.0f);
				OUT.texcoordA1 = IN.texcoord + fixed2( Scroll,0.5f);
				OUT.texcoordB = IN.texcoord + fixed2( 0.0f,-Scroll);

				return OUT;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 texA = tex2D(_Tex0, IN.texcoord);
				fixed4 texAa0 = tex2D(_Tex0, IN.texcoordA0).a;
				fixed4 texAa1 = tex2D(_Tex0, IN.texcoordA1).a;
				fixed4 texB = tex2D(_Tex1, IN.texcoordB);
				fixed4 texC = tex2D(_Tex2, IN.texcoord);

				fixed4	_res = fixed4(0,0,0,1);	//ベース初期化
				//紫色の霧をアルファ合成
				fixed	texAa = pow(saturate(texAa0 + texAa1),2.0f);
				_res = _res * (1.0f - texAa) + fixed4(texA.rgb,0.0f) * texAa;
				//星を加算合成
				_res.rgb += texB.rgb;
				//煙をアルファ合成
				fixed	texCa = texC.a * (cos(_TexA_ST.z * 20.0f) * 0.5f + 0.5f);
				_res = _res * (1.0f - texCa) + fixed4(texC.rgb,0.0f) * texCa;

//				_res.a = 1.0f - _res.a;	//Blend One SrcAlpha に変更したので不要
				return _res;
			}
		ENDCG
		}
	}
}
