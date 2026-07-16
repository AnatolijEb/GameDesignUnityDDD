using UnityEngine;

/// <summary>
/// Zentrale, dauerhaft gespeicherte Spieleinstellungen (PlayerPrefs). Statisch, damit sie von
/// überall (Menü, Pause, Audio, Effekte) erreichbar sind und szenenübergreifend gelten.
///
/// Aktuell:
///   - <see cref="MusicVolume"/>: Master-Lautstärke der Musik (0..1, 0 = aus).
///   - Pro-Effekt-Schalter über <see cref="IsEffectEnabled"/> / <see cref="SetEffectEnabled"/>:
///     jeder Effekt (Hickup, Sekundenschlaf, Switchup, ...) lässt sich einzeln an/aus schalten.
///
/// <see cref="OnChanged"/> feuert bei jeder Änderung, damit z.B. laufende Musik sofort reagiert.
/// </summary>
public static class GameSettings
{
    private const string MusicKey = "Settings_MusicVolume";
    private const string EffectPrefix = "Settings_Effect_";

    /// <summary>
    /// Zentrale Liste der einzeln abschaltbaren Effekte (Schlüssel -> Anzeigename). Das Einstellungs-
    /// Popup baut daraus automatisch je einen Schalter. Ein NEUER Effekt braucht nur: hier einen
    /// Eintrag ergänzen UND am Effekt-SO denselben 'settingsKey' setzen.
    /// </summary>
    public static readonly (string key, string label)[] Effects =
    {
        ("hiccup", "Hickups"),
        ("microsleep", "Sekundenschlaf"),
        ("switchup", "Switchup"),
    };

    private static bool loaded;
    private static float musicVolume = 1f;

    /// <summary>Wird bei jeder Änderung einer Einstellung gefeuert.</summary>
    public static event System.Action OnChanged;

    private static void EnsureLoaded()
    {
        if (loaded) return;
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 1f));
        loaded = true;
    }

    public static float MusicVolume
    {
        get { EnsureLoaded(); return musicVolume; }
        set
        {
            EnsureLoaded();
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, musicVolume)) return;
            musicVolume = value;
            PlayerPrefs.SetFloat(MusicKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    /// <summary>Ist der Effekt mit diesem Schlüssel aktiv? Leerer Schlüssel = immer aktiv.</summary>
    public static bool IsEffectEnabled(string key)
    {
        if (string.IsNullOrEmpty(key)) return true;
        return PlayerPrefs.GetInt(EffectPrefix + key, 1) != 0;
    }

    public static void SetEffectEnabled(string key, bool enabled)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (IsEffectEnabled(key) == enabled) return;
        PlayerPrefs.SetInt(EffectPrefix + key, enabled ? 1 : 0);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }
}
