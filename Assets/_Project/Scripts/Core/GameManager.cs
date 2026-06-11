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
            Debug.Log("[GameManager] Instance initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGameOver()
    {
        Debug.Log("[GameManager] Game Over Triggered.");
        
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.SaveScores();
        }
        
        RestartGame();
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting Game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
