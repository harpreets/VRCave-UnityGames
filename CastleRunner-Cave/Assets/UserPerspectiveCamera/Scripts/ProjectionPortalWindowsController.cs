using UnityEngine;
using System.Collections;

public class ProjectionPortalWindowsController : MonoBehaviour {
	
	static public ProjectionPortalWindowsController instance;
//	public GameObject[] projectionPortalWindows;
	
	private Transform thisTransform;
	private Transform initialTransform;
	
	void Awake(){
		instance = this;
	}
	
	// Use this for initialization
	void Start () {
		thisTransform = transform;
		initialTransform = transform;
	}
	
	public void setAsChildObject(bool setScreensAsChild, GameObject parentGameObject = null){
		if(setScreensAsChild && parentGameObject != null){
			thisTransform.parent = parentGameObject.transform;
		}else{
			thisTransform.parent = null;
			transform.position = initialTransform.position;
			transform.rotation = initialTransform.rotation;
			transform.localScale = initialTransform.localScale;
		}
	}
	
}
