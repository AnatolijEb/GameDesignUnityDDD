using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor-Werkzeug: legt die Lebens-Icons als ECHTE, bearbeitbare Szenen-Objekte im vorhandenen
/// LivesContainer an und verdrahtet sie mit dem <see cref="GameHUD"/> (Feld 'lifeIcons').
///
/// Vorher wurden die Icons zur Laufzeit erzeugt und waren daher im Editor nicht bearbeitbar.
/// Nach dem Ausführen liegen echte Objekte in der Szene, die du frei anpassen kannst (Größe via
/// LayoutElement, Sprite, etc.). Der HorizontalLayoutGroup ordnet sie weiterhin automatisch an.
///
/// Bedienung:
///   1. Gameplay-Szene öffnen.
///   2. Menü „Tools/DDD/Setup Lebens-Icons (Gameplay-Szene)".
///   3. Szene speichern (Ctrl+S).
/// Erneut ausführbar (baut die Icons sauber neu auf).
/// </summary>
public static class GameHudSetup
{
    [MenuItem("Tools/DDD/Setup Lebens-Icons (Gameplay-Szene)")]
    public static void SetupLifeIcons()
    {
        GameHUD hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud == null)
        {
            Debug.LogWarning("[GameHudSetup] Kein GameHUD in der offenen Szene gefunden.");
            return;
        }

        var so = new SerializedObject(hud);
        Transform container = so.FindProperty("lifeIconContainer").objectReferenceValue as Transform;
        if (container == null)
        {
            Debug.LogWarning("[GameHudSetup] Am GameHUD ist kein 'lifeIconContainer' zugewiesen.", hud);
            return;
        }

        Sprite fullSprite = so.FindProperty("lifeFullSprite").objectReferenceValue as Sprite;
        Vector2 size = so.FindProperty("lifeIconSize").vector2Value;
        if (size.x <= 0f || size.y <= 0f) size = new Vector2(55f, 55f);

        var life = Object.FindFirstObjectByType<PlayerLifeSystem>();
        int maxLives = life != null ? life.MaxLives : 4;

        Undo.SetCurrentGroupName("Setup Lebens-Icons");
        int group = Undo.GetCurrentGroup();

        // Vorhandene Icons (aus früherem Lauf oder Laufzeit) sauber entfernen.
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
        }

        var icons = new Image[maxLives];
        for (int i = 0; i < maxLives; i++)
        {
            var iconObj = new GameObject($"LifeIcon_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            Undo.RegisterCreatedObjectUndo(iconObj, "Create Life Icon");
            iconObj.layer = container.gameObject.layer;

            var rt = iconObj.GetComponent<RectTransform>();
            rt.SetParent(container, false);
            rt.sizeDelta = size;

            var img = iconObj.GetComponent<Image>();
            img.sprite = fullSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;

            var le = iconObj.GetComponent<LayoutElement>();
            le.minWidth = size.x;
            le.minHeight = size.y;
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 0f;

            icons[i] = img;
        }

        // In das (serialisierte, private) Feld 'lifeIcons' des GameHUD schreiben.
        SerializedProperty iconsProp = so.FindProperty("lifeIcons");
        iconsProp.arraySize = maxLives;
        for (int i = 0; i < maxLives; i++)
        {
            iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
        }
        so.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
        Selection.activeGameObject = container.gameObject;

        Debug.Log($"[GameHudSetup] {maxLives} Lebens-Icons als echte Objekte angelegt und ins GameHUD verdrahtet. Szene speichern nicht vergessen (Ctrl+S).", hud);
    }
}
