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

    /// <summary>Erzeugt eine frische Laufzeit-Instanz mit eigenem Zustand (Timer, Richtung, ...).</summary>
    public abstract PlayerEffectRuntime CreateRuntime();
}
