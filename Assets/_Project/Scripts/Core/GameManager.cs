using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
