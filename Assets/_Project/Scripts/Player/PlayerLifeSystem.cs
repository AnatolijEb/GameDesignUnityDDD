using UnityEngine;

public class PlayerLifeSystem : MonoBehaviour
{
    [Header("Life Settings")]
    [SerializeField] private int maxLives = 4;
    [SerializeField] private int currentLives;
    
    [Header("Invulnerability Settings")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    private float invulnerabilityTimer = 0f;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;

    private void Awake()
    {
        ResetLives();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        invulnerabilityTimer = 0f;
        Debug.Log($"[LifeSystem] Lives reset to {currentLives}/{maxLives}");
    }

    public void LoseLife()
    {
        if (invulnerabilityTimer > 0) return;

        currentLives--;
        invulnerabilityTimer = invulnerabilityDuration;
        
        Debug.Log($"[LifeSystem] Life lost! Current lives: {currentLives}/{maxLives}");

        if (currentLives <= 0)
        {
            currentLives = 0;
            TriggerGameOver();
        }
    }

    public void AddLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            Debug.Log($"[LifeSystem] Life gained! Current lives: {currentLives}/{maxLives}");
        }
        else
        {
            Debug.Log("[LifeSystem] Already at max lives.");
        }
    }

    public bool HasLivesRemaining()
    {
        return currentLives > 0;
    }

    private void TriggerGameOver()
    {
        Debug.Log("[LifeSystem] No lives remaining. Triggering Game Over.");
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.TriggerGameOver();
        }
        else
        {
            // Fallback
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
