using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
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

	public void SetPanelInnactive() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		buildingsTools.DiscolorAll ();
		buildingsTools.SelectedBuilding = null;

		Main.panel.SetActive (false);
	}

	public void IncrementBuildingHeight() {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.IncrementSelectedBuildingHeight ();
	}
}