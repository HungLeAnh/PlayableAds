using UnityEngine;
using UnityEngine.UI;

public class PulsatingVignette : MonoBehaviour
{
    public Image vignetteImage;
    public float pulseSpeed = 4f;
    public float maxAlpha = 0.6f;
    public Color vignetteColor = Color.red;

    private bool isPulsing = false;

    void Awake()
    {
        if (vignetteImage == null)
            vignetteImage = GetComponent<Image>();
        
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
            vignetteImage.raycastTarget = false;
        }
    }

    public void SetPulsing(bool state)
    {
        isPulsing = state;
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(state);
            if (!state)
            {
                Color c = vignetteImage.color;
                c.a = 0;
                vignetteImage.color = c;
            }
        }
    }

    void Update()
    {
        if (!isPulsing || vignetteImage == null) return;

        float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        alpha *= maxAlpha;
        
        Color c = vignetteColor;
        c.a = alpha;
        vignetteImage.color = c;
    }
}
