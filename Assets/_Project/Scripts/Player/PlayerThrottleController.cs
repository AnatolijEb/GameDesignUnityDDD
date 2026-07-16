using UnityEngine;

public class PlayerThrottleController : MonoBehaviour
{
    [Header("Throttle Settings")]
    [Tooltip("Wie schnell der Throttle-Wert der Vor-/Zurück-Eingabe folgt. Höher = direkter, niedriger = träger/weicher.")]
    public float throttleResponsiveness = 3f;

    [Header("Gas/Bremse Zeit-Limit")]
    [Tooltip("Zeit-Limit aktivieren. Aus = Gas/Bremse unbegrenzt (wie früher).")]
    public bool enableThrottleTimeLimit = true;
    [Tooltip("Wie lange man am Stück schneller/langsamer fahren darf, bevor es automatisch auf " +
             "Basis-Geschwindigkeit zurückgeht (Sekunden). Der Zähler startet beim ersten Gas/Bremse-Impuls.")]
    public float maxThrottleDuration = 3f;
    [Tooltip("Puffer/Pause nach Ablauf, bevor man wieder Gas/Bremse geben kann (Sekunden). " +
             "In dieser Zeit bleibt man auf Basis-Geschwindigkeit, egal was gedrückt wird.")]
    public float throttleCooldown = 3f;

    [Header("Effekte")]
    [Tooltip("Vorzeichen des Throttle. Effekte (z.B. Steuerungs-Twist) setzen dies auf -1, um vor/zurück umzukehren. Nicht im Inspector ändern.")]
    public float throttleSign = 1f;
    [Tooltip("Zähler für gesperrtes Gas/Bremse. Effekte (z.B. Sekundenschlaf) erhöhen ihn um 1 und senken ihn beim Ende wieder. >0 = Gas/Bremse gesperrt. Nicht im Inspector ändern.")]
    public int controlLockCount = 0;

    private float throttle = 0f;

    // Zustandsautomat für das Zeit-Limit:
    //   Ready    = bereit, Gas/Bremse würfelt beim ersten Impuls die aktive Phase an
    //   Active   = darf beschleunigen/bremsen; läuft maxThrottleDuration Sekunden
    //   Cooldown = gesperrt (Basis-Geschwindigkeit); läuft throttleCooldown Sekunden
    private enum ThrottlePhase { Ready, Active, Cooldown }
    private ThrottlePhase phase = ThrottlePhase.Ready;
    private float phaseTimer; // Restzeit der laufenden Active-/Cooldown-Phase

    // -1 (volles Zurücklehnen/Bremsen) .. 0 (neutral) .. 1 (volles Vorlehnen/Beschleunigen)
    public float Throttle => throttle;

    // Für HUD/Debug: läuft gerade der Puffer? Wie viel Zeit ist noch übrig?
    public bool IsOnCooldown => phase == ThrottlePhase.Cooldown;
    public bool IsActivePhase => phase == ThrottlePhase.Active;
    public float PhaseTimeRemaining => phaseTimer;

    private void Update()
    {
        // Bei gesperrter Steuerung (z.B. Sekundenschlaf) zählt keine Eingabe -> das Gas ebbt weich gegen 0 ab.
        float input = (controlLockCount > 0) ? 0f : Input.GetAxis("Vertical") * throttleSign;

        // Zeit-Limit: Gas/Bremse nur eine Weile, dann zurück auf Basis + Puffer.
        if (enableThrottleTimeLimit)
        {
            input = ApplyTimeLimit(input);
        }
        else if (phase != ThrottlePhase.Ready)
        {
            // Feature nachträglich abgeschaltet -> sauber zurücksetzen.
            phase = ThrottlePhase.Ready;
            phaseTimer = 0f;
        }

        throttle = Mathf.MoveTowards(throttle, input, throttleResponsiveness * Time.deltaTime);
    }

    /// <summary>
    /// Begrenzt die Gas/Bremse-Eingabe zeitlich. Gibt den (ggf. auf 0 gesperrten) Eingabewert zurück.
    /// Ablauf: erster Impuls -> Active (maxThrottleDuration), danach Cooldown (throttleCooldown, gesperrt),
    /// danach wieder Ready. Während Active zählt es hoch- oder runterfahren gleichermaßen als "aktiv".
    /// </summary>
    private float ApplyTimeLimit(float input)
    {
        bool wantsThrottle = Mathf.Abs(input) > 0.01f;
        float dt = Time.deltaTime;

        switch (phase)
        {
            case ThrottlePhase.Ready:
                if (wantsThrottle)
                {
                    phase = ThrottlePhase.Active;
                    phaseTimer = maxThrottleDuration;
                }
                return input; // im selben Frame schon erlaubt

            case ThrottlePhase.Active:
                phaseTimer -= dt;
                if (phaseTimer <= 0f)
                {
                    // Aktive Zeit aufgebraucht -> zurück auf Basis + Puffer starten.
                    phase = ThrottlePhase.Cooldown;
                    phaseTimer = throttleCooldown;
                    return 0f;
                }
                return input;

            case ThrottlePhase.Cooldown:
                phaseTimer -= dt;
                if (phaseTimer <= 0f)
                {
                    phase = ThrottlePhase.Ready;
                    phaseTimer = 0f;
                }
                return 0f; // während des Puffers gesperrt -> Basis-Geschwindigkeit

            default:
                return input;
        }
    }
}
