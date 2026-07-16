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

    [Header("Sound")]
    [Tooltip("AudioSource für Effekt-Sounds. Leer lassen = wird automatisch als 2D-Quelle erzeugt.")]
    [SerializeField] private AudioSource audioSource;

    /// <summary>Für spätere HUD-/Sound-Hooks: wird bei jedem angewendeten Effekt gefeuert.</summary>
    public event System.Action<PlayerEffectSO> OnEffectApplied;

    /// <summary>
    /// Wird gefeuert, wenn ein Effekt endet/entfernt wird. Übergibt das Quell-SO (oder null bei
    /// per Code gebauten Runtimes). Vom Effekt-HUD genutzt, um Icons auszublenden bzw. den
    /// Abschluss-Punch zu spielen.
    /// </summary>
    public event System.Action<PlayerEffectSO> OnEffectRemoved;

    /// <summary>
    /// Zusätzlicher Dreh-Winkel (Grad, um die Hochachse) für das Mofa, den Effekte setzen
    /// können (z.B. Öl-Dreher). Wird vom PlayerBalanceController in die Visual-Rotation
    /// eingerechnet. 0 = keine Extra-Drehung. Effekte setzen ihn im Tick und nullen ihn im OnRemove.
    /// </summary>
    public float VisualYaw { get; set; }

    /// <summary>
    /// Zusätzlicher Nick-Winkel (Grad, um die Seitenachse) für das Mofa – für einen
    /// Überschlag/Purzelbaum, z.B. wenn der Spieler frontal in ein Hindernis fährt.
    /// Wird wie <see cref="VisualYaw"/> vom PlayerBalanceController in die Visual-Rotation
    /// eingerechnet. 0 = kein Überschlag. Effekte setzen ihn im Tick und nullen ihn im OnRemove.
    /// </summary>
    public float VisualPitch { get; set; }

    private PlayerEffectContext ctx;
    private readonly List<PlayerEffectRuntime> active = new List<PlayerEffectRuntime>();

    /// <summary>
    /// True, solange mindestens ein Effekt MIT DAUER läuft. Von den Zufalls-Spawnern genutzt,
    /// um Effekte zu serialisieren (nicht überlappen / nicht "hintereinander" auslösen).
    /// </summary>
    public bool HasActiveEffect => active.Count > 0;

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
            CollisionHandler = GetComponent<PlayerCollisionHandler>(),
            Visual = playerVisual
        };

        baseVisualLocalPos = playerVisual.localPosition;

        // Eigene 2D-AudioSource sicherstellen (getrennt von anderen Sounds am Player).
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D -> immer gleich hörbar
        audioSource.mute = false;
        audioSource.volume = 1f;       // Basis 1, PlayOneShot skaliert exakt auf soundVolume
    }

    /// <summary>
    /// Wendet einen Effekt an. Sofort-Effekte laufen direkt durch (OnApply → OnRemove),
    /// Effekte mit Dauer werden bis zum Ablauf getickt.
    /// </summary>
    public void Apply(PlayerEffectSO effect)
    {
        if (effect == null) return;

        // Über die Einstellungen einzeln abschaltbare Effekte (Hickup, Sekundenschlaf, Switchup)
        // ignorieren, wenn ihr Schalter aus ist. Belohnungen (z.B. Rampe) haben das Flag nicht.
        if (effect.disableableBySettings && !GameSettings.IsEffectEnabled(effect.settingsKey)) return;

        // Selbst-aufhebende Effekte (z.B. Öl-Dreher): ist bereits einer aktiv, hebt das erneute
        // Auslösen ihn auf, statt einen zweiten zu stapeln.
        if (effect.cancelIfActive && TryCancelActive(effect)) return;

        PlayerEffectRuntime runtime = effect.CreateRuntime();
        runtime.Source = effect;
        runtime.OnApply(ctx);

        if (runtime.HasDuration)
        {
            active.Add(runtime);
        }
        else
        {
            runtime.OnRemove(ctx);
            OnEffectRemoved?.Invoke(effect);
        }

        // Optionalen Effekt-Sound abspielen.
        AudioClip clip = effect.GetRandomSound();
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, effect.soundVolume);
        }

        Debug.Log($"[Effects] Applied: {effect.displayName}");
        OnEffectApplied?.Invoke(effect);
    }

    /// <summary>
    /// Wie <see cref="Apply"/>, nimmt aber eine bereits im Code gebaute Runtime entgegen
    /// (z.B. eine Kollisions-Reaktion, deren Richtung erst zur Laufzeit feststeht und die
    /// daher nicht aus einem festen SO-Asset erzeugt werden kann).
    /// </summary>
    public void ApplyRuntime(PlayerEffectRuntime runtime)
    {
        if (runtime == null) return;

        runtime.OnApply(ctx);

        if (runtime.HasDuration)
        {
            active.Add(runtime);
        }
        else
        {
            runtime.OnRemove(ctx);
            OnEffectRemoved?.Invoke(runtime.Source);
        }
    }

    /// <summary>
    /// Von Effekten aufgerufen, um das Mofa (PlayerVisual) diesen Frame anzuheben (additiv).
    /// Die Neigung/Rotation macht weiterhin ausschließlich der PlayerBalanceController –
    /// hier wird NUR die Höhe (localPosition.y) verändert, es gibt also keinen Konflikt.
    /// </summary>
    public void AddVisualHeight(float y) => visualHeightThisFrame += y;

    /// <summary>
    /// Hebt eine bereits aktive Instanz desselben Effekts auf. Liefert true, wenn eine gefunden wurde
    /// (dann wird KEIN neuer Effekt hinzugefügt). Sanfter Abbruch (CancelGracefully) lässt die Runtime
    /// noch normal auslaufen; sonst wird sie sofort entfernt.
    /// </summary>
    private bool TryCancelActive(PlayerEffectSO effect)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i].Source != effect) continue;

            if (!active[i].CancelGracefully())
            {
                PlayerEffectRuntime removed = active[i];
                removed.OnRemove(ctx);
                active.RemoveAt(i);
                OnEffectRemoved?.Invoke(removed.Source);
            }
            return true;
        }
        return false;
    }

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
                OnEffectRemoved?.Invoke(r.Source);
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
