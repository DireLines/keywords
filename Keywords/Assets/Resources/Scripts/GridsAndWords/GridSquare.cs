﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : MonoBehaviour {
    public GameObject tile;
    //	public char letter;
    public Color highlightedColor;
    [HideInInspector]
    public Color normalColor;
    private SpriteRenderer sr;
    public int x;
    public int y;
    private int playersOnMe;
    [HideInInspector]
    public bool recolored;

    private Animator anim;
    void Awake() {
        tile = null;
        sr = GetComponent<SpriteRenderer>();
        normalColor = sr.color;
        anim = GetComponent<Animator>();
    }
    //TODO: make it rigorously impossible for there to be no active grid square if the player is within the confines of a grid

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name.Contains("Player") && other.isTrigger) {
            playersOnMe++;
            sr.color = highlightedColor;
            other.transform.gameObject.GetComponent<PlayerController>().SetActiveSquare(gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name.Contains("Player") && other.isTrigger) {
            playersOnMe--;
            if (playersOnMe <= 0) {
                playersOnMe = 0;
                sr.color = normalColor;
            }
            GameObject activeSquare = other.transform.gameObject.GetComponent<PlayerController>().activeSquare;
            if (activeSquare == gameObject) {
                other.transform.gameObject.GetComponent<PlayerController>().SetActiveSquare(null);
            }
        }
    }

    public char GetLetter() {
        if (tile == null || !tile.GetComponent<LetterTile>()) {
            return transform.parent.gameObject.GetComponent<GridControl>().placeholder;
        }
        return tile.GetComponent<LetterTile>().letter;
    }

    public void SetTile(GameObject newTile) {
        if (newTile != null && !newTile.GetComponent<Placeable>()) {
            print("tried to set tile to something not placeable");
        }
        tile = newTile;
    }

    public void Swell() {
        anim.SetTrigger("Swell");
    }

    public void SetColor(Color color) {
        float d = 0.7f;
        Color darkerColor = new Color(color.r * d, color.g * d, color.b * d, 1f);
        GetComponent<SpriteRenderer>().color = darkerColor;
        normalColor = darkerColor;
        highlightedColor = color;
    }
}
