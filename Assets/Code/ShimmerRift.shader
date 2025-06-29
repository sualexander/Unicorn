Shader "Unlit/ShimmerRift"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.05, 0.2, 1)
        _ShimmerColor ("Shimmer Color", Color) = (0.6, 0.3, 1, 1)
        _EdgeColor ("Edge Glow Color", Color) = (1, 0.5, 1, 1)
        
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 2.0
        _ScrollSpeed ("Shimmer Speed", Float) = 1.0
        
        _ShimmerIntensity ("Shimmer Intensity", Range(0, 2)) = 1.0
        _ShimmerFrequency ("Shimmer Frequency", Float) = 3.0
        
        _GradientPower ("Gradient Sharpness", Range(1, 10)) = 2.0
        _GradientOffset ("Gradient Center Offset", Range(-0.5, 0.5)) = 0.0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 noiseUV1 : TEXCOORD1;
                float2 noiseUV2 : TEXCOORD2;
            };
            
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            
            fixed4 _BaseColor;
            fixed4 _ShimmerColor;
            fixed4 _EdgeColor;
            
            float _NoiseScale;
            float _ScrollSpeed;
            float _ShimmerIntensity;
            float _ShimmerFrequency;
            float _GradientPower;
            float _GradientOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // Create two layers of noise UVs for more complex shimmer
                float time = _Time.y * _ScrollSpeed;
                o.noiseUV1 = v.uv * _NoiseScale + float2(time * 0.1, time);
                o.noiseUV2 = v.uv * _NoiseScale * 0.7 + float2(-time * 0.05, time * 0.8);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample noise textures
                float noise1 = tex2D(_NoiseTex, i.noiseUV1).r;
                float noise2 = tex2D(_NoiseTex, i.noiseUV2).r;
                
                // Combine noises for more interesting patterns
                float combinedNoise = (noise1 + noise2) * 0.5;
                
                // Add time-based pulsing to the shimmer
                float pulse = sin(_Time.y * _ShimmerFrequency) * 0.5 + 0.5;
                combinedNoise = combinedNoise * (0.7 + pulse * 0.3);
                
                // Calculate vertical gradient (0 at edges, 1 at center)
                float verticalPos = i.uv.y;
                float distFromCenter = abs(verticalPos - 0.5 - _GradientOffset) * 2.0;
                float gradient = 1.0 - pow(distFromCenter, _GradientPower);
                gradient = saturate(gradient);
                
                // Create edge glow effect
                float edgeGlow = pow(1.0 - gradient, 3.0);
                
                // Mix colors
                fixed4 baseColor = _BaseColor;
                fixed4 shimmer = _ShimmerColor * combinedNoise * _ShimmerIntensity;
                fixed4 edge = _EdgeColor * edgeGlow * 0.5;
                
                // Combine all colors
                fixed4 finalColor = baseColor + shimmer + edge;
                
                // Apply gradient to alpha
                finalColor.a = gradient * _BaseColor.a;
                
                // Boost alpha slightly in areas with strong shimmer
                finalColor.a += combinedNoise * 0.1 * gradient;
                finalColor.a = saturate(finalColor.a);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
