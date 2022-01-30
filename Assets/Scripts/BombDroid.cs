using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BombDroid
{
    public int segmentIndex;
    public float offsetX, dropDelayRefTime;
    public Transform droid;
    public SpriteRenderer droidSprite;
}
