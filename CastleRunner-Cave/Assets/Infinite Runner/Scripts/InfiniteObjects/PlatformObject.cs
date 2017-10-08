using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlatformSlope { None, Up, Down }
/*
 * A platform is the base track layer. It can hold up to n collidable objects within the left, center, and right slots.
 */
[RequireComponent(typeof(AppearanceProbability))]
[RequireComponent(typeof(PlatformAppearanceRules))]
public class PlatformObject : InfiniteObject {
	
	public delegate void PlatformDeactivationHandler( );
	public event PlatformDeactivationHandler onPlatformDeactivation;

    // Override the size if the object manager can't get the size right (a value of Vector3.zero will let the object manager calculate the size)
    public Vector3 overrideSize;

    // True if this piece is used for section transitions
    [HideInInspector]
    public bool sectionTransition;
    // If section transition is true, this list contains the sections that it can transition from (used with the toSection list)
    [HideInInspector]
    public List<int> fromSection;
    // If section transition is true, this list contains the sections that it can transition to (used with the fromSection list)
    [HideInInspector]
    public List<int> toSection;

    // For turning. Both booleans can be enabled.
    public bool isLeftTurn;
    public bool isRightTurn;
    public float turnLengthOffset; // the offset from the start of the platform to the part of the platform that starts the actual turn

    // If the platform is a jump there won't be a mesh associated with it, however it'll need to know how far the jump is
    public bool isJump;
	public float jumpLength;

    // The platform has a slope if it changes heights. The object generator will then take into account the height of the platform and its associated
    // scene object. A downward slope is when the start of the platform is higher than the end of the platform
    public PlatformSlope slope;
	
	// force different collidable object types to spawn on top of the platform, such as obstacle and coin
	// (assuming the propabilities allow the object to spawn)
	public bool forceDifferentCollidableTypes;
	
	// the number of collidable objects that can fit on one platform. The objects are spaced along the local z position of the platform
	public int collidablePositions;

    // boolean to determine what horizontal location objects can spawn. If collidablePositions is greater than 0 then at least one
    // of these booleans must be true
	public bool collidableLeftSpawn;
	public bool collidableCenterSpawn;
	public bool collidableRightSpawn;
	
	private List<int> slotPositions;
    private int collidableSpawnPosition;

    // true if a scene object has linked to this platform. No other scene objects will be able to spawn near this object.
    private bool requireLinkedSceneObject;

    public override void init()
    {
        base.init();
        objectType = ObjectType.Platform;	
    }
	
	public override void Awake ()
	{
		base.Awake ();
		collidableSpawnPosition = 0;
        requireLinkedSceneObject = false;
		
		slotPositions = new List<int>();
		if (collidableCenterSpawn) {
			slotPositions.Add(0);	
		}
		
		if (collidableLeftSpawn) {
			slotPositions.Add(-1);
		}
		
		if (collidableRightSpawn) {
			slotPositions.Add(1);
		}
	}

    public void enableLinkedSceneObjectRequired()
    {
        requireLinkedSceneObject = true;
    }

    public bool linkedSceneObjectRequired()
    {
        return requireLinkedSceneObject;
    }

	public Vector3 getRandomSlot()
	{
        if (slotPositions.Count > 0) {
            return thisTransform.right * slotPositions[Random.Range(0, slotPositions.Count)];
        }
		return Vector3.zero;
	}
	
	// Determine if an object is already spawned in the same position. Do this using bitwise and/or.
	// For example, the following situation might occur:
	// Spawn pos 3:
	// 0 |= (2 ^ 3), result of 0000 1000 (decimal 8)
	// Spawn pos 1:
	// 8 |= (2 ^ 1), result of 0000 1010 (decimal 10)
	// Check pos 3:
	// 10 & (2 ^ 3), result of 0000 1000 (decimal 8), position is not free
	// Check pos 2:
	// 10 & (2 ^ 2), result of 0000 0000 (decimal 0), space is free
	// Spawn pos 0:
	// 10 |= (2 ^ 0), result of 0000 1011 (decimal 11)
	public bool canSpawnCollidable(int pos)
	{
        return slotPositions.Count > 0 && (collidableSpawnPosition & (int)Mathf.Pow(2, pos)) == 0;
	}
	
	public bool canSpawnCollidable()
	{
		return collidablePositions != 0 && collidableSpawnPosition != (int)Mathf.Pow(2, collidablePositions) - 1;
	}
	
	public void collidableSpawned(int pos)
	{
		collidableSpawnPosition |= (int)Mathf.Pow(2, pos);
	}

    public override void orient(Vector3 position, Quaternion rotation)
	{
        base.orient(position, rotation);
		
		// reset the number of collidables that have been spawned on top of the platform
		collidableSpawnPosition = 0;
	}
	
	public override void deactivate()
	{
		// platforms have collidable children. Make sure they get deactivated properly
		if (onPlatformDeactivation != null) {
			onPlatformDeactivation();
			onPlatformDeactivation = null;
		}
		
		base.deactivate();
	}
}