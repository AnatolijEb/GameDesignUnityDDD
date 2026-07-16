using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private string gameplaySceneName = "SampleScene";

    [Header("Audio")]
    [Tooltip("Sound beim Drücken von Start (z.B. 'ding'). Leer = kein Sound.")]
    [SerializeField] private AudioClip startSound;
    [Range(0f, 1f)] [SerializeField] private float startSoundVolume = 1f;
    [Tooltip("Maximale Verzögerung des Szenenwechsels, damit der Ding hörbar bleibt (Sekunden).")]
    [SerializeField] private float maxStartDelay = 0.6f;

    private const string HighScoreKey = "HighScore";
    private bool isStarting;

    private void Start()
    {
        LoadAndDisplayHighScore();
    }

    private void LoadAndDisplayHighScore()
    {
        int highscore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (highscoreText != null)
        {
            highscoreText.text = $"Highscore: {highscore}";
        }
    }

    public void StartGame()
    {
        Debug.Log($"[MainMenu] Start button clicked. Loading scene: {gameplaySceneName}");

        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("[MainMenu] Gameplay scene name is empty!");
            return;
        }

        if (isStarting) return; // Doppelklick abfangen
        isStarting = true;

        if (startSound != null)
        {
            StartCoroutine(PlayStartSoundThenLoad());
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    /// <summary>
    /// Spielt den Start-Sound und wechselt erst danach die Szene, damit der Ding nicht sofort
    /// vom Szenenwechsel abgeschnitten wird.
    /// </summary>
    private IEnumerator PlayStartSoundThenLoad()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.clip = startSound;
        src.spatialBlend = 0f; // 2D
        src.volume = startSoundVolume;
        src.Play();

        float wait = Mathf.Min(startSound.length, maxStartDelay);
        yield return new WaitForSecondsRealtime(wait);

        SceneManager.LoadScene(gameplaySceneName);
    }
}
