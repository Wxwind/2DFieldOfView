Shader "Custom/Stencil/Gray"{
    
    Properties{
        _ID("Mask ID",int) = 1
        _BaseColor ("BaseColor", Color) = (1, 1, 1, 1) 
        _MainTex ("MainTex", 2D) = "white"{}
    }
    SubShader{

         Tags { 
            "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
        } 
       
        Pass{
        
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off 
            Stencil{
                 Ref[_ID]
                 Comp Equal
                 pass Keep
            }
            
            HLSLPROGRAM
     
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
                half3 mainColor=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half grey = dot(_BaseColor,half3(0.22, 0.707, 0.071));
                return half4(grey,grey,grey,_BaseColor.a);
            }           
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}