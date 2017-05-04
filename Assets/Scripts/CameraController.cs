using UnityEngine;
using System.Collections;
using System;

public class CameraController : MonoBehaviour {
	private EditionController editionController;

	public void Start() {
		editionController = UIManager.editionController;
	}

	// Update is called once per frame
	void Update () {
		Vector3 localPosition = transform.localPosition;

		if(editionController.CameraState == EditionController.CameraStates.FREE) {
			if (Input.GetKey (KeyCode.LeftControl)) {
				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					transform.Rotate (new Vector3 (-1, 0, 0));

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					transform.Rotate (new Vector3 (1, 0, 0));

				if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q))
					transform.RotateAround (localPosition, Vector3.up, -1);

				if (Input.GetKey ("right") || Input.GetKey (KeyCode.D))
					transform.RotateAround (localPosition, Vector3.up, 1);
			} else if (Input.GetKey (KeyCode.LeftShift)) {
				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					transform.Translate(new Vector3(0, 0.1f, 0));

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					transform.Translate(new Vector3(0, -0.1f, 0));
			} else {
				float cameraAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
				float cosOffset = 0.1F * (float)Math.Cos (cameraAngle);
				float sinOffset = 0.1F * (float)Math.Sin (cameraAngle);

				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					transform.localPosition = new Vector3 (localPosition.x + sinOffset, localPosition.y, localPosition.z + cosOffset);

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					transform.localPosition = new Vector3(localPosition.x - sinOffset, localPosition.y, localPosition.z - cosOffset);

				if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q))
					transform.localPosition = new Vector3(localPosition.x - cosOffset, localPosition.y, localPosition.z + sinOffset);

				if (Input.GetKey ("right") || Input.GetKey (KeyCode.D))
					transform.localPosition = new Vector3(localPosition.x + cosOffset, localPosition.y, localPosition.z - sinOffset);
			}
		}
	}
}
