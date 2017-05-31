using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère la rotation de l'objet sélectionné lorsque l'utilisateur clique et laisse appuyer sur la poignée de
/// 	déplacement.
/// </summary>
public class TurningEditor : ObjectEditor {
	/// <summary>
	/// 	Etats de rotation de l'objet sélectionné. En effet, celui-ci peut-être soit immobile, soit en mouvement.
	/// </summary>
	public enum TurningStates { MOTIONLESS, TURNING }

	/// <summary>Etat courant de rotation de l'objet.</summary>
	private TurningStates turningState;


	/// <summary>Position de départ du mur sélectionné pour la rotation courante.</summary>
	protected float selectedWallStartAngle;

	/// <summary>Position de départ du bâtiment sélectionné pour la rotation courante.</summary>
	protected float selectedBuildingStartAngle;


	/// <summary>Poignée permettant à l'utilisateur de choisir la nouvelle orientation de l'objet sélectionné.</summary>
	private GameObject turnHandler;

	/// <summary>Décalage angulaire de la poignée avec la souris lors de la prise en main de la poignée.</summary>
	private float turnHandlerStartOffset;


	public TurningEditor (GameObject turnHandler) {
		this.turningState = TurningStates.MOTIONLESS;

		this.selectedBuildingStartAngle = 0F;
		this.selectedWallStartAngle = 0F;

		this.turnHandler = turnHandler;
		this.turnHandler.SetActive (false);
	}


	// <summary>
	/// 	Initialise le mode de déplacement en définissant l'orientation de départ de l'objet sélectionné pour la
	/// 	rotation courante.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void InitializeTurningMode(EditionController.SelectionRanges selectionRange) {
		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		Vector3 objectScreenPosition = Vector3.zero;

		// Affectation de l'orientation courante de l'objet à l'orientation initiale
		if (selectionRange == EditionController.SelectionRanges.WALL) {
			objectScreenPosition = Camera.main.WorldToScreenPoint (selectedWall.transform.position);
			selectedWallStartAngle = selectedWall.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z, 360 - selectedWall.transform.rotation.eulerAngles.y); 
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			objectScreenPosition = Camera.main.WorldToScreenPoint (selectedBuilding.transform.position);
			selectedBuildingStartAngle = selectedBuilding.transform.rotation.eulerAngles.y;
			turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z,  360 - selectedBuilding.transform.rotation.eulerAngles.y); 
		}
	}


	/// <summary>
	/// 	Démarre la rotation de l'objet sélectionné en considérant, selon les cas, un mur ou un bâtiment comme
	/// 	sélectionné.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void StartObjectTurning(EditionController.SelectionRanges selectionRange) {
		Vector3 turnHandlerStartPosition = turnHandler.transform.position;
		float turnHandlerStartAngle = turnHandler.transform.rotation.eulerAngles.z;

		// Calcul du décalage entre la la poignée et la souris
		Vector2 relativeMousePosition = new Vector2(turnHandlerStartPosition.x, turnHandlerStartPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI)) * Mathf.Rad2Deg;
		turnHandlerStartOffset = mouseAngle - turnHandlerStartAngle;

		// Mise à jour des témoins de sélection
		if (selectionRange == EditionController.SelectionRanges.WALL)
			wallTransformed = true;
		else if (selectionRange == EditionController.SelectionRanges.BUILDING)
			buildingTransformed = true;

		// Rotation de l'objet à sa position initiale
		this.TurnObject (selectionRange);

		turningState = TurningStates.TURNING;
	}


	/// <summary>
	/// 	Met à jour l'orientation de l'objet sélectionné en fonction de l'emplacement de la souris.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void UpdateObjectTurning(EditionController.SelectionRanges selectionRange) {
		Vector3 turnHandlerPosition = turnHandler.transform.position;

		// Calcul de la nouvelle orientation de la poignée
		Vector2 relativeMousePosition = new Vector2(turnHandlerPosition.x, turnHandlerPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI)) * Mathf.Rad2Deg;
		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		turnHandler.transform.rotation = Quaternion.Euler(turnHandlerRotation.x, turnHandlerRotation.y, (mouseAngle - turnHandlerStartOffset));

		// Rotation de l'objet à sa nouvelle orientation
		this.TurnObject (selectionRange);
	}


	/// <summary>
	/// 	Oriente l'objet sélectionné selon l'angle de la poignée.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	private void TurnObject(EditionController.SelectionRanges selectionRange) {
		// Calcul de l'orientation à effecter à l'objet (le sens étant inversé)
		float turnHandlerAngle = 360 - turnHandler.transform.rotation.eulerAngles.z;

		// Affectation de l'orientation à l'objet
		if (selectionRange == EditionController.SelectionRanges.WALL) {
			Quaternion selectedWallRotation = selectedWall.transform.rotation;
			selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.x, turnHandlerAngle, selectedWallRotation.z);
		} else if (selectionRange == EditionController.SelectionRanges.BUILDING) {
			Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
			selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turnHandlerAngle, selectedBuildingRotation.z);
			selectedBuildingNodes.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turnHandlerAngle, selectedBuildingRotation.z);
		}
	}


	/// <summary>
	/// 	Termine la rotation  de l'objet sélectionné en indiquant l'état de rotation comme immobile.
	/// </summary>
	public void EndObjectTurning() {
		turningState = TurningStates.MOTIONLESS;
	}

	public override void ValidateTransform() {
		if (wallTransformed) {
			if (!transformedObjects.Contains(selectedWall))
				transformedObjects.Add(selectedWall);
		} else if (buildingTransformed) {
			if (!transformedObjects.Contains(selectedBuilding))
				transformedObjects.Add(selectedBuilding);

			buildingTools.UpdateNodesPosition(selectedBuilding);
		}
	}

	public override void CancelTransform() {
		if (wallTransformed) {
			Quaternion selectedWallRotation = selectedWall.transform.rotation;
			selectedWall.transform.rotation = Quaternion.Euler(selectedWallRotation.x, selectedWallStartAngle, selectedWallRotation.z);
		} else if (buildingTransformed) {
			Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
			selectedBuilding.transform.rotation = Quaternion.Euler(selectedBuildingRotation.x, selectedBuildingStartAngle, selectedBuildingRotation.z);

			float buildingAngle = selectedBuilding.transform.rotation.eulerAngles.y;
			Quaternion buildingNodesGroupRotation = selectedBuildingNodes.transform.rotation;

			selectedBuildingNodes.transform.rotation = Quaternion.Euler(buildingNodesGroupRotation.x, buildingAngle, buildingNodesGroupRotation.z);

			buildingTools.UpdateNodesPosition(selectedBuilding);
		}
	}

	/// <summary>
	/// 	Indique si l'objet est immobile.
	/// </summary>
	/// <returns><c>true</c> si l'objet est immobile; sinon, <c>false</c>.</returns>
	public bool IsMotionless() {
		return turningState == TurningStates.MOTIONLESS;
	}

	/// <summary>
	/// 	Indique si l'objet est en rotation.
	/// </summary>
	/// <returns><c>true</c> si l'objet est en rotation; sinon, <c>false</c>.</returns>
	public bool IsTurning() {
		return turningState == TurningStates.TURNING;
	}

	public TurningStates TurningState {
		get { return turningState; }
		set { turningState = value; }
	}

	public float SelectedBuildingStartAngle {
		get { return selectedBuildingStartAngle; }
		set { selectedBuildingStartAngle = value; }
	}

	public float SelectedWallStartAngle {
		get { return selectedWallStartAngle; }
		set { selectedWallStartAngle = value; }
	}

	public GameObject TurnHandler {
		get { return turnHandler; }
		set { turnHandler = value; }
	}
}
