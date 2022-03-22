using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car Pose Series")]
public class CarModel : ScriptableObject
{
    //public Sprite straightOverhead, slightLeft, left, hardLeft, slightRight, right, hardRight, uphill, downhill,
    //                redStraightOverhead, redSlightLeft, redLeft, redHardLeft, redSlightRight, redRight, redHardRight, redUphill, redDownhill;
    public SpriteFiveViews standardDriving, crashFramesLeft, crashFramesRight, spinFramesLeft, spinFramesRight, explodeFramesLeft, explodeFramesRight;
    public List<Sprite> sparkFrontFrames, sparkBackFrames, sparkLeftFrames, sparkRightFrames,
                        redSparkFrontFrames, redSparkBackFrames, redSparkLeftFrames, redSparkRightFrames;
    public List<Sprite> curExplodeFrames, curRedExplodeFrames;
    public List<int> explodeFrameStartIndices;  // If there are multiple sets of crashout animations, these are the indices of the first frames of each

    private List<SpriteFiveViews> curCollisionFrames = new List<SpriteFiveViews>();

    [Range(0.0f, 100.0f)]
    public float maxArmor;
    public float mass;
    [Range(0.0f, 200.0f)]
    public float roadGripPercentageRelativeToPlayerCar;
    public float width;
    public float lengthInRoadSegs;
    public float acceleration, topSpeed, topLaneChangeSpeed, laneChangeTimerMin, laneChangeTimerMax;
    public float impactSpinoutThreshold, impactExplosionThreshold, crashDamage, spinoutDamage, explosionDamage;
    [Range(0.0f, 100.0f)]
    public float percentCurrentSpeedAfterCrash, percentCurrentSpeedAfterSpinout, percentCurrentSpeedAfterExplosion;
    public float sparkOffsetFront, sparkOffsetBack, sparkOffsetLeft, sparkOffsetRight;
    public bool isSensorDetectable;
    public int numOfFlickersMin, numOfFlickersMax;
    public float flickerTimeIntervalMin, flickerTimeIntervalMax, flickerWaitMin, flickerWaitMax;

    public float attackRangeZ, attackSpeedX, attackOffsetX;

    [HideInInspector]
    public enum BehaviorType { Simple, LaneChanger, Attacker }
    public BehaviorType Behavior;
}
