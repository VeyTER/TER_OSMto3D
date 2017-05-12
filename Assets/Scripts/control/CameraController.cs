using UnityEngine;
using System.Collections;
using System;

public class CameraController : MonoBehaviour {
	public enum CameraStates { FREE, FLYING, FIXED }
	private CameraStates cameraState;

	private Vector3 initPosition;
	private Quaternion initRotation;

	private EditionController editionController;

	public void Start() {
		this.cameraState = CameraStates.FREE;
		this.editionController = UiManager.editionController;
	}

	// Update is called once per frame
	void Update () {
		Vector3 localPosition = transform.localPosition;

		if(cameraState == CameraStates.FREE) {
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

	public IEnumerator MoveToSituation(Vector3 targetPosition, Quaternion targetRotation, Action finalAction) {
		cameraState = CameraStates.FLYING;

		Vector3 buildingPosition = transform.position;
		Quaternion buildingRotation = transform.rotation;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (buildingPosition, targetPosition, cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (buildingRotation, targetRotation, cursor);

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		transform.position = targetPosition;
		transform.rotation = targetRotation;

		cameraState = CameraStates.FREE;

		if(finalAction != null)
			finalAction ();
	}

	public IEnumerator MoveToBuilding(GameObject building, Action finalAction) {
		cameraState = CameraStates.FLYING;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		Vector3 initPosition = transform.position;
		Quaternion initRotation = transform.rotation;

		Vector3 buildingCenterPosition = buildingsTools.BuildingCenter (building);
		Vector3 targetPosition = new Vector3(buildingCenterPosition.x, building.transform.position.y, buildingCenterPosition.z);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = building.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius (building);
		float targetPosZ = (float) (buildingHeight + buildingRadius / Math.Tan (cameraFOV)) * 0.8F;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (initPosition, new Vector3(targetPosition.x, targetPosZ, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (initRotation, targetRotation, cursor);

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		transform.position = new Vector3(targetPosition.x, targetPosZ, targetPosition.z);
		transform.rotation = targetRotation;

		cameraState = CameraStates.FIXED;

		if(finalAction != null)
			finalAction ();
	}

	public CameraStates CameraState {
		get { return cameraState; }
		set { cameraState = value; }
	}

	public Vector3 InitPosition {
		get { return initPosition; }
		set { initPosition = value; }
	}

	public Quaternion InitRotation {
		get { return initRotation; }
		set { initRotation = value; }
	}
}
