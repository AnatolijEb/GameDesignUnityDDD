using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLifeSystem playerLifeSystem;
    
    [Header("Lives Display")]
    [SerializeField] private GameObject lifeIconPrefab; // Not strictly requested as prefab, but cleaner.
    [SerializeField] private Transform lifeIconContainer;
    [SerializeField] private Sprite lifeFullSprite;
    [SerializeField] private Sprite lifeEmptySprite;
    
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI lastRunScoreText;
    [SerializeField] private TextMeshProUGUI comparisonText;
    [SerializeField] private TextMeshProUGUI scoreMultiplierText;

    [Header("Drunkenness Display")]
    [SerializeField] private UnityEngine.UI.Image drunkennessBarFill;
    [Tooltip("Fill color while sober (left end of the bar).")]
    [SerializeField] private Color drunkennessLowColor = new Color(0.30f, 0.82f, 0.35f);
    [Tooltip("Fill color at maximum drunkenness (right end of the bar).")]
    [SerializeField] private Color drunkennessHighColor = new Color(0.90f, 0.15f, 0.15f);
    [Tooltip("Optional marker that always sits exactly at the current drunkenness position on the bar.")]
    [SerializeField] private RectTransform drunkennessPointer;

    private List<Image> lifeIcons = new List<Image>();
    private DrunkennessSystem drunkennessSystem;

    private void Awake()
    {
        if (playerLifeSystem == null)
        {
            playerLifeSystem = Object.FindFirstObjectByType<PlayerLifeSystem>();
        }
    }

    private void OnEnable()
    {
        if (playerLifeSystem != null)
        {
            playerLifeSystem.OnLivesChanged += UpdateLivesDisplay;
        }

        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged += HandleScoreChanged;
            ScoreSystem.Instance.OnScoreDataChanged += HandleScoreDataChanged;
        }

        BindDrunkennessSystem();
    }

    private void OnDisable()
    {
        if (playerLifeSystem != null)
        {
            playerLifeSystem.OnLivesChanged -= UpdateLivesDisplay;
        }

        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged -= HandleScoreChanged;
            ScoreSystem.Instance.OnScoreDataChanged -= HandleScoreDataChanged;
        }

        UnbindDrunkennessSystem();
    }

    private void Start()
    {
        InitializeLifeIcons();
        if (playerLifeSystem != null)
        {
            UpdateLivesDisplay(playerLifeSystem.CurrentLives, playerLifeSystem.MaxLives);
        }
        
        if (ScoreSystem.Instance != null)
        {
            HandleScoreChanged(ScoreSystem.Instance.CurrentScore);
            HandleScoreDataChanged();
        }

        BindDrunkennessSystem();
        if (drunkennessSystem != null)
        {
            HandleDrunkennessChanged(drunkennessSystem.CurrentDrunkenness, drunkennessSystem.MaxDrunkenness);
            HandleMultiplierChanged(drunkennessSystem.CurrentMultiplier);
        }
    }

    private void BindDrunkennessSystem()
    {
        if (drunkennessSystem != null) return;

        drunkennessSystem = DrunkennessSystem.Instance;
        if (drunkennessSystem == null)
        {
            drunkennessSystem = Object.FindFirstObjectByType<DrunkennessSystem>();
        }

        if (drunkennessSystem != null)
        {
            drunkennessSystem.OnDrunkennessChanged += HandleDrunkennessChanged;
            drunkennessSystem.OnMultiplierChanged += HandleMultiplierChanged;
            Debug.Log($"[GameHUD] Drunkenness HUD initialized. DrunkennessSystem reference found. Initial value: {drunkennessSystem.CurrentDrunkenness}");
        }
    }

    private void UnbindDrunkennessSystem()
    {
        if (drunkennessSystem != null)
        {
            drunkennessSystem.OnDrunkennessChanged -= HandleDrunkennessChanged;
            drunkennessSystem.OnMultiplierChanged -= HandleMultiplierChanged;
            drunkennessSystem = null;
        }
    }

    private void HandleDrunkennessChanged(float current, float max)
    {
        float fillAmount = Mathf.Clamp01((max > 0f) ? (current / max) : 0f);
        if (drunkennessBarFill != null)
        {
            drunkennessBarFill.fillAmount = fillAmount;
            drunkennessBarFill.color = Color.Lerp(drunkennessLowColor, drunkennessHighColor, fillAmount);
        }
        if (drunkennessPointer != null)
        {
            drunkennessPointer.anchorMin = new Vector2(fillAmount, drunkennessPointer.anchorMin.y);
            drunkennessPointer.anchorMax = new Vector2(fillAmount, drunkennessPointer.anchorMax.y);
        }
        Debug.Log($"[GameHUD] Update received when drunkenness changes. Current: {current}, Max: {max}, Fill Amount: {fillAmount}");
    }

    private void HandleMultiplierChanged(int multiplier)
    {
        if (scoreMultiplierText != null)
        {
            scoreMultiplierText.text = $"Multiplier: {multiplier}x";
        }
        Debug.Log($"[GameHUD] Multiplier changed. New multiplier: {multiplier}x");
    }

    private void HandleScoreChanged(int score)
    {
        UpdateScore(score);
        UpdateComparison(score);
    }

    private void HandleScoreDataChanged()
    {
        if (ScoreSystem.Instance == null) return;

        if (highscoreText != null)
            highscoreText.text = $"High: {ScoreSystem.Instance.HighScore}";
        
        if (lastRunScoreText != null)
            lastRunScoreText.text = $"Last: {ScoreSystem.Instance.LastRunScore}";
        
        UpdateComparison(ScoreSystem.Instance.CurrentScore);
    }

    private void UpdateComparison(int score)
    {
        if (comparisonText == null || ScoreSystem.Instance == null) return;

        int high = ScoreSystem.Instance.HighScore;
        int last = ScoreSystem.Instance.LastRunScore;

        if (score < last && last > 0)
        {
            comparisonText.text = $"Last run in: {last - score}";
            comparisonText.gameObject.SetActive(true);
        }
        else if (score < high && high > 0)
        {
            comparisonText.text = $"Highscore in: {high - score}";
            comparisonText.gameObject.SetActive(true);
        }
        else
        {
            comparisonText.text = "";
            comparisonText.gameObject.SetActive(false);
        }
    }

    private void InitializeLifeIcons()
    {
        // Clear existing children in container
        foreach (Transform child in lifeIconContainer)
        {
            Destroy(child.gameObject);
        }
        lifeIcons.Clear();

        // Create 4 icons as per requirements
        int maxLives = playerLifeSystem != null ? playerLifeSystem.MaxLives : 4;
        for (int i = 0; i < maxLives; i++)
        {
            GameObject iconObj = new GameObject("LifeIcon_" + i, typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(lifeIconContainer, false);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = lifeFullSprite;
            img.raycastTarget = false;
            lifeIcons.Add(img);
        }
    }

    private void UpdateLivesDisplay(int currentLives, int maxLives)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (i < currentLives)
            {
                lifeIcons[i].sprite = lifeFullSprite;
            }
            else
            {
                lifeIcons[i].sprite = lifeEmptySprite;
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    // Alias for SetScore as per requirements
    public void SetScore(int score) => UpdateScore(score);
}
