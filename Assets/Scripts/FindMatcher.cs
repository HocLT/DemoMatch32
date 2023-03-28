using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindMatcher : MonoBehaviour
{
    Board board;

    public List<Gem> allMatches = new List<Gem>();

    private void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindMatches()
    {
        allMatches.Clear();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Gem currentGem = board.allGems[x, y];
                if (currentGem != null)
                {
                    if (x > 0 && x < board.width - 1)
                    {
                        Gem leftGem = board.allGems[x - 1, y];
                        Gem rightGem = board.allGems[x + 1, y];
                        if (leftGem != null && rightGem != null)
                        {
                            if (currentGem.type == leftGem.type && currentGem.type == rightGem.type)
                            {
                                currentGem.isMatched = true;
                                leftGem.isMatched = true;
                                rightGem.isMatched = true;

                                allMatches.Add(currentGem);
                                allMatches.Add(rightGem);
                                allMatches.Add(leftGem);
                            }
                        }
                    }

                    if (y > 0 && y < board.height - 1)
                    {
                        Gem aboveGem = board.allGems[x, y + 1];
                        Gem belowGem = board.allGems[x, y - 1];
                        if (aboveGem != null && belowGem != null)
                        {
                            if (currentGem.type == aboveGem.type && currentGem.type == belowGem.type)
                            {
                                currentGem.isMatched = true;
                                aboveGem.isMatched = true;
                                belowGem.isMatched = true;

                                allMatches.Add(currentGem);
                                allMatches.Add(aboveGem);
                                allMatches.Add(belowGem);
                            }
                        }
                    }
                }
            }
        }

        if (allMatches.Count > 0)
        {
            allMatches = allMatches.Distinct().ToList();    // xóa trùng
        }

        CheckForBomb();
    }

    public void CheckForBomb()
    {
        for (int i = 0; i < allMatches.Count; i++)
        {
            Gem gem = allMatches[i];
            int x = gem.pos.x;
            int y = gem.pos.y;

            if (x > 0)
            {
                if (board.allGems[x-1, y] != null)
                {
                    if (board.allGems[x - 1, y].type == Gem.GemType.bomb)
                    {
                        MarkForBomb(new Vector2Int(x-1, y), board.allGems[x - 1, y]);
                    }
                }
            }

            if (x < board.width - 1)
            {
                if (board.allGems[x + 1, y] != null)
                {
                    if (board.allGems[x + 1, y].type == Gem.GemType.bomb)
                    {
                        MarkForBomb(new Vector2Int(x + 1, y), board.allGems[x + 1, y]);
                    }
                }
            }

            if (y > 0)
            {
                if (board.allGems[x, y - 1] != null)
                {
                    if (board.allGems[x, y - 1].type == Gem.GemType.bomb)
                    {
                        MarkForBomb(new Vector2Int(x, y - 1), board.allGems[x, y - 1]);
                    }
                }
            }

            if (y < board.height - 1)
            {
                if (board.allGems[x, y + 1] != null)
                {
                    if (board.allGems[x, y + 1].type == Gem.GemType.bomb)
                    {
                        MarkForBomb(new Vector2Int(x, y + 1), board.allGems[x, y + 1]);
                    }
                }
            }
        }
    }

    public void MarkForBomb(Vector2Int bombPos, Gem theBomb)
    {
        for (int x = bombPos.x - theBomb.bombRadius; x <= bombPos.x + theBomb.bombRadius; x++)
        {
            for (int y = bombPos.y - theBomb.bombRadius; y <= bombPos.y + theBomb.bombRadius; y++)
            {
                if (x >= 0 && x < board.width && y >= 0 && y < board.height)
                {
                    if (board.allGems[x, y] != null)
                    {
                        board.allGems[x, y].isMatched = true;
                        allMatches.Add(board.allGems[x, y]);
                    }
                }
            }
        }
        allMatches = allMatches.Distinct().ToList();
    }
}
