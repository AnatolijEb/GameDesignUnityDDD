using UnityEngine;

public class PlayerThrottleController : MonoBehaviour
{
    [Header("Throttle Settings")]
    [Tooltip("Wie schnell der Throttle-Wert der Vor-/Zurück-Eingabe folgt. Höher = direkter, niedriger = träger/weicher.")]
    public float throttleResponsiveness = 3f;

    private float throttle = 0f;

    // -1 (volles Zurücklehnen/Bremsen) .. 0 (neutral) .. 1 (volles Vorlehnen/Beschleunigen)
    public float Throttle => throttle;

    private void Update()
    {
        float input = Input.GetAxisRaw("Vertical");
        throttle = Mathf.MoveTowards(throttle, input, throttleResponsiveness * Time.deltaTime);
    }
}
