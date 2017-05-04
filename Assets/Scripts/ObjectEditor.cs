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

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public bool WallEdited {
		get { return wallEdited; }
		set { wallEdited = value; }
	}

	public bool BuildingEdited {
		get { return buildingEdited; }
		set { buildingEdited = value; }
	}
}

