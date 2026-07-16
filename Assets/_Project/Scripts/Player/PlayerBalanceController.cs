using UnityEngine;

// Muss NACH dem PlayerEffectController laufen: der setzt jeden Frame visualTarget.localPosition
// (Sprung-/Hop-Höhe). Der Wheelie dreht per RotateAround um das Hinterrad und verändert dabei
// AUCH die Position – liefe der Effect-Controller danach, würde er diese Kompensation überschreiben
// und das Heck säcke wieder in die Straße.
[DefaultExecutionOrder(100)]
public class PlayerBalanceController : MonoBehaviour
{
    // ===== Vom SPIELER gesteuert (Lenken) =====
    [Header("SPIELER-STEUERUNG (Lenken)")]
    [Tooltip("Wie schnell das Gewicht kippt (Reaktion auf Tastendruck). Höher = direkter/dynamischer.")]
    public float counterForce = 2.5f;
    [Tooltip("Wenn aktiv, wird die Lenkung mit der Fahrgeschwindigkeit skaliert: schneller fahren = schärfer lenken, langsamer = träger.")]
    public bool scaleWithSpeed = true;
    [Tooltip("Die Lenk-Kurve: formt, wie der Lenkeinschlag in die tatsächliche Lenkwirkung übersetzt wird.\n" +
             "x = Lenkeinschlag (0 = Mitte .. 1 = voll), y = Wirkung (0 .. 1).\n" +
             "Gerade Linie (Default) = wie bisher (linear). Flach->steil (Ease-In) = sanfte Mitte, " +
             "giftiges Ende (dynamischere Kurven). Wirkt auf Neigung, Eindrehen UND Seitwärtsbewegung " +
             "gemeinsam, damit Optik und Bewegung immer zusammenpassen.")]
    public AnimationCurve steerResponseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("Wie weit sich das Mofa zur Seite neigt (Schräglage in Grad) bei vollem Lenkeinschlag.")]
    public float maxTiltAngle = 30f;
    [Tooltip("Wie weit sich die Nase optisch in die Kurve eindreht (Yaw), abhängig vom Lenkeinschlag. " +
             "0 = kein Eindrehen, ~10 = dezent. Beide Richtungen automatisch. Rein visuell.")]
    public float maxTurnAngle = 10f;
    [Tooltip("Weiches Geraderichten nach einem Wand-Bounce: Dauer (Sekunden), in der die Neigung sanft " +
             "auf 0 gezogen wird. 0 = hart/sofort, ~0.15 = weich.")]
    public float wallResetDuration = 0.15f;

    [Space(14)]
    // ===== AUTOMATISCH (Betrunkenheit, ohne Spieler-Input) =====
    [Header("BETRUNKENES SCHWANKEN (automatisch, ohne Spieler)")]
    [Tooltip("Wie stark das Mofa von allein zur Seite zieht (betrunkenes Schwanken). Höher = stärkeres Driften.")]
    public float balanceDriftSpeed = 0.6f;
    [Tooltip("Minimale Zeit, bis die Schwank-Richtung neu gewürfelt wird (Sekunden).")]
    public float driftChangeMinTime = 1.5f;
    [Tooltip("Maximale Zeit, bis die Schwank-Richtung neu gewürfelt wird (Sekunden).")]
    public float driftChangeMaxTime = 3.5f;

    [Header("Visuals")]
    public Transform visualTarget;

    [Header("Effekte")]
    [Tooltip("Vorzeichen der Lenkung. Effekte (z.B. Steuerungs-Twist) setzen dies auf -1, um links/rechts umzukehren. Nicht im Inspector ändern.")]
    public float steeringSign = 1f;
    [Tooltip("Zähler für gesperrte Lenkung. Effekte (z.B. Sekundenschlaf) erhöhen ihn um 1 und senken ihn beim Ende wieder. >0 = Lenkung ist gesperrt. Nicht im Inspector ändern.")]
    public int controlLockCount = 0;

    [Header("Wheelie (nur visuell)")]
    [Tooltip("Wheelie aktivieren: Beim Vorwärts-Gas hebt sich das Vorderrad (Nase hoch). Rein visuell.")]
    public bool enableWheelie = true;
    [Tooltip("Maximaler Wheelie-Winkel in Grad (Vorderrad hoch) bei vollem Vorwärts-Gas.")]
    public float wheelieMaxAngle = 40f;
    [Tooltip("Wie schnell der Wheelie auf-/abgebaut wird (Grad pro Sekunde).")]
    public float wheelieChangeSpeed = 120f;
    [Tooltip("Throttle-Controller (liefert das Vorwärts-Gas). Leer = wird automatisch gesucht.")]
    public PlayerThrottleController throttleController;
    [Tooltip("Dreh-Pivot des Wheelies = Hinterrad (z.B. das 'Helper_BackWheelPosition'-Objekt im Mofa). " +
             "Leer lassen ist ok: wird beim Start automatisch anhand von 'Wheelie Pivot Name' gesucht.")]
    public Transform wheeliePivot;
    [Tooltip("Name des Hinterrad-Objekts, das automatisch als Wheelie-Pivot verwendet wird, solange " +
             "'Wheelie Pivot' leer ist. Steckt beim Vespa-Mofa im verschachtelten Prefab, daher Auto-Suche.")]
    public string wheeliePivotName = "Helper_BackWheelPosition";

    private float balanceAngle = 0f;
    private float driftDirection = 1f;
    private float nextDriftChange = 0f;
    private float currentWheelie = 0f;
    private float wallResetTimer = 0f; // >0 => läuft gerade das weiche Geraderichten nach einem Wand-Bounce

    public float BalanceAngle => balanceAngle;

    /// <summary>
    /// Geformter Lenkwert (-1..1): Vorzeichen von <see cref="balanceAngle"/>, Betrag durch
    /// <see cref="steerResponseCurve"/> geschickt. Neigung, Eindrehen (Yaw) und die Seitwärtsbewegung
    /// (PlayerMovementController) lesen ALLE diesen Wert, damit Optik und Bewegung immer zusammenpassen.
    /// Bei linearer Standard-Kurve identisch zu <see cref="BalanceAngle"/> (nichts ändert sich).
    /// </summary>
    public float SteerOutput
    {
        get
        {
            float sign = balanceAngle < 0f ? -1f : 1f;
            return sign * steerResponseCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(balanceAngle)));
        }
    }

    /// <summary>
    /// Stellt das Mofa wieder gerade (Neigung/Lenkzustand → 0). Wird z.B. beim Wand-Bounce aufgerufen,
    /// damit man direkt geradeaus weiterfahren kann und nicht erst die aufgebaute Neigung weglenken muss.
    /// Bei <see cref="wallResetDuration"/> &gt; 0 geschieht das WEICH (über die Dauer im Update),
    /// sonst sofort/hart.
    /// </summary>
    public void ResetBalance()
    {
        if (wallResetDuration <= 0f)
        {
            balanceAngle = 0f;              // hart: sofort gerade
        }
        else
        {
            wallResetTimer = wallResetDuration; // weich: wird im Update sanft auf 0 gezogen
        }
    }

    private void Start()
    {
        if (visualTarget == null) visualTarget = transform;
        if (throttleController == null) throttleController = GetComponent<PlayerThrottleController>();

        // Wheelie-Pivot automatisch finden (das Hinterrad steckt im verschachtelten Vespa-Prefab und
        // lässt sich im Inspector nicht immer zuweisen). Nur suchen, wenn nicht manuell gesetzt.
        if (wheeliePivot == null && !string.IsNullOrEmpty(wheeliePivotName))
        {
            Transform searchRoot = visualTarget != null ? visualTarget : transform;
            wheeliePivot = FindDeepChild(searchRoot, wheeliePivotName);
            if (wheeliePivot == null)
                Debug.LogWarning($"[Wheelie] Kein Objekt namens '{wheeliePivotName}' unter '{searchRoot.name}' " +
                                 "gefunden – Wheelie dreht ersatzweise um den Ursprung (Heck sinkt ein).");
        }

        // Initial drift change time
        nextDriftChange = Time.time + Random.Range(driftChangeMinTime, driftChangeMaxTime);
    }

    /// <summary>Rekursive Suche nach einem Kind-Transform mit exaktem Namen (auch tief verschachtelt).</summary>
    private static Transform FindDeepChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private void Update()
    {
        // Random Balance Drift
        if (Time.time > nextDriftChange)
        {
            driftDirection = (Random.value > 0.5f) ? 1f : -1f;
            nextDriftChange = Time.time + Random.Range(driftChangeMinTime, driftChangeMaxTime);
        }

        // Apply drift
        balanceAngle += driftDirection * balanceDriftSpeed * Time.deltaTime;

        // Player Input (Counter-force), an aktuelle Geschwindigkeit gekoppelt: schneller = schärfer, langsamer = träger
        float speedFactor = (scaleWithSpeed && RunSpeedManager.Instance != null) ? RunSpeedManager.Instance.SteerMultiplier : 1f;
        // Sekundenschlaf & Co. können die Lenkung komplett sperren (controlLockCount > 0): dann zählt keine
        // Spieler-Eingabe mehr. Der Zufalls-Drift oben läuft aber weiter -> das Mofa driftet unkontrolliert
        // (hilflos, wie eingeschlafen), man kann nicht gegenlenken.
        float input = (controlLockCount > 0) ? 0f : Input.GetAxis("Horizontal");
        balanceAngle += input * counterForce * speedFactor * steeringSign * Time.deltaTime;

        // Weiches Geraderichten nach einem Wand-Bounce: zieht die Neigung über wallResetDuration sanft
        // auf 0. Der Pull ist stark genug, um Drift/Eingabe kurz zu dominieren -> man wird gerade gestellt,
        // danach (Timer abgelaufen) hat man wieder volle Kontrolle.
        if (wallResetTimer > 0f)
        {
            wallResetTimer -= Time.deltaTime;
            float straightenSpeed = 1f / Mathf.Max(0.0001f, wallResetDuration); // volle Auslenkung in der Dauer
            balanceAngle = Mathf.MoveTowards(balanceAngle, 0f, straightenSpeed * Time.deltaTime);
        }

        // Clamp balanceAngle between -1 and 1
        balanceAngle = Mathf.Clamp(balanceAngle, -1f, 1f);

        // Visual Tilt (Rotation um Z) + optionale Effekt-Drehungen (Yaw um Y z.B. Öl-Dreher,
        // Pitch um X z.B. Purzelbaum bei Frontal-Kollision).
        float effectYaw = (PlayerEffectController.Instance != null) ? PlayerEffectController.Instance.VisualYaw : 0f;
        float effectPitch = (PlayerEffectController.Instance != null) ? PlayerEffectController.Instance.VisualPitch : 0f;

        // Neigung an die Blickrichtung koppeln: bei rückwärts gedrehtem Mofa (Yaw ~180°) würde die
        // lokal gesetzte Neigung aus Kamerasicht gespiegelt erscheinen. cos(Yaw) dreht sie zurück,
        // sodass die Neigung IMMER zur (weltbasierten) Seitwärtsbewegung passt. Vorwärts (0°): cos=1 -> unverändert.
        float tiltDirection = Mathf.Cos(effectYaw * Mathf.Deg2Rad);

        // Wheelie (nur visuell): bei Vorwärts-Gas das Vorderrad anheben (Nase hoch = negativer Pitch).
        // NUR wenn das Mofa nach vorne schaut. facingForward = clamp01(cos(Yaw)) ist 0, wenn das Mofa
        // rückwärts schaut (Öl-Pfütze) -> dort macht Vorwärts-Gas nur schneller, ABER keinen Wheelie.
        float wheelieTarget = 0f;
        if (enableWheelie && throttleController != null)
        {
            float forwardThrottle = Mathf.Max(0f, throttleController.Throttle); // nur Beschleunigen zählt
            float facingForward = Mathf.Clamp01(tiltDirection);
            wheelieTarget = forwardThrottle * wheelieMaxAngle * facingForward;
        }
        currentWheelie = Mathf.MoveTowards(currentWheelie, wheelieTarget, wheelieChangeSpeed * Time.deltaTime);

        // Geformter Lenkwert: Neigung, Eindrehen (Yaw) und Seitwärtsbewegung nutzen ALLE diesen Wert,
        // damit Optik und tatsächliche Bewegung immer zusammenpassen (siehe SteerOutput).
        float steer = SteerOutput;

        // Eindrehen in die Kurve (Yaw um die Hochachse): proportional zum Lenkeinschlag, ADDITIV zum
        // Effekt-Yaw (damit Öl-Dreher & Co. erhalten bleiben). Rechts fahren -> Nase dreht leicht nach rechts.
        float yaw = effectYaw + steer * maxTurnAngle;

        float roll = -steer * maxTiltAngle * tiltDirection;

        if (wheeliePivot != null)
        {
            // Basis-Rotation OHNE Wheelie: Purzelbaum (effectPitch), Yaw (Effekt + Eindrehen) und Neigung
            // drehen um den Objekt-Ursprung.
            visualTarget.rotation = Quaternion.Euler(effectPitch, yaw, roll);

            // Wheelie um den Hinterrad-Kontaktpunkt kippen, statt um den Ursprung. So bleibt das
            // Hinterrad auf der Straße und nur die Nase hebt sich. Nase hoch = negative Drehung um
            // die mofa-lokale Seitenachse (visualTarget.right nach der Basis-Rotation).
            if (!Mathf.Approximately(currentWheelie, 0f))
                visualTarget.RotateAround(wheeliePivot.position, visualTarget.right, -currentWheelie);
        }
        else
        {
            // Kein Pivot gesetzt -> altes Verhalten (dreht um den Ursprung, Heck sinkt ein).
            float pitch = effectPitch - currentWheelie;
            visualTarget.rotation = Quaternion.Euler(pitch, yaw, roll);
        }
    }
}
