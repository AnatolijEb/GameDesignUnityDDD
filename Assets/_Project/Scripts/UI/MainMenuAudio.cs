using UnityEngine;

/// <summary>
/// Menü-Audio: spielt die Hintergrundmusik (standardmäßig ~30% leiser als im Spiel) in Schleife
/// und wirft dazwischen in zufälligen Abständen Hickup-Sounds ein. Legt seine AudioSources selbst
/// an – einfach auf ein dauerhaft aktives GameObject im Main-Menu legen.
/// </summary>
public class MainMenuAudio : MonoBehaviour
{
    [Header("Musik")]
    [SerializeField] private AudioClip music;
    [Tooltip("Menü-Lautstärke der Musik. 0.35 = ca. 30% leiser als die Spiel-Lautstärke (0.5).")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.35f;

    [Header("Zufalls-Hickups")]
    [SerializeField] private AudioClip[] hiccupSounds;
    [Range(0f, 1f)] [SerializeField] private float hiccupVolume = 0.7f;
    [Tooltip("Minimale Pause zwischen zwei Hickups (Sekunden).")]
    [SerializeField] private float minInterval = 4f;
    [Tooltip("Maximale Pause zwischen zwei Hickups (Sekunden).")]
    [SerializeField] private float maxInterval = 12f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private float nextHiccupTime;

    private void OnEnable() => GameSettings.OnChanged += ApplyMusicVolume;
    private void OnDisable() => GameSettings.OnChanged -= ApplyMusicVolume;

    /// <summary>Lautstärke live an die Einstellung anpassen (Master · Menü-Basis-Lautstärke).</summary>
    private void ApplyMusicVolume()
    {
        if (musicSource != null) musicSource.volume = musicVolume * GameSettings.MusicVolume;
    }

    private void Start()
    {
        EnsureAudioListener();

        // Eigene 2D-Musikquelle (2D = immer gleich laut, unabhängig von Kamera-Position).
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume * GameSettings.MusicVolume;
        if (music != null) musicSource.Play();

        // Getrennte Quelle für die Hickup-Einwürfe, damit sie die Musik nicht abschneiden.
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        ScheduleNextHiccup();
    }

    private void ScheduleNextHiccup()
    {
        nextHiccupTime = Time.unscaledTime + Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (hiccupSounds == null || hiccupSounds.Length == 0) return;
        if (Time.unscaledTime < nextHiccupTime) return;

        AudioClip clip = hiccupSounds[Random.Range(0, hiccupSounds.Length)];
        if (clip != null) sfxSource.PlayOneShot(clip, hiccupVolume);
        ScheduleNextHiccup();
    }

    /// <summary>Ohne AudioListener ist gar nichts hörbar – sicherstellen, dass genau einer da ist.</summary>
    private void EnsureAudioListener()
    {
        if (Object.FindFirstObjectByType<AudioListener>() != null) return;
        Camera cam = Camera.main;
        (cam != null ? cam.gameObject : gameObject).AddComponent<AudioListener>();
    }
}
