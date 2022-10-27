// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "Starter/ForwardRendering"
{
    Properties
    {
        _Diffuse("Diffuse",Color)=(1,1,1,1)
        _Specular("Specular",Color)=(1,1,1,1)
        _Gloss("Gloss",Range(8.0,256))=20

    }
    SubShader
    {
        Pass
        {
            Tags{"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            fixed4 _Diffuse;
            fixed4 _Specular;
            float _Gloss;

            struct a2v
            {
                float4 vertex :  POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world_pos: TEXCOORD0;
                float3 world_normal : TEXCOORD1;
                SHADOW_COORDS(2)//_ShadowCoord 变量  ，参数为下一个可用的TEXCOORD
            };


            v2f vert (a2v v)
            {
			 	v2f o;
			 	o.pos = UnityObjectToClipPos(v.vertex);
			 	
			 	o.world_normal = UnityObjectToWorldNormal(v.normal);

			 	o.world_pos = mul(unity_ObjectToWorld, v.vertex).xyz;
			 	
			 	// Pass shadow coordinates to pixel shader
			 	TRANSFER_SHADOW(o);
			 	
			 	return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 world_normal=normalize(i.world_normal);
                float3 light_dir=normalize(UnityWorldSpaceLightDir(i.world_pos));
                float3 view_dir=normalize(UnityWorldSpaceViewDir(i.world_pos));
                float3 half_dir=normalize(view_dir+light_dir);

                fixed3 light_color=_LightColor0.rgb;
                fixed3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
                fixed3 diffuse=light_color* _Diffuse.rgb*saturate(dot(world_normal,light_dir));
                fixed3 specular=light_color*_Specular.rgb*pow(saturate(dot(world_normal,half_dir)),_Gloss);
                
                // fixed atten=1.0;
                // fixed shadow = SHADOW_ATTENUATION(i);//用shadow map坐标采样shadow map
                UNITY_LIGHT_ATTENUATION(atten,i,i.world_pos);

                return fixed4(ambient+(diffuse+specular)*atten,1.0);
            }
            ENDCG
        }

        Pass
        {
            Tags{"LightMode"="ForwardAdd"}
            Blend One One
            CGPROGRAM

            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag

            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            fixed4 _Diffuse;
            fixed4 _Specular;
            float _Gloss;

            struct a2v
            {
                float4 vertex :  POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world_pos: TEXCOORD0;//注意通常用三维
                float3 world_normal : TEXCOORD1;
                SHADOW_COORDS(2)//_ShadowCoord 变量  ，参数为下一个可用的TEXCOORD

            };


            v2f vert (a2v v)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.world_normal=UnityObjectToWorldNormal(v.normal);
                o.world_pos=mul(unity_ObjectToWorld,v.vertex).xyz;
			 	TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 world_normal=normalize(i.world_normal);
                
                #ifdef USING_DIRECTIONAL_LIGHT//没有位置的平行光直接获得light_dir
                    fixed3 light_dir=normalize(_WorldSpaceLightPos0.xyz);
                #else//有位置的光源需要自行通过位置构造dir
                    fixed3 light_dir=normalize(_WorldSpaceLightPos0.xyz-i.world_pos.xyz);
                #endif

                float3 view_dir=normalize(UnityWorldSpaceViewDir(i.world_pos));
                float3 half_dir=normalize(view_dir+light_dir);

                fixed3 light_color=_LightColor0.rgb;
                fixed3 diffuse=light_color* _Diffuse.rgb*saturate(dot(world_normal,light_dir));
                fixed3 specular=light_color*_Specular.rgb*pow(saturate(dot(world_normal,half_dir)),_Gloss);

                // #ifdef USING_DIRECTIONAL_LIGHT
                //     fixed atten=1.0;
                // #else //预计算的光源衰减，通过texture查找
                //     float3 light_coord=mul(unity_WorldToLight,i.world_pos).xyz;//光源空间坐标系，以光源为中心0~1
                //     fixed atten=tex2D(_LightTexture0,dot(light_coord,light_coord).rr).UNITY_ATTEN_CHANNEL;
                // #endif

                UNITY_LIGHT_ATTENUATION(atten,i,i.world_pos); //world_pos必须是三维

                return fixed4((diffuse+specular)*atten,1.0);
            }

            ENDCG
        }

    }

    FallBack "Specular"  //借用Shadow Cast Pass，才能参与到shadow map 的计算，才能正确接收阴影

}
