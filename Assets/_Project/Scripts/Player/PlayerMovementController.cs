using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Steering")]
    public float steerStrength = 4f;
    public PlayerBalanceController balanceController;

    [Header("Speed Coupling")]
    [Tooltip("Wenn aktiv, wird steerStrength mit RunSpeedManager.SteerMultiplier skaliert: schneller fahren = stärkere/weitere Seitwärtsbewegung bei gleicher Neigung, langsamer fahren = schwächere.")]
    public bool scaleWithSpeed = true;

    [Header("Grenzen (Straßenrand = Wand)")]
    [Tooltip("Maximale seitliche Auslenkung. Der Spieler wird hier gestoppt – DAS ist praktisch die Wand. " +
             "Wird er über diesen Wert hinaus gedrückt, gilt er als 'an der Wand' (IsAgainstWall).")]
    public float maxX = 7.25f;

    /// <summary>True, solange der Spieler seitlich gegen den Rand (die Wand) gedrückt wird.</summary>
    public bool IsAgainstWall { get; private set; }
    /// <summary>Seite des Wandkontakts: -1 = links, +1 = rechts, 0 = kein Kontakt.</summary>
    public int WallSide { get; private set; }

    private float initialZ;
    private float externalPushX; // von Effekten (z.B. Hickup) gesetzter seitlicher Stoß, pro Frame

    private void Awake()
    {
        if (balanceController == null)
        {
            balanceController = GetComponent<PlayerBalanceController>();
        }

        initialZ = transform.position.z;
    }

    /// <summary>
    /// Von Effekten aufgerufen, um dem Spieler zusätzlich einen seitlichen Stoß zu geben
    /// (Einheit: Geschwindigkeit in X, wird mit deltaTime verrechnet). Additiv pro Frame.
    /// </summary>
    public void AddPush(float velocityX) => externalPushX += velocityX;

    private void Update()
    {
        // 1. Steering (Lean translates to sideways movement) - Move in World Space to ignore tilt
        if (balanceController != null)
        {
            float speedFactor = (scaleWithSpeed && RunSpeedManager.Instance != null) ? RunSpeedManager.Instance.SteerMultiplier : 1f;
            // SteerOutput (statt BalanceAngle): der über die Response-Kurve geformte Lenkwert. So passen
            // Neigung/Eindrehen (PlayerBalanceController) und diese Seitwärtsbewegung immer zusammen.
            transform.Translate(Vector3.right * balanceController.SteerOutput * steerStrength * speedFactor * Time.deltaTime, Space.World);
        }

        // 1b. Externer Stoß (z.B. Hickup) – überlagert die normale Lenkung.
        if (externalPushX != 0f)
        {
            transform.Translate(Vector3.right * externalPushX * Time.deltaTime, Space.World);
            externalPushX = 0f;
        }

        // 2. Wandkontakt erkennen + X begrenzen.
        //    Der Clamp hält den Spieler am Rand (±maxX), d.h. er erreicht die Wand-Collider nie –
        //    DESHALB ist "an der Wand" eine Positions-Abfrage (Spieler wird über den Rand gedrückt),
        //    kein Trigger-Kontakt. Das ist unabhängig von Collider-Overlap und Spielerhöhe.
        Vector3 pos = transform.position;

        if (pos.x > maxX) { IsAgainstWall = true; WallSide = 1; }
        else if (pos.x < -maxX) { IsAgainstWall = true; WallSide = -1; }
        else { IsAgainstWall = false; WallSide = 0; }

        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.z = initialZ;
        transform.position = pos;
    }
}
