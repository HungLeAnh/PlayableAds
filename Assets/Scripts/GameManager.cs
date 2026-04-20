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
    public Image vignetteImage;
    public float vignettePulseSpeed = 4f;
    
    private float timeRemaining;
    public bool isGameActive = true;
    
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
        
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
            vignetteImage.raycastTarget = false; // Ensure it doesn't block input
        }
    }

    void Update()
    {
        if (!isGameActive || !useTimer) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();
        UpdateVignette();

        if (timeRemaining <= 0)
        {
            OnGameLose();
        }
    }

    void UpdateVignette()
    {
        if (vignetteImage == null) return;

        // Changed threshold to 6 seconds as requested
        if (timeRemaining <= 6f && timeRemaining > 0)
        {
            if (!vignetteImage.gameObject.activeSelf) vignetteImage.gameObject.SetActive(true);
            
            // Pulse at constant speed (removed speedMultiplier)
            float alpha = (Mathf.Sin(Time.time * vignettePulseSpeed) + 1f) / 2f;
            alpha *= 0.6f; // Max alpha 0.6
            vignetteImage.color = new Color(1, 0, 0, alpha);
        }
        else
        {
            if (vignetteImage.gameObject.activeSelf) vignetteImage.gameObject.SetActive(false);
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
        if (vignetteImage != null) vignetteImage.gameObject.SetActive(false);
        LifeCycle.GameEnded();
        Debug.Log("Game Complete!");
    }

    void OnGameLose()
    {
        isGameActive = false;
        if (vignetteImage != null) vignetteImage.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        LifeCycle.GameEnded();
        Debug.Log("Game Over - Time Out!");
    }
}
