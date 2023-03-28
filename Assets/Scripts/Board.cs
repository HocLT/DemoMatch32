using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width = 7;
    public int height = 7;

    public GameObject tileBG;

    public Gem[] gems;

    public Gem[,] allGems;

    public float gemSpeed = 7f;

    public FindMatcher findMatcher;

    public enum BoardState { move, wait }
    public BoardState state = BoardState.move;

    public Gem bomb;
    public float bombChange = 2f;

    [HideInInspector]
    public RoundManager roundManager;

    private void Awake()
    {
        roundManager = FindObjectOfType<RoundManager>();
        findMatcher = FindObjectOfType<FindMatcher>();
    }

    // Start is called before the first frame update
    void Start()
    {
        allGems = new Gem[width, height];
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        //findMatcher.FindMatches();
    }

    void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // vẽ 1 tile background
                GameObject tile = Instantiate(tileBG, new Vector2(x, y), Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile BG - {x}, {y}";

                int gemToUse = Random.Range(0, gems.Length);

                // xử lý sinh new gem tạo ra match3
                int iteration = 0;
                while (Match(new Vector2Int(x, y), gems[gemToUse]) && iteration < 100) 
                {
                    gemToUse = Random.Range(0, gems.Length);
                    iteration++;
                }

                SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
            }
        }
    }

    void SpawnGem(Vector2Int pos, Gem gem)
    {
        if (Random.Range(0, 100f) < bombChange)
        {
            gem = bomb;
        }

        Gem spawnGem = Instantiate(gem, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        spawnGem.transform.parent = transform;
        spawnGem.name = $"Gem - {pos.x}, {pos.y}";

        spawnGem.SetupGem(pos, this);
        allGems[pos.x, pos.y] = spawnGem;   // lưu lại gem đã tạo
    }

    bool Match(Vector2Int posToCheck, Gem gemToCheck)
    {
        if (posToCheck.x > 1 && gemToCheck.type == allGems[posToCheck.x-1, posToCheck.y].type && gemToCheck.type == allGems[posToCheck.x - 2, posToCheck.y].type)
        {
            return true;
        }
        if (posToCheck.y > 1 && gemToCheck.type == allGems[posToCheck.x, posToCheck.y-1].type && gemToCheck.type == allGems[posToCheck.x, posToCheck.y-2].type)
        {
            return true;
        }
        return false;
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < findMatcher.allMatches.Count; i++)
        {
            ScoreCheck(findMatcher.allMatches[i]);
            DestroyMatchAt(findMatcher.allMatches[i].pos);
        }

        StartCoroutine(DecreaseRowCo());
    }

    void DestroyMatchAt(Vector2Int pos)
    {
        if (allGems[pos.x, pos.y] != null && allGems[pos.x, pos.y].isMatched)
        {
            Instantiate(allGems[pos.x, pos.y].destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);

            Destroy(allGems[pos.x, pos.y].gameObject);
            allGems[pos.x, pos.y] = null;
        }
    }

    IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.5f);

        int nullCounter = 0;    // đếm số ô trống của 1 cột

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    nullCounter++;
                }
                else if (nullCounter > 0)   // chuyển từ ô không có Gem sang ô có Gem
                {
                    allGems[x, y].pos.y -= nullCounter;
                    allGems[x, y - nullCounter] = allGems[x, y];
                    allGems[x, y] = null;
                }
            }
            nullCounter = 0;
        }
        StartCoroutine(FillBoardCo());
    }

    IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(.5f);
        RefillBoard();

        yield return new WaitForSeconds(.5f);
        // thực hiện tìm kiếm match3 lại
        findMatcher.FindMatches();
        if (findMatcher.allMatches.Count > 0)
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(.5f);
            state = BoardState.move;
        }
    }

    void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    int gemToUse = Random.Range(0, gems.Length);
                    SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
                }
            }
        }
        CheckMisplaceGem();
    }

    void CheckMisplaceGem()
    {
        List<Gem> foundGems = new List<Gem>();
        foundGems.AddRange(FindObjectsOfType<Gem>());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundGems.Contains(allGems[x, y]))
                {
                    foundGems.Remove(allGems[x, y]);
                }
            }
        }

        int count = foundGems.Count;
        for (int i = count-1; i >=0; i--)
        {
            Destroy(foundGems[i].gameObject);
            foundGems.Remove(foundGems[i]);
        }
        Debug.Log(foundGems.Count);
    }

    public void ScoreCheck(Gem gem)
    {
        roundManager.scoreValue += gem.scoreValue;
    }
}
