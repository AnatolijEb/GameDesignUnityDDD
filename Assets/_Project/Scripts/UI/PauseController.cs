using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause während des Spiels: <see cref="pauseKey"/> (Standard ESC) hält das Spiel an
/// (Time.timeScale = 0) und blendet ein VORHANDENES Pause-Overlay (Szenen-Objekt) ein/aus.
/// Nichts wird zur Laufzeit gebaut – alles im Editor anpassbar; Referenzen im Inspector zuweisen
/// (das Editor-Tool „Tools/DDD/..." baut & verdrahtet sie automatisch).
///
/// Während des Game Over (timeScale bereits 0, aber nicht von uns pausiert) tut die Taste nichts.
/// </summary>
public class PauseController : MonoBehaviour
{
    [Header("Tasten / Szene")]
    public KeyCode pauseKey = KeyCode.Escape;
    public string mainMenuSceneName = "MainMenu";

    [Header("Referenzen (im Editor zuweisen)")]
    [Tooltip("Wurzel des Pause-Overlays, die ein-/ausgeblendet wird.")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button menuButton;
    public UISettingsPopup settingsPopup;

    private bool isPaused;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (menuButton != null) menuButton.onClick.AddListener(ToMenu);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(pauseKey)) return;

        if (settingsPopup != null && settingsPopup.IsOpen)
        {
            settingsPopup.Close();
            return;
        }

        if (isPaused) Resume();
        // Nur pausieren, wenn ein Overlay zugewiesen ist (sonst würde das Spiel unsichtbar einfrieren)
        // und nicht schon anderweitig pausiert (z.B. Game Over).
        else if (Time.timeScale > 0f && pausePanel != null) Pause();
    }

    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        if (settingsPopup != null) settingsPopup.Open();
    }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
