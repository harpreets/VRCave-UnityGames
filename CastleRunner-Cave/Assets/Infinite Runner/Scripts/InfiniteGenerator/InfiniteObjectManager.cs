using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct ObjectSpawnData {
	public float largestScene;
    public bool useWidthBuffer;
    public int section;
    public bool sectionTransition;
    public int prevSection;
}

/*
 * Used in conjuction with the infinite object generator, the manager keeps track of all of the objects. The infinite object generator requests a new
 * object through getNextObjectIndex/objectFromPool and the object manager will return the object from the object pool based on the appearance rules/
 * probability.
 */

public class InfiniteObjectManager : MonoBehaviour {
	
	static public InfiniteObjectManager instance;
	
	// Platforms:
	public InfiniteObject[] platforms;
	public Transform platformParent;
	
	// Scene prefabs:
	public InfiniteObject[] scenes;
	public Transform sceneParent;
	
	// Obstacles:
	public InfiniteObject[] obstacles;
	public Transform obstacleParent;
	
	// Coins:
	public InfiniteObject[] coins;
	public Transform coinParent;
	
	// Power ups:
	public InfiniteObject[] powerUps;
	public Transform powerUpParent;
	
	// Tutorial:
    public InfiniteObjectPersistence tutorialObjects;

    // Startup:
    public InfiniteObjectPersistence startupObjects;
	
	// Save all of the instantiated platforms in a pool to prevent instantiating and destroying objects
	private List<List<InfiniteObject>> objectsPool;
	private List<int> objectPoolIndex;
	
	private List<AppearanceRules> appearanceRules;
	private List<AppearanceProbability> appearanceProbability;
	private List<float> probabilityCache;
	private List<bool> objectCanSpawnCache;

    private InfiniteObjectHistory infiniteObjectHistory;
	
	public void Awake()
	{
		instance = this;	
	}
	
	public void init()
	{
        infiniteObjectHistory = InfiniteObjectHistory.instance;
		
		objectsPool = new List<List<InfiniteObject>>();
		objectPoolIndex = new List<int>();
		
		appearanceRules = new List<AppearanceRules>();
		appearanceProbability = new List<AppearanceProbability>();
		probabilityCache = new List<float>();
		objectCanSpawnCache = new List<bool>();
		
		int totalObjs = platforms.Length + scenes.Length + obstacles.Length + coins.Length + powerUps.Length;
        InfiniteObject infiniteObject;
		for (int i = 0; i < totalObjs; ++i) {
			objectsPool.Add(new List<InfiniteObject>());
			objectPoolIndex.Add(0);
			
			probabilityCache.Add(0);
			objectCanSpawnCache.Add(false);

            infiniteObject = objectIndexToObject(i);
            infiniteObject.init();
            appearanceRules.Add(infiniteObject.GetComponent<AppearanceRules>());
			appearanceRules[i].init();
            appearanceProbability.Add(infiniteObject.GetComponent<AppearanceProbability>());
			appearanceProbability[i].init();
		}

        // wait until all of the appearance rules have been initialized before the object index is assigned
        for (int i = 0; i < totalObjs; ++i) {
            infiniteObject = objectIndexToObject(i);
            for (int j = 0; j < totalObjs; ++j) {
                objectIndexToObject(j).GetComponent<AppearanceRules>().assignIndexToObject(infiniteObject, i);
            }
        }
	}
	
    // Measure the size of the platforms and scenes
    public void getObjectSizes(out Vector3[] platformSizes, out Vector3[] sceneSizes, out float straightPlatformWidth, out float largestSceneLength)
	{
		platformSizes = new Vector3[platforms.Length];
        straightPlatformWidth = -1;
		PlatformObject platform;
		for (int i = 0; i < platforms.Length; ++i) {
			platform = platforms[i] as PlatformObject;
            if (platform.overrideSize != Vector3.zero) {
                platformSizes[i] = platform.overrideSize;
            } else if (platform.isJump) {
				platformSizes[i] = Vector3.zero;
				platformSizes[i].z = platform.jumpLength;
			} else {
                platformSizes[i] = platforms[i].GetComponent<Renderer>().bounds.size;
                if (!platform.isLeftTurn && !platform.isRightTurn && platform.slope == PlatformSlope.None && straightPlatformWidth == -1) {
                    straightPlatformWidth = platformSizes[i].x;
                }
                Vector3 heightChange = platformSizes[i];
                if (platform.slope != PlatformSlope.None) {
                    heightChange.y *= platform.slope == PlatformSlope.Down ? -1 : 1;
                } else {
                    heightChange.y = 0;
                }
                platformSizes[i] = heightChange;
            }
		}
		
		// the parent scene object must represent the children's size
        sceneSizes = new Vector3[scenes.Length];
        largestSceneLength = 01;
        SceneObject scene;
        for (int i = 0; i < scenes.Length; ++i) {
            scene = scenes[i] as SceneObject;
            if (scene.overrideSize != Vector3.zero) {
                sceneSizes[i] = scene.overrideSize;
            } else {
                sceneSizes[i] = scenes[i].GetComponent<Renderer>().bounds.size;
                sceneSizes[i].x += scene.horizontalOffset;
            }
            if (largestSceneLength < sceneSizes[i].z) {
                largestSceneLength = sceneSizes[i].z;
            }
		}
		
		// The scene appearance rules need to know how much buffer space there is between the platform and scene
        if (sceneSizes.Length > 0) {
            float buffer = (sceneSizes[0].x - platformSizes[0].x) / 2 + platformSizes[0].x;
            for (int i = 0; i < scenes.Length; ++i) {
                scenes[i].GetComponent<SceneAppearanceRules>().init(buffer, sceneSizes[i].z);
            }
        }
	}

    // Returns the specified object from the pool
    public InfiniteObject objectFromPool(int localIndex, ObjectType objectType)
	{
		InfiniteObject obj = null;
        int objectIndex = localIndexToObjectIndex(localIndex, objectType);
		List<InfiniteObject> objectPool = objectsPool[objectIndex];
		int poolIndex = objectPoolIndex[objectIndex];
		
		// keep a start index to prevent the constant pushing and popping from the list		
		if (objectPool.Count > 0 && objectPool[poolIndex].isActive() == false) {
			obj = objectPool[poolIndex];
			objectPoolIndex[objectIndex] = (poolIndex + 1) % objectPool.Count;
			return obj;	
		}
		
		// No inactive objects, need to instantiate a new one
		InfiniteObject[] objects = null;
		switch (objectType) {
            case ObjectType.Platform:
                objects = platforms;
                break;
            case ObjectType.Scene:
                objects = scenes;
                break;
            case ObjectType.Obstacle:
                objects = obstacles;
                break;
            case ObjectType.Coin:
                objects = coins;
                break;
            case ObjectType.PowerUp:
                objects = powerUps;
                break;
		}

        obj = (GameObject.Instantiate(objects[localIndex].gameObject) as GameObject).GetComponent<InfiniteObject>();

        assignParent(obj, objectType);
        obj.setLocalIndex(localIndex);
		
		objectPool.Insert(poolIndex, obj);
		objectPoolIndex[objectIndex] = (poolIndex + 1) % objectPool.Count;
		return obj;
	}

    public void assignParent(InfiniteObject infiniteObject, ObjectType objectType)
    {
        switch (objectType) {
            case ObjectType.Platform:
                infiniteObject.setParent(platformParent);
                break;
            case ObjectType.Scene:
                infiniteObject.setParent(sceneParent);
                break;
            case ObjectType.Obstacle:
                infiniteObject.setParent(obstacleParent);
                break;
            case ObjectType.Coin:
                infiniteObject.setParent(coinParent);
                break;
            case ObjectType.PowerUp:
                infiniteObject.setParent(powerUpParent);
                break;
		}
    }
    
    // Converts local index to object index
    public int localIndexToObjectIndex(int localIndex, ObjectType objectType)
	{
		switch(objectType) {
		    case ObjectType.Platform:
			    return localIndex;
		    case ObjectType.Scene:
			    return platforms.Length + localIndex;
		    case ObjectType.Obstacle:
                return platforms.Length + scenes.Length + localIndex;
		    case ObjectType.Coin:
                return platforms.Length + scenes.Length + obstacles.Length + localIndex;
		    case ObjectType.PowerUp:
                return platforms.Length + scenes.Length + obstacles.Length + coins.Length + localIndex;
		}
		return -1; // error
	}
    // Converts object index to local index
	public int objectIndexToLocalIndex(int objectIndex, ObjectType objectType)
	{
		switch(objectType) {
		    case ObjectType.Platform:
			    return objectIndex;
		    case ObjectType.Scene:
			    return objectIndex - platforms.Length;
		    case ObjectType.Obstacle:
                return objectIndex - platforms.Length - scenes.Length;
		    case ObjectType.Coin:
                return objectIndex - platforms.Length - scenes.Length - obstacles.Length;
		    case ObjectType.PowerUp:
                return objectIndex - platforms.Length - scenes.Length - obstacles.Length - coins.Length;
		}
		return -1; // error	
	}

    public InfiniteObject localIndexToInfiniteObject(int localIndex, ObjectType objectType)
    {
        switch (objectType) {
            case ObjectType.Platform:
                return platforms[localIndex];
            case ObjectType.Scene:
                return scenes[localIndex];
            case ObjectType.Obstacle:
                return obstacles[localIndex];
            case ObjectType.Coin:
                return coins[localIndex];
            case ObjectType.PowerUp:
                return powerUps[localIndex];
        }
        return null; // error	
    }
	
    // Returns the number of total objects
	public int getTotalObjectCount()
	{
        return platforms.Length + scenes.Length + obstacles.Length + coins.Length + powerUps.Length;	
	}
	
    // Converts the object index to an infinite object
	private InfiniteObject objectIndexToObject(int objectIndex)
	{
		if (objectIndex < platforms.Length) {
			return platforms[objectIndex];
        } else if (objectIndex < platforms.Length + scenes.Length) {
            return scenes[objectIndex - platforms.Length];
        } else if (objectIndex < platforms.Length + scenes.Length + obstacles.Length) {
            return obstacles[objectIndex - platforms.Length - scenes.Length];
        } else if (objectIndex < platforms.Length + scenes.Length + obstacles.Length + coins.Length) {
            return coins[objectIndex - platforms.Length - scenes.Length - obstacles.Length];
        } else if (objectIndex < platforms.Length + scenes.Length + obstacles.Length + coins.Length + powerUps.Length) {
            return powerUps[objectIndex - platforms.Length - scenes.Length - obstacles.Length - coins.Length];
		}
		return null;
	}
		
	/**
     * The next platform is determined by probabilities as well as object rules.
	 * spawnData contains any extra data that is needed to make a decision if the object can be spawned
	 */
	public int getNextObjectIndex(ObjectType objectType, ObjectSpawnData spawnData)
	{
		InfiniteObject[] objects = null;
		switch(objectType) {
		case ObjectType.Platform:
			objects = platforms;
			break;
		case ObjectType.Scene:
            objects = scenes;
			break;
		case ObjectType.Obstacle:
			objects = obstacles;
			break;
		case ObjectType.Coin:
			objects = coins;
			break;
		case ObjectType.PowerUp:
			objects = powerUps;
			break;
		}
		float totalProbability = 0;
        float distance = infiniteObjectHistory.getTotalDistance(objectType == ObjectType.Scene);
		int objectIndex;
		for (int localIndex = 0; localIndex < objects.Length; ++localIndex) {
			objectIndex = localIndexToObjectIndex(localIndex, objectType);
			// cache the result
            objectCanSpawnCache[objectIndex] = appearanceRules[objectIndex].canSpawnObject(distance, spawnData);
			if (!objectCanSpawnCache[objectIndex]) {
				continue;
			}

            probabilityCache[objectIndex] = appearanceProbability[objectIndex].getProbability(distance) * appearanceRules[objectIndex].probabilityAdjustment(distance);
			totalProbability += probabilityCache[objectIndex];
		}
		
		// chance of spawning nothing (especially in the case of collidable objects)
		if (totalProbability == 0) {
			return -1;
		}
		
		float randomValue = Random.value;
		float prevObjProbability = 0;
		float objProbability = 0;
		// with the total probability we can determine a platform
		// minor optimization: don't check the last platform. If we get that far into the loop then regardless we are selecting that platform
		for (int localIndex = 0; localIndex < objects.Length - 1; ++localIndex) { 
			objectIndex = localIndexToObjectIndex(localIndex, objectType);
			if (!objectCanSpawnCache[objectIndex]) {
				continue;
			}
			
			objProbability = probabilityCache[objectIndex];
			if (randomValue <= (prevObjProbability + objProbability) / totalProbability) {
				return localIndex;
			}
			prevObjProbability += objProbability;
		}
		return objects.Length - 1;
	}
	
    public GameObject createStartupObjects(bool tutorial)
    {
        InfiniteObjectPersistence prefab = (tutorial ? tutorialObjects : startupObjects);
        if (prefab != null) {
            return GameObject.Instantiate(prefab.gameObject) as GameObject;
        }
        return null;
    }
}
