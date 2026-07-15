using System.Collections;
using UnityEngine;

/// <summary>
/// Auslöser #1: Löst in zufälligen Abständen einen Effekt aus (z.B. das Hickup).
/// Auf dem Player-Root (oder einem Manager) platzieren.
///
/// Weitere Zufalls-Effekte hinzufügen = einfach der gewichteten Liste ein Asset ergänzen.
/// </summary>
public class RandomEffectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public PlayerEffectSO effect;
        [Min(0f), Tooltip("Relatives Gewicht (nicht Prozent). Höher = wird häufiger gewählt.")]
        public float weight = 1f;
    }

    [Header("Effekte (gewichtete Zufallsauswahl)")]
    public Entry[] effects;

    [Header("Timing")]
    [Tooltip("Minimaler Abstand zwischen zwei Effekten (Sekunden) bei nüchternem Zustand.")]
    public float minInterval = 6f;
    [Tooltip("Maximaler Abstand zwischen zwei Effekten (Sekunden) bei nüchternem Zustand.")]
    public float maxInterval = 14f;
    [Tooltip("Schonzeit am Anfang, in der noch nichts passiert (Sekunden).")]
    public float startGracePeriod = 8f;

    [Header("Drunkenness-Kopplung (optional)")]
    [Tooltip("Wenn aktiv, kommen die Effekte bei höherem Drunkenness-Multiplikator häufiger.")]
    public bool scaleFrequencyWithDrunkenness = true;
    [Tooltip("Wie viel häufiger die Effekte beim maximalen Rausch (6x) auftreten. 3 = dreimal so oft wie nüchtern.")]
    public float frequencyAtMaxDrunkenness = 3f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(startGracePeriod);

        while (true)
        {
            // Höhere Drunkenness -> kürzere Wartezeit -> häufigere Hickups.
            float wait = Random.Range(minInterval, maxInterval) / CurrentFrequencyScale();
            yield return new WaitForSeconds(wait);
            TriggerOne();
        }
    }

    private float CurrentFrequencyScale()
    {
        if (!scaleFrequencyWithDrunkenness || DrunkennessSystem.Instance == null) return 1f;

        // CurrentMultiplier ist 1..6 -> t 0..1 -> Skala zwischen 1x und frequencyAtMaxDrunkenness.
        float t = Mathf.InverseLerp(1f, 6f, DrunkennessSystem.Instance.CurrentMultiplier);
        return Mathf.Lerp(1f, Mathf.Max(1f, frequencyAtMaxDrunkenness), t);
    }

    private void TriggerOne()
    {
        if (PlayerEffectController.Instance == null || effects == null || effects.Length == 0) return;

        float total = 0f;
        foreach (Entry e in effects)
        {
            if (e != null && e.effect != null) total += Mathf.Max(0f, e.weight);
        }
        if (total <= 0f) return;

        float roll = Random.value * total;
        foreach (Entry e in effects)
        {
            if (e == null || e.effect == null) continue;
            float w = Mathf.Max(0f, e.weight);
            if (roll < w)
            {
                PlayerEffectController.Instance.Apply(e.effect);
                return;
            }
            roll -= w;
        }
    }
}
