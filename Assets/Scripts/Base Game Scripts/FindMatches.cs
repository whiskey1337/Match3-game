using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCoroutine());
    }

    private List<GameObject> IsAdjacentBomb(Dot element1, Dot element2, Dot element3)
    {
        List<GameObject> currentElements = new List<GameObject>();
        if (element1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentElements(element1.column, element1.row));
        }

        if (element2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentElements(element2.column, element2.row));
        }

        if (element3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentElements(element3.column, element3.row));
        }
        return currentElements;
    }

    private List<GameObject> IsRowBomb(Dot element1, Dot element2, Dot element3)
    {
        List<GameObject> currentElements = new List<GameObject>();
        if (element1.isRowBomb)
        {
            currentMatches.Union(GetRowElements(element1.row));
            board.BombRow(element1.row);
        }

        if (element2.isRowBomb)
        {
            currentMatches.Union(GetRowElements(element2.row));
            board.BombRow(element2.row);
        }

        if (element3.isRowBomb)
        {
            currentMatches.Union(GetRowElements(element3.row));
            board.BombRow(element3.row);
        }
        return currentElements;
    }

    private List<GameObject> IsColumnBomb(Dot element1, Dot element2, Dot element3)
    {
        List<GameObject> currentElements = new List<GameObject>();
        if (element1.isColumnBomb)
        {
            currentMatches.Union(GetColumnElements(element1.column));
            board.BombColumn(element1.column);
        }

        if (element2.isColumnBomb)
        {
            currentMatches.Union(GetColumnElements(element2.column));
            board.BombColumn(element2.column);
        }

        if (element3.isColumnBomb)
        {
            currentMatches.Union(GetColumnElements(element3.column));
            board.BombColumn(element3.column);
        }
        return currentElements;
    }

    private void AddToListAndMatch(GameObject element)
    {
        if (!currentMatches.Contains(element))
        {
            currentMatches.Add(element);
        }
        element.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyElements(GameObject element1, GameObject element2, GameObject element3)
    {
        AddToListAndMatch(element1);
        AddToListAndMatch(element2);
        AddToListAndMatch(element3);
    }

    private IEnumerator FindAllMatchesCoroutine()
    {
        //yield return new WaitForSeconds(.1f);
        //yield return null;
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentElement = board.allDots[i, j];
                if (currentElement != null)
                {
                    Dot currentElementDot = currentElement.GetComponent<Dot>();
                    if (i > 0 && i < board.width - 1) // ЛАГИ ПРИ СТАРТЕ ИЗ-ЗА ЭТОГО БЛОКА IF
                    {
                        GameObject leftElement = board.allDots[i - 1, j];
                        GameObject rightElement = board.allDots[i + 1, j];
                        if (leftElement != null && rightElement != null)
                        {
                            Dot leftElementDot = leftElement.GetComponent<Dot>();
                            Dot rightElementDot = rightElement.GetComponent<Dot>();
                            if (leftElement.tag == currentElement.tag && rightElement.tag == currentElement.tag)
                            {
                                currentMatches.Union(IsRowBomb(leftElementDot, currentElementDot, rightElementDot));

                                currentMatches.Union(IsColumnBomb(leftElementDot, currentElementDot, rightElementDot));

                                currentMatches.Union(IsAdjacentBomb(leftElementDot, currentElementDot, rightElementDot));

                                GetNearbyElements(leftElement, currentElement, rightElement);
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1) // ЛАГИ ПРИ СТАРТЕ ИЗ-ЗА ЭТОГО БЛОКА IF
                    {
                        GameObject upElement = board.allDots[i, j + 1];
                        GameObject downElement = board.allDots[i, j - 1];
                        if (upElement != null && downElement != null)
                        {
                            Dot upElementDot = upElement.GetComponent<Dot>();
                            Dot downElementDot = downElement.GetComponent<Dot>();
                            if (upElement.tag == currentElement.tag && downElement.tag == currentElement.tag)
                            {
                                currentMatches.Union(IsColumnBomb(upElementDot, currentElementDot, downElementDot));

                                currentMatches.Union(IsRowBomb(upElementDot, currentElementDot, downElementDot));

                                currentMatches.Union(IsAdjacentBomb(upElementDot, currentElementDot, downElementDot));

                                GetNearbyElements(upElement, currentElement, downElement);
                            }
                        }
                    }
                }
            }
        }
        yield return null;
    }

    public void MatchElementsOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j= 0; j < board.height; j++)
            {
                // Проверка на существование элемента
                if (board.allDots[i, j] != null)
                {
                    // Проверка тега элемента
                    if (board.allDots[i, j].tag == color)
                    {
                        // Элемент подходит
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentElements(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                // Проверка - элемент внутри игрового поля
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allDots[i, j] != null)
                    {
                        dots.Add(board.allDots[i, j]);
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColumnElements(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                Dot dot = board.allDots[column, i].GetComponent<Dot>();
                if (dot.isRowBomb)
                {
                    dots.Union(GetRowElements(i)).ToList();
                }
                dots.Add(board.allDots[column, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowElements(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                Dot dot = board.allDots[i, row].GetComponent<Dot>();
                if (dot.isColumnBomb)
                {
                    dots.Union(GetColumnElements(i)).ToList();
                }
                dots.Add(board.allDots[i, row]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs(MatchType matchType)
    {
        // Перемещал ли игрок элементы?
        if (board.currentDot != null)
        {
            // Совпал ли элемент, который игрок переместил?
            if (board.currentDot.isMatched && board.currentDot.tag == matchType.color)
            {
                // Сделать его несовпадающим
                board.currentDot.isMatched = false;
                // Решить какой вид бомбы сгенерировать
                /*
                int typeOfBomb = Random.Range(0, 100);
                if (typeOfBomb < 50)
                {
                    // Генерируем бомбу в ряд
                    board.currentDot.MakeRowBomb();
                } else if (typeOfBomb >= 50)
                {
                    // Генерируем бомбу в столбец
                    board.currentDot.MakeColumnBomb();
                }
                */
                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                    || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                } else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            // Совпал ли другой элемент?
            else if (board.currentDot.otherElement != null)
            {
                Dot otherDot = board.currentDot.otherElement.GetComponent<Dot>();
                if (otherDot.isMatched && otherDot.tag == matchType.color)
                {
                    // Сделать его несовпадающим
                    otherDot.isMatched = false;
                    // Решить какой вид бомбы сгенерировать
                    /*
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        // Генерируем бомбу в ряд
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        // Генерируем бомбу в столбец
                        otherDot.MakeColumnBomb();
                    }
                    */
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                    || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }
}
