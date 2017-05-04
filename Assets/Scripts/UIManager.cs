using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	public static EditionController editionController;

	private ObjectBuilder objectBuilder;

	public UIManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public void Update() {
		switch (name) {
		case UINames.MOVE_HANDLER:
			if (Input.GetMouseButton (0)
			&& editionController.EditionState == EditionController.EditionStates.MOVING_MODE
				&& editionController.MovingState == EditionController.MovingStates.MOVING) {
				editionController.UpdateObjectMoving ();
				editionController.ShiftCamera ();
			}
			break;
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		switch (name) {
		case UINames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && editionController.MovingState == EditionController.MovingStates.MOTIONLESS) {
				editionController.StartObjectMoving ();
			}
			break;
		case UINames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && editionController.TurningState == EditionController.TurningStates.MOTIONLESS) {
				editionController.StartObjectTurning ();
			}
			break;
		}
	}

	public void OnDrag (PointerEventData eventData) {
		switch (name) {
		case UINames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && editionController.MovingState == EditionController.MovingStates.MOVING) {
				editionController.UpdateObjectMoving ();
			}
			break;
		case UINames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && editionController.TurningState == EditionController.TurningStates.TURNING) {
				editionController.UpdateObjectTurning ();
			}
			break;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		switch (name) {
		case UINames.MOVE_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && editionController.MovingState == EditionController.MovingStates.MOVING) {
				editionController.EndObjectMoving ();
			}
			break;
		case UINames.TURN_HANDLER:
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && editionController.TurningState == EditionController.TurningStates.TURNING) {
				editionController.EndObjectTurning ();
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
		case UINames.BUILDING_NODES_BUTTON:
			this.ToggleBuildingNodesVisibility ();
			break;
		case UINames.HIGHWAY_NODES_BUTTON:
			this.ToggleHighwayNodesVisibility ();
			break;
		case UINames.WALLS_BUTTON:
			this.ToggleWallsVisibility ();
			break;
		case UINames.ROOFS_BUTTON:
			this.ToggleRoofsVisibility ();
			break;
		case UINames.HIGHWAYS_BUTTON:
			this.ToggleHighwaysVisibility ();
			break;
		case UINames.FOOTWAYS_BUTTON:
			this.ToggleFootwaysVisibility ();
			break;
		case UINames.CYCLEWAYS_BUTTON:
			this.ToggleCyclewaysVisibility ();
			break;
		case UINames.TREES_BUTTON:
			this.ToggleTreesVisibility ();
			break;

		case UINames.BUILDING_NAME_TEXT_INPUT:
			
			break;
		case UINames.TEMPERATURE_INDICATOR_TEXT_INPUT:
			
			break;
		case UINames.HUMIDITY_INDICATOR_TEXT_INPUT:
			
			break;
		case UINames.MOVE_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterMovingMode ();
				editionController.InitialiseMovingMode ();
			}
			break;
		case UINames.TURN_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterTurningMode ();
				editionController.InitialiseTurningMode ();
			}
			break;
		case UINames.CHANGE_HEIGHT_BUTTON:
			
			break;
		case UINames.CHANGE_COLOR_BUTTON:
			
			break;
		case UINames.SLIDE_PANEL_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				if (editionController.PanelState == EditionController.PanelStates.CLOSED)
					editionController.TogglePanel (null);
				else if (editionController.PanelState == EditionController.PanelStates.OPEN)
					editionController.TogglePanel (null);
			}
			break;
		case UINames.VALIDATE_BUTTON:
			
			break;
		case UINames.CANCEL_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.ExitBuilding ();
			}
			break;
		case UINames.WALL_RANGE_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.WALL;
				editionController.InitialiseMovingMode ();
				editionController.InitialiseTurningMode ();
				this.enableWallRangeButton ();
			}
			break;
		case UINames.BUILDING_RANGE_BUTTON:
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.BUILDING;
				editionController.InitialiseMovingMode ();
				editionController.InitialiseTurningMode ();
				this.enableBuildingRangeButton ();
			}
			break;
		case UINames.VALDIATE_EDITION_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE) {
				editionController.ValidateEdit ();
				editionController.ExitMovingMode ();
			} else if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE) {
				editionController.ValidateEdit ();
				editionController.ExitTurningMode ();
			}
			break;
		case UINames.CANCEL_EDITION_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE) {
				editionController.CancelEdit ();
				editionController.ExitMovingMode ();
			} if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE) {
				editionController.CancelEdit ();
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
		GameObject buildingRangeButton = GameObject.Find (UINames.BUILDING_RANGE_BUTTON);

		Button buildingButtonComponent = buildingRangeButton.GetComponent<Button> ();
		Button wallButtonComponent = GetComponent<Button> ();

		buildingButtonComponent.interactable = true;
		wallButtonComponent.interactable = false;
	}

	public void enableBuildingRangeButton() {
		GameObject wallRangeButton = GameObject.Find (UINames.WALL_RANGE_BUTTON);

		Button wallButtonComponent = wallRangeButton.GetComponent<Button> ();
		Button buildingButtonComponent = GetComponent<Button> ();

		wallButtonComponent.interactable = true;
		buildingButtonComponent.interactable = false;
	}

	public void IncrementBuildingHeight(GameObject selectedBuilding) {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.IncrementBuildingHeight (selectedBuilding);
	}
}