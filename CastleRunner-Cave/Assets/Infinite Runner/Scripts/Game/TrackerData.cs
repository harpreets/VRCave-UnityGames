using UnityEngine;
using System.Collections;

public class TrackerData : MonoBehaviour {
	static public TrackerData instance;
	
	public Transform trackedHead;
	public Transform trackedFeet;
	public Transform trackedKnee;
	public Transform trackedNeck;
	
	void Awake(){
		instance = this;
	}
	
}
