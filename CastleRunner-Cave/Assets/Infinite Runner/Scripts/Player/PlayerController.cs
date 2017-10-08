using UnityEngine;
using System.Collections;

/*
 * Calculate the target position/rotation of the player every frame, as well as move the objects around the player.
 * This class also manages when the player is sliding/jumping, and calls any animations.
 * The player has a collider which only collides with the platforms/walls. All obstacles/coins/power ups have their
 * own trigger system and will call the player controller if they need to.
 */
public enum SlotPosition { Left = -1, Center, Right }
public enum Character { Character1, Character2, None }
//[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerController : MonoBehaviour {
	
	public static PlayerController instance;

    public int maxCollisions;
    [HideInInspector]
    public DistanceValueList forwardSpeeds;
    public float horizontalSpeed;
	public float rotationSpeed;
	public float jumpForce;
	public float slideDuration;
    public bool allowAttack;
    public float closeAttackDistance; // the minimum distance that allows the attack to hit the target
    public float farAttackDistance; // the maximum distance that allows the attack to hit the target
    public bool restrictTurns; // if true, can only turn on turn platforms
    public float turnGracePeriod; // if restrictTurns is on, if the player swipes within the grace period before a turn then the character will turn
    public float simultaneousTurnPreventionTime; // the amount of time that must elapse in between two different turns
	
	public GameObject coinMagnetTrigger;
    public ParticleSystem collisionParticleSystem;
    public ParticleSystem powerUpParticleSystem;

    public float totalMoveDistance;
	private SlotPosition currentSlotPosition;
	private Quaternion targetRotation;
	public Vector3 targetPosition;
	private float targetSlotValue;

    public bool isJumping;
    // isJumping gets set to false when the player lands on a platform within OnCollisionEnter. OnCollisionEnter may get called even before the player does
    // the jump though (such as switching from one platform to another) so isJumpPending will be set to true when the jump is initiated and to false
    // in OnCollisionExit, when the player actually leaves the platform for a jump
    private bool isJumpPending;
    private bool isSliding;
//	public bool isZorbActive;
    private float turnRequestTime;
    private float turnTime;
	
	private int platformLayer;
	private int wallLayer;
    private int obstacleLayer;
	
	private Vector3 startPosition;
	private Quaternion startRotation;
    private Vector3 turnOffset;
	
	// for pausing:
	private CoroutineData slideData;
    private Vector3 pauseVelocity;
    private bool pauseCollisionParticlePlaying;
    private bool pausePowerUpParticlePlaying;
	
	private Transform thisTransform;
	private Rigidbody thisRigidbody;
    private CapsuleCollider capsuleCollider;
    private PlayerAnimation playerAnimation;
    private InfiniteObjectGenerator infiniteObjectGenerator;
    private InfiniteObjectHistory infiniteObjectHistory;
    private PowerUpManager powerUpManager;
	private ProjectionCamerasController projectionCamerasController;
	private ProjectionPortalWindowsController projectionPortalWindowsController;
//	private MainCameraController mainCameraController;
    private GameManager gameManager;
//	public bool zorbModeActive;
	
	public void Awake()
	{
		instance = this;
	}
	
	public void init()
	{
		infiniteObjectGenerator = InfiniteObjectGenerator.instance;
        powerUpManager = PowerUpManager.instance;
        gameManager = GameManager.instance;
		projectionCamerasController = ProjectionCamerasController.instance;
		projectionPortalWindowsController = ProjectionPortalWindowsController.instance;
//		mainCameraController = MainCameraController.instance;
		
		platformLayer = LayerMask.NameToLayer("Platform");
		wallLayer = LayerMask.NameToLayer("Wall");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
		
		thisTransform = transform;
		thisRigidbody = rigidbody;
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerAnimation.init();
		
		startPosition = thisTransform.position;
		startRotation = thisTransform.rotation;

        slideData = new CoroutineData();
        forwardSpeeds.init();
		
        // make sure the coin magnet trigger is deactivated
		activatePowerUp(PowerUpTypes.CoinMagnet, false, Color.white);

        reset();
        enabled = false;
//		zorbModeActive = false;
	}
	
	public void reset()
	{
		thisTransform.position = startPosition;
		thisTransform.rotation = startRotation;

        slideData.duration = 0;

        isJumping = false;
        isJumpPending = false;
		isSliding = false;
		currentSlotPosition = SlotPosition.Center;
        targetSlotValue = (int)currentSlotPosition * infiniteObjectGenerator.slotDistance;
        thisRigidbody.useGravity = true;
        playerAnimation.reset();
        pauseCollisionParticlePlaying = false;
        pausePowerUpParticlePlaying = false;
        totalMoveDistance = 0;
        turnOffset = Vector3.zero;
        turnTime = Time.time;
		
		targetRotation = startRotation;
		updateTargetPosition(targetRotation.eulerAngles.y);
		forwardSpeeds.init();
	}
	
	public void startGame()
    {
        playerAnimation.run();
        enabled = true;
        gameManager.onPauseGame += gamePaused;	
	}
	
	public void deregisterPause(){
	}
	
    // There character doesn't move, all of the objects around it do. Make sure the character is in the correct position and
    // move all of those objects.
	public void Update ()
	{
		// height is independent from the target position
		//Zorb stays hereas well simila to the character but it happens only when the size of the zorb is small
		
//		targetPosition.y = thisTransform.position.y;
//		if (thisTransform.position != targetPosition) {
//			thisTransform.position = Vector3.MoveTowards(thisTransform.position, targetPosition, horizontalSpeed * Time.deltaTime);
//		}
		
		if (thisTransform.rotation != targetRotation) {
			thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, targetRotation, rotationSpeed);	
		}

        float forwardSpeed = forwardSpeeds.getValue(totalMoveDistance) * Time.deltaTime;
        totalMoveDistance += forwardSpeed;
        infiniteObjectGenerator.moveObjects(forwardSpeed);
		
		if(Input.GetButton("EnableGodMode")){
			gameManager.godMode = true;
		}
	}

    public void FixedUpdate()
    {
        // Apply a foce to keep the player sticking to the ground
        //if (!isJumping) {
            //thisRigidbody.AddForce(0, -5, 0, ForceMode.VelocityChange);
        //}
		//rigidbody.AddForce(Vector3.forward);
//		Debug.Log(rigidbody.GetPointVelocity(rigidbody.position));
    }

    public bool aboveTurn()
    {
        RaycastHit hit;
        if (Physics.Raycast(thisTransform.position + Vector3.up / 2, -thisTransform.up, out hit, Mathf.Infinity, 1 << platformLayer)) {
            PlatformObject platform = hit.collider.GetComponent<PlatformObject>();

            // don't allow a turn if the player is trying to do a 180 degree turn
            if (Mathf.Abs(thisTransform.eulerAngles.y - platform.getTransform().eulerAngles.y) > 0.1f) {
                return false;
            }

            if (platform != null) {
                return platform.isRightTurn || platform.isLeftTurn;
            }
        }

        return true;
    }

    // Turn left or right
	public void turn(bool rightTurn, bool fromInputManager)
    {
        // prevent two turns from occurring really close to each other (for example, to prevent a 180 degree turn)
        if (Time.time - turnTime < simultaneousTurnPreventionTime) {
            return;
        }

        bool isAboveTurn = aboveTurn();

        // if we are restricting a turn, don't turn unless we are above a turn platform
        if (restrictTurns) {
            if (fromInputManager) {
                turnRequestTime = Time.time;
                return;
            }

            // don't turn if restrict turns is on and the player hasn't swipped within the grace period time or if the player isn't above a turn platform
            if (!powerUpManager.isPowerUpActive(PowerUpTypes.Invincibility) && !gameManager.godMode && (Time.time - turnRequestTime > turnGracePeriod || !isAboveTurn)) {
                return;
            }
        } else if (!fromInputManager && !powerUpManager.isPowerUpActive(PowerUpTypes.Invincibility) && !gameManager.godMode) {
            return;
        }

        turnTime = Time.time;
        Vector3 direction = thisTransform.right * (rightTurn ? 1 : -1);
        if (isAboveTurn) {
            turnOffset = infiniteObjectGenerator.changeMoveDirection(direction, rightTurn);
        }

		targetRotation = Quaternion.LookRotation(direction);
		updateTargetPosition(targetRotation.eulerAngles.y);
	}
	
    // There are three slots on a track. Move left or right if there is a slot available
	public void changeSlots(bool right)
	{
		currentSlotPosition = (SlotPosition)Mathf.Clamp((int)currentSlotPosition + (right ? 1 : -1), (int)SlotPosition.Left, (int)SlotPosition.Right);
		targetSlotValue = (int)currentSlotPosition * infiniteObjectGenerator.slotDistance;
		
		updateTargetPosition(thisTransform.eulerAngles.y);
	}

    // There are three slots on a track. The accelorometer determins the slot position
	public void changeSlots(SlotPosition targetSlot)
	{
		if (targetSlot == currentSlotPosition)
			return;
		
		currentSlotPosition = targetSlot;
		targetSlotValue = (int)currentSlotPosition * infiniteObjectGenerator.slotDistance;
		
		updateTargetPosition(thisTransform.eulerAngles.y);
	}

    // attack the object in front of the player if it can be destroyed
    public void attack()
    {
        if (!allowAttack)
            return;

        if (!isJumping && !isJumpPending && !isSliding) {
            playerAnimation.attack();

            RaycastHit hit;
            if (Physics.Raycast(thisTransform.position + Vector3.up / 2, thisTransform.forward, out hit, farAttackDistance, 1 << obstacleLayer)) {
                // the player will collide with the obstacle if they are too close
                if (hit.distance > closeAttackDistance) {
                    ObstacleObject obstacle = hit.collider.GetComponent<ObstacleObject>();
                    if (obstacle.isDestructible) {
                        obstacle.obstacleAttacked();
                    }
                }
            }
        }
    }
	
	private void updateTargetPosition(float yAngle)
	{
		targetPosition = thisTransform.position;
        targetPosition.x = targetSlotValue * Mathf.Cos(yAngle * Mathf.Deg2Rad) + startPosition.z * Mathf.Sin(yAngle * Mathf.Deg2Rad);	
        targetPosition.z = targetSlotValue * -Mathf.Sin(yAngle * Mathf.Deg2Rad) + startPosition.z * Mathf.Cos(yAngle * Mathf.Deg2Rad);
        targetPosition += turnOffset;
	}
	
	public void jump()
	{
		if (!isJumping && !isSliding) {
            isJumpPending = isJumping = true;
			projectionPortalWindowsController.setAsChildObject(true, gameObject);
			projectionCamerasController.setAsChildObject(true, gameObject);
			thisRigidbody.AddRelativeForce(0, jumpForce, 0, ForceMode.VelocityChange);
            playerAnimation.jump();
		}
	}
	
	public void slide()
	{
        if (!isJumping && !isSliding) {
            isSliding = true;
            playerAnimation.slide();
			
			// adjust the collider bounds
			float height = capsuleCollider.height;
			height /= 2;
            Vector3 center = capsuleCollider.center;
            center.y = center.y - (capsuleCollider.height - height) / 2;
            capsuleCollider.height = height;
            capsuleCollider.center = center;
			
			slideData.duration = slideDuration;
			StartCoroutine("doSlide");
		}	
	}
	
    // stay in the slide postion for a certain amount of time
    private IEnumerator doSlide()
	{
        slideData.startTime = Time.time;
        yield return new WaitForSeconds(slideData.duration);

        playerAnimation.run();
        // let the run animation start
        yield return new WaitForSeconds(playerAnimation.runTransitionTime);

        isSliding = false;

		// adjust the collider bounds
        float height = capsuleCollider.height;
        height *= 2;
        capsuleCollider.height = height;
        Vector3 center = capsuleCollider.center;
        center.y = capsuleCollider.height / 2;
        capsuleCollider.center = center;
	}

    // the player collided with an obstacle, play some particle effects
    public void obstacleCollision(Transform obstacle, Vector3 position)
    {
        // Make sure the particle system is active
#if UNITY_3_5
        if (!collisionParticleSystem.gameObject.active)
		    collisionParticleSystem.gameObject.active = true;
#else
        if (!collisionParticleSystem.gameObject.activeSelf)
		    collisionParticleSystem.gameObject.SetActive(true);
#endif
        collisionParticleSystem.transform.position = position;
        collisionParticleSystem.transform.parent = obstacle;
        collisionParticleSystem.Clear();
        collisionParticleSystem.Play();
    }
	
    // called when the player collides with the platform
	public void OnCollisionEnter(Collision collision)
	{
		int collisionLayer = collision.gameObject.layer;
        if (collisionLayer == platformLayer) {
            if (isJumping && !isJumpPending && thisRigidbody.velocity.y < 0.1f) {
                isJumping = false;
                playerAnimation.run();
				projectionPortalWindowsController.setAsChildObject(false);
				projectionCamerasController.setAsChildObject(false);
            }
		} else if (collisionLayer == wallLayer) {
			gameManager.gameOver(GameOverType.Wall, true);
            thisRigidbody.velocity = Vector3.zero;
		}
	}

    public void OnCollisionExit(Collision collision)
    {
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer == platformLayer) {
            if (isJumpPending) {
                isJumpPending = false;
            }
        }
    }
	
    public void activatePowerUp(PowerUpTypes powerUp, bool activate, Color color)
    {
        if (activate) {
            powerUpParticleSystem.startColor = color;
            powerUpParticleSystem.Play();
        } else {
            powerUpParticleSystem.Stop();
        }

        if (powerUp == PowerUpTypes.CoinMagnet) {
#if UNITY_3_5
            coinMagnetTrigger.active = activate;
#else
			coinMagnetTrigger.SetActive(activate);
#endif
        }
    }

    public void gameOver(GameOverType gameOverType)
    {
        playerAnimation.gameOver(gameOverType);
        thisRigidbody.velocity = Vector3.zero;
        collisionParticleSystem.transform.parent = null;
        enabled = false;
        gameManager.onPauseGame -= gamePaused;	
	}
	
    // disable the script if paused to stop the objects from moving
	private void gamePaused(bool paused)
	{
        thisRigidbody.isKinematic = paused;
        if (paused) {
            pauseVelocity = thisRigidbody.velocity;

            if (collisionParticleSystem.isPlaying) {
                pauseCollisionParticlePlaying = true;
                collisionParticleSystem.Pause();
            }

            if (powerUpParticleSystem.isPlaying) {
                pausePowerUpParticlePlaying = true;
                powerUpParticleSystem.Pause();
            }
        } else {
            thisRigidbody.velocity = pauseVelocity;

            if (pauseCollisionParticlePlaying) {
                collisionParticleSystem.Play();
                pauseCollisionParticlePlaying = false;
            }

            if (pausePowerUpParticlePlaying) {
                powerUpParticleSystem.Play();
                pausePowerUpParticlePlaying = false;
            }
        }
        if (isSliding) {
			if (paused) {
                StopCoroutine("doSlide");
				slideData.calcuateNewDuration();
			} else {
                StartCoroutine("doSlide");
			}
		}
		enabled = !paused;
	}
}
