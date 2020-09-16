﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {
    #region fields
    //back end - grid of rooms
    public string mode;
    public int width;
    public List<RoomParams> presetRooms;
    private int numSquares;
    private int numCheckedOff;
    private int numRooms;
    [HideInInspector]
    public int[,] rooms;
    Dictionary<int, Room> roomGraph;

    //front end - generated game objects
    private const float epsilon = 0.005f; //makes borders between walls/corners look better
    private const float smallWallOffset = 0.32f;
    private float cellSize;
    private Vector3 basePosition;
    private Quaternion vertical;
    public GameObject DoorContainer;
    public GameObject WallContainer;
    public GameObject TileContainer;
    public GameObject LootContainer;
    public GameObject GridContainer;
    public GameObject FogOfWarContainer;//container for fog of war objects
    public GameObject[,] FogOfWarArray;//grid of fog of war objects
    public GameObject Wall;
    public GameObject Corner;
    public GameObject Door;
    public GameObject WallSmall;
    public GameObject Void;//fog of war objects
    public GameObject Tile;
    public GameObject Grid;//grid prefab
    public GameObject[] loot;
    public List<GameObject> CloseLoot;  // loot nearby
    public List<GameObject> DeepLoot; // loot far away
    #endregion

    #region main
    // Use this for initialization
    void Awake() {
        Destroy(GetComponent<SpriteRenderer>());
        cellSize = Wall.transform.localScale.x + Corner.transform.localScale.x - epsilon;
        // print("cellSize: " + cellSize);
        basePosition = new Vector3(-(width / 2) * cellSize, (width / 2) * cellSize, 0f);
        vertical = Quaternion.Euler(0, 0, 90);
        FogOfWarArray = new GameObject[width, width];

        FillRooms();
        FillRoomGraph();
        GenerateWallsAndLoot();
        //PlaceDebugDoors();
        PlaceFogOfWar();
        MakeLetterTiles();
        print("source word: " + GetComponent<Words>().GetSourceWord(0) + " score: " + GetComponent<Words>().levelScore);
    }
    #endregion

    #region backend
    //BACK END
    void FillRooms() {
        rooms = new int[width, width];
        numRooms = 1;
        numCheckedOff = 0;
        numSquares = width * width;
        while (numCheckedOff < numSquares) {
            MakeRoom(Random.Range(0, width), Random.Range(0, width), Random.Range(4, 7), Random.Range(4, 7));
        }
        MakePresetRooms();
    }

    void MakePresetRooms() {
        int halfX = width / 2;
        foreach (RoomParams r in presetRooms) {
            MakeRoom(halfX + r.x, halfX + r.y, r.width, r.height, (int)r.id);
        }
    }

    //is this coordinate in bounds?
    bool InBounds(int x, int y) {
        return (x >= 0 && x < width && y >= 0 && y < width);
    }

    //makes a room of width w and height h centered at [x,y]
    void MakeRoom(int x, int y, int w, int h) {
        for (int i = -(w / 2); i <= w / 2; i++) {
            for (int j = -(h / 2); j <= h / 2; j++) {
                int a = i + x;
                int b = j + y;
                if (InBounds(a, b)) {
                    if (rooms[a, b] == 0) {
                        numCheckedOff++;
                    }
                    rooms[a, b] = numRooms;
                }
            }
        }
        numRooms++;
    }

    //makes a room of width w and height h centered at [x,y] with room designation num
    //for special rooms
    void MakeRoom(int x, int y, int w, int h, int num) {
        for (int i = -(w / 2); i <= w / 2; i++) {
            for (int j = -(h / 2); j <= h / 2; j++) {
                int a = i + x;
                int b = j + y;
                if (InBounds(a, b)) {
                    if (rooms[a, b] == 0) {
                        numCheckedOff++;
                    }
                    rooms[a, b] = num;
                }
            }
        }
    }

    //C# mod is not too useful
    int correctmod(int a, int n) {
        return ((a % n) + n) % n;
    }

    //test if back end is giving the right output by mapping room nums to ascii chars and filling a string with that
    //makes it absurdly slow for large sizes because Unity's print buffer is expecting a whole bunch of small messages rather than a single large message
    void PrintRooms() {
        string result = "";
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < width; j++) {
                result += (char)(correctmod(rooms[j, i], 94) + 32) + " ";
            }
            result += "\n";
        }
        print(result);
    }

    List<Vector2Int> GetNeighbors(Vector2Int xy) {
        int x = xy.x;
        int y = xy.y;
        List<Vector2Int> result = new List<Vector2Int>();
        if (InBounds(x + 1, y)) {
            result.Add(new Vector2Int(x + 1, y));
        }
        if (InBounds(x, y + 1)) {
            result.Add(new Vector2Int(x, y + 1));
        }
        if (InBounds(x - 1, y)) {
            result.Add(new Vector2Int(x - 1, y));
        }
        if (InBounds(x, y - 1)) {
            result.Add(new Vector2Int(x, y - 1));
        }
        return result;
    }

    List<Vector2Int> GetNeighborsRightAndBottom(Vector2Int xy) {
        int x = xy.x;
        int y = xy.y;
        List<Vector2Int> result = new List<Vector2Int>();
        if (InBounds(x + 1, y)) {
            result.Add(new Vector2Int(x + 1, y));
        }
        if (InBounds(x, y + 1)) {
            result.Add(new Vector2Int(x, y + 1));
        }
        return result;
    }

    //helper for FillRoomGraph
    //starting at (x,y), find all neighboring squares
    void FloodFillRoomIntoGraph(int x, int y, int newroomnum, Dictionary<Vector2Int, int> reachedSquares) {
        int orig_roomnum = rooms[x, y];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Vector2Int xy = new Vector2Int(x, y);
        q.Enqueue(xy);
        reachedSquares[xy] = 0;
        while (q.Count > 0) {
            Vector2Int square = q.Dequeue();
            roomGraph[newroomnum].squares.Add(square);
            rooms[square.x, square.y] = newroomnum;
            foreach (Vector2Int neighbor in GetNeighborsRightAndBottom(square)) {
                if (rooms[neighbor.x, neighbor.y] == orig_roomnum && !reachedSquares.ContainsKey(neighbor)) {
                    q.Enqueue(neighbor);
                    reachedSquares[neighbor] = 0;
                }
            }
        }
    }

    //analyze room grid and convert it into an adjacency list of room objects
    void FillRoomGraph() {
        roomGraph = new Dictionary<int, Room>();
        Dictionary<Vector2Int, int> reachedSquares = new Dictionary<Vector2Int, int>();
        //put correct squares in each room
        //also, scan for bisected rooms and rename each chunk
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < width; y++) {
                int roomnum = rooms[x, y];
                //print(roomnum);
                if (roomGraph.ContainsKey(roomnum)) {
                    if (roomGraph[roomnum].reached && !reachedSquares.ContainsKey(new Vector2Int(x, y))) {
                        //get new room num
                        roomnum = numRooms;
                        numRooms++;
                        //add new room, floodfill
                        roomGraph.Add(roomnum, new Room(roomnum));
                        FloodFillRoomIntoGraph(x, y, roomnum, reachedSquares);
                        roomGraph[roomnum].reached = true;
                    }
                } else {
                    //add new room, floodfill
                    roomGraph.Add(roomnum, new Room(roomnum));
                    FloodFillRoomIntoGraph(x, y, roomnum, reachedSquares);
                    roomGraph[roomnum].reached = true;
                }
            }
        }

        //find correct neighbors for each room
        foreach (Room room in roomGraph.Values) {
            foreach (Vector2Int square in room.squares) {
                int numDissimilar = 0;
                foreach (Vector2Int neighbor in GetNeighborsRightAndBottom(square)) {
                    if (rooms[neighbor.x, neighbor.y] != rooms[square.x, square.y]) {
                        numDissimilar++;
                        Room otherGuy = roomGraph[rooms[neighbor.x, neighbor.y]];
                        if (!room.neighbors.Contains(otherGuy)) {
                            otherGuy.neighbors.Add(room);
                            room.neighbors.Add(otherGuy);
                        }
                    }
                }
            }
        }
        //I probably reuse room.reached in a later floodfill pass so I will set that to false
        foreach (Room room in roomGraph.Values) {
            room.reached = false;
        }
    }
    #endregion

    #region frontend
    //FRONT END
    //runs BFS starting from player starting rooms.
    void GenerateWallsAndLoot() {
        // print("makin dungeon walls");
        Queue<Room> q1 = new Queue<Room>();//all rooms at the current layer of depth
        Queue<Room> q2 = new Queue<Room>();//all rooms at the next layer of depth

        //start with player starting rooms
        if (mode == "Versus") {
            q1.Enqueue(roomGraph[(int)RoomID.Player1Start]);
            q1.Enqueue(roomGraph[(int)RoomID.Player2Start]);
            q1.Enqueue(roomGraph[(int)RoomID.Player3Start]);
            q1.Enqueue(roomGraph[(int)RoomID.Player4Start]);
        } else if (mode == "2v2") {
            q1.Enqueue(roomGraph[(int)RoomID.Team1Start]);
            q1.Enqueue(roomGraph[(int)RoomID.Team2Start]);
        }

        //Make the rest of the stuff
        for (int depth = 1; depth < 11; depth++) {

            while (q1.Count > 0) {
                Room a = q1.Dequeue();
                MakeLootInRoom(a, depth);
                //				GameObject roomnumIndicator = a.SpawnItemAtCenter (Door, null);
                //				roomnumIndicator.GetComponent<Door> ().keyNum = a.roomID;
                a.reached = true;
                foreach (Room neighbor in a.neighbors) {
                    int w = DoorWeightFor(depth);
                    if (neighbor.squares.Count < 3) {
                        MakeBorderBetween(a, neighbor, false);
                    } else {
                        if (!neighbor.reached) {
                            q2.Enqueue(neighbor);
                            neighbor.reached = true;
                            neighbor.neighbors.Remove(a);
                            MakeBorderBetween(a, neighbor, true, w);
                        } else {
                            MakeBorderBetween(a, neighbor, false, w, doorChance: 0.15f);
                            neighbor.neighbors.Remove(a);
                        }
                    }
                }
            }
            q1 = new Queue<Room>(q2);
            q2.Clear();
        }

        //make corners
        for (int x = -1; x < width; x++) {
            for (int y = -1; y < width; y++) {
                if (ThereShouldBeABottomRightCornerAt(x, y)) {
                    PlaceBottomRightCornerAt(x, y);
                }
            }
        }

        //make walls at edge of floor
        for (int i = 0; i < width; i++) {
            PlaceRightWallAt(-1, i);
            PlaceRightWallAt(width - 1, i);
        }
        for (int i = 0; i < width; i++) {
            PlaceBottomWallAt(i, -1);
            PlaceBottomWallAt(i, width - 1);
        }
    }

    //private void PlaceDebugDoors() {
    //    foreach(Room r in rooms) {

    //    }
    //}

    int DoorBaseWeightFor(int depth) {
        Words w = GetComponent<Words>();
        int averageMaxDepth = 7;
        if (depth == 1) {
            return 2;
        }
        int deepestAmount = (int)(w.levelScore * w.humanKnowledgeFactor);//scale according to how good the level is
        float howDeepAmI = (float)(depth * depth) / (averageMaxDepth * averageMaxDepth);//scale quadratically with depth
        //float howDeepAmI = (float)(depth) / (averageMaxDepth);//scale linearly with depth
        return (int)(deepestAmount * howDeepAmI);
    }

    int DoorWeightFor(int depth) {
        int variance = Random.Range(-(depth - 1), depth + 1); //add a pinch of salt
                                                              //		print ("depth: " + depth +" fancy number: " + baseWeight);
        return variance + DoorBaseWeightFor(depth);
    }

    void MakeBorderBetween(Room a, Room b, bool door, int weight = 1, float doorChance = 0f) {
        //		int smallID = Mathf.Min (a.roomID, b.roomID);
        //		int bigID = Mathf.Max (a.roomID, b.roomID);
        //		print ("making border between " + smallID + " and " + bigID);
        //		print ("making border between " + a.roomID + " and " + b.roomID);
        //Find border
        List<Vector2Int> rightBorderSquares = new List<Vector2Int>();//squares immediately to the left of the border (so, if you placed a wall to the right it would be the border)
        List<Vector2Int> bottomBorderSquares = new List<Vector2Int>();//squares immediately above the border (so, if you placed a wall on the bottom it would be the border)
        foreach (Vector2Int square in a.squares) {
            Vector2Int toTheRight = new Vector2Int(square.x + 1, square.y);
            if (b.squares.Contains(toTheRight)) {
                rightBorderSquares.Add(square);
            }
            Vector2Int toTheBottom = new Vector2Int(square.x, square.y + 1);
            if (b.squares.Contains(toTheBottom)) {
                bottomBorderSquares.Add(square);
            }
        }
        foreach (Vector2Int square in b.squares) {
            Vector2Int toTheRight = new Vector2Int(square.x + 1, square.y);
            if (a.squares.Contains(toTheRight)) {
                rightBorderSquares.Add(square);
            }
            Vector2Int toTheBottom = new Vector2Int(square.x, square.y + 1);
            if (a.squares.Contains(toTheBottom)) {
                bottomBorderSquares.Add(square);
            }
        }

        if (!door) {
            //Put walls everywhere on the border
            foreach (Vector2Int square in rightBorderSquares) {
                PlaceRightWallAndMaybeDoorAt(square.x, square.y, doorChance, weight);
            }
            foreach (Vector2Int square in bottomBorderSquares) {
                PlaceBottomWallAndMaybeDoorAt(square.x, square.y, doorChance, weight);
            }
            return;
        }
        //Pick a random square on the border, make a door there
        int randomIndex = Random.Range(0, rightBorderSquares.Count + bottomBorderSquares.Count);
        Vector2Int selectedBorder = new Vector2Int();
        if (randomIndex >= rightBorderSquares.Count) {
            randomIndex -= rightBorderSquares.Count;
            selectedBorder = bottomBorderSquares[randomIndex];
            PlaceBottomDoorAt(selectedBorder.x, selectedBorder.y, weight);
            bottomBorderSquares.RemoveAt(randomIndex);
        } else {
            selectedBorder = rightBorderSquares[randomIndex];
            PlaceRightDoorAt(selectedBorder.x, selectedBorder.y, weight);
            rightBorderSquares.RemoveAt(randomIndex);
        }
        //Put walls everywhere else on the border
        foreach (Vector2Int square in rightBorderSquares) {
            PlaceRightWallAndMaybeDoorAt(square.x, square.y, doorChance, weight);
        }
        foreach (Vector2Int square in bottomBorderSquares) {
            PlaceBottomWallAndMaybeDoorAt(square.x, square.y, doorChance, weight);
        }
    }
    void PlaceDebugDoors() {
        foreach (Room r in roomGraph.Values) {
            GameObject debugDoor = r.SpawnItemAtCenter(Door);
            debugDoor.GetComponent<Door>().keyNum = r.roomID;
        }
    }

    void PlaceFogOfWar() {
        // print("placing fog of war objects");
        for (int x = -3; x < width + 3; x++) {
            for (int y = -3; y < width + 3; y++) {
                PlaceFogOfWarAt(x, y);
            }
        }
    }

    bool ThereShouldBeARightWallAt(int x, int y) {
        if (!InBounds(x, y) && !InBounds(x + 1, y)) {
            return false;
        }
        if (!InBounds(x, y)) {
            return true;
        }
        if (!InBounds(x + 1, y)) {
            return true;
        }
        if (rooms[x, y] != rooms[x + 1, y]) {
            return true;
        }
        return false;
    }
    bool ThereShouldBeABottomWallAt(int x, int y) {
        if (!InBounds(x, y) && !InBounds(x, y + 1)) {
            return false;
        }
        if (!InBounds(x, y)) {
            return true;
        }
        if (!InBounds(x, y + 1)) {
            return true;
        }
        if (rooms[x, y] != rooms[x, y + 1]) {
            return true;
        }
        return false;
    }
    bool ThereShouldBeABottomRightCornerAt(int x, int y) {
        if (!InBounds(x, y)) {
            return true;
        }
        if (!InBounds(x + 1, y + 1)) {
            return true;
        }
        if (rooms[x, y] != rooms[x + 1, y] || rooms[x, y] != rooms[x, y + 1] || rooms[x, y] != rooms[x + 1, y + 1]) {
            return true;
        }
        return false;
    }

    public Vector3 GetCellPositionFor(int x, int y) {
        return new Vector3(basePosition.x + x * cellSize, basePosition.y - y * cellSize, basePosition.z);
    }
    GameObject PlaceRightDoorAt(int x, int y, int keyNum) {
        Instantiate(WallSmall, GetCellPositionFor(x, y) + new Vector3(cellSize * 0.5f, cellSize * smallWallOffset, 0f), vertical, WallContainer.transform);
        GameObject newDoor = Instantiate(Door, GetCellPositionFor(x, y) + new Vector3(cellSize * 0.5f, 0f, 0f), vertical, DoorContainer.transform);
        Instantiate(WallSmall, GetCellPositionFor(x, y) + new Vector3(cellSize * 0.5f, -cellSize * smallWallOffset, 0f), vertical, WallContainer.transform);
        newDoor.GetComponent<Door>().keyNum = keyNum;
        return newDoor;
    }
    GameObject PlaceBottomDoorAt(int x, int y, int keyNum) {
        Instantiate(WallSmall, GetCellPositionFor(x, y) + new Vector3(cellSize * smallWallOffset, -cellSize * 0.5f, 0f), Quaternion.identity, WallContainer.transform);
        GameObject newDoor = Instantiate(Door, GetCellPositionFor(x, y) + new Vector3(0f, -cellSize * 0.5f, 0f), Quaternion.identity, DoorContainer.transform);
        Instantiate(WallSmall, GetCellPositionFor(x, y) + new Vector3(-cellSize * smallWallOffset, -cellSize * 0.5f, 0f), Quaternion.identity, WallContainer.transform);
        newDoor.GetComponent<Door>().keyNum = keyNum;
        return newDoor;
    }
    GameObject PlaceRightWallAt(int x, int y) {
        return Instantiate(Wall, GetCellPositionFor(x, y) + new Vector3(cellSize * 0.5f, 0f, 0f), vertical, WallContainer.transform);
    }
    GameObject PlaceBottomWallAt(int x, int y) {
        return Instantiate(Wall, GetCellPositionFor(x, y) + new Vector3(0f, -cellSize * 0.5f, 0f), Quaternion.identity, WallContainer.transform);
    }
    GameObject PlaceRightWallAndMaybeDoorAt(int x, int y, float doorChance, int weight) {
        float randy = Random.value;
        if (randy <= doorChance) {
            return PlaceRightDoorAt(x, y, weight);
        }
        return PlaceRightWallAt(x, y);
    }
    GameObject PlaceBottomWallAndMaybeDoorAt(int x, int y, float doorChance, int weight) {
        float randy = Random.value;
        if (randy <= doorChance) {
            return PlaceBottomDoorAt(x, y, weight);
        }
        return PlaceBottomWallAt(x, y);
    }
    void PlaceBottomRightCornerAt(int x, int y) {
        Instantiate(Corner, GetCellPositionFor(x, y) + new Vector3(cellSize * 0.5f, -cellSize * 0.5f, 0f), Quaternion.identity, WallContainer.transform);
    }

    void PlaceFogOfWarAt(int x, int y) {
        if (!InBounds(x, y)) {
            Instantiate(Void, GetCellPositionFor(x, y), Quaternion.identity, FogOfWarContainer.transform);
            return;
        }
        GameObject newFog = Instantiate(Void, GetCellPositionFor(x, y), Quaternion.identity, FogOfWarContainer.transform);
        newFog.GetComponent<FogOfWar>().room = rooms[x, y];
        //GameObject debugDoor = Instantiate(Door, newFog.transform.position, Quaternion.identity, newFog.transform);
        //debugDoor.GetComponent<Door>().keyNum = rooms[x, y];
        FogOfWarArray[x, y] = newFog;
        if (!ThereShouldBeARightWallAt(x - 1, y)) {
            newFog.GetComponent<FogOfWar>().neighbors.Add(FogOfWarArray[x - 1, y]);
            FogOfWarArray[x - 1, y].GetComponent<FogOfWar>().neighbors.Add(newFog);
        }
        if (!ThereShouldBeABottomWallAt(x, y - 1)) {
            newFog.GetComponent<FogOfWar>().neighbors.Add(FogOfWarArray[x, y - 1]);
            FogOfWarArray[x, y - 1].GetComponent<FogOfWar>().neighbors.Add(newFog);
        }
    }
    #endregion

    #region loot
    //will be called on each room in the dungeon during GenerateWallsAndLoot
    void MakeLootInRoom(Room r, int depth) {
        //print("room " + r.roomID + " depth: " + depth);
        Words w = GetComponent<Words>();
        // room is the place to spawn, depth is the difficulty
        List<GameObject> thingsToSpawn = new List<GameObject>();
        List<GameObject> itemPool = new List<GameObject>(loot);
        if (depth >= 4) {
            itemPool = DeepLoot;
        }

        // figure out what to spawn in r
        if (depth == 1) {
            PlaceGoodTilesInRoom(r, 8);
        }
        if (depth == 2) {
            PlaceTilesInRoom(r, 2);
        }
        if (depth > 1 && r.squares.Count > 3 && r.roomID != -5) {//do not make loot in starting rooms or really small rooms or the central chamber
            float diceRoll = Random.value;
            if (diceRoll < 0.4f) {//spawn loot 40% of the time (for now, this is too often for real gameplay)
                thingsToSpawn.Add(itemPool[Random.Range(0, itemPool.Count)]);
            }
            if (r.squares.Count > 30) {//definitely spawn loot in big enough room
                thingsToSpawn.Add(itemPool[Random.Range(0, itemPool.Count)]);
            }
        }
        if (r.squares.Count > 30) {//spawn grid in center of big enough room
            if (depth >= 4) { //do not spawn new grids too close to starting rooms
                if (r.roomID != -5) {//do not spawn in central chamber
                    r.SpawnItemAtCenter(Grid, GridContainer.transform);
                }
            }
        }

        // spawn stuff
        foreach (GameObject g in thingsToSpawn) {
            GameObject objSpawned = r.SpawnItem(g, LootContainer.transform);
            Game.RepositionHeight(objSpawned, Height.OnFloor);
            if (objSpawned.name.Contains("ExoticTile")) {
                objSpawned.GetComponent<LetterTile>().SetLetter(w.GetRandomNonSourceChar());
                objSpawned.GetComponent<LetterTile>().SetLifespan(Random.Range(10, 16));
            } else if (objSpawned.name.Contains("InfiniteTile")) {
                objSpawned.GetComponent<LetterTile>().SetLetter(w.GetRandomVowel());
                objSpawned.GetComponent<LetterTile>().SetLifespan(16);
            }
        }
    }

    void MakeLetterTiles() {
        // print("making some sweet loot");
        Words w = GetComponent<Words>();
        for (int i = 0; i < (width * width) / 16; i++) {
            GameObject newTile = Instantiate(Tile, Random.insideUnitCircle * cellSize * width / 2, Quaternion.Euler(0, 0, Random.Range(-30f, 30f)), TileContainer.transform);
            newTile.GetComponent<LetterTile>().SetLetter(w.GetRandomSourceChar());
            newTile.GetComponent<LetterTile>().SetLifespan(Random.Range(3, 9));
        }
    }

    void PlaceGoodTilesInRoom(Room r, int numTiles) {
        Words w = GetComponent<Words>();
        string startingTileSet = "";
        do {
            startingTileSet = "";
            for (int i = 0; i < numTiles; i++) {
                startingTileSet += w.GetRandomSourceChar();
            }
        } while (w.GetScoreExact(startingTileSet) < DoorBaseWeightFor(3));
        char[] startingTiles = startingTileSet.ToCharArray();
        for (int i = 0; i < numTiles; i++) {
            GameObject newTile = r.SpawnItem(Tile, Quaternion.Euler(0, 0, Random.Range(-30f, 30f)), TileContainer.transform);
            newTile.GetComponent<LetterTile>().SetLetter(startingTiles[i]);
            newTile.GetComponent<LetterTile>().SetLifespan(Random.Range(3, 9));
        }
    }

    void PlaceTilesInRoom(Room r, int numTiles) {
        Words w = GetComponent<Words>();
        string startingTileSet = "";
        for (int i = 0; i < numTiles; i++) {
            startingTileSet += w.GetRandomSourceChar();
        }
        char[] startingTiles = startingTileSet.ToCharArray();
        for (int i = 0; i < numTiles; i++) {
            GameObject newTile = r.SpawnItem(Tile, Quaternion.Euler(0, 0, Random.Range(-30f, 30f)), TileContainer.transform);
            newTile.GetComponent<LetterTile>().SetLetter(startingTiles[i]);
            newTile.GetComponent<LetterTile>().SetLifespan(Random.Range(3, 9));
        }
    }

    #endregion
}
