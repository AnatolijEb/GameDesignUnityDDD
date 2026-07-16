using UnityEngine;

/// <summary>
/// Basis-Datenobjekt für ALLE Spieler-Effekte (Hickup, Steuerungs-Twist, Rampe, ...).
///
/// Ein konkreter Effekt besteht immer aus zwei Teilen:
///   1. einer <see cref="PlayerEffectSO"/>-Unterklasse (dieses Asset) mit den Tuning-Werten,
///   2. einer <see cref="PlayerEffectRuntime"/>-Unterklasse mit dem eigentlichen Verhalten.
///
/// Ein NEUER Effekt wird hinzugefügt, ohne bestehende Skripte zu ändern:
/// eine neue SO+Runtime-Datei anlegen, ein Asset davon erstellen und über einen
/// beliebigen Auslöser an <see cref="PlayerEffectController.Apply"/> geben.
/// </summary>
public abstract class PlayerEffectSO : ScriptableObject
{
    [Header("Effekt-Info")]
    [Tooltip("Nur für Debug-Logs / spätere HUD-Anzeige.")]
    public string displayName = "Effect";

    [Header("Sound (optional)")]
    [Tooltip("Wird beim Auslösen abgespielt. Mehrere Clips = zufällige Auswahl. Leer lassen = kein Sound.")]
    public AudioClip[] sounds;
    [Range(0f, 1f)]
    [Tooltip("Lautstärke des Effekt-Sounds.")]
    public float soundVolume = 0.8f;

    [Header("HUD-Anzeige (optional)")]
    [Tooltip("Icon, das im Effekt-HUD erscheint, wenn dieser Effekt ausgelöst wird. Leer = kein HUD-Icon. " +
             "Ein neuer Effekt muss NUR hier ein Icon gesetzt bekommen, um im HUD aufzutauchen.")]
    public Sprite hudIcon;
    [Tooltip("true = Icon bleibt dauerhaft sichtbar (Dauerzustand, z.B. Hickup). " +
             "false = Icon verschwindet mit einem Abschluss-Punch, wenn der Effekt endet (z.B. Öl-Dreher).")]
    public bool hudPersistsAfterEnd = false;

    [Header("Verhalten")]
    [Tooltip("true = Wird dieser Effekt erneut ausgelöst, während er schon aktiv ist, HEBT er sich auf, " +
             "statt sich zu stapeln (z.B. Öl-Dreher: zweite Pfütze dreht wieder nach vorne).")]
    public bool cancelIfActive = false;

    [Tooltip("true = Dieser Effekt kann über die Einstellungen einzeln abgeschaltet werden " +
             "(z.B. Hickup, Sekundenschlaf, Switchup). false = läuft immer (z.B. Belohnungen wie Rampe).")]
    public bool disableableBySettings = false;
    [Tooltip("Schlüssel für den Einstellungs-Schalter dieses Effekts (z.B. 'hiccup', 'microsleep', " +
             "'switchup'). Muss zu einem Eintrag in GameSettings.Effects passen. Nur relevant, wenn " +
             "'Disableable By Settings' an ist.")]
    public string settingsKey = "";

    /// <summary>Erzeugt eine frische Laufzeit-Instanz mit eigenem Zustand (Timer, Richtung, ...).</summary>
    public abstract PlayerEffectRuntime CreateRuntime();

    /// <summary>Liefert einen zufälligen Sound-Clip (oder null, wenn keiner gesetzt ist).</summary>
    public AudioClip GetRandomSound()
    {
        if (sounds == null || sounds.Length == 0) return null;
        return sounds[Random.Range(0, sounds.Length)];
    }
}
