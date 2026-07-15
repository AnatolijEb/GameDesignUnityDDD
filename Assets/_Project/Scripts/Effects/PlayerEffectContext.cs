using UnityEngine;

/// <summary>
/// Bündel aller Referenzen, die ein Effekt-Runtime benötigt. Wird EINMALIG vom
/// <see cref="PlayerEffectController"/> gebaut und an jeden Runtime durchgereicht,
/// damit die Effekte selbst nichts per GetComponent/Find suchen müssen.
///
/// Neue Effekte brauchen mehr Referenzen? Einfach hier ein Feld ergänzen und im
/// Controller einmalig befüllen – die Effekte greifen dann darüber zu.
/// </summary>
public class PlayerEffectContext
{
    public PlayerEffectController Controller;
    public PlayerBalanceController Balance;
    public PlayerMovementController Movement;
    public PlayerThrottleController Throttle;
    public PlayerLifeSystem Life;

    /// <summary>Das PlayerVisual (Mofa). Wird für Hop/Sprung genutzt (nur localPosition.y).</summary>
    public Transform Visual;
}
