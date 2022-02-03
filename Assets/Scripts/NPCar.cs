using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public SpriteRenderer carSprite;
}
