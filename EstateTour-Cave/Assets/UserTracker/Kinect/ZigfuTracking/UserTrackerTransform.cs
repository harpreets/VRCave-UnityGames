using UnityEngine;
using System.Collections;

public class UserTrackerTransform : MonoBehaviour {
	
	public Transform trackedUserHeadPosition;

	private Vector3 mappedHeadPosition;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.position = new Vector3(trackedUserHeadPosition.position.x, trackedUserHeadPosition.position.y , trackedUserHeadPosition.position.z);
	}
	
	float Map(float tobMapped, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp){
		float outVal = ((tobMapped - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin);
		
		if(clamp){
			if(outputMax < outputMin){
				if(outVal < outputMax) outVal = outputMax;
				else if (outVal > outputMin) outVal = outputMin;
			}else{
				if(outVal > outputMax) outVal = outputMax; 
				else if (outVal < outputMin) outVal = outputMin;
			}
		}
		return outVal;
	}
}
