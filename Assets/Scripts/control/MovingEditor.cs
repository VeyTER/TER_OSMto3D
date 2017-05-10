﻿using System;
using UnityEngine;

public class MovingEditor : ObjectEditor{
	public enum MovingStates { MOTIONLESS, MOVING}
	private MovingStates movingState;

	protected Vector3 selectedWallInitPos;
	protected Vector3 selectedBuildingInitPos;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitOffset;

	public MovingEditor (GameObject moveHandler) {
		this.movingState = MovingStates.MOTIONLESS;

		this.selectedWallInitPos = Vector3.zero;
		this.selectedBuildingInitPos = Vector3.zero;

		this.moveHandler = moveHandler;
		this.moveHandler.SetActive (false);
	}

	public void InitializeMovingMode(EditionController.SelectionRanges selectionRange) {
		Camera mainCamera = Camera.main;

		Vector3 objectPosition = Vector3.zero;
		Vector3 objectScale = Vector3.zero;

		if (selectionRange == EditionController.SelectionRanges.WALL) {
			objectPosition = selectedWall.transform.position;
			objectScale = selectedWall.transform.localScale;
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			objectPosition = selectedBuilding.transform.position;
			objectScale = selectedBuilding.transform.localScale;
		}

		Vector3 objectScreenPosition = mainCamera.WorldToScreenPoint (objectPosition);
		moveHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);

		if (selectionRange == EditionController.SelectionRanges.WALL)
			selectedWallInitPos = objectPosition;
		else if (selectionRange == EditionController.SelectionRanges.BUILDING)
			selectedBuildingInitPos = objectPosition;
	}

	public void StartObjectMoving(EditionController.SelectionRanges selectionRange) {
		Vector2 moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;

		if (selectionRange == EditionController.SelectionRanges.WALL) {
			wallEdited = true;
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			buildingEdited = true;
		}

		this.MoveObject (selectionRange);

		movingState = MovingStates.MOVING;
	}

	public void UpdateObjectMoving(EditionController.SelectionRanges selectionRange) {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerInitOffset;
		this.MoveObject (selectionRange);
	}

	private void MoveObject(EditionController.SelectionRanges selectionRange) {
		Camera mainCamera = Camera.main;
		Vector3 moveHandlerPosition = moveHandler.transform.position;

		Vector3 selectedObjectCurrentPos = mainCamera.ScreenToWorldPoint(new Vector3(moveHandlerPosition.x, moveHandlerPosition.y, mainCamera.transform.position.y));
		if (selectionRange == EditionController.SelectionRanges.WALL)
			selectedWall.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedWall.transform.position.y, selectedObjectCurrentPos.z);
		else if (selectionRange == EditionController.SelectionRanges.BUILDING)
			selectedBuilding.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedBuilding.transform.position.y, selectedObjectCurrentPos.z);
	}

	public void EndObjectMoving() {
		movingState = MovingStates.MOTIONLESS;
	}

	public void ShiftCamera() {
		GameObject wallsGroups = ObjectBuilder.GetInstance ().WallGroups;
		float cameraAngle = Mathf.Deg2Rad * wallsGroups.transform.rotation.eulerAngles.y;

		float cosOffset = 0.1F * (float)Math.Cos (cameraAngle);
		float sinOffset = 0.1F * (float)Math.Sin (cameraAngle);

		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		Camera mainCamera = Camera.main;
		Vector3 newCameraPosition = mainCamera.transform.localPosition;

		if (mousePosition.x > Screen.width * 0.8F)
			newCameraPosition = new Vector3 (newCameraPosition.x - sinOffset, newCameraPosition.y, newCameraPosition.z - cosOffset);
		else if (mousePosition.x < Screen.width * 0.2F)
			newCameraPosition = new Vector3 (newCameraPosition.x + sinOffset, newCameraPosition.y, newCameraPosition.z + cosOffset);

		if (mousePosition.y > Screen.height * 0.8F)
			newCameraPosition = new Vector3 (newCameraPosition.x + cosOffset, newCameraPosition.y, newCameraPosition.z - sinOffset);
		else if (mousePosition.y < Screen.height * 0.2F)
			newCameraPosition = new Vector3 (newCameraPosition.x - cosOffset, newCameraPosition.y, newCameraPosition.z + sinOffset);

		mainCamera.transform.localPosition = newCameraPosition;
	}

	public bool IsMotionless() {
		return movingState == MovingStates.MOTIONLESS;
	}
	public bool IsMoving() {
		return movingState == MovingStates.MOVING;
	}

	public MovingStates MovingState {
		get { return movingState; }
		set { movingState = value; }
	}

	public Vector3 SelectedWallInitPos {
		get { return selectedWallInitPos; }
		set { selectedWallInitPos = value; }
	}

	public Vector3 SelectedBuildingInitPos {
		get { return selectedBuildingInitPos; }
		set { selectedBuildingInitPos = value; }
	}

	public GameObject MoveHandler {
		get { return moveHandler; }
		set { moveHandler = value; }
	}
}