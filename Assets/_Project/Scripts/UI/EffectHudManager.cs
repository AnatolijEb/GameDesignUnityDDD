using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zentrales Effekt-HUD. Zeigt für jeden aktiven Effekt mit gesetztem <see cref="PlayerEffectSO.hudIcon"/>
/// ein Icon in der Ecke an. Mehrere Effekte werden übereinander gestapelt (neuester oben), alle wabern,
/// und ein Icon wächst nur kurz ("Punch"), wenn mit ihm interagiert wird:
///   - beim Auslösen (auch erneutes Auslösen, z.B. jeder Hickup),
///   - beim Beenden eines zeitlichen Effekts (danach blendet das Icon aus).
///
/// ERWEITERBAR OHNE CODE-ÄNDERUNG: Ein neuer Effekt taucht automatisch auf, sobald sein SO ein
/// hudIcon gesetzt hat. Dauerzustände (hudPersistsAfterEnd = true, z.B. Hickup) bleiben stehen;
/// zeitliche Effekte (false, z.B. Öl-Dreher) verschwinden am Ende.
///
/// Auf das HUD-Objekt legen. Icons werden zur Laufzeit als Kinder dieses RectTransforms erzeugt;
/// ein evtl. vorhandenes eigenes Image wird ausgeblendet.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class EffectHudManager : MonoBehaviour
{
    [Header("Layout (Ecke)")]
    [Tooltip("Position des untersten Icons, relativ zur Ecke dieses Objekts.")]
    [SerializeField] private Vector2 basePosition = Vector2.zero;
    [Tooltip("Vertikaler Abstand zwischen gestapelten Icons (nach oben).")]
    [SerializeField] private float spacing = 105f;
    [Tooltip("Höhe jedes Icons in Pixeln (y). Die Breite ergibt sich aus dem Sprite-Seitenverhältnis, " +
             "damit alle Icons gleich groß wirken. (x wird nicht mehr direkt genutzt.)")]
    [SerializeField] private Vector2 iconSize = new Vector2(200f, 90f);
    [Tooltip("Anker/Pivot-Ecke der Icons (0,0 = unten-links, wie das bisherige Hickup-Icon).")]
    [SerializeField] private Vector2 corner = Vector2.zero;

    [Header("Look: Wabern")]
    [SerializeField] private float displayScale = 1.3f;
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private float wobbleRotationSpeed = 2f;
    [SerializeField] private float wobbleRotationAmplitude = 8f;
    [SerializeField] private float wobbleScaleSpeed = 1.6f;
    [SerializeField] private float wobbleScaleAmplitude = 0.05f;

    [Header("Look: Punch (bei Interaktion)")]
    [SerializeField] private float punchScaleAmount = 0.6f;
    [SerializeField] private float punchRotationAmount = 30f;
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private float punchDamping = 4f;
    [SerializeField] private float punchFrequency = 2.5f;

    [Header("Umstapeln")]
    [Tooltip("Wie schnell Icons beim Nachrücken ihre neue Position anfahren.")]
    [SerializeField] private float repositionSpeed = 12f;

    private PlayerEffectController controller;

    // Sichtbare Icons, ältestes zuerst (Index 0 = unten). Neue kommen oben drauf.
    private readonly List<Entry> entries = new List<Entry>();

    private class Entry
    {
        public PlayerEffectSO effect;
        public EffectIcon icon;
        public int refCount; // wie viele Instanzen dieses Effekts gerade aktiv sind
    }

    private void Awake()
    {
        // Falls das HUD-Objekt selbst noch ein statisches Image trägt (Altbestand vom Hickup-Icon),
        // ausblenden – die Icons werden dynamisch als Kinder erzeugt.
        Image own = GetComponent<Image>();
        if (own != null) own.enabled = false;
    }

    private void OnEnable() => TryBind();

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

    private void HandleApplied(PlayerEffectSO effect)
    {
        if (effect == null || effect.hudIcon == null) return;

        Entry e = Find(effect);
        if (e == null) e = CreateEntry(effect);

        e.refCount++;
        e.icon.Punch(); // wächst bei jedem Auslösen
    }

    private void HandleRemoved(PlayerEffectSO effect)
    {
        if (effect == null || effect.hudIcon == null) return;

        Entry e = Find(effect);
        if (e == null) return;

        if (e.refCount > 0) e.refCount--;

        // Dauerzustand (z.B. Hickup): Icon bleibt, kein Abschluss-Punch.
        if (effect.hudPersistsAfterEnd) return;

        // Noch weitere Instanzen aktiv (z.B. zweite Pfütze überlappt)? Dann noch nicht ausblenden.
        if (e.refCount > 0) return;

        // Zeitlicher Effekt endet: Abschluss-Punch, dann ausblenden und Slot frei machen.
        entries.Remove(e);
        e.icon.VanishAfterPunch(null);
        Restack();
    }

    private Entry Find(PlayerEffectSO effect)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].effect == effect) return entries[i];
        return null;
    }

    private Entry CreateEntry(PlayerEffectSO effect)
    {
        GameObject go = new GameObject($"EffectIcon_{effect.displayName}",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(EffectIcon));

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = corner;
        rt.anchorMax = corner;
        rt.pivot = corner;
        // Größe setzt EffectIcon.Setup anhand des Sprite-Seitenverhältnisses (einheitliche Höhe).

        EffectIcon icon = go.GetComponent<EffectIcon>();
        int slot = entries.Count; // neu = oberster Slot
        Vector2 pos = basePosition + Vector2.up * (spacing * slot);
        icon.Setup(effect.hudIcon, pos, BuildStyle());

        Entry e = new Entry { effect = effect, icon = icon, refCount = 0 };
        entries.Add(e);
        return e;
    }

    private void Restack()
    {
        for (int i = 0; i < entries.Count; i++)
            entries[i].icon.SetTargetPosition(basePosition + Vector2.up * (spacing * i));
    }

    private EffectIcon.Style BuildStyle() => new EffectIcon.Style
    {
        displayScale = displayScale,
        iconHeight = iconSize.y,
        appearDuration = appearDuration,
        wobbleRotationSpeed = wobbleRotationSpeed,
        wobbleRotationAmplitude = wobbleRotationAmplitude,
        wobbleScaleSpeed = wobbleScaleSpeed,
        wobbleScaleAmplitude = wobbleScaleAmplitude,
        punchScaleAmount = punchScaleAmount,
        punchRotationAmount = punchRotationAmount,
        punchDuration = punchDuration,
        punchDamping = punchDamping,
        punchFrequency = punchFrequency,
        repositionSpeed = repositionSpeed,
    };
}
