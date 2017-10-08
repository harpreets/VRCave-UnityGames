using UnityEngine;
using System.Collections;

public class GameObjUsrHeadTransform : MonoBehaviour {
	public GameObject trackedHeadGameObject;
	public GameObject trackedFeetGameObject;
	public GameObject trackedKneeGameObject;
	public GameObject trackedNeckGameObject;
	
	private ZigTrackedUser trackedUser;
	
	void Start () {
		trackedUser = null;
	}
	
	// Update is called once per frame
	void Update () {
		if(trackedUser != null && trackedHeadGameObject != null){
//			Vector3 updatedHeadPos = trackedUser.Skeleton[(int)ZigJointId.Head].Position / 1000.0f;
//			Vector3 updatedHeadPosition = new Vector3(-updatedHeadPos.x, updatedHeadPos.y, -updatedHeadPos.z);
			
			Vector3 updatedNeckPos = trackedUser.Skeleton[(int)ZigJointId.Neck].Position / 1000.0f;
			Vector3 updatedNeckPosition = new Vector3(-updatedNeckPos.x, updatedNeckPos.y, -updatedNeckPos.z);
			trackedNeckGameObject.transform.position = updatedNeckPosition;
			
			//actual head position
			trackedHeadGameObject.transform.position = new Vector3(updatedNeckPosition.x, updatedNeckPosition.y + 0.05f, updatedNeckPosition.z);
			
			Vector3 updatedFeetPos = trackedUser.Skeleton[(int)ZigJointId.LeftFoot].Position / 1000.0f;
			Vector3 updatedFeetPosition = new Vector3(-updatedFeetPos.x, updatedFeetPos.y, -updatedFeetPos.z);
			trackedFeetGameObject.transform.position = updatedFeetPosition;
			
			Vector3 updatedKneePos = trackedUser.Skeleton[(int)ZigJointId.LeftKnee].Position / 1000.0f;
			Vector3 updatedKneePosition = new Vector3(-updatedKneePos.x, updatedKneePos.y, -updatedKneePos.z);
			trackedKneeGameObject.transform.position = updatedKneePosition;
		}
	}
	
	void UserEngaged(ZigEngageSingleUser user){
		trackedUser = user.engagedTrackedUser;
	}
	
	void UserDisengaged(){
		trackedUser = null;
	}
}
