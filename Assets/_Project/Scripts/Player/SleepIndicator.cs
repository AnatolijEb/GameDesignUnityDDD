using UnityEngine;
using TMPro;

/// <summary>
/// Zeigt eine „ZZZ"-Anzeige über dem Kopf des Charakters, solange er schläft
/// (Sekundenschlaf-Effekt). Wird auf ein aktives Objekt am Spieler gelegt (z.B. den
/// Player-Root) und toggelt ein zugewiesenes Kind-Objekt (das eigentliche ZZZ-Bild/Text).
///
/// Der <see cref="MicroSleepEffectSO"/>-Runtime sucht diese Komponente automatisch
/// (GetComponentInChildren) und ruft <see cref="Show"/> / <see cref="Hide"/> auf –
/// es ist also keine manuelle Verdrahtung mit dem Effekt nötig.
///
/// Die Animation ist bewusst rein transform-basiert (Wippen + Puls), damit sie mit
/// JEDEM Anzeige-Typ funktioniert (TextMeshPro-3D, Legacy 3D-Text, Sprite, ...).
/// </summary>
public class SleepIndicator : MonoBehaviour
{
    [Header("Anzeige")]
    [Tooltip("Das ZZZ-Objekt (Kind), das ein-/ausgeblendet wird. Standardmäßig ausgeschaltet. " +
             "Kann ein 3D-Text (TextMeshPro/Legacy) oder ein Sprite sein.")]
    [SerializeField] private GameObject indicatorRoot;

    [Header("Animation (optional, rein visuell)")]
    [Tooltip("Sanftes Auf-/Ab-Wippen und Pulsieren, solange die Anzeige sichtbar ist.")]
    [SerializeField] private bool animate = true;
    [Tooltip("Höhe des Auf-/Ab-Wippens (lokale Einheiten).")]
    [SerializeField] private float bobHeight = 0.15f;
    [Tooltip("Tempo des Wippens.")]
    [SerializeField] private float bobSpeed = 2f;
    [Tooltip("Stärke des Größen-Pulsierens (0.1 = ±10 %).")]
    [SerializeField] private float pulseScale = 0.1f;
    [Tooltip("Tempo des Pulsierens.")]
    [SerializeField] private float pulseSpeed = 3f;

    [Header("Buchstaben nacheinander (Z -> Z z -> Z z z -> ...)")]
    [Tooltip("Textfeld für die ZZZ-Stufen. Leer = wird automatisch im Anzeige-Objekt gesucht (TextMeshPro).")]
    [SerializeField] private TMP_Text letterText;
    [Tooltip("Stufen, die nacheinander gezeigt werden und dann von vorn beginnen. Leer = kein Cyceln.")]
    [SerializeField] private string[] letterStages = { "Z", "Z z", "Z z z" };
    [Tooltip("Sekunden pro Stufe.")]
    [SerializeField] private float letterInterval = 0.35f;

    private Vector3 baseLocalPos;
    private Vector3 baseScale;
    private float animTime;
    private bool showing;

    private int stageIndex;
    private float letterTimer;

    /// <summary>True, solange die ZZZ-Anzeige aktuell sichtbar ist.</summary>
    public bool IsShowing => showing;

    private void Awake()
    {
        if (indicatorRoot != null)
        {
            baseLocalPos = indicatorRoot.transform.localPosition;
            baseScale = indicatorRoot.transform.localScale;

            // Textfeld automatisch finden (auch wenn das Objekt inaktiv startet).
            if (letterText == null) letterText = indicatorRoot.GetComponentInChildren<TMP_Text>(true);

            indicatorRoot.SetActive(false); // startet unsichtbar
        }
    }

    /// <summary>Blendet die ZZZ-Anzeige ein und startet die Animation von vorn.</summary>
    public void Show()
    {
        showing = true;
        animTime = 0f;
        stageIndex = 0;
        letterTimer = 0f;
        if (letterText != null && letterStages != null && letterStages.Length > 0)
            letterText.text = letterStages[0];
        if (indicatorRoot != null) indicatorRoot.SetActive(true);
    }

    /// <summary>Blendet die ZZZ-Anzeige aus und setzt Position/Größe zurück.</summary>
    public void Hide()
    {
        showing = false;
        if (indicatorRoot != null)
        {
            indicatorRoot.transform.localPosition = baseLocalPos;
            indicatorRoot.transform.localScale = baseScale;
            indicatorRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!showing || indicatorRoot == null) return;

        // Buchstaben nacheinander einblenden: Z -> Z z -> Z z z -> wieder von vorn.
        if (letterText != null && letterStages != null && letterStages.Length > 0)
        {
            letterTimer += Time.deltaTime;
            if (letterTimer >= letterInterval)
            {
                letterTimer -= letterInterval;
                stageIndex = (stageIndex + 1) % letterStages.Length;
                letterText.text = letterStages[stageIndex];
            }
        }

        if (animate)
        {
            animTime += Time.deltaTime;
            float bob = Mathf.Sin(animTime * bobSpeed) * bobHeight;
            float pulse = 1f + Mathf.Sin(animTime * pulseSpeed) * pulseScale;

            indicatorRoot.transform.localPosition = baseLocalPos + Vector3.up * bob;
            indicatorRoot.transform.localScale = baseScale * pulse;
        }
    }
}
