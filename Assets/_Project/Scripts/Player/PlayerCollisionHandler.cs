using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerLifeSystem lifeSystem;

    // > 0 => Hindernisse werden ignoriert (z.B. während eines Rampen-Sprungs). Wände sind nicht betroffen.
    private float obstacleImmunityTimer;

    [Header("Hit Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 0.8f;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        lifeSystem = GetComponent<PlayerLifeSystem>();
        
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
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
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

        // Always play the hit sound first so it is heard even if the hit is fatal.
        PlayRandomHitSound();

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
