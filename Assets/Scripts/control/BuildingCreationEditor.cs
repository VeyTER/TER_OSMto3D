using UnityEngine;
using UnityEditor;

public class BuildingCreationEditor : ObjectEditor {
	private CameraController cameraController;
	private CityBuilder cityBuilder;

	public BuildingCreationEditor() {
		this.cameraController = Camera.main.GetComponent<CameraController>();
		this.cityBuilder = CityBuilder.GetInstance();
	}

	public void InitializeBuildingCreation() {
		cameraController.SwitchToFullLocalMode();

		Vector3 screenPointPosition = new Vector3(Screen.width / 2F, Screen.height / 2F, 0);
		Vector3 buildingPosition = Camera.main.ScreenToWorldPoint(screenPointPosition);

		string newBuildingId = this.NewIdOrReference(10);
		while (buildingTools.IsInfoAttributeValueUsed(XmlAttributes.ID, newBuildingId))
			newBuildingId = this.NewIdOrReference(10);

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

		selectedBuilding = building;

		Debug.Log("ok");
	}

	private Node NewWallNode(int index, Vector3 buildingCenter, Vector2 localPosition) {
		string newNodeReference = this.NewIdOrReference(10);
		while (buildingTools.IsInfoAttributeValueUsed(XmlAttributes.REFERENCE, newNodeReference))
			newNodeReference = this.NewIdOrReference(10);
		return new Node(newNodeReference, index, buildingCenter.z + localPosition.x, buildingCenter.x + localPosition.y);
	}

	private string NewIdOrReference(int nbDigits) {
		string res = "+";
		System.Random randomGenerator = new System.Random();
		for (int i = 0; i < nbDigits; i++)
			res += System.Math.Floor((double) randomGenerator.Next(0, 10));
		return res;
	}

	public void UpdateBuildingSituation() {
		Vector3 screenPointPosition = new Vector3(Screen.width / 2F, Screen.height / 2F, 0);
		Vector3 buildingPosition = Camera.main.ScreenToWorldPoint(screenPointPosition);
	}

	public override void CancelTransform() {
		cameraController.SwitchToSemiLocalMode();
	}

	public override void ValidateTransform() {
		cameraController.SwitchToSemiLocalMode();
	}
}