using UnityEngine;
using UnityEngine.UI;

public class SelectionLine : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void SetLine(Vector2 startPos, Vector2 endPos, float width, Color color)
    {
        if (image == null || rectTransform == null || rectTransform.parent == null) return;

        image.color = color;
        
        // Convert world positions to local positions relative to the lineRoot parent
        Vector2 localStartPos = rectTransform.parent.InverseTransformPoint(startPos);
        Vector2 localEndPos = rectTransform.parent.InverseTransformPoint(endPos);
        
        Vector2 dir = localEndPos - localStartPos;
        float distance = dir.magnitude;
        
        // Set size and pivot
        rectTransform.sizeDelta = new Vector2(distance + width, width);
        rectTransform.pivot = new Vector2(width / 2f / (distance + width), 0.5f);
        
        // Set position and rotation
        rectTransform.anchoredPosition = localStartPos;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Ensure it's visible in front of other elements if needed, but keep z stable
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
    }
}
