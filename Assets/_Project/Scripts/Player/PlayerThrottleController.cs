using UnityEngine;

public class PlayerThrottleController : MonoBehaviour
{
    [Header("Throttle Settings")]
    [Tooltip("Wie schnell der Throttle-Wert der Vor-/Zurück-Eingabe folgt. Höher = direkter, niedriger = träger/weicher.")]
    public float throttleResponsiveness = 3f;

    [Header("Effekte")]
    [Tooltip("Vorzeichen des Throttle. Effekte (z.B. Steuerungs-Twist) setzen dies auf -1, um vor/zurück umzukehren. Nicht im Inspector ändern.")]
    public float throttleSign = 1f;
    [Tooltip("Zähler für gesperrtes Gas/Bremse. Effekte (z.B. Sekundenschlaf) erhöhen ihn um 1 und senken ihn beim Ende wieder. >0 = Gas/Bremse gesperrt. Nicht im Inspector ändern.")]
    public int controlLockCount = 0;

    private float throttle = 0f;

    // -1 (volles Zurücklehnen/Bremsen) .. 0 (neutral) .. 1 (volles Vorlehnen/Beschleunigen)
    public float Throttle => throttle;

    private void Update()
    {
        // Bei gesperrter Steuerung (z.B. Sekundenschlaf) zählt keine Eingabe -> das Gas ebbt weich gegen 0 ab.
        float input = (controlLockCount > 0) ? 0f : Input.GetAxis("Vertical") * throttleSign;
        throttle = Mathf.MoveTowards(throttle, input, throttleResponsiveness * Time.deltaTime);
    }
}
