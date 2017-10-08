using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ScoreObjectCountRule
{
    // True if the minimum distance applies to any object of the same type as the object this script is attached to
    public bool minDistanceSameObjectType;

	// Minimum distance is the minimum distance that two objects can spawn next to each other.
	// For example:
	//	Object			Location
	//	PlatformA		50
	//	PlatformB		60
	//	PlatformC		65
	//	PlatformB		70
	//	PlatformA		75
	// If PlatformB had a minimum distance of 15 then it would not have been able to spawn the second time. In the case
	// of PlatformA, it could be considered to have a minimum distance of 25. minDistance doesn't have to apply to two of the
	// same objects. In this example, PlatformA could also have a minimum distance of 10 from PlatformC.
	public int minDistance;
	
	// The number of objects which must be in between two objects
	// Consider the same order of platforms as the last example:
	//	Object			Spawn Index
	//	PlatformA		5
	//	PlatformB		6
	//	PlatformC		7
	//	PlatformB		8
	//	PlatformA		9
	// In this example, PlatformA could have a minimum object count of 3 when compared to itself. Similarly, PlatformC may have had
	// a mimimum object count of 1 compared to PlatformA. 
    public int minObjectSeparation;

    public DistanceValue probability;
	
	private InfiniteObjectHistory infiniteObjectHistory;

    public ScoreObjectCountRule(int md, bool mdsot, int mos, DistanceValue p)
    {
        minDistance = md;
        minDistanceSameObjectType = mdsot;
        minObjectSeparation = mos;
        probability = p;
    }
	
	public void init(InfiniteObjectHistory objectHistory)
	{
		infiniteObjectHistory = objectHistory;	
	}

    public bool canSpawnObject(float distance, ObjectType thisObjectType, int targetObjectIndex, ObjectType targetObjectType)
    {
        // return true if the parameters do not apply to the current distance
        if (!probability.withinDistance(distance))
			return true;

        // The target object doesn't matter if we are using objects of the same object type
		float totalDistance = infiniteObjectHistory.getTotalDistance(thisObjectType == ObjectType.Scene);
        if (minDistanceSameObjectType) {
            // lastSpawnDistance: the distance of the last object spawned of the inputted object type
            float lastSpawnDistance = infiniteObjectHistory.getLastObjectTypeSpawnDistance(thisObjectType);
            if (totalDistance - lastSpawnDistance <= minDistance) {
                return false;
            }
        }

        // The rest of the tests need the target object, so if there is no target object then we are done early
        if (targetObjectIndex == -1)
            return true;

		// objectSpawnIndex: spawn index of the last object of the same type (for example, the last duck obstacle spawn index)
        int objectSpawnIndex = infiniteObjectHistory.getObjectSpawnIndex(targetObjectIndex);
		// can always spawn if the object hasn't been spawned before and it is within the probabilities
		if (objectSpawnIndex == -1)
			return true;
		
		// latestSpawnIndex: spawn index of the latest object type
        int latestSpawnIndex = infiniteObjectHistory.getObjectTypeSpawnIndex(targetObjectType);
		// can't spawn if there isn't enough object separation
        if (latestSpawnIndex - objectSpawnIndex <= minObjectSeparation)
			return false;
		
		// objectLastDistance: distance of the last spawned object of the same type
		float objectLastDistance = infiniteObjectHistory.getSpawnDistance(targetObjectIndex);
		// can't spawn if we are too close to another object
		if (totalDistance - objectLastDistance <= minDistance)
			return false;
		
		// looks like we can spawn
		return true;
	}
	
	// probability adjustment is the opposite of can spawn object. Only return the adjusted probability of the object in question is within the object
	// count / distance specified. Otherwise, if it is outside of that range, return -1 meaning no adjustment.
    public float probabilityAdjustment(float distance, int targetObjectIndex, ObjectType targetObjectType)
    {
        // objectSpawnIndex: spawn index of the last object of the same type (for example, the last duck obstacle spawn index)
        int objectSpawnIndex = infiniteObjectHistory.getObjectSpawnIndex(targetObjectIndex);
        // No probability adjustment if the target object hasn't even spawned yet
        if (objectSpawnIndex == -1)
            return -1;

        // latestSpawnIndex: spawn index of the latest object type
        int latestSpawnIndex = infiniteObjectHistory.getObjectTypeSpawnIndex(targetObjectType);
        // No probability adjustment if we are outside the range of the minimum object separation
        if (minObjectSeparation != 0 && latestSpawnIndex - objectSpawnIndex > minObjectSeparation)
            return -1;


        float totalDistance = infiniteObjectHistory.getTotalDistance(targetObjectType == ObjectType.Scene);
        // objectLastDistance: distance of the last spawned object of the same type
        float objectLastDistance = infiniteObjectHistory.getSpawnDistance(targetObjectIndex);
        // No probability adjustment if we are outside the range of the minimum distance
        if (minDistance != 0 && totalDistance - objectLastDistance > minDistance)
            return -1;

        return probability.getValue(distance);
	}
}

/**
 * The object rule map links and object to its corresponding rules. The rules are described more in the class above (ScoreObjectCountRule), 
 * but the rules may prevent the object from spawning or change the probability that an object spawns based on another object occurring before it.
 */
[System.Serializable]
public class ObjectRuleMap {
	public InfiniteObject targetObject;
	public List<ScoreObjectCountRule> rules;

    private int targetObjectIndex; // the object index of the infinite object that we are interested in
    private bool targetObjectIsScene; // is the target object a scene object
    private ObjectType thisObjectType;

    public ObjectRuleMap(InfiniteObject io, ScoreObjectCountRule r)
    {
        targetObject = io;
        rules = new List<ScoreObjectCountRule>();
        rules.Add(r);
    }
	
	public void init(InfiniteObjectHistory objectHistory, ObjectType objectType)
    {
        targetObjectIndex = -1;
        thisObjectType = objectType;
		for (int i = 0; i < rules.Count; ++i) {
			rules[i].init(objectHistory);
		}	
	}
	
	public bool assignIndexToObject(InfiniteObject obj, int index)
	{
        if (targetObject == null) {
            return false;
        }

        if (obj == targetObject) {
			targetObjectIndex = index;
            targetObjectIsScene = targetObject.getObjectType() == ObjectType.Scene;
			return true;
		}
		
		return false;
	}

    // Objects may not be able to be spawned if they are too close to another object, for example
    public bool canSpawnObject(float distance)
	{
		for (int i = 0; i < rules.Count; ++i) {
            if (!rules[i].canSpawnObject(distance, thisObjectType, targetObjectIndex, (targetObject != null ? targetObject.getObjectType() : ObjectType.Last)))
				return false;
		}
		return true;
	}

    // The probability of this object occuring can be based on the previous objects spawned.
	public bool probabilityAdjustment(InfiniteObjectHistory infiniteObjectHistory, float distance, ref float localDistance, ref float probability)
	{
		for (int i = 0; i < rules.Count; ++i) {
            if ((probability = rules[i].probabilityAdjustment(distance, targetObjectIndex, targetObject.getObjectType())) != -1) {
                localDistance = infiniteObjectHistory.getTotalDistance(targetObjectIsScene) - infiniteObjectHistory.getSpawnDistance(targetObjectIndex);
				return true;
			}
		}
		return false;
	}
}

/**
 * Each object can have multiple object rules map. This class will keep track of them all and use the correct one when canSpawnObject or
 * probabilityAdjustment is called.
 */
public class AppearanceRules : MonoBehaviour {
	
	// Don't spawn an object if it is within a predefined distance of another object
	public List<ObjectRuleMap> avoidObjectRuleMaps;
	// Allows the probability of an object to be changed based on previous objects
	public List<ObjectRuleMap> probabilityAdjustmentMaps;
	
	protected InfiniteObjectHistory infiniteObjectHistory;

    private InfiniteObject infiniteObject;
	
	public virtual void init()
	{
		infiniteObjectHistory = InfiniteObjectHistory.instance;
        infiniteObject = GetComponent<InfiniteObject>();

        ObjectType objectType = infiniteObject.getObjectType();
		for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
            avoidObjectRuleMaps[i].init(infiniteObjectHistory, objectType);
		}
		
		for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
            probabilityAdjustmentMaps[i].init(infiniteObjectHistory, objectType);
		}
	}
	
	public virtual void assignIndexToObject(InfiniteObject infiniteObject, int index)
	{
		for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
			avoidObjectRuleMaps[i].assignIndexToObject(infiniteObject, index);
		}
		
		for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
			probabilityAdjustmentMaps[i].assignIndexToObject(infiniteObject, index);
		}
	}

    // Objects may not be able to be spawned if they are too close to another object, for example
    public virtual bool canSpawnObject(float distance, ObjectSpawnData spawnData)
	{
        // can't spawn if the sections don't match up
        if (!infiniteObject.canSpawnInSection(spawnData.section)) {
            return false;
        }

		for (int i = 0; i < avoidObjectRuleMaps.Count; ++i) {
            if (!avoidObjectRuleMaps[i].canSpawnObject(distance))
				return false; // all it takes is one
		}
		return true;
	}
	
	// The probability of this object occuring can be based on the previous objects spawned.
	public float probabilityAdjustment(float distance)
	{
		float closestObjectDistance = float.MaxValue;
		float closestProbabilityAdjustment = 1;
		float localDistance = 0;
		float probability = 0f;
        // Find the closest object within the probability adjustment map
		for (int i = 0; i < probabilityAdjustmentMaps.Count; ++i) {
            if (probabilityAdjustmentMaps[i].probabilityAdjustment(infiniteObjectHistory, distance, ref localDistance, ref probability)) {
                if (localDistance < closestObjectDistance) {
                    closestObjectDistance = localDistance;
					closestProbabilityAdjustment = probability;
				}
			}
		}
		return closestProbabilityAdjustment;
	}
}

/**
 * An infinite object may or may not be able to spawn with the last platform spawned. For example, it doesn't make sense to have a duck obstacle spawn on top of the
 * jump platform.
 */
[System.Serializable]
public class PlatformPlacementRule
{
    public InfiniteObject platform;
    public bool canSpawn;

    private int platformIndex;

    public PlatformPlacementRule(InfiniteObject p, bool c)
    {
        platform = p;
        canSpawn = c;
    }

    public bool assignIndexToObject(InfiniteObject infiniteObject, int index)
    {
        if (infiniteObject == platform) {
            platformIndex = index;
            return true;
        }
        return false;
    }

    public bool canSpawnObject(int index)
    {
        if (index == platformIndex) {
            return canSpawn;
        }
        return !canSpawn;
    }
}