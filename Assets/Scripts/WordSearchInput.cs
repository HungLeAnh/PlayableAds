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
        if (cell != null && cell != (selectedCells.Count > 0 ? selectedCells[selectedCells.Count - 1] : null))
        {
            if (IsValidSelection(cell))
            {
                UpdateSelection(cell);
            }
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
                selectedCells.Add(cell);
                cell.SetSelected(true);
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
            GridCell cell = result.gameObject.GetComponentInParent<GridCell>();
            if (cell != null) return cell;
        }
        return null;
    }

    GridCell GetCellAt(int row, int col)
    {
        return gridManager.GetCellAt(row, col);
    }
}
