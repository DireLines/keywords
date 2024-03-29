﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public static Words words;
    public static DungeonGenerator makeWalls;
    public static Quit quit;

    //public int playerCount = 4;
    public GameObject[] players;
    public TextOverlayHandler[] textOverlayHandlers;
    public Camera[] cameras;
    public PauseMenu pauseMenu;

    public Team[] teams;
    private Team dummyTeam;

    public Dictionary<string, AudioSource> sfx;

    private DoorCollisionCheck DCC;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
        } else {
            instance = this;
        }

        words = GetComponent<Words>();
        makeWalls = GetComponent<DungeonGenerator>();
        quit = GetComponent<Quit>();

        if (players.Length == 0) {
            players = new GameObject[4];
            players[0] = GameObject.Find("Player1");
            players[1] = GameObject.Find("Player2");
            players[2] = GameObject.Find("Player3");
            players[3] = GameObject.Find("Player4");
        }

        if (textOverlayHandlers.Length == 0) {
            textOverlayHandlers = new TextOverlayHandler[4];
            textOverlayHandlers[0] = GameObject.Find("PlayerUI1").transform.Find("WordList").GetComponent<TextOverlayHandler>();
            textOverlayHandlers[1] = GameObject.Find("PlayerUI2").transform.Find("WordList").GetComponent<TextOverlayHandler>();
            textOverlayHandlers[2] = GameObject.Find("PlayerUI3").transform.Find("WordList").GetComponent<TextOverlayHandler>();
            textOverlayHandlers[3] = GameObject.Find("PlayerUI4").transform.Find("WordList").GetComponent<TextOverlayHandler>();
        }

        if (cameras.Length == 0) {
            cameras = new Camera[4];
            cameras[0] = GameObject.Find("Camera1").GetComponent<Camera>();
            cameras[1] = GameObject.Find("Camera2").GetComponent<Camera>();
            cameras[2] = GameObject.Find("Camera3").GetComponent<Camera>();
            cameras[3] = GameObject.Find("Camera4").GetComponent<Camera>();
        }
        SceneManager.sceneLoaded += FindPauseMenu;
        sfx = new Dictionary<string, AudioSource>();
        GameObject sfxContainer = GameObject.Find("SFX");
        foreach (Transform child in sfxContainer.transform) {
            if (child.gameObject.GetComponent<AudioSource>()) {
                sfx.Add(child.gameObject.name, child.gameObject.GetComponent<AudioSource>());
            }
        }
    }

    private void Start() {
        DCC = GameObject.Find("Doors").GetComponent<DoorCollisionCheck>();
    }
    public static GameObject[] GetPlayers() {
        return instance.players;
    }

    public static TextOverlayHandler[] GetWordOverlayHandlers() {
        return instance.textOverlayHandlers;
    }

    public static Camera[] GetCameras() {
        return instance.cameras;
    }

    public static GameObject GetPlayer(int playerNum) {
        return instance.players[playerNum - 1];
    }

    public static TextOverlayHandler GetTextOverlayHandler(int playerNum) {
        return instance.textOverlayHandlers[playerNum - 1];
    }

    public static Camera GetCamera(int playerNum) {
        return instance.cameras[playerNum - 1];
    }

    public static Team[] GetTeams() {
        return instance.teams;
    }

    public static Team teamByID(int id) {
        foreach (Team team in instance.teams) {
            if (team.id == id) {
                return team;
            }
        }
        print("you passed in a team id that no team has");
        return instance.dummyTeam;
    }

    public static List<GameObject> playersInTeam(Team team) {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject player in instance.players) {
            if (player.GetComponent<PlayerInfo>().teamNum == team.id) {
                result.Add(player);
            }
        }
        return result;
    }

    public static void addScoreToEveryone(int amount = 1) {
        foreach (Team team in instance.teams) {
            addScore(team.id, amount);
        }
    }

    public static void addScore(int teamID, int amount = 1) {
        Team team = teamByID(teamID);
        team.score += amount;
        foreach (GameObject player in playersInTeam(team)) {
            player.GetComponent<PlayerInfo>().SetScoreUI(team.score);
            instance.DCC.SetDoorCollisions(player, team.score);
        }
    }

    private void FindPauseMenu(Scene scene, LoadSceneMode mode) {
        if (!pauseMenu) {
            pauseMenu = GameObject.Find("Menus").GetComponent<PauseMenu>();
        }
    }
}
