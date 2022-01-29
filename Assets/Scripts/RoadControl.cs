using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool canDrive, isSlipping, isOffRoading, isCrashing, isSpinningOut, isExploding, isRolling;

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
    private float topSpeed, acceleration, deceleration, maxTurning;
    private float curPlayerSpeed, curPlayerTurning, camCurZ, playerZoffset, curPlayerPosX;

    // Start is called before the first frame update
    void Start()
    {
        playerCarSprite = playerCar.GetComponent<SpriteRenderer>();

        curPlayerSpeed = 0;
        camCurZ = 0;
        distCamToScreen = (numScreenLines / 2f) / (Mathf.Tan(FOV / 2f));

        curAddedSegmentZ = camCurZ + roadStartZ;
        straightSegCounter = 0;
        rumbleStripCounter = 0;
        InitializeRoadStrips();

        Debug.Log("# road screen line sprites = " + roadScreenSprites.Count);
    }

    // Update is called once per frame
    void Update()
    {
        curSegmentIndex = GetCurrentRoadSegmentIndex();
        ManagePlayerPosition();
        UpdateRoad();

        Debug.Log(roadSegments.Count);
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

                        Debug.Log("hi, " + straightSegCounter);
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
        Debug.Log(curSegmentIndex);
        int highestScreenLineDrawn = -1;    // First screen line is roadScreenLines[0], so let's use -1 as a reference

        float x = 0;
        float dx;

        for (int i = curSegmentIndex; i < (Mathf.Min(curSegmentIndex + numSegsToDraw - 1, numStraightSegs)); i++) {
            int nearEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeNearZ - camCurZ)));
            int farEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * (camHeight - roadSegments[i].Y) / (roadSegments[i].EdgeFarZ - camCurZ)));
            nearEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeNearZ - camCurZ));
            farEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeFarZ - camCurZ));

            Debug.Log(roadSegments[i].EdgeNearZ);
            Debug.Log(nearEdgeHeight + ", " + farEdgeHeight);

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
                            Debug.Log(roadScreenLines[j].localScale.x);
                        }
                    }
                    Debug.Log("thick seg, screen line #" + nearEdgeHeight);
                }
                else {
                    roadScreenSprites[farEdgeHeight].sprite = roadSegments[i].SpriteVariation;
                    roadScreenLines[farEdgeHeight].position = new Vector3(x + dx,
                                                                        roadScreenLines[farEdgeHeight].position.y, roadScreenLines[farEdgeHeight].position.z);
                    roadScreenLines[farEdgeHeight].localScale = new Vector3(farEdgeWidthScale, 1.0f, 1.0f);
                    Debug.Log("single seg, screen line #" + nearEdgeHeight);
                }
            }

            x += dx;
            highestScreenLineDrawn = farEdgeHeight;
            //Debug.Log(highestScreenLineDrawn);
        }
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

        if (Input.GetKey(KeyCode.UpArrow)) {
            if (curPlayerSpeed < topSpeed)  { curPlayerSpeed += acceleration * Time.deltaTime; }
            else                            { curPlayerSpeed = topSpeed; }
        }
        else {
            if (curPlayerSpeed > 0) { curPlayerSpeed -= deceleration * Time.deltaTime; }
            else                    { curPlayerSpeed = 0.0f; }
        }

        if (Input.GetKey(KeyCode.LeftArrow)) {
            playerCarSprite.sprite = playerCarHardLeft;
            if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                playerCar.Translate(-curPlayerTurning * Time.deltaTime, 0, 0);
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow)) {
            playerCarSprite.sprite = playerCarHardRight;
            if (Mathf.Abs(playerCar.position.x) < screenHalfWidth) {
                playerCar.Translate(curPlayerTurning * Time.deltaTime, 0, 0);
            }
        }
        else {
            playerCarSprite.sprite = playerCarStraight;
        }

        camCurZ += curPlayerSpeed * Time.deltaTime;
    }
}
