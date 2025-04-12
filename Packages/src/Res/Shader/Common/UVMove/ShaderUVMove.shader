Shader "KissShader/ShaderUVMove"
{
    Properties
    {
        _MainTex("主纹理", 2D) = "white" {}
        _Speed("UV移动速度", Vector) = (0.1, 0.0, 0, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Speed;

            struct appdata
            {
                float4 vertex : POSITION; // 顶点位置
                float2 uv : TEXCOORD0; // 纹理坐标
            };

            struct v2f
            {
                float4 vertex : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0; // 将传递给片元着色器的纹理坐标
            };

            v2f vert(appdata v)
            {
                v2f o;
                // 根据速度和平移时间计算新的纹理坐标
                float2 uvOffset = v.uv + _Speed.xy * _Time.x;
                // 使用 frac 保证纹理坐标在 0～1 内循环显示
                o.uv = frac(uvOffset);
                // 转换顶点坐标到裁剪空间
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 根据新的纹理坐标采样主纹理
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}