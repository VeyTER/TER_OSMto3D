using System;
using UnityEngine;
using System.Collections;

public class ObjectEditor {
	protected GameObject selectedWall;
	protected GameObject selectedBuilding;

	protected GameObject selectedWallNodeGroup;
	protected GameObject selectedBuildingNodeGroup;

	protected bool wallEdited;
	protected bool buildingEdited;

	public ObjectEditor () {
		this.selectedWall = null;
		this.selectedBuilding = null;

		this.selectedWallNodeGroup = null;
		this.selectedBuildingNodeGroup = null;

		this.wallEdited = false;
		this.buildingEdited = false;
	}

	public void Initialize(GameObject selectedWall, GameObject selectedBuilding) {
		this.SelectedWall = selectedWall;
		this.SelectedBuilding = selectedBuilding;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance ();

		NodeGroup buildingNode = buildingsTools.GameObjectToNodeGroup (selectedBuilding);

		int buildingNodeGroupId = (int)objectBuilder.BuildingNodesIdTable[buildingNode.Id];

		Transform buildingNodesTransform = objectBuilder.BuildingNodes.transform;
		int i = 0;
		for (; i < buildingNodesTransform.childCount && buildingNodesTransform.GetChild(i).transform.GetInstanceID() != buildingNodeGroupId; i++);
		if (i < buildingNodesTransform.childCount)
			SelectedBuildingNodes = buildingNodesTransform.GetChild (i).gameObject;
		else
			SelectedBuildingNodes = null;

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
		get { return selectedWallNodeGroup; }
		set { selectedWallNodeGroup = value; }
	}
	
	public GameObject SelectedBuildingNodes {
		get { return selectedBuildingNodeGroup; }
		set { selectedBuildingNodeGroup = value; }
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

