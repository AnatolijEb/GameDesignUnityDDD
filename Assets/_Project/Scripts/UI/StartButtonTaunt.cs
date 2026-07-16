using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Verwandelt beim Überfahren (Hover) des Start-Buttons einen vorhandenen TMP-Text (z.B. das
/// "Seriously Don't!"-Feld) in einen zufälligen, warnenden – aber witzig gemeinten – Spruch, der
/// dem Spieler klarmacht, dass Losfahren eine ganz schlechte Idee ist. Der Spruch BLEIBT stehen;
/// erst beim erneuten Überfahren erscheint ein neuer.
///
/// Anhängen: auf den StartButton legen und das "Seriously Don't!"-Textfeld (Text2, in DERSELBEN
/// Szene) als "Taunt Text" zuweisen. Läuft problemlos zusätzlich zum vorhandenen ButtonCursorHover
/// und zu UIPulseScale – Unity ruft beide Hover-Handler auf, und das Pulsieren skaliert nur.
/// </summary>
public class StartButtonTaunt : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("Textfeld, das beim Hover die Sprüche zeigt (das 'Seriously Don't!'-Feld). Muss in DERSELBEN Szene liegen.")]
    [SerializeField] private TMP_Text tauntText;

    [Tooltip("Sprüche, die zufällig beim Hover erscheinen. Frei ergänzbar/änderbar.")]
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

    private int lastIndex = -1;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tauntText == null || taunts == null || taunts.Length == 0) return;

        int index = Random.Range(0, taunts.Length);
        // Nicht zweimal hintereinander denselben Spruch zeigen.
        if (taunts.Length > 1 && index == lastIndex)
            index = (index + 1) % taunts.Length;
        lastIndex = index;

        tauntText.text = taunts[index];
    }
}
