using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class BuildingCreationEditor : ObjectEditor {
	private CameraController cameraController;
	private CityBuilder cityBuilder;

	public BuildingCreationEditor() {
		this.cameraController = Camera.main.GetComponent<CameraController>();
		this.cityBuilder = CityBuilder.GetInstance();
	}

	public void InitializeBuildingCreation() {
		//cameraController.StartCoroutine(cameraController.MoveToSituation(Camera.main.transform.position, Quaternion.Euler(45, Camera.main.transform.rotation.y, Camera.main.transform.rotation.z), null));
		//cameraController.RotationLock = new bool [] { true, true, false };

		float REMOTNESS = 2;
		float cosOffset = (float)(Math.Cos(Camera.main.transform.rotation.y) * REMOTNESS);
		float sinOffset = (float)(Math.Sin(Camera.main.transform.rotation.y) * REMOTNESS);

		Vector3 buildingPosition = Camera.main.transform.position + new Vector3(cosOffset, 0, sinOffset);
		NodeGroup buildingNodeGroup = buildingTools.NewBasicNodeGroup(buildingPosition, Vector2.one);

		GameObject building = cityBuilder.BuildSingleWallGroup(buildingNodeGroup);
		cityBuilder.SetupSingleWallGroup(building, buildingNodeGroup);

		building.transform.parent = Camera.main.transform;
		GameObject.Destroy(building.GetComponent<UiManager>());

		selectedBuilding = building;
	}

	public void CompensateCameraMoves() {
		Vector3 buildingPosition = selectedBuilding.transform.position;
		Quaternion buildingRotation = selectedBuilding.transform.rotation;

		Transform firstWallTransform = selectedBuilding.transform.GetChild(0);
		float buildingHeight = firstWallTransform.localScale.y;

		selectedBuilding.transform.position = new Vector3(buildingPosition.x, buildingHeight / 2F, buildingPosition.z);
		selectedBuilding.transform.rotation = Quaternion.Euler(0, buildingRotation.y, 0);
	}

	public void UpdateSituation(Vector3 newPosition) {
		Vector3 delta = newPosition - selectedBuilding.transform.position;
		Camera.main.transform.position = Camera.main.transform.position + delta;
	}

	public void UpdateSituation(Vector3 newPosition, float newOrientation) {
		this.UpdateSituation(newPosition);

		Quaternion buildingRotation = selectedBuilding.transform.rotation;
		selectedBuilding.transform.rotation = Quaternion.Euler(buildingRotation.x * Mathf.Rad2Deg, newOrientation, buildingRotation.z * Mathf.Rad2Deg);
	}

	public void UpdateSituation(Vector3 newPosition, float newOrientation, Vector2 newScale) {
		this.UpdateSituation(newPosition, newOrientation);

		NodeGroup buildingNodeGroup = buildingTools.NewBasicNodeGroup(selectedBuilding.transform.position, newScale);

		GameObject building = cityBuilder.BuildSingleWallGroup(buildingNodeGroup);
		cityBuilder.SetupSingleWallGroup(building, buildingNodeGroup);

		selectedBuilding = building;
	}

	public void UpdateDisplayedPosition() {
		GameObject buildingCreationXCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_X_COORD_INPUT);
		GameObject buildingCreationZCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_Z_COORD_INPUT);
		GameObject buildingCreationOrientationInput = GameObject.Find(UiNames.BUILDING_CREATION_ORIENTATION_INPUT);

		InputField xCoordInputTextInput = buildingCreationXCoordInput.GetComponent<InputField>();
		InputField zCoordInputTextInput = buildingCreationZCoordInput.GetComponent<InputField>();
		InputField orientationInputTextInput = buildingCreationOrientationInput.GetComponent<InputField>();

		xCoordInputTextInput.text = selectedBuilding.transform.position.x.ToString();
		zCoordInputTextInput.text = selectedBuilding.transform.position.z.ToString();
		orientationInputTextInput.text = selectedBuilding.transform.rotation.y.ToString();
	}

	public override void CancelTransform() {
		cameraController.SwitchToSemiLocalMode();
	}

	public override void ValidateTransform() {
		selectedBuilding.AddComponent<UiManager>();
		cameraController.SwitchToSemiLocalMode();
	}
}