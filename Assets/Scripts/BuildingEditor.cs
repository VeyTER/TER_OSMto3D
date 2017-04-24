﻿using System;
using UnityEngine;
using System.Collections;

public class BuildingEditor : MonoBehaviour {
	private enum EditionStates { NONE_SELECTION, MOVING_TO_BUILDING, READY_TO_EDIT, RENAMING, TRANSLATING, TURNING, VERTICAL_SCALING, MOVING_TO_INITIAL_SITUATION }
	private enum SelectionRanges { WALL, BUILDING }

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	public void Start() {
		this.editionState = EditionStates.NONE_SELECTION;
		this.selectionRange = SelectionRanges.WALL;

		this.objectBuilder = ObjectBuilder.GetInstance ();
		this.buildingsTools = BuildingsTools.GetInstance ();
	}

	public void OnMouseDown() {
		if (editionState == EditionStates.NONE_SELECTION) {
			GameObject mainCamera = objectBuilder.MainCamera;

			UIManager UIManager = FindObjectOfType<UIManager> ();
			UIManager.BuildingEditor = this;

			this.StartCoroutine ("MoveToBuilding");
		}
	}

	public IEnumerator MoveToBuilding() {
		GameObject mainCamera = objectBuilder.MainCamera;
		GameObject building = transform.parent.gameObject;

		Vector3 cameraPosition = mainCamera.transform.position;
		Quaternion cameraRotation = mainCamera.transform.rotation;

		cameraInitPosition = new Vector3 (cameraPosition.x, cameraPosition.y, cameraPosition.z);
		cameraInitRotation = new Quaternion (cameraRotation.x, cameraRotation.y, cameraRotation.z, cameraRotation.w);

		Vector3 targetPosition = buildingsTools.BuildingCenter (building);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		editionState = EditionStates.MOVING_TO_BUILDING;

		for (double i = 0; i <= 1; i += 0.1F) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (cameraInitPosition, new Vector3(targetPosition.x, 1.242F, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (cameraInitRotation, targetRotation, cursor);

			mainCamera.transform.position = cameraCurrentPosition;
			mainCamera.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		editionState = EditionStates.READY_TO_EDIT;
	}

	public IEnumerator MoveToInitSituation() {
		GameObject mainCamera = objectBuilder.MainCamera;
		GameObject building = transform.parent.gameObject;

		Vector3 buildingPosition = mainCamera.transform.position;
		Quaternion buildingRotation = mainCamera.transform.rotation;

		Vector3 targetPosition = cameraInitPosition;
		Quaternion targetRotation = cameraInitRotation;

		editionState = EditionStates.MOVING_TO_INITIAL_SITUATION;

		for (double i = 0; i <= 1; i += 0.1F) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (buildingPosition, targetPosition, cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (buildingRotation, targetRotation, cursor);

			mainCamera.transform.position = cameraCurrentPosition;
			mainCamera.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		editionState = EditionStates.NONE_SELECTION;
	}

	public bool InUse() {
		return editionState != EditionStates.NONE_SELECTION;
	}
}