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
	public static EditController editController;

	/// <summary>Controlleur gérant le déplacement d'un seul objet (déplacement d'un bâtiment par ex).</summary>
	private static MovingEditor movingEditor;

	/// <summary>Controlleur gérant la rotation d'un seul objet (déplacement d'un bâtiment par ex).</summary>
	private static TurningEditor turningEditor;

	private static HeightChangingEditor heightChangingEditor;
	private static SkinChangingEditor skinChangingEditor;

	private static ControlPanelManager controlPanelManager;

	private static BuildingCreationEditor buildingCreationEditor;

	/// <summary>
	/// 	Unique instance du singleton CityBuilder servant construire la ville en 3D à partir des données OSM.
	/// </summary>
	private CityBuilder cityBuilder;

	public UiManager() {
		this.cityBuilder = CityBuilder.GetInstance ();
	}

	public void Start() {
		// Initialisation des "transformateurs" d'objet s'ils sont null
		if (editController != null) {
			if (movingEditor == null)
				movingEditor = editController.MovingEditor;

			if (turningEditor == null)
				turningEditor = editController.TurningEditor;

			if (heightChangingEditor == null)
				heightChangingEditor = editController.HeightChangingEditor;

			if (skinChangingEditor == null)
				skinChangingEditor = editController.SkinChangingEditor;
		}

		if (buildingCreationEditor == null)
			buildingCreationEditor = new BuildingCreationEditor();

		if (controlPanelManager == null)
			controlPanelManager = ControlPanelManager.GetInstance();
	}

	public void Update() {
		switch (name) {
		case UiNames.MOVE_HANDLER:
			// Décalage de la caméra et mise à jour de la position de l'objet sélectionné si on est en cours de
			// déplacement d'un objet
			if (Input.GetMouseButton (0) && editController.EditState == EditController.EditStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editController.SelectionRange);
				movingEditor.ShiftCamera ();
			}
			break;
		case UiNames.CANCEL_BUILDING_CREATION_BUTTON:
			if (controlPanelManager.ControlState != ControlPanelManager.ControlStates.NONE) {
				if (this.IsPlayerMoving()) {
					buildingCreationEditor.CompensateCameraMoves();
					buildingCreationEditor.UpdateDisplayedPosition();
				}
			}
			break;
		}
	}

	private bool IsPlayerMoving() {
		return (Input.GetKey("up") || Input.GetKey(KeyCode.Z)
			 || Input.GetKey("down") || Input.GetKey(KeyCode.S)
			 || Input.GetKey("left") || Input.GetKey(KeyCode.Q)
			 || Input.GetKey("right") || Input.GetKey(KeyCode.D)
			 || Input.GetKey("up") || Input.GetKey(KeyCode.Z)
			 || Input.GetKey("down") || Input.GetKey(KeyCode.S));
	}

	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur entre un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnValueChanged(InputField originInputFiled) {
		switch (originInputFiled.name) {
		case UiNames.BUILDING_NAME_INPUT:
			// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
			if (!editController.SelectedBuilding.name.Equals(originInputFiled.text))
				editController.RenameBuilding(editController.SelectedBuilding, originInputFiled.text);
			break;
		case UiNames.BUILDING_CREATION_X_COORD_INPUT:
			Vector3 buildingPositionInXEdit = buildingCreationEditor.SelectedBuilding.transform.position;

			float newPosX = this.ProcessInputValue(originInputFiled);
			if (!float.IsNaN(newPosX))
				buildingCreationEditor.UpdateSituation(new Vector3(newPosX, buildingPositionInXEdit.y, buildingPositionInXEdit.z));

			break;
		case UiNames.BUILDING_CREATION_Z_COORD_INPUT:
			Vector3 buildingPositionInZEdit = buildingCreationEditor.SelectedBuilding.transform.position;

			float newPosZ = this.ProcessInputValue(originInputFiled);
			if(!float.IsNaN(newPosZ))
				buildingCreationEditor.UpdateSituation(new Vector3(buildingPositionInZEdit.x, buildingPositionInZEdit.y, newPosZ));

			break;
		case UiNames.BUILDING_CREATION_ORIENTATION_INPUT:
			Vector3 buildingPositionInOrientationEdit = buildingCreationEditor.SelectedBuilding.transform.position;
			Quaternion buildingRotationInOrientationEdit = buildingCreationEditor.SelectedBuilding.transform.rotation;

			float newOrientation = this.ProcessInputValue(originInputFiled);
			if (!float.IsNaN(newOrientation))
				buildingCreationEditor.UpdateSituation(new Vector3(buildingPositionInOrientationEdit.x, buildingPositionInOrientationEdit.y, buildingPositionInOrientationEdit.z), newOrientation);

			break;
		//case UiNames.BUILDING_CREATION_LENGTH_INPUT:
		//	Vector3 buildingPositionInZEdit = buildingCreationEditor.SelectedBuilding.transform.position;
		//	Quaternion buildingRotationInZEdit = buildingCreationEditor.SelectedBuilding.transform.rotation;

		//	float newPosZ = this.ProcessInputValue(originInputFiled);
		//	if (!float.IsNaN(newPosZ))
		//		buildingCreationEditor.UpdateSituation(new Vector3(buildingPositionInZEdit.x, buildingPositionInZEdit.y, newPosZ), buildingRotationInZEdit.z);

		//	break;
		//case UiNames.BUILDING_CREATION_WIDTH_INPUT:
		//	Vector3 buildingPositionInZEdit = buildingCreationEditor.SelectedBuilding.transform.position;
		//	Quaternion buildingRotationInZEdit = buildingCreationEditor.SelectedBuilding.transform.rotation;

		//	float newPosZ = this.ProcessInputValue(originInputFiled);
		//	if (!float.IsNaN(newPosZ))
		//		buildingCreationEditor.UpdateSituation(new Vector3(buildingPositionInZEdit.x, buildingPositionInZEdit.y, newPosZ), buildingRotationInZEdit.z);

		//	break;
		}
	}

	private float ProcessInputValue(InputField originInputFiled) {
		float parsedValue = 0;
		string inputValue = originInputFiled.text;

		if (inputValue.Length == 0)
			return 0;

		bool parsingSuccessinZEdit = float.TryParse(inputValue, out parsedValue);
		if (parsingSuccessinZEdit) {

			return parsedValue;
		} else {
			originInputFiled.selectionColor = ThemeColors.DARK_RED;
			return float.NaN;
		}
	}

	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur a terminé d'entrer un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnEndEdit(InputField originInputFiled) {
		switch (originInputFiled.name) {
		case UiNames.BUILDING_NAME_INPUT:
			// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
			if (!editController.SelectedBuilding.name.Equals(originInputFiled.text))
				editController.RenameBuilding(editController.SelectedBuilding, originInputFiled.text);
			break;
		}
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && movingEditor.IsMotionless()) {
				movingEditor.StartObjectMoving (editController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			// Démarrage de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && turningEditor.IsMotionless()) {
				turningEditor.StartObjectTurning (editController.SelectionRange);
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.UpdateObjectMoving (editController.SelectionRange);
			}
			break;
		case UiNames.TURN_HANDLER:
			// Mise à jour de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans
			// le bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.UpdateObjectTurning (editController.SelectionRange);
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && movingEditor.IsMoving()) {
				movingEditor.EndObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			// Fin de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && turningEditor.IsTurning()) {
				turningEditor.EndObjectTurning ();
			}
			break;
		}
	}

	public void OnMouseDown() {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
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
			if (editController.EditState == EditController.EditStates.NONE_SELECTION || editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.SwitchBuilding(gameObject);
			} else if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
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
		 if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = heightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0)
				heightChangingEditor.TopFloorColorController.SetHovered();
			else if (expansionDirection < 0)
				heightChangingEditor.BottomFloorColorController.SetHovered();

		}
	}

	private void OnMouseExit() {
		 if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
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
		PanelController panelController1 = controlPanelManager.GetPanelController(UiNames.VISIBILITY_WHEEL_PANEL);
		PanelController panelController2 = controlPanelManager.GetPanelController(UiNames.BUILDING_CREATION_BOX_PANEL);

		WheelPanelController visibilityPanelController = null;
		if(panelController1.GetType() == typeof(WheelPanelController))
			visibilityPanelController = (WheelPanelController) panelController1;

		BoxPanelController buildingCreationPanelController = null;
		if (panelController2.GetType() == typeof(BoxPanelController))
			buildingCreationPanelController = (BoxPanelController) panelController2;

		switch (name.Split('_')[0]) {
		case UiNames.CREATE_BUILDING_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.NONE) {
				buildingCreationPanelController.transform.gameObject.SetActive(true);
				buildingCreationPanelController.OpenPanel(null);
				buildingCreationEditor.InitializeBuildingCreation();
				buildingCreationEditor.UpdateDisplayedPosition();
				controlPanelManager.ControlState = ControlPanelManager.ControlStates.BUILDING_CREATION;
			}
			break;

		case UiNames.VALIDATE_BUILDING_CREATION_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.BUILDING_CREATION) {
				buildingCreationEditor.ValidateTransform();
				buildingCreationPanelController.ClosePanel(() => {
					buildingCreationPanelController.transform.gameObject.SetActive(false);
					controlPanelManager.ControlState = ControlPanelManager.ControlStates.NONE;
				});
			}
			break;

		case UiNames.CANCEL_BUILDING_CREATION_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.BUILDING_CREATION) {
				buildingCreationEditor.CancelTransform();
				buildingCreationPanelController.ClosePanel(() => {
					buildingCreationPanelController.transform.gameObject.SetActive(false);
					controlPanelManager.ControlState = ControlPanelManager.ControlStates.NONE;
				});
			}
			break;

		// ==== Gestion des boutons controllant la visibilité des objets ====
		case UiNames.TOGGLE_VISIBILITY_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.NONE) {
				visibilityPanelController.transform.gameObject.SetActive(true);
				visibilityPanelController.OpenPanel(null);
				controlPanelManager.ControlState = ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING;
			}
			break;
		case UiNames.CLOSE_VISIBILITY_WHEEL_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityPanelController.ClosePanel(() => {
					visibilityPanelController.transform.gameObject.SetActive(false);
					controlPanelManager.ControlState = ControlPanelManager.ControlStates.NONE;
				});
			}
			break;
		case UiNames.DISABLED_BUILDING_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.BuildingNodes.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_HIGHWAY_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.HighwayNodes.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_WALLS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.WallGroups.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_ROOFS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Roofs.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_HIGHWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Highways.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_FOOTWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Footways.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_CYCLEWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Cycleways.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_TREES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Trees.SetActive(true);
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;

		case UiNames.ENABLED_BUILDING_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.BuildingNodes.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_HIGHWAY_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.HighwayNodes.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_WALLS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.WallGroups.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_ROOFS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Roofs.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_HIGHWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Highways.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_FOOTWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Footways.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_CYCLEWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Cycleways.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_TREES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				cityBuilder.Trees.SetActive(false);
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;

		// ==== Gestion des élément d'interface en rapport avec la modification d'objets ====
		case UiNames.MOVE_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterMovingMode ();
			}
			break;
		case UiNames.TURN_BUTTON:
			// Préparation de la rotation d'un objet si le controlleur est prêt
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterTurningMode ();
			}
			break;
		case UiNames.CHANGE_HEIGHT_BUTTON:
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterHeightChangingMode();
			}
			break;
		case UiNames.CHANGE_SKIN_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterSkinChangingMode();
			}
			break;
		case UiNames.SLIDE_BUTTON:
			// Inversion du panneau latéral lors du clic sur le bouton correspondant si le controlleur n'est pas en
			// attente d'une sélection de bâtiment
			EditPanelController editPanelController = editController.EditPanelController;
			if (editController.EditState != EditController.EditStates.NONE_SELECTION) {
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
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.ValidateEdit ();
				editController.ExitBuilding ();
			}
			break;
		case UiNames.CANCEL_BUTTON:
			// Annulation de la modification d'une série de bâtiments si le controlleur est prêt
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.CancelEdit ();
				editController.ExitBuilding ();
			}
			break;
		case UiNames.WALL_RANGE_BUTTON:
			// Inversion de l'étendue de sélection et du statuts des boutons de d'étende de sélection si le controlleur
			// de modification n'est pas en attente d'une sélection de bâtiment et si le bouton est bien actif
			if (editController.EditState != EditController.EditStates.NONE_SELECTION) {
				editController.SelectionRange = EditController.SelectionRanges.WALL;
				if(editController.EditState == EditController.EditStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editController.SelectionRange);
				else if(editController.EditState == EditController.EditStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editController.SelectionRange);
				this.ActivateWallRangeButton ();
			}
			break;
		case UiNames.BUILDING_RANGE_BUTTON:
			// Inversion de l'étendue de sélection et du statuts des boutons de d'étende de sélection si le controlleur
			// de modification n'est pas en attente d'une sélection de bâtiment et si le bouton est bien actif
			if (editController.EditState != EditController.EditStates.NONE_SELECTION) {
				editController.SelectionRange = EditController.SelectionRanges.BUILDING;
				if(editController.EditState == EditController.EditStates.MOVING_MODE)
					movingEditor.InitializeMovingMode (editController.SelectionRange);
				else if(editController.EditState == EditController.EditStates.TURNING_MODE)
					turningEditor.InitializeTurningMode (editController.SelectionRange);
				this.ActivateBuildingRangeButton ();
			}
			break;
		case UiNames.MATERIALS_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.SwitchPallet(gameObject);
				skinChangingEditor.SkinPanelController.SlideSliderRight();
			}
			break;
		case UiNames.COLORS_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.SwitchPallet(gameObject);
				skinChangingEditor.SkinPanelController.SlideSliderLeft();
			}
			break;
		case UiNames.MATERIAL_ITEM_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.UpdateMaterialItems(gameObject);
				skinChangingEditor.ChangeBuildingMaterial(gameObject);
			}
			break;
		case UiNames.COLOR_ITEM_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				skinChangingEditor.UpdateColorItems(gameObject);
				skinChangingEditor.ChangeBuildingColor(gameObject);
			}
			break;
		case UiNames.VALIDATE_EDIT_BUTTON:
			// Validation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editController.Transforming ()) {
				editController.ValidateTransform ();
				editController.ExitTransformMode ();
			}
			break;
		case UiNames.CANCEL_EDIT_BUTTON:
			// Annulation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editController.Transforming ()) {

				editController.CancelTransform ();
				editController.ExitTransformMode ();
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