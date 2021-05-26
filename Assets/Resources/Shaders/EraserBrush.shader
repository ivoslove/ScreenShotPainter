// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/EraserBrush"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrushTex("Brush Texture",2D)= "white" {}
		_Color("Color",Color)=(1,1,1,1)
		_UV("UV",Vector)=(0,0,0,0)
		_Size("Size",Range(1,1000))=1

		
		_SizeY("SizeY",Range(1,1000))=1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
		Blend Off
		//Blend One DstColor
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BrushTex;
			fixed4 _UV;
			float _Size;
			float _SizeY;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
		       float size = _Size;
               float2 uv = i.uv + (0.5f / size);
               uv = uv - _UV.xy;
               uv *= size;





			   float sizeY = _SizeY;
			   uv.y=uv.y*sizeY/size;

			   	float cirle =  pow(uv.x-0.5f, 2)+pow(uv.y-0.5f*sizeY/size, 2);
				if(cirle>0.25f)
				   discard;
				fixed4 col = tex2D(_BrushTex,uv);
				col.rgb = 1;
				col *= _Color;

				return col;
			}
			ENDCG
		}
	}
}
