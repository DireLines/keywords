using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityStandardAssets._2D;

public delegate void CollisionEvent(Collision2D collision);

public class PlayerController : MonoBehaviour {
    #region fields
    private bool allInputDisabled;
    private Rigidbody2D rb;
    private Inventory inventory;

    public GameObject activeSquare;//the grid square the player's currently on
    private GameObject TileContainer; //the parent object for the letter tiles

    public Camera cam;
    private Camera2DFollow camScript;

    //controls
    private PlayerInfo me;
    private KeyCode LeftBumper;
    private KeyCode RightBumper;
    private KeyCode AButton;
    private KeyCode BButton;
    private KeyCode YButton;
    private KeyCode XButton;
    private KeyCode StartButton;

    public float speed;
    private float pMovHandleBase = 0.8f; // Player movmement "handling" when player is "slow" (within max speed)
    private float pMovHandleFast = 0.05f; // When moving fast, drag/handling
    private bool pMovDisable = false; // Disables basic movement mechanics entirely
    private float pMovCurrentSpeed;
    private Coroutine pMovSpeedResetCoroutine;
    private float pMovHandle; // Current value of movement handling: used to lerp velocity to input velocity (0 to 1)
    private Coroutine pMovHandleResetCoroutine;
    const float pickupRadius = 0.2f; //how far away can the player pick up an object?
    public Vector3 holdOffset; //what's the hold position of the currently held inventory item?
    const float epsilon = 0.001f;

    private int playerNum;
    private Color mycolor;
    private int keyboardControlledPlayer = 1; //for debug / testing without controllers - one player can be controlled by the keyboard at a time;

    //Idle variables
    public float timeUntilIdle = 3f;
    [HideInInspector]
    public bool idle;
    private bool idleLF;
    public float timeUntilLongIdle = 3f;
    [HideInInspector]
    public bool longIdle;
    private bool longIdleLF;
    GameObject stars;
    private float timeSinceLastMoved;

    public Vector2 lsInput;
    private bool ls_pressed;
    const float lsPressThreshold = 0.75f;
    private Func<string, float> GetAxis;

    private GameObject aimIndicator;
    private bool rt_pressed;
    private bool lt_pressed;
    const float triggerPressThreshold = 0.9f;
    const float triggerReleaseThreshold = 0.1f;

    private Fist fist;
    public CollisionEvent CollisionEvent;

    // Unique state variables
    public bool bonked = false;
    private Coroutine bonkedResetCoroutine;
    #endregion

    #region start
    // Use this for initialization
    void Start() {
        stars = transform.Find("Stars").gameObject;
        stars.SetActive(true);
        foreach (Transform star in stars.transform) {
            star.gameObject.GetComponent<Star>().Circle(transform.position);
        }
        stars.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        me = GetComponent<PlayerInfo>();
        camScript = cam.GetComponent<Camera2DFollow>();
        playerNum = me.playerNum;
        inventory = GetComponent<Inventory>();
        TileContainer = GameObject.Find("Tiles");
        mycolor = GetComponent<SpriteRenderer>().color;
        aimIndicator = transform.Find("AimIndicator").gameObject;
        aimIndicator.GetComponent<SpriteRenderer>().color = mycolor;
        SetControls();
        //Idle
        timeSinceLastMoved = 0f;
        idle = false;
        idleLF = false;

        // movement
        pMovCurrentSpeed = speed;
        pMovHandle = pMovHandleBase;

        // punching
        fist = transform.Find("Fist").GetComponent<Fist>();
        transform.FindDeepChild("FistSprite").GetComponent<SpriteRenderer>().color = mycolor;
    }
    private void SetControls() {
        AButton = me.GetKeyCode("A");
        BButton = me.GetKeyCode("B");
        YButton = me.GetKeyCode("Y");
        XButton = me.GetKeyCode("X");
        StartButton = me.GetKeyCode("Start");
        LeftBumper = me.GetKeyCode("LeftBumper");
        RightBumper = me.GetKeyCode("RightBumper");
        GetAxis = me.GetAxisWindows;
        if (Game.IsOnOSX) {
            GetAxis = me.GetAxisOSX;
        } else if (Game.IsOnLinux) {
            GetAxis = me.GetAxisLinux;
        }
    }
    #endregion

    #region update
    // Update is called once per frame
    void Update() {
        // Pause menu
        if (Input.GetKeyDown(StartButton)) {
            GameManager.instance.pauseMenu.Toggle();
        }
        if (Game.Paused()) {
            float lsVert = GetAxis("Vertical");
            if (!ls_pressed && Mathf.Abs(lsVert) > lsPressThreshold) {
                ls_pressed = true;
                if (lsVert > 0f) {

                } else {

                }
            }
            return;
        }

        if (allInputDisabled) {
            return;
        }

        //aiming and firing
        Vector2 aim_raw = new Vector2(0, 0);
        if (playerNum == keyboardControlledPlayer) {
            if (Input.GetKey(KeyCode.I)) {
                aim_raw.y += 1;
            }
            if (Input.GetKey(KeyCode.K)) {
                aim_raw.y -= 1;
            }
            if (Input.GetKey(KeyCode.J)) {
                aim_raw.x -= 1;
            }
            if (Input.GetKey(KeyCode.L)) {
                aim_raw.x += 1;
            }
        }
        if (aim_raw == new Vector2(0, 0)) {
            aim_raw = new Vector2(GetAxis("Horizontal_R"), GetAxis("Vertical_R"));
        }
        aim_raw = Vector2.ClampMagnitude(aim_raw, 1.0f);
        if (aim_raw.sqrMagnitude < epsilon) {
            aim_raw = Vector2.zero;
            aimIndicator.GetComponent<SpriteRenderer>().enabled = false;
        } else if (!inventory.Get() || inventory.Get().GetComponent<Fireable>()) {
            aimIndicator.GetComponent<SpriteRenderer>().enabled = true;
        }


        Vector2 aim = aim_raw.normalized;

        bool keyboardFire = false;
        if (playerNum == keyboardControlledPlayer) {
            keyboardFire = Input.GetKeyDown(KeyCode.Space);
        }
        float trigger = GetAxis("RTrigger");
        if ((!rt_pressed && trigger > triggerPressThreshold) || keyboardFire) {
            //fire weapon/tool if aiming, else switch inventory slots
            rt_pressed = true;
            if (aim.Equals(Vector2.zero)) {
                //inventory.IncSlot();
            } else {
                //print("activating held item");
                if (inventory.Get()) {
                    Fireable f = inventory.Get().GetComponent<Fireable>();
                    if (f) {
                        f.Fire(aim, gameObject);
                    }
                } else {
                    Punch(aim);
                }
            }
        }
        if (rt_pressed && trigger < triggerReleaseThreshold) {
            rt_pressed = false;
        }

        //debug
        aimIndicator.transform.position = (Vector2)transform.position + aim_raw;
        float trigger_percentage = (trigger + 1) / 2;
        Color trigger_color = (1 - trigger_percentage) * GetComponent<SpriteRenderer>().color;
        aimIndicator.GetComponent<SpriteRenderer>().color = new Color(trigger_color.r, trigger_color.g, trigger_color.b);

        float ltrigger = GetAxis("LTrigger");
        if (!lt_pressed && ltrigger > triggerPressThreshold) {
            //do not switch inventory slot
            lt_pressed = true;
            //inventory.DecSlot();
        }
        if (lt_pressed && ltrigger < triggerReleaseThreshold) {
            lt_pressed = false;
        }



        if (rb.velocity.sqrMagnitude > float.Epsilon * float.Epsilon) {
            timeSinceLastMoved = 0f;
            idle = false;
            longIdle = false;
        } else {
            if (timeSinceLastMoved > timeUntilIdle) {
                idle = true;
            }

            if (timeSinceLastMoved > timeUntilLongIdle) {
                longIdle = true;
            }

            timeSinceLastMoved += Time.deltaTime;
        }

        if (idle && !idleLF) {
            GameManager.GetTextOverlayHandler(playerNum).AppearWords();
        } else if (!idle && idleLF) {
            GameManager.GetTextOverlayHandler(playerNum).DisappearWords();
        }

        if (longIdle && !longIdleLF) {
            GameManager.GetTextOverlayHandler(playerNum).AppearDefinitions();
        } else if (!longIdle && longIdleLF) {
            GameManager.GetTextOverlayHandler(playerNum).DisappearDefinitions();
        }

        idleLF = idle;
        longIdleLF = longIdle;

        ////Interact with world
        if (Input.GetKeyDown(AButton) || (me.playerNum == keyboardControlledPlayer && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))) {
            Interact();
        } else if (Input.GetKeyDown(BButton) || (me.playerNum == keyboardControlledPlayer && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.R)))) {
            Drop();
        }

        ////Adjust camera height out
        if (Input.GetKeyDown(YButton) || (me.playerNum == keyboardControlledPlayer && Input.GetKeyDown(KeyCode.LeftShift))) {
            camScript.ToggleZoom(zoomIn: false);
        }

        ////Adjust camera height in
        if (Input.GetKeyDown(XButton) || (me.playerNum == keyboardControlledPlayer && Input.GetKeyDown(KeyCode.RightShift))) {
            camScript.ToggleZoom(zoomIn: true);
        }

        ////Change which item is active
        if (Input.GetKeyDown(LeftBumper) || (me.playerNum == keyboardControlledPlayer && Input.GetKeyDown(KeyCode.LeftArrow))) {
            inventory.DecSlot();
        } else if (Input.GetKeyDown(RightBumper) || (me.playerNum == keyboardControlledPlayer && Input.GetKeyDown(KeyCode.RightArrow))) {
            inventory.IncSlot();
        }

        //make keyboardControlledPlayer adjustable by keyboard
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            keyboardControlledPlayer = 1;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            keyboardControlledPlayer = 2;
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            keyboardControlledPlayer = 3;
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            keyboardControlledPlayer = 4;
        } else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            keyboardControlledPlayer = 0;
        }
    }

    // runs every physics calculation frame, used for movement
    void FixedUpdate() {
        float axisX, axisY;
        if (playerNum == keyboardControlledPlayer) {
            axisX = Input.GetAxisRaw("DebugHorizontal");
            axisY = Input.GetAxisRaw("DebugVertical");
        } else {
            axisX = GetAxis("Horizontal");
            axisY = GetAxis("Vertical");
        }
        //lsInput = new Vector2(axisX, axisY);
        //rb.velocity = pMovSpeed * lsInput; 
        HandleMovement(axisX, axisY);
        // if (Input.GetKeyDown(AButton) || (me.playerNum == keyboardControlledPlayer && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))) {
        //     DebugDash(axisX, axisY);
        // }
    }
    #endregion

    #region interact
    private void Interact() {
        bool hoveringOverGrid = activeSquare != null;
        bool holdingItem = inventory.Get() != null;
        bool inventoryFull = inventory.Full();
        bool itemHasAction = holdingItem ? inventory.Get().GetComponent<Activatable>() : false;
        bool holdingLetterTile = holdingItem ? inventory.Get().GetComponent<Placeable>() : false;
        bool squareContainsTile = hoveringOverGrid ? activeSquare.GetComponent<GridSquare>().tile != null : false;

        if (hoveringOverGrid) {
            if (holdingItem && holdingLetterTile) {
                if (squareContainsTile) {
                    SwapWithSquare();
                } else {
                    PlaceOnSquare();
                }
            } else if (!holdingItem && !holdingLetterTile && squareContainsTile) {
                TakeFromSquare();
            } else if (!inventoryFull) {
                NormalGrab();
            }
        } else if (holdingItem && itemHasAction) {
            ActivateItem();
        } else if (!inventoryFull) {
            NormalGrab();
        }
    }

    private void ActivateItem() {
        inventory.Get().GetComponent<Activatable>().Activate();
    }
    private void Drop() {
        GameObject itemToDrop = inventory.Get();
        if (itemToDrop != null) {
            itemToDrop.transform.SetParent(TileContainer.transform);
            Game.RepositionHeight(itemToDrop, Height.OnFloor);
            Game.EnablePhysics(itemToDrop);
            if (itemToDrop.GetComponent<Fireable>()) {
                itemToDrop.GetComponent<Fireable>().Drop();
            }
            inventory.Remove();
        }
    }
    private void PlaceOnSquare() {
        GameObject itemToPlace = inventory.Get();
        itemToPlace.transform.SetParent(activeSquare.transform);
        itemToPlace.transform.position = activeSquare.transform.position;
        Game.RepositionHeight(itemToPlace, Height.OnGridSquare);
        activeSquare.GetComponent<GridSquare>().SetTile(itemToPlace);
        itemToPlace.GetComponent<Placeable>().PlaceOn(activeSquare, gameObject);
        inventory.Remove();
        if (itemToPlace.GetComponent<Flag>()) {
            GridControl gc = activeSquare.transform.parent.gameObject.GetComponent<GridControl>();
            if (gc) {
                gc.SetOwnership(playerNum);
            }
        }
    }

    private void TakeFromSquare() {
        GameObject itemToTake = activeSquare.GetComponent<GridSquare>().tile;
        itemToTake.transform.SetParent(transform);
        inventory.Add(itemToTake);
        itemToTake.transform.localPosition = holdOffset;
        itemToTake.transform.rotation = Quaternion.identity;
        Game.RepositionHeight(itemToTake, Height.Held);
        activeSquare.GetComponent<GridSquare>().SetTile(null);
        itemToTake.GetComponent<Placeable>().TakeFrom(activeSquare, gameObject);
    }

    private void SwapWithSquare() {
        GameObject temp = activeSquare.GetComponent<GridSquare>().tile;
        temp.transform.SetParent(transform);
        temp.transform.localPosition = holdOffset;
        temp.transform.rotation = Quaternion.identity;
        Game.RepositionHeight(temp, Height.Held);
        PlaceOnSquare();
        inventory.Add(temp);
    }

    private void NormalGrab() {
        //pick up nearest item within pickup radius
        GameObject closestObject = Game.ClosestItemInRadius(transform.position, pickupRadius);
        if (closestObject == null) {
            return;
        }
        // pick up flag
        if (closestObject.GetComponent<Flag>()) {
            closestObject.GetComponent<Flag>().PickFlag(playerNum, gameObject);
        }
        if (closestObject.GetComponent<Fireable>()) {
            closestObject.GetComponent<Fireable>().PickUp(gameObject);
        }
        //put item in inventory
        closestObject.transform.SetParent(transform);
        inventory.Add(closestObject);
        closestObject.transform.localPosition = holdOffset;
        closestObject.transform.rotation = Quaternion.identity;
        Game.RepositionHeight(closestObject, Height.Held);
        Game.DisablePhysics(closestObject);
    }

    public void SetActiveSquare(GameObject newSquare) {
        activeSquare = newSquare;
    }
    #endregion

    #region handlemovement
    private void HandleMovement(float GetAxisX, float GetAxisY) {
        // Store movement vector.

        if (pMovDisable) return;

        Vector2 move = Vector2.ClampMagnitude(new Vector2(GetAxisX, GetAxisY), 1) * pMovCurrentSpeed;
        float handling = pMovHandle;
        // When above player max speed, we let reduce control so that momentum is preserved
        if (rb.velocity.magnitude > pMovCurrentSpeed) {
            handling = pMovHandleFast;
        } else {
            // can't reverse direction ezpz
            if (Vector2.Dot(rb.velocity, move) < -0.1) {
                handling *= 0.3f;
            }
        }

        rb.velocity = Vector2.Lerp(rb.velocity, move, handling);
    }
    private void DebugDash(float GetAxisX, float GetAxisY) {
        Vector2 move = Vector2.ClampMagnitude(new Vector2(GetAxisX, GetAxisY), 1);
        rb.velocity = move * pMovCurrentSpeed * 6;
    }

    // movement modifier access
    public bool getMovDisabled() {
        return pMovDisable;
    }

    public void setMovDisabled(bool disabled) {
        pMovDisable = disabled;
    }
    public float getMovHandleBase() {
        return pMovHandleBase;
    }
    public float getMovHandle() {
        return pMovHandle;
    }
    public void setMovHandle(float value, float duration) {
        if (pMovHandleResetCoroutine != null) {
            StopCoroutine(pMovHandleResetCoroutine);
        }
        pMovHandle = value;
        pMovHandleResetCoroutine = StartCoroutine(resetMovHandling(pMovHandleBase, duration));
    }
    // Coroutine: Waits duration seconds, then sets pMovHandle to value.
    public IEnumerator resetMovHandling(float value, float duration) {
        yield return new WaitForSeconds(duration);
        pMovHandle = value;
    }
    public float getMovSpeedBase() {
        return speed;
    }
    public float getMovSpeed() {
        return pMovCurrentSpeed;
    }
    public void setMovSpeed(float value, float duration) {
        if (pMovSpeedResetCoroutine != null) {
            StopCoroutine(pMovSpeedResetCoroutine);
        }
        pMovCurrentSpeed = value;
        pMovSpeedResetCoroutine = StartCoroutine(resetMovSpeed(speed, duration));
    }
    // Coroutine: Waits duration seconds, then sets pMovSpeed to value.
    public IEnumerator resetMovSpeed(float value, float duration) {
        yield return new WaitForSeconds(duration);
        pMovCurrentSpeed = value;
    }
    #endregion

    #region punchandbonk

    AudioSource bonkSFX;

    public void Punch(Vector2 dir) {
        if (pMovDisable || bonked)
            return;
        fist.Punch(dir);

    }

    public bool Bonk(Vector2 dir, float duration, int bonkingTeamNum = -1) {
        if (bonkingTeamNum == me.teamNum) {
            return false;
        }
        bonkSFX = GameManager.instance.sfx["BonkSFX"];
        DropAll(dir);
        rb.velocity = dir.normalized * 0.5f;
        // fx and stuff
        // play tweety bird animation
        setMovHandle(0.004f, duration);
        setStarsActive(duration);
        setMovSpeed(speed * 0.2f, duration);
        bonkSFX.Play();
        camScript.Shake(0.35f);
        return true;
    }
    public void setStarsActive(float duration) {
        if (bonkedResetCoroutine != null) {
            StopCoroutine(bonkedResetCoroutine);
        }
        stars.SetActive(true);
        bonked = true;
        bonkedResetCoroutine = StartCoroutine(StarsInactive(duration));
    }
    public IEnumerator StarsInactive(float duration) {
        yield return new WaitForSeconds(duration);
        stars.SetActive(false);
        bonked = false;
    }


    private void DropAll(Vector2 dir) {
        List<GameObject> itemsToScatter = new List<GameObject>();
        for (int i = 0; i < inventory.Size(); i++) {
            GameObject item = inventory.Get();
            if (item) {
                itemsToScatter.Add(item);
                Drop();
            }
            inventory.IncSlot();
        }
        float maxDropDistance = 10f;
        foreach (GameObject item in itemsToScatter) {
            Vector2 targetVector = (dir + UnityEngine.Random.insideUnitCircle).normalized * UnityEngine.Random.Range(0f, maxDropDistance);
            Rigidbody2D item_rb = item.GetComponent<Rigidbody2D>();
            if (item_rb && !item_rb.isKinematic) {
                float rb_scaleFactor = 10000f;
                item_rb.velocity = targetVector * rb_scaleFactor;
            } else {
                RaycastHit2D[] hits = Physics2D.RaycastAll(item.transform.position, targetVector.normalized, targetVector.magnitude);
                foreach (RaycastHit2D hit in hits) {
                    if (hit.collider.gameObject != gameObject) {
                        targetVector = hit.point;
                        break;
                    }
                }
                item.transform.position = targetVector;
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        CollisionEvent?.Invoke(collision);
    }
    #endregion
}
