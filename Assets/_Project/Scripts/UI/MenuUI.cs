using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Kleine Sammlung von Helfern, um zur Laufzeit einheitlich gestyltes UI (Overlays, Panels, Buttons,
/// Slider, Texte) zu bauen – im Farbschema des Hauptmenüs. Wird von PauseController,
/// UISettingsPopup und MainMenuSettings genutzt, damit alles gleich aussieht.
/// </summary>
public static class MenuUI
{
    // Menü-Palette (wie die Main-Menu-Buttons).
    public static readonly Color Orange = new Color(0.988f, 0.706f, 0.467f, 1f);
    public static readonly Color Pink = new Color(0.925f, 0.400f, 0.545f, 1f);
    public static readonly Color Purple = new Color(0.447f, 0.251f, 0.596f, 1f);
    public static readonly Color Cream = new Color(1f, 0.96f, 0.89f, 1f);
    public static readonly Color DarkText = new Color(0.30f, 0.18f, 0.36f, 1f);

    /// <summary>Erzeugt ein eigenes Screen-Space-Overlay-Canvas (mit Scaler + Raycaster).</summary>
    public static Canvas CreateOverlayCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static GameObject NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>Vollflächiges Bild (z.B. Hintergrund-Dimmer), das Klicks abfängt.</summary>
    public static Image CreateFullscreen(Transform parent, Color color)
    {
        var go = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Stretch(rt);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    public static Image CreatePanel(Transform parent, Vector2 size, Vector2 pos, Color color)
    {
        var go = NewRect("Panel", parent, size, pos);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, Vector2 size, Vector2 pos, Color color)
    {
        var go = NewRect("Text", parent, size, pos);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    public static Button CreateButton(Transform parent, string label, Vector2 size, Vector2 pos, UnityAction onClick)
    {
        var go = NewRect("Button_" + label, parent, size, pos);
        var img = go.AddComponent<Image>();
        img.color = Orange;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = Orange;
        cb.highlightedColor = Pink;
        cb.pressedColor = Purple;
        cb.selectedColor = Orange;
        cb.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.1f;
        btn.colors = cb;
        if (onClick != null) btn.onClick.AddListener(onClick);

        var t = CreateText(go.transform, label, Mathf.Min(32f, size.y * 0.5f), size, Vector2.zero, Color.white);
        Stretch((RectTransform)t.transform);
        return btn;
    }

    /// <summary>
    /// Scrollbares Textfeld im Menü-Stil (für längere Texte wie die Anleitung). Baut ScrollView +
    /// Viewport (mit Maske) + ein TMP-Textfeld, dessen Höhe automatisch mitwächst.
    /// </summary>
    public static ScrollRect CreateScrollText(Transform parent, Vector2 size, Vector2 pos, string text, float fontSize, Color textColor)
    {
        var go = NewRect("ScrollView", parent, size, pos);
        var scroll = go.AddComponent<ScrollRect>();
        var bg = go.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.25f);

        // Viewport mit Maske
        var viewport = NewRect("Viewport", go.transform, Vector2.zero, Vector2.zero);
        var vpRT = (RectTransform)viewport.transform;
        Stretch(vpRT);
        vpRT.offsetMin = new Vector2(14f, 14f);
        vpRT.offsetMax = new Vector2(-14f, -14f);
        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.001f); // fast unsichtbar, aber nötig als Maskengrafik
        viewport.AddComponent<RectMask2D>();

        // Inhalt = ein mitwachsendes Textfeld
        var contentGO = NewRect("Content", viewport.transform, Vector2.zero, Vector2.zero);
        var contentRT = (RectTransform)contentGO.transform;
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = new Vector2(0f, 0f);
        contentRT.offsetMax = new Vector2(0f, 0f);
        contentRT.anchoredPosition = Vector2.zero;

        var t = contentGO.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = textColor;
        t.alignment = TextAlignmentOptions.TopLeft;
        // Zeilenumbruch ist bei TMP standardmäßig aktiv – kein API-Aufruf nötig (versionssicher).
        t.richText = true;
        t.raycastTarget = false;

        var fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = vpRT;
        scroll.content = contentRT;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        return scroll;
    }

    /// <summary>Einfacher, funktionsfähiger Slider (0..1) im Menü-Stil, ohne externe Sprites.</summary>
    public static Slider CreateSlider(Transform parent, Vector2 size, Vector2 pos, float value, UnityAction<float> onChanged)
    {
        var go = NewRect("Slider", parent, size, pos);
        var slider = go.AddComponent<Slider>();

        var bg = go.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.30f);

        // Fill
        var fillArea = NewRect("Fill Area", go.transform, Vector2.zero, Vector2.zero);
        var fillAreaRT = (RectTransform)fillArea.transform;
        Stretch(fillAreaRT);
        fillAreaRT.offsetMin = new Vector2(2f, 2f);
        fillAreaRT.offsetMax = new Vector2(-2f, -2f);

        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        var fillRT = (RectTransform)fillGO.transform;
        fillRT.SetParent(fillArea.transform, false);
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fillGO.GetComponent<Image>().color = Pink;

        // Handle
        var handleArea = NewRect("Handle Slide Area", go.transform, Vector2.zero, Vector2.zero);
        var handleAreaRT = (RectTransform)handleArea.transform;
        Stretch(handleAreaRT);
        handleAreaRT.offsetMin = new Vector2(10f, 0f);
        handleAreaRT.offsetMax = new Vector2(-10f, 0f);

        var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        var handleRT = (RectTransform)handleGO.transform;
        handleRT.SetParent(handleArea.transform, false);
        handleRT.sizeDelta = new Vector2(24f, 0f);
        handleRT.anchorMin = new Vector2(0f, 0f);
        handleRT.anchorMax = new Vector2(0f, 1f);
        handleGO.GetComponent<Image>().color = Purple;

        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleGO.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
        return slider;
    }
}
