﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public static Words words;
	public static MakeWalls makeWalls;
	public static Quit quit;

	//public int playerCount = 4;
	public GameObject[] players;
	public WordOverlayHandler[] wordOverlayHandlers;
	public Camera[] cameras;

	private void Awake () {
		if (instance) {
			Destroy(gameObject);
		} else {
			instance = this;
		}

		words = GetComponent<Words>();
		makeWalls = GetComponent<MakeWalls>();
		quit = GetComponent<Quit>();

		if (players.Length == 0) {
			players = new GameObject[4];
			players[0] = GameObject.Find("Player1");
			players[1] = GameObject.Find("Player2");
			players[2] = GameObject.Find("Player3");
			players[3] = GameObject.Find("Player4");
		}

		if (wordOverlayHandlers.Length == 0) {
			wordOverlayHandlers = new WordOverlayHandler[4];
			wordOverlayHandlers[0] = GameObject.Find("PlayerUI1").transform.Find("WordList").GetComponent<WordOverlayHandler>();
			wordOverlayHandlers[1] = GameObject.Find("PlayerUI2").transform.Find("WordList").GetComponent<WordOverlayHandler>();
			wordOverlayHandlers[2] = GameObject.Find("PlayerUI3").transform.Find("WordList").GetComponent<WordOverlayHandler>();
			wordOverlayHandlers[3] = GameObject.Find("PlayerUI4").transform.Find("WordList").GetComponent<WordOverlayHandler>();
		}

		if (cameras.Length == 0) {
			cameras = new Camera[4];
			cameras[0] = GameObject.Find("Camera1").GetComponent<Camera>();
			cameras[1] = GameObject.Find("Camera2").GetComponent<Camera>();
			cameras[2] = GameObject.Find("Camera3").GetComponent<Camera>();
			cameras[3] = GameObject.Find("Camera4").GetComponent<Camera>();
		}
	}

	public static GameObject[] GetPlayers () {
		return instance.players;
	}

	public static WordOverlayHandler[] GetWordOverlayHandlers () {
		return instance.wordOverlayHandlers;
	}

	public static Camera[] GetCameras () {
		return instance.cameras;
	}

	public static GameObject GetPlayer (int playerNum) {
		return instance.players[playerNum-1];
	}

	public static WordOverlayHandler GetWordOverlayHandler (int playerNum) {
		return instance.wordOverlayHandlers[playerNum-1];
	}

	public static Camera GetCamera (int playerNum) {
		return instance.cameras[playerNum-1];
	}
}