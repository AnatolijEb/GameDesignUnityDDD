using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dunkle Vignette rund um die Kamera während des Sekundenschlafs: blendet langsam ein, wenn ein
/// <see cref="MicroSleepEffectSO"/> aktiv wird, und wieder aus, wenn er endet. Die Mitte bleibt klar,
/// nur die Ränder dunkeln ab (moderate Deckkraft, man sieht weiterhin genug).
///
/// Vollständig eigenständig: erzeugt sein eigenes Screen-Space-Overlay-Canvas und eine radiale
/// Schwarz-Textur zur Laufzeit. Einfach die Komponente auf ein aktives Objekt in der Spielszene legen.
/// </summary>
public class MicroSleepVignette : MonoBehaviour
{
    [Header("Aussehen")]
    [Tooltip("Maximale Deckkraft an den Bildrändern (0..1). Bewusst moderat, damit man noch etwas sieht.")]
    [Range(0f, 1f)] [SerializeField] private float maxAlpha = 0.6f;
    [Tooltip("Wie schnell die Vignette ein-/ausblendet (höher = schneller).")]
    [SerializeField] private float fadeSpeed = 2.5f;
    [Tooltip("Bis zu diesem Radius (0..~1.4, Mitte=0) bleibt das Bild klar.")]
    [Range(0f, 1.4f)] [SerializeField] private float innerRadius = 0.35f;
    [Tooltip("Ab diesem Radius ist die Vignette voll dunkel.")]
    [Range(0f, 1.4f)] [SerializeField] private float outerRadius = 1.0f;
    [Tooltip("Zeichen-Reihenfolge des Overlays. Höher = weiter vorne.")]
    [SerializeField] private int sortingOrder = 50;

    private PlayerEffectController controller;
    private Image image;
    private int activeCount;
    private float currentAlpha;

    private void Start()
    {
        BuildOverlay();
        TryBind();
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnEffectApplied -= HandleApplied;
            controller.OnEffectRemoved -= HandleRemoved;
            controller = null;
        }
    }

    private void Update()
    {
        if (controller == null) TryBind();

        float target = activeCount > 0 ? maxAlpha : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, target, fadeSpeed * Time.deltaTime);

        if (image != null)
        {
            Color c = image.color;
            c.a = currentAlpha;
            image.color = c;
            image.enabled = currentAlpha > 0.001f;
        }
    }

    private void TryBind()
    {
        controller = PlayerEffectController.Instance;
        if (controller != null)
        {
            controller.OnEffectApplied += HandleApplied;
            controller.OnEffectRemoved += HandleRemoved;
        }
    }

    private void HandleApplied(PlayerEffectSO e)
    {
        if (e is MicroSleepEffectSO) activeCount++;
    }

    private void HandleRemoved(PlayerEffectSO e)
    {
        if (e is MicroSleepEffectSO && activeCount > 0) activeCount--;
    }

    private void BuildOverlay()
    {
        GameObject canvasGO = new GameObject("MicroSleepVignetteCanvas", typeof(Canvas));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        GameObject imgGO = new GameObject("Vignette", typeof(Image));
        RectTransform rt = (RectTransform)imgGO.transform;
        rt.SetParent(canvasGO.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        image = imgGO.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = false; // Vollbild -> elliptische Vignette passend zum Bildschirm
        image.sprite = BuildVignetteSprite();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.enabled = false;
    }

    private Sprite BuildVignetteSprite()
    {
        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        Color32[] px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x / (float)(size - 1) - 0.5f) * 2f;
                float dy = (y / (float)(size - 1) - 0.5f) * 2f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(innerRadius, outerRadius, dist));
                px[y * size + x] = new Color32(0, 0, 0, (byte)(Mathf.Clamp01(a) * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
