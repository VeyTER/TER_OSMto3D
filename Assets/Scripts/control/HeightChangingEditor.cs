using System;
using System.Collections.Generic;
using UnityEngine;

public class HeightChangingEditor : ObjectEditor {
	private ObjectBuilder objectBuilder;

	private int selectedBuildingStartHeight;

	private GameObject topFloor;
	private GameObject bottomFloor;

	private FloorColorController topFloorColorController;
	private FloorColorController bottomFloorColorController;

	public HeightChangingEditor() {
		this.objectBuilder = ObjectBuilder.GetInstance();

		this.selectedBuildingStartHeight = -1;

		this.topFloor = null;
		this.bottomFloor = null;
	}

	public void InitializeHeightChangingMode() {
		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);

		Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
		Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

		topFloor = objectBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor + 1, greenOverlay);
		bottomFloor = objectBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor, redOverlay);

		topFloor.AddComponent<FloorColorController>();
		bottomFloor.AddComponent<FloorColorController>();

		topFloorColorController = topFloor.GetComponent<FloorColorController>();
		bottomFloorColorController = bottomFloor.GetComponent<FloorColorController>();

		topFloorColorController.TargetMaterial = greenOverlay;
		bottomFloorColorController.TargetMaterial = redOverlay;

		topFloorColorController.ColorHue = 144;
		bottomFloorColorController.ColorHue = 0;

		topFloorColorController.StartCoroutine( topFloorColorController.Animate() );
		bottomFloorColorController.StartCoroutine( bottomFloorColorController.Animate() );

		selectedBuildingStartHeight = nodeGroup.NbFloor;
	}

	public int DesiredDirection(GameObject clickedWall) {
		Transform topWallsTransform = topFloor.transform;
		Transform bottomWallsTransform = bottomFloor.transform;

		int i = 0;
		for (; i < topWallsTransform.childCount && clickedWall != topWallsTransform.GetChild(i).gameObject; i++) ;

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
		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);
		objectBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor + 1);
		nodeGroup.NbFloor++;

		this.ShiftFloor(1);
		this.UpdateCameraPosition();
	}

	public void DecrementObjectHeight() {
		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);
		objectBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor - 1);
		nodeGroup.NbFloor--;

		this.ShiftFloor(-1);
		this.UpdateCameraPosition();
	}

	private void ShiftFloor(int direction) {
		int expansionDirection = direction > 0 ? 1 : -1;

		foreach (Transform wallTransform in topFloor.transform) {
			Vector3 topWallPosition = wallTransform.transform.position;
			wallTransform.position = new Vector3(topWallPosition.x, topWallPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), topWallPosition.z);
		}

		foreach (Transform wallTransform in bottomFloor.transform) {
			Vector3 bottomWallPosition = wallTransform.transform.position;
			wallTransform.position = new Vector3(bottomWallPosition.x, bottomWallPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), bottomWallPosition.z);
		}
	}

	private void UpdateCameraPosition() {
		CameraController cameraController = Camera.main.GetComponent<CameraController>();
		float cameraOrientation = cameraController.RelativeOrientation(selectedBuilding);
		cameraController.TeleportToBuilding(selectedBuilding, true, cameraOrientation, 15);
	}

	public override void ValidateTransform() {
		if (!transformedObjects.Contains(selectedBuilding))
			transformedObjects.Add(selectedBuilding);
	}

	public override void CancelTransform() {
		objectBuilder.RebuildBuilding(selectedBuilding, selectedBuildingStartHeight);

		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);
		nodeGroup.NbFloor = selectedBuildingStartHeight;
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

	public FloorColorController TopFloorColorController {
		get { return topFloorColorController; }
		set { topFloorColorController = value; }
	}

	public FloorColorController BottomFloorColorController {
		get { return bottomFloorColorController; }
		set { bottomFloorColorController = value; }
	}
}
