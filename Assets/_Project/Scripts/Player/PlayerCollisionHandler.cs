using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerLifeSystem lifeSystem;

    // > 0 => Hindernisse werden ignoriert (z.B. während eines Rampen-Sprungs). Wände sind nicht betroffen.
    private float obstacleImmunityTimer;

    // Wand-Kontakt-Schaden: solange der Spieler eine Wand berührt, verliert er periodisch ein Leben.
    private int wallContacts;         // Anzahl aktuell überlappter Wand-Collider (falls eine Wand doch erreichbar ist)
    private float wallBumpTimer;       // Fortschritt bis zum nächsten Wand-Bump (Kollision)
    private bool wasTouchingWall;     // für den "Kontakt begonnen"-Log
    private PlayerMovementController movement; // liefert IsAgainstWall (Hauptquelle wegen X-Clamp)

    [Header("Hit Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 0.8f;
    [SerializeField] private AudioSource audioSource;

    [Header("Kollisions-Reaktion (Rückstoß / Überschlag)")]
    [Tooltip("Bei einem Kontakt wird der Spieler nicht mehr einfach durchgeschoben, sondern " +
             "reagiert physisch: frontal über das Hindernis mit Purzelbaum, seitlich mit sanftem Rückstoß.")]
    [SerializeField] private CollisionKnockbackSettings knockback = new CollisionKnockbackSettings();

    [Header("Wand-Kontakt (Bump + Schaden)")]
    [Tooltip("Zeit zwischen zwei Wand-Bumps, solange man dagegen fährt (Sek.). JEDER Bump ist eine " +
             "Kollision: Rückstoß + Sound + ein Leben weg. Die Unverwundbarkeit im PlayerLifeSystem " +
             "verhindert, dass schnelle Bumps sofort alle Leben abziehen (wie bei einem Hindernis-Treffer).")]
    [SerializeField] private float wallBumpInterval = 0.5f;

    private void Awake()
    {
        lifeSystem = GetComponent<PlayerLifeSystem>();
        movement = GetComponent<PlayerMovementController>();

        // Fallback: if not assigned in Inspector, try to get it from the GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Secondary fallback: if still null, add it so the game never breaks
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D so the hit sound is always audible
            audioSource.mute = false;
            audioSource.volume = 1f; // Base volume 1 so PlayOneShot scales exactly to hitVolume
        }
    }

    /// <summary>
    /// Macht den Spieler für <paramref name="seconds"/> Sekunden immun gegen Hindernisse
    /// (z.B. während eines Sprungs). Verlängert ein laufendes Fenster, kürzt es aber nie.
    /// </summary>
    public void GrantObstacleImmunity(float seconds)
    {
        obstacleImmunityTimer = Mathf.Max(obstacleImmunityTimer, seconds);
    }

    private void Update()
    {
        if (obstacleImmunityTimer > 0f)
        {
            obstacleImmunityTimer -= Time.deltaTime;
        }

        TickWallDamage(Time.deltaTime);
    }

    // Solange der Spieler gegen die Wand fährt, wird er wiederholt weggeschubst (Bump).
    // Jeder Bump ist eine Kollision und kostet ein Leben (siehe ApplyWallBump) – kein Intervall-Schaden.
    private void TickWallDamage(float dt)
    {
        // Wandkontakt = der Spieler wird gegen den Rand gedrückt (Positions-Abfrage, Hauptfall wegen
        // des X-Clamps) ODER ein Wand-Collider überlappt tatsächlich (falls mal erreichbar).
        bool touching = (movement != null && movement.IsAgainstWall) || wallContacts > 0;

        if (touching && !wasTouchingWall)
        {
            Debug.Log("[Collision] Wall contact started");
        }
        wasTouchingWall = touching;

        if (touching)
        {
            // Wiederholt anstoßen, solange man an der Wand entlangfährt.
            wallBumpTimer -= dt;
            if (wallBumpTimer <= 0f)
            {
                ApplyWallBump();
                wallBumpTimer = wallBumpInterval;
            }
        }
        else
        {
            wallBumpTimer = 0f; // beim nächsten Kontakt sofort wieder anstoßen
        }
    }

    // Rückstoß weg von der Wand – nutzt dieselbe seitliche Knockback-Reaktion wie ein Hindernis,
    // aber OHNE Hindernis-Immunität (an der Wand soll man nicht durch Autos gleiten können).
    private void ApplyWallBump()
    {
        int side = (movement != null && movement.WallSide != 0) ? movement.WallSide : 1;

        if (PlayerEffectController.Instance != null)
        {
            // dx-Vorzeichen = -side: Wand rechts (+1) -> nach links schubsen, Wand links (-1) -> nach rechts.
            PlayerEffectController.Instance.ApplyRuntime(
                new CollisionKnockbackRuntime(CollisionKnockbackRuntime.HitKind.Side, -side, knockback, grantImmunity: false));
        }

        PlayRandomHitSound();

        // Jeder Bump ist eine Wand-Kollision und kostet ein Leben. Die Unverwundbarkeit im
        // PlayerLifeSystem verhindert dabei, dass mehrere Bumps in schneller Folge sofort alle
        // Leben abziehen (gleiches Verhalten wie bei einem Hindernis-Treffer).
        Debug.Log("[Collision] Wall bump -> Kollision (Leben -1, sofern nicht unverwundbar)");
        ApplyDamage();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall")) wallContacts++;
        HandleHit(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall")) wallContacts = Mathf.Max(0, wallContacts - 1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider other)
    {
        if (!other.CompareTag("Wall") && !other.CompareTag("Obstacle"))
        {
            return;
        }

        // Hindernisse können statt Schaden einen Effekt auslösen (z.B. Rampe): dann
        // kein Lebensverlust und kein Crash-Sound. Normale Hindernisse haben contactEffect = null.
        if (other.CompareTag("Obstacle"))
        {
            // Während eines Sprungs (Rampe) werden Hindernisse "überflogen" -> ignorieren.
            if (obstacleImmunityTimer > 0f)
            {
                Debug.Log($"[Collision] Ignored (airborne): {other.gameObject.name}");
                return;
            }

            ObstacleBase obstacle = other.GetComponent<ObstacleBase>();
            if (obstacle == null) obstacle = other.GetComponentInParent<ObstacleBase>();

            if (obstacle != null && obstacle.Data != null && obstacle.Data.contactEffect != null)
            {
                Debug.Log($"[Collision] Effect obstacle: {other.gameObject.name} -> {obstacle.Data.contactEffect.displayName}");
                if (PlayerEffectController.Instance != null)
                {
                    PlayerEffectController.Instance.Apply(obstacle.Data.contactEffect);
                }
                return;
            }
        }

        Debug.Log($"[Collision] Hit: {other.gameObject.name} (Tag: {other.tag})");

        // Trefferrichtung klassifizieren: frontal (head-on) vs. seitliches Streifen (glancing),
        // anhand der seitlichen Position des Spielers relativ zur Mitte/Breite des Hindernisses.
        // Wände zählen immer als seitlich (man soll nicht über eine Wand purzeln).
        Bounds b = other.bounds;
        float dx = transform.position.x - b.center.x;
        float halfWidth = Mathf.Max(0.01f, b.extents.x);
        bool isWall = other.CompareTag("Wall");
        bool headOn = !isWall && Mathf.Abs(dx) <= halfWidth * knockback.headOnWidthFraction;

        CollisionKnockbackRuntime.HitKind kind = headOn
            ? CollisionKnockbackRuntime.HitKind.HeadOn
            : CollisionKnockbackRuntime.HitKind.Side;

        // Physische Reaktion einspeisen (kümmert sich auch um die Immunität während der Reaktion).
        if (PlayerEffectController.Instance != null)
        {
            PlayerEffectController.Instance.ApplyRuntime(new CollisionKnockbackRuntime(kind, dx, knockback));
        }

        // Always play the hit sound first so it is heard even if the hit is fatal.
        PlayRandomHitSound();

        // Frontal kostet immer ein Leben; seitliches Streifen an HINDERNISSEN nur, wenn so eingestellt.
        // Wände kosten NICHT pro Kontakt ein Leben – sie machen periodischen Schaden (siehe TickWallDamage).
        bool costsLife = headOn || (knockback.sideHitCostsLife && !isWall);
        if (costsLife)
        {
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        if (lifeSystem != null)
        {
            lifeSystem.LoseLife();
        }
        else
        {
            // Fallback to old behavior if no life system
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.TriggerGameOver();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private void PlayRandomHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0 || audioSource == null)
        {
            return;
        }

        int randomIndex = Random.Range(0, hitSounds.Length);
        AudioClip clip = hitSounds[randomIndex];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, hitVolume);
            Debug.Log($"[Collision] Played random hit sound: {clip.name} at volume {hitVolume}");
        }
    }
}
