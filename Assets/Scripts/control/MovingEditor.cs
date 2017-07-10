using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère le déplacement de l'objet sélectionné lorsque l'utilisateur clique et laisse appuyer sur la poignée de
/// 	déplacement.
/// </summary>
public class MovingEditor : ObjectEditor {
	/// <summary>
	/// 	Etats de déplacement de l'objet sélectionné. En effet, celui-ci peut-être soit immobile, soit en mouvement.
	/// </summary>
	public enum MovingStates { MOTIONLESS, MOVING }

	/// <summary>Etat courant de déplacement de l'objet.</summary>
	private MovingStates movingState;


	/// <summary>Position de départ du bâtiment sélectionné pour le déplacement courant.</summary>
	protected Vector3 selectedBuildingStartPos;

	
	/// <summary>Poignée permettant à l'utilisateur de choisir la nouvelle position de l'objet sélectionné.</summary>
	private GameObject moveHandler;

	/// <summary>Décalage en position de la poignée avec la souris lors de la prise en main de la poignée.</summary>
	private Vector2 moveHandlerStartOffset;


	public MovingEditor (GameObject moveHandler) {
		this.movingState = MovingStates.MOTIONLESS;

		this.selectedBuildingStartPos = Vector3.zero;

		this.moveHandler = moveHandler;
		this.moveHandler.SetActive (false);
	}


	/// <summary>
	/// 	Initialise le mode de déplacement en définissant la position de départ de l'objet sélectionné pour le
	/// 	déplacement courant.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void InitializeMovingMode() {
		Vector3 objectPosition = Vector3.zero;
		Vector3 objectScale = Vector3.zero;

		// Affectation de la position courante de l'objet à la position initiale
		objectPosition = selectedBuilding.transform.position;
		objectScale = selectedBuilding.transform.localScale;
		selectedBuildingStartPos = objectPosition;

		// Initialisation de la position de la poignée de déplacement
		Vector3 objectScreenPosition = Camera.main.WorldToScreenPoint (objectPosition);
		moveHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);
	}


	/// <summary>
	/// 	Démarre le déplacement de l'objet sélectionné en considérant, selon les cas, un mur ou un bâtiment comme
	/// 	sélectionné.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void StartObjectMoving() {
		Vector2 moveHandlerStartPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

		// Calcul du décalage entre la la poignée et la souris
		moveHandlerStartOffset = mousePosition - moveHandlerStartPosition;

		// Mise à jour des témoins de sélection
		buildingTransformed = true;

		// Déplacement de l'objet à sa position initiale
		this.MoveObject ();

		movingState = MovingStates.MOVING;
	}


	/// <summary>
	/// 	Met à jour la position de l'objet sélectionné en fonction de l'emplacement de la souris.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	public void UpdateObjectMoving() {
		// Calcul de la nouvelle position de la poignée
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerStartOffset;

		// Déplacement de l'objet à sa nouvelle position
		this.MoveObject ();
	}


	/// <summary>
	/// 	Déplace l'objet sélectionné à la position correspondante de la poignée dans le repère 3D.
	/// </summary>
	/// <param name="selectionRange">Etendue de la sélection (mur ou bâtiment).</param>
	private void MoveObject() {
		Camera mainCamera = Camera.main;
		Vector3 moveHandlerPosition = moveHandler.transform.position;

		// Déplace l'objet ainsi que les nodes noeuds 3D correspondant si l'objet est un bâtiment. Les murs ne sont en
		// effet pas pris en charge.
		Vector3 selectedObjectCurrentPos = mainCamera.ScreenToWorldPoint(new Vector3(moveHandlerPosition.x, moveHandlerPosition.y, mainCamera.transform.position.y));
		selectedBuilding.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedBuilding.transform.position.y, selectedObjectCurrentPos.z);
		selectedBuildingNodes.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedBuildingNodes.transform.position.y, selectedObjectCurrentPos.z);
	}


	/// <summary>
	/// 	Termine le déplacement de l'objet sélectionné en indiquant l'état de déplacement comme immobile.
	/// </summary>
	public void EndObjectMoving() {
		movingState = MovingStates.MOTIONLESS;
	}


	/// <summary>
	/// 	Décale la caméra lorsque le l'utilisateur est en train de déplacer un objet et que la souris se trouve
	/// 	trop proche des bors de l'écran. Cela permet de ne pas se cantonner à la vue courante.
	/// </summary>
	public void ShiftCamera() {
		GameObject wallsGroups = CityBuilder.GetInstance ().WallGroups;
		float cameraAngle = Mathf.Deg2Rad * wallsGroups.transform.rotation.eulerAngles.y;

		float cosOffset = 0.1F * (float)Math.Cos (cameraAngle);
		float sinOffset = 0.1F * (float)Math.Sin (cameraAngle);

		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		Camera mainCamera = Camera.main;
		Vector3 newCameraPosition = mainCamera.transform.localPosition;

		if (mousePosition.x > Screen.width * 0.8F)
			newCameraPosition = new Vector3 (newCameraPosition.x - sinOffset, newCameraPosition.y, newCameraPosition.z - cosOffset);
		else if (mousePosition.x < Screen.width * 0.2F)
			newCameraPosition = new Vector3 (newCameraPosition.x + sinOffset, newCameraPosition.y, newCameraPosition.z + cosOffset);

		if (mousePosition.y > Screen.height * 0.8F)
			newCameraPosition = new Vector3 (newCameraPosition.x + cosOffset, newCameraPosition.y, newCameraPosition.z - sinOffset);
		else if (mousePosition.y < Screen.height * 0.2F)
			newCameraPosition = new Vector3 (newCameraPosition.x - cosOffset, newCameraPosition.y, newCameraPosition.z + sinOffset);

		mainCamera.transform.localPosition = newCameraPosition;
	}

	public override void ValidateTransform() {
		if(buildingTransformed) {
			if (!transformedObjects.Contains(selectedBuilding))
				transformedObjects.Add(selectedBuilding);
			buildingsTools.UpdateNodesPosition(selectedBuilding);
		}
	}

	public override void CancelTransform() {
		if(buildingTransformed) {
			selectedBuilding.transform.position = selectedBuildingStartPos;

			Vector3 buildingPosition = selectedBuilding.transform.position;
			Vector3 buildingNodesGroupPosition = selectedBuildingNodes.transform.position;

			selectedBuildingNodes.transform.position = new Vector3(buildingPosition.x, buildingNodesGroupPosition.y, buildingPosition.z);

			buildingsTools.UpdateNodesPosition(selectedBuilding);
		}
	}

	/// <summary>
	/// 	Indique si l'objet est immobile.
	/// </summary>
	/// <returns><c>true</c> si l'objet est immobile; sinon, <c>false</c>.</returns>
	public bool IsMotionless() {
		return movingState == MovingStates.MOTIONLESS;
	}

	/// <summary>
	/// 	Indique si l'objet est en mouvement.
	/// </summary>
	/// <returns><c>true</c> si l'objet est en mouvement; sinon, <c>false</c>.</returns>
	public bool IsMoving() {
		return movingState == MovingStates.MOVING;
	}


	public MovingStates MovingState {
		get { return movingState; }
		set { movingState = value; }
	}

	public Vector3 SelectedBuildingStartPos {
		get { return selectedBuildingStartPos; }
		set { selectedBuildingStartPos = value; }
	}

	public GameObject MoveHandler {
		get { return moveHandler; }
		set { moveHandler = value; }
	}
}
