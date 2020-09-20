using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridControl : MonoBehaviour {
    public GameObject[,] grid;
    private List<GameObject> reachedTiles;
    private List<GameObject> validWordTiles;
    public char placeholder = ' ';
    private Words words;
    private AudioSource getKeySource;
    public int ownerNum;
    public bool globalGrid;
    public bool claimable;

    private int width;

    void Awake() {
        grid = new GameObject[GetComponent<MakeGrid>().width, GetComponent<MakeGrid>().width];
        reachedTiles = new List<GameObject>();
        validWordTiles = new List<GameObject>();
    }

    void Start() {
        width = GetComponent<MakeGrid>().width;
        words = GameManager.words;
        getKeySource = GameObject.Find("GetKeySFX").GetComponent<AudioSource>();
        globalGrid = ownerNum == 0;
        if (!globalGrid) {
            //recolor grid squares
            Color ownerColor = GameManager.teamByID(ownerNum).color;
            foreach (Transform child in transform) {
                child.gameObject.GetComponent<GridSquare>().SetColor(ownerColor);
            }
        }
    }

    //called on any space in the grid the player just interacted with to see if any new words have formed
    //player is the player who just placed the tile in the grid
    public void ValidateWords(int x, int y, GameObject player) {
        validWordTiles.Clear();
        int makerNum = player.GetComponent<PlayerInfo>().playerNum; //who made the word?
        if (grid[x, y].GetComponent<GridSquare>().GetLetter() == placeholder) {
            ValidateWord(x - 1, y, makerNum, horizontal: true);
            ValidateWord(x + 1, y, makerNum, horizontal: true);
            ValidateWord(x, y - 1, makerNum, horizontal: false);
            ValidateWord(x, y + 1, makerNum, horizontal: false);
        } else {
            ValidateWord(x, y, makerNum, horizontal: true);
            ValidateWord(x, y, makerNum, horizontal: false);
        }
        for (int i = 0; i < GetScore(validWordTiles.Count); i++) {
            AddKey();
        }
        foreach (GameObject tile in validWordTiles) {
            tile.GetComponent<LetterTile>().DecLifespan();
        }
        validWordTiles.Clear();
    }

    public void ValidateWord(int x, int y, int makerNum, bool horizontal = false) {
        string word;
        if (horizontal) {
            word = GetHorizontalWord(x, y);
        } else {
            word = GetVerticalWord(x, y);
        }
        if (words.ValidateWord(word, ownerNum, makerNum, globalGrid)) {
            foreach (GameObject tile in reachedTiles) {
                if (!validWordTiles.Contains(tile)) {
                    validWordTiles.Add(tile);
                }
            }
        }
    }
    public void AddKey() {
        if (globalGrid) {
            //give everyone a key
            GameManager.addScoreToEveryone();
        } else {
            GameManager.addScore(ownerNum);
        }
    }

    //Gets the longest word including tile [x,y] which is horizontal
    public string GetHorizontalWord(int x, int y) {
        reachedTiles.Clear();
        int width = GetComponent<MakeGrid>().width;
        if (x < 0 || x >= width || y < 0 || y >= width || grid[x, y].GetComponent<GridSquare>().GetLetter() == placeholder) {
            //			print ("out of bounds - GetHorizontalWord returning empty string");
            return "";
        }
        string result = grid[x, y].GetComponent<GridSquare>().GetLetter().ToString();
        reachedTiles.Add(grid[x, y].GetComponent<GridSquare>().tile);
        int i = x - 1;
        while (i != -1) {
            //			print (i);
            if (i < 0 || grid[i, y].GetComponent<GridSquare>().GetLetter() == placeholder) {
                i = -1;
            } else {
                result = grid[i, y].GetComponent<GridSquare>().GetLetter() + result;
                reachedTiles.Add(grid[i, y].GetComponent<GridSquare>().tile);
                i--;
            }
        }
        i = x + 1;
        while (i != -1) {
            //			print (i);
            if (i >= width || grid[i, y].GetComponent<GridSquare>().GetLetter() == placeholder) {
                i = -1;
            } else {
                result += grid[i, y].GetComponent<GridSquare>().GetLetter();
                reachedTiles.Add(grid[i, y].GetComponent<GridSquare>().tile);
                i++;
            }
        }
        return result;
    }

    //Gets the longest word including tile [x,y] which is vertical
    public string GetVerticalWord(int x, int y) {
        reachedTiles.Clear();
        int width = GetComponent<MakeGrid>().width;
        if (x < 0 || x >= width || y < 0 || y >= width || grid[x, y].GetComponent<GridSquare>().GetLetter() == placeholder) {
            //			print ("out of bounds - GetVerticalWord returning empty string");
            return "";
        }
        string result = grid[x, y].GetComponent<GridSquare>().GetLetter().ToString();
        reachedTiles.Add(grid[x, y].GetComponent<GridSquare>().tile);
        int i = y - 1;
        while (i != -1) {
            //			print (i);
            if (i < 0 || grid[x, i].GetComponent<GridSquare>().GetLetter() == placeholder) {
                i = -1;
            } else {
                result = grid[x, i].GetComponent<GridSquare>().GetLetter() + result;
                reachedTiles.Add(grid[x, i].GetComponent<GridSquare>().tile);
                i--;
            }
        }
        i = y + 1;
        while (i != -1) {
            //			print (i);
            if (i >= width || grid[x, i].GetComponent<GridSquare>().GetLetter() == placeholder) {
                i = -1;
            } else {
                result += grid[x, i].GetComponent<GridSquare>().GetLetter();
                reachedTiles.Add(grid[x, i].GetComponent<GridSquare>().tile);
                i++;
            }
        }
        return result;
    }


    public int GetScore(int wordLength) {
        if (wordLength < 4) {
            return 0;
        }
        if (wordLength < 6) {
            return 1;
        }
        if (wordLength == 6) {
            return 2;
        }
        return wordLength - 4;
    }

    public void SetOwnership(int newOwnerNum) {
        if (claimable) {
            ownerNum = newOwnerNum;
            globalGrid = false;
        }
    }

    //recolor animation upon placing flag
    private bool InBounds(int x, int y) {
        if (x >= 0 && x < width && y >= 0 && y < width) {
            return true;
        }
        return false;
    }

    public List<GameObject> GetNeighbors(GameObject square) {
        if (!square.GetComponent<GridSquare>()) {
            return new List<GameObject>();
        }
        List<GameObject> result = new List<GameObject>();
        int x = square.GetComponent<GridSquare>().x;
        int y = square.GetComponent<GridSquare>().y;
        if (InBounds(x + 1, y)) {
            result.Add(grid[x + 1, y]);
        }
        if (InBounds(x - 1, y)) {
            result.Add(grid[x - 1, y]);
        }
        if (InBounds(x, y + 1)) {
            result.Add(grid[x, y + 1]);
        }
        if (InBounds(x, y - 1)) {
            result.Add(grid[x, y - 1]);
        }
        return result;
    }

    public void StartRecoloring(Color ownerColor, Color darkerColor, GameObject square) {
        StartCoroutine(Recolor(ownerColor, darkerColor, new List<GameObject> { square }));
    }

    private IEnumerator Recolor(Color ownerColor, Color darkerColor, List<GameObject> squaresToApply) {
        yield return new WaitForSeconds(0.05f);
        List<GameObject> nextRound = new List<GameObject>();
        foreach (GameObject square in squaresToApply) {
            square.GetComponent<SpriteRenderer>().color = darkerColor;
            square.GetComponent<GridSquare>().normalColor = darkerColor;
            square.GetComponent<GridSquare>().highlightedColor = ownerColor;
            square.GetComponent<GridSquare>().Swell();
            List<GameObject> neighbors = GetNeighbors(square);
            foreach (GameObject neighbor in neighbors) {
                if (neighbor.GetComponent<SpriteRenderer>().color != darkerColor && !nextRound.Contains(neighbor)) {
                    nextRound.Add(neighbor);
                }
            }
        }
        if (nextRound.Count > 0) {
            StartCoroutine(Recolor(ownerColor, darkerColor, nextRound));
        }
    }
}