using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Use the start/end distance and the start/end value to interpolate a value at the given distance.
 */
[System.Serializable]
public class DistanceValue
{
    public int startDistance;
    public float startValue;
    public int endDistance;
    public float endValue;
	public bool useEndDistance;

    public DistanceValue(int sd, float sv, int ed, float ev, bool ued)
    {
        startDistance = sd;
        startValue = sv;
        endDistance = ed;
        endValue = ev;
        useEndDistance = ued;
    }
	
	public float getValue(float distance)
	{
		// if the distance is before the start distance or after the end distance, return 0
        if (distance < startDistance || (useEndDistance && distance > endDistance))
			return 0;

        if (startDistance == endDistance || !useEndDistance) {
            return startValue;	
		}

        float distancePercent = ((distance - startDistance) / (endDistance - startDistance));

        // distancePercent can be greater than 1 if distance > endDistance
        if (distancePercent > 1)
            distancePercent = 1;
		
		// linear interpolation
        return startValue + (distancePercent * (endValue - startValue));
	}

    public bool withinDistance(float distance)
    {
        return distance >= startDistance && (!useEndDistance || (useEndDistance && distance <= endDistance));
    }
}

/*
 * Holds a list of distance values
 */
[System.Serializable]
public class DistanceValueList
{
    // this array MUST be in order according to startDistance/EndDistance, with no overlap
    public List<DistanceValue> values;
    // if true and the distance reached past the end of the last distance set, the probabilities will loop back around to the start
    public bool loop;

    private int lastEndDistanceIndex;
    private int[] endDistanceIndex;
    private float previousLoopDistance;

    public DistanceValueList(DistanceValue v)
    {
        values = new List<DistanceValue>();
        values.Add(v);
    }

    public void init()
    {
        endDistanceIndex = new int[values.Count];
        lastEndDistanceIndex = 0;
        for (int i = 0; i < endDistanceIndex.Length; ++i) {
            endDistanceIndex[i] = values[i].endDistance;
        }
    }

    public int count()
    {
        if (values == null)
            return 0;
        return values.Count;
    }

    public float getValue(float distance)
    {
        if (values.Count == 0)
            return 0;

        if (loop) {
            distance = distance % values[endDistanceIndex.Length - 1].endDistance;
            // reset lastEndDistanceIndex if the distance looped around
            if (distance < previousLoopDistance) {
                lastEndDistanceIndex = 0;
            }
            previousLoopDistance = distance;
        }

        return values[getIndexFromDistance(distance)].getValue(distance);
    }

    // Don't loop from the start of the list each time. Keep a cache of the latest index. This works because the 
    // list is in order from the start distance to the end distance.
    private int getIndexFromDistance(float distance)
    {
        for (int i = lastEndDistanceIndex; i < endDistanceIndex.Length; ++i) {
            if (distance <= endDistanceIndex[i]) {
                lastEndDistanceIndex = i;
                return i;
            }
        }

        return endDistanceIndex.Length - 1;
    }
}

/*
 * An object can have difference probabilities of occurring throughout the game. This class will help manage all of the probabilities
 * for a given object
 */
public class AppearanceProbability : MonoBehaviour {

    public DistanceValueList occurProbabilities;

    // probabilities that an object WON'T occur
    public DistanceValueList noOccurProbabilities;

	public void init()
	{
        occurProbabilities.init();
        noOccurProbabilities.init();
	}
	
    // Returns the probability that this object should appear based off of the current distance
	public float getProbability(float distance)
	{
		// Chance of no probability of no occur says so
		if (noOccurProbabilities.count() > 0) {
            float prob = noOccurProbabilities.getValue(distance);
			if (Random.value < prob) {
				return 0;
			}
		}

        return occurProbabilities.getValue(distance);
	}
}
