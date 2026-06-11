using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private string gameplaySceneName = "SampleScene";

    private const string HighScoreKey = "HighScore";

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
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("[MainMenu] Gameplay scene name is empty!");
        }
    }
}
