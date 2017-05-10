using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UiManager : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	public static EditionController editionController;

	public static MovingEditor movingEditor;
	public static TurningEditor turningEditor;

	private ObjectBuilder objectBuilder;

	public UiManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public void Start() {
		if(editionController != null && (movingEditor == null || turningEditor == null)) {
			movingEditor = editionController.MovingEditor;
			turningEditor = editionController.TurningEditor;
		}
	}

	public void Update() {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			if (Input.GetMouseButton (0)
			&& editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editionController.SelectionRange);
				movingEditor.ShiftCamera ();
			}
			break;
		}
	}

	public void OnValueChanged(InputField originInputFiled) {
		if(!editionController.SelectedBuilding.name.Equals(originInputFiled.text))
			editionController.RenameBuilding (editionController.SelectedBuilding, originInputFiled.text);
	}

	public void OnEndChanged(InputField originInputFiled) {
		if(!editionController.SelectedBuilding.name.Equals(originInputFiled.text))
			editionController.RenameBuilding (editionController.SelectedBuilding, originInputFiled.text);
	}

	public void OnBeginDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMotionless()) {
				movingEditor.StartObjectMoving (editionController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsMotionless()) {
				turningEditor.StartObjectTurning (editionController.SelectionRange);
			}
			break;
		}
	}

	public void OnDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editionController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.UpdateObjectTurning (editionController.SelectionRange);
			}
			break;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.EndObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.EndObjectTurning ();
			}
			break;
		}
	}

	public void OnMouseUp () {
		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()
			&& (editionController.EditionState == EditionController.EditionStates.NONE_SELECTION || editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT)) {
			objectBuilder = ObjectBuilder.GetInstance ();
			editionController.SwitchBuilding (gameObject);
		}
	}

	public void OnPointerUp (PointerEventData eventData) {
		switch (name) {
		case UiNames.BUILDING_NODES_BUTTON:
			this.ToggleBuildingNodesVisibility ();
			break;
		case UiNames.HIGHWAY_NODES_BUTTON:
			this.ToggleHighwayNodesVisibility ();
			break;
		case UiNames.WALLS_BUTTON:
			this.ToggleWallsVisibility ();
			break;
		case UiNames.ROOFS_BUTTON:
			this.ToggleRoofsVisibility ();
			break;
		case UiNames.HIGHWAYS_BUTTON:
			this.ToggleHighwaysVisibility ();
			break;
		case UiNames.FOOTWAYS_BUTTON:
			this.ToggleFootwaysVisibility ();
			break;
		case UiNames.CYCLEWAYS_BUTTON:
			this.ToggleCyclewaysVisibility ();
			break;
		case UiNames.TREES_BUTTON:
			this.ToggleTreesVisibility ();
			break;

		case UiNames.BUILDING_NAME_TEXT_INPUT:
			
			break;
		case UiNames.TEMPERATURE_INDICATOR_TEXT_INPUT:
			
			break;
		case UiNames.HUMIDITY_INDICATOR_TEXT_INPUT:
			
			break;
		case UiNames.MOVE_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterMovingMode ();
				movingEditor.InitializeMovingMode (editionController.SelectionRange);
			}
			break;
		case UiNames.TURN_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterTurningMode ();
				turningEditor.InitializeTurningMode (editionController.SelectionRange);
			}
			break;
		case UiNames.CHANGE_HEIGHT_BUTTON:
			
			break;
		case UiNames.CHANGE_COLOR_BUTTON:
			
			break;
		case UiNames.SLIDE_PANEL_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				if (editionController.PanelState == EditionController.PanelStates.CLOSED)
					editionController.TogglePanel (null);
				else if (editionController.PanelState == EditionController.PanelStates.OPEN)
					editionController.TogglePanel (null);
			}
			break;
		case UiNames.VALIDATE_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.ValidateEdit ();
				editionController.ExitBuilding ();
			}
			break;
		case UiNames.CANCEL_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.CancelEdit ();
				editionController.ExitBuilding ();
			}
			break;
		case UiNames.WALL_RANGE_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.WALL;
				if(editionController.EditionState == EditionController.EditionStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editionController.SelectionRange);
				else if(editionController.EditionState == EditionController.EditionStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editionController.SelectionRange);
				this.enableWallRangeButton ();
			}
			break;
		case UiNames.BUILDING_RANGE_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.BUILDING;
				if(editionController.EditionState == EditionController.EditionStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editionController.SelectionRange);
				else if(editionController.EditionState == EditionController.EditionStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editionController.SelectionRange);
				this.enableBuildingRangeButton ();
			}
			break;
		case UiNames.VALDIATE_EDITION_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE) {
				editionController.ValidateTransform ();
				editionController.ExitMovingMode ();
			} else if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE) {
				editionController.ValidateTransform ();
				editionController.ExitTurningMode ();
			}
			break;
		case UiNames.CANCEL_EDITION_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE) {
				editionController.CancelTransform ();
				editionController.ExitMovingMode ();
			} if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE) {
				editionController.CancelTransform ();
				editionController.ExitTurningMode ();
			}
			break;
		}
	}

	public void ToggleBuildingNodesVisibility() {
		GameObject buildingNodes = objectBuilder.BuildingNodes;
		buildingNodes.SetActive (!buildingNodes.activeInHierarchy);
	}

	public void ToggleHighwayNodesVisibility() {
		GameObject highwayNodes = objectBuilder.HighwayNodes;
		highwayNodes.SetActive (!highwayNodes.activeInHierarchy);
	}

	public void ToggleWallsVisibility() {
		GameObject walls = objectBuilder.WallGroups;
		walls.SetActive (!walls.activeInHierarchy);
	}

	public void ToggleRoofsVisibility() {
		GameObject roofs = objectBuilder.Roofs;
		roofs.SetActive (!roofs.activeInHierarchy);
	}

	public void ToggleHighwaysVisibility() {
		GameObject highways = objectBuilder.Highways;
		highways.SetActive (!highways.activeInHierarchy);
	}

	public void ToggleFootwaysVisibility() {
		GameObject footways = objectBuilder.Footways;
		footways.SetActive (!footways.activeInHierarchy);
	}

	public void ToggleCyclewaysVisibility() {
		GameObject cycleways = objectBuilder.Cycleways;
		cycleways.SetActive (!cycleways.activeInHierarchy);
	}

	public void ToggleTreesVisibility() {
		GameObject trees = objectBuilder.Trees;
		trees.SetActive (!trees.activeInHierarchy);
	}

	public void enableWallRangeButton() {
		GameObject buildingRangeButton = GameObject.Find (UiNames.BUILDING_RANGE_BUTTON);

		Button buildingButtonComponent = buildingRangeButton.GetComponent<Button> ();
		Button wallButtonComponent = GetComponent<Button> ();

		buildingButtonComponent.interactable = true;
		wallButtonComponent.interactable = false;
	}

	public void enableBuildingRangeButton() {
		GameObject wallRangeButton = GameObject.Find (UiNames.WALL_RANGE_BUTTON);

		Button wallButtonComponent = wallRangeButton.GetComponent<Button> ();
		Button buildingButtonComponent = GetComponent<Button> ();

		wallButtonComponent.interactable = true;
		buildingButtonComponent.interactable = false;
	}

	public void IncrementBuildingHeight(GameObject selectedBuilding) {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.IncrementHeight (selectedBuilding);
	}
}