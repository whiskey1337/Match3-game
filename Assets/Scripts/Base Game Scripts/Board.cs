using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

// Замечания в P31 по SetUp()

public enum GameState
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileCategory
{
    Breakable,
    Blank,
    Lock,
    Concrete,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileCategory tileCategory;
}

public class Board : MonoBehaviour
{
    [Header("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;

    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject[,] allDots;
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;
    

    [Header("Layout")]
    public TileType[] boardLayout;
    public Dot currentDot;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    public BackgroundTile[,] concreteTiles;

    [Header("Match Stuff")]
    public MatchType matchType;
    private FindMatches findMatches;
    public int baseElementValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        goalManager = FindObjectOfType<GoalManager>();
        soundManager = FindObjectOfType<SoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileCategory == TileCategory.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileCategory == TileCategory.Breakable)
            {
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject element = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = element.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateLockTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileCategory == TileCategory.Lock)
            {
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject element = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = element.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateConcreteTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileCategory == TileCategory.Concrete)
            {
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject element = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = element.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + ", " + j + " )";
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";
                    allDots[i, j] = dot;
                }
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject element)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == element.tag && allDots[column - 2, row].tag == element.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
            {
                if (allDots[column, row - 1].tag == element.tag && allDots[column, row - 2].tag == element.tag)
                {
                    return true;
                }
            }
        } else if (column <= 1 || row <= 1) 
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == element.tag && allDots[column, row - 2].tag == element.tag)
                    {
                        return true;
                    }
                }
            }

            if (column > 1)
            {
                if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
                {
                    if (allDots[column - 1, row].tag == element.tag && allDots[column - 2, row].tag == element.tag)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private MatchType ColumnOrRow()
    {
        
        // Сделать копию текущих комбинаций
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = 0;
        matchType.color = "";

        // Пройтись циклом по копии комбинаций и решить нужна ли генерация бомбы
        for (int i = 0; i < matchCopy.Count; i++)
        {
            // Сохранить элемент
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;

            // Пройтись циклом по остальным элементам и сравнить
            for (int j = 0; j < matchCopy.Count; j++)
            {
                // Сохранить следующий элемент
                Dot nextDot = matchCopy[j].GetComponent<Dot>();
                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.column == thisDot.column && nextDot.tag == color)
                {
                    columnMatch++;
                }
                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            // return 3, если три в ряд
            // return 2, если стандартная бомба
            // return 1, если цветовая бомба
            if (columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if (columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }
        }
        matchType.type = 0;
        matchType.color = "";
        return matchType;
        
        /*
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstElement = findMatches.currentMatches[0].GetComponent<Dot>();
        if (firstElement != null)
        {
            foreach (GameObject currentElement in findMatches.currentMatches)
            {
                Dot element = currentElement.GetComponent<Dot>();
                if (element.row == firstElement.row)
                {
                    numberHorizontal++;
                }
                if (element.column == firstElement.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
        */
    }

    private void CheckToMakeBombs()
    {
        
        if (findMatches.currentMatches.Count > 3)
        {
            MatchType typeOfMatch = ColumnOrRow();
            if (typeOfMatch.type == 1)
            {
                // Сгенерировать цветную бомбу
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeColorBomb();
                }
                else 
                {
                    if (currentDot.otherElement != null)
                    {
                        Dot otherElement = currentDot.otherElement.GetComponent<Dot>();
                        if (otherElement.isMatched && otherElement.tag == typeOfMatch.color)
                        {
                            otherElement.isMatched = false;
                            otherElement.MakeColorBomb();
                        }
                    }
                }
            }
            else if (typeOfMatch.type == 2)
            {
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjacentBomb();
                }
                else if (currentDot.otherElement != null)
                {
                    Dot otherElement = currentDot.otherElement.GetComponent<Dot>();
                    if (otherElement.isMatched && otherElement.tag == typeOfMatch.color)
                    {
                        otherElement.isMatched = false;
                        otherElement.MakeAdjacentBomb();
                    }
                }
            }
            else if (typeOfMatch.type == 3)
            {
                findMatches.CheckBombs(typeOfMatch);
            }
        }
        
        /*
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }

        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                // Сгенерировать цветную бомбу
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    } else {
                        if (currentDot.otherElement != null)
                        {
                            Dot otherElement = currentDot.otherElement.GetComponent<Dot>();
                            if (otherElement.isMatched)
                            {
                                if (!otherElement.isColorBomb)
                                {
                                    otherElement.isMatched = false;
                                    otherElement.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            } else {
                // Сгенерировать бомбу 3x3
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherElement != null)
                        {
                            Dot otherElement = currentDot.otherElement.GetComponent<Dot>();
                            if (otherElement.isMatched)
                            {
                                if (!otherElement.isAdjacentBomb)
                                {
                                    otherElement.isMatched = false;
                                    otherElement.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
        */
    }

    public void BombRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[i, row])
            {
                concreteTiles[i, row].TakeDamage(1);
                if (concreteTiles[i, row].hitPoints <= 0)
                {
                    concreteTiles[i, row] = null;
                }
            }
        }
    }

    public void BombColumn(int column)
    {
        for (int i = 0; i < width; i++)
        {
            if (concreteTiles[column, i])
            {
                concreteTiles[column, i].TakeDamage(1);
                if (concreteTiles[column, i].hitPoints <= 0)
                {
                    concreteTiles[column, i] = null;
                }
            }
        }
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            if (breakableTiles[column, row] != null)
            {
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            if (lockTiles[column, row] != null)
            {
                lockTiles[column, row].TakeDamage(1);
                if (lockTiles[column, row].hitPoints <= 0)
                {
                    lockTiles[column, row] = null;
                }
            }
            DamageConcrete(column, row);
            if (goalManager != null)
            {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }

            if (soundManager != null)
            {
                soundManager.PlayRandomDestroySound();
            }
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            allDots[column, row].GetComponent<Dot>().PopAnimation();
            Destroy(allDots[column, row], 0.2f);
            scoreManager.IncreaseScore(baseElementValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        // Какое количество элементов в комбинации из FindMatches?
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCoroutine2());
    }

    private void DamageConcrete(int column, int row)
    {
        if (column > 0)
        {
            if (concreteTiles[column - 1, row])
            {
                concreteTiles[column - 1, row].TakeDamage(1);
                if (concreteTiles[column - 1, row].hitPoints <= 0)
                {
                    concreteTiles[column - 1, row] = null;
                }
            }
        }
        if (column < width - 1)
        {
            if (concreteTiles[column + 1, row])
            {
                concreteTiles[column + 1, row].TakeDamage(1);
                if (concreteTiles[column + 1, row].hitPoints <= 0)
                {
                    concreteTiles[column + 1, row] = null;
                }
            }
        }
        if (row > 0)
        {
            if (concreteTiles[column, row - 1])
            {
                concreteTiles[column, row - 1].TakeDamage(1);
                if (concreteTiles[column, row - 1].hitPoints <= 0)
                {
                    concreteTiles[column, row - 1] = null;
                }
            }
        }
        if (row < height - 1)
        {
            if (concreteTiles[column, row + 1])
            {
                concreteTiles[column, row + 1].TakeDamage(1);
                if (concreteTiles[column, row + 1].hitPoints <= 0)
                {
                    concreteTiles[column, row + 1] = null;
                }
            }
        }
    }

    private IEnumerator DecreaseRowCoroutine2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // если текущая позиция не blank и пустая
                if (!blankSpaces[i, j] && allDots[i, j] == null && !concreteTiles[i, j])
                {
                    // цикл с элемента выше и до конца столбца
                    for (int k = j + 1; k < height; k++)
                    {
                        // если элемент найден
                        if (allDots[i, k] != null)
                        {
                            // передвинуть элемент в пустое место
                            allDots[i, k].GetComponent<Dot>().row = j;
                            // сделать позицию null
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f); //.4f
        StartCoroutine(FillBoardCoroutine());
    }

    private IEnumerator DecreaseRowCoroutine()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                } else if (nullCount > 0) {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay * 0.5f); //.4f
        StartCoroutine(FillBoardCoroutine());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }
                    maxIterations = 0;
                    GameObject element = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = element;
                    element.GetComponent<Dot>().row = j;
                    element.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        findMatches.FindAllMatches();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCoroutine()
    {
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        {
            streakValue ++;
            DestroyMatches();
            yield break;
            //yield return new WaitForSeconds(2 * refillDelay);
        }
        currentDot = null;

        if(IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!!");
        }
        yield return new WaitForSeconds(refillDelay);
        System.GC.Collect();
        if (currentState != GameState.pause)
            currentState = GameState.move;
        streakValue = 1;
    }

    private void SwitchElements(int column, int row, Vector2 direction)
    {
        if (allDots[column + (int)direction.x, row + (int)direction.y] != null)
        {
            // сохранить второй элемент
            GameObject holder = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
            // переместить первый элемент на вторую позицию
            allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
            // установить позицию первого элемента второму
            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    // проверка выхода за зону игрового поля
                    if (i < width - 2)
                    {
                        // проверка на существование двух элементов справа
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        // проверка на существование двух элементов сверху
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchElements(column, row, direction);
        if(CheckForMatches())
        {
            SwitchElements(column, row, direction);
            return true;
        }
        SwitchElements(column, row, direction);
        return false;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        List<GameObject> newBoard = new List<GameObject>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i, j])
                {
                    int elementToUse = Random.Range(0, newBoard.Count);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, newBoard[elementToUse]) && maxIterations < 100)
                    {
                        elementToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    Dot element = newBoard[elementToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    element.column = i;
                    element.row = j;
                    allDots[i, j] = newBoard[elementToUse];
                    newBoard.Remove(newBoard[elementToUse]);
                }
            }
        }
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }
}
