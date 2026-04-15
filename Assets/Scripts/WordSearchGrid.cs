using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WordSearchGrid : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public List<string> wordsToFind = new List<string> { "LUNA", "PLAYABLE", "UNITY", "ADS", "SEARCH", "WORD" };
    public GameObject cellPrefab;
    public Transform gridRoot;
    public TextMeshProUGUI wordListText;
    public GameObject ctaPanel; // Call To Action panel

    private char[,] grid;
    private GridCell[,] cellObjects;
    private List<string> foundWords = new List<string>();

    public GridCell GetCellAt(int row, int col)
    {
        if (cellObjects == null) return null;
        if (col < 0 || col >= gridWidth || row < 0 || row >= gridHeight) return null;
        return cellObjects[col, row];
    }

    void Awake()
    {
        GenerateGrid();
        UpdateWordListUI();
    }

    void GenerateGrid()
    {
        grid = new char[gridWidth, gridHeight];
        cellObjects = new GridCell[gridWidth, gridHeight];

        // 1. Initialize empty grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = ' ';
            }
        }

        // 2. Place words
        foreach (string word in wordsToFind)
        {
            PlaceWord(word.ToUpper());
        }

        // 3. Fill empty spots with random letters
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == ' ')
                {
                    grid[x, y] = (char)('A' + Random.Range(0, 26));
                }
            }
        }

        // 4. Instantiate grid cells
        GridLayoutGroup gridLayout = gridRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraintCount = gridWidth;
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridRoot);
                GridCell cell = cellObj.GetComponent<GridCell>();
                cell.SetLetter(grid[x, y], y, x);
                cellObjects[x, y] = cell;
            }
        }
    }

    void PlaceWord(string word)
    {
        int attempts = 0;
        bool placed = false;

        while (!placed && attempts < 100)
        {
            attempts++;
            int startX = Random.Range(0, gridWidth);
            int startY = Random.Range(0, gridHeight);
            int dirX = Random.Range(-1, 2);
            int dirY = Random.Range(-1, 2);

            if (dirX == 0 && dirY == 0) continue;

            if (CanPlaceWord(word, startX, startY, dirX, dirY))
            {
                for (int i = 0; i < word.Length; i++)
                {
                    grid[startX + i * dirX, startY + i * dirY] = word[i];
                }
                placed = true;
            }
        }
    }

    bool CanPlaceWord(string word, int x, int y, int dx, int dy)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int nx = x + i * dx;
            int ny = y + i * dy;

            if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight) return false;
            if (grid[nx, ny] != ' ' && grid[nx, ny] != word[i]) return false;
        }
        return true;
    }

    public void OnWordFound(string word)
    {
        if (wordsToFind.Contains(word) && !foundWords.Contains(word))
        {
            foundWords.Add(word);
            UpdateWordListUI();
            CheckWinCondition();
        }
    }

    void UpdateWordListUI()
    {
        string text = "Words to find:\n";
        foreach (string word in wordsToFind)
        {
            if (foundWords.Contains(word))
                text += "<s>" + word + "</s>\t";
            else
                text += word + "\t";
        }
        wordListText.text = text;
    }

    void CheckWinCondition()
    {
        if (foundWords.Count == wordsToFind.Count)
        {
            ctaPanel.SetActive(true);
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameComplete();
        }
    }
}
