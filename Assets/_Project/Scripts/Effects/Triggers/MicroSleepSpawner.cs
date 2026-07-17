using System.Collections;
using UnityEngine;

/// <summary>
/// Auslöser für den Sekundenschlaf (Micro Sleep). In zufälligen Abständen lässt er den
/// Charakter kurz einschlafen (Steuerung gesperrt + ZZZ über dem Kopf, siehe
/// <see cref="MicroSleepEffectSO"/>). Auf dem Player-Root (oder einem Manager) platzieren.
///
/// Häufigkeit: Wie beim <see cref="RandomEffectSpawner"/> hängt der Abstand am Drunkenness-
/// Multiplikator und wird über <see cref="PlayerEffectUtil.RandomEffectInterval"/> berechnet.
/// Über <see cref="frequentWhenDrunk"/> lässt sich die Richtung mit EINEM Haken umdrehen:
///   • true  (Standard) = betrunken -> häufiger,
///   • false            = nüchtern/"müde" -> häufiger.
///
/// Kein "hintereinander": Es wird nur ausgelöst, wenn gerade KEIN Effekt läuft.
///
/// Stellschrauben:
///   • Wie lange geschlafen wird -> am Effekt-Asset (<see cref="MicroSleepEffectSO.sleepDuration"/>)
///   • Wie häufig                -> <see cref="intervalRare"/> / <see cref="intervalFrequent"/>
///   • Richtung (drunk/nüchtern) -> <see cref="frequentWhenDrunk"/>
/// </summary>
public class MicroSleepSpawner : MonoBehaviour
{
    [Header("Effekt")]
    [Tooltip("Das Sekundenschlaf-Asset (SO_Effect_MicroSleep). Die Schlaf-DAUER wird dort eingestellt.")]
    public PlayerEffectSO microSleepEffect;

    [Header("Sicherheit")]
    [Tooltip("Kein Sekundenschlaf, wenn der Spieler weniger als so viele Leben hat. " +
             "2 = beim letzten Leben (1 Leben) NICHT mehr einschlafen – dadurch zu sterben fühlt sich unfair an.")]
    public int minLivesForSleep = 2;
    [Tooltip("Lebenssystem des Spielers. Leer lassen = wird automatisch gesucht.")]
    public PlayerLifeSystem lifeSystem;

    [Header("Häufigkeit (Ruhepause zwischen zwei Einschlaf-Effekten)")]
    [Tooltip("Ø Ruhepause im SELTENEN Zustand (Sekunden).")]
    public float intervalRare = 10f;
    [Tooltip("Ø Ruhepause im HÄUFIGEN Zustand (Sekunden, kürzer).")]
    public float intervalFrequent = 6.5f;
    [Range(0f, 1f)]
    [Tooltip("Streuung um den Durchschnitt (0 = fester Takt, 0.35 = ±35 %).")]
    public float randomness = 0.35f;
    [Tooltip("true = betrunken -> häufiger (Standard). false = nüchtern/'müde' -> häufiger. " +
             "Ein Haken dreht die ganze Häufigkeits-Richtung um.")]
    public bool frequentWhenDrunk = true;
    [Tooltip("Schonzeit am Anfang, in der noch nichts passiert (Sekunden).")]
    public float startGracePeriod = 8f;

    [Header("Drunkenness-Voraussetzung (harter Gate)")]
    [Tooltip("Der Sekundenschlaf kommt NUR, wenn der Drunkenness-Multiplikator (1..6) im Bereich [Min, Max] liegt – " +
             "sonst GAR NICHT (an/aus, nicht 'wahrscheinlicher'). Nickerchen-Standard: Max 2 (nur nüchtern).")]
    public int maxDrunkenness = 2;
    [Tooltip("Untere erlaubte Grenze des Multiplikators (inklusive). Standard 1 (Minimum).")]
    public int minDrunkenness = 1;

    private void Start()
    {
        if (lifeSystem == null) lifeSystem = GetComponent<PlayerLifeSystem>();
        if (lifeSystem == null) lifeSystem = Object.FindFirstObjectByType<PlayerLifeSystem>();

        StartCoroutine(SleepLoop());
    }

    private IEnumerator SleepLoop()
    {
        yield return new WaitForSeconds(startGracePeriod);

        while (true)
        {
            // Warten, bis kein Effekt mehr läuft (kein Overlap / kein "hintereinander").
            yield return new WaitUntil(() => !EffectActive());

            // Ruhepause bis zum nächsten Einschlafen.
            yield return new WaitForSeconds(NextInterval());

            // Falls in der Ruhepause doch ein Effekt begonnen hat: neu ansetzen.
            if (EffectActive()) continue;

            // Beim letzten Leben nicht einschlafen – dadurch zu sterben fühlt sich unfair an.
            if (lifeSystem != null && lifeSystem.CurrentLives < minLivesForSleep) continue;

            // Harter Drunkenness-Gate: nur im erlaubten Bereich (Standard: Multiplikator <= 2).
            if (!DrunkennessInRange()) continue;

            TriggerSleep();
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

    // Harter Gate: nur einschlafen, wenn der Drunkenness-Multiplikator im erlaubten Bereich liegt.
    // Ohne DrunkennessSystem in der Szene wird nicht geblockt.
    private bool DrunkennessInRange()
    {
        if (DrunkennessSystem.Instance == null) return true;
        int m = DrunkennessSystem.Instance.CurrentMultiplier;
        return m >= minDrunkenness && m <= maxDrunkenness;
    }

    private void TriggerSleep()
    {
        if (PlayerEffectController.Instance != null && microSleepEffect != null)
        {
            PlayerEffectController.Instance.Apply(microSleepEffect);
        }
    }
}
