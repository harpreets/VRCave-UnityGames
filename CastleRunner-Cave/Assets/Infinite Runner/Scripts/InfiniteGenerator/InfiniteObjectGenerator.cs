using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ObjectLocation { Center, Left, Right, Last }

/*
 * The InfiniteObjectGenerator is the controlling class of when different objects spawn.
 */
public class InfiniteObjectGenerator : MonoBehaviour 
{
	static public InfiniteObjectGenerator instance;
	
    // How far out in the distance objects spawn
	public float horizon;

    // The distance behind the camera that the objects will be removed and added back to the object pool
	public float removeHorizon;
	
	// the number of units between the slots in the track
	public float slotDistance;

    // Spawn the full length of objects, useful when creating a tutorial or startup objects
    public bool spawnFullLength;

    // the probability that no collidables will spawn on the platform
    [HideInInspector]
    public DistanceValueList noCollidableProbability;

    private SectionSelection sectionSelection;
	
	private Vector3 moveDirection;
	private float[] localDistance;
	private float[] localSceneDistance;
    private float[] localPlatformHeight;
    private float[] localSceneHeight;
    private Vector3 turnOffset;
	
	private PlatformObject[] turnPlatform;
	
	private Vector3[] platformSizes;
    private Vector3[] sceneSizes;
    private float straightPlatformWidth;
    private float largestSceneLength;

    private bool stopObjectSpawns;
	private ObjectSpawnData spawnData;
	
	private Transform cameraTransform;
    private Transform playerTransform;
	private InfiniteObjectManager infiniteObjectManager;
	private InfiniteObjectHistory infiniteObjectHistory;
	
	public void Awake()
	{
		instance = this;	
	}
	
	public void Start ()
	{
		cameraTransform = Camera.main.transform;
		infiniteObjectManager = InfiniteObjectManager.instance;
		infiniteObjectManager.init();
		infiniteObjectHistory = InfiniteObjectHistory.instance;
		infiniteObjectHistory.init(infiniteObjectManager.getTotalObjectCount());
        sectionSelection = SectionSelection.instance;
		
		moveDirection = Vector3.forward;
        turnOffset = Vector3.zero;
		turnPlatform = new PlatformObject[(int)ObjectLocation.Last];
		
		localDistance = new float[(int)ObjectLocation.Last];
        localSceneDistance = new float[(int)ObjectLocation.Last];
        localPlatformHeight = new float[(int)ObjectLocation.Last];
        localSceneHeight = new float[(int)ObjectLocation.Last];

        infiniteObjectManager.getObjectSizes(out platformSizes, out sceneSizes, out straightPlatformWidth, out largestSceneLength);

        stopObjectSpawns = false;
		spawnData = new ObjectSpawnData();
        spawnData.largestScene = largestSceneLength;
        spawnData.useWidthBuffer = true;
        spawnData.section = 0;
        spawnData.sectionTransition = false;

        noCollidableProbability.init();

        showStartupObjects(false);

        spawnObjectRun(true);
	}
	
    // creates any startup objects, returns null if no prefabs are assigned
	public bool showStartupObjects(bool tutorial)
	{
        GameObject startupObjects = infiniteObjectManager.createStartupObjects(tutorial);
        if (startupObjects == null)
            return false;

        ObjectLocation location;
        Transform objectTypeParent;
        Transform parentObject;
        Transform transformParent;
        InfiniteObject parentInfiniteObject;
        bool isSceneObject;
        for (int i = 0; i < 2; ++i) {
            isSceneObject = i == 0;
            objectTypeParent = startupObjects.transform.FindChild((isSceneObject ? "Scene" : "Platforms"));
            // iterate high to low because assignParent is going to move set a new parent thus changing the number of children in startup objects
            for (int j = objectTypeParent.childCount - 1; j >= 0; --j) {
                parentObject = objectTypeParent.GetChild(j);

                // determine the direction the object is facing based off of the y angle
                float yAngle = parentObject.eulerAngles.y;
                if (yAngle > 360 || yAngle < 0) {
                    yAngle = yAngle % 360;
                }

                if (yAngle > 269) {
                    location = ObjectLocation.Left;
                } else if (yAngle > 89) {
                    location = ObjectLocation.Right;
                } else {
                    location = ObjectLocation.Center;
                }

                infiniteObjectHistory.setTopInfiniteObject(location, isSceneObject, parentObject.GetComponent<InfiniteObject>());
                infiniteObjectManager.assignParent(parentObject.GetComponent<InfiniteObject>(), (isSceneObject ? ObjectType.Scene : ObjectType.Platform));

                InfiniteObject[] childObjects;
                if (isSceneObject) {
                    childObjects = parentObject.GetComponentsInChildren<SceneObject>() as SceneObject[];
                } else {
                    childObjects = parentObject.GetComponentsInChildren<PlatformObject>() as PlatformObject[];
                }
                for (int k = 0; k < childObjects.Length; ++k) {
                    childObjects[k].enableDestroyOnDeactivation();
                    transformParent = childObjects[k].getTransform().parent;
                    if ((parentInfiniteObject = transformParent.GetComponent<InfiniteObject>()) != null) {
                        childObjects[k].setInfiniteObjectParent(parentInfiniteObject);
                    }

                    // If there are no infinite objects under the current object, it is the last object in the hierarchy
                    if ((isSceneObject && childObjects[k].GetComponentsInChildren<SceneObject>().Length == 1) || 
                        (!isSceneObject && childObjects[k].GetComponentsInChildren<PlatformObject>().Length == 1)) {
                        infiniteObjectHistory.setBottomInfiniteObject(location, isSceneObject, childObjects[k].GetComponent<InfiniteObject>());
                    }

                    if (!isSceneObject) { // platform
                        PlatformObject platformObject = ((PlatformObject)childObjects[k]);
                        if (platformObject.isLeftTurn || platformObject.isRightTurn) {
                            turnPlatform[(int)ObjectLocation.Center] = platformObject;
                        }
                        // mark the coin objects as destructible so they get destroyed if they are collected
                        CoinObject[] coinObjects = platformObject.GetComponentsInChildren<CoinObject>() as CoinObject[];
                        for (int l = 0; l < coinObjects.Length; ++l) {
                            coinObjects[l].enableDestroyOnDeactivation();
                        }
                    }
                }
            }
        }

        // Get the persistent objects
        InfiniteObjectPersistence persistence = startupObjects.GetComponent<InfiniteObjectPersistence>();
        infiniteObjectHistory.loadInfiniteObjectPersistence(persistence);
        loadInfiniteObjectPersistence(persistence);

        // All of the important objects have been taken out, destroy the game object
        Destroy(startupObjects.gameObject);

        return true;
	}

    public void startGame()
    {
        playerTransform = PlayerController.instance.transform;
    }

	// An object run contains many platforms strung together with collidables: obstacles, power ups, and coins. If spawnObjectRun encounters a turn,
	// it will spawn the objects in the correct direction
    public void spawnObjectRun(bool activateImmediately)
	{
		while (localDistance[(int)ObjectLocation.Center] < horizon && turnPlatform[(int)ObjectLocation.Center] == null) {
            PlatformObject platform = spawnObjects(ObjectLocation.Center, localDistance[(int)ObjectLocation.Center] * moveDirection + localPlatformHeight[(int)ObjectLocation.Center] * Vector3.up + turnOffset, 
                                                   moveDirection, activateImmediately);
			if (platform == null)
                return;

            platformSpawned(platform, ObjectLocation.Center, moveDirection, Vector3.zero, activateImmediately);

            if (spawnFullLength)
                spawnObjectRun(activateImmediately);
		}

        if (turnPlatform[(int)ObjectLocation.Center] != null) {
			Vector3 turnDirection = turnPlatform[(int)ObjectLocation.Center].getTransform().right;
			
			// spawn the platform and scene objects for the left and right turns
			for (int i = 0; i < 2; ++i) {
				ObjectLocation location = (i == 0 ? ObjectLocation.Right : ObjectLocation.Left);
				
				bool canTurn = (location == ObjectLocation.Right && turnPlatform[(int)ObjectLocation.Center].isRightTurn) || 
								(location == ObjectLocation.Left && turnPlatform[(int)ObjectLocation.Center].isLeftTurn);
				if (canTurn && turnPlatform[(int)location] == null) {
					infiniteObjectHistory.setActiveLocation(location);
                    Vector3 centerDistance = (localDistance[(int)ObjectLocation.Center] + turnPlatform[(int)ObjectLocation.Center].turnLengthOffset) * moveDirection;
                    if (localDistance[(int)location] < horizon) {
                        PlatformObject platform = spawnObjects(location, centerDistance + turnDirection * localDistance[(int)location] + localPlatformHeight[(int)location] * Vector3.up + turnOffset, 
                                                               turnDirection, activateImmediately);
						if (platform == null)
							return;

                        platformSpawned(platform, location, turnDirection, centerDistance, activateImmediately);
					}
				}
				turnDirection *= -1;
			}
			
			// reset
			infiniteObjectHistory.setActiveLocation(ObjectLocation.Center);
        }
	}
	
	// spawn the platforms, obstacles, power ups, and coins
    private PlatformObject spawnObjects(ObjectLocation location, Vector3 position, Vector3 direction, bool activateImmediately)
    {
        setupSection(location, false);
		int localIndex = infiniteObjectManager.getNextObjectIndex(ObjectType.Platform, spawnData);
		if (localIndex == -1) {
			print("Unable to spawn platform. No platforms can be spawned based on the probability rules");
			return null;
		}
        PlatformObject platform = spawnPlatform(localIndex, location, position, direction, activateImmediately);

        if (platform.canSpawnCollidable() && Random.value >= noCollidableProbability.getValue(infiniteObjectHistory.getTotalDistance(false))) {
            // First try to spawn an obstacle. If there is any space remaining on the platform, then try to spawn a coin.
            // If there is still some space remaing, try to spawn a powerup.
            // An extension of this would be to randomize the order of ObjectType, but this way works if the probabilities
            // are setup fairly
            spawnCollidable(ObjectType.Obstacle, position, direction, location, platform, localIndex, activateImmediately);
            if (platform.canSpawnCollidable()) {
                spawnCollidable(ObjectType.Coin, position, direction, location, platform, localIndex, activateImmediately);
                if (platform.canSpawnCollidable()) {
                    spawnCollidable(ObjectType.PowerUp, position, direction, location, platform, localIndex, activateImmediately);
                }
            }
        }
		
		return platform;
	}
	
	// returns the length of the created platform
    private PlatformObject spawnPlatform(int localIndex, ObjectLocation location, Vector3 position, Vector3 direction, bool activateImmediately)
    {
		PlatformObject platform = (PlatformObject)infiniteObjectManager.objectFromPool(localIndex, ObjectType.Platform);
        platform.orient(position + (direction * platformSizes[localIndex].z / 2), Quaternion.LookRotation(direction));

		int objectIndex = infiniteObjectManager.localIndexToObjectIndex(localIndex, ObjectType.Platform);
        InfiniteObject prevTopPlatform = infiniteObjectHistory.objectSpawned(objectIndex, 0, location, ObjectType.Platform, platform);
		// the current platform now becames the parent of the previous top platform
        if (prevTopPlatform != null) {
            prevTopPlatform.setInfiniteObjectParent(platform);
        } else {
            infiniteObjectHistory.setBottomInfiniteObject(location, false, platform);
        }
        infiniteObjectHistory.addTotalDistance(platformSizes[localIndex].z, location, false);
        if (activateImmediately)
            platform.activate();

		return platform;
	}
	
	// a platform has been spawned, now spawn the scene objects and setup for a turn if needed
    private void platformSpawned(PlatformObject platform, ObjectLocation location, Vector3 direction, Vector3 distance, bool activateImmediately)
    {
		int localIndex;
		bool isTurn = platform.isRightTurn || platform.isLeftTurn;
        if (isTurn || spawnFullLength) {
            // set largestScene to 0 to prevent the scene spawner from waiting for space for the largest scene object
            spawnData.largestScene = 0;
            spawnData.useWidthBuffer = false;
		}

        // spawn all of the scene objects until we have spawned enough scene objects
        setupSection(location, true);
		while ((localIndex = infiniteObjectManager.getNextObjectIndex(ObjectType.Scene, spawnData)) != -1) {
            spawnSceneObject(localIndex, location, distance + localSceneDistance[(int)location] * direction + localSceneHeight[(int)location] * Vector3.up + turnOffset, direction, activateImmediately);
		}

        if (isTurn) {
            spawnData.largestScene = largestSceneLength;
            spawnData.useWidthBuffer = true;
			
			turnPlatform[(int)location] = platform;
			
			if (location == ObjectLocation.Center) {
				setupPlatformTurn();
			}
		} else {
            localDistance[(int)location] += platformSizes[infiniteObjectHistory.getLastLocalIndex(ObjectType.Platform)].z;
            localPlatformHeight[(int)location] += platformSizes[infiniteObjectHistory.getLastLocalIndex(ObjectType.Platform)].y;
            if (platform.sectionTransition) {
                infiniteObjectHistory.didSpawnSectionTranition(location, false);
            }
		}
	}

    // before platforms are about to be spawned setup the section data to ensure the correct platforms are spawned
    private void setupSection(ObjectLocation location, bool isSceneObject)
    {
        if (sectionSelection.useSectionTransitions) {
            int prevSection = sectionSelection.getActiveSection(isSceneObject);
            spawnData.section = sectionSelection.getSection(infiniteObjectHistory.getTotalDistance(isSceneObject), isSceneObject);
            if (spawnData.section != prevSection) {
                infiniteObjectHistory.setPreviousSection(prevSection, isSceneObject);
            }
            if (spawnData.section != infiniteObjectHistory.getPreviousSection() && !infiniteObjectHistory.hasSpawnedSectionTransition(location, isSceneObject)) {
                spawnData.sectionTransition = true;
                spawnData.prevSection = infiniteObjectHistory.getPreviousSection();
            } else {
                spawnData.sectionTransition = false;
            }
        }
    }
	
	// setup the distance and history for a platform turn. This should only be called when the center track has the turn that needs to spawn
	// platforms from it. Don't call it immediately on a right or left track - wait until the player turns to see which turn platforms need to be spawned from
	private void setupPlatformTurn()
	{
		int platformIndex = infiniteObjectHistory.getLastLocalIndex(ObjectType.Platform);
        float platformSize = platformSizes[platformIndex].x;
        if (turnPlatform[(int)ObjectLocation.Center].isLeftTurn && turnPlatform[(int)ObjectLocation.Center].isRightTurn)
            platformSize /= 2;
        else {
            platformSize -= straightPlatformWidth / 2;
        }
        // same with the infinite object manager, the localDistance and localSceneDistance must start out equal
        localDistance[(int)ObjectLocation.Left] = localDistance[(int)ObjectLocation.Right] = platformSize;
        localSceneDistance[(int)ObjectLocation.Left] = localSceneDistance[(int)ObjectLocation.Right] = platformSize;
        localPlatformHeight[(int)ObjectLocation.Left] = localPlatformHeight[(int)ObjectLocation.Right] = localPlatformHeight[(int)ObjectLocation.Center];
        localSceneHeight[(int)ObjectLocation.Left] = localSceneHeight[(int)ObjectLocation.Right] = localSceneHeight[(int)ObjectLocation.Center];

		infiniteObjectHistory.resetTurnCount();
	}
	
	// returns true if there is still space on the platform for a collidable object to spawn
    private void spawnCollidable(ObjectType objectType, Vector3 position, Vector3 direction, ObjectLocation location, PlatformObject platform, int platformLocalIndex, bool activateImmediately)
	{
		int collidablePositions = platform.collidablePositions;
		// can't do anything if the platform doesn't accept any collidable object spawns
		if (collidablePositions == 0)
			return;
		
		Vector3 offset = platformSizes[platformLocalIndex] * 0.1f;
		float zDelta = platformSizes[platformLocalIndex].z * .8f / (1 + collidablePositions);
		
		for (int i = 0; i < collidablePositions; ++i) {
			if (platform.canSpawnCollidable(i)) {
                int localIndex = infiniteObjectManager.getNextObjectIndex(objectType, spawnData);
				if (localIndex != -1) {
					InfiniteObject collidable = infiniteObjectManager.objectFromPool(localIndex, objectType);
                    collidable.orient(platform, position + (offset.z + ((i + 1) * zDelta)) * direction + platform.getRandomSlot() * slotDistance, Quaternion.LookRotation(direction));
					int objectIndex = infiniteObjectManager.localIndexToObjectIndex(localIndex, objectType);
                    infiniteObjectHistory.objectSpawned(objectIndex, (offset.z + ((i + 1) * zDelta)), location, objectType);
                    platform.collidableSpawned(i);
                    if (activateImmediately)
                        collidable.activate();
					
					// don't allow any more of the same collidable type if we are forcing a different collidable
					if (platform.forceDifferentCollidableTypes)
						break;
				}
			}
		}
	}
	
    // spawn a scene object at the specified location
    private void spawnSceneObject(int localIndex, ObjectLocation location, Vector3 position, Vector3 direction, bool activateImmediately)
	{
        SceneObject scene = (SceneObject)infiniteObjectManager.objectFromPool(localIndex, ObjectType.Scene);
        scene.orient(position + direction * sceneSizes[localIndex].z / 2, Quaternion.LookRotation(direction));

        localSceneDistance[(int)location] += sceneSizes[localIndex].z;
        int assocaitedPlatformLocalIndex = infiniteObjectHistory.getFirstPlatformIndex();
        PlatformObject associatedPlatform = infiniteObjectManager.localIndexToInfiniteObject(assocaitedPlatformLocalIndex, ObjectType.Platform) as PlatformObject;
        if (associatedPlatform.slope != PlatformSlope.None) {
            localSceneHeight[(int)location] += platformSizes[assocaitedPlatformLocalIndex].y;
        }
		
		int objectIndex = infiniteObjectManager.localIndexToObjectIndex(localIndex, ObjectType.Scene);
        InfiniteObject prevTopScene = infiniteObjectHistory.objectSpawned(objectIndex, 0, location, ObjectType.Scene, scene);
        // the current scene now becames the parent of the previous top scene
        if (prevTopScene != null) {
            prevTopScene.setInfiniteObjectParent(scene);
        } else {
            infiniteObjectHistory.setBottomInfiniteObject(location, true, scene);
        }

        infiniteObjectHistory.addTotalDistance(sceneSizes[localIndex].z, location, true);
        if (scene.sectionTransition) {
            infiniteObjectHistory.didSpawnSectionTranition(location, true);
        }

        if (activateImmediately)
            scene.activate();
    }
	
	// move all of the active objects
	public void moveObjects(float moveDistance)
	{
		if (moveDistance == 0)
			return;
		
        // the distance to move the objects
		Vector3 delta = moveDirection * moveDistance;

        // only move the top most platform/scene of each ObjectLocation because all of the other objects are children of these two
        // objects. Only have to check the bottom-most platform/scene as well to determine if it should be removed
        InfiniteObject infiniteObject = null;
        Transform objectTransform = null;
        PlatformObject platformObject = null;
        for (int i = 0; i < 2; ++i) { // loop through the platform and scenes
            for (int j = 0; j < (int)ObjectLocation.Last; ++j) {
                // move
                infiniteObject = infiniteObjectHistory.getTopInfiniteObject((ObjectLocation)j, i == 0);
                if (infiniteObject != null) {
                    objectTransform = infiniteObject.getTransform();
                    Vector3 pos = objectTransform.position;
                    pos -= delta;
                    objectTransform.position = pos;

                    // check for removal.. there will always be a bottom object if there is a top object
                    infiniteObject = infiniteObjectHistory.getBottomInfiniteObject((ObjectLocation)j, i == 0);
                    if (cameraTransform.InverseTransformPoint(infiniteObject.getTransform().position).z < removeHorizon) {
                        // if the infinite object is a platform and it has changes height, move everything down by that height
                        if (i == 1) { // 1 are platforms
                            platformObject = infiniteObject as PlatformObject;
                            if (platformObject.slope != PlatformSlope.None) {
                                transitionHeight(platformSizes[platformObject.getLocalIndex()].y);
                            }
                        }

                        infiniteObjectHistory.objectRemoved((ObjectLocation)j, i == 0);
                        infiniteObject.deactivate();
                    }
                }
            }
            
            // loop through all of the turn objects
            infiniteObject = infiniteObjectHistory.getTopTurnInfiniteObject(i == 0);
            if (infiniteObject != null) {
                objectTransform = infiniteObject.getTransform();
                Vector3 pos = objectTransform.position;
                pos -= delta;
                objectTransform.position = pos;

                infiniteObject = infiniteObjectHistory.getBottomTurnInfiniteObject(i == 0);
                if (cameraTransform.InverseTransformPoint(infiniteObject.getTransform().position).z < removeHorizon) {
                    infiniteObjectHistory.turnObjectRemoved(i == 0);
                    infiniteObject.deactivate();
                }
            }
        }
		
		localDistance[(int)ObjectLocation.Center] -= moveDistance;
        localSceneDistance[(int)ObjectLocation.Center] -= moveDistance;
        
        if (!stopObjectSpawns)
			spawnObjectRun(true);
	}

    // When a platform with delta height is removed, move all of the objects back to their original heights to reduce the chances
    // of floating point errors
    private void transitionHeight(float amount)
    {
        // Move the position of every object by -amount
        InfiniteObject infiniteObject;
        Transform infiniteObjectTransform;
        Vector3 position;
        for (int i = 0; i < 2; ++i) { // loop through the platform and scenes
            for (int j = 0; j < (int)ObjectLocation.Last; ++j) {
                infiniteObject = infiniteObjectHistory.getTopInfiniteObject((ObjectLocation)j, i == 0);
                if (infiniteObject != null) {
                    position = (infiniteObjectTransform = infiniteObject.getTransform()).position;
                    position.y -= amount;
                    infiniteObjectTransform.position = position;
                    if (i == 0) {
                        localSceneHeight[j] -= amount;
                    } else {
                        localPlatformHeight[j] -= amount;
                    }
                }
            }
        }

        position = playerTransform.position;
        position.y -= amount;
        playerTransform.position = position;
    }
	
    // turn the player to the specified location
	public Vector3 changeMoveDirection(Vector3 newDirection, bool rotateRight)
    {
        if (turnPlatform[(int)ObjectLocation.Center] != null) {
            turnOffset += moveDirection * (turnPlatform[(int)ObjectLocation.Center].turnLengthOffset + localDistance[(int)ObjectLocation.Center]);
        }

        moveDirection = newDirection;

        // don't change move directions if there are still turn objects. It is about to be game over anyway since multiple turns aren't supported this closely
        if (infiniteObjectHistory.getBottomTurnInfiniteObject(true)) {
            stopObjectSpawns = true;
            return turnOffset;
        }
		
		ObjectLocation turnLocation = (rotateRight ? ObjectLocation.Right : ObjectLocation.Left);
		localDistance[(int)ObjectLocation.Center] = localDistance[(int)turnLocation];
        localSceneDistance[(int)ObjectLocation.Center] = localSceneDistance[(int)turnLocation];
        localPlatformHeight[(int)ObjectLocation.Center] = localPlatformHeight[(int)turnLocation];
        localSceneHeight[(int)ObjectLocation.Center] = localSceneHeight[(int)turnLocation];
		turnPlatform[(int)ObjectLocation.Center] = turnPlatform[(int)turnLocation];
		turnPlatform[(int)ObjectLocation.Right] = turnPlatform[(int)ObjectLocation.Left] = null;

        // The center objects and the objects in the location opposite of turn are grouped togeter with the center object being the top most object
        for (int i = 0; i < 2; ++i) {
            InfiniteObject infiniteObject = infiniteObjectHistory.getTopInfiniteObject((turnLocation == ObjectLocation.Right ? ObjectLocation.Left : ObjectLocation.Right), i == 0);
            // may be null if the turn only turns one direction
            if (infiniteObject != null) {
                InfiniteObject centerObject = infiniteObjectHistory.getBottomInfiniteObject(ObjectLocation.Center, i == 0);
                infiniteObject.setInfiniteObjectParent(centerObject);
            }
        }

		infiniteObjectHistory.turn(turnLocation);
		
		if (turnPlatform[(int)ObjectLocation.Center] != null) {
			setupPlatformTurn();
		}

        return turnOffset;
	}
	
	// clear everything out and reset the generator back to the beginning, keeping the current set of objects activated before new objects are generated
	public void reset()
    {
		moveDirection = Vector3.forward;
		
		for (int i = 0; i < (int)ObjectLocation.Last; ++i) {
			turnPlatform[i] = null;
			localDistance[i] = 0;
            localSceneDistance[i] = 0;
            localPlatformHeight[i] = 0;
            localSceneHeight[i] = 0;
		}
        turnOffset = Vector3.zero;

        stopObjectSpawns = false;
        spawnData.largestScene = largestSceneLength;
        spawnData.useWidthBuffer = true;
        spawnData.section = 0;
        spawnData.sectionTransition = false;
		
		infiniteObjectHistory.saveObjectsReset();
        sectionSelection.reset();
	}

    // remove the saved infinite objects and activate the set of objects for the next game
    public void readyFromReset()
    {
        // deactivate the saved infinite objects from the previous game
        InfiniteObject infiniteObject = infiniteObjectHistory.getSavedInfiniteObjects();
        InfiniteObject[] childObjects;
        for (int i = 0; i < 2; ++i) { // loop through the platform and scenes
            if (i == 0) { // scene
                childObjects = infiniteObject.GetComponentsInChildren<SceneObject>() as SceneObject[];
            } else {
                childObjects = infiniteObject.GetComponentsInChildren<PlatformObject>() as PlatformObject[];
            }

            for (int j = 0; j < childObjects.Length; ++j) {
                childObjects[j].deactivate();
            }
        }

        // activate the objects for the current game
        for (int i = 0; i < 2; ++i) { // loop through the platform and scenes
            for (int j = 0; j < (int)ObjectLocation.Last; ++j) {
                infiniteObject = infiniteObjectHistory.getTopInfiniteObject((ObjectLocation)j, i == 0);
                if (infiniteObject != null) {
                    if (i == 0) { // scene
                        childObjects = infiniteObject.GetComponentsInChildren<SceneObject>() as SceneObject[];
                    } else {
                        childObjects = infiniteObject.GetComponentsInChildren<PlatformObject>() as PlatformObject[];
                    }

                    for (int k = 0; k < childObjects.Length; ++k) {
                        childObjects[k].activate();
                    }
                }
            }
        }
    }

    // For persisting the data:
    public void saveInfiniteObjectPersistence(ref InfiniteObjectPersistence persistence)
    {
        persistence.localDistance = localDistance;
        persistence.localSceneDistance = localSceneDistance;
        persistence.localPlatformHeight = localPlatformHeight;
        persistence.localSceneHeight = localSceneHeight;
    }

    private void loadInfiniteObjectPersistence(InfiniteObjectPersistence persistence)
    {
        localDistance = persistence.localDistance;
        localSceneDistance = persistence.localSceneDistance;
        localPlatformHeight = persistence.localPlatformHeight;
        localSceneHeight = persistence.localSceneHeight;
    }
}