using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WordSearchInput : MonoBehaviour
{
    public WordSearchGrid gridManager;
    public GameObject linePrefab;
    public Transform lineRoot;
    public float lineWidth = 50f;

    private List<GridCell> selectedCells = new List<GridCell>();
    private bool isSelecting = false;
    private GridCell startCell;
    private SelectionLine currentLine;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            if (isSelecting) EndSelection();
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                StartSelection(touchPos);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (isSelecting) ContinueSelection(touchPos);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (isSelecting) EndSelection();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartSelection(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && isSelecting)
            {
                ContinueSelection(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                EndSelection();
            }
        }
    }

    void StartSelection(Vector2 position)
    {
        if (GameManager.Instance != null) GameManager.Instance.ResetIdleTimer();
        GridCell cell = GetCellAtPosition(position);
        if (cell != null)
        {
            isSelecting = true;
            startCell = cell;
            selectedCells.Clear();
            
            GameObject lineObj = Instantiate(linePrefab, lineRoot);
            currentLine = lineObj.GetComponent<SelectionLine>();
            
            AddCell(cell);
        }
    }

    void ContinueSelection(Vector2 position)
    {
        GridCell cell = GetCellAtPosition(position);
        if (cell != null)
        {
            if (IsValidSelection(cell))
            {
                UpdateSelection(cell);
                UpdateLineVisual(cell);
            }
        }
    }

    void UpdateLineVisual(GridCell endCell)
    {
        if (currentLine != null)
        {
            currentLine.SetLine(startCell.transform.position, endCell.transform.position, lineWidth, new Color(1, 1, 0, 0.5f));
        }
    }

    void EndSelection()
    {
        isSelecting = false;
        
        if (selectedCells.Count == 0)
        {
            if (currentLine != null) Destroy(currentLine.gameObject);
            currentLine = null;
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var cell in selectedCells) sb.Append(cell.letter);

        string selectedWord = sb.ToString();
        string reversedWord = ReverseString(selectedWord);

        bool found = false;
        string matchingWord = FindMatchingWord(selectedWord);
        if (matchingWord == null) matchingWord = FindMatchingWord(reversedWord);

        if (matchingWord != null)
        {
            Color randomColor = GetRandomColor();
            randomColor.a = 0.6f; // Semi-transparent for the line

            foreach (var cell in selectedCells) cell.SetFound();
            
            // Finalize the line
            if (currentLine != null)
            {
                currentLine.SetLine(startCell.transform.position, selectedCells[selectedCells.Count-1].transform.position, lineWidth, randomColor);
            }

            gridManager.OnWordFound(matchingWord);
            found = true;
        }

        if (!found)
        {
            if (currentLine != null) Destroy(currentLine.gameObject);
            ClearSelectionVisuals();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayWrongLetter();
        }
        
        currentLine = null;
        selectedCells.Clear();
    }

    private string FindMatchingWord(string word)
    {
        if (gridManager == null || gridManager.wordsToFind == null) return null;
        
        string normalizedTarget = word.ToUpper().Replace(" ", "");
        foreach (string w in gridManager.wordsToFind)
        {
            if (string.Equals(w.ToUpper().Replace(" ", ""), normalizedTarget))
                return w;
        }
        return null;
    }

    public void CreatePermanentLine(GridCell start, GridCell end)
    {
        if (linePrefab == null || lineRoot == null) return;
        
        GameObject lineObj = Instantiate(linePrefab, lineRoot);
        SelectionLine line = lineObj.GetComponent<SelectionLine>();
        Color randomColor = GetRandomColor();
        randomColor.a = 0.6f;
        
        line.SetLine(start.transform.position, end.transform.position, lineWidth, randomColor);
    }

    Color GetRandomColor()
    {
        // Generate a vibrant random color
        float h = UnityEngine.Random.Range(0f, 1f);   // Hue (0-1)
        float s = UnityEngine.Random.Range(0.5f, 1f); // Saturation (0.5-1)
        float v = UnityEngine.Random.Range(0.7f, 1f); // Value/Brightness (0.7-1)

        return Color.HSVToRGB(h, s, v);
    }

    string ReverseString(string s)
    {
        char[] charArray = s.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    GridCell GetCellAtPosition(Vector2 position)
    {
        if (EventSystem.current == null) return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GridCell cell = result.gameObject.GetComponentInParent<GridCell>();
            if (cell != null) return cell;
        }
        return null;
    }

    GridCell GetCellAt(int row, int col)
    {
        return gridManager.GetCellAt(row, col);
    }

    void AddCell(GridCell cell)
    {
        if (!selectedCells.Contains(cell))
        {
            selectedCells.Add(cell);
            cell.SetSelected(true);
        }
    }

    void ClearSelectionVisuals()
    {
        foreach (var cell in selectedCells)
        {
            cell.SetSelected(false);
        }
    }

    bool IsValidSelection(GridCell currentCell)
    {
        if (startCell == null || currentCell == null) return false;
        int dx = Mathf.Abs(currentCell.col - startCell.col);
        int dy = Mathf.Abs(currentCell.row - startCell.row);

        return dx == 0 || dy == 0 || dx == dy;
    }

    void UpdateSelection(GridCell currentCell)
    {
        if (gridManager == null) return;

        int oldCount = selectedCells.Count;
        ClearSelectionVisuals();
        selectedCells.Clear();

        int colDist = currentCell.col - startCell.col;
        int rowDist = currentCell.row - startCell.row;
        
        int steps = Mathf.Max(Mathf.Abs(colDist), Mathf.Abs(rowDist));
        int dx = colDist == 0 ? 0 : colDist / Mathf.Abs(colDist);
        int dy = rowDist == 0 ? 0 : rowDist / Mathf.Abs(rowDist);

        for (int i = 0; i <= steps; i++)
        {
            int c = startCell.col + i * dx;
            int r = startCell.row + i * dy;
            
            GridCell cell = gridManager.GetCellAt(r, c);
            if (cell != null)
            {
                AddCell(cell);
            }
        }

        if (selectedCells.Count > oldCount && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLetterSelected();
        }
    }
}
