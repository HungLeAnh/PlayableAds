using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridCell : MonoBehaviour
{
    public TextMeshProUGUI letterText;
    public Image background;
    
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
        background.color = Color.white;
    }

    public void SetSelected(bool selected)
    {
        if (isFound) return;
        background.color = selected ? Color.yellow : Color.white;
    }

    public void SetFound(Color color)
    {
        isFound = true;
        background.color = color;
    }
}
