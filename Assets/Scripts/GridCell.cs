using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridCell : MonoBehaviour
{
    public TextMeshProUGUI letterText;
    
    public int row;
    public int col;
    public char letter;
    private bool isFound = false;

    public void SetLetter(char c, int r, int cc)
    {
        letter = c;
        row = r;
        col = cc;
        letterText.text = c.ToString().ToUpper();
        ResetCell();
    }

    public void ResetCell()
    {
        isFound = false;
    }

    public void SetSelected(bool selected)
    {
        
    }

    private void Awake()
    {
        Image img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); 
        }
        img.raycastTarget = true;
    }

    public void SetFound()
    {
        isFound = true;
        StartCoroutine(PopEffect());
    }

    private System.Collections.IEnumerator PopEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.15f;
        
        float duration = 0.1f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}
