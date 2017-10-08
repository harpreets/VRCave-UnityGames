using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectionScreensController : MonoBehaviour {
	
	static public ProjectionScreensController instance;
	public GameObject[] projectionWalls;
	
	private Transform thisTransform;
	
	private PMappingController[] projectionWallsControllers;
	private PMappingController prevSelectedController;
	private bool isControlPressed, isEditButtonPressed;
	private bool isEditModeEnabled;
	private int currentSelectedObject;
	private PMappingKeyboardController pMappingKeyboardController;
	
	void Awake(){
		instance = this;	
	}
	
	// Use this for initialization
	void Start () {
		thisTransform = transform;
		prevSelectedController = null;
		isControlPressed = isEditButtonPressed = false;
		currentSelectedObject = -1;
		
		projectionWallsControllers = new PMappingController[projectionWalls.Length];
		for(int i=0;i<projectionWalls.Length;++i){
			projectionWallsControllers[i] = projectionWalls[i].GetComponent<PMappingController>();
		}
		
		pMappingKeyboardController = gameObject.AddComponent<PMappingKeyboardController>();
		pMappingKeyboardController.setActiveScreenController(projectionWallsControllers[0]);
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)){
			isControlPressed = true;
		}
		if(Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)){
			isControlPressed = false;
		}
		if(Input.GetKeyDown(KeyCode.E)){
			isEditButtonPressed = true;
		}
		if(Input.GetKeyDown(KeyCode.W)){
			isEditButtonPressed = false;
		}
		
		if(isControlPressed && isEditButtonPressed){
			isEditModeEnabled = true;
		}else if(isControlPressed && !isEditButtonPressed){
			isEditModeEnabled = false;
			Debug.Log("Edit mode has been disbaled");
		}
		
		if(isEditModeEnabled){
			if(Input.GetButtonDown("IterateObjects")){
				pMappingKeyboardController.showGUI = true;
				currentSelectedObject = (currentSelectedObject+1)%(projectionWalls.Length);
				Debug.Log("Current selected object: " + currentSelectedObject);
				if(prevSelectedController == null){
					SetScreenControllersState(false);
				}else{
					prevSelectedController.toggleMappingAdornersVisibility(false);
				}
				projectionWallsControllers[currentSelectedObject].toggleMappingAdornersVisibility(true);
				pMappingKeyboardController.setActiveScreenController( projectionWallsControllers[currentSelectedObject]);
//				pMappingKeyboardController.activeProjectionScreenController = projectionWallsControllers[currentSelectedObject];
			}
		}else{
			pMappingKeyboardController.showGUI = false;
			SetScreenControllersState(true);
		}	
	}
	
	private void SetScreenControllersState(bool enabled){
		for(int i=0;i<projectionWallsControllers.Length;++i){
			projectionWallsControllers[i].toggleMappingAdornersVisibility(enabled);
		}
	}
	
	
}
