using UnityEngine;

/// <summary>Kleine gemeinsame Helfer für Effekte (damit Formeln nicht dupliziert werden).</summary>
public static class PlayerEffectUtil
{
    /// <summary>
    /// Symmetrischer Sprungbogen (Parabel): 0 bei t=0, Maximum (=height) bei t=0.5,
    /// wieder 0 bei t=1. Ideal für "hoch springen und zurück auf den Boden fallen".
    /// </summary>
    public static float JumpArc(float t, float height)
    {
        t = Mathf.Clamp01(t);
        return height * 4f * t * (1f - t);
    }
}
