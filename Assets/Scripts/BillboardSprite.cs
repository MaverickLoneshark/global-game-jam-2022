using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BillboardSprite
{
    public Sprite spriteType;
    [HideInInspector]
    public Transform spriteTransform;
    [HideInInspector]
    public SpriteRenderer spriteRend;
    public int segmentIndex, offsetX, howManyRows = 1, howManyCols = 1, rowSpacingInSegs = 20, colSpacingInPixels;
}
