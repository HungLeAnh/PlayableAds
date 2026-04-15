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
        image.color = color;
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;
        
        rectTransform.sizeDelta = new Vector2(distance + width, width);
        rectTransform.pivot = new Vector2(width / 2 / (distance + width), 0.5f);
        rectTransform.position = startPos;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
