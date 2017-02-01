using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 position = this.transform.position;

		if (Input.GetKey (KeyCode.LeftControl)) {
			if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z)) {
				this.transform.Rotate(new Vector3(1,0,0));
			}
			if (Input.GetKey ("down") || Input.GetKey (KeyCode.S)) {
				this.transform.Rotate(new Vector3(-1,0,0));
			}
			if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q)) {
				this.transform.RotateAround(Vector3.zero, Vector3.up,1);
			}
			if (Input.GetKey ("right") || Input.GetKey (KeyCode.D)) {
				this.transform.RotateAround(Vector3.zero, Vector3.up,-1);
			}		
		} 
		else if (Input.GetKey (KeyCode.LeftShift)) {
			if (position.y<-0.4f && (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))) {
				position.y +=0.1f;
			}
			if (Input.GetKey ("down") || Input.GetKey (KeyCode.S)) {
				position.y -=0.1f;
			}		
		} 
		else {
			if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z)) {
				position.x +=0.1f;
			}
			if (Input.GetKey ("down") || Input.GetKey (KeyCode.S)) {
				position.x -=0.1f;
			}
			if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q)) {
				position.z -=0.1f;
			}
			if (Input.GetKey ("right") || Input.GetKey (KeyCode.D)) {
				position.z +=0.1f;
			}
		}




		this.transform.position = position;
	}
}
