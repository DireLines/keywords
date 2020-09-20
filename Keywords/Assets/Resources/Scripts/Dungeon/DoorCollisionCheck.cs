using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCollisionCheck : MonoBehaviour {

    private Transform doors;//door container object

    void Start() {
        doors = transform;
    }
    public void SetDoorCollisions(GameObject playerObj, int numKeys) {
        PlayerInfo player = playerObj.GetComponent<PlayerInfo>();
        int playerNum = player.playerNum;
        foreach (Transform child in doors) {
            Door door = child.gameObject.GetComponent<Door>();
            if (door.CheckLocked(playerNum) && numKeys >= door.keyNum) {
                door.Unlock(playerNum);
                Physics2D.IgnoreCollision(player.gameObject.GetComponent<CircleCollider2D>(), door.GetComponent<BoxCollider2D>());

                // Create indicator that goes towards the door when it unlocks. If it's close or it's mid/late game
                if (numKeys > 13 || (door.transform.position - player.transform.position).magnitude < 15f) {
                    GameObject adoorable = Instantiate(Resources.Load("Prefabs/FX/GlowingOrbFX"), player.transform.position, Quaternion.identity) as GameObject;
                    adoorable.GetComponent<GoToDoor>().GoTo(door.transform);
                    string layerName = "P" + playerNum.ToString();
                    Game.SetLayer(adoorable, LayerMask.NameToLayer(layerName));
                }
            }
        }
    }
}
