using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

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
	private static SkinChangingEditor skinChangingEditor;

	public static WheelPanelController visibilityPanelController;

	/// <summary>
	/// 	Unique instance du singleton CityBuilder servant construire la ville en 3D à partir des données OSM.
	/// </summary>
	private CityBuilder cityBuilder;

	public UiManager() {
		this.cityBuilder = CityBuilder.GetInstance ();
	}

	public void Start() {
		// Initialisation des "transformateurs" d'objet s'ils sont null
		if(editionController != null && (movingEditor == null || turningEditor == null || heightChangingEditor == null || skinChangingEditor == null)) {
			movingEditor = editionController.MovingEditor;
			turningEditor = editionController.TurningEditor;
			heightChangingEditor = editionController.HeightChangingEditor;
			skinChangingEditor = editionController.SkinChangingEditor;
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

	public void OnMouseDown() {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if (editionController.EditionState == EditionController.EditionStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0) {
				heightChangingEditor.TopFloorColorController.SetPressed();
			} else if (expansionDirection < 0) {
				heightChangingEditor.BottomFloorColorController.SetPressed();
			}
		}
	}

	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur l'objet sélectionné.
	/// </summary>
	public void OnMouseUp() {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()) {
			if (editionController.EditionState == EditionController.EditionStates.NONE_SELECTION || editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.SwitchBuilding(gameObject);
			} else if (editionController.EditionState == EditionController.EditionStates.HEIGHT_CHANGING_MODE) {
				int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
				if (expansionDirection > 0) {
					heightChangingEditor.IncrementObjectHeight();
					heightChangingEditor.TopFloorColorController.SetHovered();
				} else if (expansionDirection < 0) {
					heightChangingEditor.DecrementObjectHeight();
					heightChangingEditor.BottomFloorColorController.SetHovered();
				}
			}
		}
	}

	public void OnMouseEnter() {
		 if (editionController.EditionState == EditionController.EditionStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0)
				heightChangingEditor.TopFloorColorController.SetHovered();
			else if (expansionDirection < 0)
				heightChangingEditor.BottomFloorColorController.SetHovered();

		}
	}

	private void OnMouseExit() {
		 if (editionController.EditionState == EditionController.EditionStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0)
				heightChangingEditor.TopFloorColorController.SetInactive();
			else if (expansionDirection < 0)
				heightChangingEditor.BottomFloorColorController.SetInactive();
		}
	}


	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur un élement
	/// 	d'interface.
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnPointerUp (PointerEventData eventData) {
		switch (name.Split('_')[0]) {
		// ==== Gestion des boutons controllant la visibilité des objets ====
		case UiNames.DISABLED_BUILDING_NODES_BUTTON:
			cityBuilder.BuildingNodes.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_HIGHWAY_NODES_BUTTON:
			cityBuilder.HighwayNodes.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_WALLS_BUTTON:
			cityBuilder.WallGroups.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_ROOFS_BUTTON:
			cityBuilder.Roofs.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_HIGHWAYS_BUTTON:
			cityBuilder.Highways.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_FOOTWAYS_BUTTON:
			cityBuilder.Footways.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_CYCLEWAYS_BUTTON:
			cityBuilder.Cycleways.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;
		case UiNames.DISABLED_TREES_BUTTON:
			cityBuilder.Trees.SetActive(true);
			visibilityPanelController.EnableButton(transform.parent.gameObject);
			break;

		case UiNames.ENABLED_BUILDING_NODES_BUTTON:
			cityBuilder.BuildingNodes.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_HIGHWAY_NODES_BUTTON:
			cityBuilder.HighwayNodes.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_WALLS_BUTTON:
			cityBuilder.WallGroups.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_ROOFS_BUTTON:
			cityBuilder.Roofs.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_HIGHWAYS_BUTTON:
			cityBuilder.Highways.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_FOOTWAYS_BUTTON:
			cityBuilder.Footways.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_CYCLEWAYS_BUTTON:
			cityBuilder.Cycleways.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;
		case UiNames.ENABLED_TREES_BUTTON:
			cityBuilder.Trees.SetActive(false);
			visibilityPanelController.DisableButton(transform.parent.gameObject);
			break;

		// ==== Gestion des élément d'interface en rapport avec la modification d'objets ====
		case UiNames.BUILDING_NAME_NPUT_FILED:
			
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
		case UiNames.CHANGE_SKIN_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editionController.EditionState == EditionController.EditionStates.READY_TO_EDIT) {
				editionController.EnterSkinChangingMode();
			}
			break;
		case UiNames.SLIDE_BUTTON:
			// Inversion du panneau latéral lors du clic sur le bouton correspondant si le controlleur n'est pas en
			// attente d'une sélection de bâtiment
			EditPanelController editPanelController = editionController.EditPanelController;
			if (editionController.EditionState != EditionController.EditionStates.NONE_SELECTION) {
				if (editPanelController.IsPanelClosed()) {
					editPanelController.OpenPanel(null);
					editPanelController.OpenSlideButton();
				} else if (editPanelController.IsPanelOpen()) {
					editPanelController.ClosePanel(null);
					editPanelController.CloseSlideButton();
				}
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
				this.ActivateWallRangeButton ();
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
				this.ActivateBuildingRangeButton ();
			}
			break;
		case UiNames.MATERIALS_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.SwitchPallet(gameObject);
				skinChangingEditor.SkinPanelController.SlideSliderRight();
			}
			break;
		case UiNames.COLORS_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.SwitchPallet(gameObject);
				skinChangingEditor.SkinPanelController.SlideSliderLeft();
			}
			break;
		case UiNames.MATERIAL_ITEM_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.UpdateMaterialItems(gameObject);
				skinChangingEditor.ChangeBuildingMaterial(gameObject);
			}
			break;
		case UiNames.COLOR_ITEM_BUTTON:
			if (editionController.EditionState == EditionController.EditionStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.UpdateColorItems(gameObject);
				skinChangingEditor.ChangeBuildingColor(gameObject);
			}
			break;
		case UiNames.VALIDIATE_EDITION_BUTTON:
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

	public void ActivateWallRangeButton() {
		GameObject buildingRangeButton = GameObject.Find (UiNames.BUILDING_RANGE_BUTTON);

		Button wallButtonComponent = GetComponent<Button> ();
		Button buildingButtonComponent = buildingRangeButton.GetComponent<Button> ();

		// Verrouillage du bouton des murs et déverrouillage du boutons des bâtiments
		wallButtonComponent.interactable = false;
		buildingButtonComponent.interactable = true;
	}

	public void ActivateBuildingRangeButton() {
		GameObject wallRangeButton = GameObject.Find (UiNames.WALL_RANGE_BUTTON);

		Button wallButtonComponent = wallRangeButton.GetComponent<Button> ();
		Button buildingButtonComponent = GetComponent<Button> ();

		// Verrouillage du bouton des bâtiments et déverrouillage du boutons des murs
		wallButtonComponent.interactable = true;
		buildingButtonComponent.interactable = false;
	}
}