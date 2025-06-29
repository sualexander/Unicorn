Shader "Unlit/BackgroundTest"
{
    Properties
    {
        // Controls the speed of the pattern's animation
        _Speed ("Speed", Range(0.1, 10)) = 1.0
        // Controls the tiling/frequency of the pattern
        _Frequency ("Frequency", Range(1, 100)) = 10.0
        // Affects the complexity and vibrancy of the colors
        _ColorAmount ("Color Amount", Range(1, 20)) = 5.0
        // NEW: Controls the rotation speed of the entire effect
        _RotationSpeed ("Rotation Speed", Range(-5, 5)) = 0.5
        // NEW: Controls the color saturation. 0 is grayscale, 1 is normal, >1 is oversaturated.
        _MaxSaturation ("Max Saturation", Range(0, 1)) = 1.0
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Declare the properties we defined above
            float _Speed;
            float _Frequency;
            float _ColorAmount;
            // NEW: Declare the new properties
            float _RotationSpeed;
            float _MaxSaturation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- UV Manipulation ---
                // Center the UV coordinates to make the pattern radiate from the middle
                float2 centeredUV = i.uv - 0.5;

                // --- Rotation ---
                // Calculate a rotation angle that changes over time based on our property
                float rotationAngle = _Time.y * _RotationSpeed;
                float s = sin(rotationAngle);
                float c = cos(rotationAngle);
                
                // Create a 2x2 rotation matrix
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                
                // Apply the rotation to our centered UVs. All subsequent calculations
                // will now be based on these rotated coordinates.
                centeredUV = mul(rotationMatrix, centeredUV);

                // Animate the pattern over time using the built-in _Time variable and our _Speed property
                float time = _Time.y * _Speed;

                // --- Pattern Generation ---
                // This is the core of the effect. We use a combination of trigonometric
                // functions (sin, cos) on the now-rotated UV coordinates and time to create a complex pattern.
                float pattern = sin(
                                    (cos(centeredUV.x * _Frequency) + sin(centeredUV.y * _Frequency)) + time
                                  );

                // --- Color Generation ---
                // We use the generated pattern value to create colors.
                float r = sin(pattern * _ColorAmount + time * 0.5) * 0.5 + 0.5;
                float g = sin(pattern * _ColorAmount + time * 0.6) * 0.5 + 0.5;
                float b = sin(pattern * _ColorAmount + time * 0.7) * 0.5 + 0.5;

                // Create the initial full color from the R, G, B channels
                fixed3 fullColor = fixed3(r, g, b);

                // --- Saturation Control ---
                // To control saturation, we blend between the full color and its grayscale equivalent.
                // First, calculate the luminance (brightness) of the color using standard coefficients.
                float luminance = dot(fullColor, fixed3(0.299, 0.587, 0.114));
                fixed3 grayscale = fixed3(luminance, luminance, luminance);

                // Use lerp to interpolate between grayscale (saturation=0) and the full color (saturation=1).
                // We allow _MaxSaturation to go above 1 for an oversaturated effect.
                fixed3 finalColor = lerp(grayscale, fullColor, _MaxSaturation);
                
                // Create the final color output
                fixed4 col = fixed4(finalColor, 1.0);

                return col;
            }
            ENDCG
        }
    }
}
