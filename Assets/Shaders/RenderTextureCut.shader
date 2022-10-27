Shader "Starter/RenderTextureCut"
{
    Properties
    {
        _MainTex ("MainTex",2d)="white" {}

    }
    SubShader
    {
        Cull off
        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;


            struct a2v
            {
                float4 vertex :  POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 screen_pos : TEXCOORD2;
            };


            v2f vert (a2v v)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.screen_pos=ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                i.screen_pos/=i.pos.w;
                
                return tex2D(_MainTex,float2(i.screen_pos.x,i.screen_pos.y));
            }
            ENDCG
        }
    }
}
