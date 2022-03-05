Shader "Custom/Stencil/Target"{
    
    Properties{
        _ID("Mask ID",int) = 0
        _BaseColor ("Color", Color) = (1, 1, 1, 1) 
        _MainTex ("MainTex", 2D) = "white"{}
    }
    SubShader{

        Tags { 
                "LightMode" = "UniversalForward"
                "RenderType"="Transparent"
                "Queue"="Transparent"
                "IgnoreProjector"="True"
               } 
       
        Pass{       
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off
            Stencil{
                 Ref[_ID]
                 Comp Equal
                 pass Keep
            }
            HLSLPROGRAM
     
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4 _BaseColor; 
            Texture2D _MainTex;
            SAMPLER(sampler_MainTex);


            struct a2v {
                float4 positionOS : POSITION;
                float2 uv:TEXCOORD0;
            };
            struct v2f{
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
            };
        
            v2f vert(a2v v){
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv=v.uv;
                return o;
            }
           
            half4 frag(v2f i) : SV_Target {
                half4 mainColor=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                mainColor*=half4(_BaseColor.rgb,1);
                return mainColor;
            }           
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}