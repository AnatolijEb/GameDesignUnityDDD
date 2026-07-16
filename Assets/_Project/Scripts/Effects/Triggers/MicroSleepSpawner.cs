using System.Collections;
using UnityEngine;

/// <summary>
/// Auslöser für den Sekundenschlaf (Micro Sleep). In zufälligen Abständen lässt er den
/// Charakter kurz einschlafen (Steuerung gesperrt + ZZZ über dem Kopf, siehe
/// <see cref="MicroSleepEffectSO"/>). Auf dem Player-Root (oder einem Manager) platzieren.
///
/// Besonderheit: Der Sekundenschlaf kommt HÄUFIGER, wenn man NÜCHTERN ist – d.h. wenn der
/// Drunkenness-Multiplikator (Score) UNTER dem Schwellwert liegt (müde statt betrunken).
///
/// Alle drei vom Benutzer gewünschten Stellschrauben:
///   • Wie lange geschlafen wird  -> am Effekt-Asset (<see cref="MicroSleepEffectSO.sleepDuration"/>)
///   • Wie häufig / wie weit auseinander -> <see cref="minInterval"/> / <see cref="maxInterval"/>
///   • „Häufiger wenn nüchtern"    -> <see cref="soberThreshold"/> / <see cref="soberFrequencyMultiplier"/>
/// </summary>
public class MicroSleepSpawner : MonoBehaviour
{
    [Header("Effekt")]
    [Tooltip("Das Sekundenschlaf-Asset (SO_Effect_MicroSleep). Die Schlaf-DAUER wird dort eingestellt.")]
    public PlayerEffectSO microSleepEffect;

    [Header("Abstand zwischen zwei Einschlaf-Effekten")]
    [Tooltip("Minimaler Abstand bis zum nächsten Einschlafen (Sekunden).")]
    public float minInterval = 10f;
    [Tooltip("Maximaler Abstand bis zum nächsten Einschlafen (Sekunden).")]
    public float maxInterval = 25f;
    [Tooltip("Schonzeit am Anfang, in der noch nichts passiert (Sekunden).")]
    public float startGracePeriod = 8f;

    [Header("Häufiger im nüchternen Zustand")]
    [Tooltip("Wenn aktiv: Unter dem Schwellwert (nüchtern) kommt der Sekundenschlaf häufiger.")]
    public bool moreFrequentWhenSober = true;
    [Tooltip("Drunkenness-Multiplikator (Score), UNTER dem man häufiger einschläft. " +
             "Standard 2 = nur bei 1x (nüchtern) häufiger.")]
    public int soberThreshold = 2;
    [Min(1f)]
    [Tooltip("Wie viel häufiger im nüchternen Zustand. 3 = dreimal so oft (Abstand wird /3 gerechnet).")]
    public float soberFrequencyMultiplier = 3f;

    private void Start()
    {
        StartCoroutine(SleepLoop());
    }

    private IEnumerator SleepLoop()
    {
        yield return new WaitForSeconds(startGracePeriod);

        while (true)
        {
            // Nüchtern -> kürzerer Abstand -> häufigeres Einschlafen.
            float wait = Random.Range(minInterval, maxInterval) / CurrentFrequencyScale();
            yield return new WaitForSeconds(wait);
            TriggerSleep();
        }
    }

    private float CurrentFrequencyScale()
    {
        if (!moreFrequentWhenSober || DrunkennessSystem.Instance == null) return 1f;

        bool sober = DrunkennessSystem.Instance.CurrentMultiplier < soberThreshold;
        return sober ? Mathf.Max(1f, soberFrequencyMultiplier) : 1f;
    }

    private void TriggerSleep()
    {
        if (PlayerEffectController.Instance != null && microSleepEffect != null)
        {
            PlayerEffectController.Instance.Apply(microSleepEffect);
        }
    }
}
