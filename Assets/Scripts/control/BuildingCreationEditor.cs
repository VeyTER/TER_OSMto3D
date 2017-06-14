using UnityEngine;
using UnityEditor;
using System;

public class BuildingCreationEditor : ObjectEditor {

	public void InitializeBuildingCreation() {
		CameraController cameraController = Camera.main.GetComponent<CameraController>();
		//cameraController.
	}

	public override void CancelTransform() {
		throw new NotImplementedException();
	}

	public override void ValidateTransform() {
		throw new NotImplementedException();
	}
}