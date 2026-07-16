using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Anleitungs-Popup, das ein VORHANDENES Szenen-Objekt ein-/ausblendet (nichts wird zur Laufzeit
/// gebaut, alles im Editor anpassbar). Zeigt einen scrollbaren Anleitungstext. Aufbau wie
/// <see cref="UISettingsPopup"/>, nur ohne Slider/Toggles.
///
/// Die Referenzen werden im Inspector zugewiesen (das Editor-Tool „Tools/DDD/Anleitung ..." baut &amp;
/// verdrahtet sie automatisch, danach frei editierbar).
/// </summary>
public class UIInstructionsPopup : MonoBehaviour
{
    [Header("Referenzen (im Editor zuweisen)")]
    [Tooltip("Wurzel des Popup-Inhalts, die ein-/ausgeblendet wird.")]
    public GameObject panel;
    public Button closeButton;

    private System.Action onClose;
    private bool wired;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Wire();
        if (panel != null) panel.SetActive(false);
    }

    private void Wire()
    {
        if (wired) return;
        wired = true;
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    /// <summary>Parameterlose Variante zum Verdrahten per Inspector-OnClick (wie beim StartButton).</summary>
    public void Show()
    {
        Open();
    }

    public void Open(System.Action onCloseCallback = null)
    {
        onClose = onCloseCallback;
        Wire();
        if (panel != null) panel.SetActive(true);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        onClose?.Invoke();
    }
}
