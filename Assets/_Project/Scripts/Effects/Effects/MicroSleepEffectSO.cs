using UnityEngine;

/// <summary>
/// Sekundenschlaf: Der Charakter „nickt" kurz ein. Für die eingestellte Dauer ist die
/// Steuerung gesperrt (der Spieler kann nicht mehr lenken), während über dem Kopf eine
/// ZZZ-Anzeige erscheint (<see cref="SleepIndicator"/>).
///
/// Der Zufalls-Drift des Mofas läuft während des Schlafs weiter – das Mofa driftet also
/// unkontrolliert, weil nicht gegengelenkt werden kann. Das ist die „Strafe"; es wird
/// KEIN Leben abgezogen.
///
/// Ausgelöst wird der Effekt typischerweise über den <see cref="MicroSleepSpawner"/>,
/// der ihn im nüchternen Zustand häufiger auftreten lässt (müde statt betrunken).
/// </summary>
[CreateAssetMenu(fileName = "SO_Effect_MicroSleep", menuName = "DDD/Effects/Micro Sleep")]
public class MicroSleepEffectSO : PlayerEffectSO
{
    [Header("Sekundenschlaf")]
    [Tooltip("Wie lange der Charakter schläft (Sekunden). In dieser Zeit ist die Steuerung gesperrt.")]
    public float sleepDuration = 1.5f;

    [Tooltip("Lenkung (links/rechts) während des Schlafs sperren.")]
    public bool lockSteering = true;

    [Tooltip("Gas/Bremse (hoch/runter) während des Schlafs ebenfalls sperren. " +
             "Aus = nur die Lenkung ist gesperrt, Beschleunigen/Bremsen geht weiter.")]
    public bool lockThrottle = true;

    public override PlayerEffectRuntime CreateRuntime() => new MicroSleepEffectRuntime(this);
}

public class MicroSleepEffectRuntime : PlayerEffectRuntime
{
    private readonly MicroSleepEffectSO data;
    private SleepIndicator indicator;

    public MicroSleepEffectRuntime(MicroSleepEffectSO data)
    {
        this.data = data;
        duration = data.sleepDuration;
    }

    public override void OnApply(PlayerEffectContext ctx)
    {
        // Steuerung sperren (Zähler-basiert -> stapelbar/selbst-aufhebend, siehe OnRemove).
        if (data.lockSteering && ctx.Balance != null) ctx.Balance.controlLockCount++;
        if (data.lockThrottle && ctx.Throttle != null) ctx.Throttle.controlLockCount++;

        // ZZZ-Anzeige über dem Kopf einschalten (falls am Spieler vorhanden).
        if (ctx.Controller != null)
        {
            indicator = ctx.Controller.GetComponentInChildren<SleepIndicator>(true);
            if (indicator != null) indicator.Show();
        }
    }

    public override void OnRemove(PlayerEffectContext ctx)
    {
        // Sperren exakt wieder freigeben (jeder ++ bekommt sein --).
        if (data.lockSteering && ctx.Balance != null) ctx.Balance.controlLockCount--;
        if (data.lockThrottle && ctx.Throttle != null) ctx.Throttle.controlLockCount--;

        if (indicator != null) indicator.Hide();
    }
}
