using System;
using UnityEngine;

public class TurningEditor : ObjectEditor {
	public enum TurningStates { MOTIONLESS, TURNING}
	private TurningStates turningState;

	protected float selectedWallInitAngle;
	protected float selectedBuildingInitAngle;

	private GameObject turnHandler;
	private float turnHandlerInitOffset;

	public TurningEditor (GameObject turnHandler) {
		this.turningState = TurningStates.MOTIONLESS;

		this.selectedBuildingInitAngle = 0F;
		this.selectedWallInitAngle = 0F;

		this.turnHandler = turnHandler;
		this.turnHandler.SetActive (false);
	}

	public void InitializeTurningMode(EditionController.SelectionRanges selectionRange) {
		Camera mainCamera = Camera.main;
		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		Vector3 objectScreenPosition = Vector3.zero;

		if (selectionRange == EditionController.SelectionRanges.WALL) {
			objectScreenPosition = mainCamera.WorldToScreenPoint (selectedWall.transform.position);
			selectedWallInitAngle = selectedWall.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z, 360 - selectedWall.transform.rotation.eulerAngles.y); 
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			objectScreenPosition = mainCamera.WorldToScreenPoint (selectedBuilding.transform.position);
			selectedBuildingInitAngle = selectedBuilding.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z,  360 - selectedBuilding.transform.rotation.eulerAngles.y); 
		}
	}

	public void StartObjectTurning(EditionController.SelectionRanges selectionRange) {
		Vector3 turnHandlerInitPosition = turnHandler.transform.position;
		float turnHandlerInitAngle = turnHandler.transform.rotation.eulerAngles.z;

		Vector2 relativeMousePosition = new Vector2(turnHandlerInitPosition.x, turnHandlerInitPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI)) * Mathf.Rad2Deg;

		turnHandlerInitOffset = mouseAngle - turnHandlerInitAngle;

		if (selectionRange == EditionController.SelectionRanges.WALL)
			wallEdited = true;
		else if (selectionRange == EditionController.SelectionRanges.BUILDING)
			buildingEdited = true;

		this.TurnObject (selectionRange);

		turningState = TurningStates.TURNING;
	}

	public void UpdateObjectTurning(EditionController.SelectionRanges selectionRange) {
		Vector3 turnHandlerPosition = turnHandler.transform.position;

		Vector2 relativeMousePosition = new Vector2(turnHandlerPosition.x, turnHandlerPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI)) * Mathf.Rad2Deg;

		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		turnHandler.transform.rotation = Quaternion.Euler(turnHandlerRotation.x, turnHandlerRotation.y, (mouseAngle - turnHandlerInitOffset));

		this.TurnObject (selectionRange);
	}

	private void TurnObject(EditionController.SelectionRanges selectionRange) {
		Camera mainCamera = Camera.main;
		float turnHandlerAngle = 360 - turnHandler.transform.rotation.eulerAngles.z;

		if (selectionRange == EditionController.SelectionRanges.WALL) {
			Quaternion selectedWallRotation = selectedWall.transform.rotation;
			selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.x, turnHandlerAngle, selectedWallRotation.z);
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
			selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turnHandlerAngle, selectedBuildingRotation.z);
		}
	}

	public void EndObjectTurning() {
		turningState = TurningStates.MOTIONLESS;
	}

	public bool IsMotionless() {
		return turningState == TurningStates.MOTIONLESS;
	}
	public bool IsTurning() {
		return turningState == TurningStates.TURNING;
	}

	public TurningStates TurningState {
		get { return turningState; }
		set { turningState = value; }
	}

	public float SelectedBuildingInitAngle {
		get { return selectedBuildingInitAngle; }
		set { selectedBuildingInitAngle = value; }
	}

	public float SelectedWallInitAngle {
		get { return selectedWallInitAngle; }
		set { selectedWallInitAngle = value; }
	}

	public GameObject TurnHandler {
		get { return turnHandler; }
		set { turnHandler = value; }
	}
}
