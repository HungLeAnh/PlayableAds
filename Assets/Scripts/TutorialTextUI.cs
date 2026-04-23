using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialTextUI : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    private CanvasGroup canvasGroup;
    private Coroutine pulseCoroutine;

    private Coroutine feedbackCoroutine;
    private string originalText = "FIND ALL THE WORDS";

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        if (tutorialText == null) tutorialText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Show(string text = "FIND ALL THE WORDS")
    {
        originalText = text;
        if (feedbackCoroutine != null) return; // Don't override feedback

        if (tutorialText != null) tutorialText.text = text;
        tutorialText.color = Color.white;
        gameObject.SetActive(true);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    public void ShowFeedback(string text, Color color, float duration = 1.5f)
    {
        gameObject.SetActive(true); // Must be active to start coroutines
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(text, color, duration));
    }

    IEnumerator FeedbackRoutine(string text, Color color, float duration)
    {
        if (tutorialText != null)
        {
            tutorialText.text = text;
            tutorialText.color = color;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

        // Hide after duration
        Hide();

        feedbackCoroutine = null;
    }

    public void Hide()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        gameObject.SetActive(false);
    }

    IEnumerator PulseAnimation()
    {
        while (true)
        {
            float alpha = minAlpha + (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f * (maxAlpha - minAlpha);
            if (canvasGroup != null) canvasGroup.alpha = alpha;
            yield return null;
        }
    }
}
