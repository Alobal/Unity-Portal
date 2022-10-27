// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Starter/ClipAfterPortal"
{
    Properties
    {
        _Color ("Color Tint",Color) =(1,1,1,1)
        _FresnelScale("Fresnel Scale",Range(0,1))=0.5
        _Cubemap("Reflection Cubemap",Cube)="_Skybox"{}
        clip_pos("Clip Pos",Vector)=(0,0,0)
        clip_normal("",Vector)=(0,0,0)

    }
    SubShader
    {
        CGINCLUDE
        fixed4 _Color;
        fixed _FresnelScale;
        float3 clip_pos;
        float3 clip_normal;
        samplerCUBE _Cubemap;
        #include "Lighting.cginc"
        #include "AutoLight.cginc"

        ENDCG
        

        Pass
        {
            Tags{"LightMode"="ForwardBase"}
            CGPROGRAM

            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world_normal : TEXCOORD0;
                float3 world_pos : TEXCOORD1;
                SHADOW_COORDS(2) 
            };

            v2f vert(a2v a)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(a.vertex);
                o.world_normal=UnityObjectToWorldNormal(a.normal);
                o.world_pos=mul(unity_ObjectToWorld,a.vertex).xyz;


                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //if offset_pos and clip_normal is not same side , dot operation will <0, and need clip.
                float3 offset_pos=i.world_pos - clip_pos;
                clip(dot(offset_pos,clip_normal));

                fixed3 world_normal=normalize(i.world_normal);
                fixed3 world_lightdir=normalize(UnityWorldSpaceLightDir(i.world_pos));
                fixed3 world_view=normalize(UnityWorldSpaceViewDir(i.world_pos));
                //逆向计算反射入射光线

                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz*_Color.rgb;
                fixed3 diffuse=_LightColor0.rgb * _Color.rgb * saturate(dot(world_normal,world_lightdir));


                fixed3 world_reflect=reflect(-world_view,world_normal);
                fixed3 reflection=texCUBE(_Cubemap,world_reflect).rgb;

                fixed fresnel = _FresnelScale + (1- _FresnelScale)*pow(1-dot(world_view,world_normal),5);

                UNITY_LIGHT_ATTENUATION(atten,i,i.world_pos);

                return fixed4(ambient+lerp(diffuse,reflection,saturate(fresnel))*(atten*0.5+0.5),1.0);
            }
            ENDCG
        }


         Pass 
         {  //自定义shadow pass 剔除透明frag的阴影
            Tags
            {
                "LightMode" ="ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float3 world_pos:TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.world_pos=mul(unity_ObjectToWorld,v.vertex).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target 
            {
                //if offset_pos and clip_normal is not same side , dot operation will <0, and need clip.
                float3 offset_pos=i.world_pos - clip_pos;
                clip(dot(offset_pos,clip_normal));

               SHADOW_CASTER_FRAGMENT(i)
                
            }
            ENDCG
         }
    }

    FallBack "Reflective/VertexLit"
}
