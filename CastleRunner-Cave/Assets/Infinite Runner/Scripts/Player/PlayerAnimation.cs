using UnityEngine;
using System.Collections;

/*
 * Basic class to manage the different animation states
 */
public class PlayerAnimation : MonoBehaviour {

    // the amount of time it takes to transition from the previous animation to a run
    public float runTransitionTime;

    private Animation thisAnimation;

	public void init() 
    {
        thisAnimation = animation;

        thisAnimation["Run"].wrapMode = WrapMode.Loop;
        thisAnimation["Run"].layer = 0;
        thisAnimation["RunJump"].wrapMode = WrapMode.ClampForever;
        thisAnimation["RunJump"].layer = 1;
        thisAnimation["RunSlide"].wrapMode = WrapMode.ClampForever;
        thisAnimation["RunSlide"].layer = 1;
        thisAnimation["Attack"].wrapMode = WrapMode.Once;
        thisAnimation["Attack"].layer = 1;
        thisAnimation["BackwardDeath"].wrapMode = WrapMode.Once;
        thisAnimation["BackwardDeath"].layer = 2;
        thisAnimation["ForwardDeath"].wrapMode = WrapMode.Once;
        thisAnimation["ForwardDeath"].layer = 2;
        thisAnimation["Idle"].wrapMode = WrapMode.Loop;
        thisAnimation["Idle"].layer = 3;
        
        GameManager.instance.onPauseGame += onPauseGame;
    }

    public void OnDisable()
    {
        GameManager.instance.onPauseGame -= onPauseGame;
    }

    public void run()
    {
        thisAnimation.CrossFade("Run", runTransitionTime, PlayMode.StopAll);
    }

    public void jump()
    {
        thisAnimation.CrossFade("RunJump");
    }

    public void slide()
    {
        thisAnimation.CrossFade("RunSlide");
    }

    public void attack()
    {
        thisAnimation.CrossFade("Attack", 0.1f);
    }

    public void gameOver(GameOverType gameOverType)
    {
        thisAnimation.Stop("Run");

        if (gameOverType != GameOverType.Quit) {
            if (gameOverType == GameOverType.JumpObstacle) {
                thisAnimation.Play("ForwardDeath");
            } else {
                thisAnimation.Play("BackwardDeath");
            }
        }
    }

    public void reset()
    {
        thisAnimation.Play("Idle");
    }

    public void onPauseGame(bool paused)
    {
        float speed = (paused ? 0 : 1);
        foreach (AnimationState state in thisAnimation) {
            state.speed = speed;
        }
    }
}
