using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car Pose Series")]
public class CarModel : ScriptableObject
{
    public Sprite straightOverhead, slightLeft, left, hardLeft, slightRight, right, hardRight, uphill, downhill,
                    redStraightOverhead, redSlightLeft, redLeft, redHardLeft, redSlightRight, redRight, redHardRight, redUphill, redDownhill;
    public List<Sprite> spinLeftFrames, spinRightFrames, crashFrames, explodeFrames,
                        redSpinLeftFrames, redSpinRightFrames, redCrashFrames, redExplodeFrames;
    public float width;
    public float lengthInRoadSegs;
    public float sparkOffsetFront, sparkOffsetBack, sparkOffsetLeft, sparkOffsetRight;
    public bool isSensorDetectable;
    
    [HideInInspector]
    public enum BehaviorType { Simple, LaneChanger, Attacker }
    public BehaviorType Behavior;
}
