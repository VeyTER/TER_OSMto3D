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

		string newBuildingId = this.NewIdOrReference(10);
		//while (buildingTools.IsInfoAttributeValueUsed(XmlAttributes.ID, newBuildingId))
		//	newBuildingId = this.NewIdOrReference(10);

		NodeGroup buildingNodeGroup = new NodeGroup(newBuildingId) {
			Name = "Nouveau bâtiment",
			NbFloor = 3
		};

		Node topRightNode = this.NewWallNode(0, buildingPosition, new Vector2(-0.5F, -0.5F));
		Node bottomWallNode = this.NewWallNode(1, buildingPosition, new Vector2(0.5F, -0.5F));
		Node rightWallNode = this.NewWallNode(2, buildingPosition, new Vector2(0.5F, 0.5F));
		Node leftWallNode = this.NewWallNode(3, buildingPosition, new Vector2(-0.5F, 0.5F));

		buildingNodeGroup.AddNode(topRightNode);
		buildingNodeGroup.AddNode(bottomWallNode);
		buildingNodeGroup.AddNode(rightWallNode);
		buildingNodeGroup.AddNode(leftWallNode);

		GameObject building = cityBuilder.BuildSingleWallGroup(buildingNodeGroup);
		cityBuilder.SetupSingleWallGroup(building, buildingNodeGroup);

		building.transform.parent = Camera.main.transform;

		selectedBuilding = building;
	}

	private Node NewWallNode(int index, Vector3 buildingCenter, Vector2 localPosition) {
		string newNodeReference = this.NewIdOrReference(10);
		//while (buildingTools.IsInfoAttributeValueUsed(XmlAttributes.REFERENCE, newNodeReference))
		//	newNodeReference = this.NewIdOrReference(10);
		return new Node(newNodeReference, index, buildingCenter.z + localPosition.x, buildingCenter.x + localPosition.y);
	}

	private string NewIdOrReference(int nbDigits) {
		string res = "+";
		System.Random randomGenerator = new System.Random();
		for (int i = 0; i < nbDigits; i++)
			res += System.Math.Floor((double) randomGenerator.Next(0, 10));
		return res;
	}

	public void CompensateCameraMoves() {
		Vector3 buildingPosition = selectedBuilding.transform.position;
		Quaternion buildingRotation = selectedBuilding.transform.rotation;

		Transform firstWallTransform = selectedBuilding.transform.GetChild(0);
		float buildingHeight = firstWallTransform.localScale.y;

		selectedBuilding.transform.position = new Vector3(buildingPosition.x, buildingHeight / 2F, buildingPosition.z);
		selectedBuilding.transform.rotation = Quaternion.Euler(0, buildingRotation.y, 0);
	}

	public void UpdateSituation(Vector3 newPosition, float newOrientation) {
		Vector3 delta = newPosition - selectedBuilding.transform.position;
		Camera.main.transform.position = Camera.main.transform.position + delta;

		Quaternion buildingRotation = selectedBuilding.transform.rotation;
		selectedBuilding.transform.rotation = Quaternion.Euler(buildingRotation.x * Mathf.Rad2Deg, buildingRotation.y * Mathf.Rad2Deg, newOrientation);
	}

	public void UpdateDisplayedPosition() {
		GameObject buildingCreationXCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_X_COORD_INPUT);
		GameObject buildingCreationZCoordInput = GameObject.Find(UiNames.BUILDING_CREATION_Z_COORD_INPUT);

		InputField xCoordInputTextInput = buildingCreationXCoordInput.GetComponent<InputField>();
		InputField yCoordInputTextInput = buildingCreationZCoordInput.GetComponent<InputField>();

		xCoordInputTextInput.text = selectedBuilding.transform.position.x.ToString();
		yCoordInputTextInput.text = selectedBuilding.transform.position.z.ToString();
	}

	public override void CancelTransform() {
		cameraController.SwitchToSemiLocalMode();
	}

	public override void ValidateTransform() {
		cameraController.SwitchToSemiLocalMode();
	}
}