using UnityEngine;
using System.Collections;

/*
 * The player collects coins to be able to purchase power ups
 */
public class CoinObject : CollidableObject {
	
	public float collectSpeed;

    private int playerLayer;
    private int coinMagnetLayer;
    private Vector3 collectPoint;
    private Vector3 startLocalPosition;
    
	private GameManager gameManager;

    public override void init()
    {
        base.init();
        objectType = ObjectType.Coin;
    }
	
	public override void Awake()
	{
		base.Awake();
		playerLayer = LayerMask.NameToLayer("Player");
        coinMagnetLayer = LayerMask.NameToLayer("CoinMagnet");
        collectPoint = new Vector3(0, 1, 0);
        enabled = false;
        startLocalPosition = thisTransform.localPosition;
	}
	
	// Updated isn't enabled until the player collects the coin and it needs to fly towards the player
	public void Update()
	{
        if (thisTransform.localPosition != collectPoint) {
            thisTransform.localPosition = Vector3.MoveTowards(thisTransform.localPosition, collectPoint, collectSpeed);
		} else {
			enabled = false;
            collidableDeactivation();
            thisTransform.localPosition = startLocalPosition;
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
        if ((other.gameObject.layer == playerLayer || other.gameObject.layer == coinMagnetLayer) && !enabled) {
			collectCoin();
		}
	}
	
	public void collectCoin()
	{
		GameManager.instance.coinCollected();
			
		// the coin may have been collected from far away with the coin magnet. Fly towards the player when collected
		thisTransform.parent = PlayerController.instance.transform;
		enabled = true;
	}
}
