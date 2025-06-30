Shader "Unlit/BackgroundTest"
{
Properties
    {
        _Speed ("Speed", Range(0.1, 10)) = 1.0
        _Frequency ("Frequency", Range(1, 512)) = 10.0
        _ColorAmount ("Color Amount", Range(1, 20)) = 5.0
        _RotationSpeed ("Rotation Speed", Range(-5, 5)) = 0.5
        _MaxSaturation ("Max Saturation", Range(0, 1)) = 1.0
        _Brightness ("Brightness", Range(0, 1)) = 0.4 // Added property to control darkness
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                // uv is no longer needed for the pattern
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD0; // Pass world position from vertex to fragment
            };

            float _Speed;
            float _Frequency;
            float _ColorAmount;
            float _RotationSpeed;
            float _MaxSaturation;
            float _Brightness; // Brightness property

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Calculate and pass the world space position of the vertex
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use the xy components of the world position for the pattern
                float2 worldCoords = i.worldPos.xy;

                float rotationAngle = _Time.y * _RotationSpeed;
                float s = sin(rotationAngle);
                float c = cos(rotationAngle);
                
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                // Rotate the world coordinates around the world origin (0,0)
                worldCoords = mul(rotationMatrix, worldCoords);

                float time = _Time.y * _Speed;
                // The pattern is now based on world coordinates, so it won't move with the object
                float pattern = sin((cos(worldCoords.x * _Frequency) + sin(worldCoords.y * _Frequency)) + time);

                float r = sin(pattern * _ColorAmount + time * 0.5) * 0.5 + 0.5;
                float g = sin(pattern * _ColorAmount + time * 0.6) * 0.5 + 0.5;
                float b = sin(pattern * _ColorAmount + time * 0.7) * 0.5 + 0.5;

                fixed3 fullColor = fixed3(r, g, b);
                float luminance = dot(fullColor, fixed3(0.299, 0.587, 0.114));
                fixed3 grayscale = fixed3(luminance, luminance, luminance);
                fixed3 finalColor = lerp(grayscale, fullColor, _MaxSaturation);
                
                // Multiply the final color by the brightness to make it darker
                finalColor *= _Brightness; 
                
                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
