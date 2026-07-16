using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Öffnet aus dem Hauptmenü das gemeinsame <see cref="UISettingsPopup"/> über einen VORHANDENEN
/// „Einstellungen"-Button (Szenen-Objekt, im Editor anpassbar). Referenzen im Inspector zuweisen
/// (das Editor-Tool „Tools/DDD/..." baut & verdrahtet sie automatisch).
/// </summary>
public class MainMenuSettings : MonoBehaviour
{
    [Header("Referenzen (im Editor zuweisen)")]
    public Button settingsButton;
    public UISettingsPopup settingsPopup;

    private void Awake()
    {
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
    }

    public void OpenSettings()
    {
        if (settingsPopup != null) settingsPopup.Open();
    }
}
