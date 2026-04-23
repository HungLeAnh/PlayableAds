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

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        if (tutorialText == null) tutorialText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Show(string text = "FIND ALL THE WORDS")
    {
        if (tutorialText != null) tutorialText.text = text;
        gameObject.SetActive(true);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    public void Hide()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
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
