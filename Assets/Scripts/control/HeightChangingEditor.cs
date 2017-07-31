﻿using System.Collections.Generic;
using UnityEngine;

public class HeightChangingEditor : ObjectEditor {
	public const int ROOF_INDEX = 0;

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

	public override void InitializeMode() {
		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);

		Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
		Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

		Triangulation triangulation = new Triangulation(buildingNodeGroup);
		triangulation.Triangulate();

		topFloor = cityBuilder.BuildVirtualLevel(selectedBuilding, buildingNodeGroup, triangulation, buildingNodeGroup.NbFloor + 1, greenOverlay, true);
		bottomFloor = cityBuilder.BuildVirtualLevel(selectedBuilding, buildingNodeGroup, triangulation, buildingNodeGroup.NbFloor, redOverlay, false);

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

		selectedBuildingStartHeight = buildingNodeGroup.NbFloor;
	}

	public int DesiredDirection(GameObject clickedBuildingPart) {
		if (clickedBuildingPart == topFloor || clickedBuildingPart == topFloor.transform.GetChild(ROOF_INDEX).gameObject)
			return 1;
		else if (clickedBuildingPart == bottomFloor)
			return -1;
		else
			return 0;
	}

	public void IncrementObjectHeight() {
		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ChangeBuildingHeight(selectedBuilding, buildingNodeGroup.NbFloor + 1);
		buildingNodeGroup.NbFloor++;

		this.ShiftVirtualFloors(1, buildingNodeGroup);
		this.UpdateBuildingWallsTexture(buildingNodeGroup);
		this.UpdateCameraPosition();
	}

	public void DecrementObjectHeight() {
		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ChangeBuildingHeight(selectedBuilding, buildingNodeGroup.NbFloor - 1);
		buildingNodeGroup.NbFloor--;

		this.ShiftVirtualFloors(-1, buildingNodeGroup);
		this.UpdateBuildingWallsTexture(buildingNodeGroup);
		this.UpdateCameraPosition();
	}

	private void ShiftVirtualFloors(int direction, BuildingNodeGroup buildingNodeGroup) {
		int expansionDirection = direction > 0 ? 1 : -1;

		Vector3 topFloorPosition = topFloor.transform.position;
		topFloor.transform.position = new Vector3(topFloorPosition.x, topFloorPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), topFloorPosition.z);

		Vector3 bottomFloorPosition = bottomFloor.transform.position;
		bottomFloor.transform.position = new Vector3(bottomFloorPosition.x, bottomFloorPosition.y + (expansionDirection * Dimensions.FLOOR_HEIGHT), bottomFloorPosition.z);
	}

	private void UpdateBuildingWallsTexture(BuildingNodeGroup buildingNodeGroup) {
		GameObject building = buildingsTools.NodeGroupToBuilding(buildingNodeGroup);
		GameObject buildingWalls = building.transform.GetChild(CityBuilder.WALLS_INDEX).gameObject;

		MeshFilter wallsMeshFilter = buildingWalls.GetComponent<MeshFilter>();

		List<Vector2> wallsUvs = new List<Vector2>();
		wallsMeshFilter.mesh.GetUVs(0, wallsUvs);
		for (int i = 0; i < wallsUvs.Count; i++)
			wallsUvs[i] = new Vector2(wallsUvs[i].x, (i % 2) * buildingNodeGroup.NbFloor);
		wallsMeshFilter.mesh.SetUVs(0, wallsUvs);
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

		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingNodeGroup.NbFloor = selectedBuildingStartHeight;
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
