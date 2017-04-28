using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	private BuildingEditor buildingEditor;

	public void Start() {
		GameObject wallGroups = GameObject.Find (CityNames.WALLS);
		buildingEditor = wallGroups.GetComponent<BuildingEditor> ();
	}

	// Update is called once per frame
	void Update () {
		Vector3 localPosition = this.transform.localPosition;
		Quaternion localRotation = this.transform.localRotation;

		if(buildingEditor.EditionState == BuildingEditor.EditionStates.NONE_SELECTION) {
			if (Input.GetKey (KeyCode.LeftControl)) {
				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					this.transform.Rotate (new Vector3 (-1, 0, 0));

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					this.transform.Rotate (new Vector3 (1, 0, 0));

				if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q))
					this.transform.RotateAround (Vector3.zero, Vector3.up, -1);

				if (Input.GetKey ("right") || Input.GetKey (KeyCode.D))
					this.transform.RotateAround (Vector3.zero, Vector3.up, 1);
			} else if (Input.GetKey (KeyCode.LeftShift)) {
				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					localPosition.y += 0.1f;

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					localPosition.y -= 0.1f;
			} else {
				if (Input.GetKey ("up") || Input.GetKey (KeyCode.Z))
					localPosition.x += 0.1f;

				if (Input.GetKey ("down") || Input.GetKey (KeyCode.S))
					localPosition.x -= 0.1f;

				if (Input.GetKey ("left") || Input.GetKey (KeyCode.Q))
					localPosition.z += 0.1f;

				if (Input.GetKey ("right") || Input.GetKey (KeyCode.D))
					localPosition.z -= 0.1f;
			}
		}

		this.transform.localPosition = localPosition;
	}
}
