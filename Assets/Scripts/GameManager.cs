using UnityEngine;
using UnityEngine.UI;
using Luna.Unity;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [SerializeField] private WordSearchGrid grid;
    [SerializeField] private int currentLevelIndex = 0;
    [SerializeField] private bool useTimer = true;
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image vignetteImage;
    [SerializeField] private PulsatingVignette vignetteEffect;
    [SerializeField] private float vignettePulseSpeed = 4f;
    [SerializeField] private float idleTimeThreshold = 3f;
    [SerializeField] private TransitionUI transitionUI;
    [SerializeField] private GamePanelUI resultPanel;
    [SerializeField] private TutorialHandUI tutorialHand;
    [SerializeField] private TutorialTextUI tutorialTextUI;

    private bool isGameActive = true;
    private float timeRemaining;
    private List<LevelData> levels;
    private bool isWinState = false;
    private float lastInteractionTime;
    private bool tutorialActive = false;
    private bool firstInteractionDone = false;

    public bool IsGameActive { get => isGameActive; set => isGameActive = value; }
    public TutorialTextUI TutorialTextUI { get => tutorialTextUI; set => tutorialTextUI = value; }
    public TutorialHandUI TutorialHand { get => tutorialHand; set => tutorialHand = value; }
    public List<LevelData> Levels { get => levels; set => levels = value; }
    public int CurrentLevelIndex { get => currentLevelIndex; set => currentLevelIndex = value; }

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

        if (resultPanel != null)
        {
            resultPanel.Setup(OnResultPanelClicked);
        }

        ResetIdleTimer(false);
        ShowTutorial(); // Initial tutorial
    }

    public void ResetIdleTimer(bool isActualInteraction = true)
    {
        lastInteractionTime = Time.time;
        if (isActualInteraction)
        {
            firstInteractionDone = true;
        }
        
        if (tutorialActive)
        {
            HideTutorial();
        }
    }

    void ShowTutorial()
    {
        if (grid == null || (tutorialHand == null && tutorialTextUI == null) || tutorialActive) return;

        bool handShown = false;
        if (tutorialHand != null)
        {
            WordSearchGrid.PlacementInfo placement = grid.GetUnfoundWordPlacement();
            if (placement.start != null && placement.end != null)
            {
                tutorialHand.Show(placement.start, placement.end);
                handShown = true;
            }
        }

        if (tutorialTextUI != null)
        {
            tutorialTextUI.Show();
        }

        tutorialActive = true;
    }

    void HideTutorial()
    {
        if (tutorialHand != null)
        {
            tutorialHand.Hide();
        }
        if (tutorialTextUI != null)
        {
            tutorialTextUI.Hide();
        }
        tutorialActive = false;
    }

    void OnResultPanelClicked()
    {
        if (resultPanel != null && !resultPanel.gameObject.activeSelf) return;

        ResetIdleTimer();
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
        
        // Show CTA
        if (grid != null && grid.ctaPanel != null)
        {
            grid.ctaPanel.SetActive(true);
        }
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
                timeLimit = 30f 
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

        ResetIdleTimer(false);
        HideTutorial();
    }

    void Update()
    {
        if (!isGameActive) return;

        if (useTimer)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
            UpdateVignette();

            if (timeRemaining <= 0)
            {
                OnGameLose();
            }
        }

        if (!tutorialActive && Time.time - lastInteractionTime > idleTimeThreshold)
        {
            ShowTutorial();
        }
    }

    void UpdateVignette()
    {
        bool shouldPulse = (timeRemaining <= 6f && timeRemaining > 0);

        if (vignetteEffect != null)
        {
            vignetteEffect.SetPulsing(shouldPulse);
        }
        else if (vignetteImage != null)
        {
            if (shouldPulse)
            {
                if (!vignetteImage.gameObject.activeSelf) vignetteImage.gameObject.SetActive(true);
                float alpha = ((Mathf.Sin(Time.time * vignettePulseSpeed) + 1f) / 2f) * 0.6f;
                vignetteImage.color = new Color(1, 0, 0, alpha);
            }
            else
            {
                if (vignetteImage.gameObject.activeSelf) vignetteImage.gameObject.SetActive(false);
            }
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
        HideTutorial();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayWin();
        if (resultPanel != null)
        {
            if (resultPanel.panelText != null) resultPanel.panelText.text = "YOU WIN";
            resultPanel.Show();
        }
        
        StartCoroutine(DelayedWinTransition());
    }

    IEnumerator DelayedWinTransition()
    {
        yield return new WaitForSeconds(2f);
        
        if (resultPanel != null && resultPanel.gameObject.activeSelf)
        {
            OnResultPanelClicked();
        }
        else if (resultPanel == null)
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
        HideTutorial();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayLose();
        if (vignetteImage != null) vignetteImage.gameObject.SetActive(false);

        if (resultPanel != null)
        {
            if (resultPanel.panelText != null) resultPanel.panelText.text = "YOU LOSE";
            resultPanel.Show();
        }

        StartCoroutine(DelayedShowGameOver());

        LifeCycle.GameEnded();
    }

    IEnumerator DelayedShowGameOver()
    {
        yield return new WaitForSeconds(2f);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

}
