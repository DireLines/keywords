﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour {

    public int room;

    public List<GameObject> neighbors;//Fog of War objects adjacent to this one and in the same room

    [HideInInspector]
    public Color floorColor;
    public float floorTintAlpha;

    void Start() {
        gameObject.layer = 8; // "wall" layer; used to generalize detecting if it is traversable by players
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            floorColor = Game.RandomDarkColor();
            floorColor.a = floorTintAlpha;
            if (room < 0) { //starting room
                floorColor.a = 0f;
            }
            HideMyself();
        }
    }

    void HideMyselfSoon() {
        Invoke("HideMyself", 0.05f);
    }

    void HideMyself() {
        foreach (GameObject neighbor in neighbors) {
            if (neighbor.GetComponent<FogOfWar>().neighbors.Contains(gameObject)) {
                neighbor.GetComponent<FogOfWar>().neighbors.Remove(gameObject);
            }
            neighbor.GetComponent<FogOfWar>().floorColor = floorColor;
            neighbor.GetComponent<FogOfWar>().HideMyselfSoon();
        }
        Game.RepositionHeight(gameObject, Height.Background);
        //gameObject.GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<SpriteRenderer>().color = floorColor;
        GetComponent<BoxCollider2D>().enabled = false;//prevent repeat triggering
    }
}
