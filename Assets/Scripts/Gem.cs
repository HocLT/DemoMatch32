using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    //[HideInInspector]
    public Vector2Int pos;
    //[HideInInspector]
    public Board board;

    Vector3 firstPosition;
    Vector3 finalPosition;

    bool mousePressed;

    float swipeAngle = 0f;

    Gem otherGem;

    public enum GemType { blue, green, yellow, red, purple, bomb }
    public GemType type;

    public bool isMatched;

    Vector2Int previousPos;

    public GameObject destroyEffect;

    public int bombRadius = 1;

    public int scoreValue = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // thực hiện di chuyển Gem trên màn hình
        if (Vector2.Distance(transform.position, pos) > .01f)
        {
            transform.position = Vector2.Lerp(transform.position, pos, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            // update lại thông tin gem
            transform.position = new Vector3(pos.x, pos.y, 0f);
            board.allGems[pos.x, pos.y] = this;
        }

        if (mousePressed && Input.GetMouseButtonUp(0)) 
        {
            mousePressed = false;

            if (board.state == Board.BoardState.move && board.roundManager.roundTime > 0)
            {
                finalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();   // gọi hàm tính góc
            }
        }
    }

    public void SetupGem(Vector2Int pos, Board board)
    {
        this.pos = pos;
        this.board = board;
    }

    private void OnMouseDown()
    {
        if (board.state == Board.BoardState.move && board.roundManager.roundTime > 0)
        {
            firstPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstPosition);
            mousePressed = true;
        }
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalPosition.y - firstPosition.y, finalPosition.x - firstPosition.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;
        //Debug.Log(swipeAngle);
        if (Vector3.Distance(finalPosition, firstPosition) > .5f)
        {
            MovePieces();
        }
    }

    void MovePieces()
    {
        previousPos = pos;  // lưu lại vị trí cũ

        if (swipeAngle > -45 && swipeAngle < 45 && pos.x < board.width-1)
        {
            otherGem = board.allGems[pos.x + 1, pos.y];
            pos.x++;
            otherGem.pos.x--;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && pos.y < board.height-1)
        {
            otherGem = board.allGems[pos.x, pos.y + 1];
            pos.y++;
            otherGem.pos.y--;
        }
        else if (swipeAngle > -135 && swipeAngle <= -45 && pos.y > 0)
        {
            otherGem = board.allGems[pos.x, pos.y - 1];
            pos.y--;
            otherGem.pos.y++;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && pos.x > 0)
        {
            otherGem = board.allGems[pos.x - 1, pos.y];
            pos.x--;
            otherGem.pos.x++;
        }

        if (otherGem != null)
        {
            // update gem in board
            board.allGems[pos.x, pos.y] = this;
            board.allGems[otherGem.pos.x, otherGem.pos.y] = otherGem;

            // kiểm tra có xảy ra match3 sau khi chuyển Gem ko?, nếu không có, chuyển ngược trở lại
            StartCoroutine(CheckMoveCo());
        }
    }

    IEnumerator CheckMoveCo()
    {
        board.state = Board.BoardState.wait;

        yield return new WaitForSeconds(.5f);

        // y/c xử lý match3
        board.findMatcher.FindMatches();
        if (otherGem != null)
        {
            if (!isMatched && !otherGem.isMatched)
            {
                otherGem.pos = pos;
                pos = previousPos;

                // update gem in board
                board.allGems[pos.x, pos.y] = this;
                board.allGems[otherGem.pos.x, otherGem.pos.y] = otherGem;

                yield return new WaitForSeconds(.5f);
                board.state = Board.BoardState.move;
            }
            else // trường hợp có match3 => xóa gem
            {
                board.DestroyMatches();
            }
        }
    }
}
