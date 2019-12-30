// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/CaptureGray"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color",Color)=(1,1,1,1)
		//_OutRegionTex ("OutRegionTexture", 2D) = "white" {}
		//_LeftDownConner("UV",Vector)=(0,0,0,0)
		//_RightUpConner("UV",Vector)=(0,0,0,0)
		_Rect("Rect",Vector)=(0,0,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
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
			//fixed4 _LeftDownConner;
			//fixed4 _RightUpConner;
			fixed4 _Rect;
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
			    fixed4 col =_Color;
				float2 uv = i.uv ;
				if(uv.x> _Rect.x&&uv.x< _Rect.z && uv.y> _Rect.y&&uv.y< _Rect.w)
				   col.a = 0;
			
				return col;
			}
			ENDCG
		}
	}
}
