using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spark
{
    public Transform sparkInstance;
    public SpriteRenderer sparkSprite;

    public NPCar sparkedCar;

    public enum SparkDirection { Front, Back, Left, Right }
    public SparkDirection SparkSide;

    public float sparkFrameRefTime;
    public int curSparkFrameIndex;
}
