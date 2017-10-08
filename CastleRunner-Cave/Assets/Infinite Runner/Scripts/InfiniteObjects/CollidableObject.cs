using UnityEngine;
using System.Collections;

/*
 * A collidable object attaches itself to the platform when activated, and moves back to the original parent on deactivation
 */
public class CollidableObject : InfiniteObject {

	private PlatformObject platformParent;

    public override void init() { base.init(); }

	public override void setParent(Transform parent)
	{
		base.setParent(parent);

        CollidableObject childCollidableObject = null;
        for (int i = 0; i < thisTransform.childCount; ++i) {
            if ((childCollidableObject = thisTransform.GetChild(i).GetComponent<CollidableObject>()) != null) {
                childCollidableObject.setStartParent(thisTransform);
            }
        }
	}

    public override void orient(PlatformObject parent, Vector3 position, Quaternion rotation)
	{
        base.orient(parent, position, rotation);
		
		platformParent = parent;
		platformParent.onPlatformDeactivation += collidableDeactivation;
	}
	
	public virtual void collidableDeactivation()
	{
        if (platformParent)
		    platformParent.onPlatformDeactivation -= collidableDeactivation;
		
		base.deactivate();
	}
}
