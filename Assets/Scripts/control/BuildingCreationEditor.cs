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
		float REMOTNESS = 2;
		float cosOffset = (float)(Math.Cos(Camera.main.transform.rotation.y) * REMOTNESS);
		float sinOffset = (float)(Math.Sin(Camera.main.transform.rotation.y) * REMOTNESS);

		Vector3 buildingPosition = Camera.main.transform.position + new Vector3(cosOffset, 0, sinOffset);
		Vector2 buildingDimensions = Vector2.one;

		this.InitializeBuilding(buildingPosition, 0, buildingDimensions);
	}

	private void InitializeBuilding(Vector3 position, float orientation, Vector2 Dimensions) {
		NodeGroup nodeGroup = buildingTools.NewBasicNodeGroup(position, Dimensions);

		GameObject building = cityBuilder.BuildSingleWallGroup(nodeGroup);
		buildingTools.ReplaceColor(building, new Color(1, 1, 1, 0.5F));

		cityBuilder.SetupSingleWallGroup(building, nodeGroup);

		building.transform.rotation = Quaternion.Euler(0, orientation, 0);

		building.transform.parent = Camera.main.transform;
		GameObject.Destroy(building.GetComponent<UiManager>());

		buildingTools.AddBuildingToNodeGroupEntry(building, nodeGroup);
		buildingTools.AddNodeGroupToBuildingEntry(nodeGroup, building);

		selectedBuilding = building;
	}

	public void CompensateCameraMoves() {
		Vector3 buildingPosition = selectedBuilding.transform.position;

		GameObject buildingCreationOrientationInput = GameObject.Find(UiNames.BUILDING_CREATION_ORIENTATION_INPUT);
		InputField orientationInputTextInput = buildingCreationOrientationInput.GetComponent<InputField>();

		float buildingOrientation = 0;
		float.TryParse(orientationInputTextInput.text, out buildingOrientation);

		Transform firstWallTransform = selectedBuilding.transform.GetChild(0);
		float buildingHeight = firstWallTransform.localScale.y;

		selectedBuilding.transform.position = new Vector3(buildingPosition.x, buildingHeight / 2F, buildingPosition.z);
		selectedBuilding.transform.rotation = Quaternion.Euler(0, (float)Math.Round(buildingOrientation, 2), 0);
	}

	public void UpdatePosition(Vector3 newPosition) {
		Vector3 delta = newPosition - selectedBuilding.transform.position;
		Camera.main.transform.position = Camera.main.transform.position + delta;
	}

	public void UpdateOrientation(float newOrientation) {
		Quaternion buildingRotation = selectedBuilding.transform.rotation;
		selectedBuilding.transform.rotation = Quaternion.Euler(buildingRotation.eulerAngles.x, newOrientation, buildingRotation.eulerAngles.z);
	}

	public void UpdateDimensions(Vector2 newDimensions) {
		Transform selectedBuilingTransform = selectedBuilding.transform;
		this.RemoveBuilding();
		this.InitializeBuilding(selectedBuilingTransform.position, selectedBuilingTransform.rotation.eulerAngles.y, newDimensions);
	}

	public void UpdateDisplayedSituation() {
		GameObject buildingCreationXCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_X_COORD_INPUT);
		GameObject buildingCreationZCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_Z_COORD_INPUT);
		GameObject buildingCreationOrientationInput = GameObject.Find(UiNames.BUILDING_CREATION_ORIENTATION_INPUT);
		GameObject buildingCreationLengthInput = GameObject.Find(UiNames.BUILDING_CREATION_LENGTH_INPUT);
		GameObject buildingCreationWidthInput = GameObject.Find(UiNames.BUILDING_CREATION_WIDTH_INPUT);

		InputField xCoordInputTextInput = buildingCreationXCoordInput.GetComponent<InputField>();
		InputField zCoordInputTextInput = buildingCreationZCoordInput.GetComponent<InputField>();
		InputField orientationInputTextInput = buildingCreationOrientationInput.GetComponent<InputField>();
		InputField lengthdInputTextInput = buildingCreationLengthInput.GetComponent<InputField>();
		InputField widthInputTextInput = buildingCreationWidthInput.GetComponent<InputField>();

		xCoordInputTextInput.text = selectedBuilding.transform.position.x.ToString();
		zCoordInputTextInput.text = selectedBuilding.transform.position.z.ToString();
		orientationInputTextInput.text = Math.Round(selectedBuilding.transform.rotation.eulerAngles.y, 2).ToString();

		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);
		lengthdInputTextInput.text = (nodeGroup.Nodes[1].Latitude - nodeGroup.Nodes[0].Latitude).ToString();
		widthInputTextInput.text = (nodeGroup.Nodes[3].Longitude - nodeGroup.Nodes[0].Longitude).ToString();
	}

	public override void CancelTransform() {
		this.RemoveBuilding();
		cameraController.SwitchToSemiLocalMode();
	}

	public override void ValidateTransform() {
		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);
		GameObject buildingNodeGroup = cityBuilder.BuildSingleBuildingNodeGroup(nodeGroup);

		selectedBuilding.AddComponent<UiManager>();
		selectedBuilding.transform.parent = cityBuilder.WallGroups.transform;
		buildingNodeGroup.transform.parent = cityBuilder.BuildingNodes.transform;

		Material defaultBuildingMaterial = Resources.Load(Materials.WALL_DEFAULT) as Material;
		buildingTools.ReplaceMaterial(selectedBuilding, defaultBuildingMaterial);

		cameraController.SwitchToSemiLocalMode();
	}

	private void RemoveBuilding() {
		NodeGroup nodeGroup = buildingTools.BuildingToNodeGroup(selectedBuilding);

		GameObject.Destroy(selectedBuilding);

		nodeGroup = null;
		selectedBuilding = null;
	}
}