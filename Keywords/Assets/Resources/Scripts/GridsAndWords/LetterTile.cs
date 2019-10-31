﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterTile : MonoBehaviour {
    public char letter;//what is my letter?
    public int lifespan;//how many more words can you make with me?
    private GameObject letterSprite;
    private GameObject numberSprite;

    private Words words;


    Animator anim;

    void Awake() {
        anim = GetComponent<Animator>();
        words = GameManager.words;
        letterSprite = transform.FindDeepChild("LetterSprite").gameObject;
        numberSprite = transform.FindDeepChild("NumberSprite").gameObject;
        //		SetLetter (words.GetRandomSourceChar ());
        //		SetLetter ((char)Random.Range (97, 123));
        //		SetMatches (Random.Range(3,9));
        //		Dec ();

    }
    public void SetLetter(char newletter) {
        if ((int)newletter < 97 || (int)newletter > 122) {//not in a-z
            print("tried to set letter tile to weird char");
            return;
        }
        letter = newletter;
        string spriteName = "LetterSprites/" + letter.ToString();
        letterSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
    }

    public void ChangeLetterSprite(char c)
    {
        string spriteName = "LetterSprites/" + c.ToString();
        letterSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
    }

    public void SetLifespan(int newLifespan) {
        if (newLifespan < 0) {//out of range
            print("tried to set letter tile to weird number");
            return;
        }
        if (newLifespan == 0) {
            //			print ("tile dead");
            GridSquare gs = transform.parent.gameObject.GetComponent<GridSquare>();
            if (gs != null) {
                gs.SetTile(null);
            }
            Destroy(gameObject);
            return;
        }
        lifespan = newLifespan;
        string spriteName = "NumberSprites/" + lifespan.ToString();
        if (lifespan >= 16) {
            lifespan = 16;
            spriteName = "NumberSprites/inf";
        }
        numberSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
    }

    //decrement
    public void DecLifespan() {
        //TODO: call animation to flash this letter tile
        this.animate();

        if (lifespan != 16) {
            SetLifespan(lifespan - 1);
        }
    }

    public void animate() {
        anim.SetTrigger("Flash");
    }
}