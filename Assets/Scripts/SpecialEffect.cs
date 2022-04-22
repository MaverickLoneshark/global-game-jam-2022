using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpecialEffect
{
    public enum EffectCategory { SmokeBack, SmokeLeft, SmokeRight, SparkFront, SparkBack, SparkLeft, SparkRight, Explosion }
    public EffectCategory TypeOfEffect;

    [SerializeField]
    private float effectTime;
}
