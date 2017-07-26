using UnityEngine;
using UnityEditor;

public class VisibilityController {
	private static VisibilityController instance;

	private CityBuilder cityBuilder;

	private bool buildingNodesVisibility;
	private bool highwaysNodesVisibility;

	private bool wallsVisibility;
	private bool roofsVisibility;

	private bool highwaysVisibility;
	private bool footwaysVisibility;
	private bool cyclewaysVisibility;

	private bool treesVisibility;

	private VisibilityController() {
		this.cityBuilder = CityBuilder.GetInstance();

		this.buildingNodesVisibility = true;
		this.highwaysNodesVisibility = true;

		this.wallsVisibility = true;
		this.roofsVisibility = true;

		this.highwaysVisibility = true;
		this.footwaysVisibility = true;
		this.cyclewaysVisibility = true;

		this.treesVisibility = true;
	}

	public static VisibilityController GetInstance() {
		if (instance == null)
			instance = new VisibilityController();
		return instance;
	}

	public void HideBuildingNodes() {
		buildingNodesVisibility = false;
		this.ChangeBuildingComponentVisibility(false, CityBuilder.BUILDING_NODES_INDEX);
	}
	public void ShowBuildingNodes() {
		buildingNodesVisibility = true;
		this.ChangeBuildingComponentVisibility(true, CityBuilder.BUILDING_NODES_INDEX);
	}

	public void HideHighwayNodes() {

	}
	public void ShowHighwayNodes() {

	}

	public void HideWalls() {
		wallsVisibility = false;
		this.ChangeBuildingComponentVisibility(false, CityBuilder.WALLS_INDEX);
	}
	public void ShowWalls() {
		wallsVisibility = true;
		this.ChangeBuildingComponentVisibility(true, CityBuilder.WALLS_INDEX);
	}

	public void HideRoofs() {
		roofsVisibility = false;
		this.ChangeBuildingComponentVisibility(false, CityBuilder.ROOF_INDEX);
	}
	public void ShowRoofs() {
		roofsVisibility = true;
		this.ChangeBuildingComponentVisibility(true, CityBuilder.ROOF_INDEX);
	}

	public void HideTrees() {
		treesVisibility = false;
		cityBuilder.Trees.SetActive(false);
	}
	public void ShowTrees() {
		treesVisibility = true;
		cityBuilder.Trees.SetActive(true);
	}

	private void ChangeBuildingComponentVisibility(bool visibility, int componentIndex) {
		foreach (Transform buildingTransform in cityBuilder.Buildings.transform)
			buildingTransform.GetChild(componentIndex).gameObject.SetActive(visibility);
	}

	public bool BuildingNodesVisibility {
		get { return buildingNodesVisibility; }
	}

	public bool HighwaysNodesVisibility {
		get { return highwaysNodesVisibility; }
	}

	public bool WallsVisibility {
		get { return wallsVisibility; }
	}

	public bool RoofsVisibility {
		get { return roofsVisibility; }
	}

	public bool HighwaysVisibility {
		get { return highwaysVisibility; }
	}

	public bool FootwaysVisibility {
		get { return footwaysVisibility; }
	}

	public bool CyclewaysVisibility {
		get { return cyclewaysVisibility; }
	}

	public bool TreesVisibility {
		get { return treesVisibility; }
	}
}