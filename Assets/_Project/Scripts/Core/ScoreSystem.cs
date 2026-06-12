using UnityEngine;
using System;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [Header("Scoring Settings")]
    [SerializeField] private float pointsPerDistanceUnit = 10f;

    private int currentScore;
    private int highscore;
    private int lastRunScore;
    private float lastDistance;
    private float scoreAccumulator;

    public int CurrentScore => currentScore;
    public int HighScore => highscore;
    public int LastRunScore => lastRunScore;

    public event Action<int> OnScoreChanged;
    public event Action OnScoreDataChanged;

    private const string HighScoreKey = "HighScore";
    private const string LastRunScoreKey = "LastRunScore";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentScore = 0;
        scoreAccumulator = 0f;
        if (RunSpeedManager.Instance != null)
        {
            lastDistance = RunSpeedManager.Instance.DistanceTravelled;
        }
        else
        {
            lastDistance = 0f;
        }
        OnScoreChanged?.Invoke(currentScore);
        OnScoreDataChanged?.Invoke();
    }

    private void Update()
    {
        if (RunSpeedManager.Instance != null)
        {
            float currentDistance = RunSpeedManager.Instance.DistanceTravelled;
            float deltaDistance = currentDistance - lastDistance;
            lastDistance = currentDistance;

            if (deltaDistance > 0f)
            {
                int multiplier = 1;
                if (DrunkennessSystem.Instance != null)
                {
                    multiplier = DrunkennessSystem.Instance.CurrentMultiplier;
                }

                scoreAccumulator += deltaDistance * pointsPerDistanceUnit * multiplier;
                int addedScore = Mathf.FloorToInt(scoreAccumulator);
                if (addedScore > 0)
                {
                    currentScore += addedScore;
                    scoreAccumulator -= addedScore;
                    OnScoreChanged?.Invoke(currentScore);
                }
            }
        }
    }

    private void LoadScores()
    {
        highscore = PlayerPrefs.GetInt(HighScoreKey, 0);
        lastRunScore = PlayerPrefs.GetInt(LastRunScoreKey, 0);
        Debug.Log($"[ScoreSystem] Loaded HighScore: {highscore}, LastRunScore: {lastRunScore}");
    }

    public void SaveScores()
    {
        lastRunScore = currentScore;
        PlayerPrefs.SetInt(LastRunScoreKey, lastRunScore);
        Debug.Log($"[ScoreSystem] LastRunScore saved: {lastRunScore}");

        if (currentScore > highscore)
        {
            highscore = currentScore;
            PlayerPrefs.SetInt(HighScoreKey, highscore);
            Debug.Log($"[ScoreSystem] New HighScore saved: {highscore}");
        }

        PlayerPrefs.Save();
        OnScoreDataChanged?.Invoke();
    }
}
