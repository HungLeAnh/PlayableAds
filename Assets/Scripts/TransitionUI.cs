using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TransitionUI : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
        
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    public IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        fadeImage.raycastTarget = true;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(1 - (elapsed / fadeDuration));
            fadeImage.color = c;
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }
}
