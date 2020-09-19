using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Flag : Placeable {
    public int ownerNum;
    private SpriteRenderer flagSprite;

    // Use this for initialization
    void Awake() {
        ownerNum = 0;
        flagSprite = transform.Find("FlagSprite").GetComponent<SpriteRenderer>();
    }

    // when the flag is picked up
    public void PickFlag(int newOwnerNum, GameObject owner) {
        // set the ownership of the flag to the team who picked it up
        ownerNum = newOwnerNum;
        flagSprite.color = owner.GetComponent<PlayerInfo>().GetTeamColor();
    }

    public override void PlaceOn(GameObject square, GameObject placingPlayer) {
        base.PlaceOn(square, placingPlayer);
        GridControl gc = square.transform.parent.gameObject.GetComponent<GridControl>();
        if (gc && gc.claimable) {
            square.transform.parent.gameObject.GetComponent<GridControl>().SetOwnership(ownerNum);
            Color ownerColor = placingPlayer.GetComponent<PlayerInfo>().GetTeamColor();
            float d = 0.7f;
            Color darkerColor = new Color(ownerColor.r * d, ownerColor.g * d, ownerColor.b * d, 1f);
            square.transform.parent.gameObject.GetComponent<GridControl>().StartRecoloring(ownerColor, darkerColor, square);
            Destroy(gameObject);
        }
    }
}
