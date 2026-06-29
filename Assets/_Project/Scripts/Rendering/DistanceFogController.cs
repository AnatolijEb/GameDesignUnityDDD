using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class DistanceFogController : MonoBehaviour
{
    [SerializeField] private bool enableFog = true;
    [SerializeField] private FogMode fogMode = FogMode.Linear;
    [SerializeField] private Color fogColor = new(0.86f, 0.68f, 0.56f, 1f);
    [SerializeField, Min(0f)] private float linearStart = 25f;
    [SerializeField, Min(0.01f)] private float linearEnd = 80f;
    [SerializeField, Min(0f)] private float density = 0.04f;
    [SerializeField] private bool applyContinuously;

    private void OnEnable()
    {
        ApplyFogSettings();
    }

    private void OnValidate()
    {
        if (linearEnd <= linearStart)
        {
            linearEnd = linearStart + 0.01f;
        }

        ApplyFogSettings();
    }

    private void Update()
    {
        if (applyContinuously)
        {
            ApplyFogSettings();
        }
    }

    private void ApplyFogSettings()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = linearStart;
        RenderSettings.fogEndDistance = linearEnd;
        RenderSettings.fogDensity = density;
    }
}
