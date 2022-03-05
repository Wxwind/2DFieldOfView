Shader "Custom/Stencil/Base"{
    
    Properties{
        //1 means no light(=gray),0 rendered by foa means no gray
        _ID("Mask ID",int)=1
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
            Cull Off
            Lighting Off
            ColorMask 0
            Stencil{
                 Ref[_ID]
                 Comp Always
                 pass Replace
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
            };
            struct v2f{
                float4 positionCS : SV_POSITION;
            };
        
            v2f vert(a2v v){
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }
           
            half4 frag(v2f i) : SV_Target {
                return half4(1,1,1,1);
            }           
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}