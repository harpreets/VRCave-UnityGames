using UnityEngine;
using System.Collections;

/*
 * Turn on any track turns when the invincibility power up is active
 */
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TurnTrigger : MonoBehaviour {
	
    private PlatformObject platform;

    public void Start()
    {
        platform = transform.parent.GetComponent<PlatformObject>();
    }
	
	public void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            //randomly choose left or right if the turn is available
			bool rotateRight = Random.value > 0.5f;
            if (rotateRight && !platform.isRightTurn || !rotateRight && !platform.isLeftTurn)
				rotateRight = !rotateRight;

            // let the player controller decide if the player should really turn
			PlayerController.instance.turn(rotateRight, false);
		}
	}
}
