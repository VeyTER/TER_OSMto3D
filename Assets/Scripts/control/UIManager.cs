using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 	Gère l'intératction de l'utilisateur avec l'interface et les éléments 3D de la scène. Il y a une instance de
/// 	cette par objet qui intéragit avec elle, les gestion du dréoulement des actions a donc été déléguée à une ou
/// 	plusieurs classes n'instanciant qu'un seul objet. Une fois les évènements interceptés, l'action requise est donc
/// 	effectuée sur une classe de contrôle adaptée.
/// </summary>
public class UiManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	/// <summary>
	/// 	Controlleur gérant l'enchainement des modifications d'un objet (déplacement de 5 bâtiments par ex).
	/// </summary>
	public static EditController editController;

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
			if (Input.GetMouseButton (0) && editController.EditState == EditController.EditStates.MOVING_MODE && editController.MovingEditor.IsMoving()) {
				editController.MovingEditor.UpdateObjectMoving ();
				editController.MovingEditor.ShiftCamera ();
			}
			break;
		case UiNames.CANCEL_BUILDING_CREATION_BUTTON:
			if (!controlPanelManager.AllPanelsClosed()) {
				if (this.IsPlayerMoving()) {
					buildingCreationEditor.CompensateCameraMoves();
					buildingCreationEditor.UpdateDisplayedSituation();
				}
			}
			break;
		}
	}

	private bool IsPlayerMoving() {
		return (Input.GetKey("up") || Input.GetKey(KeyCode.Z)
			 || Input.GetKey("down") || Input.GetKey(KeyCode.S)
			 || Input.GetKey("left") || Input.GetKey(KeyCode.Q)
			 || Input.GetKey("right") || Input.GetKey(KeyCode.D));
	}

	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur entre un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnValueChanged(InputField originInputFiled) {
		switch (originInputFiled.name.Split('_')[0]) {
		case UiNames.BUILDING_NAME_INPUT:
			// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
			if (!editController.SelectedBuilding.name.Equals(originInputFiled.text))
				editController.RenameBuilding(editController.SelectedBuilding, originInputFiled.text);
			break;
		case UiNames.BUILDING_CREATION_X_COORD_INPUT:
			Vector3 buildingPositionInXEdit = buildingCreationEditor.SelectedBuilding.transform.position;

			float newPosX = this.ProcessIndicatorLabel(originInputFiled, 0);
			if (!float.IsNaN(newPosX))
				buildingCreationEditor.UpdatePosition(new Vector3(newPosX, buildingPositionInXEdit.y, buildingPositionInXEdit.z));
			break;
		case UiNames.BUILDING_CREATION_Z_COORD_INPUT:
			Vector3 buildingPositionInZEdit = buildingCreationEditor.SelectedBuilding.transform.position;

			float newPosZ = this.ProcessIndicatorLabel(originInputFiled, 0);
			if(!float.IsNaN(newPosZ))
				buildingCreationEditor.UpdatePosition(new Vector3(buildingPositionInZEdit.x, buildingPositionInZEdit.y, newPosZ));
			break;
		case UiNames.BUILDING_CREATION_ORIENTATION_INPUT:
			Quaternion buildingRotationInOrientationEdit = buildingCreationEditor.SelectedBuilding.transform.rotation;

			float newOrientation = this.ProcessIndicatorLabel(originInputFiled, 0);
			if (!float.IsNaN(newOrientation))
				buildingCreationEditor.UpdateOrientation(newOrientation);
			break;
		case UiNames.BUILDING_CREATION_LENGTH_INPUT:
			Vector3 buildingScaleInLengthEdit = buildingCreationEditor.SelectedBuilding.transform.localScale;
			NodeGroup nodeGroupInLengthEdit = BuildingsTools.GetInstance().BuildingToNodeGroup(buildingCreationEditor.SelectedBuilding);
			float buildingWidth = (float) Math.Abs(nodeGroupInLengthEdit.GetNode(0).Longitude - nodeGroupInLengthEdit.GetNode(2).Longitude);

			float newLength = this.ProcessIndicatorLabel(originInputFiled, 1);
			if (!float.IsNaN(newLength))
				buildingCreationEditor.UpdateDimensions(new Vector2(newLength, buildingWidth));
			break;
		case UiNames.BUILDING_CREATION_WIDTH_INPUT:
			Vector3 buildingScaleInWidthEdit = buildingCreationEditor.SelectedBuilding.transform.localScale;
			NodeGroup nodeGroupInWidthEdit = BuildingsTools.GetInstance().BuildingToNodeGroup(buildingCreationEditor.SelectedBuilding);
			float buildingLength = (float)Math.Abs(nodeGroupInWidthEdit.GetNode(0).Latitude - nodeGroupInWidthEdit.GetNode(2).Latitude);

			float newWidth = this.ProcessIndicatorLabel(originInputFiled, 1);
			if (!float.IsNaN(newWidth))
				buildingCreationEditor.UpdateDimensions(new Vector2(buildingLength, newWidth));
			break;
		}
	}

	/// <summary>
	/// 	Trigger se déclanchant lorsque l'utilisateur a terminé d'entrer un nouveau nom pour un bâtiment.
	/// </summary>
	/// <param name="originInputFiled">Champ de saisie source.</param>
	public void OnEndEdit(InputField originInputFiled) {
		switch (originInputFiled.name.Split('_')[0]) {
		case UiNames.BUILDING_NAME_INPUT:
			// Changement du nom du bâtiment si le nom en entrée est différent du nom courant
			if (!editController.SelectedBuilding.name.Equals(originInputFiled.text))
				editController.RenameBuilding(editController.SelectedBuilding, originInputFiled.text);
			break;
		case UiNames.ACTUATOR_VALUE_INPUT:
			GameObject dataDisplay = originInputFiled.gameObject;
			for (; !dataDisplay.tag.Equals(GoTags.DATA_CANVAS) && dataDisplay.transform.parent != null; dataDisplay = dataDisplay.transform.parent.gameObject) ;

			if (dataDisplay != null && dataDisplay.tag.Equals(GoTags.DATA_CANVAS)) {
				GameObject attachedBuilding = BuildingsTools.GetInstance().DataDisplayToBuilding(dataDisplay);
				BuildingComponentsController componentsController = attachedBuilding.GetComponent<BuildingComponentsController>();
				componentsController.FixActuatorValue(originInputFiled.gameObject);
			}
			break;
		}
	}

	private float ProcessIndicatorLabel(InputField originInputFiled, float defaultValue) {
		float parsedValue = 0;
		string inputValue = originInputFiled.text;

		Image inputImage = originInputFiled.gameObject.GetComponent<Image>();

		if (inputValue.Length == 0) {
			inputImage.color = new Color(ThemeColors.BLUE.r, ThemeColors.BLUE.g, ThemeColors.BLUE.b, 0.6F);
			return defaultValue;
		}

		bool parsingOutcome = float.TryParse(inputValue, out parsedValue);
		if (parsingOutcome) {
			inputImage.color = new Color(ThemeColors.BLUE.r, ThemeColors.BLUE.g, ThemeColors.BLUE.b, 0.6F);
			return parsedValue;
		} else {
			inputImage.color = new Color(ThemeColors.RED.r * 1.5F, ThemeColors.RED.g * 1.5F, ThemeColors.RED.b * 1.5F, 0.65F);
			return float.NaN;
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && editController.MovingEditor.IsMotionless()) {
				editController.MovingEditor.StartObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			// Démarrage de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && editController.TurningEditor.IsMotionless()) {
				editController.TurningEditor.StartObjectTurning ();
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && editController.MovingEditor.IsMoving()) {
				editController.MovingEditor.UpdateObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			// Mise à jour de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans
			// le bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && editController.TurningEditor.IsTurning()) {
				editController.TurningEditor.UpdateObjectTurning ();
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
			if (editController.EditState == EditController.EditStates.MOVING_MODE && editController.MovingEditor.IsMoving()) {
				editController.MovingEditor.EndObjectMoving ();
			}
			break;
		case UiNames.TURN_HANDLER:
			// Fin de la rotation si l'objet est immobile et que le controlleur de modification se trouve dans le
			// bon état
			if (editController.EditState == EditController.EditStates.TURNING_MODE && editController.TurningEditor.IsTurning()) {
				editController.TurningEditor.EndObjectTurning ();
			}
			break;
		}
	}

	public void OnMouseDown() {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = editController.HeightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0) {
				editController.HeightChangingEditor.TopFloorColorController.SetPressed();
			} else if (expansionDirection < 0) {
				editController.HeightChangingEditor.BottomFloorColorController.SetPressed();
			}
		}
	}

	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur l'objet sélectionné.
	/// </summary>
	public void OnMouseUp() {
		// Préparation de la modification si l'objet sur lequel a cliqué l'utilisateur est un mur
		if ((tag.Equals (GoTags.WALLS_TAG) || tag.Equals(GoTags.ROOF_TAG)) && !EventSystem.current.IsPointerOverGameObject ()) {
			if ((editController.EditState == EditController.EditStates.NONE_SELECTION || editController.EditState == EditController.EditStates.READY_TO_EDIT) && controlPanelManager.AllPanelsClosed()) {
				editController.SwitchBuilding(transform.parent.gameObject);
			} else if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE && controlPanelManager.AllPanelsClosed()) {
				int expansionDirection = editController.HeightChangingEditor.DesiredDirection(gameObject);
				if (expansionDirection > 0) {
					editController.HeightChangingEditor.IncrementObjectHeight();
					editController.HeightChangingEditor.TopFloorColorController.SetHovered();
				} else if (expansionDirection < 0) {
					editController.HeightChangingEditor.DecrementObjectHeight();
					editController.HeightChangingEditor.BottomFloorColorController.SetHovered();
				}
			}
		}
	}

	public void OnMouseEnter() {
		 if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = editController.HeightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0)
				editController.HeightChangingEditor.TopFloorColorController.SetHovered();
			else if (expansionDirection < 0)
				editController.HeightChangingEditor.BottomFloorColorController.SetHovered();
		}
	}

	private void OnMouseExit() {
		 if (editController.EditState == EditController.EditStates.HEIGHT_CHANGING_MODE) {
			int expansionDirection = editController.HeightChangingEditor.DesiredDirection(gameObject);
			if (expansionDirection > 0)
				editController.HeightChangingEditor.TopFloorColorController.SetInactive();
			else if (expansionDirection < 0)
				editController.HeightChangingEditor.BottomFloorColorController.SetInactive();
		}
	}


	public void OnPointerDown(PointerEventData eventData) {
		switch (name.Split('_')[0]) {
		case UiNames.MOVE_HANDLER:
			if (editController.EditState == EditController.EditStates.MOVING_MODE) {
				Animator animator = this.GetComponent<Animator>();
				animator.Play("PointerDownAnimation");
			}
			break;
		case UiNames.TURN_HANDLER:
			if (editController.EditState == EditController.EditStates.TURNING_MODE) {
				Animator animator = this.GetComponent<Animator>();
				animator.Play("PointerDownAnimation");
			}
			break;
		}
	}

	/// <summary>
	/// 	Méthode appelée lorsque l'utilisateur relâche la pression d'un bouton de la souris sur un élement
	/// 	d'interface.
	/// </summary>
	/// <param name="eventData">Données sur l'évènement.</param>
	public void OnPointerUp(PointerEventData eventData) {
		if (controlPanelManager.AllPanelsClosed() && editController.IsInactive()) {
			this.OnPointerUpControlPanel();
			this.OnPointerUpEditControler();
			this.OnPointerUpBuildingComponentsController();
		} else {
			if (!controlPanelManager.AllPanelsClosed()) {
				this.OnPointerUpControlPanel();
			}

			if (!editController.IsInactive()) {
				this.OnPointerUpEditControler();
			} else {
				this.OnPointerUpBuildingComponentsController();
			}
		}
	}

	private void OnPointerUpControlPanel() {
		PanelController panelController1 = controlPanelManager.GetPanelController(UiNames.VISIBILITY_WHEEL_PANEL);
		PanelController panelController2 = controlPanelManager.GetPanelController(UiNames.BUILDING_CREATION_BOX_PANEL);

		WheelPanelController visibilityPanelController = null;
		if (panelController1.GetType() == typeof(WheelPanelController))
			visibilityPanelController = (WheelPanelController) panelController1;

		BoxPanelController buildingCreationPanelController = null;
		if (panelController2.GetType() == typeof(BoxPanelController))
			buildingCreationPanelController = (BoxPanelController) panelController2;

		VisibilityController visibilityController = VisibilityController.GetInstance();

		switch (name.Split('_')[0]) {
		case UiNames.CREATE_BUILDING_BUTTON:
			if (controlPanelManager.AllPanelsClosed()) {
				buildingCreationPanelController.transform.gameObject.SetActive(true);
				buildingCreationPanelController.OpenPanel(null);
				buildingCreationEditor.InitializeMode();
				buildingCreationEditor.UpdateDisplayedSituation();
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
			if (controlPanelManager.AllPanelsClosed()) {
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
				visibilityController.ShowBuildingNodes();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_ROAD_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowRoadsNodes();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_WALLS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowWalls();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_ROOFS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowRoofs();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_ROADS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowRoads();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}	
			break;
		case UiNames.DISABLED_FOOTWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowFootways();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_CYCLEWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowCycleways();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.DISABLED_TREES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.ShowTrees();
				visibilityPanelController.EnableButton(transform.parent.gameObject);
			}
			break;

		case UiNames.ENABLED_BUILDING_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideBuildingNodes();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_ROAD_NODES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideRoadsNodes();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_WALLS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideWalls();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_ROOFS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideRoofs();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_ROADS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideRoads();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_FOOTWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideFootways();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_CYCLEWAYS_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideCycleways();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		case UiNames.ENABLED_TREES_BUTTON:
			if (controlPanelManager.ControlState == ControlPanelManager.ControlStates.VISIBILITY_TOGGLELING) {
				visibilityController.HideTrees();
				visibilityPanelController.DisableButton(transform.parent.gameObject);
			}
			break;
		}
	}

	private void OnPointerUpEditControler() {
		switch (name.Split('_')[0]) {
		case UiNames.MOVE_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editController.IsTransforming() || editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterMovingMode();
			}
			break;
		case UiNames.MOVE_HANDLER:
			if (editController.EditState == EditController.EditStates.MOVING_MODE) {
				Animator animator = this.GetComponent<Animator>();
				animator.Play("PointerUpAnimation");
			}
			break;
		case UiNames.TURN_BUTTON:
			// Préparation de la rotation d'un objet si le controlleur est prêt
			if (editController.IsTransforming() || editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterTurningMode();
			}
			break;
		case UiNames.TURN_HANDLER:
			if (editController.EditState == EditController.EditStates.TURNING_MODE) {
				Animator animator = this.GetComponent<Animator>();
				animator.Play("PointerUpAnimation");
			}
			break;
		case UiNames.CHANGE_HEIGHT_BUTTON:
			if (editController.IsTransforming() || editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.EnterHeightChangingMode();
			}
			break;
		case UiNames.CHANGE_SKIN_BUTTON:
			// Préparation du déplacement d'un objet si le controlleur est prêt
			if (editController.IsTransforming() || editController.EditState == EditController.EditStates.READY_TO_EDIT) {
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
				editController.ValidateEdit();
				editController.ExitBuilding();
			}
			break;
		case UiNames.CANCEL_BUTTON:
			// Annulation de la modification d'une série de bâtiments si le controlleur est prêt
			if (editController.EditState == EditController.EditStates.READY_TO_EDIT) {
				editController.CancelEdit();
				editController.ExitBuilding();
			}
			break;
		case UiNames.MATERIALS_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				editController.SkinChangingEditor.SwitchPallet(gameObject);
				editController.SkinChangingEditor.SkinPanelController.SlideSliderRight();
			}
			break;
		case UiNames.COLORS_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				editController.SkinChangingEditor.SwitchPallet(gameObject);
				editController.SkinChangingEditor.SkinPanelController.SlideSliderLeft();
			}
			break;
		case UiNames.MATERIAL_ITEM_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				editController.SkinChangingEditor.UpdateMaterialItems(gameObject);
				editController.SkinChangingEditor.ChangeBuildingMaterial(gameObject);
			}
			break;
		case UiNames.COLOR_ITEM_BUTTON:
			if (editController.EditState == EditController.EditStates.SKIN_CHANGING_MODE) {
				editController.SkinChangingEditor.UpdateColorItems(gameObject);
				editController.SkinChangingEditor.ChangeBuildingColor(gameObject);
			}
			break;
		case UiNames.VALIDATE_EDIT_BUTTON:
			// Validation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editController.IsTransforming()) {
				editController.ValidateTransform();
				editController.ExitTransformMode();
			}
			break;
		case UiNames.CANCEL_EDIT_BUTTON:
			// Annulation d'une transformation si le controlleur de modification est bien en cours de modification
			if (editController.IsTransforming()) {
				editController.CancelTransform();
				editController.ExitTransformMode();
			}
			break;
		}
	}

	private void OnPointerUpBuildingComponentsController() {
		GameObject dataDisplay = gameObject;
		for (; !dataDisplay.tag.Equals(GoTags.DATA_CANVAS) && dataDisplay.transform.parent != null; dataDisplay = dataDisplay.transform.parent.gameObject) ;

		if (dataDisplay != null && dataDisplay.tag.Equals(GoTags.DATA_CANVAS)) {
			GameObject attachedBuilding = BuildingsTools.GetInstance().DataDisplayToBuilding(dataDisplay);
			BuildingComponentsController componentsController = attachedBuilding.GetComponent<BuildingComponentsController>();

			switch (name.Split('_')[0]) {
			case UiNames.BUILDING_DATA_ICON_BUTTON:
				componentsController.ToggleHeightState();
				break;
			case UiNames.ACTUATOR_DECREASE_BUTTON:
				GameObject decreaseActuatorInput = transform.parent.Find(UiNames.ACTUATOR_VALUE_INPUT).gameObject;
				componentsController.ShiftActuatorValue(decreaseActuatorInput, -1);
				break;
			case UiNames.ACTUATOR_INCREASE_BUTTON:
				GameObject increaseActuatorInput = transform.parent.Find(UiNames.ACTUATOR_VALUE_INPUT).gameObject;
				componentsController.ShiftActuatorValue(increaseActuatorInput, 1);
				break;
			}
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