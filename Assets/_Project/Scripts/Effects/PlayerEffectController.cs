using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zentrale Verwaltung aller aktiven Spieler-Effekte. Wird auf dem Player-Root platziert.
///
/// EINZIGER Eintrittspunkt: <see cref="Apply"/>. Alle Auslöser (Zufalls-Timer,
/// Trigger-Zonen, Hindernis-Kontakt) rufen nur diese Methode auf. Dieser Controller
/// muss beim Hinzufügen NEUER Effekte NICHT geändert werden.
/// </summary>
public class PlayerEffectController : MonoBehaviour
{
    public static PlayerEffectController Instance { get; private set; }

    [Header("Referenzen (leer lassen = automatisch suchen)")]
    [Tooltip("Das Mofa/Visual, das für Hop & Sprung angehoben wird. Standard: visualTarget des PlayerBalanceController.")]
    [SerializeField] private Transform playerVisual;

    /// <summary>Für spätere HUD-/Sound-Hooks: wird bei jedem angewendeten Effekt gefeuert.</summary>
    public event System.Action<PlayerEffectSO> OnEffectApplied;

    private PlayerEffectContext ctx;
    private readonly List<PlayerEffectRuntime> active = new List<PlayerEffectRuntime>();

    private Vector3 baseVisualLocalPos;
    private float visualHeightThisFrame;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        PlayerBalanceController balance = GetComponent<PlayerBalanceController>();

        if (playerVisual == null && balance != null) playerVisual = balance.visualTarget;
        if (playerVisual == null) playerVisual = transform;

        ctx = new PlayerEffectContext
        {
            Controller = this,
            Balance = balance,
            Movement = GetComponent<PlayerMovementController>(),
            Throttle = GetComponent<PlayerThrottleController>(),
            Life = GetComponent<PlayerLifeSystem>(),
            Visual = playerVisual
        };

        baseVisualLocalPos = playerVisual.localPosition;
    }

    /// <summary>
    /// Wendet einen Effekt an. Sofort-Effekte laufen direkt durch (OnApply → OnRemove),
    /// Effekte mit Dauer werden bis zum Ablauf getickt.
    /// </summary>
    public void Apply(PlayerEffectSO effect)
    {
        if (effect == null) return;

        PlayerEffectRuntime runtime = effect.CreateRuntime();
        runtime.OnApply(ctx);

        if (runtime.HasDuration)
        {
            active.Add(runtime);
        }
        else
        {
            runtime.OnRemove(ctx);
        }

        Debug.Log($"[Effects] Applied: {effect.displayName}");
        OnEffectApplied?.Invoke(effect);
    }

    /// <summary>
    /// Von Effekten aufgerufen, um das Mofa (PlayerVisual) diesen Frame anzuheben (additiv).
    /// Die Neigung/Rotation macht weiterhin ausschließlich der PlayerBalanceController –
    /// hier wird NUR die Höhe (localPosition.y) verändert, es gibt also keinen Konflikt.
    /// </summary>
    public void AddVisualHeight(float y) => visualHeightThisFrame += y;

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            PlayerEffectRuntime r = active[i];
            r.Tick(ctx, dt);

            if (r.IsFinished)
            {
                r.OnRemove(ctx);
                active.RemoveAt(i);
            }
        }

        // Angesammelte Sprung-/Hop-Höhe auf das PlayerVisual anwenden.
        // Ohne aktiven Effekt ist visualHeightThisFrame == 0 => Mofa steht auf dem Boden.
        if (playerVisual != null)
        {
            playerVisual.localPosition = baseVisualLocalPos + Vector3.up * visualHeightThisFrame;
        }
        visualHeightThisFrame = 0f;
    }
}
