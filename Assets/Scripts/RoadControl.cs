using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadControl : MonoBehaviour
{
    [SerializeField]
    private Transform playerCar, playerCarEffect;
    private Sprite playerCarSprite, playerCarEffectSprite;
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
    private float numTotalSegs, numSegsToDraw;
    private float distCamToScreen;
    private float nearEdgeWidthScale, farEdgeWidthScale;

    private List<RoadSegment> roadSegments = new List<RoadSegment>();

    private int curSegmentIndex;

    [SerializeField]
    private float topSpeed, acceleration, deceleration;
    private float curPlayerSpeed, camCurZ, playerZoffset, curPlayerPosX;

    // Start is called before the first frame update
    void Start()
    {
        curPlayerSpeed = 0;
        camCurZ = 0;
        distCamToScreen = (numScreenLines / 2f) / (Mathf.Tan(FOV / 2f));

        InitializeRoadStrips();

        Debug.Log("# road screen line sprites = " + roadScreenSprites.Count);
    }

    // Update is called once per frame
    void Update()
    {
        curSegmentIndex = GetCurrentRoadSegmentIndex();
        ManagePosition();
        UpdateRoad();
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

        int stripCounter = 1;
        int curRoadStripSpriteIndex = 0;
        Sprite curRoadStripSprite = roadStripSprites[curRoadStripSpriteIndex];

        for (int i = 0; i < numTotalSegs; i++) {
            RoadSegment newSeg = new RoadSegment();
            newSeg.EdgeNearZ = camCurZ + roadStartZ + roadSegmentLength * i;
            newSeg.EdgeFarZ = camCurZ + roadStartZ + roadSegmentLength * (i + 1);
            newSeg.SpriteVariation = curRoadStripSprite;
            roadSegments.Add(newSeg);

            stripCounter++;
            if (stripCounter > numSegsPerRumble) {
                curRoadStripSpriteIndex = (curRoadStripSpriteIndex + 1) % roadStripSprites.Count;
                curRoadStripSprite = roadStripSprites[curRoadStripSpriteIndex];
                stripCounter = 1;
            }
        }
    }

    private int GetCurrentRoadSegmentIndex() {
        return ((int)Mathf.Floor(camCurZ / roadSegmentLength));
    }

    private void UpdateRoad() {
        Debug.Log(curSegmentIndex);
        int highestScreenLineDrawn = -1;    // First screen line is roadScreenLines[0], so let's use -1 as a reference

        for (int i = curSegmentIndex; i < (Mathf.Min(curSegmentIndex + numSegsToDraw - 1, numTotalSegs)); i++) {
            int nearEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * camHeight / (roadSegments[i].EdgeNearZ - camCurZ)));
            int farEdgeHeight = (int)(numScreenLines - Mathf.Floor(distCamToScreen * camHeight / (roadSegments[i].EdgeFarZ - camCurZ)));
            nearEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeNearZ - camCurZ));
            farEdgeWidthScale = (distCamToScreen / (roadSegments[i].EdgeFarZ - camCurZ));

            Debug.Log(roadSegments[i].EdgeNearZ);
            Debug.Log(nearEdgeHeight + ", " + farEdgeHeight);

            nearEdgeHeight -= 130;
            farEdgeHeight -= 130;

            if (farEdgeHeight > highestScreenLineDrawn && farEdgeHeight < numScreenLines) {
                if (farEdgeHeight > nearEdgeHeight) {
                    for (int j = nearEdgeHeight; j <= farEdgeHeight; j++) {
                        float scaleIncreaseFromNearEdge = (j - nearEdgeHeight) * ((farEdgeWidthScale - nearEdgeWidthScale) / (farEdgeHeight - nearEdgeHeight));
                        if (j >= 0) {
                            roadScreenSprites[j].sprite = roadSegments[i].SpriteVariation;
                            roadScreenLines[j].localScale = new Vector3(nearEdgeWidthScale + scaleIncreaseFromNearEdge, 1.0f, 1.0f);
                            Debug.Log(roadScreenLines[j].localScale.x);
                        }
                    }
                    Debug.Log("thick seg, screen line #" + nearEdgeHeight);
                }
                else {
                    roadScreenSprites[farEdgeHeight].sprite = roadSegments[i].SpriteVariation;
                    roadScreenLines[farEdgeHeight].localScale = new Vector3(farEdgeWidthScale, 1.0f, 1.0f);
                    Debug.Log("single seg, screen line #" + nearEdgeHeight);
                }
            }

            highestScreenLineDrawn = farEdgeHeight;
        }
    }

    private void ManagePosition() {
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (curPlayerSpeed < topSpeed)  { curPlayerSpeed += acceleration * Time.deltaTime; }
            else                            { curPlayerSpeed = topSpeed; }
        }
        else {
            if (curPlayerSpeed > 0) { curPlayerSpeed -= deceleration * Time.deltaTime; }
            else                    { curPlayerSpeed = 0.0f; }
        }

        camCurZ += curPlayerSpeed * Time.deltaTime;
    }
}
