using System;
using UnityEngine;

public class HeightChangingEditor : ObjectEditor {
	private ObjectBuilder objectBuilder;

	private int selectedBuildingStartHeight;

	private GameObject topFloor;
	private GameObject bottomFloor;

	public HeightChangingEditor () {
		this.objectBuilder = ObjectBuilder.GetInstance();

		this.selectedBuildingStartHeight = -1;

		this.topFloor = null;
		this.bottomFloor = null;
	}

	public void InitializeHeightChangingMode() {
		NodeGroup nodeGroup = BuildingsTools.GetInstance().BuildingToNodeGroup(selectedBuilding);

		Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
		Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

		topFloor = objectBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor + 1, greenOverlay);
		bottomFloor = objectBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor, redOverlay);

		selectedBuildingStartHeight = nodeGroup.NbFloor;

		//Vector2 nearestPoint = this.NearestBuildingPoint(nodeGroup);
	}

	//private Vector2 NearestBuildingPoint(NodeGroup nodeGroup) {
	//	Vector2 res = Vector2.zero;
	//	Vector3 buildingCenter = buildingsTools.BuildingCenter(selectedBuilding);
	//	float minDistance = float.PositiveInfinity;
	//	foreach (Node node in nodeGroup.Nodes) {
	//		Vector2 nodeLocation = new Vector2((float) node.Longitude, (float) node.Latitude);
	//		float distance = Vector2.Distance(nodeLocation, new Vector2(buildingCenter.x, buildingCenter.z));
	//		if (distance < minDistance) {
	//			res = nodeLocation;
	//			minDistance = distance;
	//		}
	//	}
	//	return res;
	//}

	public int ExpansionDirection(GameObject clickedWall) {
		Transform topWallsTransform = topFloor.transform;
		Transform bottomWallsTransform = bottomFloor.transform;

		int i = 0;
		for (; i < topWallsTransform.childCount && clickedWall != topWallsTransform.GetChild(i).gameObject; i++);

		if (i < topWallsTransform.childCount) {
			return 1;
		} else {
			int j = 0;
			for (; j < bottomWallsTransform.childCount && clickedWall != bottomWallsTransform.GetChild(j).gameObject; j++) ;

			if (j < bottomWallsTransform.childCount)
				return -1;
			else
				return 0;
		}
	}

	public void IncrementObjectHeight() {
		NodeGroup nodeGroup = BuildingsTools.GetInstance().BuildingToNodeGroup(selectedBuilding);
		objectBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor + 1);
		nodeGroup.NbFloor++;
	}

	public void DecrementObjectHeight() {
		NodeGroup nodeGroup = BuildingsTools.GetInstance().BuildingToNodeGroup(selectedBuilding);
		objectBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor - 1);
		nodeGroup.NbFloor--;
	}

	public int SelectedBuildingStartHeight {
		get { return selectedBuildingStartHeight; }
		set { selectedBuildingStartHeight = value; }
	}

	public GameObject TopFloor {
		get { return topFloor; }
		set { topFloor = value; }
	}

	public GameObject BottomFloor {
		get { return bottomFloor; }
		set { bottomFloor = value; }
	}
}
