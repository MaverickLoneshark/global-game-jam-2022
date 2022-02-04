using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car Pose Series")]
public class CarModel : ScriptableObject
{
    public Sprite straightOverhead, left, hardLeft, right, hardRight, uphill, downhill;
    public List<Sprite> spinLeftFrames, spinRightFrames, crashFrames, explodeFrames;
    public float width;
    public float lengthInRoadSegs;
    
    [HideInInspector]
    public enum BehaviorType { Simple, LaneChanger, Attacker }
    public BehaviorType Behavior;
}
