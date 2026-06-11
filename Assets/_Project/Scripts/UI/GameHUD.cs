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

    private List<Image> lifeIcons = new List<Image>();

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
