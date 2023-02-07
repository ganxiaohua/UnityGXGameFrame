Shader "SoulGames/Effects/Additive"
{
	Properties
	{
		_MainTex("Base", 2D) = "white" {}
	/*[HDR]*/_TintColor("TintColor", Color) = (0.5, 0.5, 0.5, 0.5)
	_DeadStrength("Color Dead Num", Range(0,1)) = 0.01
	_StencilComp("Stencil Comparison", Float) = 8
	_Stencil("Stencil ID", Float) = 0
	_StencilOp("Stencil Operation", Float) = 0
	_StencilWriteMask("Stencil Write Mask", Float) = 255
	_StencilReadMask("Stencil Read Mask", Float) = 255

	_BlendFactor("Blend Factor", Range(0,1)) = 0.3
	}

		CGINCLUDE



#include "UnityCG.cginc"

		sampler2D _MainTex;
	fixed4 _TintColor;
	fixed _DeadStrength;
	fixed _BlendFactor;

	#ifdef CLIPPED
	float4 _ClipBox = float4(-2, -2, 0, 0);
	#endif

	struct v2f
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		fixed4 color : COLOR;
		#ifdef CLIPPED
		float2 clipPos : TEXCOORD1;
		#endif
	};

	float4 _MainTex_ST;

	v2f vert(appdata_full v)
	{
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.color = v.color;
		#ifdef CLIPPED
		o.clipPos = mul(unity_ObjectToWorld, v.vertex).xy * _ClipBox.zw + _ClipBox.xy;
		#endif
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		fixed4 finalColor = tex2D(_MainTex, i.uv) * i.color * _TintColor;
		if (finalColor.r + finalColor.g + finalColor.b < _DeadStrength)
		{
			finalColor.a = 0;
		}

		#ifdef CLIPPED
		float2 factorZ = abs(i.clipPos);
		finalColor.a *= step(max(factorZ.x, factorZ.y), 1);
		#endif
		return finalColor * 2.0f;
	}

	fixed4 frag_alpha(v2f i) : COLOR
	{
		fixed4 finalColor = tex2D(_MainTex, i.uv);
		if (finalColor.r + finalColor.g + finalColor.b < _DeadStrength)
		{
			finalColor.a = 0;
		}
		else
		{
			finalColor.a = finalColor.a*_BlendFactor;
		}
		return fixed4(finalColor.rgb * 2.0f, finalColor.a);
	}

		ENDCG

		SubShader
		{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Off
			Lighting Off
			ZWrite Off
			Fog{ Mode Off }


			Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}


			Pass
		{
			Blend SrcAlpha One
			ColorMask RGB
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma multi_compile _ CLIPPED


			ENDCG
		}
			Pass
		{
			Blend SrcAlpha One
			BlendOp RevSub
			ColorMask A
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_alpha
			#pragma fragmentoption ARB_precision_hint_fastest 

			ENDCG
		}

		}
	FallBack Off
}