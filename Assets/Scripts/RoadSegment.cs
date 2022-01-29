using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment
{
    private float edgeNearZ, edgeFarZ, curve, y;
    public float EdgeNearZ { get => edgeNearZ; set => edgeNearZ = value; }
    public float EdgeFarZ { get => edgeFarZ; set => edgeFarZ = value; }
    public float Curve { get => curve; set => curve = value; }
    public float Y { get => y; set => y = value; }
    private Sprite spriteVariation;
    public Sprite SpriteVariation { get => spriteVariation; set => spriteVariation = value; }
}
