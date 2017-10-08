using UnityEngine;
using System.Collections;

/*
 * Show text on how to control the game
 */
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TutorialTrigger : MonoBehaviour {

    public TutorialType tutorialType;

    private int playerLayer;

    public void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer) {
            StartCoroutine(showTutorial(true));
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer) {
            StartCoroutine(showTutorial(false));
        }
    }

    private IEnumerator showTutorial(bool show)
    {
        yield return new WaitForEndOfFrame();

        GUIManager.instance.showTutorial(show, tutorialType);
    }
}
