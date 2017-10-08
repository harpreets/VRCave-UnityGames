using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Place the List<int> of sections into a HashSet to make lookup really fast (O(1))
 */
public class SectionList
{
    public HashSet<int> sections;

    public SectionList(List<int> sectionArray)
    {
        sections = new HashSet<int>();
        for (int i = 0; i < sectionArray.Count; ++i) {
            sections.Add(sectionArray[i]);
        }
    }

    public bool containsSection(int section)
    {
        return sections.Contains(section);
    }

    public int count()
    {
        return sections.Count;
    }
}

/*
 * Base class for any object created by the infinite object manager. These objects are used and reset multiple times after
 * being pushed and popped from the object pool. This is done to instantiate as little as possible
 */
public enum ObjectType { Platform, Scene, Obstacle, PowerUp, Coin, Last }
public abstract class InfiniteObject : MonoBehaviour
{
    [HideInInspector]
    public List<int> sections;
    private SectionList sectionList;

	protected ObjectType objectType;
	
	private Vector3 startPosition;
    private Quaternion startRotation;
    private Transform startParent;

    private int localIndex;
    private InfiniteObject infiniteObjectParent;

    private bool destroyOnDeactivation;

	private GameObject thisGameObject;
	protected Transform thisTransform;
    
    public virtual void init()
    {
        sectionList = new SectionList(sections);
    }
	
	public virtual void Awake()
	{
		thisGameObject = gameObject;
		thisTransform = transform;
		
		startPosition = thisTransform.position;
		startRotation = thisTransform.rotation;

        destroyOnDeactivation = false;
	}
	
	public ObjectType getObjectType()
	{
		return objectType;
	}
	
	public GameObject getGameObject()
	{
		return thisGameObject;
	}
	
	public Transform getTransform()
	{
		return thisTransform;	
	}

    public void setLocalIndex(int index)
    {
        localIndex = index;
    }

    public int getLocalIndex()
    {
        return localIndex;
    }
	
	public virtual void setParent(Transform parent)
	{
        thisTransform.parent = parent;

        setStartParent(parent);
	}

    public void setStartParent(Transform parent)
    {
        startParent = parent;
    }

    public void setInfiniteObjectParent(InfiniteObject parentObject)
    {
        infiniteObjectParent = parentObject;
        thisTransform.parent = parentObject.getTransform();
    }

    public InfiniteObject getInfiniteObjectParent()
    {
        return infiniteObjectParent;
    }

	// orient for platform and scene objects
	public virtual void orient(Vector3 position, Quaternion rotation)
	{
		Vector3 pos = Vector3.zero;
        float yAngle = rotation.eulerAngles.y;
        pos.Set(startPosition.x * Mathf.Cos(yAngle * Mathf.Deg2Rad) + startPosition.z * Mathf.Sin(yAngle * Mathf.Deg2Rad), startPosition.y,
                -startPosition.x * Mathf.Sin(yAngle * Mathf.Deg2Rad) + startPosition.z * Mathf.Cos(yAngle * Mathf.Deg2Rad));
		pos += position;
        thisTransform.position = pos;
        thisTransform.rotation = startRotation;
        thisTransform.Rotate(0, yAngle, 0, Space.World);
	}

    // orient for collidables which have a platform as a parent
    public virtual void orient(PlatformObject parent, Vector3 position, Quaternion rotation)
	{
		thisTransform.parent = parent.getTransform();
		Vector3 pos = startPosition;
		pos += position;
		thisTransform.localPosition = parent.getTransform().InverseTransformPoint(pos);
        thisTransform.rotation = startRotation;
        thisTransform.Rotate(0, rotation.eulerAngles.y, 0, Space.World);
	}

    public void activate()
    {
#if UNITY_3_5
        thisGameObject.SetActiveRecursively(true);
#else
		thisGameObject.SetActive(true);
#endif
    }

    // startup/tutorial objects will just be destroyed
    public void enableDestroyOnDeactivation()
    {
        destroyOnDeactivation = true;
    }
	
	public virtual void deactivate()
	{
        if (destroyOnDeactivation) {
            // don't destroy the collision particle effect
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            if (PlayerController.instance) {
                ParticleSystem collisionParticleSystem = PlayerController.instance.collisionParticleSystem;
                for (int i = 0; i < particleSystems.Length; ++i) {
                    if (particleSystems[i] == collisionParticleSystem)
                        particleSystems[i].transform.parent = null;
                }
            }
            Destroy(gameObject);
            return;
        }

        thisTransform.parent = startParent;
        infiniteObjectParent = null;
#if UNITY_3_5
		thisGameObject.SetActiveRecursively(false);
#else
		thisGameObject.SetActive(false);
#endif
	}
	
	public bool isActive()
	{
#if UNITY_3_5
		return thisGameObject.active;
#else
		return thisGameObject.activeSelf;
#endif
	}

    // the obejct can spawn if it contains the section or there are no sections
    public virtual bool canSpawnInSection(int section)
    {
        return sectionList.count() == 0 || sectionList.containsSection(section);
    }
}
