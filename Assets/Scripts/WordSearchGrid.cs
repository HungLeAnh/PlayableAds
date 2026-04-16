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
    public float gridPadding = 20f;

    private char[,] grid;
    private GridCell[,] cellObjects;
    private List<string> foundWords = new List<string>();
    private Dictionary<string, Placement> wordPlacements = new Dictionary<string, Placement>();

    public GridCell GetCellAt(int row, int col)
    {
        if (cellObjects == null) return null;
        if (col < 0 || col >= gridWidth || row < 0 || row >= gridHeight) return null;
        return cellObjects[col, row];
    }

    void Awake()
    {
    }

    void Start()
    {
        GenerateGrid();
        UpdateWordListUI();
    }

    void GenerateGrid()
    {
        // Force UI layout to update to get correct dimensions
        Canvas.ForceUpdateCanvases();
        
        int maxRetries = 100;
        int currentRetry = 0;
        bool allWordsPlaced = false;

        while (!allWordsPlaced && currentRetry < maxRetries)
        {
            currentRetry++;
            allWordsPlaced = TryGenerateGrid();
        }

        if (!allWordsPlaced)
        {
            Debug.LogError($"[WordSearch] Failed to generate a valid grid after {maxRetries} retries!");
        }
        else
        {
            Debug.Log($"[WordSearch] Grid generated successfully in {currentRetry} attempts.");
        }

        // 4. Instantiate grid cells
        GridLayoutGroup gridLayout = gridRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridWidth;

            // Force layout rebuild
            RectTransform rectTransform = gridRoot.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        cellObjects = new GridCell[gridWidth, gridHeight];
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

    bool TryGenerateGrid()
    {
        grid = new char[gridWidth, gridHeight];
        wordPlacements.Clear();

        // 1. Initialize empty grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = ' ';
            }
        }

        // 2. Prepare and sort words
        List<string> sortedWords = new List<string>(wordsToFind);
        sortedWords.Sort((a, b) => b.Length.CompareTo(a.Length));

        foreach (string word in sortedWords)
        {
            string upperWord = word.ToUpper().Replace(" ", "");
            if (upperWord.Length > Mathf.Max(gridWidth, gridHeight))
            {
                Debug.LogError($"Word {upperWord} is too long for the grid!");
                continue;
            }
            
            if (!PlaceWord(upperWord))
            {
                return false; // Failed to place a word, triggers a retry
            }
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

        return true;
    }

    private struct Placement
    {
        public int x, y, dx, dy;
    }

    bool PlaceWord(string word)
    {
        List<Placement> possiblePlacements = new List<Placement>();
        int maxOverlap = -1;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int overlap = GetOverlapScore(word, x, y, dx, dy);
                        if (overlap != -1)
                        {
                            if (overlap > maxOverlap)
                            {
                                maxOverlap = overlap;
                                possiblePlacements.Clear();
                                possiblePlacements.Add(new Placement { x = x, y = y, dx = dx, dy = dy });
                            }
                            else if (overlap == maxOverlap)
                            {
                                possiblePlacements.Add(new Placement { x = x, y = y, dx = dx, dy = dy });
                            }
                        }
                    }
                }
            }
        }

        if (possiblePlacements.Count > 0)
        {
            Placement p = possiblePlacements[Random.Range(0, possiblePlacements.Count)];
            for (int i = 0; i < word.Length; i++)
            {
                grid[p.x + i * p.dx, p.y + i * p.dy] = word[i];
            }
            wordPlacements[word] = p; // Store the placement
            Debug.Log($"[WordSearch] Placed '{word}' at (col: {p.x}, row: {p.y}) direction: ({p.dx}, {p.dy})");
            return true;
        }
        
        return false;
    }

    public void HighlightRandomWord()
    {
        List<string> remainingWords = new List<string>();
        foreach (var word in wordsToFind)
        {
            if (!foundWords.Contains(word.ToUpper().Replace(" ", "")))
            {
                remainingWords.Add(word.ToUpper().Replace(" ", ""));
            }
        }

        if (remainingWords.Count > 0)
        {
            string wordToHighlight = remainingWords[Random.Range(0, remainingWords.Count)];
            if (wordPlacements.ContainsKey(wordToHighlight))
            {
                Placement p = wordPlacements[wordToHighlight];
                for (int i = 0; i < wordToHighlight.Length; i++)
                {
                    GridCell cell = GetCellAt(p.y + i * p.dy, p.x + i * p.dx);
                    if (cell != null)
                    {
                        cell.SetFound(); // Using SetFound() as a highlight effect (triggers pop)
                    }
                }
                Debug.Log($"[WordSearch] Highlighted word: {wordToHighlight}");
            }
        }
    }

    int GetOverlapScore(string word, int x, int y, int dx, int dy)
    {
        int score = 0;
        for (int i = 0; i < word.Length; i++)
        {
            int nx = x + i * dx;
            int ny = y + i * dy;
            if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight) return -1;
            char existingChar = grid[nx, ny];
            if (existingChar != ' ' && existingChar != word[i]) return -1;
            if (existingChar == word[i]) score++;
        }
        return score;
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
        string text = "";
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
