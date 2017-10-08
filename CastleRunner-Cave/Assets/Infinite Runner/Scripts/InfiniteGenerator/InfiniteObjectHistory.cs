using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Infinite Object History keeps a record of the objects spawned which is mostly used by the appearance rules
 */
public class InfiniteObjectHistory : MonoBehaviour {
	
	static public InfiniteObjectHistory instance;

    // local index is the index of the object within its own array
    // object index is the index of the object unique to all of the other objects (array independent)
	// spawn index is the index of the object spawned within its own object type.
    // 
	// For example, the following objects would have the corresponding local, object and spawn indexes:
    //
    // Name			Local Index		Object Index	Spawn Index     Notes
	// PlatformA	0				0 				0               
	// ObstacleA	0				3				0               ObstacleA has an object index of 3 because it is the third object in the complete object array:
	// PlatformB	1				1				1                   PlatformA, PlatformB, PlatformC, ObstacleA, ...
	// ObstacleA	0				3				1
	// PlatformC	2				2				2
	// ObstacleB	1				4				2
	// ObstacleC	2				5				3
	// PlatformA	0				0				3
	// ObstacleA	0				3				4
	// ObstacleC	2				5				5
	// PlatformC	2				2				4
	
    // The relative location of the objects being spawned: Center, Right, Left
	private ObjectLocation activeObjectLocation;

    // Spawn index for the given object index
	private List<int>[] objectSpawnIndex;
    // Spawn index for the given object type
	private int[][] objectTypeSpawnIndex;

    // local index for the given object type
	private int[][] lastLocalIndex;
    // spawn location (distance) for the given object index
	private List<float>[] lastObjectSpawnDistance;
    // distance of the last spawned object for the given object type
	private float[][] latestObjectTypeSpawnDistance;
	
    // The total distance spawned for both platforms and scenes
    private float[] totalDistance;
	private float[] totalSceneDistance;

    // distance at which the platform spawned. Indexes will be removed from this list when a scene object has spawned over it.
    private PlatformDistanceIndexMap[] platformDistanceIndexMap;

    // Keep track of the top-most and bottom-most objects in the scene hierarchy. When a new object is spawned, it is placed as the parent of the respective previous
    // objects. When the generator moves the platforms and scenes, it will only need to move the top-most object. It will also only need to check the bottom-most object
    // to see if it needs to be removed
    private InfiniteObject[] topPlatformObjectSpawned;
    private InfiniteObject[] bottomPlatformObjectSpawned;
    private InfiniteObject[] topSceneObjectSpawned;
    private InfiniteObject[] bottomSceneObjectSpawned;
    private InfiniteObject topTurnPlatformObjectSpawned;
    private InfiniteObject topTurnSceneObjectSpawned;
    private InfiniteObject bottomTurnPlatformObjectSpawned;
    private InfiniteObject bottomTurnSceneObjectSpawned;

    private InfiniteObject savedInfiniteObjects;

    // the previous section that occurred
    private int previousSection;
    private bool[] spawnedPlatformSectionTransition;
    private bool[] spawnedSceneSectionTransition;

	private InfiniteObjectManager infiniteObjectManager;
	
	public void Awake()
	{
		instance = this;
	}
	
	public void init(int objectCount)
	{
		activeObjectLocation = ObjectLocation.Center;
		objectSpawnIndex = new List<int>[(int)ObjectLocation.Last];
		objectTypeSpawnIndex = new int[(int)ObjectLocation.Last][];
		lastLocalIndex = new int[(int)ObjectLocation.Last][];
        latestObjectTypeSpawnDistance = new float[(int)ObjectLocation.Last][];
		
		lastObjectSpawnDistance = new List<float>[(int)ObjectLocation.Last];
		
		totalDistance = new float[(int)ObjectLocation.Last];
		totalSceneDistance = new float[(int)ObjectLocation.Last];

        platformDistanceIndexMap = new PlatformDistanceIndexMap[(int)ObjectLocation.Last];

        topPlatformObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
        bottomPlatformObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
        topSceneObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];
        bottomSceneObjectSpawned = new InfiniteObject[(int)ObjectLocation.Last];

        spawnedPlatformSectionTransition = new bool[(int)ObjectLocation.Last];
        spawnedSceneSectionTransition = new bool[(int)ObjectLocation.Last];

		for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
			objectSpawnIndex[i] = new List<int>();
			objectTypeSpawnIndex[i] = new int[(int)ObjectType.Last];
			lastLocalIndex[i] = new int[(int)ObjectType.Last];
			latestObjectTypeSpawnDistance[i] = new float[(int)ObjectType.Last];

            lastObjectSpawnDistance[i] = new List<float>();

            platformDistanceIndexMap[i] = new PlatformDistanceIndexMap();
			
			for (int j = 0; j < objectCount; ++j) {
				objectSpawnIndex[i].Add(-1);
				lastObjectSpawnDistance[i].Add(0);
			}
			for (int j = 0; j < (int)ObjectType.Last; ++j) {
				objectTypeSpawnIndex[i][j] = -1;
				lastLocalIndex[i][j] = -1;
				latestObjectTypeSpawnDistance[i][j] = -1;
			}
		}
		
		infiniteObjectManager = InfiniteObjectManager.instance;
	}
	
	// get the object history prepped for a new turn
	public void resetTurnCount()
	{
		for (int i = 0; i < objectSpawnIndex[(int)ObjectLocation.Center].Count; ++i) {
            objectSpawnIndex[(int)ObjectLocation.Left][i] = objectSpawnIndex[(int)ObjectLocation.Right][i] = objectSpawnIndex[(int)ObjectLocation.Center][i];
            lastObjectSpawnDistance[(int)ObjectLocation.Left][i] = lastObjectSpawnDistance[(int)ObjectLocation.Right][i] = lastObjectSpawnDistance[(int)ObjectLocation.Center][i];
		}
		
		for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
			objectTypeSpawnIndex[(int)ObjectLocation.Left][i] = objectTypeSpawnIndex[(int)ObjectLocation.Right][i] = objectTypeSpawnIndex[(int)ObjectLocation.Center][i];
			lastLocalIndex[(int)ObjectLocation.Left][i] = lastLocalIndex[(int)ObjectLocation.Right][i] = lastLocalIndex[(int)ObjectLocation.Center][i];
			latestObjectTypeSpawnDistance[(int)ObjectLocation.Left][i] = latestObjectTypeSpawnDistance[(int)ObjectLocation.Right][i] = latestObjectTypeSpawnDistance[(int)ObjectLocation.Center][i];
        }
		
		totalDistance[(int)ObjectLocation.Left] = totalDistance[(int)ObjectLocation.Right] = totalDistance[(int)ObjectLocation.Center];
		// on a turn, the scene catches up to the platforms, so the total scene distance equals the total distance
        totalSceneDistance[(int)ObjectLocation.Left] = totalSceneDistance[(int)ObjectLocation.Right] = totalDistance[(int)ObjectLocation.Center];

        platformDistanceIndexMap[(int)ObjectLocation.Left].reset();
        platformDistanceIndexMap[(int)ObjectLocation.Right].reset();

        spawnedPlatformSectionTransition[(int)ObjectLocation.Left] = spawnedPlatformSectionTransition[(int)ObjectLocation.Right] = spawnedPlatformSectionTransition[(int)ObjectLocation.Center];
        spawnedSceneSectionTransition[(int)ObjectLocation.Left] = spawnedSceneSectionTransition[(int)ObjectLocation.Right] = spawnedSceneSectionTransition[(int)ObjectLocation.Center];
	}
	
    // set the new active location
	public void setActiveLocation(ObjectLocation location)
	{
		activeObjectLocation = location;
	}
	
	// the player has turned left or right. Replace the center values with the corresponding turn values if they aren't -1
	public void turn(ObjectLocation location)
	{
        for (int i = 0; i < objectSpawnIndex[(int)ObjectLocation.Center].Count; ++i) {
            lastObjectSpawnDistance[(int)ObjectLocation.Center][i] = lastObjectSpawnDistance[(int)location][i];
			if (objectSpawnIndex[(int)location][i] != -1) {				
				objectSpawnIndex[(int)ObjectLocation.Center][i] = objectSpawnIndex[(int)location][i];
			}
		}
		
		for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
			if (objectTypeSpawnIndex[(int)location][i] != -1) {
				objectTypeSpawnIndex[(int)ObjectLocation.Center][i] = objectTypeSpawnIndex[(int)location][i];
			}
			
			lastLocalIndex[(int)ObjectLocation.Center][i] = lastLocalIndex[(int)location][i];
			latestObjectTypeSpawnDistance[(int)ObjectLocation.Center][i] = latestObjectTypeSpawnDistance[(int)location][i];
        }

        totalDistance[(int)ObjectLocation.Center] = totalDistance[(int)location];
        totalSceneDistance[(int)ObjectLocation.Center] = totalSceneDistance[(int)location];

        platformDistanceIndexMap[(int)ObjectLocation.Center] = platformDistanceIndexMap[(int)location];

        spawnedPlatformSectionTransition[(int)ObjectLocation.Center] = spawnedPlatformSectionTransition[(int)location];
        spawnedSceneSectionTransition[(int)ObjectLocation.Center] = spawnedSceneSectionTransition[(int)location];

        // use the center location if there aren't any objects in the location across from the turn location
        ObjectLocation acrossLocation = (location == ObjectLocation.Right ? ObjectLocation.Left : ObjectLocation.Right);
        if (bottomPlatformObjectSpawned[(int)acrossLocation] == null) {
            acrossLocation = ObjectLocation.Center;
        }

        bottomTurnPlatformObjectSpawned = bottomPlatformObjectSpawned[(int)acrossLocation];
        bottomTurnSceneObjectSpawned = bottomSceneObjectSpawned[(int)acrossLocation];
        topTurnPlatformObjectSpawned = topPlatformObjectSpawned[(int)ObjectLocation.Center];
        topTurnSceneObjectSpawned = topSceneObjectSpawned[(int)ObjectLocation.Center];

        topPlatformObjectSpawned[(int)ObjectLocation.Center] = topPlatformObjectSpawned[(int)location];
        bottomPlatformObjectSpawned[(int)ObjectLocation.Center] = bottomPlatformObjectSpawned[(int)location];
        topSceneObjectSpawned[(int)ObjectLocation.Center] = topSceneObjectSpawned[(int)location];
        bottomSceneObjectSpawned[(int)ObjectLocation.Center] = bottomSceneObjectSpawned[(int)location];

        for (int i = (int)ObjectLocation.Left; i < (int)ObjectLocation.Last; ++i) {
            topPlatformObjectSpawned[i] = null;
            bottomPlatformObjectSpawned[i] = null;
            topSceneObjectSpawned[i] = null;
            bottomSceneObjectSpawned[i] = null;
        }
	}
	
    // Keep track of the object spawned. Returns the previous object at the top position
	public InfiniteObject objectSpawned(int index, float locationOffset, ObjectLocation location, ObjectType objectType, InfiniteObject infiniteObject = null)
	{
		lastObjectSpawnDistance[(int)location][index] = (objectType == ObjectType.Scene ? totalSceneDistance[(int)location] : totalDistance[(int)location]) + locationOffset;
		objectTypeSpawnIndex[(int)location][(int)objectType] += 1;
		objectSpawnIndex[(int)location][index] = objectTypeSpawnIndex[(int)location][(int)objectType];
		latestObjectTypeSpawnDistance[(int)location][(int)objectType] = lastObjectSpawnDistance[(int)location][index];
		lastLocalIndex[(int)location][(int)objectType] = infiniteObjectManager.objectIndexToLocalIndex(index, objectType);

        InfiniteObject prevTopObject = null;
        if (objectType == ObjectType.Platform) {
            prevTopObject = topPlatformObjectSpawned[(int)location];
            topPlatformObjectSpawned[(int)location] = infiniteObject;
        } else if (objectType == ObjectType.Scene) {
            prevTopObject = topSceneObjectSpawned[(int)location];
            topSceneObjectSpawned[(int)location] = infiniteObject;
        }

        return prevTopObject;
	}

    // the bottom infinite object only needs to be set for the very first object at the object location.. objectRemoved will otherwise take care of making sure the
    // bottom object is correct
    public void setBottomInfiniteObject(ObjectLocation location, bool isSceneObject, InfiniteObject infiniteObject)
    {
        if (isSceneObject) {
            bottomSceneObjectSpawned[(int)location] = infiniteObject;
        } else {
            bottomPlatformObjectSpawned[(int)location] = infiniteObject;
        }
    }

    public void setTopInfiniteObject(ObjectLocation location, bool isSceneObject, InfiniteObject infiniteObject)
    {
        if (isSceneObject) {
            topSceneObjectSpawned[(int)location] = infiniteObject;
        } else {
            topPlatformObjectSpawned[(int)location] = infiniteObject;
        }
    }

    public void objectRemoved(ObjectLocation location, bool isSceneObject)
    {
        if (isSceneObject) {
            bottomSceneObjectSpawned[(int)location] = bottomSceneObjectSpawned[(int)location].getInfiniteObjectParent();
        } else {
            bottomPlatformObjectSpawned[(int)location] = bottomPlatformObjectSpawned[(int)location].getInfiniteObjectParent();
        }
    }

    public void turnObjectRemoved(bool isSceneObject)
    {
        if (isSceneObject) {
            bottomTurnSceneObjectSpawned = bottomTurnSceneObjectSpawned.getInfiniteObjectParent();
            if (bottomTurnSceneObjectSpawned == null) {
                topTurnSceneObjectSpawned = null;
            }
        } else {
            bottomTurnPlatformObjectSpawned = bottomTurnPlatformObjectSpawned.getInfiniteObjectParent();
            if (bottomTurnPlatformObjectSpawned == null) {
                topTurnPlatformObjectSpawned = null;
            }
        }
    }
	
    // Increase the distance travelled by the specified amount
	public void addTotalDistance(float amount, ObjectLocation location, bool isSceneObject)
	{
        if (isSceneObject) {
            totalSceneDistance[(int)location] += amount;
            // truncate to prevent precision errors
            totalSceneDistance[(int)location] = ((int)(totalSceneDistance[(int)location] * 1000f)) / 1000f;
            platformDistanceIndexMap[(int)location].checkForRemoval(totalSceneDistance[(int)location]);
        } else {
            platformDistanceIndexMap[(int)location].addDistanceIndex(totalDistance[(int)location], lastLocalIndex[(int)location][(int)ObjectType.Platform]);
            totalDistance[(int)location] += amount;
            // truncate to prevent precision errors
            totalDistance[(int)location] = ((int)(totalDistance[(int)location] * 1000f)) / 1000f;
        }
	}
	
	// returns the spawn index for the given object type
	public int getObjectTypeSpawnIndex(ObjectType objectType)
	{
		return objectTypeSpawnIndex[(int)activeObjectLocation][(int)objectType];
	}
	
	// returns the spawn index for the given object index
	public int getObjectSpawnIndex(int index)
	{
		return objectSpawnIndex[(int)activeObjectLocation][index];
	}
	
	// returns the local index for the given object type
	public int getLastLocalIndex(ObjectType objectType)
	{
		return lastLocalIndex[(int)activeObjectLocation][(int)objectType];
	}
	
	// returns the spawn location (distance) for the given object index
	public float getSpawnDistance(int index)
	{
		return lastObjectSpawnDistance[(int)activeObjectLocation][index];
	}
	
	// returns the distance of the last spawned object for the given object type
	public float getLastObjectTypeSpawnDistance(ObjectType objectType)
	{
		return latestObjectTypeSpawnDistance[(int)activeObjectLocation][(int)objectType];
	}
	
	// returns the total distance for a scene object or platform object
	public float getTotalDistance(bool isSceneObject)
	{
		return (isSceneObject ? totalSceneDistance[(int)activeObjectLocation] : totalDistance[(int)activeObjectLocation]);
	}

    public int getFirstPlatformIndex()
    {
        return platformDistanceIndexMap[(int)activeObjectLocation].firstIndex();
    }

    // returns the top-most platform or scene object
    public InfiniteObject getTopInfiniteObject(ObjectLocation location, bool isSceneObject)
    {
        return (isSceneObject ? topSceneObjectSpawned[(int)location] : topPlatformObjectSpawned[(int)location]);
    }

    // returns the bottom-most platform or scene object
    public InfiniteObject getBottomInfiniteObject(ObjectLocation location, bool isSceneObject)
    {
        return (isSceneObject ? bottomSceneObjectSpawned[(int)location] : bottomPlatformObjectSpawned[(int)location]);
    }

    // returns the top-most turn platform or scene object
    public InfiniteObject getTopTurnInfiniteObject(bool isSceneObject)
    {
        return (isSceneObject ? topTurnSceneObjectSpawned : topTurnPlatformObjectSpawned);
    }

    // returns the bottom-most turn platform or scene object
    public InfiniteObject getBottomTurnInfiniteObject(bool isSceneObject)
    {
        return (isSceneObject ? bottomTurnSceneObjectSpawned : bottomTurnPlatformObjectSpawned);
    }
	
	// set everything back to 0 for a new game
    public void saveObjectsReset()
	{
        // save off the current objects. They will be deactivated after new objects have been sapwned
        if (topPlatformObjectSpawned[(int)ObjectLocation.Center] != null) {
            savedInfiniteObjects = topPlatformObjectSpawned[(int)ObjectLocation.Center];
            
            for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
                if (i != (int)ObjectLocation.Center && topPlatformObjectSpawned[i])
                    topPlatformObjectSpawned[i].setInfiniteObjectParent(savedInfiniteObjects);
                if (bottomPlatformObjectSpawned[i] != null)
                    bottomPlatformObjectSpawned[i].setInfiniteObjectParent(savedInfiniteObjects);
                if (topSceneObjectSpawned[i] != null)
                    topSceneObjectSpawned[i].setInfiniteObjectParent(savedInfiniteObjects);
                if (bottomSceneObjectSpawned[i] != null)
                    bottomSceneObjectSpawned[i].setInfiniteObjectParent(savedInfiniteObjects);

                if (topTurnPlatformObjectSpawned != null)
                    topTurnPlatformObjectSpawned.setInfiniteObjectParent(savedInfiniteObjects);
            }
        } else {
            // topPlatformObjectSpawned is null when the player turns the wrong way off of a turn
            savedInfiniteObjects = topTurnPlatformObjectSpawned;
        }

        if (bottomTurnPlatformObjectSpawned != null)
            bottomTurnPlatformObjectSpawned.setInfiniteObjectParent(savedInfiniteObjects);
        if (topTurnSceneObjectSpawned != null)
            topTurnSceneObjectSpawned.setInfiniteObjectParent(savedInfiniteObjects);
        if (bottomTurnSceneObjectSpawned != null)
            bottomTurnSceneObjectSpawned.setInfiniteObjectParent(savedInfiniteObjects);

		activeObjectLocation = ObjectLocation.Center;
		for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
			totalDistance[i] = 0;
			totalSceneDistance[i] = 0;
            platformDistanceIndexMap[i].reset();

            topPlatformObjectSpawned[i] = bottomPlatformObjectSpawned[i] = null;
            topSceneObjectSpawned[i] = bottomSceneObjectSpawned[i] = null;

            spawnedSceneSectionTransition[i] = true;
            spawnedPlatformSectionTransition[i] = true;
            previousSection = 0;

			for (int j = 0; j < objectSpawnIndex.Length; ++j) {
				objectSpawnIndex[i][j] = -1;
				lastObjectSpawnDistance[i][j] = 0;
			}
			for (int j = 0; j < (int)ObjectType.Last; ++j) {
				objectTypeSpawnIndex[i][j] = -1;
				lastLocalIndex[i][j] = -1;
				latestObjectTypeSpawnDistance[i][j] = -1;
			}
		}

        topTurnPlatformObjectSpawned = bottomTurnPlatformObjectSpawned = null;
        topTurnSceneObjectSpawned = bottomTurnSceneObjectSpawned = null;
	}

    public InfiniteObject getSavedInfiniteObjects()
    {
        return savedInfiniteObjects;
    }

    public void setPreviousSection(int section, bool isSceneObject)
    {
        previousSection = section;
        for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
            if (isSceneObject) {
                spawnedSceneSectionTransition[i] = false;
            } else {
                spawnedPlatformSectionTransition[i] = false;
            }
        }
    }

    public int getPreviousSection()
    {
        return previousSection;
    }

    public void didSpawnSectionTranition(ObjectLocation location, bool isSceneObject)
    {
        if (isSceneObject) {
            spawnedSceneSectionTransition[(int)location] = true;
        } else {
            spawnedPlatformSectionTransition[(int)location] = true;
        }
    }

    public bool hasSpawnedSectionTransition(ObjectLocation location, bool isSceneObject)
    {
        return (isSceneObject ? spawnedSceneSectionTransition[(int)location] : spawnedPlatformSectionTransition[(int)location]);
    }

    // For persisting the data:
    public void saveInfiniteObjectPersistence(ref InfiniteObjectPersistence persistence)
    {
        persistence.totalDistance = totalDistance;
        persistence.totalSceneDistance = totalSceneDistance;
        
        int objectCount = objectSpawnIndex[0].Count;
        persistence.objectSpawnIndex = new int[(int)ObjectLocation.Last * objectCount];
        persistence.lastObjectSpawnDistance = new float[(int)ObjectLocation.Last * objectCount];
        persistence.objectTypeSpawnIndex = new int[(int)ObjectLocation.Last * (int)ObjectType.Last];
        persistence.lastLocalIndex = new int[(int)ObjectLocation.Last * (int)ObjectType.Last];
        persistence.latestObjectTypeSpawnDistance = new float[(int)ObjectLocation.Last * (int)ObjectType.Last];

        int width = (int)ObjectLocation.Last;
        for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
            for (int j = 0; j < objectCount; ++j) {
                persistence.objectSpawnIndex[i * width + j] = objectSpawnIndex[i][j];
                persistence.lastObjectSpawnDistance[i * width + j] = lastObjectSpawnDistance[i][j];
            }
            for (int j = 0; j < (int)ObjectType.Last; ++j) {
                persistence.objectTypeSpawnIndex[i * width + j] = objectTypeSpawnIndex[i][j];
                persistence.lastLocalIndex[i * width + j] = lastLocalIndex[i][j];
                persistence.latestObjectTypeSpawnDistance[i * width + j] = latestObjectTypeSpawnDistance[i][j];
            }
        }
    }

    public void loadInfiniteObjectPersistence(InfiniteObjectPersistence persistence)
    {
        totalDistance = persistence.totalDistance;
        totalSceneDistance = persistence.totalSceneDistance;

        int objectCount = objectSpawnIndex[0].Count;
        int width = (int)ObjectLocation.Last;
        for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
            for (int j = 0; j < objectCount; ++j) {
                objectSpawnIndex[i][j] = persistence.objectSpawnIndex[i * width + j];
                lastObjectSpawnDistance[i][j] = persistence.lastObjectSpawnDistance[i * width + j];
            }
            for (int j = 0; j < (int)ObjectType.Last; ++j) {
                objectTypeSpawnIndex[i][j] = persistence.objectTypeSpawnIndex[i * width + j];
                lastLocalIndex[i][j] = persistence.lastLocalIndex[i * width + j];
                latestObjectTypeSpawnDistance[i][j] = persistence.latestObjectTypeSpawnDistance[i * width + j];
            }
        }
    }
}

/**
 * Maps the platform distance to a local platform index. Used by the scenes to be able to determine which platform they are spawning near
 */
[System.Serializable]
public class PlatformDistanceIndexMap
{
    List<float> distances;
    List<int> localIndexes;

    public PlatformDistanceIndexMap()
    {
        distances = new List<float>();
        localIndexes = new List<int>();
    }

    // a new platform has been spawned, add the distance
    public void addDistanceIndex(float distance, int index)
    {
        distances.Add(distance);
        localIndexes.Add(index);
    }

    // remove the distance/index if the scene distance is greater than the earliest platform distance
    public void checkForRemoval(float distance)
    {
        if (distances.Count > 0) {
            if (distances[0] <= distance) {
                distances.RemoveAt(0);
                localIndexes.RemoveAt(0);
            }
        }
    }

    // returns the first platform index who doesnt have a scene spawned near it
    public int firstIndex()
    {
        if (localIndexes.Count > 0) {
            return localIndexes[0];
        }
        return -1;
    }

    // returns the first platform distance who doesnt have a scene spawned near it
    public float firstDistance()
    {
        if (distances.Count > 0) {
            return distances[0];
        }
        return -1;
    }

    public void reset()
    {
        distances.Clear();
        localIndexes.Clear();
    }
}
