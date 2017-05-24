using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 	Gère l'intératction de l'utilisateur avec l'interface et les éléments 3D de la scène. Il y a une instance de
/// 	cette par objet qui intéragit avec elle, les gestion du dréoulement des actions a donc été déléguée à une ou
/// 	plusieurs classes n'instanciant qu'un seul objet. Une fois les évènements interceptés, l'action requise est donc
/// 	effectuée sur une classe de contrôle adaptée.
/// </summary>
public class UiManager : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	/// <summary>
	/// 	Controlleur gérant l'enchainement des modifications d'un objet (déplacement de 5 bâtiments par ex).
	/// </summary>
	public static EditionController editionController;


	/// <summary>Controlleur gérant le déplacement d'un seul objet (déplacement d'un bâtiment par ex).</summary>
	private static MovingEditor movingEditor;

	/// <summary>Controlleur gérant la rotation d'un seul objet (déplacement d'un bâtiment par ex).</summary>
	private static TurningEditor turningEditor;

	private static HeightChangingEditor heightChangingEditor;

	/// <summary>
	/// 	Unique instance du singleton ObjectBuilder servant construire la ville en 3D à partir des données OSM.
	/// </summary>
	private ObjectBuilder objectBuilder;


	public UiManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public void Start() {
		// Initialisation des "transformateurs" d'objet s'ils sont null
		if(editionController != null && (movingEditor == null || turningEditor == null || heightChangingEditor == null)) {
			movingEditor = editionController.MovingEditor;
			turningEditor = editionController.TurningEditor;
			heightChangingEditor = editionController.HeightChangingEditor;
		}
	}

	public void Update() {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			// Décalage de la caméra et mise à jour de la position de l'objet sélectionné si on est en cours de
			// déplacement d'un objet
			if (Input.GetMouseButton (0) && editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editionController.SelectionRange);
				movingEditor.ShiftCamera ();
			}
			break;
		}
	}


	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur entre un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnValueChanged(InputField originInputFiled) {
		// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
		if(!editionController.SelectedBuilding.name.Equals(originInputFiled.text))
			editionController.RenameBuilding (editionController.SelectedBuilding, originInputFiled.text);
	}

	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur a terminé d'entrer un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnEndChanged(InputField originInputFiled) {
		// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
		if(!editionController.SelectedBuilding.name.Equals(originInputFiled.text))
			editionController.RenameBuilding (editionController.SelectedBuilding, originInputFiled.text);
	}


	/// <summary>
	/// 	Trigger se déclenchant lorsque l'utilisateur commence à manipuler une poignée (déplacement ou rotation à
	/// 	priori).
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnBeginDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			// Démarrage du déplacement si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMotionless()) {
				movingEditor.StartObjectMoving (editionController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			// Démarrage de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsMotionless()) {
				turningEditor.StartObjectTurning (editionController.SelectionRange);
			}
			break;
		}
	}


	/// <summary>
	/// 	Trigger se déclenchant lorsque l'utilisateur déplace la souris tout en tenant une poignée (en déplacement ou
	/// 	en rotation à priori).
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			// Mise à jour du déplacement si l'objet est immobile et que le controlleur de modification se trouve dans
			// le bon état
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editionController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			// Mise à jour de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans
			// le bon état
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.UpdateObjectTurning (editionController.SelectionRange);
			}
			break;
		}
	}


	/// <summary>
	/// 	Trigger se déclenchant lorsque l'utilisateur a terminé de manimuler une poignée (déplacement ou rotation à
	/// 	priori).
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnEndDrag (PointerEventData eventData) {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			// Fin du déplacement si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editionController.EditionState == EditionController.EditionStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.EndObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			// Fin de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editionController.EditionState == EditionController.EditionStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.EndObjectTurning ();
			}
			break;
		}
	}


	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur l'objet sélectionné.
	/// </summary>
	public void OnMouseUp () {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()) {
			if (editionController.EditionState == EditionController.EditionStates.NONE_SELECTION || editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.SwitchBuilding(gameObject);
			} else if(editionController.EditionState == EditionController.EditionStates.HEIGHT_CHANGING_MODE) {
				int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
				if (expansionDirection > 0)
					heightChangingEditor.IncrementObjectHeight();
				else if (expansionDirection < 0)
					heightChangingEditor.DecrementObjectHeight();

			}
		}
	}


	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur un élement
	/// 	d'interface.
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnPointerUp (PointerEventData eventData) {
		switch (name) {
		// ==== Gestion des boutons controllant la visibilité des objets ====
		case UiNames.BUILDING_NODES_BUTTON:
			this.ToggleBuildingNodesVisibility ();
			break;
		case UiNames.HIGHWAY_NODES_BUTTON:
			this.ToggleHighwayNodesVisibility ();
			break;
		case UiNames.WALLS_BUTTON:
			this.ToggleWallsVisibility ();
			break;
		case UiNames.ROOFS_BUTTON:
			this.ToggleRoofsVisibility ();
			break;
		case UiNames.HIGHWAYS_BUTTON:
			this.ToggleHighwaysVisibility ();
			break;
		case UiNames.FOOTWAYS_BUTTON:
			this.ToggleFootwaysVisibility ();
			break;
		case UiNames.CYCLEWAYS_BUTTON:
			this.ToggleCyclewaysVisibility ();
			break;
		case UiNames.TREES_BUTTON:
			this.ToggleTreesVisibility ();
			break;

		// ==== Gestion des élément d'interface en rapport avec la modification d'objets ====
		case UiNames.BUILDING_NAME_TEXT_INPUT:
			
			break;
		case UiNames.TEMPERATURE_INDICATOR_TEXT_INPUT:
			
			break;
		case UiNames.HUMIDITY_INDICATOR_TEXT_INPUT:
			
			break;
		case UiNames.MOVE_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterMovingMode ();
			}
			break;
		case UiNames.TURN_BUTTON:
			// Préparation de la rotation d'un objet si le controlleur est prêt
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterTurningMode ();
			}
			break;
		case UiNames.CHANGE_HEIGHT_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterHeightChangingMode();
			}
			break;
		case UiNames.CHANGE_COLOR_BUTTON:
			
			break;
		case UiNames.SLIDE_PANEL_BUTTON:
			// Inversion du panneau latéral lors du clic sur le bouton correspondant si le controlleur n'est pas en
			// attente d'une sélection de bâtiment
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				if (editionController.PanelState == EditionController.PanelStates.CLOSED)
					editionController.TogglePanel (null);
				else if (editionController.PanelState == EditionController.PanelStates.OPEN)
					editionController.TogglePanel (null);
			}
			break;
		case UiNames.VALIDATE_BUTTON:
			// Validation de la modification d'une série de bâtiments si le controlleur est prêt
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.ValidateEdit ();
				editionController.ExitBuilding ();
			}
			break;
		case UiNames.CANCEL_BUTTON:
			// Annulation de la modification d'une série de bâtiments si le controlleur est prêt
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.CancelEdit ();
				editionController.ExitBuilding ();
			}
			break;
		case UiNames.WALL_RANGE_BUTTON:
			// Inversion de l'étendue de sélection et du statuts des boutons de d'étende de sélection si le controlleur
			// de modification n'est pas en attente d'une sélection de bâtiment et si le bouton est bien actif
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.WALL;
				if(editionController.EditionState == EditionController.EditionStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editionController.SelectionRange);
				else if(editionController.EditionState == EditionController.EditionStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editionController.SelectionRange);
				this.EnableWallRangeButton ();
			}
			break;
		case UiNames.BUILDING_RANGE_BUTTON:
			// Inversion de l'étendue de sélection et du statuts des boutons de d'étende de sélection si le controlleur
			// de modification n'est pas en attente d'une sélection de bâtiment et si le bouton est bien actif
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				editionController.SelectionRange = EditionController.SelectionRanges.BUILDING;
				if(editionController.EditionState == EditionController.EditionStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editionController.SelectionRange);
				else if(editionController.EditionState == EditionController.EditionStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editionController.SelectionRange);
				this.EnableBuildingRangeButton ();
			}
			break;
		case UiNames.VALDIATE_EDITION_BUTTON:
			// Validation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editionController.Transforming ()) {
				editionController.ValidateTransform ();
				editionController.ExitTransformMode ();
			}
			break;
		case UiNames.CANCEL_EDITION_BUTTON:
			// Annulation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editionController.Transforming ()) {
				editionController.CancelTransform ();
				editionController.ExitTransformMode ();
			}
			break;
		}
	}

	/// <summary>
	/// 	Inverse la visibilité des nodes 3D de bâtiments.
	/// </summary>
	public void ToggleBuildingNodesVisibility() {
		GameObject buildingNodes = objectBuilder.BuildingNodes;
		buildingNodes.SetActive (!buildingNodes.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des nodes 3D de bâtiments.
	/// </summary>
	public void ToggleHighwayNodesVisibility() {
		GameObject highwayNodes = objectBuilder.HighwayNodes;
		highwayNodes.SetActive (!highwayNodes.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des nodes 3D de routes.
	/// </summary>
	public void ToggleWallsVisibility() {
		GameObject walls = objectBuilder.WallGroups;
		walls.SetActive (!walls.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des nodes 3D de toits.
	/// </summary>
	public void ToggleRoofsVisibility() {
		GameObject roofs = objectBuilder.Roofs;
		roofs.SetActive (!roofs.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des routes.
	/// </summary>
	public void ToggleHighwaysVisibility() {
		GameObject highways = objectBuilder.Highways;
		highways.SetActive (!highways.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des chemins pietons.
	/// </summary>
	public void ToggleFootwaysVisibility() {
		GameObject footways = objectBuilder.Footways;
		footways.SetActive (!footways.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des pistes cyclables.
	/// </summary>
	public void ToggleCyclewaysVisibility() {
		GameObject cycleways = objectBuilder.Cycleways;
		cycleways.SetActive (!cycleways.activeInHierarchy);
	}

	/// <summary>
	/// 	Inverse la visibilité des arbres.
	/// </summary>
	public void ToggleTreesVisibility() {
		GameObject trees = objectBuilder.Trees;
		trees.SetActive (!trees.activeInHierarchy);
	}

	/// <summary>
	/// 	Met le bouton d'étendude de sélection correspondant aux murs en surbrillance.
	/// </summary>
	public void EnableWallRangeButton() {
		GameObject buildingRangeButton = GameObject.Find (UiNames.BUILDING_RANGE_BUTTON);

		Button buildingButtonComponent = buildingRangeButton.GetComponent<Button> ();
		Button wallButtonComponent = GetComponent<Button> ();

		// Verrouillage du bouton des murs et déverrouillage du boutons des bâtiments
		buildingButtonComponent.interactable = true;
		wallButtonComponent.interactable = false;
	}

	/// <summary>
	/// 	Met le bouton d'étendude de sélection correspondant aux bâtiments en surbrillance.
	/// </summary>
	public void EnableBuildingRangeButton() {
		GameObject wallRangeButton = GameObject.Find (UiNames.WALL_RANGE_BUTTON);

		Button wallButtonComponent = wallRangeButton.GetComponent<Button> ();
		Button buildingButtonComponent = GetComponent<Button> ();

		// Verrouillage du bouton des bâtiments et déverrouillage du boutons des murs
		wallButtonComponent.interactable = true;
		buildingButtonComponent.interactable = false;
	}
}