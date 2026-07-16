using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Öffnet aus dem Hauptmenü das <see cref="UIInstructionsPopup"/> über einen VORHANDENEN
/// „Anleitung"-Button (Szenen-Objekt, im Editor anpassbar). Analog zu <see cref="MainMenuSettings"/>.
/// Referenzen im Inspector zuweisen (das Editor-Tool „Tools/DDD/Anleitung ..." baut &amp; verdrahtet
/// sie automatisch).
/// </summary>
public class MainMenuInstructions : MonoBehaviour
{
    [Header("Referenzen (im Editor zuweisen)")]
    public Button instructionsButton;
    public UIInstructionsPopup instructionsPopup;

    private void Awake()
    {
        if (instructionsButton != null) instructionsButton.onClick.AddListener(OpenInstructions);
    }

    public void OpenInstructions()
    {
        if (instructionsPopup != null) instructionsPopup.Open();
    }
}
