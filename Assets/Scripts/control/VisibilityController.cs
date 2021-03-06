﻿using UnityEngine;
using UnityEditor;

public class VisibilityController {
	private static VisibilityController instance;

	private CityBuilder cityBuilder;

	private bool buildingNodesVisibility;
	private bool roadsNodesVisibility;

	private bool wallsVisibility;
	private bool roofsVisibility;

	private bool roadsVisibility;
	private bool footwaysVisibility;
	private bool cyclewaysVisibility;

	private bool treesVisibility;

	private VisibilityController() {
		this.cityBuilder = CityBuilder.GetInstance();

		this.buildingNodesVisibility = true;
		this.roadsNodesVisibility = true;

		this.wallsVisibility = true;
		this.roofsVisibility = true;

		this.roadsVisibility = true;
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
		this.ChangeBuildingDeviceVisibility(false, CityBuilder.BUILDING_NODES_INDEX);
	}
	public void ShowBuildingNodes() {
		buildingNodesVisibility = true;
		this.ChangeBuildingDeviceVisibility(true, CityBuilder.BUILDING_NODES_INDEX);
	}

	public void HideRoadsNodes() {
		roadsNodesVisibility = false;
		this.ChangeRoadDeviceVisibility(false, CityBuilder.ROAD_NODES_INDEX);
	}
	public void ShowRoadsNodes() {
		roadsNodesVisibility = true;
		this.ChangeRoadDeviceVisibility(true, CityBuilder.ROAD_NODES_INDEX);
	}

	public void HideWalls() {
		wallsVisibility = false;
		this.ChangeBuildingDeviceVisibility(false, CityBuilder.WALLS_INDEX);
	}
	public void ShowWalls() {
		wallsVisibility = true;
		this.ChangeBuildingDeviceVisibility(true, CityBuilder.WALLS_INDEX);
	}

	public void HideRoofs() {
		roofsVisibility = false;
		this.ChangeBuildingDeviceVisibility(false, CityBuilder.ROOF_INDEX);
	}
	public void ShowRoofs() {
		roofsVisibility = true;
		this.ChangeBuildingDeviceVisibility(true, CityBuilder.ROOF_INDEX);
	}

	public void HideRoads() {
		roadsVisibility = false;
		this.ChangeRoadDeviceVisibility(false, CityBuilder.ROAD_SECTIONS_INDEX);
	}
	public void ShowRoads() {
		roadsVisibility = true;
		this.ChangeRoadDeviceVisibility(true, CityBuilder.ROAD_SECTIONS_INDEX);
	}

	public void HideFootways() {
		footwaysVisibility = false;
		cityBuilder.Footways.SetActive(false);
	}
	public void ShowFootways() {
		footwaysVisibility = true;
		cityBuilder.Footways.SetActive(true);
	}

	public void HideCycleways() {
		cyclewaysVisibility = false;
		cityBuilder.Cycleways.SetActive(false);
	}
	public void ShowCycleways() {
		cyclewaysVisibility = true;
		cityBuilder.Cycleways.SetActive(true);
	}

	public void HideTrees() {
		treesVisibility = false;
		cityBuilder.Trees.SetActive(false);
	}
	public void ShowTrees() {
		treesVisibility = true;
		cityBuilder.Trees.SetActive(true);
	}

	private void ChangeBuildingDeviceVisibility(bool visibility, int deviceIndex) {
		foreach (Transform buildingTransform in cityBuilder.Buildings.transform)
			buildingTransform.GetChild(deviceIndex).gameObject.SetActive(visibility);
	}

	private void ChangeRoadDeviceVisibility(bool visibility, int deviceIndex) {
		foreach (Transform buildingTransform in cityBuilder.Roads.transform)
			buildingTransform.GetChild(deviceIndex).gameObject.SetActive(visibility);
	}

	public bool BuildingNodesVisibility {
		get { return buildingNodesVisibility; }
	}

	public bool RoadsNodesVisibility {
		get { return roadsNodesVisibility; }
	}

	public bool WallsVisibility {
		get { return wallsVisibility; }
	}

	public bool RoofsVisibility {
		get { return roofsVisibility; }
	}

	public bool RoadsVisibility {
		get { return roadsVisibility; }
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