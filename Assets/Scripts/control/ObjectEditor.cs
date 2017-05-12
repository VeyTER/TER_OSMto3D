using System;
using UnityEngine;
using System.Collections;

public class ObjectEditor {
	protected GameObject selectedWall;
	protected GameObject selectedBuilding;

	protected GameObject selectedWallNodes;
	protected GameObject selectedBuildingNodes;

	protected bool wallEdited;
	protected bool buildingEdited;

	public ObjectEditor () {
		this.selectedWall = null;
		this.selectedBuilding = null;

		this.selectedWallNodes = null;
		this.selectedBuildingNodes = null;

		this.wallEdited = false;
		this.buildingEdited = false;
	}

	public void Initialize(GameObject selectedWall, GameObject selectedBuilding) {
		this.SelectedWall = selectedWall;
		this.SelectedBuilding = selectedBuilding;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		SelectedBuildingNodes = buildingsTools.BuildingToBuildingNodeGroup (selectedBuilding);

//		TODO
//		this.SelectedWallNodes = selectedWall;

		WallTransformed = false;
		BuildingTransformed = false;
	}

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public GameObject SelectedWallNodes {
		get { return selectedWallNodes; }
		set { selectedWallNodes = value; }
	}
	
	public GameObject SelectedBuildingNodes {
		get { return selectedBuildingNodes; }
		set { selectedBuildingNodes = value; }
	}

	public bool WallTransformed {
		get { return wallEdited; }
		set { wallEdited = value; }
	}

	public bool BuildingTransformed {
		get { return buildingEdited; }
		set { buildingEdited = value; }
	}
}

