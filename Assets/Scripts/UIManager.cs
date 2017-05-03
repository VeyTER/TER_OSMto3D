using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	private ObjectBuilder objectBuilder;

	public UIManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public void Update() {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		switch (name) {
		case UINames.MOVE_HANDLER:
			if (Input.GetMouseButton (0) && buildingEditor.MovingState == BuildingEditor.MovingStates.MOVING) {
				buildingEditor.UpdateObjectMoving ();
				buildingEditor.ShiftCamera ();
			}
			break;
		}
	}

	public void OnBeginDrag (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		switch (name) {
		case UINames.MOVE_HANDLER:
			if(buildingEditor.MovingState == BuildingEditor.MovingStates.MOTIONLESS)
				buildingEditor.StartObjectMoving ();
			break;
		}
	}

	public void OnDrag (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		switch (name) {
		case UINames.MOVE_HANDLER:
			if(buildingEditor.MovingState == BuildingEditor.MovingStates.MOVING)
				buildingEditor.UpdateObjectMoving ();
			break;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		switch (name) {
		case UINames.MOVE_HANDLER:
			if(buildingEditor.MovingState == BuildingEditor.MovingStates.MOVING)
				buildingEditor.EndObjectMoving ();
			break;
		}
	}

	public void OnMouseUp () {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()
			&& (buildingEditor.EditionState == BuildingEditor.EditionStates.NONE_SELECTION || buildingEditor.EditionState == BuildingEditor.EditionStates.READY_TO_EDIT)) {
			objectBuilder = ObjectBuilder.GetInstance ();

			buildingEditor.SwitchBuilding (gameObject);
		}
	}

	public void OnPointerUp (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

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
			this.SetPanelInactive ();
			break;
		case UINames.TEMPERATURE_INDICATOR_TEXT_INPUT:
			this.SetPanelInactive ();
			break;
		case UINames.HUMIDITY_INDICATOR_TEXT_INPUT:
			this.SetPanelInactive ();
			break;
		case UINames.MOVE_BUTTON:
			if (buildingEditor.EditionState == BuildingEditor.EditionStates.READY_TO_EDIT) {
				buildingEditor.EnterMovingMode ();
				buildingEditor.InitialiseMovingMode ();
			}
			break;
		case UINames.TURN_BUTTON:
			this.SetPanelInactive ();
			break;
		case UINames.CHANGE_HEIGHT_BUTTON:
			this.SetPanelInactive ();
			break;
		case UINames.CHANGE_COLOR_BUTTON:
			this.SetPanelInactive ();
			break;
		case UINames.SLIDE_PANEL_BUTTON:
			buildingEditor.TogglePanel (null);
			break;
		case UINames.VALIDATE_BUTTON:
			this.SetPanelInactive ();
			break;
		case UINames.CANCEL_BUTTON:
			this.SetPanelInactive ();
			break;
		case UINames.WALL_RANGE_BUTTON:
			buildingEditor.SelectionRange = BuildingEditor.SelectionRanges.WALL;
			buildingEditor.InitialiseMovingMode ();
			this.enableWallRangeButton ();
			break;
		case UINames.BUILDING_RANGE_BUTTON:
			buildingEditor.SelectionRange = BuildingEditor.SelectionRanges.BUILDING;
			buildingEditor.InitialiseMovingMode ();
			this.enableBuildingRangeButton ();
			break;
		case UINames.VALDIATE_EDITION_BUTTON:
			if (buildingEditor.EditionState == BuildingEditor.EditionStates.MOVING_MODE) {
				buildingEditor.ValidateEdit ();
				buildingEditor.ExitMovingMode ();
			}
			break;
		case UINames.CANCEL_EDITION_BUTTON:
			if (buildingEditor.EditionState == BuildingEditor.EditionStates.MOVING_MODE) {
				buildingEditor.CancelEdit ();
				buildingEditor.ExitMovingMode ();
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

	public void SetPanelInactive() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		if (buildingEditor.EditionState == BuildingEditor.EditionStates.READY_TO_EDIT) {
			buildingEditor.SelectedBuilding = null;
			buildingEditor.SelectedWall = null;

			buildingEditor.EditionState = BuildingEditor.EditionStates.MOVING_TO_INITIAL_SITUATION;
			buildingEditor.CameraState = BuildingEditor.CameraStates.FLYING;

			buildingsTools.DiscolorAllBuildings ();

			buildingEditor.StartCoroutine (
				buildingEditor.MoveToInitSituation(() => {
					buildingEditor.EditionState = BuildingEditor.EditionStates.NONE_SELECTION;
					buildingEditor.CameraState = BuildingEditor.CameraStates.FREE;
				})
			);
			buildingEditor.ClosePanel (() => {
				buildingEditor.LateralPanel.SetActive (false);
			});
		}
	}
}