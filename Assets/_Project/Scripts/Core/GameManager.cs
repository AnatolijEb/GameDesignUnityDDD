using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private AudioSource musicSource;

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
        EnsureAudioListener();
        PlayMusicFromStart();
    }

    /// <summary>
    /// Starts the background music from the beginning. Called on game start and on reset.
    /// </summary>
    public void PlayMusicFromStart()
    {
        // Fallback: if not assigned in Inspector, try to find it on the GameObject
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        // Secondary fallback: if still null, add it so the game never breaks
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicSource == null || backgroundMusic == null)
        {
            Debug.LogWarning("[GameManager] No background music or music source assigned.");
            return;
        }

        // Force settings so the music is always audible (2D, not muted, enabled).
        musicSource.enabled = true;
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.mute = false;
        musicSource.spatialBlend = 0f; // 2D so distance to the AudioListener never silences it
        musicSource.volume = musicVolume;

        musicSource.Stop();
        musicSource.Play();
        Debug.Log($"[GameManager] Started playing background music: {backgroundMusic.name} at volume {musicVolume}");
    }

    /// <summary>
    /// Guarantees there is exactly one active AudioListener in the scene, otherwise
    /// no sound (music or hit sounds) can be heard at all.
    /// </summary>
    private void EnsureAudioListener()
    {
        if (Object.FindFirstObjectByType<AudioListener>() != null)
        {
            return;
        }

        Camera cam = Camera.main;
        GameObject host = cam != null ? cam.gameObject : gameObject;
        host.AddComponent<AudioListener>();
        Debug.LogWarning($"[GameManager] No AudioListener found in scene. Added one to '{host.name}' so audio can be heard.");
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