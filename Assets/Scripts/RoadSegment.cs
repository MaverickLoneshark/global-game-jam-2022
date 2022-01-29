using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment
{
    private float edgeNearZ, edgeFarZ;
    public float EdgeNearZ { get => edgeNearZ; set => edgeNearZ = value; }
    public float EdgeFarZ { get => edgeFarZ; set => edgeFarZ = value; }
    private Sprite spriteVariation;
    public Sprite SpriteVariation { get => spriteVariation; set => spriteVariation = value; }
}
