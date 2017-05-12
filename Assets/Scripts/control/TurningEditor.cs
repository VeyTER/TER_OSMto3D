using System;
using UnityEngine;

public class TurningEditor : ObjectEditor {
	public enum TurningStates { MOTIONLESS, TURNING}
	private TurningStates turningState;

	protected float selectedWallStartAngle;
	protected float selectedBuildingStartAngle;

	private GameObject turnHandler;
	private float turnHandlerStartOffset;

	public TurningEditor (GameObject turnHandler) {
		this.turningState = TurningStates.MOTIONLESS;

		this.selectedBuildingStartAngle = 0F;
		this.selectedWallStartAngle = 0F;

		this.turnHandler = turnHandler;
		this.turnHandler.SetActive (false);
	}

	public void InitializeTurningMode(EditionController.SelectionRanges selectionRange) {
		Camera mainCamera = Camera.main;
		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		Vector3 objectScreenPosition = Vector3.zero;

		if (selectionRange == EditionController.SelectionRanges.WALL) {
			objectScreenPosition = mainCamera.WorldToScreenPoint (selectedWall.transform.position);
			selectedWallStartAngle = selectedWall.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z, 360 - selectedWall.transform.rotation.eulerAngles.y); 
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			objectScreenPosition = mainCamera.WorldToScreenPoint (selectedBuilding.transform.position);
			selectedBuildingStartAngle = selectedBuilding.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z,  360 - selectedBuilding.transform.rotation.eulerAngles.y); 
		}
	}

	public void StartObjectTurning(EditionController.SelectionRanges selectionRange) {
		Vector3 turnHandlerStartPosition = turnHandler.transform.position;
		float turnHandlerStartAngle = turnHandler.transform.rotation.eulerAngles.z;

		Vector2 relativeMousePosition = new Vector2(turnHandlerStartPosition.x, turnHandlerStartPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI)) * Mathf.Rad2Deg;

		turnHandlerStartOffset = mouseAngle - turnHandlerStartAngle;

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
		turnHandler.transform.rotation = Quaternion.Euler(turnHandlerRotation.x, turnHandlerRotation.y, (mouseAngle - turnHandlerStartOffset));

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
			selectedBuildingNodes.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turnHandlerAngle, selectedBuildingRotation.z);
			BuildingsTools.GetInstance().UpdateNodes (selectedBuilding);
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

	public float SelectedBuildingStartAngle {
		get { return selectedBuildingStartAngle; }
		set { selectedBuildingStartAngle = value; }
	}

	public float SelectedWallStartAngle {
		get { return selectedWallStartAngle; }
		set { selectedWallStartAngle = value; }
	}

	public GameObject TurnHandler {
		get { return turnHandler; }
		set { turnHandler = value; }
	}
}
