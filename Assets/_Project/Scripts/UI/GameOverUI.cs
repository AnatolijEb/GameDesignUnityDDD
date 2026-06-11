using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameOverUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject overlayContainer;
    [SerializeField] private GameObject newHighscoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Visual Effects")]
    [SerializeField] private Volume postProcessVolume;
    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (overlayContainer != null) overlayContainer.SetActive(false);
        if (newHighscoreText != null) newHighscoreText.SetActive(false);
        
        // Use the profile instance to avoid modifying the asset on disk
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (!postProcessVolume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = postProcessVolume.profile.Add<ColorAdjustments>(true);
            }
            // Initially ensure it's not desaturated
            colorAdjustments.saturation.Override(0f);
        }
    }

    public void Show(int score, bool isNewHighscore)
    {
        Time.timeScale = 0f;
        if (overlayContainer != null) overlayContainer.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score: " + score;
        if (newHighscoreText != null) newHighscoreText.SetActive(isNewHighscore);

        // Apply B&W effect
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.Override(-100f);
        }
        
        Debug.Log("[GameOverUI] Showing Game Over screen.");
    }

    public void RestartGame()
    {
        Debug.Log("[GameOverUI] Restart button clicked.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void BackToMenu()
    {
        Debug.Log("[GameOverUI] Back to Menu button clicked.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
