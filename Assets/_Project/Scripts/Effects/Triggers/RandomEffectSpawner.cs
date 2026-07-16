using System.Collections;
using UnityEngine;

/// <summary>
/// Auslöser #1: Löst in zufälligen Abständen einen Effekt aus (z.B. den Hickup).
/// Auf dem Player-Root (oder einem Manager) platzieren.
///
/// Häufigkeit: Der Abstand zwischen zwei Effekten hängt am Drunkenness-Multiplikator und wird
/// über <see cref="PlayerEffectUtil.RandomEffectInterval"/> berechnet – wahrscheinlichkeitsbasiert
/// (gestreut), nicht auf einen festen Takt festgenagelt.
///
/// Kein "hintereinander": Es wird nur ausgelöst, wenn gerade KEIN Effekt läuft. Dadurch überlappen
/// sich Effekte nicht (auch nicht mit dem Sekundenschlaf). Der Abstand zählt als Ruhepause zwischen
/// zwei Effekten.
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

    [Header("Häufigkeit (Ruhepause zwischen zwei Effekten)")]
    [Tooltip("Ø Ruhepause im SELTENEN Zustand (Sekunden). Standard: nüchtern.")]
    public float intervalRare = 10f;
    [Tooltip("Ø Ruhepause im HÄUFIGEN Zustand (Sekunden, kürzer). Standard: max. Rausch.")]
    public float intervalFrequent = 6.5f;
    [Range(0f, 1f)]
    [Tooltip("Streuung um den Durchschnitt (0 = fester Takt, 0.35 = ±35 %).")]
    public float randomness = 0.35f;
    [Tooltip("true = betrunken -> häufiger (Standard). false = nüchtern -> häufiger.")]
    public bool frequentWhenDrunk = true;
    [Tooltip("Schonzeit am Anfang, in der noch nichts passiert (Sekunden).")]
    public float startGracePeriod = 8f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(startGracePeriod);

        while (true)
        {
            // Warten, bis kein Effekt mehr läuft (kein Overlap / kein "hintereinander").
            yield return new WaitUntil(() => !EffectActive());

            // Ruhepause bis zum nächsten Effekt.
            yield return new WaitForSeconds(NextInterval());

            // Falls in der Ruhepause doch ein Effekt begonnen hat: neu ansetzen.
            if (EffectActive()) continue;

            TriggerOne();
        }
    }

    private static bool EffectActive()
    {
        return PlayerEffectController.Instance != null && PlayerEffectController.Instance.HasActiveEffect;
    }

    private float NextInterval()
    {
        return PlayerEffectUtil.RandomEffectInterval(intervalRare, intervalFrequent, randomness, frequentWhenDrunk);
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
