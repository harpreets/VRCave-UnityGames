using UnityEngine;
using System.Collections;

public class ProjectionCamerasController : MonoBehaviour {
	
	static public ProjectionCamerasController instance;
	
	public GameObject[] projectionCameras;
	
	private UserTrackerTransform[] camUserTrackerTransform;

	private Transform thisTransform;
	private Transform parentObjectTransform;
	
	private Vector3 camerasParentPosition;
	private Vector3[] cameraPositions;
	
	enum ProjectionElementsControlMode {UnderParentControl, SelfControlled}; 
		
	void Awake(){
		instance = this;
	}
	
	public void Start () {
		thisTransform = transform;
		
		camUserTrackerTransform = new UserTrackerTransform[projectionCameras.Length];
		cameraPositions = new Vector3[projectionCameras.Length];
		
		for(int i=0;i<projectionCameras.Length;++i){
			camUserTrackerTransform[i] = projectionCameras[i].GetComponent<UserTrackerTransform>();
		}
		
	}
	
	public void setAsChildObject(bool setCamerasAsChild, GameObject parentGameObject = null){
		if(setCamerasAsChild && parentGameObject != null){
			setCamUserTrackerComponents(false);
			parentObjectTransform = parentGameObject.transform;
			thisTransform.parent = parentObjectTransform;
			setProjectionElementsControl(ProjectionElementsControlMode.UnderParentControl);
		}else{
			setCamUserTrackerComponents(true);
			thisTransform.parent = null;
			setProjectionElementsControl(ProjectionElementsControlMode.SelfControlled);
		}
	}
	
	private void setCamUserTrackerComponents(bool enabled){
		for(int i=0;i<camUserTrackerTransform.Length;++i){
			camUserTrackerTransform[i].enabled = enabled;
		}
	}
	
	private void setProjectionElementsControl(ProjectionElementsControlMode controlMode){
		if(controlMode == ProjectionElementsControlMode.SelfControlled){
			loadPositions();
			
		}else if(controlMode == ProjectionElementsControlMode.UnderParentControl){
			savePositions();
			setProjectionElelementsHierarchy();
		}
	}
	
	private void setProjectionElelementsHierarchy(){
		transform.localPosition = new Vector3(0, 0.5f, 0);
			for(int i=0;i<projectionCameras.Length;++i){
				projectionCameras[i].transform.localPosition = Vector3.zero;
		}
	}
	
	private void savePositions(){
		camerasParentPosition = transform.position;
		for(int i=0;i<projectionCameras.Length;++i){
			cameraPositions[i] = projectionCameras[i].transform.position;
		}
	}
	
	public void loadPositions(){
		thisTransform.position = camerasParentPosition;
		for(int i=0;i<projectionCameras.Length;++i){
			projectionCameras[i].transform.position = cameraPositions[i];
		}
	}
}
