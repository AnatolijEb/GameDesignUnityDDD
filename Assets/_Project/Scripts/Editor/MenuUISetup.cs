using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
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

        // Hinweis: Im Spiel/Pause-Menü wird BEWUSST kein Anleitung-Button erzeugt –
        // die Anleitung gibt es nur im Hauptmenü.

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

        AddMainMenuInstructions(canvas);

        Finish(canvas.gameObject, "Einstellungen-Button");
    }

    // ------------------------------------------------------------------
    // Anleitung (nachträglich hinzufügbar, falls das Menü schon steht)
    // ------------------------------------------------------------------

    [MenuItem("Tools/DDD/Anleitung hinzufügen (Hauptmenü)")]
    public static void SetupMainMenuInstructions()
    {
        if (Object.FindFirstObjectByType<MainMenuInstructions>() != null)
        {
            EditorUtility.DisplayDialog("Schon vorhanden",
                "In dieser Szene gibt es bereits eine Anleitung (MainMenuInstructions). Bitte erst das zugehörige Objekt löschen und dann erneut ausführen.", "OK");
            return;
        }

        var canvas = FindOrCreateCanvas("MenuUICanvas", 60);
        AddMainMenuInstructions(canvas);
        Finish(canvas.gameObject, "Anleitung (Hauptmenü)");
    }

    private static void AddMainMenuInstructions(Canvas canvas)
    {
        var popup = BuildInstructionsPopup(canvas.transform);

        // Bevorzugt: den vorhandenen StartButton klonen -> exakt gleicher Look (Font/Größe/Farbe/Form).
        Button btn = CloneButtonTemplate("StartButton", "Anleitung");
        if (btn == null)
        {
            // Fallback, falls kein StartButton gefunden wird: generischer Button im Menü-Stil.
            btn = MenuUI.CreateButton(canvas.transform, "Anleitung", new Vector2(300f, 80f), Vector2.zero, null);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-140f, 170f);
        }

        // Reihenfolge (oben nach unten): Start -> Einstellungen -> Anleitung, als vertikaler Stapel.
        StackMainMenuButtons(btn);

        var mi = Object.FindFirstObjectByType<MainMenuInstructions>();
        if (mi == null) mi = canvas.gameObject.AddComponent<MainMenuInstructions>();
        mi.instructionsButton = btn;
        mi.instructionsPopup = popup;
        EditorUtility.SetDirty(mi);
    }

    /// <summary>
    /// Ordnet Start-, Einstellungen- und Anleitung-Button als vertikalen Stapel an (Start oben).
    /// Nutzt den StartButton als Bezugspunkt; Einstellungen und Anleitung erben dessen Anker/Pivot.
    /// </summary>
    private static void StackMainMenuButtons(Button instructionsBtn)
    {
        var startGO = GameObject.Find("StartButton");
        var settingsGO = GameObject.Find("Button_Einstellungen");
        if (startGO == null) return;

        var startRT = (RectTransform)startGO.transform;
        float gap = startRT.sizeDelta.y + 30f; // Zeilenabstand = Button-Höhe + etwas Luft
        Vector2 top = startRT.anchoredPosition;

        // Einstellungen eine Zeile unter Start
        if (settingsGO != null)
            AlignUnder((RectTransform)settingsGO.transform, startRT, top - new Vector2(0f, gap));

        // Anleitung eine weitere Zeile darunter
        if (instructionsBtn != null)
            AlignUnder((RectTransform)instructionsBtn.transform, startRT, top - new Vector2(0f, gap * 2f));
    }

    private static void AlignUnder(RectTransform target, RectTransform reference, Vector2 anchoredPos)
    {
        target.anchorMin = reference.anchorMin;
        target.anchorMax = reference.anchorMax;
        target.pivot = reference.pivot;
        target.anchoredPosition = anchoredPos;
        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// Klont einen vorhandenen Button (z.B. „StartButton") als Vorlage, benennt/beschriftet ihn neu,
    /// entfernt alte OnClick-Verknüpfungen und versetzt ihn leicht. So sieht der neue Button exakt aus
    /// wie das Original. Gibt null zurück, wenn die Vorlage nicht gefunden wird.
    /// </summary>
    private static Button CloneButtonTemplate(string templateName, string label)
    {
        var template = GameObject.Find(templateName);
        if (template == null) return null;

        var clone = Object.Instantiate(template, template.transform.parent);
        clone.name = "Button_" + label;
        Undo.RegisterCreatedObjectUndo(clone, "Clone Menu Button");

        var txt = clone.GetComponentInChildren<TMP_Text>();
        if (txt != null) { txt.text = label; EditorUtility.SetDirty(txt); }

        var btn = clone.GetComponent<Button>();
        if (btn != null)
        {
            // Alte Verknüpfungen (z.B. StartGame) entfernen, sonst würde der Klon das Spiel starten.
            for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(btn.onClick, i);
        }

        // Etwas versetzen, damit der Klon nicht exakt auf dem Original liegt (danach frei verschiebbar).
        var rt = clone.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition += new Vector2(0f, 110f);

        EditorUtility.SetDirty(clone);
        return btn;
    }

    private static Canvas FindOrCreateCanvas(string name, int sortingOrder)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            var c = existing.GetComponent<Canvas>();
            if (c != null) return c;
        }
        var canvas = MenuUI.CreateOverlayCanvas(name, sortingOrder);
        Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Create Menu Canvas");
        return canvas;
    }

    private static UIInstructionsPopup BuildInstructionsPopup(Transform canvas)
    {
        var popupGO = MenuUI.NewRect("InstructionsPopup", canvas, Vector2.zero, Vector2.zero);
        MenuUI.Stretch((RectTransform)popupGO.transform);
        var popup = popupGO.AddComponent<UIInstructionsPopup>();

        var content = MenuUI.NewRect("Content", popupGO.transform, Vector2.zero, Vector2.zero);
        MenuUI.Stretch((RectTransform)content.transform);
        MenuUI.CreateFullscreen(content.transform, new Color(0f, 0f, 0f, 0.55f));

        var card = MenuUI.CreatePanel(content.transform, new Vector2(1000f, 860f), Vector2.zero, MenuUI.Cream);
        var p = card.transform;

        MenuUI.CreateText(p, "Spielanleitung", 54f, new Vector2(920f, 90f), new Vector2(0f, 360f), MenuUI.DarkText);

        // Scrollbarer Anleitungstext
        MenuUI.CreateScrollText(p, new Vector2(920f, 600f), new Vector2(0f, -10f), InstructionsText(), 30f, MenuUI.DarkText);

        var closeBtn = MenuUI.CreateButton(p, "Zurück", new Vector2(300f, 80f), new Vector2(0f, -370f), null);

        popup.panel = content;
        popup.closeButton = closeBtn;
        EditorUtility.SetDirty(popup);

        content.SetActive(false); // versteckt; zum Bearbeiten im Editor kurz aktivieren
        return popup;
    }

    /// <summary>Der Anleitungstext (TMP-Rich-Text). Zentral hier, damit er leicht editierbar ist.</summary>
    private static string InstructionsText()
    {
        const string head = "#6E409A"; // Lila-Überschriften (wie Menü-Palette)

        return
$"<b><color={head}>Die Story</color></b>\n" +
"Es ist spät. Die letzte Pizza des Abends muss noch raus, die Straßen sind leer – eigentlich ein Kinderspiel. Wäre da nicht das kleine Problem: Du bist viel zu betrunken zum Fahren. Dein Roller schlingert, dein Kopf dröhnt, und irgendwo da vorne wartet ein hungriger Kunde. Bring die Lieferung so weit wie möglich, solange dich der Rausch nicht von der Straße wirft.\n\n" +

$"<b><color={head}>Dein Ziel</color></b>\n" +
"Fahr so weit wie du kannst, sammle Punkte und knacke den Highscore. Je betrunkener du fährst, desto mehr Punkte pro Meter – aber desto chaotischer wird die Fahrt. Risiko zahlt sich aus. Das Spiel endet, wenn du alle Pizzen (Leben) verloren hast.\n\n" +

$"<b><color={head}>Steuerung</color></b>\n" +
"Der Roller fährt von allein – du hältst nur die Spur und das Tempo.\n" +
"Lenken links / rechts:   A / D   (oder Pfeiltasten links/rechts)\n" +
"Gas geben (vorlehnen):   W   (oder Pfeiltaste hoch)\n" +
"Bremsen (zurücklehnen):   S   (oder Pfeiltaste runter)\n" +
"Pause:   ESC\n" +
"<b>Wichtig:</b> Dein Roller kippt ständig von selbst zur Seite – das ist der Alkohol. Du musst permanent gegenlenken. Und Vorsicht: Je schneller du fährst, desto nervöser und schärfer reagiert die Lenkung.\n\n" +

$"<b><color={head}>Dein Pegel</color></b>\n" +
"Die Promille-Leiste (unten mittig, grün zu rot):\n" +
"•  Hoher Rausch = mehr Punkte (bis zum 6-fachen Multiplikator).\n" +
"•  Aber: je betrunkener, desto häufiger schlagen die Störeffekte zu.\n" +
"•  Der Rausch baut sich mit der Zeit von allein ab – wer wieder Punkte will, muss nachtanken.\n\n" +

$"<b><color={head}>Störeffekte – womit der Alkohol dich ärgert</color></b>\n" +
"Je nach Rauschpegel treffen dich zufällig:\n" +
"•  Schluckauf – ein kurzer, harter Ruck zur Seite. Schnell gegenlenken!\n" +
"•  Sekundenschlaf – Bildschirm wird dunkel, Steuerung komplett gesperrt. Du driftest hilflos weiter. (Tückisch: passiert eher, wenn du zu nüchtern wirst!)\n" +
"•  Ölpfütze – dein Roller dreht sich einmal komplett, Lenkung verkehrt herum.\n\n" +

$"<b><color={head}>Was du aufsammeln kannst</color></b>\n" +
"Am Straßenrand tauchen Dinge auf – fahr sie ein:\n" +
"•  Pizza  ->  +1 Leben (maximal 4). Deine Lebensversicherung.\n" +
"•  Getränke  ->  erhöhen deinen Rausch  ->  mehr Punkte, mehr Chaos. Deine Wahl.\n\n" +

$"<b><color={head}>Wodurch du Pizzen verlierst</color></b>\n" +
"•  Hindernisse frontal rammen  ->  -1 Leben\n" +
"•  Am Straßenrand kleben  ->  du stößt immer wieder an und verlierst Leben. Bleib in der Mitte!\n" +
"Nach jedem Treffer bist du kurz unverwundbar – nutz die Zeit, um dich zu fangen.";
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
