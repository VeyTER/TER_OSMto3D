using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IPointerUpHandler {
	private ObjectBuilder objectBuilder;

	private bool wallsActive;
	private bool roofsActive;
	private bool highwaysActive;
	private bool treesActive;
	private bool cyclewaysActive;
	private bool footwaysActive;
//	private bool busLanesActive = true;
	private bool highwayNodesActive;
	private bool buildingNodesActive;

	private BuildingEditor buildingEditor;

	public UIManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.wallsActive = true;
		this.roofsActive = true;
		this.highwaysActive = true;
		this.treesActive = true;
		this.cyclewaysActive = true;
		this.footwaysActive = true;
//		this.busLanesActive = true;
		this.highwayNodesActive = false;
		this.buildingNodesActive = false;

		this.buildingEditor = null;
	}

	public void OnPointerUp (PointerEventData eventData) {
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
//		case UINames.TEST_BUTTON:
//			this.IncrementBuildingHeight ();
//			break;
		case UINames.CANCEL_BUTTON:
			this.SetPanelInnactive ();
			break;
		}
	}

	public void ToggleBuildingNodesVisibility() {
		GameObject buildingNodes = objectBuilder.BuildingNodes;
		buildingNodesActive = !buildingNodesActive;
		buildingNodes.SetActive (buildingNodesActive);
	}

	public void ToggleHighwayNodesVisibility() {
		GameObject highwayNodes = objectBuilder.HighwayNodes;
		highwayNodesActive = !highwayNodesActive;
		highwayNodes.SetActive (highwayNodesActive);
	}

	public void ToggleWallsVisibility() {
		GameObject walls = objectBuilder.WallGroups;
		wallsActive = !wallsActive;
		walls.SetActive (wallsActive);
	}

	public void ToggleRoofsVisibility() {
		GameObject roofs = objectBuilder.Roofs;
		roofsActive = !roofsActive;
		roofs.SetActive (roofsActive);
	}

	public void ToggleHighwaysVisibility() {
		GameObject highways = objectBuilder.Highways;
		highwaysActive = !highwaysActive;
		highways.SetActive (highwaysActive);
	}

	public void ToggleFootwaysVisibility() {
		GameObject footways = objectBuilder.Footways;
		footwaysActive = !footwaysActive;
		footways.SetActive (footwaysActive);
	}

	public void ToggleCyclewaysVisibility() {
		GameObject cycleways = objectBuilder.Cycleways;
		cyclewaysActive = !cyclewaysActive;
		cycleways.SetActive (cyclewaysActive);
	}

	public void ToggleTreesVisibility() {
		GameObject trees = objectBuilder.Trees;
		treesActive = !treesActive;
		trees.SetActive (treesActive);
	}

	public void IncrementBuildingHeight() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.IncrementSelectedBuildingHeight ();
	}

	public void SetPanelInnactive() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		BuildingEditor[] childrenBuildingEditor = buildingsTools.SelectedBuilding.GetComponentsInChildren<BuildingEditor> ();
		foreach (BuildingEditor buildingEditor in childrenBuildingEditor) {
			if (buildingEditor.InUse ()) {
				buildingEditor.StartCoroutine ("MoveToInitSituation");
				buildingEditor.StartCoroutine ("ClosePanel");
			}
		}
		
		buildingsTools.DiscolorAll ();
		buildingsTools.SelectedBuilding = null;
	}

	public BuildingEditor BuildingEditor {
		get { return buildingEditor; }
		set { buildingEditor = value; }
	}
}