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

		topFloor = cityBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor + 1, greenOverlay, true);
		bottomFloor = cityBuilder.BuildVirtualFloor(selectedBuilding, nodeGroup.NbFloor, redOverlay, false);

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

	public int DesiredDirection(GameObject clickedBuildingPart) {
		if (clickedBuildingPart == topFloor)
			return 1;
		else if (clickedBuildingPart == bottomFloor)
			return -1;
		else
			return 0;
	}

	public void IncrementObjectHeight() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ChangeBuildingHeight(selectedBuilding, nodeGroup.NbFloor + 1);
		nodeGroup.NbFloor++;

		this.ShiftVirtualFloors(1);
		this.UpdateCameraPosition();
	}

	public void DecrementObjectHeight() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ChangeBuildingHeight(selectedBuilding, nodeGroup.NbFloor - 1);
		nodeGroup.NbFloor--;

		this.ShiftVirtualFloors(-1);
		this.UpdateCameraPosition();
	}

	private void ShiftVirtualFloors(int direction) {
		int expansionDirection = direction > 0 ? 1 : -1;

		Vector3 topFloorPosition = topFloor.transform.position;
		topFloor.transform.position = new Vector3(topFloorPosition.x, topFloorPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), topFloorPosition.z);

		Vector3 bottomFloorPosition = bottomFloor.transform.position;
		bottomFloor.transform.position = new Vector3(bottomFloorPosition.x, bottomFloorPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), bottomFloorPosition.z);
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
		buildingsTools.ChangeBuildingHeight(selectedBuilding, selectedBuildingStartHeight);

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
