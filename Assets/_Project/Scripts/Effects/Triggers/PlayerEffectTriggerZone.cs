using UnityEngine;

/// <summary>
/// Auslöser #2: Wendet beim Durchfahren einen Effekt an. Auf ein Prefab mit einem
/// Trigger-Collider packen (Pfütze, Schanze, Rampe, ...).
///
/// Braucht KEINE Änderung am Kollisions-/Lebens-System – ideal für positive oder
/// neutrale Effekte, die keinen Schaden und keinen Crash-Sound auslösen sollen.
/// Das Objekt muss NICHT den Tag "Obstacle" tragen.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerEffectTriggerZone : MonoBehaviour
{
    [Header("Effekt")]
    public PlayerEffectSO effect;
    [Tooltip("Nur einmal auslösen (verhindert Mehrfach-Trigger bei breiten Collidern).")]
    public bool triggerOnce = true;

    private bool used;

    private void Reset()
    {
        // Beim Hinzufügen der Komponente den Collider gleich als Trigger setzen.
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && used) return;

        if (PlayerEffectController.Instance != null && effect != null)
        {
            PlayerEffectController.Instance.Apply(effect);
            used = true;
        }
    }
}
