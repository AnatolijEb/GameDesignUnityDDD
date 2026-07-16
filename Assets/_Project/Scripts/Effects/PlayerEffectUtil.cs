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

    /// <summary>
    /// Statistischer Abstand (Sekunden) bis zum nächsten Zufalls-Effekt, gekoppelt an den
    /// Drunkenness-Multiplikator (1 = nüchtern .. 6 = max. Rausch).
    ///
    /// <paramref name="avgRare"/>  = Ø-Abstand im SELTENEN Zustand,
    /// <paramref name="avgFrequent"/> = Ø-Abstand im HÄUFIGEN Zustand (kürzer).
    /// <paramref name="frequentWhenDrunk"/> legt fest, welcher Trunkenheitsgrad "häufig" ist:
    ///   true  = betrunken -> häufig (Standard),
    ///   false = nüchtern  -> häufig.
    /// <paramref name="randomness"/> (0..1) streut den tatsächlichen Wert um den Durchschnitt
    /// (0 = immer exakt Ø, 0.35 = ±35 %), damit die Auslösung nicht auf einen festen Takt fällt.
    /// </summary>
    public static float RandomEffectInterval(float avgRare, float avgFrequent, float randomness, bool frequentWhenDrunk)
    {
        float mult = DrunkennessSystem.Instance != null ? DrunkennessSystem.Instance.CurrentMultiplier : 1f;
        float t = Mathf.InverseLerp(1f, 6f, mult);                 // 0 nüchtern .. 1 max. Rausch
        float frequentness = frequentWhenDrunk ? t : 1f - t;       // 1 = häufig (kurzer Abstand)
        float avg = Mathf.Lerp(avgRare, avgFrequent, frequentness);

        randomness = Mathf.Clamp01(randomness);
        return Mathf.Max(0.1f, avg * Random.Range(1f - randomness, 1f + randomness));
    }
}
