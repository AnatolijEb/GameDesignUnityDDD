using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor-Werkzeug: baut das Pause-Overlay und das Einstellungs-Popup als ECHTE, bearbeitbare
/// Szenen-Objekte (kein Laufzeit-Erzeugen) und verdrahtet sie mit den vorhandenen Controllern.
/// Danach kannst du alles frei im Editor anpassen. Menü: „Tools/DDD/...".
///
/// Bedienung:
///   1. Gameplay-Szene öffnen -> „Setup Pause + Einstellungen (Gameplay-Szene)".
///   2. Hauptmenü-Szene öffnen -> „Setup Einstellungen-Button (Hauptmenü)".
///   3. Szene speichern (Ctrl+S).
/// Erneut ausführbar erst, nachdem das erzeugte „MenuUICanvas" gelöscht wurde.
/// </summary>
public static class MenuUISetup
{
    [MenuItem("Tools/DDD/Setup Pause + Einstellungen (Gameplay-Szene)")]
    public static void SetupGameplay()
    {
        if (AlreadyExists()) return;

        var canvas = MenuUI.CreateOverlayCanvas("MenuUICanvas", 120);
        Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Setup Pause UI");

        // Pause-Overlay
        var pauseOverlay = MenuUI.NewRect("PauseOverlay", canvas.transform, Vector2.zero, Vector2.zero);
        MenuUI.Stretch((RectTransform)pauseOverlay.transform);
        MenuUI.CreateFullscreen(pauseOverlay.transform, new Color(0.98f, 0.55f, 0.45f, 0.55f));
        var card = MenuUI.CreatePanel(pauseOverlay.transform, new Vector2(620f, 620f), Vector2.zero, MenuUI.Cream);
        MenuUI.CreateText(card.transform, "Pause", 72f, new Vector2(560f, 120f), new Vector2(0f, 210f), MenuUI.DarkText);
        var resumeBtn = MenuUI.CreateButton(card.transform, "Resume", new Vector2(360f, 90f), new Vector2(0f, 70f), null);
        var settingsBtn = MenuUI.CreateButton(card.transform, "Einstellungen", new Vector2(360f, 90f), new Vector2(0f, -40f), null);
        var menuBtn = MenuUI.CreateButton(card.transform, "Menü", new Vector2(360f, 90f), new Vector2(0f, -150f), null);

        var popup = BuildSettingsPopup(canvas.transform);

        var pc = Object.FindFirstObjectByType<PauseController>();
        if (pc == null) pc = canvas.gameObject.AddComponent<PauseController>();
        pc.pausePanel = pauseOverlay;
        pc.resumeButton = resumeBtn;
        pc.settingsButton = settingsBtn;
        pc.menuButton = menuBtn;
        pc.settingsPopup = popup;
        EditorUtility.SetDirty(pc);

        pauseOverlay.SetActive(true); // im Editor sichtbar; PauseController blendet es zur Laufzeit aus
        Finish(canvas.gameObject, "Pause + Einstellungen");
    }

    [MenuItem("Tools/DDD/Setup Einstellungen-Button (Hauptmenü)")]
    public static void SetupMainMenu()
    {
        if (AlreadyExists()) return;

        var canvas = MenuUI.CreateOverlayCanvas("MenuUICanvas", 60);
        Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Setup Menu Settings");

        var settingsBtn = MenuUI.CreateButton(canvas.transform, "Einstellungen", new Vector2(240f, 90f), Vector2.zero, null);
        var rt = (RectTransform)settingsBtn.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-140f, 70f);

        var popup = BuildSettingsPopup(canvas.transform);

        var ms = Object.FindFirstObjectByType<MainMenuSettings>();
        if (ms == null) ms = canvas.gameObject.AddComponent<MainMenuSettings>();
        ms.settingsButton = settingsBtn;
        ms.settingsPopup = popup;
        EditorUtility.SetDirty(ms);

        Finish(canvas.gameObject, "Einstellungen-Button");
    }

    private static UISettingsPopup BuildSettingsPopup(Transform canvas)
    {
        var popupGO = MenuUI.NewRect("SettingsPopup", canvas, Vector2.zero, Vector2.zero);
        MenuUI.Stretch((RectTransform)popupGO.transform);
        var popup = popupGO.AddComponent<UISettingsPopup>();

        var content = MenuUI.NewRect("Content", popupGO.transform, Vector2.zero, Vector2.zero);
        MenuUI.Stretch((RectTransform)content.transform);
        MenuUI.CreateFullscreen(content.transform, new Color(0f, 0f, 0f, 0.45f));

        int effectCount = GameSettings.Effects.Length;
        float panelHeight = 430f + effectCount * 80f;
        var card = MenuUI.CreatePanel(content.transform, new Vector2(760f, panelHeight), Vector2.zero, MenuUI.Cream);
        var p = card.transform;

        float y = panelHeight * 0.5f - 70f;
        MenuUI.CreateText(p, "Einstellungen", 54f, new Vector2(700f, 90f), new Vector2(0f, y), MenuUI.DarkText);
        y -= 110f;

        MenuUI.CreateText(p, "Musik", 34f, new Vector2(300f, 50f), new Vector2(-200f, y), MenuUI.DarkText);
        var musicValue = MenuUI.CreateText(p, "100%", 34f, new Vector2(160f, 50f), new Vector2(240f, y), MenuUI.DarkText);
        y -= 50f;
        var slider = MenuUI.CreateSlider(p, new Vector2(660f, 34f), new Vector2(0f, y), GameSettings.MusicVolume, null);
        y -= 70f;

        MenuUI.CreateText(p, "Effekte", 34f, new Vector2(660f, 50f), new Vector2(0f, y), MenuUI.DarkText);
        y -= 60f;

        var toggles = new List<UISettingsPopup.EffectToggleBinding>();
        foreach (var e in GameSettings.Effects)
        {
            var btn = MenuUI.CreateButton(p, e.label + ": An", new Vector2(600f, 64f), new Vector2(0f, y), null);
            var lbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            toggles.Add(new UISettingsPopup.EffectToggleBinding { key = e.key, displayName = e.label, button = btn, label = lbl });
            y -= 76f;
        }

        y -= 10f;
        var closeBtn = MenuUI.CreateButton(p, "Schließen", new Vector2(300f, 80f), new Vector2(0f, y), null);

        popup.panel = content;
        popup.musicSlider = slider;
        popup.musicValueLabel = musicValue;
        popup.effectToggles = toggles.ToArray();
        popup.closeButton = closeBtn;
        EditorUtility.SetDirty(popup);

        content.SetActive(false); // versteckt; zum Bearbeiten im Editor kurz aktivieren
        return popup;
    }

    private static bool AlreadyExists()
    {
        if (GameObject.Find("MenuUICanvas") == null) return false;
        EditorUtility.DisplayDialog("Schon vorhanden",
            "In dieser Szene gibt es bereits ein 'MenuUICanvas'. Bitte erst löschen und dann erneut ausführen.", "OK");
        return true;
    }

    private static void Finish(GameObject created, string what)
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = created;
        Debug.Log($"[MenuUISetup] '{what}' als Szenen-Objekte gebaut & verdrahtet. Bitte Szene speichern (Ctrl+S).");
    }
}
