using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Time.timeScale = 1f; // Ensure time is running
            Debug.Log("[GameManager] Instance initialized. Time.timeScale reset to 1.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.5f; // Set a default pleasant volume
            musicSource.Play();
            Debug.Log($"[GameManager] Started playing background music: {backgroundMusic.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] No background music assigned.");
        }
    }

    public void TriggerGameOver()
    {
        Debug.Log("[GameManager] Game Over Triggered.");
        
        int finalScore = 0;
        bool isNewHighscore = false;

        if (ScoreSystem.Instance != null)
        {
            finalScore = ScoreSystem.Instance.CurrentScore;
            isNewHighscore = finalScore > ScoreSystem.Instance.HighScore;
            ScoreSystem.Instance.SaveScores();
        }
        
        GameOverUI gameOverUI = Object.FindFirstObjectByType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.Show(finalScore, isNewHighscore);
        }
        else
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting Game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
