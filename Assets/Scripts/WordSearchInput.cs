using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

public class WordSearchInput : MonoBehaviour
{
    public WordSearchGrid gridManager;
    private List<GridCell> selectedCells = new List<GridCell>();
    private bool isSelecting = false;
    private GridCell startCell;

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
            AddCell(cell);
        }
    }

    void ContinueSelection()
    {
        GridCell cell = GetCellUnderMouse();
        if (cell != null && cell != startCell && !selectedCells.Contains(cell))
        {
            // Logic to restrict selection to a straight line
            if (IsValidSelection(cell))
            {
                UpdateSelection(cell);
            }
        }
    }

    bool IsValidSelection(GridCell currentCell)
    {
        int dx = currentCell.col - startCell.col;
        int dy = currentCell.row - startCell.row;

        // Check if horizontal, vertical, or diagonal (45 degrees)
        return dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy);
    }

    void UpdateSelection(GridCell currentCell)
    {
        ClearSelectionVisuals();
        selectedCells.Clear();

        int dx = Mathf.Clamp(currentCell.col - startCell.col, -1, 1);
        int dy = Mathf.Clamp(currentCell.row - startCell.row, -1, 1);
        
        int steps = Mathf.Max(Mathf.Abs(currentCell.col - startCell.col), Mathf.Abs(currentCell.row - startCell.row));

        for (int i = 0; i <= steps; i++)
        {
            int x = startCell.col + i * dx;
            int y = startCell.row + i * dy;
            
            // This is a bit tricky since we need to find the cell by coordinates.
            // I'll update WordSearchGrid to provide a way to get cell by row/col.
            GridCell cell = GetCellAt(y, x);
            if (cell != null)
            {
                AddCell(cell);
            }
        }
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

    void EndSelection()
    {
        isSelecting = false;
        
        StringBuilder sb = new StringBuilder();
        foreach (var cell in selectedCells)
        {
            sb.Append(cell.letter);
        }

        string word = sb.ToString().ToUpper();
        string reversedWord = ReverseString(word);

        if (gridManager.wordsToFind.Contains(word))
        {
            foreach (var cell in selectedCells) cell.SetFound();
            gridManager.OnWordFound(word);
        }
        else if (gridManager.wordsToFind.Contains(reversedWord))
        {
             foreach (var cell in selectedCells) cell.SetFound();
             gridManager.OnWordFound(reversedWord);
        }
        else
        {
            ClearSelectionVisuals();
        }
        
        selectedCells.Clear();
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
            GridCell cell = result.gameObject.GetComponent<GridCell>();
            if (cell != null) return cell;
        }
        return null;
    }

    GridCell GetCellAt(int row, int col)
    {
        return gridManager.GetCellAt(row, col);
    }
}
