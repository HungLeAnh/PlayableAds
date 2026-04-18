using UnityEngine;
using UnityEngine.UI;
using Luna.Unity;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public WordSearchGrid grid;
    public bool useTimer = true;
    public float gameDuration = 60f;
    public TextMeshProUGUI timerText;
    public Slider progressSlider;
    public GameObject gameOverPanel;
    
    private float timeRemaining;
    private bool isGameActive = true;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (useTimer)
        {
            timeRemaining = gameDuration;
        }
        UpdateTimerUI();
        UpdateProgress(0);
        LifeCycle.GameStarted();
    }

    void Update()
    {
        if (!isGameActive || !useTimer) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            OnGameLose();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (useTimer)
            {
                int seconds = Mathf.Max(0, Mathf.FloorToInt(timeRemaining));
                timerText.text = $"{seconds}";
            }
            else
            {
                timerText.text = "Unlimited Mode";
            }
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01(progress);
        }
    }

    public void OnGameComplete()
    {
        isGameActive = false;
        LifeCycle.GameEnded();
        Debug.Log("Game Complete!");
    }

    void OnGameLose()
    {
        isGameActive = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        LifeCycle.GameEnded();
        Debug.Log("Game Over - Time Out!");
    }
}
