Shader "DreamEngine/UIGrayScale"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _GrayScaleAmount ("Gray Scale Amount", Range(0, 1)) = 1.0
        _Transparency ("Transparency", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            // 混合模式设置为支持透明度
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _GrayScaleAmount;
            float _Transparency;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                // 计算灰度值
                float gray = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                texColor.rgb = lerp(texColor.rgb, float3(gray, gray, gray), _GrayScaleAmount);
                // 使用透明度控制 alpha 通道
                texColor.a *= _Transparency;
                return texColor;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
