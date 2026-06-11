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
        OnScoreChanged?.Invoke(currentScore);
        OnScoreDataChanged?.Invoke();
    }

    private void Update()
    {
        if (RunSpeedManager.Instance != null)
        {
            int newScore = Mathf.FloorToInt(RunSpeedManager.Instance.DistanceTravelled * pointsPerDistanceUnit);
            if (newScore != currentScore)
            {
                currentScore = newScore;
                OnScoreChanged?.Invoke(currentScore);
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
