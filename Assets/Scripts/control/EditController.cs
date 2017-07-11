﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 	<para>
/// 		Composant censé être unique et controllant la modification d'un objet tel qu'un bâtiment. Cette classe va de
/// 		pair avec la classe UiManager qui, elle, est présente dans plusieurs GameObject à chaque fois sous une
/// 		instance différente. On aurait pu avoir une seule classe réceptionnant les actions sur les GameObject et
/// 		agissant en conséquence, mais la gestion des états à cheuval sur plusieurs instance risquait d'être bancale.
/// 	</para>
/// 	<para></para>
///  	<para>
/// 		Cette classe avait été conçue à l'origine pour modifier soit un mur seul, soit un bâtiment entier, mais
/// 		la modification d'un mur seul a été désactivée (les boutons ont été masqués) car jugée peu pertinente.
/// 	</para>
/// 	<para></para>
/// 	<para>
/// 		ATTENTION : La modification des murs ayant été abandonnée en cours de route, celle-ci ne fonctionnera pas
/// 		entièrement si elle est réactivée.
/// 	</para>
/// </summary>
public class EditController : MonoBehaviour {
	/// <summary>
	/// 	Les différents états dans lesquels peut se trouver le controlleur. L'état NONE_SELECTION signifie que le
	/// 	controlleur n'est affecté à aucun objet, MOVING_TO_OBJECT symbolise le déplacement vers l'objet courant
	/// 	tandis que READY_TO_EDIT indique que le controlleur est positionné au niveau de l'objet et prêt à effectuer
	/// 	les modifications.
	/// </summary>
	public enum EditStates {
		NONE_SELECTION,
		MOVING_TO_OBJECT,
		READY_TO_EDIT,
		RENAMING_MODE,
		MOVING_MODE,
		TURNING_MODE,
		HEIGHT_CHANGING_MODE,
		SKIN_CHANGING_MODE,
		MOVING_TO_INITIAL_SITUATION
	}


	/// <summary>Etat courant de modification.</summary>
	private EditStates editState;


	/// <summary>Editeur de la position des objets, permet de controler le déplacement un objet.</summary>
	private MovingEditor movingEditor;

	/// <summary>Editeur de l'orientation des objets, permet de controler la rotation d'un objet.</summary>
	private TurningEditor turningEditor;

	private HeightChangingEditor heightChangingEditor;

	private SkinChangingEditor skinChangingEditor;


	/// <summary>Unique instance du singleton BuildingsTools contenant des outils pour les bâtiments.</summary>
	private BuildingsTools buildingsTools;

	private CityBuilder cityBuilder;

	private UiBuilder uiBuilder;

	/// <summary>Bâtiments renommés durant la période de modification.</summary>
	private Dictionary<GameObject, string> renamedBuildings;


	/// <summary>Bâtiment sélectionné pour modification.</summary>
	private GameObject selectedBuilding;


	/// <summary>
	/// 	Positions initiales des bâtiments avant qu'ils ne soient modififés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, Vector3> buildingsInitPos;


	/// <summary>
	/// 	Angles initiaux des bâtiments avant qu'ils ne soient modifiés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, float> buildingsInitAngle;

	private Dictionary<GameObject, int> buildingsInitHeight;

	private Dictionary<GameObject, Material> buildingsInitMaterial;
	private Dictionary<GameObject, Color> buildingsInitColor;


	/// <summary>Unique instance de la classe CameraCOntroller permettant de contrôler la caméra.</summary>
	private CameraController cameraController;


	/// <summary>Bouton désactivé permettant de ne sélectionner que le mur courant pour modification.</summary>
	private GameObject wallRangeButton;

	/// <summary>Bouton désactivé permettant de sélectionner le batiment entier courant pour modification.</summary>
	private GameObject buildingRangeButton;

	private GameObject controlPanel;

	/// <summary>Panneau d'modification contenant l'interface graphique de modification.</summary>
	private GameObject editPanel;

	private EditPanelController editPanelController;

	public void Start() {
		this.editState = EditStates.NONE_SELECTION;

		this.movingEditor = new MovingEditor (GameObject.Find(UiNames.MOVE_HANDLER));
		this.turningEditor = new TurningEditor (GameObject.Find(UiNames.TURN_HANDLER));
		this.heightChangingEditor = new HeightChangingEditor();
		this.skinChangingEditor = new SkinChangingEditor(GameObject.Find(UiNames.SKIN_SELECTION_PANEL));

		this.buildingsTools = BuildingsTools.GetInstance ();
		this.cityBuilder = CityBuilder.GetInstance();
		this.uiBuilder = UiBuilder.GetInstance();

		this.renamedBuildings = new Dictionary<GameObject, string> ();
		this.buildingsInitPos = new Dictionary<GameObject, Vector3>();
		this.buildingsInitAngle = new Dictionary<GameObject, float>();
		this.buildingsInitHeight = new Dictionary<GameObject, int>();

		this.buildingsInitMaterial = new Dictionary<GameObject, Material>();
		this.buildingsInitColor = new Dictionary<GameObject, Color>();

		this.wallRangeButton = GameObject.Find(UiNames.WALL_RANGE_BUTTON);
		this.buildingRangeButton = GameObject.Find(UiNames.BUILDING_RANGE_BUTTON);

		this.controlPanel = GameObject.Find(UiNames.CONTROL_PANEL);

		this.editPanel = GameObject.Find (UiNames.EDIT_PANEL);
		this.editPanel.SetActive(false);

		this.cameraController = Camera.main.GetComponent<CameraController> ();
		this.editPanelController = editPanel.GetComponent<EditPanelController>();

		RectTransform editPanelRect = (RectTransform) this.editPanelController.transform;
		this.editPanelController.StartPosition = new Vector3(editPanelRect.localPosition.x, 0, 0);
		this.editPanelController.EndPosition = new Vector3(editPanelRect.localPosition.x - editPanelRect.rect.width, 0, 0);
	}


	/// <summary>
	/// 	Change le bâtiment sélectionné et déplace la caméra vers lui. Cette méthode utilise pour cela le mur
	/// 	sélectionné et remonte au bâtiment entier grâce aux liens de parenté.
	/// </summary>
	/// <param name="selectedWall">Mur sur lequel l'utilisateur a cliqué.</param>
	public void SwitchBuilding(GameObject selectedWall) {
		if(selectedBuilding != null)
			buildingsTools.DiscolorAsSelected(selectedBuilding);

		GameObject newBuilding = selectedWall.transform.parent.gameObject;

		// Changement de la couleur du bâtiment sélectionné
		buildingsTools.ColorAsSelected(newBuilding);

		if (newBuilding != selectedBuilding) {
			selectedBuilding = newBuilding;

			NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);

			// Activation et ourverture du panneau latéral s'il est inactif
			if (!editPanel.activeInHierarchy) {
				editPanel.SetActive(true);
				editPanelController.OpenPanel(null);
				editPanelController.OpenSlideButton();
			}

			// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment
			GameObject buildingNameField = GameObject.Find(UiNames.BUILDING_NAME_INPUT);
			InputField buildingNameTextInput = buildingNameField.GetComponent<InputField>();
			buildingNameTextInput.text = selectedBuilding.name;

			GameObject IdValueLabel = GameObject.Find(UiNames.ID_INDICATOR_LABEL);
			Text idText = IdValueLabel.GetComponent<Text>();
			idText.text = nodeGroup.Id;

			// Enregistrement de la situation initiale du bâtiment courant
			if (!buildingsInitPos.ContainsKey(selectedBuilding) && !buildingsInitAngle.ContainsKey(selectedBuilding)) {
				buildingsInitPos.Add(selectedBuilding, selectedBuilding.transform.position);
				buildingsInitAngle.Add(selectedBuilding, selectedBuilding.transform.rotation.eulerAngles.y);
			}

			if (!buildingsInitHeight.ContainsKey(SelectedBuilding))
				buildingsInitHeight.Add(selectedBuilding, nodeGroup.NbFloor);

			GameObject firstWall = SelectedBuilding.transform.GetChild(0).gameObject;
			MeshRenderer meshRenderer = firstWall.GetComponent<MeshRenderer>();

			if (!buildingsInitMaterial.ContainsKey(SelectedBuilding))
				buildingsInitMaterial.Add(SelectedBuilding, meshRenderer.materials[0]);

			if (!buildingsInitColor.ContainsKey(SelectedBuilding))
				buildingsInitColor.Add(SelectedBuilding, meshRenderer.materials[0].color);

			// Enregistrement de la situation initiale de la caméra
			if (editState == EditStates.NONE_SELECTION) {
				GameObject mainCamera = Camera.main.gameObject;
				cameraController.InitPosition = mainCamera.transform.position;
				cameraController.InitRotation = mainCamera.transform.rotation;
			}

			if (controlPanel.activeInHierarchy)
				controlPanel.SetActive(false);

			if (uiBuilder.BuildingDataDisplays.activeInHierarchy)
				uiBuilder.BuildingDataDisplays.SetActive(false);

			// Déplacement de la caméra jusqu'au bâtiment sélectionné avec mise à jour de l'état de modification à la fin
			// du déplacement
			editState = EditStates.MOVING_TO_OBJECT;
			cameraController.StartCoroutine(
				cameraController.MoveToBuilding(selectedBuilding, false, () => {
					editState = EditStates.READY_TO_EDIT;
				}, 90)
			);
		}
	}

	/// <summary>
	/// 	Quitte le bâtiment jusqu'alors en cours de modification en refermant le panneau latéral et en déplaçant la
	/// 	caméra à la position qu'elle occupait avant modification.
	/// </summary>
	public void ExitBuilding() {
		// Décoloration du bâtiment courant
		buildingsTools.DiscolorAsSelected(selectedBuilding);

		selectedBuilding = null;

		// Fermeture du panneau latéral et désactivation de ce dernier lorsqu'il est fermé
		editPanelController.ClosePanel (() => {
			editPanel.SetActive (false);
			uiBuilder.BuildingDataDisplays.SetActive(true);
		});

		editPanelController.CloseSlideButton();

		// Déplacement de la caméra à sa position initiale et réinitialization de l'état de modification à la fin du
		// déplacement
		editState = EditController.EditStates.MOVING_TO_INITIAL_SITUATION;
		cameraController.StartCoroutine (
			cameraController.MoveToSituation(cameraController.InitPosition, cameraController.InitRotation, () => {
				controlPanel.SetActive(true);
				editState = EditController.EditStates.NONE_SELECTION;
			})
		);
	}


	/// <summary>
	/// 	Change le nom du NodeGroup et du GameObject correspondant au bâtiment et enregistre le nom inital. Cette
	/// 	méthode ne modifie pas le nom dans les fichiers car l'utilisateur n'a pas encore enregistré les
	/// 	modifications.
	/// </summary>
	/// <param name="building">Bâtiment dont on veut changer le nom.</param>
	/// <param name="newName">Nouveau nom à donner au bâtiment.</param>
	public void RenameBuilding(GameObject building, string newName) {
		// Enregistrement du nom initial si c'est la 1ère fois qu'il est modifié durant la période courante
		// de modification.
		if (!renamedBuildings.ContainsKey(building))
			renamedBuildings.Add(building, building.name);

		// Changement du nom du GameObject et du NodeGroupe correspondant au bâtiment
		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup (building);
		nodeGroup.Name = newName;
		building.name = newName;

		// Changement du nom des murs apparentés au bâtiment
		for (int i = 0; i < building.transform.childCount; i++)
			building.transform.GetChild (i).name = newName + "_mur_" + i;
	}


	/// <summary>
	/// 	Active le mode de déplacement d'un bâtiment en entrant dans un mode de transformation et en initialisant
	/// 	les éditeurs.
	/// </summary>
	public void EnterMovingMode() {
		Action initializationSequence = () => {
			movingEditor.InitializeBasics(selectedBuilding);
			movingEditor.MoveHandler.SetActive(true);
			movingEditor.InitializeMovingMode();
			editState = EditStates.MOVING_MODE;
		};
		this.EnterTransformMode (initializationSequence);
	}

	/// <summary>
	/// 	Active le mode de rotation d'un bâtiment en entrant dans un mode de transformation et en initialisant
	/// 	les éditeurs.
	/// </summary>
	public void EnterTurningMode() {
		Action initializationSequence = () => {
			turningEditor.InitializeBasics(selectedBuilding);
			turningEditor.TurnHandler.SetActive(true);
			turningEditor.InitializeTurningMode();
			editState = EditStates.TURNING_MODE;
		};
		this.EnterTransformMode (initializationSequence);
	}

	public void EnterHeightChangingMode() {
		Action initializationSequence = () => {
			heightChangingEditor.InitializeBasics(selectedBuilding);
			heightChangingEditor.InitializeHeightChangingMode();
			cameraController.StartCoroutine(cameraController.MoveToBuilding(selectedBuilding, true, null, 15));
			editState = EditStates.HEIGHT_CHANGING_MODE;
		};
		this.EnterTransformMode(initializationSequence);
	}


	public void EnterSkinChangingMode() {
		Action initializationSequence = () => {
			skinChangingEditor.InitializeBasics(selectedBuilding);
			skinChangingEditor.InitializeSkinChangingMode();

			cameraController.StartCoroutine(cameraController.MoveToBuilding(selectedBuilding, true, () => {
				cameraController.StartCoroutine(cameraController.TurnAroundBuilding(selectedBuilding, 15));
			}, 15));

			buildingsTools.DiscolorAsSelected(selectedBuilding);
			editState = EditStates.SKIN_CHANGING_MODE;
		};
		this.EnterTransformMode(initializationSequence);
	}


	/// <summary>
	/// 	Active le mode de transformation d'un bâtiment en fermant le panneau latéral et en inversant
	/// 	l'affichage des boutons flottants.
	/// </summary>
	private void EnterTransformMode(Action initializationSequence) {
		if (this.IsTransforming()) {
			this.ValidateTransform();
			this.ExitTransformMode(initializationSequence);
		} else {
			initializationSequence();
		}
		editPanelController.ClosePanel(null);
		editPanelController.CloseSlideButton();
		editPanelController.OpenFloattingButtons();
	}


	/// <summary>
	/// 	Désactive le mode de transformation courant d'un bâtiment en effetuant les tâches permettant de revenir à
	/// 	la configuration d'avant la modification.
	/// </summary>
	public void ExitTransformMode(Action initializationSequence = null) {
		switch (editState) {
		case EditStates.MOVING_MODE:
			movingEditor.MoveHandler.SetActive (false);
			break;
		case EditStates.TURNING_MODE:
			turningEditor.TurnHandler.SetActive (false);
			break;
		case EditStates.HEIGHT_CHANGING_MODE:
			Destroy(HeightChangingEditor.TopFloor);
			Destroy(HeightChangingEditor.BottomFloor);
			break;
		case EditStates.SKIN_CHANGING_MODE:
			buildingsTools.ColorAsSelected(selectedBuilding);

			// Fermeture du panneau latéral et désactivation de ce dernier lorsqu'il est fermé
			cameraController.StopTurningAround();
			skinChangingEditor.SkinPanelController.ClosePanel(() => {
				skinChangingEditor.SkinPanel.SetActive(false);
			});
			break;
		}

		if (editPanelController.IsPanelClosed()) {
			editPanelController.OpenPanel(null);
			editPanelController.OpenSlideButton();
		}

		editPanelController.CloseFloattingButtons();

		// Mise à jour de la situation de la caméra pour la repositionner au-dessus du bâtiment courant qui aura
		// probablement bougé
		editState = EditStates.MOVING_TO_OBJECT;
		cameraController.StartCoroutine (
			cameraController.MoveToBuilding (selectedBuilding, false, () => {
				if (initializationSequence == null)
					editState = EditStates.READY_TO_EDIT;
				else
					initializationSequence();
			}, 90)
		);
	}

	public void ValidateTransform() {
		switch (editState) {
		case EditStates.MOVING_MODE:
			movingEditor.ValidateTransform();
			break;
		case EditStates.TURNING_MODE:
			turningEditor.ValidateTransform();
			break;
		case EditStates.HEIGHT_CHANGING_MODE:
			heightChangingEditor.ValidateTransform();
			break;
		case EditStates.SKIN_CHANGING_MODE:
			skinChangingEditor.ValidateTransform();
			break;
		}
	}

	public void CancelTransform() {
		switch (editState) {
		case EditStates.MOVING_MODE:
			movingEditor.CancelTransform();
			break;
		case EditStates.TURNING_MODE:
			turningEditor.CancelTransform();
			break;
		case EditStates.HEIGHT_CHANGING_MODE:
			heightChangingEditor.CancelTransform();
			break;
		case EditStates.SKIN_CHANGING_MODE:
			skinChangingEditor.CancelTransform();
			break;
		}
	}

	/// <summary>
	/// 	Indique si un objet est en cours de transformation (en déplacement par ex).
	/// </summary>
	/// <returns><c>true</c>, un objet est en cours de transformation, <c>false</c> sinon.</returns>
	public bool IsTransforming() {
		return editState == EditStates.MOVING_MODE
			|| editState == EditStates.TURNING_MODE
			|| editState == EditStates.RENAMING_MODE
			|| editState == EditStates.HEIGHT_CHANGING_MODE
			|| editState == EditStates.SKIN_CHANGING_MODE;
	}


	/// <summary>
	/// 	Valide l'modification courante en mettant à jour les groupes de noeuds concernés et en faisant appel à l'instance
	/// 	de BuildingTools pour mettre à jour les objets dans les différents fichiers.
	/// </summary>
	public void ValidateEdit() {
		// Rennomage des bâtiments et de leurs murs dans les fichiers si ceux-ci ont bien changé le nom
		foreach (KeyValuePair<GameObject, string> buildingEntry in renamedBuildings) {
			GameObject renamedBuilding = (GameObject)buildingEntry.Key;
			string oldName = buildingEntry.Value;

			if(!renamedBuilding.name.Equals(oldName))
				buildingsTools.UpdateName (renamedBuilding);
		}

		// Stockage des objets modifiés dans un ensemble pour éviter de mettre à jour deux fois les mêmes bâtiments
		// (un HashSet supprimant les doublons)
		HashSet<GameObject> movedOrTurnedObjects = new HashSet<GameObject> ();

		// Mise à jour des groupes de noeuds correspondant aux bâtiments déplacés
		foreach (KeyValuePair<GameObject, Vector3> buildingPositionEntry in buildingsInitPos) {
			GameObject building = buildingPositionEntry.Key;
			buildingsTools.UpdateNodesPosition (building);
			movedOrTurnedObjects.Add (building);
		}

		// Mise à jour des groupes de noeuds correspondant aux bâtiments tournés
		foreach (KeyValuePair<GameObject, float> buildingAngleEntry in buildingsInitAngle) {
			GameObject building = buildingAngleEntry.Key;
			buildingsTools.UpdateNodesPosition (building);
			movedOrTurnedObjects.Add (building);
		}

		// Mise à jour, dans les fichiers, des données conernant les bâtiments modifiés
		foreach (GameObject building in movedOrTurnedObjects) {
			Vector3 buildingInitPos = buildingsInitPos[building];
			float buildingInitAngle = buildingsInitAngle[building];

			if (!building.transform.position.Equals (buildingInitPos) || building.transform.rotation.eulerAngles.y != buildingInitAngle)
				buildingsTools.UpdateLocation (building);

			if (cityBuilder.SensorsEquippedBuildings.ContainsKey(building.name))
				this.UpdateAttachedDisplayPosition(building);
		}

		foreach (KeyValuePair<GameObject, int> buildingHeightEntry in buildingsInitHeight) {
			GameObject building = buildingHeightEntry.Key;
			int buildingInitHeight = buildingHeightEntry.Value;

			NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(building);
			if (buildingInitHeight != nodeGroup.NbFloor)
				buildingsTools.UpdateHeight(building, nodeGroup.NbFloor);

			if (cityBuilder.SensorsEquippedBuildings.ContainsKey(building.name))
				this.UpdateAttachedDisplayPosition(building);
		}

		foreach (KeyValuePair<GameObject, Material> buildingMaterialEntry in buildingsInitMaterial) {
			GameObject building = buildingMaterialEntry.Key;
			Material buildingInitMaterial = buildingMaterialEntry.Value;

			NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(building);
			if (!buildingInitMaterial.Equals(nodeGroup.CustomMaterial))
				buildingsTools.UpdateMaterial(building, nodeGroup.CustomMaterial);
		}

		foreach (KeyValuePair<GameObject, Color> buildingColorEntry in buildingsInitColor) {
			GameObject building = buildingColorEntry.Key;
			Color buildingInitColor = buildingColorEntry.Value;

			NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(building);
			if (!buildingInitColor.Equals(nodeGroup.OverlayColor))
				buildingsTools.UpdateColor(building, nodeGroup.OverlayColor);
		}

		// Suppression des situations initiales des objets modifiés
		this.ClearHistory ();
	}


	/// <summary>
	/// 	Annule la modification de tous les bâtiments de la session courante en leur affectant leurs caractéristiques
	/// 	de départ.
	/// </summary>
	public void CancelEdit() {
		// Rennomage des bâtiments et de leurs murs avec leur nom initial
		foreach (KeyValuePair<GameObject, string> buildingEntry in renamedBuildings) {
			GameObject renamedBuilding = (GameObject)buildingEntry.Key;
			string oldName = buildingEntry.Value;

			renamedBuilding.name = oldName;
			for (int i = 0; i < renamedBuilding.transform.childCount; i++)
				renamedBuilding.transform.GetChild (i).name = oldName + "_mur_" + i;
		}

		// Affectation à chaque batiment de la position qu'il avait lorsqu'il a été sélectionné par l'utilisateur
		foreach (KeyValuePair<GameObject, Vector3> buildingPositionEntry in buildingsInitPos) {
			GameObject building = buildingPositionEntry.Key;
			building.transform.position = buildingPositionEntry.Value;

			GameObject buildingNodes = buildingsTools.BuildingToBuildingNodeGroup (building);

			Vector3 buildingPosition = building.transform.position;
			Vector3 buildingNodesGroupPosition = buildingNodes.transform.position;

			buildingNodes.transform.position = new Vector3 (buildingPosition.x, buildingNodesGroupPosition.y, buildingPosition.z);
			buildingsTools.UpdateNodesPosition (building);

			if (cityBuilder.SensorsEquippedBuildings.ContainsKey(building.name))
				this.UpdateAttachedDisplayPosition(building);
		}

		// Affectation à chaque batiment de l'angle qu'il avait lorsqu'il a été sélectionné par l'utilisateur
		foreach (KeyValuePair<GameObject, float> buildingAngleEntry in buildingsInitAngle) {
			GameObject building = buildingAngleEntry.Key;
			Quaternion buildingRotation = building.transform.rotation;
			building.transform.rotation = building.transform.rotation = Quaternion.Euler (buildingRotation.x, buildingAngleEntry.Value, buildingRotation.z);

			GameObject buildingNodes = buildingsTools.BuildingToBuildingNodeGroup (building);

			float buildingAngle = building.transform.rotation.eulerAngles.y;
			Quaternion buildingNodesGroupRotation = buildingNodes.transform.rotation;

			buildingNodes.transform.rotation = Quaternion.Euler (buildingNodesGroupRotation.x, buildingAngle, buildingNodesGroupRotation.z);
			buildingsTools.UpdateNodesPosition (building);
		}

		foreach (KeyValuePair<GameObject, int> buildingHeightEntry in buildingsInitHeight) {
			GameObject building = buildingHeightEntry.Key;
			int buildingHeight = buildingHeightEntry.Value;

			NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(building);
			nodeGroup.NbFloor = buildingHeight;

			cityBuilder.RebuildBuilding(building, buildingHeight);

			if (cityBuilder.SensorsEquippedBuildings.ContainsKey(building.name))
				this.UpdateAttachedDisplayPosition(building);
		}

		foreach (KeyValuePair<GameObject, Material> buildingMaterialEntry in buildingsInitMaterial) {
			GameObject building = buildingMaterialEntry.Key;
			Material buildingInitMaterial = buildingMaterialEntry.Value;
			buildingsTools.ReplaceMaterial(building, buildingInitMaterial);
		}

		foreach (KeyValuePair<GameObject, Color> buildingColorEntry in buildingsInitColor) {
			GameObject building = buildingColorEntry.Key;
			Color buildingInitColor = buildingColorEntry.Value;
			buildingsTools.ReplaceColor(building, buildingInitColor);
		}

		// Suppression des situations initiales des objets modifiés
		this.ClearHistory ();
	}

	private void UpdateAttachedDisplayPosition(GameObject building) {
		GameObject firstWall = building.transform.GetChild(0).gameObject;
		GameObject dataDisplay = buildingsTools.BuildingToDataDisplay(building);

		Vector3 buildingPosition = building.transform.position;
		dataDisplay.transform.position = new Vector3(buildingPosition.x, firstWall.transform.localScale.y, buildingPosition.z);
	}

	/// <summary>
	///		Supprime la situation initiale de chaque bâtiment et de chaque mur.
	/// </summary>
	private void ClearHistory() {
		renamedBuildings.Clear();

		buildingsInitPos.Clear();
		buildingsInitAngle.Clear();

		buildingsInitHeight.Clear();

		buildingsInitMaterial.Clear();
		buildingsInitColor.Clear();

		movingEditor.ClearHistory();
		turningEditor.ClearHistory();
		heightChangingEditor.ClearHistory();
		skinChangingEditor.ClearHistory();
	}

	public bool IsInactive() {
		return editState == EditStates.NONE_SELECTION;
	}

	public EditStates EditState {
		get { return editState; }
		set { editState = value; }
	}

	public MovingEditor MovingEditor {
		get { return movingEditor; }
		set { movingEditor = value; }
	}

	public TurningEditor TurningEditor {
		get { return turningEditor; }
		set { turningEditor = value; }
	}

	public HeightChangingEditor HeightChangingEditor {
		get { return heightChangingEditor; }
		set { heightChangingEditor = value; }
	}

	public SkinChangingEditor SkinChangingEditor {
		get { return skinChangingEditor; }
		set { skinChangingEditor = value; }
	}

	public Dictionary<GameObject, string> RenamedBuildings {
		get { return renamedBuildings; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public GameObject EditPanel {
		get { return editPanel; }
	}

	public EditPanelController EditPanelController {
		get { return editPanelController; }
		set { editPanelController = value; }
	}
}