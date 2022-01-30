using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RoadControl : MonoBehaviour
{
    [SerializeField]
    private Transform playerCar, playerCarEffect;
    private SpriteRenderer playerCarSprite, playerCarEffectSprite;
    [SerializeField]
    private Sprite playerCarStraight, playerCarLeft, playerCarHardLeft, playerCarRight, playerCarHardRight, 
                    playerCarCrash, playerCarCrashLeft, playerCarCrashRight;
    public List<Sprite> playerCarSpinoutFrames, playerCarExplodeFrames, playerCarRollFrames,
                        smokeStraightFrames, smokeLeftFrames, smokeRightFrames,
                        sparkStraightFrames, sparkLeftFrames, sparkRightFrames,
                        explosionFrames;
    [SerializeField]
    private float playerCarAnimationFrameTime;
    private float playerCarAnimationRefTime;
    private int playerCarAnimationIndex;

    [SerializeField, Range(0.0f, 1.0f)]
    private float steeringDeadZoneAlpha, turnFrameThreshold, hardTurnFrameThreshold;

    

    [SerializeField]
    private Transform refRoadStrip;
    [SerializeField]
    private int numSegsPerRumble, numScreenLines;
    [SerializeField]
    private float screenLineHeight;
    public List<Sprite> roadStripSprites;

    private List<Transform> roadScreenLines = new List<Transform>();
    private List<SpriteRenderer> roadScreenSprites = new List<SpriteRenderer>();

    // Camera is at depth = 0; road is at height = 0
    [SerializeField]
    private float camHeight, roadLength, roadWidth, screenHalfWidth, roadSegmentLength, roadStartZ, FOV;    // roadwidth is actually half the width
    [SerializeField]
    private int numStraightSegs, numSegsToDraw;
    private int straightSegCounter;         // This is essentially an index for determining where to add the RoadCurves
    private int rumbleStripCounter;         // Used to determine when a rumble strip is complete and the sprite should be changed for the next one
    private float curAddedSegmentZ;         // This is for keeping track of the Z position of the near edge of the next RoadSegment to add to the track
    private int curRoadStripSpriteIndex;
    private Sprite curRoadStripSprite;
    private float distCamToScreen;
    private float nearEdgeWidthScale, farEdgeWidthScale;

    private List<RoadSegment> roadSegments = new List<RoadSegment>();
    
    [SerializeField]
    public List<RoadCurve> roadCurves;
    [SerializeField]
    public RoadCurve rdCurveExample;

    private int curSegmentIndex;

    [SerializeField]
    private float topSpeed, autopilotSpeed, acceleration, deceleration, maxTurning, maxArmor;
    private float curPlayerSpeed, curPlayerTurning, camCurZ, playerZoffset, curPlayerPosX, curArmor;

    // GAME STATES
    private enum GameState { InMenu, TrackStart, Driving, Crashing, Exploding, TrackComplete, GameOver }
    private GameState GameMode;

    private bool canDrive, isOnAutopilot, isSlipping, isOffRoading, isCrashing, isSpinningOut, isExploding, isRolling;

    // ENEMIES

    // BILLBOARDS and OTHER SPRITES
    public List<BillboardSprite> billboardData;

    private List<BillboardSprite> billboardDataExpanded = new List<BillboardSprite>();
    private List<BillboardSprite> visibleBillboards = new List<BillboardSprite>();

    [SerializeField]
    private float billboardSizeCorrectionFactor;

    // Menu
    [SerializeField]
    private RawImage menuBgd;

    // HUD
    [SerializeField]
    private RawImage speedometer, speedometerNeedle, progressTrack, progressCar, armor, armorFrame;
    [SerializeField]
    private Text speedometerReading, timer, timerLabel, armorLabel, messageText;
    [SerializeField]
    private string trackStartReadyMessage, trackStartGoMessage, gameOverMessage, trackCompleteMessage; 
    [SerializeField]
    private float speedometerRotationFactor, timeLimit;
    private float timeElapsed, lastStartTime;
    [SerializeField]
    private float messageDelay;
    private float messageDelayRefTime;
    private bool readyMessageShown, goMessageShown, victoryMessageShown, gameOverMessageShown;
    private float progressCarInitialY;

    // Start is called before the first frame update
    void Start()
    {
        playerCarSprite = playerCar.GetComponent<SpriteRenderer>();

        if (billboardData.Count > 0) {
            foreach (BillboardSprite bb in billboardData) {
                if (bb.howMany > 1) {
                    for (int i = 0; i < bb.howMany; i++) {
                        BillboardSprite bbTemp = bb;
                        bbTemp.howMany = 1;
                        bbTemp.segmentIndex = bb.segmentIndex + 1;
                        billboardDataExpanded.Add(bbTemp);
                    }
                }
                else {
                    billboardDataExpanded.Add(bb);
                }
            }
        }

        curPlayerSpeed = 0;
        camCurZ = 0;
        distCamToScreen = (numScreenLines / 2f) / (Mathf.Tan(FOV / 2f));

        curAddedSegmentZ = camCurZ + roadStartZ;
        straightSegCounter = 0;
        rumbleStripCounter = 0;

        progressCarInitialY = progressCar.rectTransform.anchoredPosition.y;

        GameMode = GameState.TrackStart;
        canDrive = false;
        isOnAutopilot = false;
        readyMessageShown = false;
        goMessageShown = false;
        victoryMessageShown = false;
        gameOverMessageShown = false;

        ShowHUD(false);
        InitializeRoadStrips();
        ShowHUD(true);

        //Debug.Log("# road screen line sprites = " + roadScreenSprites.Count);
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(roadSegments.Count);

        switch (GameMode) {
            case GameState.InMenu:

                break;
            case GameState.TrackStart:

                menuBgd.enabled = false;
                

                curSegmentIndex = GetCurrentRoadSegmentIndex();

                UpdateRoad();
                isOnAutopilot = true;
                ManagePlayerPosition();

                if (!goMessageShown) {
                    if (!readyMessageShown) {
                        if (Time.time - messageDelayRefTime > messageDelay) {
                            messageText.text = trackStartReadyMessage;
                            messageText.enabled = true;

                            readyMessageShown = true;
                            messageDelayRefTime = Time.time;
                        }
                    }
                    else {
                        if (Time.time - messageDelayRefTime > messageDelay) {
                            messageText.text = trackStartGoMessage;

                            goMessageShown = true;
                            messageDelayRefTime = Time.time;
                        }
                    }
                }
                else {
                    if (Time.time - messageDelayRefTime > messageDelay) {
                        messageText.enabled = false;
                        GameMode = GameState.Driving;

                        lastStartTime = Time.time;
                        timer.text = ((int)timeLimit).ToString();
                        timer.enabled = true;
                        timerLabel.enabled = true;

                        progressTrack.enabled = true;
                        progressCar.enabled = true;

                        isOnAutopilot = false;
                        canDrive = true;
                    }
                }

                break;
            case GameState.Driving:

                timer.text = ((int)(timeLimit - (Time.time - lastStartTime))).ToString();

                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManagePlayerPosition();
                UpdateRoad();

                if (Time.time - lastStartTime >= timeLimit) {
                    canDrive = false;

                    messageDelayRefTime = Time.time;
                    GameMode = GameState.GameOver;
                }
                else {

                }

                break;
            case GameState.Crashing:
                break;
            case GameState.Exploding:
                break;
            case GameState.TrackComplete:

                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManagePlayerPosition();
                UpdateRoad();

                if (!victoryMessageShown) {
                    if (Time.time - messageDelayRefTime > messageDelay) {
                        messageText.text = trackCompleteMessage;
                        messageText.enabled = true;

                        victoryMessageShown = true;
                        messageDelayRefTime = Time.time;
                    }
                }
                else {
                    if (Time.time - messageDelayRefTime > messageDelay) {

                        ShowHUD(false);
                        menuBgd.enabled = true;
                        GameMode = GameState.InMenu;
                    }
                }

                break;
            case GameState.GameOver:

                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManagePlayerPosition();
                UpdateRoad();

                if (!gameOverMessageShown) {
                    if (Time.time - messageDelayRefTime > messageDelay) {
                        messageText.text = gameOverMessage;
                        messageText.enabled = true;

                        messageDelayRefTime = Time.time;
                        gameOverMessageShown = true;
                    }
                }
                else {
                    if (Time.time - messageDelayRefTime > messageDelay) {

                        ShowHUD(false);
                        menuBgd.enabled = true;
                        GameMode = GameState.InMenu;
                    }
                }

                break;
        }
    }

    private void InitializeRoadStrips() {

        roadScreenLines.Clear();
        roadScreenLines.Add(refRoadStrip);
        roadScreenSprites.Clear();
        roadScreenSprites.Add(refRoadStrip.GetComponent<SpriteRenderer>());

        for (int i = 1; i < numScreenLines; i++) {
            GameObject newStrip = new GameObject();
            newStrip.transform.position = refRoadStrip.transform.position + new Vector3(0, screenLineHeight * i, 0);
            newStrip.AddComponent<SpriteRenderer>();
            roadScreenLines.Add(newStrip.transform);
            roadScreenSprites.Add(newStrip.GetComponent<SpriteRenderer>());
        }
        
        curRoadStripSpriteIndex = 0;
        curRoadStripSprite = roadStripSprites[curRoadStripSpriteIndex];

        while (straightSegCounter < numStraightSegs) {

            if (rumbleStripCounter > numSegsPerRumble) {
                curRoadStripSpriteIndex = (curRoadStripSpriteIndex + 1) % roadStripSprites.Count;
                curRoadStripSprite = roadStripSprites[curRoadStripSpriteIndex];
                rumbleStripCounter = 1;
            }
            RoadSegment newSeg = new RoadSegment();
            //newSeg.EdgeNearZ = camCurZ + roadStartZ + roadSegmentLength * i;
            //newSeg.EdgeFarZ = camCurZ + roadStartZ + roadSegmentLength * (i + 1);
            newSeg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            newSeg.EdgeFarZ = curAddedSegmentZ;
            newSeg.Curve = 0;
            newSeg.Y = 0;
            newSeg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(newSeg);

            if (roadCurves.Count > 0) {
                foreach(RoadCurve crv in roadCurves) {
                    if (crv.segmentInsertIndex == straightSegCounter) {
                        AddRoadCurve(crv);

                        //Debug.Log("hi, " + straightSegCounter);
                    }
                }
            }
            
            straightSegCounter++;
        }
    }

    private int GetCurrentRoadSegmentIndex() {
        return ((int)(Mathf.Floor(camCurZ / roadSegmentLength) % roadSegments.Count));
    }

    private void UpdateRoad() {
        //Debug.Log(curSegmentIndex);
        int highestScreenLineDrawn = -1;    // First screen line is roadScreenLines[0], so let's use -1 as a reference

        UpdateBillboards();

        float x = 0;
        float dx;

        for (int i = curSegmentIndex; i < (Mathf.Min(curSegmentIndex + numSegsToDraw - 1, numStraightSegs)); i++) {
            int nearEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeNearZ - camCurZ)));
            int farEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeFarZ - camCurZ)));
            nearEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeNearZ - camCurZ));
            farEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeFarZ - camCurZ));

            //Debug.Log(roadSegments[i].EdgeNearZ);
            //Debug.Log(nearEdgeHeight + ", " + farEdgeHeight);

            nearEdgeHeight -= 130;
            farEdgeHeight -= 130;

            dx = roadSegments[i].Curve;

            if (farEdgeHeight > highestScreenLineDrawn && farEdgeHeight < numScreenLines) {
                if (farEdgeHeight > nearEdgeHeight) {
                    for (int j = nearEdgeHeight; j <= farEdgeHeight; j++) {
                        float dxFraction = (j - nearEdgeHeight) * (dx / (farEdgeHeight - nearEdgeHeight));
                        float scaleIncreaseFromNearEdge = (j - nearEdgeHeight) * ((farEdgeWidthScale - nearEdgeWidthScale) / (farEdgeHeight - nearEdgeHeight));
                        if (j >= 0) {
                            roadScreenSprites[j].sprite = roadSegments[i].SpriteVariation;
                            roadScreenLines[j].position = new Vector3(x + dxFraction,
                                                                    roadScreenLines[j].position.y, roadScreenLines[j].position.z);
                            roadScreenLines[j].localScale = new Vector3(nearEdgeWidthScale + scaleIncreaseFromNearEdge, 1.0f, 1.0f);
                            //Debug.Log(roadScreenLines[j].localScale.x);
                        }
                    }
                    //Debug.Log("thick seg, screen line #" + nearEdgeHeight);
                }
                else {
                    roadScreenSprites[farEdgeHeight].sprite = roadSegments[i].SpriteVariation;
                    roadScreenLines[farEdgeHeight].position = new Vector3(x + dx,
                                                                        roadScreenLines[farEdgeHeight].position.y, roadScreenLines[farEdgeHeight].position.z);
                    roadScreenLines[farEdgeHeight].localScale = new Vector3(farEdgeWidthScale, 1.0f, 1.0f);
                    //Debug.Log("single seg, screen line #" + nearEdgeHeight);
                }
            }

            foreach (BillboardSprite bb in visibleBillboards) {
                if (bb.segmentIndex == i) {

                    float roadWidthOffset;
                    if (bb.offsetX < 0) { roadWidthOffset = screenHalfWidth * -1; }
                    else                { roadWidthOffset = screenHalfWidth; }

                    float sizeCorrection = nearEdgeWidthScale * billboardSizeCorrectionFactor;

                    bb.spriteTransform.localScale = new Vector3(sizeCorrection, sizeCorrection, 1);
                    bb.spriteTransform.position = new Vector3(x + (bb.offsetX + roadWidthOffset) * nearEdgeWidthScale,
                                                                nearEdgeHeight + bb.spriteType.rect.height * nearEdgeWidthScale / 2, -1f);
                    bb.spriteRend.enabled = true;
                }
            }

            x += dx;
            highestScreenLineDrawn = farEdgeHeight;
            //Debug.Log(highestScreenLineDrawn);
        }

        float progressTrackLength = progressTrack.rectTransform.rect.height;
        Vector2 pcPos = progressCar.rectTransform.anchoredPosition;
        float pcPosOffset = (progressTrackLength / roadSegments.Count) * curSegmentIndex;

        progressCar.rectTransform.anchoredPosition = new Vector2(progressCar.rectTransform.anchoredPosition.x, progressCarInitialY + pcPosOffset);
    }

    // This method is used when first loading the road to see if there are any sprites in the immediate vicinity (not just at the visibility edge)
    private void LoadOpeningBillboards() {
        foreach (BillboardSprite bb in billboardDataExpanded) {
            if (bb.segmentIndex <= curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject();
                obj.AddComponent<SpriteRenderer>();
                bb.spriteTransform = obj.transform;
                bb.spriteRend = obj.GetComponent<SpriteRenderer>();
                bb.spriteRend.enabled = false;
                bb.spriteRend.sprite = bb.spriteType;


                visibleBillboards.Add(bb);
                Debug.Log(bb.spriteTransform);
            }
        }
    }

    private void UpdateBillboards() {

        List<BillboardSprite> tempBBs = new List<BillboardSprite>();

        foreach (BillboardSprite bb in billboardDataExpanded) {
            if (bb.segmentIndex == curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject();
                obj.AddComponent<SpriteRenderer>();
                bb.spriteTransform = obj.transform;
                bb.spriteRend = obj.GetComponent<SpriteRenderer>();
                bb.spriteRend.enabled = false;
                bb.spriteRend.sprite = bb.spriteType;

                visibleBillboards.Add(bb);
                BillboardSprite tempBB = bb;
                tempBBs.Add(tempBB);
            }
        }

        foreach (BillboardSprite tmp in tempBBs) { billboardDataExpanded.Remove(tmp); }
        tempBBs.Clear();

        foreach (BillboardSprite bb in visibleBillboards) {
            if (bb.segmentIndex < curSegmentIndex) {
                Destroy(bb.spriteTransform.gameObject);
                tempBBs.Add(bb);
            }
        }

        foreach (BillboardSprite bb in tempBBs) { visibleBillboards.Remove(bb); }
    }

    private void AddRoadCurve(RoadCurve rdCurve) {

        float startY = roadSegments[roadSegments.Count - 1].Y;
        float endY = startY + rdCurve.elevationShift * roadSegmentLength;
        float total = rdCurve.numEnterSegments + rdCurve.numHoldSegments + rdCurve.numExitSegments;

        for (int i = 0; i < rdCurve.numEnterSegments; i++) {
            CheckRumbleStripCounter();
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            seg.Curve = rdCurve.curveIntensity * Mathf.Pow(i / rdCurve.numEnterSegments, 2);
            seg.Y = startY + (endY - startY) * ((-Mathf.Cos((i / total) * Mathf.PI) / 2) + 0.5f);
            seg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(seg);
        }
        for (int i = 0; i < rdCurve.numHoldSegments; i++) {
            CheckRumbleStripCounter();
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            seg.Curve = rdCurve.curveIntensity;
            seg.Y = startY + (endY - startY) * ((-Mathf.Cos(((i + rdCurve.numEnterSegments) / total) * Mathf.PI) / 2) + 0.5f);
            seg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(seg);
        }
        for (int i = 0; i < rdCurve.numExitSegments; i++) {
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            seg.Curve = rdCurve.curveIntensity - rdCurve.curveIntensity * (-Mathf.Cos((i / rdCurve.numEnterSegments) * Mathf.PI) / 2 + 0.5f);
            seg.Y = startY + (endY - startY) * ((-Mathf.Cos(((i + rdCurve.numEnterSegments + rdCurve.numHoldSegments) / total) * Mathf.PI) / 2) + 0.5f);
            seg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(seg);
        }
    }

    private void CheckRumbleStripCounter() {
        if (rumbleStripCounter > numSegsPerRumble) {
            curRoadStripSpriteIndex = (curRoadStripSpriteIndex + 1) % roadStripSprites.Count;
            curRoadStripSprite = roadStripSprites[curRoadStripSpriteIndex];
            rumbleStripCounter = 1;
        }
    }

    private void IncrementSegmentsAndRumbleStrips() {
        curAddedSegmentZ += roadSegmentLength;
        rumbleStripCounter++;
    }

    private void ManagePlayerPosition() {

        curPlayerTurning = maxTurning;

        speedometerNeedle.rectTransform.rotation = Quaternion.Euler(0, 0, curPlayerSpeed * speedometerRotationFactor);
        speedometerReading.text = ((int)curPlayerSpeed / 12).ToString();

        if (canDrive) {
            if (InputMapper.inputMapper[(int)InputMapper.CONTROLS.up]) {
                if (curPlayerSpeed < topSpeed) { curPlayerSpeed += acceleration * Time.deltaTime; }
                else { curPlayerSpeed = topSpeed; }
            }
            else {
                if (curPlayerSpeed > 0) { curPlayerSpeed -= deceleration * Time.deltaTime; }
                else { curPlayerSpeed = 0.0f; }
            }

            if (InputMapper.inputMapper[(int)InputMapper.CONTROLS.left]) {

                if (InputMapper.inputMapper[(int)InputMapper.CONTROLS.action]) {
                    playerCarSprite.sprite = playerCarHardLeft;
                    if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                        playerCar.Translate(-maxTurning * Time.deltaTime, 0, 0);
                    }
                }
                else {
                    playerCarSprite.sprite = playerCarLeft;
                    if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                        playerCar.Translate(-maxTurning / 2 * Time.deltaTime, 0, 0);
                    }
                }
            }
            else if (InputMapper.inputMapper[(int)InputMapper.CONTROLS.right]) {

                if (InputMapper.inputMapper[(int)InputMapper.CONTROLS.action]) {
                    playerCarSprite.sprite = playerCarHardRight;
                    if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                        playerCar.Translate(maxTurning * Time.deltaTime, 0, 0);
                    }
                }
                else {
                    playerCarSprite.sprite = playerCarRight;
                    if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                        playerCar.Translate(maxTurning / 2 * Time.deltaTime, 0, 0);
                    }
                }
            }
            else {
                playerCarSprite.sprite = playerCarStraight;
            }
        }
        else if (isOnAutopilot) {
            curPlayerSpeed = autopilotSpeed;
        }
        else {
            if (curPlayerSpeed > 0) { curPlayerSpeed -= deceleration * Time.deltaTime; }
            else { curPlayerSpeed = 0.0f; }
        }

        camCurZ += curPlayerSpeed * Time.deltaTime;
    }

    private void ShowHUD(bool showOrNo) {
        if (showOrNo) {
            armor.enabled = true;
            armorLabel.enabled = true;
            armorFrame.enabled = true;

            speedometer.enabled = true;
            speedometerNeedle.enabled = true;
            speedometerReading.enabled = true;

            timerLabel.enabled = true;

            progressCar.enabled = true;
            progressTrack.enabled = true;

            
        }
        else {
            armor.enabled = false;
            armorFrame.enabled = false;
            armorLabel.enabled = false;

            speedometer.enabled = false;
            speedometerNeedle.enabled = false;
            speedometerReading.enabled = false;

            timer.enabled = false;
            timerLabel.enabled = false;

            progressCar.enabled = false;
            progressTrack.enabled = false;

            messageText.enabled = false;
        }
    }
}
