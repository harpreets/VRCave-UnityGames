using UnityEngine;
using System.Collections;

/*
 * The player can only run into so many obstacle objects before it is game over.
 */
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AppearanceProbability))]
[RequireComponent(typeof(CollidableAppearanceRules))]
public class ObstacleObject : CollidableObject
{
    // Used for the player death animation. On a jump the player will flip over, while a duck the player will fall backwards
    public bool isJump;

    // True if the object can be destroyed through an attack. The method obstacleAttacked will be called to play the handle the destruction
    public bool isDestructible;

    public ParticleSystem destructionParticles;

    private bool collideWithPlayer;
	private int playerLayer;
    private WaitForSeconds destructionDelay;

    private Animation thisAnimation;
    private BoxCollider boxCollider;
    private GameManager gameManager;

    public override void init()
    {
        base.init();
        objectType = ObjectType.Obstacle;
    }
	
	public override void Awake()
	{
		base.Awake();
        playerLayer = LayerMask.NameToLayer("Player");
	}
	
	public void Start()
	{
        thisAnimation = animation;
		gameManager = GameManager.instance;

        if (thisAnimation) {
            thisAnimation["PostBreak"].wrapMode = WrapMode.Once;
            destructionDelay = new WaitForSeconds(0.2f);
        }

        collideWithPlayer = true;
	}

    public void OnTriggerEnter(Collider other) 
	{
        if (other.gameObject.layer == playerLayer && collideWithPlayer) {
			gameManager.obstacleCollision(this, other.ClosestPointOnBounds(thisTransform.position));
		}
	}

    public void obstacleAttacked()
    {
        collideWithPlayer = false;

        if (thisAnimation) {
            StartCoroutine(playAttackedAnimation());
        }
    }

    private IEnumerator playAttackedAnimation()
    {
        yield return destructionDelay;
        thisAnimation.Play();
        destructionParticles.Play();
    }

    public override void collidableDeactivation()
    {
        base.collidableDeactivation();

        // reset
        collideWithPlayer = true;
        if (thisAnimation) {
            thisAnimation.Rewind();
            thisAnimation.Play();
            thisAnimation.Sample();
            thisAnimation.Stop();

            destructionParticles.Clear();
        }
    }
}
