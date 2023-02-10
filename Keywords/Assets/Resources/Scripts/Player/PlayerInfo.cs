using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour {

    public int playerNum;
    public int teamNum;
    public GameObject UI;//this player's UI;
    private TMPro.TextMeshProUGUI scoreUI;//UI which displays how many keys the player has

    private void Start() {
        scoreUI = UI.transform.Find("Keys").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetScoreUI(int score) {
        scoreUI.text = score.ToString();
    }

    public KeyCode GetKeyCode(string controlName) {
        if (Game.IsOnOSX) {
            return GetKeyCodeOSX(controlName);
        }
        Dictionary<string, KeyCode[]> controlSets = new Dictionary<string, KeyCode[]> {
            ["A"] = new KeyCode[4] {
                KeyCode.Joystick1Button0,
                KeyCode.Joystick2Button0,
                KeyCode.Joystick3Button0,
                KeyCode.Joystick4Button0
            },
            ["B"] = new KeyCode[4] {
                KeyCode.Joystick1Button1,
                KeyCode.Joystick2Button1,
                KeyCode.Joystick3Button1,
                KeyCode.Joystick4Button1
            },
            ["X"] = new KeyCode[4] {
                KeyCode.Joystick1Button2,
                KeyCode.Joystick2Button2,
                KeyCode.Joystick3Button2,
                KeyCode.Joystick4Button2
            },
            ["Y"] = new KeyCode[4] {
                KeyCode.Joystick1Button3,
                KeyCode.Joystick2Button3,
                KeyCode.Joystick3Button3,
                KeyCode.Joystick4Button3
            },
            ["LeftBumper"] = new KeyCode[4] {
                KeyCode.Joystick1Button4,
                KeyCode.Joystick2Button4,
                KeyCode.Joystick3Button4,
                KeyCode.Joystick4Button4
            },
            ["RightBumper"] = new KeyCode[4] {
                KeyCode.Joystick1Button5,
                KeyCode.Joystick2Button5,
                KeyCode.Joystick3Button5,
                KeyCode.Joystick4Button5
            },
            ["Start"] = new KeyCode[4] {
                KeyCode.Joystick1Button7,
                KeyCode.Joystick2Button7,
                KeyCode.Joystick3Button7,
                KeyCode.Joystick4Button7
            },
        };
        if (!controlSets.ContainsKey(controlName)) {
            print($"control name {controlName} not recognized");
        }
        return controlSets[controlName][playerNum - 1];
    }

    public KeyCode GetKeyCodeOSX(string controlName) {
        KeyCode[] controlSet = new KeyCode[4];
        Dictionary<string, KeyCode[]> controlSets = new Dictionary<string, KeyCode[]> {
            ["A"] = new KeyCode[4] {
                KeyCode.Joystick1Button0,
                KeyCode.Joystick2Button0,
                KeyCode.Joystick3Button0,
                KeyCode.Joystick4Button0
            },
            ["B"] = new KeyCode[4] {
                KeyCode.Joystick1Button1,
                KeyCode.Joystick2Button1,
                KeyCode.Joystick3Button1,
                KeyCode.Joystick4Button1
            },
            ["Y"] = new KeyCode[4] {
                KeyCode.Joystick1Button3,
                KeyCode.Joystick2Button3,
                KeyCode.Joystick3Button3,
                KeyCode.Joystick4Button3
            },
            ["X"] = new KeyCode[4] {
                KeyCode.Joystick1Button2,
                KeyCode.Joystick2Button2,
                KeyCode.Joystick3Button2,
                KeyCode.Joystick4Button2
            },
            ["LeftBumper"] = new KeyCode[4] {
                KeyCode.Joystick1Button6,
                KeyCode.Joystick2Button6,
                KeyCode.Joystick3Button6,
                KeyCode.Joystick4Button6
            },
            ["RightBumper"] = new KeyCode[4] {
                KeyCode.Joystick1Button7,
                KeyCode.Joystick2Button7,
                KeyCode.Joystick3Button7,
                KeyCode.Joystick4Button7
            },
            ["Start"] = new KeyCode[4] {
                KeyCode.Joystick1Button11,
                KeyCode.Joystick2Button11,
                KeyCode.Joystick3Button11,
                KeyCode.Joystick4Button11
            },
        };
        if (!controlSets.ContainsKey(controlName)) {
            print($"control name {controlName} not recognized");
        }
        return controlSets[controlName][playerNum - 1];
    }

    public float GetAxisWindows(string axisName) {
        if (playerNum < 1 || playerNum > 4) {
            print("playerNum not a valid number fix it");
            return 0f;
        }
        if (axisName == "Horizontal") {
            return Input.GetAxis("P" + playerNum + "_Horizontal");
        } else if (axisName == "Vertical") {
            return Input.GetAxis("P" + playerNum + "_Vertical");
        } else if (axisName == "Horizontal_R") {
            return Input.GetAxis("P" + playerNum + "_Horizontal_R");
        } else if (axisName == "Vertical_R") {
            return Input.GetAxis("P" + playerNum + "_Vertical_R");
        } else if (axisName == "RTrigger") {
            return Input.GetAxis("P" + playerNum + "_RTrigger_Windows");
        } else if (axisName == "LTrigger") {
            return Input.GetAxis("P" + playerNum + "_LTrigger_Windows");
        } else {
            print("axis name not recognized");
            return 0f;
        }
    }

    public float GetAxisOSX(string axisName) {
        if (playerNum < 1 || playerNum > 4) {
            print("playerNum not a valid number fix it");
            return 0f;
        }
        if (axisName == "Horizontal") {
            return Input.GetAxis("P" + playerNum + "_Horizontal");
        } else if (axisName == "Vertical") {
            return Input.GetAxis("P" + playerNum + "_Vertical");
        } else if (axisName == "Horizontal_R") {
            return Input.GetAxis("P" + playerNum + "_Horizontal_R_OSX");
        } else if (axisName == "Vertical_R") {
            return Input.GetAxis("P" + playerNum + "_Vertical_R_OSX");
        } else if (axisName == "RTrigger") {
            return Input.GetAxis("P" + playerNum + "_RTrigger");
        } else if (axisName == "LTrigger") {
            return Input.GetAxis("P" + playerNum + "_LTrigger_OSX");
        } else {
            print("axis name not recognized");
            return 0f;
        }
    }

    public float GetAxisLinux(string axisName) {
        if (playerNum < 1 || playerNum > 4) {
            print("playerNum not a valid number fix it");
            return 0f;
        }
        if (axisName == "Horizontal") {
            return Input.GetAxis("P" + playerNum + "_Horizontal");
        } else if (axisName == "Vertical") {
            return Input.GetAxis("P" + playerNum + "_Vertical");
        } else if (axisName == "Horizontal_R") {
            return Input.GetAxis("P" + playerNum + "_Horizontal_R");
        } else if (axisName == "Vertical_R") {
            return Input.GetAxis("P" + playerNum + "_Vertical_R");
        } else if (axisName == "RTrigger") {
            return Input.GetAxis("P" + playerNum + "_RTrigger");
        } else if (axisName == "LTrigger") {
            return Input.GetAxis("P" + playerNum + "_LTrigger_Linux");
        } else {
            print("axis name not recognized");
            return 0f;
        }
    }

    public Color GetTeamColor() {
        return GameManager.teamByID(teamNum).color;
    }
}
