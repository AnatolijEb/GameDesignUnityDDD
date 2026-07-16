using UnityEngine;

/// <summary>
/// Bewegt die Welt (Chunks) relativ zum Spieler.
///
/// WICHTIG: Statt jeden Frame inkrementell zu verschieben (transform.Translate), wird die
/// Z-Position jedes Frame ABSOLUT aus einer einzigen, global geteilten Scroll-Distanz berechnet:
///
///     z = baseZ - RunSpeedManager.DistanceTravelled
///
/// Warum? Beim inkrementellen Verschieben bewegt sich jeder Chunk unabhängig. Dadurch entstehen
/// Lücken zwischen den Chunks:
///   - Spawn-Race: Ob ein gerade instanziierter Chunk in seinem ersten Frame schon einen Move
///     ausführt, ist von Unity nicht garantiert -> es kann eine dauerhaft "eingebrannte" Lücke
///     von CurrentSpeed * deltaTime entstehen (größer bei hohem Tempo / Frame-Drops).
///   - Aufsummierte Float-Rundungsfehler zwischen unabhängig bewegten Chunks.
///
/// Mit der absoluten Berechnung teilen sich ALLE Chunks exakt dieselbe DistanceTravelled und
/// behalten ihren Abstand (90 Units) exakt bei — unabhängig von Frame-Timing, Spawn-Reihenfolge
/// oder Speed-Änderungen (Beschleunigen/Drosseln/Rückwärts über den Scroll-Multiplier).
/// </summary>
public class WorldScrollMover : MonoBehaviour
{
    public RunSpeedManager runSpeedManager;

    // Basis-Z beim Erzeugen des Chunks: aktuelle Welt-Z + bereits zurückgelegte Distanz.
    // Ab dann gilt jeden Frame: z = baseZ - DistanceTravelled.
    private float baseZ;
    private bool initialized;

    private void Awake()
    {
        // Awake läuft synchron während Instantiate(...), also mit der korrekten Distanz zum
        // Spawn-Zeitpunkt und der bereits vom Manager gesetzten Spawn-Position.
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        ResolveManager();
        baseZ = transform.position.z + CurrentDistance();
        initialized = true;
    }

    private void ResolveManager()
    {
        if (runSpeedManager == null)
        {
            runSpeedManager = RunSpeedManager.Instance;
            if (runSpeedManager == null)
            {
                runSpeedManager = Object.FindFirstObjectByType<RunSpeedManager>();
            }
        }
    }

    private float CurrentDistance()
    {
        return runSpeedManager != null ? runSpeedManager.DistanceTravelled : 0f;
    }

    private void Update()
    {
        if (runSpeedManager == null)
        {
            ResolveManager();
            if (runSpeedManager == null) return;
        }

        // Absolute Positionierung: alle Chunks nutzen dieselbe Distanz -> keine relativen Lücken.
        Vector3 pos = transform.position;
        pos.z = baseZ - runSpeedManager.DistanceTravelled;
        transform.position = pos;
    }
}
