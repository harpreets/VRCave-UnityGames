using UnityEngine;
using System.Collections;

/*
 * Basic camera script which doesn't really do anything besides hold variables so it knows where to reset
 */
public class CameraController : MonoBehaviour
{
    static public CameraController instance;
    
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Transform thisTransform;
    private Transform playerTransform;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        thisTransform = transform;

        startPosition = thisTransform.position;
        startRotation = thisTransform.rotation;
    }

    public void setPlayerAsParent(bool playerParent)
    {
        if (playerParent) {
            playerTransform = PlayerController.instance.transform;
            thisTransform.parent = playerTransform;
        } else {
            thisTransform.parent = null;
        }
    }

    public void reset()
    {
        thisTransform.position = startPosition;
        thisTransform.rotation = startRotation;
    }
}
