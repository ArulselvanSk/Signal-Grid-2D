using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignalGridManager : MonoBehaviour
{
    #region Singleton
    public static SignalGridManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Inspector Fields
    [Space(20)]
    [Header("Grid Setup")]
    [Space(10)]
    [SerializeField] private GameObject Tile_Object;
    [Space(10)]
    [SerializeField] private Transform gridParent;
    [Space(10)]
    [SerializeField] private int Dimension = 5;
    [Space(20)]
    [Header("UI Elements")]
    [Space(10)]
    [SerializeField] private TMP_Text moveCounterText;
    [Space(10)]
    [SerializeField] private TMP_Text timerText;
    [Space(10)]
    [SerializeField] private TMP_Text feedbackText;
    [Space(10)]
    [SerializeField] private TMP_Text TileChangedCountText;
    [Space(10)]
    [SerializeField] private GameObject winPanel;
    [Space(10)]
    [SerializeField] private TMP_Text peekPatternText;
    [Space(10)]
    [SerializeField] private GameObject peekOverlay;
    [Space(10)]
    [SerializeField] private TMP_Text winStatsText;
    [Space(10)]
    [SerializeField] private Button peekButton;
    [Space(20)]
    [Header("Game Settings")]
    [Space(10)]
    [SerializeField] private bool enableTimer = true;
    [Space(10)]
    [SerializeField] private int peekUsesAllowed = 1;
    [Space(10)]
    [SerializeField] private AudioSource audioSource;
    #endregion

    #region Private Variables
    private Tile[,] tileGrid;
    private List<Tile> allTiles = new List<Tile>();
    private int moveCount = 0;
    private float gameTime = 0f;
    private bool isGameActive = false;
    private int peekUsesRemaining;
    private Tile lastPeekedTile = null;
    private bool isPeekModeActive = false;
    #endregion

    void Start()
    {
        //GameInitialization();
    }

    void Update()
    {
        if (isGameActive && enableTimer)
        {
            gameTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    #region Initialization
    public void GameInitialization()
    {
        moveCount = 0;
        gameTime = 0f;
        isGameActive = true;
        if (peekOverlay != null) peekOverlay.SetActive(false);
        peekUsesRemaining = peekUsesAllowed;
        allTiles.Clear();

        tileGrid = new Tile[Dimension, Dimension];

        for (int y = 0; y < Dimension; y++)
        {
            for (int x = 0; x < Dimension; x++)
            {
                GameObject tileObj = Instantiate(Tile_Object, gridParent != null ? gridParent : transform);
                UpdateTilePosition(x, y, tileObj);

                Tile tile = tileObj.GetComponent<Tile>();
                if (tile == null)
                {
                    tile = tileObj.AddComponent<Tile>();
                }

                int index = y * Dimension + x;
                tile.Initialize(x, y, index);

                tileGrid[x, y] = tile;
                allTiles.Add(tile);
            }
        }

        GenerateTogglePatterns();
        RandomizeInitialState();
        UpdateMoveCounter();
        UpdateTimerDisplay();
        UpdateFeedbackText(string.Empty);
        TileChangedCountText.text = string.Empty;

        if (winPanel != null) winPanel.SetActive(false);
        if (peekButton != null)
        {
            peekButton.onClick.AddListener(EnablePeekMode);
            UpdatePeekButton();
        }
    }
    #endregion

    #region Pattern Creation Handler
    void GenerateTogglePatterns()
    {
        // Define different pattern types
        System.Random rand = new System.Random();

        for (int i = 0; i < allTiles.Count; i++)
        {
            Tile tile = allTiles[i];
            int x = tile.GridX;
            int y = tile.GridY;

            // Randomly select a pattern type for this tile
            int patternType = rand.Next(0, 23);

            List<int> pattern = new List<int>();
            pattern.Add(i); // Always toggle itself

            switch (patternType)
            {
                case 0: // Plus pattern (immediate neighbors only)
                    AddIfValid(pattern, x - 1, y); // Left
                    AddIfValid(pattern, x + 1, y); // Right
                    AddIfValid(pattern, x, y - 1); // Up
                    AddIfValid(pattern, x, y + 1); // Down
                    break;

                case 1: // X pattern (immediate diagonals only)
                    AddIfValid(pattern, x - 1, y - 1);
                    AddIfValid(pattern, x + 1, y - 1);
                    AddIfValid(pattern, x - 1, y + 1);
                    AddIfValid(pattern, x + 1, y + 1);
                    break;

                case 2: // Horizontal line (2 tiles)
                    AddIfValid(pattern, x + 1, y);
                    break;

                case 3: // Horizontal line (3 tiles)
                    AddIfValid(pattern, x - 1, y);
                    AddIfValid(pattern, x + 1, y);
                    break;

                case 4: // Vertical line (2 tiles)
                    AddIfValid(pattern, x, y + 1);
                    break;

                case 5: // Vertical line (3 tiles)
                    AddIfValid(pattern, x, y - 1);
                    AddIfValid(pattern, x, y + 1);
                    break;

                case 6: // L-shape (right and down)
                    AddIfValid(pattern, x + 1, y);
                    AddIfValid(pattern, x, y + 1);
                    break;

                case 7: // L-shape (left and down)
                    AddIfValid(pattern, x - 1, y);
                    AddIfValid(pattern, x, y + 1);
                    break;

                case 8: // Knight shape 1 (L to top-right)
                    AddIfValid(pattern, x + 1, y);
                    AddIfValid(pattern, x + 1, y - 1);
                    AddIfValid(pattern, x + 1, y - 2);
                    break;

                case 9: // Knight shape 2 (L to bottom-left)
                    AddIfValid(pattern, x - 1, y);
                    AddIfValid(pattern, x - 1, y + 1);
                    AddIfValid(pattern, x - 1, y + 2);
                    break;

                case 10: // T-shape (up, left, right)
                    AddIfValid(pattern, x - 1, y);
                    AddIfValid(pattern, x + 1, y);
                    AddIfValid(pattern, x, y - 1);
                    break;

                case 11: // 2x2 Square - Top-Left corner
                    AddIfValid(pattern, x + 1, y);     // Right
                    AddIfValid(pattern, x, y + 1);     // Down
                    AddIfValid(pattern, x + 1, y + 1); // Diagonal
                    break;

                case 12: // 2x2 Square - Top-Right corner
                    AddIfValid(pattern, x - 1, y);     // Left
                    AddIfValid(pattern, x, y + 1);     // Down
                    AddIfValid(pattern, x - 1, y + 1); // Diagonal
                    break;

                case 13: // 2x2 Square - Bottom-Left corner
                    AddIfValid(pattern, x + 1, y);     // Right
                    AddIfValid(pattern, x, y - 1);     // Up
                    AddIfValid(pattern, x + 1, y - 1); // Diagonal
                    break;

                case 14: // 2x2 Square - Bottom-Right corner
                    AddIfValid(pattern, x - 1, y);     // Left
                    AddIfValid(pattern, x, y - 1);     // Up
                    AddIfValid(pattern, x - 1, y - 1); // Diagonal
                    break;

                case 15: // Full Row
                    for (int cx = 0; cx < Dimension; cx++)
                    {
                        if (cx != x)
                            AddIfValid(pattern, cx, y);
                    }
                    break;

                case 16: // Full Column
                    for (int cy = 0; cy < Dimension; cy++)
                    {
                        if (cy != y)
                            AddIfValid(pattern, x, cy);
                    }
                    break;

                case 17: // Mirror Horizontal (same row, opposite side)
                    int mirrorX = Dimension - 1 - x;
                    AddIfValid(pattern, mirrorX, y);
                    break;

                case 18: // Mirror Vertical (same column, opposite side)
                    int mirrorY = Dimension - 1 - y;
                    AddIfValid(pattern, x, mirrorY);
                    break;

                case 19: // Mirror Diagonal (main diagonal)
                    AddIfValid(pattern, y, x);
                    break;

                case 20: // Horizontal line (4 tiles) - left
                    AddIfValid(pattern, x - 1, y);
                    AddIfValid(pattern, x - 2, y);
                    AddIfValid(pattern, x - 3, y);
                    break;

                case 21: // Horizontal line (4 tiles) - right
                    AddIfValid(pattern, x + 1, y);
                    AddIfValid(pattern, x + 2, y);
                    AddIfValid(pattern, x + 3, y);
                    break;

                case 22: // Vertical line (4 tiles) - down
                    AddIfValid(pattern, x, y + 1);
                    AddIfValid(pattern, x, y + 2);
                    AddIfValid(pattern, x, y + 3);
                    break;

                case 23: // Only itself (decoy)
                    // Already added itself above
                    break;
            }

            tile.togglePattern = pattern;
        }
    }
    #endregion

    #region Tile Click Handler
    public void OnTileClicked(Tile clickedTile)
    {
        if (!isGameActive) return;

        // Check if we're in peek mode
        if (lastPeekedTile != null)
        {
            HidePeekInfo();
            return;
        }

        clickedTile.PlayClickAnimation();
        ToggleGroup(clickedTile.togglePattern);

        moveCount++;
        UpdateMoveCounter();

        // Show feedback
        int tilesChanged = clickedTile.togglePattern.Count;
        TileChangedCountText.text = ($"{tilesChanged} tile(s) changed!");

        // Check win condition
        if (AreAllTilesOff())
        {
            OnGameWon();
        }
    }
    #endregion

    #region Private Helper Functions

    string GeneratePatternText(Tile clickedTile)
    {
        string result = string.Empty;

        for (int y = 0; y < Dimension; y++)
        {
            for (int x = 0; x < Dimension; x++)
            {
                int index = y * Dimension + x;

                if (clickedTile.togglePattern.Contains(index))
                {
                    result += "O  ";
                }
                else
                {
                    result += "X  ";
                }
            }
            result += "\n";
        }

        return result;
    }
    void RandomizeInitialState()
    {
        // Apply random clicks to create a solvable puzzle
        System.Random rand = new System.Random();
        int randomClicks = rand.Next(Dimension * 2, Dimension * 4);

        for (int i = 0; i < randomClicks; i++)
        {
            int randomIndex = rand.Next(0, allTiles.Count);
            ToggleGroup(allTiles[randomIndex].togglePattern);
        }

        moveCount = 0;
        UpdateMoveCounter();
    }

    void AddIfValid(List<int> pattern, int x, int y)
    {
        if (x >= 0 && x < Dimension && y >= 0 && y < Dimension)
        {
            int index = y * Dimension + x;
            if (!pattern.Contains(index))
            {
                pattern.Add(index);
            }
        }
    }

    void ToggleGroup(List<int> indexes)
    {
        foreach (int index in indexes)
        {
            if (index >= 0 && index < allTiles.Count)
            {
                allTiles[index].Toggle();
            }
        }
    } 

    bool AreAllTilesOff()
    {
        foreach (Tile tile in allTiles)
        {
            if (tile.IsOn) return false;
        }
        return true;
    }

    void UpdateMoveCounter()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = $"Moves: {moveCount}";
        }
    }

    void UpdateTilePosition(int x, int y, GameObject obj)
    {
        int xPos = -380;
        int yPos = 380;
        xPos += 190 * x;
        yPos -= 190 * y;
        obj.transform.localPosition = new Vector3(xPos, yPos, 0);
    }
    
    void OnGameWon()
    {
        isGameActive = false;

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (winStatsText != null)
            {
                string stats = $"Congratulations!\n\n";
                stats += $"Moves: {moveCount}\n";
                if (enableTimer)
                {
                    stats += $"Time: {FormatTime(gameTime)}\n";
                }
                stats += $"\nYou've solved the Signal Grid!";
                winStatsText.text = stats;
            }
        }
        UpdateFeedbackText("PUZZLE SOLVED!");
    }
    #endregion

    #region Timer
    void UpdateTimerDisplay()
    {
        if (timerText != null && enableTimer)
        {
            timerText.text = $"Time: {FormatTime(gameTime)}";
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    #endregion

    #region Feedback Text Handler
    void UpdateFeedbackText(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    #endregion

    #region Peek Action Handler
    public bool IsPeekModeActive()
    {
        return isPeekModeActive;
    }

    void EnablePeekMode()
    {
        if (peekUsesRemaining <= 0 || !isGameActive) return;
        isPeekModeActive = true;
        UpdateFeedbackText("Click a tile to see its pattern!");
        StartCoroutine(WaitForPeekSelection());
    }

    IEnumerator WaitForPeekSelection()
    {
        // Wait for the player to click a tile in the next frame.
        // The next tile clicked will show its pattern.
        // This is handled by modifying the click behavior temporarily
        yield return null;
    }

    public void ShowPeekInfo(Tile tile)
    {
        if (peekUsesRemaining <= 0) return;
        isPeekModeActive = false;
        peekUsesRemaining--;
        lastPeekedTile = tile;
        UpdatePeekButton();

        // Generate text pattern
        if (peekPatternText != null)
        {
            string patternText = GeneratePatternText(tile);
            peekOverlay.SetActive(true);
            peekPatternText.text = patternText;
        }

        UpdateFeedbackText("Click anywhere to continue.");
    }

    void HidePeekInfo()
    {
        lastPeekedTile = null;
        if (peekOverlay != null) peekOverlay.SetActive(false);
        UpdateFeedbackText(string.Empty);
    }

    IEnumerator HighlightTile(Tile tile)
    {
        Image img = tile.GetComponent<Image>();
        Color original = img.color;
        Color highlight = Color.white;

        float duration = 0.5f;
        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                img.color = Color.Lerp(original, highlight, Mathf.PingPong(elapsed * 2, 1));
                yield return null;
            }
        }

        img.color = original;
    }

    void UpdatePeekButton()
    {
        if (peekButton != null)
        {
            TextMeshProUGUI buttonText = peekButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Peek ({peekUsesRemaining})";
            }
            peekButton.interactable = peekUsesRemaining > 0 && isGameActive;
        }
    }

    #endregion

    #region Others
    public void RestartGame()
    {
        foreach (Tile tile in allTiles)
        {
            if (tile != null)
                Destroy(tile.gameObject);
        }

        allTiles.Clear();

        GameInitialization();
    }

    public void PlayButtonClickSound()
    {
        audioSource.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
