using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }
        else if (Input.GetMouseButton(0) && isSelecting)
        {
            ContinueSelection();
        }
        else if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            EndSelection();
        }
    }

    void StartSelection()
    {
        GridCell cell = GetCellUnderMouse();
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

    void ContinueSelection()
    {
        GridCell cell = GetCellUnderMouse();
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
        
        StringBuilder sb = new StringBuilder();
        foreach (var cell in selectedCells) sb.Append(cell.letter);

        string word = sb.ToString().ToUpper();
        string reversedWord = ReverseString(word);

        bool found = false;
        if (gridManager.wordsToFind.Contains(word) || gridManager.wordsToFind.Contains(reversedWord))
        {
            string finalWord = gridManager.wordsToFind.Contains(word) ? word : reversedWord;
            Color randomColor = GetRandomColor();
            randomColor.a = 0.6f; // Semi-transparent for the line

            foreach (var cell in selectedCells) cell.SetFound();
            
            // Finalize the line
            if (currentLine != null)
            {
                currentLine.SetLine(startCell.transform.position, selectedCells[selectedCells.Count-1].transform.position, lineWidth, randomColor);
            }
            
            gridManager.OnWordFound(finalWord);
            found = true;
        }

        if (!found)
        {
            if (currentLine != null) Destroy(currentLine.gameObject);
            ClearSelectionVisuals();
        }
        
        currentLine = null;
        selectedCells.Clear();
    }

    Color GetRandomColor()
    {
        // Generate a vibrant random color
        return Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
    }

    string ReverseString(string s)
    {
        char[] charArray = s.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    GridCell GetCellUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
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
    }
}
