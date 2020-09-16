using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomID {
    Player1Start = -1,
    Player2Start = -2,
    Player3Start = -3,
    Player4Start = -4,
    CentralChamber = -5,
    Team1Start = -6,
    Team2Start = -7
};

[System.Serializable]
public struct RoomParams {
    public string name;
    public RoomID id;
    public int x;
    public int y;
    public int width;
    public int height;

}

//A representation of a room in a dungeon
class Room {
    public List<Vector2Int> squares;
    public List<Room> neighbors;//list of all neighboring rooms
    public List<GameObject> doors;//list of doors at edge of this room
    public bool reached; //have I been reached in MST?
    public int roomID;
    public Room(int roomnum) {
        squares = new List<Vector2Int>();
        neighbors = new List<Room>();
        doors = new List<GameObject>();
        reached = false;
        this.roomID = roomnum;
    }

    //spawns item somewhere within the confines of the room
    //default rotation
    public GameObject SpawnItem(GameObject item, Transform parent = null) {
        int randomSquareIndex = Random.Range(0, squares.Count);
        Vector2Int square = squares[randomSquareIndex];
        Vector3 pos = GameManager.makeWalls.GetCellPositionFor(square.x, square.y);
        float centerToWall = GameManager.makeWalls.Wall.transform.localScale.x / 2f;
        pos += new Vector3(Random.Range(-centerToWall, centerToWall), Random.Range(-centerToWall, centerToWall), 0f);
        return GameObject.Instantiate(item, pos, Quaternion.identity, parent);
    }

    //spawns item somewhere within the confines of the room
    //specified rotation
    public GameObject SpawnItem(GameObject item, Quaternion rot, Transform parent = null) {
        int randomSquareIndex = Random.Range(0, squares.Count);
        Vector2Int square = squares[randomSquareIndex];
        Vector3 pos = GameManager.makeWalls.GetCellPositionFor(square.x, square.y);
        float centerToWall = GameManager.makeWalls.Wall.transform.localScale.x / 2f;
        pos += new Vector3(Random.Range(-centerToWall, centerToWall), Random.Range(-centerToWall, centerToWall), 0f);
        return GameObject.Instantiate(item, pos, rot, parent);
    }

    //spawns item at the center of the room (average of square positions)
    //will be used for rare/unique items for dramatic effect
    public GameObject SpawnItemAtCenter(GameObject item, Transform parent = null) {
        Vector3 weightedAvg = new Vector3(0, 0, 0);
        foreach (Vector2Int square in squares) {
            weightedAvg += GameManager.makeWalls.GetCellPositionFor(square.x, square.y);
        }
        weightedAvg *= (1f / squares.Count);
        Vector2Int closestSquare = squares[0];
        float minDist = Vector3.Distance(weightedAvg, GameManager.makeWalls.GetCellPositionFor(closestSquare.x, closestSquare.y));
        foreach (Vector2Int square in squares) {
            float dist = Vector3.Distance(weightedAvg, GameManager.makeWalls.GetCellPositionFor(square.x, square.y));
            if (dist < minDist) {
                closestSquare = square;
                minDist = dist;
            }
        }
        Vector3 finalPos = GameManager.makeWalls.GetCellPositionFor(closestSquare.x, closestSquare.y);
        return GameObject.Instantiate(item, finalPos, Quaternion.identity, parent);
    }
}