using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerLifeSystem lifeSystem;

    // > 0 => Hindernisse werden ignoriert (z.B. während eines Rampen-Sprungs). Wände sind nicht betroffen.
    private float obstacleImmunityTimer;

    // Wand-Kontakt-Schaden: solange der Spieler eine Wand berührt, verliert er periodisch ein Leben.
    private int wallContacts;         // Anzahl aktuell überlappter Wand-Collider (falls eine Wand doch erreichbar ist)
    private float wallContactMemory;  // Restzeit der Nachwirkzeit, nachdem der letzte Wandkontakt endete
    private float wallDamageTimer;     // Fortschritt bis zum nächsten Lebensverlust
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

    [Header("Wand-Kontakt Schaden")]
    [Tooltip("Solange der Spieler eine Wand berührt, verliert er alle X Sekunden ein Leben.")]
    [SerializeField] private float wallDamageInterval = 2.5f;
    [Tooltip("Kurze Nachwirkzeit (Sek.): so lange nach dem letzten Wandkontakt gilt man noch als 'an der Wand', " +
             "damit ein kurzer Rückstoß-Abpraller den Schaden-Timer nicht zurücksetzt.")]
    [SerializeField] private float wallContactGrace = 0.5f;

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

    // Solange der Spieler eine Wand berührt (mit kurzer Nachwirkzeit gegen Rückstoß-Abpraller),
    // verliert er alle wallDamageInterval Sekunden ein Leben.
    private void TickWallDamage(float dt)
    {
        // Wandkontakt = der Spieler wird gegen den Rand gedrückt (Positions-Abfrage, Hauptfall wegen
        // des X-Clamps) ODER ein Wand-Collider überlappt tatsächlich (falls mal erreichbar).
        bool touching = (movement != null && movement.IsAgainstWall) || wallContacts > 0;

        if (touching && !wasTouchingWall)
        {
            Debug.Log($"[Collision] Wall contact started (Schaden alle {wallDamageInterval}s ein Leben)");
        }
        wasTouchingWall = touching;

        if (touching)
        {
            wallContactMemory = wallContactGrace;
        }
        else if (wallContactMemory > 0f)
        {
            wallContactMemory -= dt;
        }

        if (touching || wallContactMemory > 0f)
        {
            wallDamageTimer += dt;
            if (wallDamageTimer >= wallDamageInterval)
            {
                wallDamageTimer -= wallDamageInterval;
                Debug.Log($"[Collision] Wall contact damage (alle {wallDamageInterval}s ein Leben)");
                PlayRandomHitSound();
                ApplyDamage();
            }
        }
        else
        {
            wallDamageTimer = 0f;
        }
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
