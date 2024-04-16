using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    public GameObject otherElement;

    private Animator animator;
    private float shineDelay;
    private float shineDelaySeconds;
    private EndGameManager endGameManager;
    private HintManager hintManager;
    private FindMatches findMatches;
    private Board board;
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;

    [Header("Swipe Variables")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Variables")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;
    public GameObject adjacentMarker;


    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        shineDelay = Random.Range(3f, 6f);
        shineDelaySeconds = shineDelay;
        animator = GetComponent<Animator>();

        endGameManager = FindObjectOfType<EndGameManager>();
        hintManager = FindObjectOfType<HintManager>();
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        //board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //previousRow = row;
        //previousColumn = column;
    }

    // Методы для тестирования и отладки
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        shineDelaySeconds -= Time.deltaTime;
        if (shineDelaySeconds <= 0)
        {
            shineDelaySeconds = shineDelay;
            StartCoroutine(StartShineCoroutine());
        }

        /*
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);
        }
        */

        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Сдвиг в сторону цели по горизонтали
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .02f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        } else {
            //Присваивание новой позиции по горизонтали
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Сдвиг в сторону цели по вертикали
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .02f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
                findMatches.FindAllMatches();
            }
        }
        else
        {
            //Присваивание новой позиции по вертикали
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    IEnumerator StartShineCoroutine()
    {
        animator.SetBool("Shine", true);
        yield return null;
        animator.SetBool("Shine", false);
    }

    public void PopAnimation()
    {
        animator.SetBool("Popped", true);
    }

    public IEnumerator CheckMoveCoroutine()
    {
        if (isColorBomb)
        {
            // Выбранный элемент является цветовой бомбой, а другой элементом для удаления
            findMatches.MatchElementsOfColor(otherElement.tag);
            isMatched = true;
        } else if (otherElement.GetComponent<Dot>().isColorBomb) {
            // Другой элемент является цветовой бомбой, а выбранный элементом для удаления
            findMatches.MatchElementsOfColor(this.gameObject.tag);
            otherElement.GetComponent<Dot>().isMatched = true;
        }
        yield return new WaitForSeconds(.2f);
        if (otherElement != null)
        {
            if (!isMatched && !otherElement.GetComponent<Dot>().isMatched)
            {
                otherElement.GetComponent<Dot>().row = row;
                otherElement.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.2f);
                board.currentDot = null;
                board.currentState = GameState.move;
            } else {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();
            }
            //otherElement = null;
        }
    }

    private void OnMouseDown()
    {
        if (animator != null)
        {
            animator.SetBool("Touched", true);
        }
        // Удалить подсказку
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        animator.SetBool("Touched", false);
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MoveElements();
            board.currentDot = this;
        } else {
            board.currentState = GameState.move;
        }
    }

    void MoveElementDirection(Vector2 direction)
    {
        otherElement = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (board.lockTiles[column, row] == null && board.lockTiles[column + (int)direction.x, row + (int)direction.y] == null)
        {
            if (otherElement != null)
            {
                otherElement.GetComponent<Dot>().column += -1 * (int)direction.x;
                otherElement.GetComponent<Dot>().row += -1 * (int)direction.y;
                column += (int)direction.x;
                row += (int)direction.y;
                StartCoroutine(CheckMoveCoroutine());
            }
            else
            {
                board.currentState = GameState.move;
            }
        }
        else
        {
            board.currentState = GameState.move;
        }
    }
    void MoveElements()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) //Сдвиг вправо
        {
            /*
            otherElement = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherElement.GetComponent<Dot>().column -= 1;
            column += 1;
            StartCoroutine(CheckMoveCoroutine());
            */
            MoveElementDirection(Vector2.right);
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) //Сдвиг вверх
        {
            /*
            otherElement = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherElement.GetComponent<Dot>().row -= 1;
            row += 1;
            StartCoroutine(CheckMoveCoroutine());
            */
            MoveElementDirection(Vector2.up);
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) //Сдвиг влево
        {
            /*
            otherElement = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherElement.GetComponent<Dot>().column += 1;
            column -= 1;
            StartCoroutine(CheckMoveCoroutine());
            */
            MoveElementDirection(Vector2.left);
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) //Сдвиг вниз
        {
            /*
            otherElement = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherElement.GetComponent<Dot>().row += 1;
            row -= 1;
            StartCoroutine(CheckMoveCoroutine());
            */
            MoveElementDirection(Vector2.down);
        } else
        {
            board.currentState = GameState.move;
        }
    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftElement = board.allDots[column - 1, row];
            GameObject rightElement = board.allDots[column + 1, row];
            if (leftElement != null && rightElement != null)
            {
                if (leftElement.tag == this.gameObject.tag && rightElement.tag == this.gameObject.tag)
                {
                    leftElement.GetComponent<Dot>().isMatched = true;
                    rightElement.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        if (row > 0 && row < board.height - 1)
        {
            GameObject upElement = board.allDots[column, row + 1];
            GameObject downElement = board.allDots[column, row - 1];
            if (upElement != null && downElement != null)
            {
                if (upElement.tag == this.gameObject.tag && downElement.tag == this.gameObject.tag)
                {
                    upElement.GetComponent<Dot>().isMatched = true;
                    downElement.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            } 
        }
    }

    public void MakeRowBomb()
    {
        /*
        if (!isColumnBomb && !isColorBomb && !isAdjacentBomb)
        {
            isRowBomb = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
        */
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        /*
        if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
        {
            isColumnBomb = true;
            GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;
        }
        */
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        /*
        if (!isColumnBomb && !isRowBomb && !isAdjacentBomb)
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
            this.gameObject.tag = "Color"; // P34 3rd bug ???
        }
        */
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
        this.gameObject.tag = "Color"; // P34 3rd bug ???
    }

    public void MakeAdjacentBomb()
    {
        /*
        if (!isColumnBomb && !isRowBomb && !isColorBomb)
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
        */
        isAdjacentBomb = true;
        GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }
}
