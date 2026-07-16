using UnityEngine;

/// <summary>
/// Lässt ein UI-Element sanft hin- und herschaukeln (z.B. das "Don't Drink and Drive"-Logo).
/// Rein visuell: verändert nur die lokale Rotation um Z. Einfach auf das Objekt legen.
/// </summary>
public class UISwing : MonoBehaviour
{
    [Tooltip("Maximaler Ausschlag nach links/rechts in Grad.")]
    public float swingAngle = 6f;
    [Tooltip("Schaukel-Tempo. Höher = schneller.")]
    public float swingSpeed = 2f;
    [Tooltip("Phasen-Versatz in Sekunden, damit mehrere schaukelnde Objekte nicht synchron laufen.")]
    public float phaseOffset = 0f;

    private Quaternion baseRotation;

    private void Awake()
    {
        baseRotation = transform.localRotation;
    }

    private void Update()
    {
        // unscaledTime, damit es auch bei pausiertem Spiel (Time.timeScale = 0) im Menü läuft.
        float angle = Mathf.Sin((Time.unscaledTime + phaseOffset) * swingSpeed) * swingAngle;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        transform.localRotation = baseRotation;
    }
}
