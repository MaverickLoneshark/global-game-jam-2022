using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadCurve
{
    public int segmentInsertIndex, numEnterSegments, numHoldSegments, numExitSegments;
    public float curveIntensity, elevationShift;
    //public int SegmentInsertIndex { get => segmentInsertIndex; set => segmentInsertIndex = value; }
    //public int NumEnterSegments { get => numEnterSegments; set => numEnterSegments = value; }
    //public int NumHoldSegments { get => numHoldSegments; set => numHoldSegments = value; }
    //public int NumExitSegments { get => numExitSegments; set => numExitSegments = value; }
    //public int CurveIntensity { get => curveIntensity; set => curveIntensity = value; }     // Positive value to curve to the right; negative to curve left
}
