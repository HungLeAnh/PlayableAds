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
    public Button hintButton;
    public TextMeshProUGUI hintCountText;
    public WordSearchInput inputManager;
    public int maxHints = 3;

    private char[,] grid;
    private GridCell[,] cellObjects;
    private List<string> foundWords = new List<string>(); // Stores the original word from wordsToFind
    private Dictionary<string, Placement> wordPlacements = new Dictionary<string, Placement>();
    private int hintsUsed = 0;

    public GridCell GetCellAt(int row, int col)
    {
        if (cellObjects == null) return null;
        if (col < 0 || col >= gridWidth || row < 0 || row >= gridHeight) return null;
        return cellObjects[col, row];
    }

    void Awake()
    {
        if (inputManager == null)
            inputManager = FindObjectOfType<WordSearchInput>();
    }

    void Start()
    {
        if (hintButton != null)
            hintButton.onClick.AddListener(HighlightRandomWord);
        
        GenerateGrid();
        UpdateWordListUI();
        UpdateHintUI();
    }

    void UpdateHintUI()
    {
        if (hintCountText != null)
        {
            hintCountText.text = $"{maxHints - hintsUsed}";
        }
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
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;
        if (hintsUsed >= maxHints) return;

        List<string> remainingWords = new List<string>();
        foreach (var word in wordsToFind)
        {
            if (!IsWordFound(word))
            {
                remainingWords.Add(word);
            }
        }

        if (remainingWords.Count > 0)
        {
            hintsUsed++;
            UpdateHintUI();
            if (hintsUsed >= maxHints && hintButton != null)
            {
                hintButton.interactable = false;
            }

            string originalWord = remainingWords[Random.Range(0, remainingWords.Count)];
            string key = originalWord.ToUpper().Replace(" ", "");

            if (wordPlacements.ContainsKey(key))
            {
                Placement p = wordPlacements[key];
                GridCell firstCell = null;
                GridCell lastCell = null;

                for (int i = 0; i < key.Length; i++)
                {
                    GridCell cell = GetCellAt(p.y + i * p.dy, p.x + i * p.dx);
                    if (cell != null)
                    {
                        cell.SetFound();
                        if (i == 0) firstCell = cell;
                        if (i == key.Length - 1) lastCell = cell;
                    }
                }
                
                // Draw selection line
                if (firstCell != null && lastCell != null && inputManager != null)
                {
                    inputManager.CreatePermanentLine(firstCell, lastCell);
                }

                // Also mark as found in the logic
                OnWordFound(originalWord);
                Debug.Log($"[WordSearch] Hint found word: {originalWord}");
            }
            else
            {
                Debug.LogWarning($"[WordSearch] Word placement not found for: {key}");
            }
        }
    }

    private bool IsWordFound(string word)
    {
        foreach (string found in foundWords)
        {
            if (string.Equals(found.ToUpper().Replace(" ", ""), word.ToUpper().Replace(" ", "")))
                return true;
        }
        return false;
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
        // Find the matching word in wordsToFind (case-insensitive)
        string matchingWord = null;
        foreach (string w in wordsToFind)
        {
            if (string.Equals(w.ToUpper().Replace(" ", ""), word.ToUpper().Replace(" ", "")))
            {
                matchingWord = w;
                break;
            }
        }

        if (matchingWord != null && !foundWords.Contains(matchingWord))
        {
            foundWords.Add(matchingWord);
            UpdateWordListUI();
            
            // Update progress: ratio of found words to total words
            if (GameManager.Instance != null)
            {
                float progress = (float)foundWords.Count / wordsToFind.Count;
                GameManager.Instance.UpdateProgress(progress);
            }
            
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
