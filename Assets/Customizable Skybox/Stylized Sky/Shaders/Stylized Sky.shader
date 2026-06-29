Shader "Stylized/Sky"
{
    Properties
    {
        [Header(Sky Transform)]
        _Rotation ("Rotation", Range(0, 360)) = 0
        _Pitch ("Pitch", Range(-45, 45)) = 0
        _HorizonOffset ("Horizon Offset", Range(-1, 1)) = 0

        [Header(Sun Disc)]
        _SunDiscColor ("Color", Color) = (1, 1, 1, 1)
        _SunDiscMultiplier ("Multiplier", float) = 25
        _SunDiscExponent ("Exponent", float) = 125000

        [Header(Sun Halo)]
        _SunHaloColor ("Color", Color) = (0.8970588, 0.7760561, 0.6661981, 1)
        _SunHaloExponent ("Exponent", float) = 125
        _SunHaloContribution ("Contribution", Range(0, 1)) = 0.75

        [Header(Horizon Line)]
        _HorizonLineColor ("Color", Color) = (0.9044118, 0.8872592, 0.7913603, 1)
        _HorizonLineExponent ("Exponent", float) = 4
        _HorizonLineContribution ("Contribution", Range(0, 1)) = 0.25
        
        [Header(Sky Gradient)]
        _SkyGradientTop ("Top", Color) = (0.172549, 0.5686274, 0.6941177, 1)
        _SkyGradientBottom ("Bottom", Color) = (0.764706, 0.8156863, 0.8509805)
        _SkyGradientExponent ("Exponent", float) = 2.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            float3 _SunDiscColor;
            float _SunDiscExponent;
            float _SunDiscMultiplier;

            float3 _SunHaloColor;
            float _SunHaloExponent;
            float _SunHaloContribution;

            float3 _HorizonLineColor;
            float _HorizonLineExponent;
            float _HorizonLineContribution;

            float3 _SkyGradientTop;
            float3 _SkyGradientBottom;
            float _SkyGradientExponent;
            float _Rotation;
            float _Pitch;
            float _HorizonOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float3 RotateAroundYInDegrees(float3 direction, float degrees)
            {
                float radians = degrees * UNITY_PI / 180.0;
                float sine;
                float cosine;
                sincos(radians, sine, cosine);
                return float3(
                    direction.x * cosine - direction.z * sine,
                    direction.y,
                    direction.x * sine + direction.z * cosine
                );
            }

            float3 RotateAroundXInDegrees(float3 direction, float degrees)
            {
                float radians = degrees * UNITY_PI / 180.0;
                float sine;
                float cosine;
                sincos(radians, sine, cosine);
                return float3(
                    direction.x,
                    direction.y * cosine - direction.z * sine,
                    direction.y * sine + direction.z * cosine
                );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDirection = normalize(i.worldPosition);
                float3 skyDirection = RotateAroundYInDegrees(viewDirection, _Rotation);
                skyDirection = RotateAroundXInDegrees(skyDirection, _Pitch);

                // Masks.
                float maskHorizon = dot(skyDirection, float3(0, 1, 0)) + _HorizonOffset;
                float maskSunDir = dot(viewDirection, _WorldSpaceLightPos0.xyz);

                // Sun disc.
                float maskSun = pow(saturate(maskSunDir), _SunDiscExponent);
                maskSun = saturate(maskSun * _SunDiscMultiplier);

                // Sun halo.
                float3 sunHaloColor = _SunHaloColor * _SunHaloContribution;
                float bellCurve = pow(saturate(maskSunDir), _SunHaloExponent * saturate(abs(maskHorizon)));
                float horizonSoften = 1 - pow(1 - saturate(maskHorizon), 50);
                sunHaloColor *= saturate(bellCurve * horizonSoften);

                // Horizon line.
                float3 horizonLineColor = _HorizonLineColor * saturate(pow(1 - abs(maskHorizon), _HorizonLineExponent));
                horizonLineColor = lerp(0, horizonLineColor, _HorizonLineContribution);

                // Sky gradient.
                float3 skyGradientColor = lerp(_SkyGradientTop, _SkyGradientBottom, pow(1 - saturate(maskHorizon), _SkyGradientExponent));

                float3 finalColor = lerp(saturate(sunHaloColor + horizonLineColor + skyGradientColor), _SunDiscColor, maskSun);
                return float4(finalColor, 1);
            }
            ENDCG
        }
    }
}
