using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bomb
{
    public Transform singleBomb;
    public SpriteRenderer bombSprite;
    public int segmentIndex;
    public float offsetX, explosionHeightOffset;
    public bool isExploding;
    public int explosionFrameIndex;
    public float explosionRefTime;
}
