using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Verwandelt beim Überfahren (Hover) eines Buttons einen vorhandenen TMP-Text in einen zufälligen,
/// warnenden – aber witzig gemeinten – Spruch. Der Spruch BLEIBT stehen; erst beim erneuten
/// Überfahren erscheint ein neuer.
///
/// Wiederverwendbar über "Message Set":
///   - Custom          -> nutzt die frei editierbare "Taunts"-Liste unten (Main-Menu StartButton).
///   - Game Over Restart-> "willst du das echt nochmal machen?"-Sprüche (auf den RestartButton).
///   - Game Over Menu   -> "richtig so, geh nach Hause"-Sprüche (auf den MenuButton).
///
/// Anhängen: auf den Button legen und das Ziel-Textfeld (in DERSELBEN Szene) als "Taunt Text"
/// zuweisen. Läuft problemlos neben ButtonCursorHover / UIPulseScale – Unity ruft beide Hover-
/// Handler auf, und das Pulsieren skaliert nur.
/// </summary>
public class StartButtonTaunt : MonoBehaviour, IPointerEnterHandler
{
    public enum TauntSet { Custom, GameOverRestart, GameOverMenu }

    [Tooltip("Textfeld, das beim Hover die Sprüche zeigt. Muss in DERSELBEN Szene liegen.")]
    [SerializeField] private TMP_Text tauntText;

    [Tooltip("Welches Sprüche-Set benutzt wird. 'Custom' = die 'Taunts'-Liste unten; sonst ein eingebautes Set.")]
    [SerializeField] private TauntSet messageSet = TauntSet.Custom;

    [Tooltip("Nur bei Message Set = Custom: Sprüche, die zufällig beim Hover erscheinen. Frei ergänzbar/änderbar.")]
    [SerializeField]
    private string[] taunts =
    {
        "Your mom would be so mad.",
        "You really shouldn't do that.",
        "This is a genuinely terrible idea.",
        "Please. Reconsider.",
        "Nothing good is behind this button.",
        "Are you SURE about this?",
        "Think of the pizzas.",
        "You can barely stand up.",
        "The road called. It's scared.",
        "Last chance to just walk home.",
        "Maybe call a taxi instead?",
        "Your insurance says absolutely not.",
        "That lamppost never stood a chance.",
        "This will not end well.",
        "Grandma is watching, you know.",
        "Keys down. Step away slowly.",
        "Bad. Idea.",
        "Even the mofa looks nervous.",
        "The pizza can wait. Really.",
        "Statistically speaking... yikes.",
        "Have you considered simply... not?",
        "The sidewalk misses you already.",
        "This is how legends get grounded.",
        "Your future self is begging you.",
        "We both know this is wrong.",
        "Don't make us say 'told you so'.",
        "Hard no from the pavement.",
        "One more won't fix your aim.",
        "Somewhere, a helmet is crying.",
        "You had ONE job: stay home.",
        "The stars are NOT aligned for this.",
        "Seriously. Just don't.",
    };

    // Eingebaute Sets für den Game-Over-Screen. Kein Tippen im Inspector nötig – über "Message Set" wählbar.
    private static readonly string[] GameOverRestartTaunts =
    {
        "Haven't you learned? That's how you want to spend your second chance at life?",
        "Well, who could have predicted this.",
        "Well, who would have thought.",
        "Back for more punishment already?",
        "Sure, THIS time it'll go great.",
        "Round two: same idea, same result.",
        "You clearly didn't learn a thing.",
        "Insanity is doing this again and again.",
        "The lamppost remembers you.",
        "Your guardian angel just clocked out.",
        "Bold of you to assume it'll go different.",
        "Again? Truly inspiring stubbornness.",
        "The pizzas forgive you. The road won't.",
        "One more run, one more regret.",
        "Hope is not a driving strategy.",
    };

    private static readonly string[] GameOverMenuTaunts =
    {
        "That's right, just go home.",
        "Good call. Walk it off.",
        "The couch is calling. Answer it.",
        "Finally, a smart decision.",
        "Yes. Home. Safe. Boring. Alive.",
        "Your bed misses you anyway.",
        "Retreat is the bravest move here.",
        "Go on, quit while you're... behind.",
        "The road thanks you for leaving.",
        "Sober choice. We're proud.",
        "Nobody will judge you. Much.",
        "Leaving already? Wise. Very wise.",
        "Take the L. Take it home.",
        "Home it is. The pizzas understand.",
        "Smart. The lamppost is relieved.",
    };

    private int lastIndex = -1;

    private string[] ActiveTaunts => messageSet switch
    {
        TauntSet.GameOverRestart => GameOverRestartTaunts,
        TauntSet.GameOverMenu => GameOverMenuTaunts,
        _ => taunts,
    };

    public void OnPointerEnter(PointerEventData eventData)
    {
        string[] source = ActiveTaunts;
        if (tauntText == null || source == null || source.Length == 0) return;

        int index = Random.Range(0, source.Length);
        // Nicht zweimal hintereinander denselben Spruch zeigen.
        if (source.Length > 1 && index == lastIndex)
            index = (index + 1) % source.Length;
        lastIndex = index;

        tauntText.text = source[index];
    }
}
