using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

	public PlayerController jc;

	//void Update(){
	//	transform.position = new Vector3(jc.transform.position.x, jc.transform.position.y, transform.position.z);
	//}
		
	void FixedUpdate () {
		transform.position = Vector3.Lerp (transform.position, new Vector3 (jc.transform.position.x, jc.transform.position.y, transform.position.z), 0.1f);
	}
}
