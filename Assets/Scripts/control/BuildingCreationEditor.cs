using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class BuildingCreationEditor : ObjectEditor {
	private CameraController cameraController;

	public BuildingCreationEditor() {
		this.cameraController = Camera.main.GetComponent<CameraController>();
	}

	public void InitializeBuildingCreation() {
		float cameraAngle = (180 - (Camera.main.transform.rotation.eulerAngles.y + 90)) * Mathf.Deg2Rad;

		float REMOTNESS = 2;
		float cosOffset = (float)(Math.Cos(cameraAngle) * REMOTNESS);
		float sinOffset = (float)(Math.Sin(cameraAngle) * REMOTNESS);

		Vector3 buildingPosition = Camera.main.transform.position + new Vector3(cosOffset, 0, sinOffset);
		Vector2 buildingDimensions = Vector2.one;

		this.InitializeBuilding(buildingPosition, 0, buildingDimensions);
	}

	private void InitializeBuilding(Vector3 position, float orientation, Vector2 Dimensions) {
		Material defaultWallMaterial = Resources.Load(Materials.WALL_DEFAULT) as Material;
		NodeGroup nodeGroup = buildingsTools.NewBasicNodeGroup(position, Dimensions, defaultWallMaterial);

		GameObject building = cityBuilder.BuildSingleWallGroup(nodeGroup);
		buildingsTools.ReplaceColor(building, new Color(1, 1, 1, 0.5F));

		cityBuilder.AddNodeGroup(nodeGroup);
		cityBuilder.SetupSingleWallGroup(building, nodeGroup);

		building.transform.rotation = Quaternion.Euler(0, orientation, 0);

		building.transform.parent = Camera.main.transform;
		GameObject.Destroy(building.GetComponent<UiManager>());

		buildingsTools.AddBuildingAndNodeGroupPair(building, nodeGroup);

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

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		lengthdInputTextInput.text = (nodeGroup.Nodes[1].Latitude - nodeGroup.Nodes[0].Latitude).ToString();
		widthInputTextInput.text = (nodeGroup.Nodes[3].Longitude - nodeGroup.Nodes[0].Longitude).ToString();
	}

	public override void CancelTransform() {
		this.RemoveBuilding();
		cameraController.SwitchToSemiLocalMode();
	}

	public override void ValidateTransform() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		GameObject buildingNodeGroup = cityBuilder.BuildSingleBuildingNodeGroup(nodeGroup);

		selectedBuilding.AddComponent<UiManager>();
		selectedBuilding.transform.parent = cityBuilder.WallGroups.transform;
		buildingNodeGroup.transform.parent = cityBuilder.BuildingNodes.transform;

		Vector3 buildingPosition = selectedBuilding.transform.position;
		buildingNodeGroup.transform.position = new Vector3(buildingPosition.x, buildingNodeGroup.transform.position.y, buildingPosition.z);
		buildingNodeGroup.transform.rotation = selectedBuilding.transform.rotation;
		buildingsTools.UpdateNodesPosition(selectedBuilding);

		Material defaultBuildingMaterial = Resources.Load(Materials.WALL_DEFAULT) as Material;
		buildingsTools.ReplaceMaterial(selectedBuilding, defaultBuildingMaterial);

		buildingsTools.AppendResumeBuilding(nodeGroup);
		buildingsTools.AppendCustomBuilding(nodeGroup);

		buildingsTools.UpdateLocation(selectedBuilding);

		cameraController.SwitchToSemiLocalMode();
	}

	private void RemoveBuilding() {
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		cityBuilder.RemoveNodeGroup(nodeGroup);

		GameObject.Destroy(selectedBuilding);

		nodeGroup = null;
		selectedBuilding = null;
	}
}