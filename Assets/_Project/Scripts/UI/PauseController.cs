using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    [Header("Score-Anzeige (optional)")]
    [Tooltip("Text im Pause-Menü, der beim Pausieren den aktuellen Score anzeigt. Im Inspector zuweisen.")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Text vor der Zahl (z.B. 'Score: '). Leer lassen für nur die Zahl.")]
    public string scorePrefix = "Score: ";

    private bool isPaused;

    // Zahlenformat mit kleiner Lücke als Tausender-Trennung (z.B. 10 000), passend zum HUD.
    private static readonly System.Globalization.NumberFormatInfo GroupedNumberFormat = CreateGroupedNumberFormat();

    private static System.Globalization.NumberFormatInfo CreateGroupedNumberFormat()
    {
        var nfi = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        nfi.NumberGroupSizes = new[] { 3 };
        return nfi;
    }

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
        UpdatePauseScore();
        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>Schreibt den aktuellen Score in den Pause-Score-Text (falls zugewiesen).</summary>
    private void UpdatePauseScore()
    {
        if (scoreText == null) return;

        int score = ScoreSystem.Instance != null ? ScoreSystem.Instance.CurrentScore : 0;
        scoreText.text = scorePrefix + score.ToString("N0", GroupedNumberFormat);
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
