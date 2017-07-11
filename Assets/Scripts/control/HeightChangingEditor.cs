using UnityEngine;

public class HeightChangingEditor : ObjectEditor {
	private int selectedBuildingStartHeight;

	private GameObject topFloor;
	private GameObject bottomFloor;

	private StageColorController topFloorColorController;
	private StageColorController bottomFloorColorController;

	public HeightChangingEditor() {
		this.selectedBuildingStartHeight = -1;

		this.topFloor = null;
		this.bottomFloor = null;
	}

	public void InitializeHeightChangingMode() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);

		Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
		Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

		topFloor = cityBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor + 1, greenOverlay);
		bottomFloor = cityBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor, redOverlay);

		topFloor.AddComponent<StageColorController>();
		bottomFloor.AddComponent<StageColorController>();

		topFloorColorController = topFloor.GetComponent<StageColorController>();
		bottomFloorColorController = bottomFloor.GetComponent<StageColorController>();

		topFloorColorController.TargetMaterial = greenOverlay;
		bottomFloorColorController.TargetMaterial = redOverlay;

		topFloorColorController.ColorHue = 144;
		bottomFloorColorController.ColorHue = 0;

		topFloorColorController.StartCoroutine( topFloorColorController.Animate() );
		bottomFloorColorController.StartCoroutine( bottomFloorColorController.Animate() );

		selectedBuildingStartHeight = nodeGroup.NbFloor;
	}

	public int DesiredDirection(GameObject clickedWall) {
		int i = 0;
		for (; i < topFloor.transform.childCount && clickedWall != topFloor.transform.GetChild(i).gameObject; i++) ;

		if (i < topFloor.transform.childCount) {
			return 1;
		} else {
			int j = 0;
			for (; j < bottomFloor.transform.childCount && clickedWall != bottomFloor.transform.GetChild(j).gameObject; j++) ;

			if (j < bottomFloor.transform.childCount)
				return -1;
			else
				return 0;
		}
	}

	public void IncrementObjectHeight() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		cityBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor + 1);
		nodeGroup.NbFloor++;

		this.ShiftFloor(1);
		this.UpdateCameraPosition();
	}

	public void DecrementObjectHeight() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		cityBuilder.RebuildBuilding(selectedBuilding, nodeGroup.NbFloor - 1);
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
		cityBuilder.RebuildBuilding(selectedBuilding, selectedBuildingStartHeight);

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
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

	public StageColorController TopFloorColorController {
		get { return topFloorColorController; }
		set { topFloorColorController = value; }
	}

	public StageColorController BottomFloorColorController {
		get { return bottomFloorColorController; }
		set { bottomFloorColorController = value; }
	}
}
