using System;
using UnityEngine;

public class ObjectEditor {
	protected GameObject selectedWall;
	protected GameObject selectedBuilding;

	protected bool wallEdited;
	protected bool buildingEdited;

	public ObjectEditor () {
		this.selectedWall = null;
		this.selectedBuilding = null;

		this.wallEdited = false;
		this.buildingEdited = false;
	}

	public void Initialize(GameObject selectedWall, GameObject selectedBuilding) {
		this.SelectedWall = selectedWall;
		this.SelectedBuilding = selectedBuilding;

		this.WallTransformed = false;
		this.BuildingTransformed = false;
	}

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
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

