using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NPCar
{
    public CarModel model;

    [HideInInspector]
    public bool isVisible;
    [HideInInspector]
    public int curSegIndex, curLane;
    public float targetSpeed, laneChangeSpeed;
    [HideInInspector]
    public float curForwardSpeed, curLaneChangeSpeed, curPosX, curPosZ;
    [HideInInspector]
    public Transform trafficCar;
    [HideInInspector]
    public RawImage blip;
    [HideInInspector]
    public SpriteRenderer carSprite;
    [HideInInspector]
    public Sprite curStraightOverhead, curSlightLeft, curLeft, curHardLeft, curSlightRight, curRight, curHardRight, curUphill, curDownhill;
    [HideInInspector]
    public List<Sprite> curSpinLeftFrames, curSpinRightFrames, curCrashFrames, curExplodeFrames;
    [HideInInspector]
    public float sparkOffsetFront, sparkOffsetBack, sparkOffsetLeft, sparkOffsetRight;

    public enum BehaviorState { StayingInLane, Crashing, SlowingDown, SpeedingUp, Ramming, Shooting, Exploding }
    public BehaviorState BehaviorMode;


}
