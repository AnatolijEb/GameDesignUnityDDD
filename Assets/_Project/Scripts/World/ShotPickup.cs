using UnityEngine;

public class ShotPickup : MonoBehaviour
{
    [Header("Shot Settings")]
    [SerializeField] private string drinkName = "Shot";
    [SerializeField] private float drunkennessIncrease = 200f;

    [Header("Sound (optional)")]
    [Tooltip("Wird beim Einsammeln abgespielt. Mehrere Clips = zufällige Auswahl. Leer = kein Sound.")]
    [SerializeField] private AudioClip[] collectSounds;
    [Range(0f, 1f)]
    [Tooltip("Lautstärke des Einsammel-Sounds.")]
    [SerializeField] private float soundVolume = 0.8f;

    /// <summary>
    /// Overrides this pickup's display name and drunkenness value. Used by PickupSpawner to turn
    /// a plain visual prefab into a specific drink variant (Beer, Wine, ...) at spawn time.
    /// </summary>
    public void Configure(string name, float value)
    {
        drinkName = name;
        drunkennessIncrease = value;
    }

    // Spielt einen zufälligen Einsammel-Sound. Da dieses Pickup sofort zerstört wird, läuft der Ton
    // NICHT auf dem eigenen Objekt (würde abgeschnitten), sondern auf einem kurzlebigen 2D-Audio-Objekt,
    // das sich nach dem Clip selbst entfernt.
    private void PlayCollectSound()
    {
        if (collectSounds == null || collectSounds.Length == 0) return;
        AudioClip clip = collectSounds[Random.Range(0, collectSounds.Length)];
        if (clip == null) return;

        GameObject sfx = new GameObject("ShotPickupSFX");
        sfx.transform.position = transform.position;
        AudioSource src = sfx.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = soundVolume;
        src.spatialBlend = 0f; // 2D -> immer gleich hörbar
        src.Play();
        Destroy(sfx, clip.length + 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DrunkennessSystem.Instance != null)
            {
                DrunkennessSystem.Instance.AddDrunkenness(drunkennessIncrease);
                Debug.Log($"[ShotPickup] Collected {drinkName}! Drunkenness increased by {drunkennessIncrease}.");
                PlayCollectSound();
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ShotPickup] DrunkennessSystem.Instance is null!");
            }
        }
    }
}
