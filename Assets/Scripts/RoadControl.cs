using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//TODO: REPLACE LISTS WITH SIZE LIMITED OBJECT POOLS
public class RoadControl : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private Transform playerCar, playerCarEffect;
    private SpriteRenderer playerCarSprite, playerCarEffectSprite;
    [SerializeField]
    private Sprite playerCarStraight, playerCarSlightLeft, playerCarLeft, playerCarHardLeft, playerCarSlightRight, playerCarRight, playerCarHardRight,
                    playerCarStraightRed, playerCarSlightLeftRed, playerCarLeftRed, playerCarHardLeftRed, playerCarSlightRightRed, playerCarRightRed, playerCarHardRightRed;

    public List<Sprite> playerCarSpinoutLeftFrames, playerCarSpinoutRightFrames,
                        playerCarCrashLeftFrames, playerCarCrashRightFrames,
                        playerCarExplodeFrames,
                        playerCarRollLeftFrames, playerCarRollRightFrames;
    public List<Sprite> redPlayerCarSpinoutLeftFrames, redPlayerCarSpinoutRightFrames,
                        redPlayerCarCrashLeftFrames, redPlayerCarCrashRightFrames,
                        redPlayerCarExplodeFrames,
                        redPlayerCarRollLeftFrames, redPlayerCarRollRightFrames;

    private Sprite curPlayerCarStraight, curPlayerCarSlightLeft, curPlayerCarLeft, curPlayerCarHardLeft, curPlayerCarSlightRight, curPlayerCarRight, curPlayerCarHardRight;
    private List<Sprite> curPlayerCarSpinoutLeftFrames = new List<Sprite>(), curPlayerCarSpinoutRightFrames = new List<Sprite>(),
                        curPlayerCarCrashLeftFrames = new List<Sprite>(), curPlayerCarCrashRightFrames = new List<Sprite>(),
                        curPlayerCarExplodeFrames = new List<Sprite>(), 
                        curPlayerCarRollLeftFrames = new List<Sprite>(), curPlayerCarRollRightFrames = new List<Sprite>();
                        
    [SerializeField]
    private float playerCarAnimationFrameTime;
    [SerializeField, Range(0.0f, 100.0f)]
    private float decayFrameTimeFactor;     // This is a percentage increase on the frame time for certain animations (e.g., spinning car rotation slows down)
    [SerializeField]
    private bool isDecayLinear;             // If true, the percentage increase is constant (not scaling up with the increasing frame time)
    private float playerCarAnimationFrameTimeAdjusted, playerCarAnimationRefTime;
    private int playerCarAnimationIndex;
    private bool playerIsCrashingLeft = false, playerIsCrashingRight = false, 
                    playerIsSpinningOutLeft = false, playerIsSpinningOutRight = false,
                    playerIsRollingLeft = false, playerIsRollingRight = false;

    [SerializeField, Range(0.0f, 1.0f)]
    private float steeringDeadZoneAlpha, turnFrameThreshold, hardTurnFrameThreshold;

    [SerializeField]
    private Transform refRoadStrip;
    [SerializeField]
    private int numSegsPerRumble, numScreenLines;
    [SerializeField]
    private float screenLineHeight;
    [SerializeField]
    private Transform roadTopLineCoverStrip, bgd;
    [SerializeField]
    private float bgdOffset;
    public List<Sprite> roadStripSprites;

    private List<Transform> roadScreenLines = new List<Transform>();
    private List<SpriteRenderer> roadScreenSprites = new List<SpriteRenderer>();

    // Camera is at depth = 0; road is at height = 0
    [SerializeField]
    private float camHeight, roadLength, roadWidth, laneWidth, screenHalfWidth, roadSegmentLength, roadStartZ, FOV;    // roadwidth is actually half the width
    [SerializeField]
    private int numStraightSegs, numSegsToDraw, segIndexSkipSegThreshold, numOfSegsToSkipPerPass;   // Skipping segments past a certain distance for improved performance
    private int straightSegCounter;         // This is essentially an index for determining where to add the RoadCurves
    private int rumbleStripCounter;         // Used to determine when a rumble strip is complete and the sprite should be changed for the next one
    private float curAddedSegmentZ;         // This is for keeping track of the Z position of the near edge of the next RoadSegment to add to the track
    private int curRoadStripSpriteIndex;
    private Sprite curRoadStripSprite;
    private float FOVInRads, distCamToScreen;
    private float nearEdgeWidthScale, farEdgeWidthScale;
    private float curCamY;

    private List<RoadSegment> roadSegments = new List<RoadSegment>();
    
    [SerializeField]
    public List<RoadCurve> roadCurves;
    [SerializeField]
    public RoadCurve rdCurveExample;

    private int curSegmentIndex;

    [SerializeField]
    private float mass, playerCarWidth, playerCarLengthInSegs, topSpeed, autopilotSpeed, maxAcceleration, deceleration, centripetal, 
                    maxTurning, turnIncrease, hardTurnIncrease, lowTurnZeroThreshold, turnDriftToCenterFactor, slipThreshold, 
                    maxArmor, roadGrip, elasticity;
    private float curPlayerSpeed, curPlayerAcceleration, curPlayerTurning, curPlayerDrift, curPlayerPosZ, playerZoffset, curPlayerPosX, playerInitialY, 
                    curArmor;

    // GAME STATES
    private enum GameState { InMenu, TrackStart, Driving, Crashing, Exploding, TrackComplete, GameOver }
    private GameState GameMode;

    private bool canDrive, isOnAutopilot, isSlipping, isOffRoading, isCrashing, isSpinningOut, 
                    isSmoking, isFinishedSlideSmoking, isExploding, isRolling, isInvulnerable;

    // ENEMIES
    [SerializeField]
    private Sprite bombVisible, bombCamo, droidHolding, droidEmptyHanded;
    [SerializeField]
    private float droidHeight, delayBeforeDropping, bombDropSpeed, bombSizeCorrectionFactor;

    public List<BombDroid> droids;
    private List<BombDroid> visibleDroids = new List<BombDroid>();
    private List<Bomb> activeBombPool = new List<Bomb>();

    public List<CarModel> trafficCars;
    private List<CarModel> enemyModels = new List<CarModel>();
    private List<CarModel> innocentModels = new List<CarModel>();
    private List<NPCar> carsOnRoad = new List<NPCar>();

    [SerializeField]
    private int numEnemyModels, numInnocentModels;
    [SerializeField, Range(0.0f, 2000.0f)]
    private float enemySpeedMin, enemySpeedMax, innocentSpeedMin, innocentSpeedMax, laneChangeMin, LaneChangeMax;
    [SerializeField]
    private float enemySlightTurnRotationThreshold, enemyTurnRotationThreshold, enemyHardTurnRotationThreshold;

    // BILLBOARDS and OTHER SPRITES
    public List<BillboardSprite> billboardData;

    private List<BillboardSprite> billboardDataExpanded = new List<BillboardSprite>();
    private List<BillboardSprite> visibleBillboards = new List<BillboardSprite>();

    [SerializeField]
    private float billboardSizeCorrectionFactor, spriteBgdZ;

    // SPECIAL EFFECTS
    [SerializeField]
    public List<Sprite> smokeFrontFrames, smokeLeftFrames, smokeRightFrames,
                        sparkFrontFrames, sparkBackFrames, 
                        sparkLeftFrames, sparkRightFrames,
                        fireFrames, explosionFrames,
                        smokeFrontFramesRed, smokeLeftFramesRed, smokeRightFramesRed,
                        sparkFrontFramesRed, sparkBackFramesRed, 
                        sparkLeftFramesRed, sparkRightFramesRed,
                        fireFramesRed, explosionFramesRed;

    private List<Sprite> curSmokeFrontFrames = new List<Sprite>(), curSmokeLeftFrames = new List<Sprite>(), curSmokeRightFrames = new List<Sprite>(),
                        curSparkFrontFrames = new List<Sprite>(), curSparkBackFrames = new List<Sprite>(), 
                        curSparkLeftFrames = new List<Sprite>(), curSparkRightFrames = new List<Sprite>(),
                        curFireFrames = new List<Sprite>(), curExplosionFrames = new List<Sprite>();

    private List<Spark> playerSparks = new List<Spark>();
    private List<Spark> sparks = new List<Spark>();

    [SerializeField]
    private float maxBombIntensity, bombExplosionFrameTime;
    [SerializeField]
    private float smokeFrameTime, sparkFrameTime;
    [SerializeField]
    private int restartSmokeStartFrameIndex;
    [SerializeField, Range(0.0f, 100.0f)]
    private float percentTopSpeedSlipStartThreshold, percentTopSpeedSlipStopThreshold,
                    percentMaxAccelerationSlipStartThreshold, percentMaxAccelerationSlipStopThreshold;
    private float smokeFrameRefTime;
    private int curSmokeFrameIndex;
    [SerializeField]
    private float playerSparkFrontOffset, playerSparkBackOffset, playerSparkLeftOffset, playerSparkRightOffset;

    //InputMapper reference
    private InputMapper inputMapper;

    // Menu
    [SerializeField]
    private RawImage menuBgd;

    // HUD
    [SerializeField]
    private RawImage centerConsole, centerConsoleCover, speedometerNeedle, 
                        progressTrack, progressCar, 
                        armor, armorFrame, armorBarCutter;
    [SerializeField]
    private SpriteRenderer sensorOverlay, sensorScanline;

    [SerializeField]
    private Text messageText;
    [SerializeField]
    private string trackStartReadyMessage, trackStartGoMessage, gameOverMessage, trackCompleteMessage;
    [SerializeField]
    private float crashTime, spinoutTime, invulnTime, invulnFlashIntervalTime;
    [SerializeField]
    private float impactThresholdForSpinout, ricochetSpeedMultiplier, ricochetDecayFactor, ricochetEndThreshold;
    private Vector3 collisionSpeedEffect = new Vector3();
    private float crashRefTime, invulnRefTime, invulnFlashIntervalRefTime;
    [SerializeField]
    private float speedometerRotationFactor;
    [SerializeField]
    private float messageDelay;
    private float messageDelayRefTime;
    private bool readyMessageShown, goMessageShown, victoryMessageShown, gameOverMessageShown;
    private float progressCarInitialY;

    [SerializeField]
    private float armorMaxValue, armorBarWidth, damageIncurredByCrashing, damageIncurredBySpinoutCrash;
    private float curArmorValue, damageTaken, armorCutterInitialX;

    public List<Sprite> sensorOverlayLoadFrames, sensorOverlayUnloadFrames;
    [SerializeField]
    private float sensorOverlayFrameTime;
    private float sensorOverlayFrameRefTime;
    private int curSensorOverlayFrameIndex;
    [SerializeField]
    private float sensorScanLineLowestHeight, sensorScanLineDisplacement;
    private float sensorScanLineInitialHeight;
    private bool sensorOverlayWasJustActivated, sensorOverlayIsActive, sensorOverlayIsLoading, sensorOverlayIsTotallyLoaded, sensorOverlayIsUnloading;
    [SerializeField]
    private float sensorDetectableDepthZ, sensorUndetectableDepthZ;     // Depth (z) values for cars and billboards that are detectable and not

    [SerializeField]
    private Transform warningRoadLinesParent;
    public List<RawImage> warningRoadLines;
    //private List<RawImage> warningRoadBlips = new List<RawImage>();

    [SerializeField]
    private Sprite warningRoadLineSprite, warningRoadLineAlternateSprite;
    [SerializeField]
    private Texture warningRoadCarBlipSprite, warningRoadBombBlipSprite;
    public List<Texture> centerConsoleCoverFrames;

    [SerializeField]
    private float sensorMaxPower, sensorDrainRate, sensorRechargeRate;
    private float curSensorPower;
    [SerializeField, Range(0.0f, 100.0f)]
    private float sensorLowPowerPercentageOfMax;
    private int curCenterConsoleFrameIndex, curWarningRoadLineIndex;
    private float warningRoadLinesInitialX, warningRoadLineBaseWidth, roadLineToWarningRoadLineConversion;
    public List<RawImage> sensorBars;
    [SerializeField]
    public Color sensorBarChargingColor, sensorBarFullyChargedColor, sensorBarLowPowerColor;
    [SerializeField]
    private float bombBlipProjectionFlashTime, bombBlipProjectionRoadOffsetY, blipSizeIncreaseFactor;
    private int bombBlipBaseWidth, bombBlipBaseHeight;

    // Start is called before the first frame update
    void Start()
    {
        inputMapper = InputMapper.inputMapper;
        playerCarSprite = playerCar.GetComponent<SpriteRenderer>();
        playerCarEffectSprite = playerCarEffect.GetComponent<SpriteRenderer>();

        BillboardSprite bbTemp;

        foreach (BillboardSprite bb in billboardData) {
            if (bb.howManyRows > 1 || bb.howManyCols > 1) {
                for (int i = 0; i < bb.howManyRows; i++) {
                    for (int j = 0; j < bb.howManyCols; j++) {
                        bbTemp = new BillboardSprite();
                        bbTemp.spriteType = bb.spriteType;
                        bbTemp.segmentIndex = bb.segmentIndex + (i * bb.rowSpacingInSegs);

                        if (bb.offsetX >= 0) {
                            bbTemp.offsetX = bb.offsetX + (j * bb.colSpacingInPixels);
                        }
                        else {
                            bbTemp.offsetX = bb.offsetX + (-j * bb.colSpacingInPixels);
                        }

                        billboardDataExpanded.Add(bbTemp);
                    }
                }
            }
            else {
                billboardDataExpanded.Add(bb);
            }
        }

        curPlayerSpeed = 0;
        curPlayerTurning = 0;
        curPlayerPosZ = 0;
        FOVInRads = FOV * Mathf.PI / 180f;
        distCamToScreen = (numScreenLines >> 1) / (Mathf.Tan(FOVInRads * 0.5f));
        //curCamY = camHeight;
        playerInitialY = playerCar.position.y;

        curAddedSegmentZ = curPlayerPosZ + roadStartZ;
        straightSegCounter = 0;
        rumbleStripCounter = 0;

        progressCarInitialY = progressCar.rectTransform.anchoredPosition.y;

        GameMode = GameState.TrackStart;
        canDrive = false;
        isOnAutopilot = false;
        isInvulnerable = false;
        isFinishedSlideSmoking = false;
        collisionSpeedEffect = Vector3.zero;

        sensorOverlayWasJustActivated = false;
        sensorOverlayIsActive = false;
        sensorOverlayIsLoading = false;
        sensorOverlayIsTotallyLoaded = false;
        sensorOverlayIsUnloading = false;
        sensorScanLineInitialHeight = sensorScanline.transform.position.y;
        SetPlayerCarSprites();
        SetEffectSprites();

        curSensorPower = sensorMaxPower;

        curWarningRoadLineIndex = 0;
        warningRoadLinesInitialX = warningRoadLines[0].rectTransform.anchoredPosition.x;
        warningRoadLineBaseWidth = warningRoadLines[0].rectTransform.rect.width;
        roadLineToWarningRoadLineConversion = ((float)warningRoadLines.Count / (float)numScreenLines);
        bombBlipBaseWidth = warningRoadBombBlipSprite.width * (int)blipSizeIncreaseFactor;
        bombBlipBaseHeight = warningRoadBombBlipSprite.height * (int)blipSizeIncreaseFactor;

        readyMessageShown = false;
        goMessageShown = false;
        victoryMessageShown = false;
        gameOverMessageShown = false;

        curArmorValue = armorMaxValue;
        damageTaken = 0.0f;

        sparks.Clear();

        ShowHUD(false);
        InitializeRoadStrips();
        LoadOpeningEnemies();
        ShowHUD(true);
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
                ManageControlsAndPlayerPosition();

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

                        progressTrack.enabled = true;
                        progressCar.enabled = true;

                        isOnAutopilot = false;
                        canDrive = true;
                    }
                }
            break;

            case GameState.Driving:
                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManageControlsAndPlayerPosition();
                UpdateRoad();

                if (curArmorValue < 0) {
                    messageDelayRefTime = Time.time;
                    gameOverMessageShown = false;
                    GameMode = GameState.GameOver;
                }
            break;

            case GameState.Crashing:
                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManageControlsAndPlayerPosition();
                UpdateRoad();

                if (isCrashing) {
                    curPlayerAcceleration = 0;

                    // Run through crash animation frames
                    if (Time.time - playerCarAnimationRefTime > playerCarAnimationFrameTimeAdjusted) {
                        if (playerIsCrashingLeft) {
                            if (playerCarAnimationIndex < curPlayerCarCrashLeftFrames.Count - 1) {
                                playerCarAnimationIndex++;
                            }

                            playerCarSprite.sprite = curPlayerCarCrashLeftFrames[playerCarAnimationIndex];
                        }
                        else if (playerIsCrashingRight) {
                            if (playerCarAnimationIndex < curPlayerCarCrashRightFrames.Count - 1) {
                                playerCarAnimationIndex++;
                            }
                            
                            playerCarSprite.sprite = curPlayerCarCrashRightFrames[playerCarAnimationIndex];
                        }
                        else if (playerIsSpinningOutLeft) {
                            playerCarAnimationIndex = (playerCarAnimationIndex >= curPlayerCarSpinoutLeftFrames.Count) ? 0 : playerCarAnimationIndex + 1;
                            playerCarSprite.sprite = curPlayerCarSpinoutLeftFrames[playerCarAnimationIndex];
                        }
                        else if (playerIsSpinningOutRight) {
                            playerCarAnimationIndex = (playerCarAnimationIndex >= curPlayerCarSpinoutRightFrames.Count) ? 0 : playerCarAnimationIndex + 1;
                            playerCarSprite.sprite = curPlayerCarSpinoutRightFrames[playerCarAnimationIndex];
                        }
                        else if (playerIsRollingLeft) {
                            if (playerCarAnimationIndex < curPlayerCarRollLeftFrames.Count - 1) {
                                playerCarAnimationIndex++;
                            }

                            playerCarSprite.sprite = curPlayerCarRollLeftFrames[playerCarAnimationIndex];
                        }
                        else if (playerIsRollingRight) {
                            if (playerCarAnimationIndex < curPlayerCarRollRightFrames.Count - 1) {
                                playerCarAnimationIndex++;
                            }

                            playerCarSprite.sprite = curPlayerCarRollRightFrames[playerCarAnimationIndex];
                        }

                        if (isDecayLinear) {
                            playerCarAnimationFrameTimeAdjusted += (decayFrameTimeFactor * playerCarAnimationFrameTime) / 100f;
                        }
                        else {
                            playerCarAnimationFrameTimeAdjusted += (decayFrameTimeFactor * playerCarAnimationFrameTimeAdjusted) / 100f;
                        }

                        playerCarAnimationRefTime = Time.time;
                    }

                    if (collisionSpeedEffect.magnitude > ricochetEndThreshold) {
                        collisionSpeedEffect = collisionSpeedEffect * ricochetDecayFactor;
                    }
                    else {
                        collisionSpeedEffect = Vector3.zero;
                    }

                    if (Time.time - crashRefTime > crashTime) {
                        playerIsCrashingLeft = false;
                        playerIsCrashingRight = false;
                        playerIsSpinningOutLeft = false;
                        playerIsSpinningOutRight = false;
                        playerIsRollingLeft = false;
                        playerIsRollingRight = false;
                        isCrashing = false;
                        canDrive = true;

                        collisionSpeedEffect = Vector3.zero;

                        playerCarSprite.sprite = curPlayerCarStraight;

                        invulnRefTime = Time.time;
                        invulnFlashIntervalRefTime = Time.time;
                    }
                }
                else {

                    if (Time.time - invulnFlashIntervalRefTime > invulnFlashIntervalTime) {
                        playerCarSprite.enabled = !playerCarSprite.enabled;
                        invulnFlashIntervalRefTime = Time.time;
                    }

                    if (Time.time - invulnRefTime > invulnTime) {
                        isInvulnerable = false;
                        playerCarSprite.enabled = true;
                        GameMode = GameState.Driving;
                    }
                }
            break;

            case GameState.Exploding:
            break;

            case GameState.TrackComplete:
                curSegmentIndex = GetCurrentRoadSegmentIndex();

                ManageControlsAndPlayerPosition();
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

                ManageControlsAndPlayerPosition();
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
                        inputMapper.TogglePauseMenu();
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

Debug.Log(roadSegments.Count);
    }

    private int GetCurrentRoadSegmentIndex() {
        return ((int)(Mathf.Floor(curPlayerPosZ / roadSegmentLength) % roadSegments.Count));
    }

    private void UpdateRoad() {
//Debug.Log(curSegmentIndex);
        int highestScreenLineDrawn = -1;    // First screen line is roadScreenLines[0], so let's use -1 as a reference

        UpdateBillboards();
        UpdateEnemies();
        UpdateSparks();

        float x = 0;
        float dx;

        int nearEdgeHeight, farEdgeHeight;
        int absHighestScreenLineDrawn = -1, roadSegIndexOfHighestScreenLine = 0;
        int halfScreenLines = numScreenLines >> 1;

        for (int i = curSegmentIndex; i < (Mathf.Min(curSegmentIndex + numSegsToDraw - 1, numStraightSegs)); i++) {
            farEdgeHeight = (int)(halfScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeFarZ - curPlayerPosZ)));
            if (farEdgeHeight > absHighestScreenLineDrawn) { roadSegIndexOfHighestScreenLine = i; }
            absHighestScreenLineDrawn = Mathf.Max(absHighestScreenLineDrawn, farEdgeHeight);
        }

//Debug.Log(absHighestScreenLineDrawn + ", " + roadSegIndexOfHighestScreenLine);

        for (int i = curSegmentIndex; i < (Mathf.Min(curSegmentIndex + numSegsToDraw - 1, numStraightSegs)); i++) {
            int segNumDrawn = i - curSegmentIndex;

            if (segNumDrawn >= 0) {//((segNumDrawn < segIndexSkipSegThreshold) || ((segNumDrawn > segIndexSkipSegThreshold) && (segNumDrawn % numOfSegsToSkipPerPass == 0))) {
                nearEdgeHeight = (int)(halfScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeNearZ - curPlayerPosZ)));
                farEdgeHeight = (int)(halfScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeFarZ - curPlayerPosZ)));
                nearEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeNearZ - curPlayerPosZ + (camHeight + roadSegments[i].Y) / Mathf.Tan(FOVInRads)));
                farEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeFarZ - curPlayerPosZ + (camHeight + roadSegments[i].Y) / Mathf.Tan(FOVInRads)));

//if (segNumDrawn % numOfSegsToSkipPerPass == 0){
//    Debug.Log(curSegmentIndex + ", " + i);
//}
//Debug.Log(roadSegments[i].EdgeNearZ);
//Debug.Log(nearEdgeHeight + ", " + farEdgeHeight);

                //nearEdgeHeight -= 100;
                //farEdgeHeight -= 100;

                dx = roadSegments[i].Curve;

                if (farEdgeHeight > highestScreenLineDrawn && farEdgeHeight < numScreenLines) {
                    if (farEdgeHeight > nearEdgeHeight) {
                        for (int j = nearEdgeHeight; j <= farEdgeHeight; j++) {
                            float dxFraction = (j - nearEdgeHeight) * (dx / (farEdgeHeight - nearEdgeHeight));
                            float scaleIncreaseFromNearEdge =
                                (j - nearEdgeHeight) * ((farEdgeWidthScale - nearEdgeWidthScale) / (farEdgeHeight - nearEdgeHeight));

                            if (j >= 0) {
                                roadScreenSprites[j].sprite = roadSegments[i].SpriteVariation;
                                roadScreenLines[j].position =
                                    new Vector3(x + dxFraction, roadScreenLines[j].position.y, roadScreenLines[j].position.z);
                                roadScreenLines[j].localScale = new Vector3(nearEdgeWidthScale + scaleIncreaseFromNearEdge, 1.0f, 1.0f);
//Debug.Log(roadScreenLines[j].localScale.x);
                            }
                        }
//Debug.Log("thick seg, screen line #" + nearEdgeHeight);
                    }
                    else {
                        roadScreenSprites[farEdgeHeight].sprite = roadSegments[i].SpriteVariation;
                        roadScreenLines[farEdgeHeight].position =
                            new Vector3(x + dx, roadScreenLines[farEdgeHeight].position.y, roadScreenLines[farEdgeHeight].position.z);
                        roadScreenLines[farEdgeHeight].localScale = new Vector3(farEdgeWidthScale, 1.0f, 1.0f);
//Debug.Log("single seg, screen line #" + nearEdgeHeight);
                    }
                }

                if ((Mathf.FloorToInt(nearEdgeHeight * roadLineToWarningRoadLineConversion) == curWarningRoadLineIndex) && 
                        (curWarningRoadLineIndex < warningRoadLines.Count - 1)) {

                    Vector2 lineBasePos = warningRoadLines[curWarningRoadLineIndex].rectTransform.anchoredPosition;
                    warningRoadLines[curWarningRoadLineIndex].rectTransform.anchoredPosition =
                        new Vector2(warningRoadLinesInitialX + x * roadLineToWarningRoadLineConversion, lineBasePos.y);
                    warningRoadLines[curWarningRoadLineIndex].rectTransform.sizeDelta =
                        new Vector2(warningRoadLineBaseWidth * nearEdgeWidthScale, 1.0f);
                    curWarningRoadLineIndex++;
                }

                if (segNumDrawn < segIndexSkipSegThreshold) {
                    if (visibleBillboards.Count > 0) {
                        foreach (BillboardSprite bb in visibleBillboards) {
                            if (bb.segmentIndex == i) {
                                float roadWidthOffset;
                                
                                if (bb.offsetX < 0) {
                                    roadWidthOffset = screenHalfWidth * -1;
                                }
                                else {
                                    roadWidthOffset = screenHalfWidth;
                                }

                                float sizeCorrection = nearEdgeWidthScale * billboardSizeCorrectionFactor;

                                float spriteZ;
                                float spriteElevation = nearEdgeHeight + bb.spriteType.rect.height * nearEdgeWidthScale * 0.5f;

                                if (nearEdgeHeight < absHighestScreenLineDrawn && roadSegIndexOfHighestScreenLine < i) {
                                    spriteZ = spriteBgdZ;
                                }
                                else {
                                    spriteZ = -1f;
                                }

                                bb.spriteTransform.localScale = new Vector3(sizeCorrection, sizeCorrection, 1);
                                bb.spriteTransform.position = new Vector3(x + (bb.offsetX) * nearEdgeWidthScale, spriteElevation, spriteZ);
                                bb.spriteRend.enabled = true;
                            }
                        }
                    }

                    if (visibleDroids.Count > 0) {
                        foreach (BombDroid bd in visibleDroids) {
                            if (bd.segmentIndex == i) {
                                float roadWidthOffset;

                                if (bd.offsetX < 0) {
                                    roadWidthOffset = screenHalfWidth * -1;
                                }
                                else {
                                    roadWidthOffset = screenHalfWidth;
                                }

                                float sizeCorrection = nearEdgeWidthScale * billboardSizeCorrectionFactor;

                                bd.droid.localScale = new Vector3(sizeCorrection, sizeCorrection, 1);
                                bd.droid.position = new Vector3(x + (bd.offsetX) * nearEdgeWidthScale,
                                    nearEdgeHeight + (droidHeight + bd.droidSprite.sprite.rect.height) * nearEdgeWidthScale * 0.5f, -1f);
                                bd.droidSprite.enabled = true;
                            }

                            //if ((Time.time - bd.dropDelayRefTime > delayBeforeDropping) && bd.droidSprite.sprite == droidHolding) {
                            if ((bd.segmentIndex < curSegmentIndex + 200) && bd.droidSprite.sprite == droidHolding) {
//TODO: make prefab for bomb + image objects
                                bd.droidSprite.sprite = droidEmptyHanded;
                                Bomb bmb = new Bomb();
                                GameObject obj = new GameObject("Bomb Sprite Renderer");

                                GameObject imgObj = new GameObject("Bomb Image");
                                RawImage img = imgObj.AddComponent<RawImage>();
                                imgObj.transform.SetParent(warningRoadLinesParent);
                                bmb.blip = img;
                                bmb.blip.texture = warningRoadBombBlipSprite;
                                bmb.blip.rectTransform.anchoredPosition = warningRoadLines[0].rectTransform.anchoredPosition;
                                bmb.blip.rectTransform.sizeDelta = Vector2.zero;
                                bmb.blip.enabled = true;

                                GameObject imgProjObj = new GameObject("Bomb Image Projection");
                                RawImage imgProj = imgProjObj.AddComponent<RawImage>();
                                imgProjObj.transform.SetParent(warningRoadLinesParent);
                                bmb.blipProjection = imgProj;
                                bmb.blipProjection.texture = warningRoadBombBlipSprite;
                                bmb.blipProjection.rectTransform.anchoredPosition =
                                    warningRoadLines[0].rectTransform.anchoredPosition + new Vector2(0.0f, bombBlipProjectionRoadOffsetY);
                                bmb.blipProjection.rectTransform.sizeDelta = Vector2.zero;
                                bmb.blipProjection.enabled = true;
                                bmb.blipProjectionFlashRefTime = Time.time;

                                obj.AddComponent<SpriteRenderer>().enabled = false;
                                bmb.segmentIndex = bd.segmentIndex;
                                bmb.singleBomb = obj.transform;
                                bmb.offsetX = bd.offsetX;
                                bmb.isExploding = false;
                                bmb.explosionHeightOffset = 50f;
                                bmb.bombSprite = obj.GetComponent<SpriteRenderer>();
                                bmb.bombSprite.sprite = bombCamo;
                                bmb.bombSprite.enabled = true;
                                bmb.explosionFrameIndex = 0;

                                activeBombPool.Add(bmb);
                            }
                        }
                    }

                    if (activeBombPool.Count > 0) {
                        foreach (Bomb bmb in activeBombPool) {
                            if (bmb.isExploding) {
                                bmb.segmentIndex = curSegmentIndex + 5;

                                if (Time.time - bmb.explosionRefTime > bombExplosionFrameTime) {
                                    if (bmb.explosionFrameIndex < curExplosionFrames.Count - 1) {
                                        bmb.explosionFrameIndex++;
                                        bmb.bombSprite.sprite = curExplosionFrames[bmb.explosionFrameIndex];
                                        bmb.explosionRefTime = Time.time;
                                    }
                                    else {
                                        bmb.isExploding = false;
                                        bmb.bombSprite.enabled = false;
                                    }
                                }
                            }

                            if (bmb.segmentIndex == i) {
                                float roadWidthOffset;

                                if (bmb.offsetX < 0) {
                                    roadWidthOffset = screenHalfWidth * -1;
                                }
                                else {
                                    roadWidthOffset = screenHalfWidth;
                                }

                                float sizeCorrection = nearEdgeWidthScale * bombSizeCorrectionFactor; //* billboardSizeCorrectionFactor;

                                bmb.singleBomb.localScale = new Vector3(sizeCorrection, sizeCorrection, 1);
                                bmb.singleBomb.position = new Vector3(x + (bmb.offsetX) * nearEdgeWidthScale,
                                    nearEdgeHeight + (bmb.bombSprite.sprite.rect.height + bmb.explosionHeightOffset) * nearEdgeWidthScale * 0.5f,
                                    -1.5f);

                                if (bmb.isExploding) {     // This will prevent the sprite from re-enabling after the explosion ends
                                    bmb.bombSprite.enabled = true;
                                }

                                bmb.blip.rectTransform.sizeDelta = new Vector2(bombBlipBaseWidth, bombBlipBaseHeight);
                                int height = Mathf.FloorToInt(nearEdgeHeight * roadLineToWarningRoadLineConversion);

                                if (height < warningRoadLines.Count && height > 0) {
                                    float blipY = warningRoadLines[height].rectTransform.anchoredPosition.y;

                                    bmb.blip.rectTransform.anchoredPosition =
                                        new Vector2(warningRoadLinesInitialX + (bmb.offsetX * nearEdgeWidthScale + x) * roadLineToWarningRoadLineConversion,
                                        blipY);
                                }

                                bmb.blipProjection.rectTransform.sizeDelta = new Vector2(bombBlipBaseWidth, bombBlipBaseHeight);
                                bmb.blipProjection.rectTransform.anchoredPosition = warningRoadLines[0].rectTransform.anchoredPosition +
                                    new Vector2(bmb.offsetX * roadLineToWarningRoadLineConversion, bombBlipProjectionRoadOffsetY);

                                if (Time.time - bmb.blipProjectionFlashRefTime > bombBlipProjectionFlashTime) {
                                    bmb.blipProjection.enabled = !bmb.blipProjection.enabled;
                                    bmb.blipProjectionFlashRefTime = Time.time;
                                }

                                if (!isInvulnerable && canDrive) {
                                    if (i == curSegmentIndex + 5 && Mathf.Abs(bmb.singleBomb.position.x - playerCar.position.x) < 30) {
                                        if (bmb.singleBomb.position.x >= playerCar.position.x) {
                                            collisionSpeedEffect =
                                                new Vector3(maxBombIntensity / (Mathf.Max(playerCar.position.x - bmb.singleBomb.position.x, -1)),
                                                -curPlayerSpeed * 0.5f);
                                            playerIsRollingLeft = true;
                                        }
                                        else {
                                            collisionSpeedEffect = new Vector3(maxBombIntensity / (Mathf.Min(playerCar.position.x - bmb.singleBomb.position.x, 1)),
                                                -curPlayerSpeed * 0.5f);
                                            playerIsRollingRight = true;
                                        }

                                        canDrive = false;
                                        isCrashing = true;
                                        isInvulnerable = true;
                                        bmb.isExploding = true;
                                        crashRefTime = Time.time;
                                        playerCarAnimationRefTime = Time.time;
                                        playerCarAnimationFrameTimeAdjusted = playerCarAnimationFrameTime;
                                        playerCarAnimationIndex = 0;

                                        GameMode = GameState.Crashing;
                                    }
                                }
                            }
                        }
                    }

                    if (carsOnRoad.Count > 0) {
                        foreach (NPCar car in carsOnRoad) {
                            if (car.GetCurSeg() == i) {
                                car.SetVisibility(true, sensorOverlayIsActive);
                                car.UseRedVariants(sensorOverlayIsActive);

                                if (car.GetCurSeg() > curSegmentIndex) {
                                    float rotationFactor = roadSegments[car.GetCurSeg()].Curve * 50f + 
                                        (playerCar.position.x - car.transform.position.x) / (Mathf.Abs(car.GetCurSeg() - curPlayerPosZ) + 10f);
                                }

                                if (car.transform.GetComponent<SpriteRenderer>().sprite != null) {
                                    float depthZ;
                                    //if (sensorOverlayIsActive && !car.model.isSensorDetectable) { depthZ = sensorUndetectableDepthZ; }
                                    //else                                                        { depthZ = sensorDetectableDepthZ; }

                                    car.SetViewAngle(roadSegments[car.GetCurSeg()].Curve * 50f, 0, curPlayerPosZ);

                                    depthZ = sensorDetectableDepthZ;

                                    car.transform.localScale = new Vector3(nearEdgeWidthScale * 2, nearEdgeWidthScale * 2, 1);
                                    car.transform.position = new Vector3(x + (car.GetCurPosX()) * nearEdgeWidthScale,
                                        nearEdgeHeight + car.transform.GetComponent<SpriteRenderer>().sprite.rect.height * nearEdgeWidthScale * 0.5f,
                                        depthZ);
                                }

                                car.GetBlip().rectTransform.sizeDelta = new Vector2(bombBlipBaseWidth, bombBlipBaseHeight);
                                int height = Mathf.FloorToInt(nearEdgeHeight * roadLineToWarningRoadLineConversion);

                                if (height < warningRoadLines.Count && height >= 0) {
                                    float blipY = warningRoadLines[height].rectTransform.anchoredPosition.y;

                                    car.GetBlip().rectTransform.anchoredPosition = 
                                        new Vector2(warningRoadLinesInitialX + (car.GetCurPosX() * nearEdgeWidthScale + x) * roadLineToWarningRoadLineConversion,
                                        blipY);
                                }

                                // Check for collision with player car
                                if (!isInvulnerable && canDrive) {
                                    float halfCarLength = car.GetLength() * 0.5f;

                                    if (i < (curSegmentIndex + halfCarLength) && i > (curSegmentIndex - halfCarLength)) {
                                        if (Mathf.Abs(car.transform.position.x - playerCar.position.x) < car.GetWidth()) {
                                            Vector2 impactMomentum = 
                                                car.InitiateCrash(mass, roadGrip,
                                                    new Vector2(curPlayerTurning, curPlayerSpeed / roadSegmentLength),
                                                    new Vector2(playerCar.position.x, curSegmentIndex));
                                            impactMomentum = new Vector2(impactMomentum.x, impactMomentum.y * roadSegmentLength);
                                            bool triggerSpinout = impactMomentum.magnitude > impactThresholdForSpinout;

                                            if (triggerSpinout) {
                                                UpdateArmorValue(damageIncurredBySpinoutCrash);
                                            }
                                            else {
                                                UpdateArmorValue(damageIncurredByCrashing);
                                            }

                                            Spark newSpark = new Spark();
                                            GameObject obj = new GameObject();
                                            newSpark.sparkInstance = obj.transform;
                                            newSpark.sparkSprite = obj.AddComponent<SpriteRenderer>();
                                            newSpark.curSparkFrameIndex = 0;

                                            if (i >= curSegmentIndex) {
                                                if (car.transform.position.x >= playerCar.position.x) {
                                                    playerIsSpinningOutLeft = triggerSpinout;
                                                    playerIsCrashingLeft = !triggerSpinout;

                                                    if (Mathf.Abs(car.GetCurPosX() - playerCar.position.x) >
                                                            Mathf.Abs(car.GetCurSeg() - curSegmentIndex) * roadSegmentLength) {
                                                        newSpark.SparkSide = Spark.SparkDirection.Right;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.right * playerSparkRightOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkRightFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                    else {
                                                        newSpark.SparkSide = Spark.SparkDirection.Front;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.up * playerSparkFrontOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkFrontFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                }
                                                else {
                                                    playerIsSpinningOutRight = triggerSpinout;
                                                    playerIsCrashingRight = !triggerSpinout;

                                                    if (Mathf.Abs(car.GetCurPosX() - playerCar.position.x) >
                                                            Mathf.Abs(car.GetCurSeg() - curSegmentIndex) * roadSegmentLength) {
                                                        newSpark.SparkSide = Spark.SparkDirection.Left;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.right * playerSparkLeftOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkLeftFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                    else {
                                                        newSpark.SparkSide = Spark.SparkDirection.Front;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + Vector3.up * playerSparkFrontOffset;
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkFrontFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                }
                                            }
                                            else {
                                                if (car.transform.position.x >= playerCar.position.x) {
                                                    playerIsSpinningOutRight = triggerSpinout;
                                                    playerIsCrashingRight = !triggerSpinout;

                                                    if (Mathf.Abs(car.GetCurPosX() - playerCar.position.x) >
                                                            Mathf.Abs(car.GetCurSeg() - curSegmentIndex) * roadSegmentLength) {
                                                        newSpark.SparkSide = Spark.SparkDirection.Right;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.right * playerSparkRightOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkRightFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                    else {
                                                        newSpark.SparkSide = Spark.SparkDirection.Back;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.up * playerSparkBackOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkBackFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                }
                                                else {
                                                    if (triggerSpinout) { playerIsSpinningOutLeft = true; }
                                                    else { playerIsCrashingLeft = true; }

                                                    if (Mathf.Abs(car.GetCurPosX() - playerCar.position.x) >
                                                            Mathf.Abs(car.GetCurSeg() - curSegmentIndex) * roadSegmentLength) {
                                                        newSpark.SparkSide = Spark.SparkDirection.Left;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.right * playerSparkLeftOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkLeftFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                    else {
                                                        newSpark.SparkSide = Spark.SparkDirection.Back;
                                                        newSpark.sparkInstance.position =
                                                            playerCar.position + (Vector3.up * playerSparkBackOffset);
                                                        newSpark.sparkInstance.SetParent(playerCar);
                                                        newSpark.sparkSprite.sprite = curSparkBackFrames[newSpark.curSparkFrameIndex];
                                                    }
                                                }
                                            }

                                            playerSparks.Add(newSpark);
                                            newSpark.sparkFrameRefTime = Time.time;

                                            canDrive = false;
                                            isCrashing = true;
                                            isInvulnerable = true;
                                            crashRefTime = Time.time;
                                            playerCarAnimationRefTime = Time.time;
                                            playerCarAnimationFrameTimeAdjusted = playerCarAnimationFrameTime;
                                            playerCarAnimationIndex = 0;

                                            GameMode = GameState.Crashing;
                                        }
                                    }
                                }
                            }
                            else if (car.GetCurSeg() < curSegmentIndex || car.GetCurSeg() > curSegmentIndex + numSegsToDraw) {
                                if (car.transform != null) {
                                    //Destroy(car.trafficCar.gameObject);
                                    //Destroy(car.blip);

                                    //car.trafficCar = null;
                                    //car.carSprite = null;

                                    car.SetVisibility(false);
                                }
                            }
                        }
                    }
                }

                x += dx;
                highestScreenLineDrawn = Mathf.Max(farEdgeHeight, highestScreenLineDrawn);
//Debug.Log(highestScreenLineDrawn);
            }
        }

        for (int i = highestScreenLineDrawn + 1; i < roadScreenLines.Count - 1; i++) {
            roadScreenLines[i].localScale = Vector3.up + Vector3.forward;
        }

        for (int i = curWarningRoadLineIndex; i < warningRoadLines.Count; i++) {
            warningRoadLines[i].rectTransform.sizeDelta = Vector2.up;
        }

        curWarningRoadLineIndex = 0;

        //foreach (BillboardSprite bb in visibleBillboards) {
        //    if (bb.spriteTransform.position.y < highestScreenLineDrawn) {
        //        bb.spriteTransform.position = new Vector3(bb.spriteTransform.position.x, bb.spriteTransform.position.y, spriteBgdZ);
        //    }
        //}

        roadTopLineCoverStrip.position =
            new Vector3(roadTopLineCoverStrip.position.x, highestScreenLineDrawn + 0.5f, roadTopLineCoverStrip.position.z);
        bgd.position = new Vector3(bgd.position.x, highestScreenLineDrawn + bgdOffset + 0.5f, bgd.position.z);

        float progressTrackLength = progressTrack.rectTransform.rect.height;
        Vector2 pcPos = progressCar.rectTransform.anchoredPosition;
        float pcPosOffset = (progressTrackLength / roadSegments.Count) * curSegmentIndex;

        progressCar.rectTransform.anchoredPosition = new Vector2(progressCar.rectTransform.anchoredPosition.x, progressCarInitialY + pcPosOffset);
    }

    // This method is used when first loading the road to see if there are any sprites in the immediate vicinity (not just at the visibility edge)
    private void LoadOpeningBillboards() {
        foreach (BillboardSprite bb in billboardDataExpanded) {
            if (bb.segmentIndex <= curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject("Billboard Sprite Renderer");
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
        for (int i = 0; i < billboardDataExpanded.Count; i++) {
            if (billboardDataExpanded[i].segmentIndex == curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject("BillBoard Expanded Renderer");
                obj.AddComponent<SpriteRenderer>();
                billboardDataExpanded[i].spriteTransform = obj.transform;
                billboardDataExpanded[i].spriteRend = obj.GetComponent<SpriteRenderer>();
                billboardDataExpanded[i].spriteRend.enabled = false;
                billboardDataExpanded[i].spriteRend.sprite = billboardDataExpanded[i].spriteType;

                visibleBillboards.Add(billboardDataExpanded[i]);
                billboardDataExpanded.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < visibleBillboards.Count; i++) {
            if (visibleBillboards[i].segmentIndex < curSegmentIndex) {
                Destroy(visibleBillboards[i].spriteTransform.gameObject);
                visibleBillboards.RemoveAt(i);
                i--;
            }
        }
    }

    private void LoadOpeningEnemies() {
        foreach (BombDroid bd in droids) {
            if (bd.segmentIndex <= curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject();
                obj.AddComponent<SpriteRenderer>();
                bd.droid = obj.transform;
                bd.droidSprite = obj.GetComponent<SpriteRenderer>();
                bd.droidSprite.enabled = false;
                bd.droidSprite.sprite = droidHolding;


                visibleDroids.Add(bd);
Debug.Log(bd.droid);
            }
        }

        if (trafficCars.Count > 0) {
            enemyModels.Clear();
            innocentModels.Clear();

            foreach (CarModel mod in trafficCars) {
                if (mod.Behavior == CarModel.BehaviorType.Attacker) {
                    enemyModels.Add(mod);
                }
                else {
                    innocentModels.Add(mod);
                }
            }

            if (enemyModels.Count > 0) {
                for (int i = 0; i < numEnemyModels; i++) {
                    GameObject obj = new GameObject("Enemy");
                    obj.AddComponent<SpriteRenderer>();
                    NPCar car = obj.AddComponent<NPCar>();
                    
                    car.InitializeCar(enemyModels[Random.Range(0, enemyModels.Count)],
                                        Random.Range(0, roadSegments.Count),
                                        roadSegmentLength,
                                        laneWidth,
                                        warningRoadCarBlipSprite,
                                        warningRoadLines[0].rectTransform.anchoredPosition,
                                        warningRoadLinesParent);
                    car.SetVisibility(false);
                    car.UseRedVariants(sensorOverlayIsActive);

                    carsOnRoad.Add(car);
                    //SetOtherCarSprites(car);
                }
            }

Debug.Log("# innocent cars: " + innocentModels.Count);

            if (innocentModels.Count > 0) {
                for (int i = 0; i < numInnocentModels; i++) {
                    GameObject obj = new GameObject("NPC");
                    obj.AddComponent<SpriteRenderer>();
                    NPCar car = obj.AddComponent<NPCar>();
                    
                    car.InitializeCar(innocentModels[Random.Range(0, innocentModels.Count)],
                                        Random.Range(0, roadSegments.Count),
                                        roadSegmentLength,
                                        laneWidth,
                                        warningRoadCarBlipSprite,
                                        warningRoadLines[0].rectTransform.anchoredPosition,
                                        warningRoadLinesParent);
                    car.SetVisibility(false);
                    car.UseRedVariants(sensorOverlayIsActive);

                    carsOnRoad.Add(car);
//Debug.Log(car.curSegIndex + ", " + car.curPosZ + ", " + car.curForwardSpeed + ", " + car);
                }
            }
        }
    }

    private void UpdateEnemies() {
        for (int i = 0; i < droids.Count; i++) {
            if (droids[i].segmentIndex == curSegmentIndex + numSegsToDraw) {
                GameObject obj = new GameObject();
                obj.AddComponent<SpriteRenderer>();
                droids[i].droid = obj.transform;
                droids[i].droidSprite = obj.GetComponent<SpriteRenderer>();
                droids[i].droidSprite.enabled = false;
                droids[i].droidSprite.sprite = droidHolding;

                droids[i].dropDelayRefTime = Time.time;
                visibleDroids.Add(droids[i]);
                droids.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < visibleDroids.Count; i++) {
            if (visibleDroids[i].segmentIndex < curSegmentIndex) {
                Destroy(visibleDroids[i].droidSprite.gameObject);
                visibleDroids.RemoveAt(i);
                i--;
            }
        }

        if (activeBombPool.Count > 0) {
            for (int i = 0; i < activeBombPool.Count; i++) {
                if (activeBombPool[i].segmentIndex < curSegmentIndex) {
                    Destroy(activeBombPool[i].singleBomb.gameObject);
                    Destroy(activeBombPool[i].blip.gameObject);
                    Destroy(activeBombPool[i].blipProjection.gameObject);
                    activeBombPool.RemoveAt(i);
                    i--;
                }
            }
        }

        //List<NPCar> carsToDelete = new List<NPCar>();

        //if (carsOnRoad.Count > 0) {
        //    foreach (NPCar car in carsOnRoad) {
        //        //car.curPosZ += car.curForwardSpeed * Time.deltaTime;
        //        //car.curPosX += car.curLaneChangeSpeed * Time.deltaTime;
        //        //car.curSegIndex = (int)(car.curPosZ / roadSegmentLength);

        //        //if (car.curSegIndex < curSegmentIndex || car.curSegIndex > curSegmentIndex + numSegsToDraw) {

        //        //    if (car.trafficCar != null) {
        //        //        carsOnRoad.Remove(car);
        //        //        Destroy(car.trafficCar.gameObject);

        //        //        car.trafficCar = null;
        //        //        car.carSprite = null;

        //        //        car.isVisible = false;
        //        //    }
        //        //}
        //    }
        //}
    }

    private void SetPlayerCarSprites() {
        if (sensorOverlayWasJustActivated) {
            curPlayerCarStraight = playerCarStraightRed;
            curPlayerCarSlightLeft = playerCarSlightLeftRed;
            curPlayerCarLeft = playerCarLeftRed;
            curPlayerCarHardLeft = playerCarHardLeftRed;
            curPlayerCarSlightRight = playerCarSlightRightRed;
            curPlayerCarRight = playerCarRightRed;
            curPlayerCarHardRight = playerCarHardRightRed;
            
            curPlayerCarSpinoutLeftFrames = redPlayerCarSpinoutLeftFrames;
            curPlayerCarSpinoutRightFrames = redPlayerCarSpinoutRightFrames;
            curPlayerCarCrashLeftFrames = redPlayerCarCrashLeftFrames;
            curPlayerCarCrashRightFrames = redPlayerCarCrashRightFrames;
            curPlayerCarExplodeFrames = redPlayerCarExplodeFrames;
            curPlayerCarRollLeftFrames = redPlayerCarRollLeftFrames;
            curPlayerCarRollRightFrames = redPlayerCarRollRightFrames;
        }
        else {
            curPlayerCarStraight = playerCarStraight;
            curPlayerCarSlightLeft = playerCarSlightLeft;
            curPlayerCarLeft = playerCarLeft;
            curPlayerCarHardLeft = playerCarHardLeft;
            curPlayerCarSlightRight = playerCarSlightRight;
            curPlayerCarRight = playerCarRight;
            curPlayerCarHardRight = playerCarHardRight;
            
            curPlayerCarSpinoutLeftFrames = playerCarSpinoutLeftFrames;
            curPlayerCarSpinoutRightFrames = playerCarSpinoutRightFrames;
            curPlayerCarCrashLeftFrames = playerCarCrashLeftFrames;
            curPlayerCarCrashRightFrames = playerCarCrashRightFrames;
            curPlayerCarExplodeFrames = playerCarExplodeFrames;
            curPlayerCarRollLeftFrames = playerCarRollLeftFrames;
            curPlayerCarRollRightFrames = playerCarRollRightFrames;
        }
    }

    //private void SetOtherCarSprites (NPCar otherCar) {
    //    if (sensorOverlayWasJustActivated) {
    //        otherCar.curStraightOverhead = otherCar.model.redStraightOverhead;
    //        otherCar.curSlightLeft = otherCar.model.redSlightLeft;
    //        otherCar.curLeft = otherCar.model.redLeft;
    //        otherCar.curHardLeft = otherCar.model.redHardLeft;
    //        otherCar.curSlightRight = otherCar.model.redSlightRight;
    //        otherCar.curRight = otherCar.model.redRight;
    //        otherCar.curHardRight = otherCar.model.redHardRight;

    //        //otherCar.curSpinLeftFrames.Clear();
    //        //otherCar.curSpinLeftFrames = otherCar.model.redSpinLeftFrames;
    //        //otherCar.curSpinRightFrames.Clear();
    //        //otherCar.curSpinRightFrames = otherCar.model.redSpinRightFrames;
    //        //otherCar.curCrashFrames.Clear();
    //        //otherCar.curCrashFrames = otherCar.model.redCrashFrames;
    //        //otherCar.curExplodeFrames.Clear();
    //        //otherCar.curExplodeFrames = otherCar.model.redExplodeFrames;
    //    }
    //    else {
    //        otherCar.curStraightOverhead = otherCar.model.straightOverhead;
    //        otherCar.curSlightLeft = otherCar.model.slightLeft;
    //        otherCar.curLeft = otherCar.model.left;
    //        otherCar.curHardLeft = otherCar.model.hardLeft;
    //        otherCar.curSlightRight = otherCar.model.slightRight;
    //        otherCar.curRight = otherCar.model.right;
    //        otherCar.curHardRight = otherCar.model.hardRight;

    //        //otherCar.curSpinLeftFrames.Clear();
    //        //otherCar.curSpinLeftFrames = otherCar.model.spinLeftFrames;
    //        //otherCar.curSpinRightFrames.Clear();
    //        //otherCar.curSpinRightFrames = otherCar.model.spinRightFrames;
    //        //otherCar.curCrashFrames.Clear();
    //        //otherCar.curCrashFrames = otherCar.model.crashFrames;
    //        //otherCar.curExplodeFrames.Clear();
    //        //otherCar.curExplodeFrames = otherCar.model.explodeFrames;
    //    }
    //}

    private void SetEffectSprites() {
        if (sensorOverlayWasJustActivated) {
            curSparkFrontFrames = sparkFrontFramesRed;
            curSparkBackFrames = sparkBackFramesRed;
            curSparkLeftFrames = sparkLeftFramesRed;
            curSparkRightFrames = sparkRightFramesRed;

            curSmokeFrontFrames = smokeFrontFramesRed;
            curSmokeLeftFrames = smokeLeftFramesRed;
            curSmokeRightFrames = smokeRightFramesRed;

            curExplosionFrames = explosionFramesRed;
        }
        else {
            curSparkFrontFrames = sparkFrontFrames;
            curSparkBackFrames = sparkBackFrames;
            curSparkLeftFrames = sparkLeftFrames;
            curSparkRightFrames = sparkRightFrames;

            curSmokeFrontFrames = smokeFrontFrames;
            curSmokeLeftFrames = smokeLeftFrames;
            curSmokeRightFrames = smokeRightFrames;

            curExplosionFrames = explosionFrames;
        }
    }

    private void UpdateSparks() {
        Spark.SparkDirection sparkSide;

        for (int i = 0; i < playerSparks.Count; i++) {
            if (Time.time - playerSparks[i].sparkFrameRefTime > sparkFrameTime) {
                playerSparks[i].sparkFrameRefTime = Time.time;
                sparkSide = playerSparks[i].SparkSide;

                switch (sparkSide) {
                    case Spark.SparkDirection.Front:
                        if (playerSparks[i].curSparkFrameIndex < curSparkFrontFrames.Count - 1) {
                            playerSparks[i].curSparkFrameIndex++;
                            playerSparks[i].sparkSprite.sprite = curSparkFrontFrames[playerSparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(playerSparks[i].sparkInstance.gameObject);
                            playerSparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Back:
                        if (playerSparks[i].curSparkFrameIndex < curSparkBackFrames.Count - 1) {
                            playerSparks[i].curSparkFrameIndex++;
                            playerSparks[i].sparkSprite.sprite = curSparkBackFrames[playerSparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(playerSparks[i].sparkInstance.gameObject);
                            playerSparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Left:
                        if (playerSparks[i].curSparkFrameIndex < curSparkLeftFrames.Count - 1) {
                            playerSparks[i].curSparkFrameIndex++;
                            playerSparks[i].sparkSprite.sprite = curSparkLeftFrames[playerSparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(playerSparks[i].sparkInstance.gameObject);
                            playerSparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Right:
                        if (playerSparks[i].curSparkFrameIndex < curSparkRightFrames.Count - 1) {
                            playerSparks[i].curSparkFrameIndex++;
                            playerSparks[i].sparkSprite.sprite = curSparkRightFrames[playerSparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(playerSparks[i].sparkInstance.gameObject);
                            playerSparks.RemoveAt(i);
                            i--;
                        }
                    break;
                }
            }
        }

        for (int i = 0; i < sparks.Count; i++) {
            if (Time.time - sparks[i].sparkFrameRefTime > sparkFrameTime) {
                sparks[i].sparkFrameRefTime = Time.time;
                sparkSide = sparks[i].SparkSide;

                switch (sparkSide) {
                    case Spark.SparkDirection.Front:
                        if (sparks[i].curSparkFrameIndex < curSparkFrontFrames.Count - 1) {
                            sparks[i].curSparkFrameIndex++;
                            sparks[i].sparkSprite.sprite = curSparkFrontFrames[sparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(sparks[i].sparkInstance.gameObject);
                            sparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Back:
                        if (sparks[i].curSparkFrameIndex < curSparkBackFrames.Count - 1) {
                            sparks[i].curSparkFrameIndex++;
                            sparks[i].sparkSprite.sprite = curSparkBackFrames[sparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(sparks[i].sparkInstance.gameObject);
                            sparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Left:
                        if (sparks[i].curSparkFrameIndex < curSparkLeftFrames.Count - 1) {
                            sparks[i].curSparkFrameIndex++;
                            sparks[i].sparkSprite.sprite = curSparkLeftFrames[sparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(sparks[i].sparkInstance.gameObject);
                            sparks.RemoveAt(i);
                            i--;
                        }
                    break;

                    case Spark.SparkDirection.Right:
                        if (sparks[i].curSparkFrameIndex < curSparkRightFrames.Count - 1) {
                            sparks[i].curSparkFrameIndex++;
                            sparks[i].sparkSprite.sprite = curSparkRightFrames[sparks[i].curSparkFrameIndex];
                        }
                        else {
                            Destroy(sparks[i].sparkInstance.gameObject);
                            sparks.RemoveAt(i);
                            i--;
                        }
                    break;
                }
            }
        }
    }

    private void AddRoadCurve(RoadCurve rdCurve) {
        float startY = roadSegments[roadSegments.Count - 1].Y;
        float endY = startY + rdCurve.elevationShift * roadSegmentLength;
        float total = rdCurve.numEnterSegments + rdCurve.numHoldSegments + rdCurve.numExitSegments;
        float decI;

        for (int i = 0; i < rdCurve.numEnterSegments; i++) {
            CheckRumbleStripCounter();
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            decI = (float)i;
            seg.Curve = EaseIn(0, rdCurve.curveIntensity, (float)(decI / rdCurve.numEnterSegments));
            seg.Y = EaseIn(startY, endY, (float)(decI / total));
            seg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(seg);
        }
        for (int i = 0; i < rdCurve.numHoldSegments; i++) {
            CheckRumbleStripCounter();
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            decI = (float)i;
            seg.Curve = rdCurve.curveIntensity;
            seg.Y = EaseInOut(startY, endY, ((float)((decI + rdCurve.numEnterSegments) / total)));
            seg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(seg);
        }
        for (int i = 0; i < rdCurve.numExitSegments; i++) {
            CheckRumbleStripCounter();
            RoadSegment seg = new RoadSegment();
            seg.EdgeNearZ = curAddedSegmentZ;
            IncrementSegmentsAndRumbleStrips();
            seg.EdgeFarZ = curAddedSegmentZ;
            decI = (float)i;
            seg.Curve = EaseOut(rdCurve.curveIntensity, 0, (float)(decI / rdCurve.numExitSegments));
            seg.Y = EaseOut(startY, endY, ((float)((decI + rdCurve.numEnterSegments + rdCurve.numHoldSegments) / total)));
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

    // EASING FUNCTIONS
    private float EaseIn(float start, float end, float percent) {
        return start + (end - start) * percent * percent;
    }
    private float EaseInOut(float start, float end, float percent) {
        return start + (end - start) * (-Mathf.Cos(percent * Mathf.PI) * 0.5f + 0.5f);
    }
    private float EaseOut(float start, float end, float percent) {
        return start + (end - start) * (1.0f - ((1.0f - percent) * (1.0f - percent)));

        //return start + (end - start) * -(Mathf.Cos(Mathf.PI * percent) - 1.0f) / 2.0f;
        //return start + (end - start) * Mathf.Sin(Mathf.PI * percent) / 2.0f;
    }

    private void IncrementSegmentsAndRumbleStrips() {
        curAddedSegmentZ += roadSegmentLength;
        rumbleStripCounter++;
    }

    private void UpdateSensorModeVisuals() {
        if (Time.time - sensorOverlayFrameRefTime > sensorOverlayFrameTime) {
            if (sensorOverlayIsLoading) {
                if (curSensorOverlayFrameIndex < sensorOverlayLoadFrames.Count - 1) {
                    curSensorOverlayFrameIndex++;
                    sensorOverlay.sprite = sensorOverlayLoadFrames[curSensorOverlayFrameIndex];
                }

                if (curCenterConsoleFrameIndex < centerConsoleCoverFrames.Count - 1) {
                    curCenterConsoleFrameIndex++;
                    centerConsoleCover.texture = centerConsoleCoverFrames[curCenterConsoleFrameIndex];
                }

                if ((curSensorOverlayFrameIndex >= sensorOverlayLoadFrames.Count - 1) &&
                    (curCenterConsoleFrameIndex >= centerConsoleCoverFrames.Count - 1)) {
                    sensorOverlayIsLoading = false;
                    sensorOverlayIsTotallyLoaded = true;
                    centerConsoleCover.enabled = false;
                    curSensorOverlayFrameIndex = 0;
                    sensorScanline.enabled = true;

                    if (warningRoadLines.Count > 0) {
                        foreach (RawImage img in warningRoadLines) {
                            img.enabled = true;
                        }
                    }
                }
            }
            else if (sensorOverlayIsTotallyLoaded) {
                if (sensorScanline.transform.position.y > sensorScanLineLowestHeight) {
                    //sensorScanline.transform.position = sensorScanline.transform.position +
                    //    new Vector3(0, sensorScanLineDisplacement * Time.deltaTime, 0);
                    sensorScanline.transform.position =
                        sensorScanline.transform.position + (Vector3.up * sensorScanLineDisplacement * Time.deltaTime);
                }
                else {
                    sensorScanline.transform.position =
                        new Vector3(sensorScanline.transform.position.x, sensorScanLineInitialHeight, sensorScanline.transform.position.z);
                }
            }
            else if (sensorOverlayIsUnloading) {
                if (curSensorOverlayFrameIndex < sensorOverlayUnloadFrames.Count - 1) {
                    curSensorOverlayFrameIndex++;
                    sensorOverlay.sprite = sensorOverlayUnloadFrames[curSensorOverlayFrameIndex];
                }

                if (curCenterConsoleFrameIndex > 0) {
                    curCenterConsoleFrameIndex--;
                    centerConsoleCover.texture = centerConsoleCoverFrames[curCenterConsoleFrameIndex];
                }

                if ((curSensorOverlayFrameIndex >= sensorOverlayUnloadFrames.Count - 1) && (curCenterConsoleFrameIndex <= 0)) {
                    sensorOverlayIsUnloading = false;
                    sensorOverlayIsActive = false;
                    sensorOverlay.enabled = false;

                    if (warningRoadLines.Count > 0) {
                        foreach (RawImage img in warningRoadLines) {
                            img.enabled = false;
                        }
                    }
                }
            }

            sensorOverlayFrameRefTime = Time.time;
        }
    }

    private void UpdateConsoleSensor() {
        if (sensorOverlayIsActive && sensorOverlayIsTotallyLoaded && curSensorPower > 0) {
            curSensorPower -= sensorDrainRate * Time.deltaTime;
        }
        else {
            if (curSensorPower < sensorMaxPower) {
                curSensorPower += sensorRechargeRate * Time.deltaTime;
            }
        }

        if (sensorBars.Count > 0  && !sensorOverlayIsActive) {
            float powerPerBar = sensorMaxPower / sensorBars.Count;
            //int numOfBarsToShow = Mathf.FloorToInt(curSensorPower / powerPerBar);
            int numOfBarsToShow = (int)(curSensorPower / powerPerBar);

            for (int i = 0; i < sensorBars.Count; i++) {
                sensorBars[i].enabled = i < numOfBarsToShow;
            }

            foreach (RawImage img in sensorBars) {
                if (((curSensorPower / sensorMaxPower) * 100.0f) < sensorLowPowerPercentageOfMax) {
                    img.color = sensorBarLowPowerColor;
                }
                else if ((((curSensorPower / sensorMaxPower) * 100.0f) >= sensorLowPowerPercentageOfMax) && (curSensorPower < sensorMaxPower)) {
                    img.color = sensorBarChargingColor;
                }
                else if (curSensorPower >= sensorMaxPower) {
                    img.color = sensorBarFullyChargedColor;
                }
            }
        }
    }

    private void UpdateArmorValue(float damageDone) {
        damageTaken += damageDone;
        curArmorValue = armorMaxValue - damageTaken;
        float damageBarWidth = armorBarWidth * (damageTaken / armorMaxValue);

        armorBarCutter.rectTransform.sizeDelta = new Vector2(damageBarWidth, armorBarCutter.rectTransform.rect.height);
        armorBarCutter.rectTransform.anchoredPosition =
            new Vector2(armorCutterInitialX + armorBarWidth - (damageBarWidth * 0.5f), armorBarCutter.rectTransform.anchoredPosition.y);
    }

    private void ManageControlsAndPlayerPosition() {
        float cumulativePlayerCarX;

        //curPlayerTurning = maxTurning;
        speedometerNeedle.rectTransform.rotation = Quaternion.Euler(0, 0, (curPlayerSpeed * speedometerRotationFactor) + 90f);

        if (canDrive) {
            if (inputMapper[(int)InputMapper.CONTROLS.up]) {
                curPlayerAcceleration = maxAcceleration;

                if (curPlayerSpeed < topSpeed) {
                    curPlayerSpeed += maxAcceleration * Time.deltaTime;
                }
                else {
                    curPlayerSpeed = topSpeed;
                }
            }
            else {
                curPlayerAcceleration = 0;

                if (curPlayerSpeed > 0) {
                    curPlayerSpeed -= deceleration * Time.deltaTime;
                }
                else {
                    curPlayerSpeed = 0.0f;
                }
            }

            if (inputMapper[(int)InputMapper.CONTROLS.left]) {
                if (inputMapper[(int)InputMapper.CONTROLS.action]) {
                    playerCarSprite.sprite = curPlayerCarHardLeft;

                    if (curPlayerTurning > -maxTurning * 1.3f) {
                        curPlayerTurning -= hardTurnIncrease * Time.deltaTime;
                    }
                }
                else {
                    //playerCarSprite.sprite = playerCarLeft;
                    if (curPlayerTurning > -maxTurning * 0.9f) {
                        curPlayerTurning -= turnIncrease * Time.deltaTime;
                    }
                }
            }
            else if (inputMapper[(int)InputMapper.CONTROLS.right]) {
                if (inputMapper[(int)InputMapper.CONTROLS.action]) {
                    playerCarSprite.sprite = playerCarHardRight;

                    if (curPlayerTurning < maxTurning * 1.3f) {
                        curPlayerTurning += hardTurnIncrease * Time.deltaTime;
                    }
                }
                else {
                    //playerCarSprite.sprite = playerCarRight;
                    if (curPlayerTurning < maxTurning * 0.9f) {
                        curPlayerTurning += turnIncrease * Time.deltaTime;
                    }
                }
            }
            else {
                if (Mathf.Abs(curPlayerTurning) > lowTurnZeroThreshold) {
                    if (curPlayerTurning > 0) {
                        curPlayerTurning -= turnDriftToCenterFactor * Time.deltaTime;
                    }
                    else {
                        curPlayerTurning += turnDriftToCenterFactor * Time.deltaTime;
                    }
                }
                else {
                    curPlayerTurning = 0.0f;
                }
            }

            //if (inputMapper[(int)InputMapper.CONTROLS.special]) {

            //    if (!sensorOverlayIsActive) {

            //        sensorOverlayIsActive = true;

            //        SetPlayerCarSprites();
            //        if (carsOnRoad.Count > 0) {
            //            foreach (NPCar car in carsOnRoad) {
            //                if (car.isVisible) { SetOtherCarSprites(car); }
            //            }
            //        }

            //        SetEffectSprites();

            //        sensorOverlayIsLoading = true;
            //        sensorOverlayFrameRefTime = Time.time;
            //        curSensorOverlayFrameIndex = 0;
            //        sensorOverlay.sprite = sensorOverlayLoadFrames[curSensorOverlayFrameIndex];
            //        sensorOverlay.enabled = true;
            //    }

            //    UpdateSensorModeVisuals();

            //    if (activeBombPool.Count > 0) {
            //        foreach (Bomb bmb in activeBombPool) {
            //            bmb.bombSprite.sprite = bombVisible;
            //        }
            //    }
            //}
            //else {

            //    if (sensorOverlayIsActive) {

            //        sensorOverlayIsActive = false;

            //        SetPlayerCarSprites();
            //        if (carsOnRoad.Count > 0) {
            //            foreach (NPCar car in carsOnRoad) {
            //                if (car.isVisible) { SetOtherCarSprites(car); }
            //            }
            //        }

            //        SetEffectSprites();

            //        sensorScanline.enabled = false;
            //        sensorOverlayIsTotallyLoaded = false;
            //        sensorOverlayIsUnloading = true;
            //        sensorOverlayFrameRefTime = Time.time;
            //        curSensorOverlayFrameIndex = 0;
            //    }

            //    if (sensorOverlayIsUnloading) { UpdateSensorModeVisuals(); }

            //    if (activeBombPool.Count > 0) {
            //        foreach (Bomb bmb in activeBombPool) {
            //            bmb.bombSprite.sprite = bombCamo;
            //        }
            //    }
            //}
        }
        else if (isOnAutopilot) {
            curPlayerSpeed = autopilotSpeed;
        }
        else {
            if (curPlayerSpeed > 0) { curPlayerSpeed -= deceleration * Time.deltaTime; }
            else { curPlayerSpeed = 0.0f; }
        }

        UpdateConsoleSensor();

        if (inputMapper[(int)InputMapper.CONTROLS.special]) {
            if (!sensorOverlayWasJustActivated && (((curSensorPower / sensorMaxPower) * 100.0f) >= sensorLowPowerPercentageOfMax)) {
                sensorOverlayWasJustActivated = true;

                SetPlayerCarSprites();
                if (carsOnRoad.Count > 0) {
                    foreach (NPCar car in carsOnRoad) {
                        //if (car.isVisible) {
                        //    //SetOtherCarSprites(car);
                        //    car.UseRedVariants(true);
                        //}
                        car.UseRedVariants(false);
                    }
                }

                SetEffectSprites();

                sensorOverlayIsActive = true;
                sensorOverlayIsLoading = true;
                sensorOverlayFrameRefTime = Time.time;
                curSensorOverlayFrameIndex = 0;
                curCenterConsoleFrameIndex = 0;
                sensorOverlay.sprite = sensorOverlayLoadFrames[curSensorOverlayFrameIndex];
                centerConsoleCover.texture = centerConsoleCoverFrames[curCenterConsoleFrameIndex];
                sensorOverlay.enabled = true;

                foreach (RawImage img in sensorBars) {
                    img.enabled = false;
                }
            }

            UpdateSensorModeVisuals();

            if (curSensorPower <= 0) {
                sensorOverlayWasJustActivated = false;

                SetPlayerCarSprites();
                if (carsOnRoad.Count > 0) {
                    foreach (NPCar car in carsOnRoad) {
                        //if (car.isVisible) {
                        //    //SetOtherCarSprites(car);
                        //    car.UseRedVariants(false);
                        //}
                        car.UseRedVariants(false);
                    }
                }

                SetEffectSprites();

                sensorScanline.enabled = false;
                sensorOverlayIsTotallyLoaded = false;
                sensorOverlayIsUnloading = true;
                sensorOverlayFrameRefTime = Time.time;
                curSensorOverlayFrameIndex = 0;
                curCenterConsoleFrameIndex = centerConsoleCoverFrames.Count - 1;
                centerConsoleCover.enabled = true;
            }

            if (activeBombPool.Count > 0) {
                foreach (Bomb bmb in activeBombPool) {
                    if (!bmb.isExploding) {
                        bmb.bombSprite.sprite = bombVisible;
                    }
                }
            }
        }
        else {
            if (sensorOverlayWasJustActivated) {
                sensorOverlayWasJustActivated = false;

                SetPlayerCarSprites();

                foreach (NPCar car in carsOnRoad) {
                    //if (car.isVisible) {
                    //    //SetOtherCarSprites(car);
                    //    car.UseRedVariants(false);        
                    //}
                    car.UseRedVariants(false);
                }

                SetEffectSprites();

                sensorScanline.enabled = false;
                sensorOverlayIsTotallyLoaded = false;
                sensorOverlayIsUnloading = true;
                sensorOverlayFrameRefTime = Time.time;
                curSensorOverlayFrameIndex = 0;
                curCenterConsoleFrameIndex = centerConsoleCoverFrames.Count - 1;
                centerConsoleCover.enabled = true;
            }

            if (sensorOverlayIsUnloading) { UpdateSensorModeVisuals(); }

            if (activeBombPool.Count > 0) {
                foreach (Bomb bmb in activeBombPool) {
                    if (!bmb.isExploding) {
                        bmb.bombSprite.sprite = bombCamo;
                    }
                }
            }
        }

        if (canDrive) {
            if (curPlayerTurning <= (-maxTurning)) {
                playerCarSprite.sprite = curPlayerCarHardLeft;
            }
            else if ((curPlayerTurning > (-maxTurning)) && (curPlayerTurning < (hardTurnFrameThreshold * -maxTurning))) {
                playerCarSprite.sprite = curPlayerCarLeft;
                isFinishedSlideSmoking = false;
            }
            else if ((curPlayerTurning >= (hardTurnFrameThreshold * -maxTurning)) && (curPlayerTurning < (turnFrameThreshold * -maxTurning))) {
                playerCarSprite.sprite = curPlayerCarSlightLeft;
            }
            else if ((curPlayerTurning >= (turnFrameThreshold * -maxTurning)) && (curPlayerTurning <= (turnFrameThreshold * maxTurning))) {
                playerCarSprite.sprite = curPlayerCarStraight;
            }
            else if ((curPlayerTurning > (turnFrameThreshold * maxTurning)) && (curPlayerTurning < (hardTurnFrameThreshold * maxTurning))) {
                playerCarSprite.sprite = curPlayerCarSlightRight;
            }
            else if ((curPlayerTurning >= (hardTurnFrameThreshold * maxTurning)) && (curPlayerTurning < (maxTurning))) {
                playerCarSprite.sprite = curPlayerCarRight;
                isFinishedSlideSmoking = false;
            }
            else if (curPlayerTurning >= (maxTurning)) {
                playerCarSprite.sprite = curPlayerCarHardRight;
            }
        }

        curPlayerSpeed += collisionSpeedEffect.y * 10f * Time.deltaTime;
        curPlayerDrift = -(curPlayerSpeed / topSpeed) * roadSegments[curSegmentIndex].Curve * centripetal * Time.deltaTime;

        float slipfactor;

        if ((Mathf.Abs(curPlayerDrift) > Mathf.Abs(curPlayerTurning)) && (Mathf.Abs(curPlayerTurning) > maxTurning)) {
            isSlipping = true;
            slipfactor = Mathf.Sin(Time.time * Mathf.PI * 15) * 0.1f;
        }
        else {
            slipfactor = 0f;
            isSlipping = false;
        }

        if (!isSmoking & !isFinishedSlideSmoking) {
            if ((curPlayerSpeed < (percentTopSpeedSlipStartThreshold * topSpeed / 100f) && 
                    curPlayerAcceleration > (percentMaxAccelerationSlipStartThreshold * maxAcceleration / 100f)) ||
                    isSlipping ||
                    playerCarSprite.sprite == curPlayerCarHardLeft ||
                    playerCarSprite.sprite == curPlayerCarHardRight) {
                isSmoking = true;
                smokeFrameRefTime = Time.time;
                curSmokeFrameIndex = 0;

                if (playerCarSprite.sprite == curPlayerCarHardLeft) {
                    playerCarEffectSprite.sprite = curSmokeLeftFrames[curSmokeFrameIndex];
                }
                else if (playerCarSprite.sprite == curPlayerCarHardRight) {
                    playerCarEffectSprite.sprite = curSmokeRightFrames[curSmokeFrameIndex];
                }
                else {
                    playerCarEffectSprite.sprite = curSmokeFrontFrames[curSmokeFrameIndex];
                }
                
                playerCarEffectSprite.enabled = true;
            }
        }
        else {
            if (Time.time - smokeFrameRefTime > smokeFrameTime) {
                if (playerCarSprite.sprite == curPlayerCarStraight) {
                    if (curSmokeFrameIndex < curSmokeFrontFrames.Count - 1) {
                        curSmokeFrameIndex++;
                    }
                    else {
                        curSmokeFrameIndex = restartSmokeStartFrameIndex;
                    }

                    playerCarEffectSprite.sprite = curSmokeFrontFrames[curSmokeFrameIndex];
                }
                if (playerCarSprite.sprite == curPlayerCarHardLeft) {
                    if (curSmokeFrameIndex < curSmokeLeftFrames.Count - 1) {
                        curSmokeFrameIndex++;
                    }
                    else {
                        //curSmokeFrameIndex = restartSmokeStartFrameIndex;
                        isFinishedSlideSmoking = true;
                        isSmoking = false;
                        playerCarEffectSprite.enabled = false;
                    }

                    playerCarEffectSprite.sprite = curSmokeLeftFrames[curSmokeFrameIndex];
                }
                if (playerCarSprite.sprite == curPlayerCarHardRight) {
                    if (curSmokeFrameIndex < curSmokeRightFrames.Count - 1) {
                        curSmokeFrameIndex++;
                    }
                    else                                                {
                        //curSmokeFrameIndex = restartSmokeStartFrameIndex;
                        isFinishedSlideSmoking = true;
                        isSmoking = false;
                        playerCarEffectSprite.enabled = false;
                    }

                    playerCarEffectSprite.sprite = curSmokeRightFrames[curSmokeFrameIndex];
                }

                smokeFrameRefTime = Time.time;
            }

            if ((curPlayerSpeed > (percentTopSpeedSlipStopThreshold * topSpeed / 100f) || 
                    curPlayerAcceleration < (percentMaxAccelerationSlipStopThreshold * maxAcceleration / 100f)) && 
                    playerCarSprite.sprite != curPlayerCarHardLeft && 
                    playerCarSprite.sprite != curPlayerCarHardRight) {
                playerCarEffectSprite.enabled = false;
                isSmoking = false;
            }
        }

        cumulativePlayerCarX = curPlayerTurning + curPlayerDrift + slipfactor + collisionSpeedEffect.x * Time.deltaTime;

        if (playerCar.position.x + cumulativePlayerCarX >= 0) {
            //playerCar.position = new Vector3(Mathf.Min(playerCar.position.x + cumulativePlayerCarX, screenHalfWidth),
            //                            playerInitialY + roadSegments[curSegmentIndex].Y / 2,
            //                            playerCar.position.z);
            playerCar.position = new Vector3(Mathf.Min(playerCar.position.x + cumulativePlayerCarX, screenHalfWidth), playerInitialY, -1f);
        } 
        else {
            //playerCar.position = new Vector3(Mathf.Max(playerCar.position.x + cumulativePlayerCarX, -screenHalfWidth),
            //                            playerInitialY + roadSegments[curSegmentIndex].Y / 2,
            //                            playerCar.position.z);
            playerCar.position = new Vector3(Mathf.Max(playerCar.position.x + cumulativePlayerCarX, -screenHalfWidth), playerInitialY, -1f);
        }

        curPlayerPosZ += curPlayerSpeed * Time.deltaTime;

        //camHeight = 150 + roadSegments[curSegmentIndex].Y / 2;
        //mainCam.transform.position = new Vector3(mainCam.transform.position.x, 100 + roadSegments[curSegmentIndex].Y / 2, mainCam.transform.position.z);
        mainCam.transform.position = new Vector3(mainCam.transform.position.x, playerCar.position.y + 70, playerCar.position.z - 1f);
    }

    private void ShowHUD(bool showOrNo) {
        if (showOrNo) {
            armor.enabled = true;
            armorFrame.enabled = true;
            armorBarCutter.enabled = true;
            UpdateArmorValue(0.0f);

            centerConsole.enabled = true;
            centerConsoleCover.enabled = true;
            speedometerNeedle.enabled = true;

            progressCar.enabled = true;
            progressTrack.enabled = true;

            
        }
        else {
            armor.enabled = false;
            armorFrame.enabled = false;
            armorBarCutter.enabled = false;

            centerConsole.enabled = false;
            centerConsoleCover.enabled = false;
            speedometerNeedle.enabled = false;

            //if (warningRoadBlips.Count > 0) {
            //    foreach (RawImage img in warningRoadBlips) { img.enabled = false; }
            //}

            if (warningRoadLines.Count > 0) {
                foreach (RawImage img in warningRoadLines) { img.enabled = false; }
            }

            if (sensorBars.Count > 0) {
                foreach (RawImage img in sensorBars) { img.enabled = false; }
            }

            progressCar.enabled = false;
            progressTrack.enabled = false;

            messageText.enabled = false;

            sensorOverlay.enabled = false;
            sensorScanline.enabled = false;
        }
    }

    public float GetRoadSegmentLength() {
        return roadSegmentLength;
    }
}
