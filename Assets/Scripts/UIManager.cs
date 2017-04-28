using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler {
	private ObjectBuilder objectBuilder;
	private BuildingEditor buildingEditor;

//	private bool wallsActive;
//	private bool roofsActive;
//	private bool highwaysActive;
//	private bool treesActive;
//	private bool cyclewaysActive;
//	private bool footwaysActive;
////	private bool busLanesActive = true;
//	private bool highwayNodesActive;
//	private bool buildingNodesActive;

	public UIManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();

//		this.wallsActive = true;
//		this.roofsActive = false;
//		this.highwaysActive = true;
//		this.treesActive = true;
//		this.cyclewaysActive = true;
//		this.footwaysActive = true;
////		this.busLanesActive = true;
//		this.highwayNodesActive = false;
//		this.buildingNodesActive = false;
	}

	public void OnBeginDrag (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		buildingEditor.StartMovingBuilding ();
	}

	public void OnDrag (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		buildingEditor.UpdateMovingBuilding ();
	}

	public void OnMouseUp () {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()
			&& (buildingEditor.EditionState == BuildingEditor.EditionStates.NONE_SELECTION || buildingEditor.EditionState == BuildingEditor.EditionStates.READY_TO_EDIT)) {
			objectBuilder = ObjectBuilder.GetInstance ();

			buildingEditor.ChangeBuilding (gameObject);
		}
	}

	public void OnPointerUp (PointerEventData eventData) {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		string sourceElementName = eventData.selectedObject.gameObject.name;

		switch (sourceElementName) {
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
			if (buildingEditor.EditionState == BuildingEditor.EditionStates.READY_TO_EDIT)
				buildingEditor.EnterMovingMode ();
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

	public void IncrementBuildingHeight(GameObject selectedBuilding) {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.IncrementBuildingHeight (selectedBuilding);
	}

	public void SetPanelInactive() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor buildingEditor = wallGroups.GetComponent<BuildingEditor> ();

		if (buildingEditor.EditionState != BuildingEditor.EditionStates.NONE_SELECTION) {
			buildingEditor.EditionState = BuildingEditor.EditionStates.MOVING_TO_INITIAL_SITUATION;
			buildingEditor.StartCoroutine (
				buildingEditor.MoveToInitSituation(() => {
					buildingEditor.EditionState = BuildingEditor.EditionStates.NONE_SELECTION;
				})
			);
			buildingEditor.ClosePanel (() => {
				Main.panel.SetActive (false);
			});
			buildingEditor.SelectedBuilding = null;
		}

		buildingsTools.DiscolorAll ();
	}

	public BuildingEditor BuildingEditor {
		get { return buildingEditor; }
		set { buildingEditor = value; }
	}
}