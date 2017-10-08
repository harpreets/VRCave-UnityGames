using UnityEngine;
using System.Collections;

/*
 * Power ups are used to give the player an extra super ability
 */
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AppearanceProbability))]
[RequireComponent(typeof(PowerUpAppearanceRules))]
public class PowerUpObject : CollidableObject
{
	public PowerUpTypes powerUpType;
	
	private GameManager gameManager;
	private int playerLayer;

    public override void init()
    {
        base.init();
        objectType = ObjectType.PowerUp;
    }
	
	public override void Awake()
	{
		base.Awake();
		playerLayer = LayerMask.NameToLayer("Player");	
	}
	
	public void Start()
	{
        gameManager = GameManager.instance;
	}
	
	void OnTriggerEnter(Collider other)
    {
		if (other.gameObject.layer == playerLayer) {
            gameManager.activatePowerUp(powerUpType, true);
			deactivate();
		}
	}
}
