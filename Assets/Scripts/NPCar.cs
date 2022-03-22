using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NPCar : MonoBehaviour
{
    private CarModel model;

    private bool isVisible;

    private int curSegIndex;
    private float curSpeedZ, curSpeedX, curPosX, prevPosX, curPosZ, lengthPerSeg;
    private Transform trafficCar;
    private RawImage blip;
    private SpriteRenderer carSprite;
    //public Sprite curStraightOverhead, curSlightLeft, curLeft, curHardLeft, curSlightRight, curRight, curHardRight, curUphill, curDownhill;
    //public List<Sprite> curSpinFrames, curCrashFrames, curExplodeFrames;
    private int curExplodeFramesFirstIndex, curExplodeFramesLastIndex;   // These are to keep track of the bounds of the current selected crashout frames
    private float crashFrameRefTime;
    private float sparkOffsetFront, sparkOffsetBack, sparkOffsetLeft, sparkOffsetRight;

    private bool useRoadGrip,        // Using road grip will make vehicles seem more or less stable based on their grip value
                    useCrashDrama;  // This will speed match crashes to the player speed to keep the action onscreen

    private float curArmor;
    private float curLaneChangeTime, laneChangeRefTime;
    private bool isAnEnemy, useRedVariants, isInvulnerable, isDead, isFlickering, isFlickeredOut;
    private float[] lanePositions = new float[3];
    private float roadSegLength;
    private int curLaneNum;

    private SpriteFiveViews curAnimationView;
    private List<Sprite> curAnimationFrames = new List<Sprite>();
    private const float animationFrameTime = 0.1f;
    private int curFrameIndex;

    private float collisionEndVelocityX, collisionEndVelocityZ;    // These are for setting final velocities for after a collision

    private enum BehaviorState { StayingInLane, ChangingLanes, Crashing, Pursuing, Attacking, Exploding, Wrecked }
    private BehaviorState BehaviorMode;
    private BehaviorState prevBehaveState;

    private enum ViewAngle { Left, BackLeft, Back, BackRight, Right }
    private ViewAngle CurrentViewAngle;

    private const float enemySlightTurnRotationThreshold = 10f, enemyTurnRotationThreshold = 30f, enemyHardTurnRotationThreshold = 50f;
    private const float laneChangeTurnViewFactor = 0.1f;        // The rotation factor is affected by curSpeedX multiplied by this value
    private const float impactSpeedEnhanceFactorX = 1f , impactSpeedEnhanceFactorZ = 1f;      // Speed adjustment due to impact

    private int curNumOfFlickers, flickerCounter;
    private float curFlickerIntervalTime, curFlickerWaitTime, flickerRefTime, flickerWaitRefTime;

    private float playerPosX, playerPosZ, playerSpeedZ;

    void Awake() {
        
    }
    void Update() {

        if (curAnimationView != null) { SetSpriteView(); }

        curSegIndex = (int)(curPosZ / roadSegLength);

        carSprite.sprite = curAnimationFrames[curFrameIndex];

        switch (BehaviorMode) {
            case BehaviorState.StayingInLane:

                curAnimationView = model.standardDriving;

                if (curSpeedZ < model.topSpeed) {
                    curSpeedZ += model.acceleration * Time.deltaTime;
                }
                curPosZ += curSpeedZ * roadSegLength * Time.deltaTime;

                if ((curPosZ - playerPosZ < model.attackRangeZ) && (curPosZ > playerPosZ)) {

                }
                else if (Time.time - laneChangeRefTime > curLaneChangeTime) {
                    ChangeCurrentLane();
                    BehaviorMode = BehaviorState.ChangingLanes;
                }

                break;
            case BehaviorState.ChangingLanes:

                curAnimationView = model.standardDriving;

                if (curSpeedZ < model.topSpeed) {
                    curSpeedZ += model.acceleration * Time.deltaTime;
                }
                curPosZ += curSpeedZ * roadSegLength * Time.deltaTime;

                if (Mathf.Abs(curPosX - lanePositions[curLaneNum]) < 1) {
                    curPosX = lanePositions[curLaneNum];
                    curSpeedX = 0.0f;

                    curLaneChangeTime = Random.Range(model.laneChangeTimerMin, model.laneChangeTimerMax);
                    laneChangeRefTime = Time.time;
                    BehaviorMode = BehaviorState.StayingInLane;
                }
                else {
                    curPosX = Mathf.Lerp(curPosX, lanePositions[curLaneNum], model.topLaneChangeSpeed * Time.deltaTime);    // Not the ideal way to do this
                    curSpeedX = (curPosX - prevPosX) / Time.deltaTime;

                    prevPosX = curPosX;
                }

                break;
            case BehaviorState.Crashing:

                //curSpeedX = Mathf.Lerp(curSpeedX, collisionEndVelocityX, 0.01f * Time.deltaTime);
                curSpeedX = 0;
                curSpeedZ = Mathf.Lerp(curSpeedZ, collisionEndVelocityZ, 0.01f * Time.deltaTime);

                //curSpeedZ = model.topSpeed * 2;

                curPosX += curSpeedX * Time.deltaTime;
                curPosZ += curSpeedZ * roadSegLength * Time.deltaTime;

                if (Time.time - crashFrameRefTime > animationFrameTime) {
                    if (curFrameIndex < curAnimationFrames.Count - 1) {
                        curFrameIndex++;
                        carSprite.sprite = curAnimationFrames[curFrameIndex];
                        crashFrameRefTime = Time.time;
                    }
                    else {
                        if (curArmor < 0.0f) {
                            BehaviorMode = BehaviorState.Wrecked;
                        }
                        else {
                            curSpeedX = 0;
                            curSpeedZ = collisionEndVelocityZ;
                            Debug.Log("velocity exiting crash = " + collisionEndVelocityZ);
                            curFrameIndex = 0;
                            laneChangeRefTime = Time.time;
                            BehaviorMode = prevBehaveState;
                            Debug.Log("Switch back to: " + BehaviorMode);
                        }
                    }
                }

                break;
            case BehaviorState.Pursuing:
                break;
            case BehaviorState.Attacking:
                break;
            case BehaviorState.Exploding:
                break;
            case BehaviorState.Wrecked:

                //curSpeedX = Mathf.Lerp(curSpeedX, collisionEndVelocityX, model.acceleration * Time.deltaTime);
                //curSpeedZ = Mathf.Lerp(curSpeedZ, collisionEndVelocityZ, model.acceleration * Time.deltaTime);

                //curSpeedX = 0;
                //curSpeedZ = 0;

                //curPosX += curSpeedX * Time.deltaTime;
                //curPosZ += curSpeedZ * roadSegLength * Time.deltaTime;

                SetVisibility(false);

                break;
        }
    }

    public void InitializeCar(CarModel carMod, int startSeg, float lenPerSeg, float widthOfLane, Texture blipSprite, Vector2 blipPos, Transform blipParent) {
        model = carMod;
        trafficCar = this.transform;
        carSprite = this.transform.GetComponent<SpriteRenderer>();

        SetLanePositions(-widthOfLane, 0, widthOfLane);
        curLaneNum = Random.Range(0, lanePositions.Length);
        curPosX = lanePositions[curLaneNum];
        prevPosX = curPosX;
        curSegIndex = startSeg;
        lengthPerSeg = lenPerSeg;
        curPosZ = startSeg * lengthPerSeg;

        curSpeedZ = model.topSpeed;
        curArmor = model.maxArmor;

        curLaneChangeTime = Random.Range(model.laneChangeTimerMin, model.laneChangeTimerMax);
        laneChangeRefTime = Time.time;

        roadSegLength = FindObjectOfType<RoadControl>().GetRoadSegmentLength();

        GameObject imgObj = new GameObject();
        RawImage img = imgObj.AddComponent<RawImage>();
        imgObj.transform.SetParent(blipParent);
        blip = img;
        blip.texture = blipSprite;
        blip.rectTransform.anchoredPosition = blipPos;
        blip.rectTransform.sizeDelta = Vector2.zero;

        SetVisibility(false);

        BehaviorMode = BehaviorState.StayingInLane;
        CurrentViewAngle = ViewAngle.Back;
        curAnimationView = model.standardDriving;
    }

    public void IsAnEnemyVehicle(bool isIt) {
        isAnEnemy = isIt;
    }

    private void MaintainSpeed() {
        if (curSpeedZ < model.topSpeed) {
            curSpeedZ += model.acceleration * Time.deltaTime;
        }
    }

    public void SetLanePositions(float leftLaneX, float middleLaneX, float rightLaneX) {
        lanePositions[0] = leftLaneX;
        lanePositions[1] = middleLaneX;
        lanePositions[2] = rightLaneX;
    }

    public void ChangeCurrentLane() {

        switch(curLaneNum) {
            case 0:
                curLaneNum = 1;
                break;
            case 1:
                int rand = Random.Range(0, 2);
                if (rand == 0)  {
                    curLaneNum = 0;
                }
                else {
                    curLaneNum = 2;
                }
                break;
            case 2:
                curLaneNum = 1;
                break;
        }
    }

    //This calculates the 
    public Vector2 InitiateCrash(float otherMass, float otherRoadGrip, Vector2 otherVelocity, Vector2 impactPoint) {
        //float impactMomentumX = sourceImpactMomentum.x;
        //float impactMomentumZ = sourceImpactMomentum.y;
        //curCrashVelocity = new Vector2(impactMomentumX / model.mass, impactMomentumZ / model.mass);

        Vector2 otherCarMomentum = otherVelocity * otherMass;

        // It's important to calculate the effect on the armor first, so that in the case of it being reduced to zero, the collision triggers an explosion
        if (otherCarMomentum.magnitude > model.impactExplosionThreshold)    { curArmor -= model.explosionDamage; }
        else if (otherCarMomentum.magnitude > model.impactSpinoutThreshold) { curArmor -= model.spinoutDamage; }
        else                                                                { curArmor -= model.crashDamage; }

        Debug.Log("CRAAAASH!  Other car's momentum = " + otherCarMomentum + ", current armor rating = " + curArmor);

        // Final velocities determined using Conservation of Momentum and the relationship of initial and final velocities in a perfectly elastic collision:
        // m1v1i + m2v2i = m1v1f + m2v2f
        // v1i + v1f = v2i + v2f
        //
        // v1f = (m1v1i + m2(2*v2i - v1i)) / (m1 + m2)
        // v2f = v1i - v2i + v1f
        // 
        // Note that the collisions are not treated as being elastic, but we're applying the "elasticity" and "road grip" factors afteward, for simplicity
        Vector2 thisCarFinalVelocity = new Vector2(((model.mass * curSpeedX + otherMass * (2 * otherVelocity.x - curSpeedX)) / (model.mass + otherMass)) *
                                                        impactSpeedEnhanceFactorX,
                                                        (model.mass * curSpeedZ + otherMass * (2 * otherVelocity.y - curSpeedZ)) / (model.mass + otherMass)) *
                                                        impactSpeedEnhanceFactorZ;
        Vector2 otherCarFinalVelocity = new Vector2(curSpeedX - otherVelocity.x + thisCarFinalVelocity.x,
                                                    curSpeedZ - otherVelocity.y + thisCarFinalVelocity.y);

        if (useRoadGrip) {
            thisCarFinalVelocity = thisCarFinalVelocity * model.roadGripPercentageRelativeToPlayerCar / 100.0f;
            otherCarFinalVelocity = otherCarFinalVelocity * otherRoadGrip / 100.0f;
        }

        // 
        if (otherCarMomentum.magnitude > model.impactExplosionThreshold || curArmor < 0.0f) {
            collisionEndVelocityX = curSpeedX * model.percentCurrentSpeedAfterExplosion / 100.0f;
            collisionEndVelocityZ = curSpeedZ * model.percentCurrentSpeedAfterExplosion / 100.0f;

            // If the collision triggers an explosion, figure out which sub-sequence in the explosion frames list is going to play
            if (model.explodeFrameStartIndices.Count > 1) {
                int indexIndex = Random.Range(0, model.explodeFrameStartIndices.Count);
                curFrameIndex = model.explodeFrameStartIndices[indexIndex];
                if (indexIndex < model.explodeFrameStartIndices.Count - 1) {
                    curExplodeFramesLastIndex = model.explodeFrameStartIndices[indexIndex + 1];
                }
                else {
                    curExplodeFramesLastIndex = model.explodeFramesLeft.left.Count - 1;
                }
            }

            if (impactPoint.x <= curPosX)   { curAnimationView = model.explodeFramesLeft; }
            else                            { curAnimationView = model.explodeFramesRight; }

        }
        else if (otherCarMomentum.magnitude > model.impactSpinoutThreshold) {
            collisionEndVelocityX = curSpeedX * model.percentCurrentSpeedAfterSpinout / 100.0f;
            collisionEndVelocityZ = curSpeedZ * model.percentCurrentSpeedAfterSpinout / 100.0f;

            if (impactPoint.x <= curPosX)   { curAnimationView = model.spinFramesLeft; }
            else                            { curAnimationView = model.spinFramesRight; }
        }
        else {
            collisionEndVelocityX = curSpeedX * model.percentCurrentSpeedAfterCrash / 100.0f;
            collisionEndVelocityZ = curSpeedZ * model.percentCurrentSpeedAfterCrash / 100.0f;

            if (impactPoint.x <= curPosX)   { curAnimationView = model.crashFramesLeft; }
            else                            { curAnimationView = model.crashFramesRight; }
        }

        curSpeedX = thisCarFinalVelocity.x;
        curSpeedZ = thisCarFinalVelocity.y;

        prevBehaveState = BehaviorMode;
        curFrameIndex = 0;
        crashFrameRefTime = Time.time;
        BehaviorMode = BehaviorState.Crashing;

        return otherCarFinalVelocity;
    }

    private void SetSpriteView() {
        switch (CurrentViewAngle) {
            case ViewAngle.Left:
                if (useRedVariants) { curAnimationFrames = curAnimationView.redLeft; }
                else                { curAnimationFrames = curAnimationView.left; }
                break;
            case ViewAngle.BackLeft:
                if (useRedVariants) { curAnimationFrames = curAnimationView.redBackLeft; }
                else                { curAnimationFrames = curAnimationView.backLeft; }
                break;
            case ViewAngle.Back:
                if (useRedVariants) { curAnimationFrames = curAnimationView.redBack; }
                else                { curAnimationFrames = curAnimationView.back; }
                break;
            case ViewAngle.BackRight:
                if (useRedVariants) { curAnimationFrames = curAnimationView.redBackRight; }
                else                { curAnimationFrames = curAnimationView.backRight; }
                break;
            case ViewAngle.Right:
                if (useRedVariants) { curAnimationFrames = curAnimationView.redRight; }
                else                { curAnimationFrames = curAnimationView.right; }
                break;
        }
    }

    public void SetVisibility(bool yOrN) {
        if (yOrN) {
            isVisible = true;
            carSprite.enabled = true;
        }
        else {
            isVisible = false;
            carSprite.enabled = false;
        }

        if (blip != null) { blip.enabled = carSprite.enabled; }
    }

    // Overload checking for sensor status
    public void SetVisibility(bool yOrN, bool sensorIsOn) {
        if (yOrN) {
            if (sensorIsOn && !model.isSensorDetectable) {
                if (isFlickering) {
                    if (Time.time - flickerRefTime > curFlickerIntervalTime) {

                        isVisible = !isVisible;
                        carSprite.enabled = !carSprite.enabled;

                        if (flickerCounter > curNumOfFlickers) {
                            curFlickerWaitTime = Random.Range(model.flickerWaitMin, model.flickerWaitMax);
                            flickerWaitRefTime = Time.time;
                            isFlickering = false;
                        }
                        else {
                            flickerCounter++;
                        }

                        flickerRefTime = Time.time;
                    }
                }
                else {
                    if (Time.time - flickerWaitRefTime > curFlickerWaitTime) {
                        curNumOfFlickers = Random.Range(model.numOfFlickersMin, model.numOfFlickersMax + 1);
                        curFlickerIntervalTime = Random.Range(model.flickerTimeIntervalMin, model.flickerTimeIntervalMax);
                        flickerCounter = 0;
                        flickerRefTime = Time.time;
                        isFlickering = true;
                    }
                }
            }
            else {
                isVisible = true;
                carSprite.enabled = true;
            }
        }
        else {
            isVisible = false;
            carSprite.enabled = false;
        }
        
        if (blip != null) {
            if (!sensorIsOn)    { blip.enabled = false; }
            else                { blip.enabled = carSprite.enabled; }
        }
    }

    public void SetViewAngle(float roadCurve, float camPosX, float curPlayerPosZ) {
        float rotationFactor = roadCurve + (camPosX - curPosX) / (Mathf.Abs(curPosZ - curPlayerPosZ) + 10f) + curSpeedX * laneChangeTurnViewFactor;
        if (Mathf.Abs(rotationFactor) < enemySlightTurnRotationThreshold) {
            CurrentViewAngle = ViewAngle.Back;
        }
        else if (Mathf.Abs(rotationFactor) >= enemySlightTurnRotationThreshold && Mathf.Abs(rotationFactor) < enemyTurnRotationThreshold) {
            if (rotationFactor > 0) { CurrentViewAngle = ViewAngle.BackRight; }    //{ car.carSprite.sprite = car.curSlightRight; }
            else                    { CurrentViewAngle = ViewAngle.BackLeft; }     //{ car.carSprite.sprite = car.curSlightLeft; }
        }
        else if (Mathf.Abs(rotationFactor) > +enemyTurnRotationThreshold && Mathf.Abs(rotationFactor) < enemyHardTurnRotationThreshold) {
            if (rotationFactor > 0) { CurrentViewAngle = ViewAngle.Right; }        //{ car.carSprite.sprite = car.curRight; }
            else                    { CurrentViewAngle = ViewAngle.Left; }         //{ car.carSprite.sprite = car.curLeft; }
        }
        else if (Mathf.Abs(rotationFactor) >= enemyHardTurnRotationThreshold) {
            if (rotationFactor > 0) { CurrentViewAngle = ViewAngle.Right; }        //{ car.carSprite.sprite = car.curHardRight; }
            else                    { CurrentViewAngle = ViewAngle.Left; }         //{ car.carSprite.sprite = car.curHardLeft; }
        }
    }

    public void UseRedVariants(bool yOrNo) {
        useRedVariants = yOrNo;
    }

    public float GetCurPosX() {
        return curPosX;
    }

    public int GetCurSeg() {
        return curSegIndex;
    }

    public float GetLength() {
        return model.lengthInRoadSegs;
    }

    public float GetWidth() {
        return model.width;
    }

    public RawImage GetBlip() {
        return blip;
    }

    public void GetPlayerPositionAndSpeed(float posX, float posZ, float speedZ) {
        playerPosX = posX;
        playerPosZ = posZ;
        playerSpeedZ = speedZ;
    }
}
