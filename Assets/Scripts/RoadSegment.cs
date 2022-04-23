using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment {
	// this is for keeping track of the Z position of the near edge of the next RoadSegment to add to the track
	public float edgeNearZ;
	// edgeNearZ + roadSegmentLength
	public float edgeFarZ;
	public float curve = 0;
	public float y = 0;
	public Sprite spriteVariation;
}
