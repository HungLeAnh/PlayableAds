using UnityEngine;
using UnityEngine.UI;
using Luna.Unity;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public WordSearchGrid grid;
    public List<LevelData> levels;
    public int currentLevelIndex = 0;
    public bool useTimer = true;
    public float gameDuration = 60f;
    public TextMeshProUGUI timerText;
    public Slider progressSlider;
    public GameObject gameOverPanel;
    public Image vignetteImage;
    public float vignettePulseSpeed = 4f;
    public TransitionUI transitionUI;
    public GamePanelUI resultPanel;

    private float timeRemaining;
    public bool isGameActive = true;
    private bool isWinState = false;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeLevels();
        LoadLevel(currentLevelIndex);
        
        LifeCycle.GameStarted();
        
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
            vignetteImage.raycastTarget = false; // Ensure it doesn't block input
        }

        if (transitionUI == null)
        {
            CreateTransitionUI();
        }

        if (resultPanel != null)
        {
            resultPanel.Setup(OnResultPanelClicked);
        }
    }

    void OnResultPanelClicked()
    {
        if (resultPanel != null) resultPanel.Hide();
        
        if (isWinState)
        {
            if (currentLevelIndex < levels.Count - 1)
            {
                StartCoroutine(TransitionToNextLevel());
            }
            else
            {
                FinishGame();
            }
        }
        else
        {
            FinishGame();
        }
    }

    void FinishGame()
    {
        isGameActive = false;
        if (vignetteImage != null) vignetteImage.gameObject.SetActive(false);
        LifeCycle.GameEnded();
        Debug.Log("Game Finished");
        
        // Show CTA
        if (grid != null && grid.ctaPanel != null)
        {
            grid.ctaPanel.SetActive(true);
        }
    }

    void CreateTransitionUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject transitionObj = new GameObject("LevelTransition");
        transitionObj.transform.SetParent(canvas.transform, false);
        transitionObj.transform.SetAsLastSibling(); // Ensure it's on top
        
        Image img = transitionObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // Start transparent
        
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        transitionUI = transitionObj.AddComponent<TransitionUI>();
        transitionUI.fadeImage = img;
    }

    void InitializeLevels()
    {
        if (levels == null || levels.Count == 0)
        {
            levels = new List<LevelData>();

            List<string> commonWords = new List<string> { "BRAIN", "LOGIC", "GENIUS", "SCIENCE", "ENERGY", "SMART" };
            int commonWidth = 6;
            int commonHeight = 8;

            // Level 1: Easy
            levels.Add(new LevelData {
                levelName = "Level 1",
                width = commonWidth,
                height = commonHeight,
                words = new List<string>(commonWords),
                timeLimit = 60f
            });

            // Level 2: Challenging (15 seconds)
            levels.Add(new LevelData {
                levelName = "Level 2",
                width = commonWidth,
                height = commonHeight,
                words = new List<string> { "BRAIN", "GENIUS", "SCIENCE", "SMART" },
                timeLimit = 15f
            });

            // Level 3: Very Hard
            levels.Add(new LevelData {
                levelName = "Level 3",
                width = commonWidth,
                height = commonHeight,
                words = new List<string>(commonWords),
                timeLimit = 30f // Adjusted for "Hard" but using same words
            });
        }
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        currentLevelIndex = index;
        LevelData data = levels[index];
        
        gameDuration = data.timeLimit;
        timeRemaining = gameDuration;
        isGameActive = true;

        if (grid != null)
        {
            grid.SetupLevel(data);
        }

        UpdateTimerUI();
        UpdateProgress(0);
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
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
        isWinState = true;
        if (resultPanel != null)
        {
            if (resultPanel.panelText != null) resultPanel.panelText.text = "YOU WIN!";
            resultPanel.Show();
        }
        else
        {
            // Fallback
            if (currentLevelIndex < levels.Count - 1)
            {
                StartCoroutine(TransitionToNextLevel());
            }
            else
            {
                FinishGame();
            }
        }
    }

    IEnumerator TransitionToNextLevel()
    {
        isGameActive = false;
        
        if (transitionUI != null)
        {
            yield return transitionUI.FadeIn();
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        LoadNextLevel();

        if (transitionUI != null)
        {
            yield return transitionUI.FadeOut();
        }
    }

    void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    void OnGameLose()
    {
        isGameActive = false;
        isWinState = false;
        if (vignetteImage != null) vignetteImage.gameObject.SetActive(false);
        
        if (resultPanel != null)
        {
            if (resultPanel.panelText != null) resultPanel.panelText.text = "TIME UP!";
            resultPanel.Show();
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        LifeCycle.GameEnded();
        Debug.Log("Game Over - Time Out!");
    }
}
