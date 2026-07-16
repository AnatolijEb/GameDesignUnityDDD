using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Einstellungs-Popup, das VORHANDENE Szenen-Objekte steuert (nichts wird zur Laufzeit gebaut,
/// alles im Editor anpassbar). Blendet <see cref="panel"/> ein/aus, koppelt den Musik-Slider an
/// <see cref="GameSettings.MusicVolume"/> und je einen Effekt-Button an
/// <see cref="GameSettings.IsEffectEnabled"/> / <see cref="GameSettings.SetEffectEnabled"/>.
///
/// Die Referenzen werden im Inspector zugewiesen (das Editor-Tool „Tools/DDD/..." baut & verdrahtet
/// sie automatisch, danach frei editierbar).
/// </summary>
public class UISettingsPopup : MonoBehaviour
{
    [System.Serializable]
    public class EffectToggleBinding
    {
        [Tooltip("Schlüssel wie in GameSettings.Effects, z.B. 'hiccup', 'microsleep', 'switchup'.")]
        public string key;
        [Tooltip("Anzeigename, z.B. 'Hickups'.")]
        public string displayName;
        [Tooltip("Der Button, der diesen Effekt an/aus schaltet.")]
        public Button button;
        [Tooltip("Das Textfeld im Button (zeigt 'Name: An/Aus').")]
        public TMP_Text label;
    }

    [Header("Referenzen (im Editor zuweisen)")]
    [Tooltip("Wurzel des Popup-Inhalts, die ein-/ausgeblendet wird.")]
    public GameObject panel;
    public Slider musicSlider;
    public TMP_Text musicValueLabel;
    public EffectToggleBinding[] effectToggles;
    public Button closeButton;

    private System.Action onClose;
    private bool wired;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Wire();
        if (panel != null) panel.SetActive(false);
    }

    private void OnEnable() { GameSettings.OnChanged += Refresh; }
    private void OnDisable() { GameSettings.OnChanged -= Refresh; }

    private void Wire()
    {
        if (wired) return;
        wired = true;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (effectToggles != null)
        {
            foreach (var t in effectToggles)
            {
                if (t == null || t.button == null) continue;
                string key = t.key; // lokale Kopie für den Closure
                t.button.onClick.AddListener(() =>
                {
                    GameSettings.SetEffectEnabled(key, !GameSettings.IsEffectEnabled(key));
                    Refresh();
                });
            }
        }

        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    public void Open(System.Action onCloseCallback = null)
    {
        onClose = onCloseCallback;
        Wire();
        if (panel != null) panel.SetActive(true);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        onClose?.Invoke();
    }

    private void OnMusicChanged(float v)
    {
        GameSettings.MusicVolume = v;
        Refresh();
    }

    private void Refresh()
    {
        if (musicValueLabel != null)
            musicValueLabel.text = Mathf.RoundToInt(GameSettings.MusicVolume * 100f) + "%";

        if (effectToggles == null) return;
        foreach (var t in effectToggles)
        {
            if (t == null || t.label == null) continue;
            t.label.text = t.displayName + (GameSettings.IsEffectEnabled(t.key) ? ": An" : ": Aus");
        }
    }
}
