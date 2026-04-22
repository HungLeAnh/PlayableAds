using UnityEngine;
using Luna.Unity;
using System.Collections;

public class CTAButton : MonoBehaviour
{
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.1f;
    
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(Pulse());
    }

    void OnDisable()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        transform.localScale = originalScale;
    }

    IEnumerator Pulse()
    {
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    public void OpenStore()
    {
        Playable.InstallFullGame("https://flowcv.com/resume/us5li710eubl");
    }
}
