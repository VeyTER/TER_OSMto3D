using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// 	<para>
/// 		Composant censé être unique controllant la modification d'un objet tel qu'un bâtiment. Cette classe va de
/// 		pair avec la classe UiManager qui, elle, est présentes dans plusieurs GameObject à chaque fois sous une
/// 		instance différente. On aurait pu avoir uen seule classe réceptionnant les actions sur les GameObject et
/// 		agissant en conséquence, mais la gestion des états aurait été plus complexe sur plusieurs instances.
/// 	</para>
/// 	<para></para>
///  	<para>
/// 		Cette classe avait été conçue à l'origine pour éditer soit un mur seul, soit un bâtiment entier, mais
/// 		l'édition d'un mur seul a été désactivée (le boutons ont été masqués) car jugée peu pertinente.
/// 	</para>
/// 	<para></para>
/// 	<para>
/// 		ATTENTION : L'édition des murs ayant été abandonnée en cours de route, celle-ci ne fonctionnera pas
/// 		entièrement si elle est réactivée.
/// 	</para>
/// </summary>
public class EditionController : MonoBehaviour {
	/// <summary>
	/// 	Les différents états dans lesquels peut se trouver le controlleur. L'état NONE_SELECTION signifie que le
	/// 	controlleur n'est affcté à aucun objet, MOVING_TO_OBJECT symbolise le déplacement vers l'objet courant
	/// 	tandis que READY_TO_EDIT indique que le controlleur est positionné au niveau de l'objet et prêt à effectuer
	/// 	les modifications.
	/// </summary>
	public enum EditionStates {
		NONE_SELECTION,
		MOVING_TO_OBJECT,
		READY_TO_EDIT,
		RENAMING_MODE,
		MOVING_MODE,
		TURNING_MODE,
		CHANGING_HEIGHT_MODE,
		CHANGING_COLOR_MDOE,
		MOVING_TO_INITIAL_SITUATION
	}


	/// <summary>
	/// 	Les différentes étendues de sélection pour le déplacement ou l'orientation. L'étendue WALL indique que seul
	/// 	le mur courant doit être modifié tandis que l'étendue BUILDING signifie que le bâtiment enier doit être
	/// 	modifié.
	/// </summary>
	public enum SelectionRanges { WALL, BUILDING }


	/// <summary>
	/// 	Les états du panneau latéral. Celui-ci peut-être fermé ou ouvert, mais également en cours d'ouverture ou
	/// 	de fermeture.
	/// </summary>
	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }


	/// <summary>Etat courant de modification.</summary>
	private EditionStates editionState;

	/// <summary>Etendue courante de sélection.</summary>
	private SelectionRanges selectionRange;


	/// <summary>Etat courant du panneau latéral.</summary>
	private PanelStates panelState;


	/// <summary>Editeur de la position des objets, permet de controler le déplacement un objet.</summary>
	private MovingEditor movingEditor;

	/// <summary>Editeur de l'orientation des objets, permet de controler la rotation d'un objet.</summary>
	private TurningEditor turningEditor;


	/// <summary>Unique instance du singleton BuildingsTools contenant des outils pour les bâtiments.</summary>
	private BuildingsTools buildingsTools;


	/// <summary>Bâtiments renommés durant la période de modification.</summary>
	private Hashtable renamedBuildings;

	/// <summary>Objets déplacés durant la période de modification.</summary>
	private List<GameObject> movedObjects;

	/// <summary>Objets tournés durant la période de modification.</summary>
	private List<GameObject> turnedObjects;


	/// <summary>Mur sélectionné pour édition</summary>
	private GameObject selectedWall;

	/// <summary>Bâtiment sélectionné pour édition</summary>
	private GameObject selectedBuilding;


	/// <summary>
	/// 	Positions initiales des murs avant qu'ils ne soient modififés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, Vector3> wallsInitPos;

	/// <summary>
	/// 	Positions initiales des bâtiments avant qu'ils ne soient modififés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, Vector3> buildingsInitPos;


	/// <summary>
	/// 	Angles initiaux des murs avant qu'ils ne soient modifiés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, float> wallsInitAngle;

	/// <summary>
	/// 	Angles initiaux des bâtiments avant qu'ils ne soient modifiés pour la 1ère fois durant une période
	/// 	de modification.
	/// </summary>
	private Dictionary<GameObject, float> buildingsInitAngle;


	/// <summary>Instance de la classe de contrôle de la caméra.</summary>
	private CameraController cameraController;


	/// <summary>Bouton permettant d'ouvrir ou fermer le panneau latéral.</summary>
	private GameObject slidePanelButton;

	/// <summary>Bouton permettant de valider la transformation d'un bâtiment (déplacement par ex).</summary>
	private GameObject validateEditionButton;

	/// <summary>Bouton permettant d'annuler la transformation d'un bâtiment (déplacement par ex).</summary>
	private GameObject cancelEditionButton;


	/// <summary>Bouton désactivé permettant de ne sélectionner que le mur courant pour modification.</summary>
	private GameObject wallRangeButton;

	/// <summary>Bouton désactivé permettant de sélectionner le batiment entier courant pour modification.</summary>
	private GameObject buildingRangeButton;


	/// <summary>Panneau latéral contenant l'interface graphique de modification.</summary>
	private GameObject lateralPanel;


	public void Start() {
		this.editionState = EditionStates.NONE_SELECTION;
		this.selectionRange = SelectionRanges.BUILDING;

		this.panelState = PanelStates.CLOSED;

		this.movingEditor = new MovingEditor (GameObject.Find(UiNames.MOVE_HANDLER));
		this.turningEditor = new TurningEditor (GameObject.Find(UiNames.TURN_HANDLER));

		this.buildingsTools = BuildingsTools.GetInstance ();

		this.renamedBuildings = new Hashtable ();
		this.movedObjects = new List<GameObject> ();
		this.turnedObjects = new List<GameObject> ();

		this.wallsInitPos = new Dictionary<GameObject, Vector3>();
		this.buildingsInitPos = new Dictionary<GameObject, Vector3>();

		this.wallsInitAngle = new Dictionary<GameObject, float>();
		this.buildingsInitAngle = new Dictionary<GameObject, float>();

		this.validateEditionButton = GameObject.Find(UiNames.VALDIATE_EDITION_BUTTON);
		this.cancelEditionButton = GameObject.Find(UiNames.CANCEL_EDITION_BUTTON);

		this.cameraController = Camera.main.GetComponent<CameraController> ();

		this.slidePanelButton = GameObject.Find (UiNames.SLIDE_PANEL_BUTTON);
		this.validateEditionButton.transform.localScale = Vector3.zero;
		this.cancelEditionButton.transform.localScale = Vector3.zero;

		this.wallRangeButton = GameObject.Find(UiNames.WALL_RANGE_BUTTON);
		this.buildingRangeButton = GameObject.Find(UiNames.BUILDING_RANGE_BUTTON);

//		this.wallRangeButton.transform.localScale = Vector3.zero;
//		this.buildingRangeButton.transform.localScale = Vector3.zero;
//
//		Button wallrangeButtonComponent = buildingRangeButton.GetComponent<Button> ();
//		wallrangeButtonComponent.interactable = false;

		this.lateralPanel = GameObject.Find (UiNames.LATERAL_PANEL);
		this.lateralPanel.SetActive(false);
	}


	/// <summary>
	/// 	Change le bâtiment sélectionné et déplace la caméra vers lui. Cette méthode utilise pour cela le mur
	/// 	sélectionné et remonte au bâtiment entier grâce aux liens de parenté.
	/// </summary>
	/// <param name="selectedWall">Mur sur lequel l'utilisateur a cliqué.</param>
	public void SwitchBuilding(GameObject selectedWall) {
		this.selectedWall = selectedWall;
		selectedBuilding = selectedWall.transform.parent.gameObject;

		// Activation et ourverture du panneau latéral s'il est inactif
		if (lateralPanel.activeInHierarchy == false) {
			lateralPanel.SetActive (true);
			this.OpenPanel (null);
		}

		// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment
		InputField[] textInputs = GameObject.FindObjectsOfType<InputField> ();
		int i = 0;
		for (; i < textInputs.Length && textInputs[i].name.Equals (UiNames.BUILDING_NAME_TEXT_INPUT); i++);
		if (i < textInputs.Length)
			textInputs[i].text = selectedBuilding.name;

		// Changement de la couleur du bâtiment sélectionné
		buildingsTools.DiscolorAll ();
		buildingsTools.ColorAsSelected (selectedBuilding);

		// Enregistrement de la situation initiale du mur courant
		if (!wallsInitPos.ContainsKey(this.selectedWall) && !wallsInitAngle.ContainsKey(this.selectedWall)) {
			wallsInitPos.Add (this.selectedWall, this.selectedWall.transform.position);
			wallsInitAngle.Add (this.selectedWall, this.selectedWall.transform.rotation.eulerAngles.y);
		}

		// Enregistrement de la situation initiale du bâtiment courant
		if (!buildingsInitPos.ContainsKey(selectedBuilding) && !buildingsInitAngle.ContainsKey(selectedBuilding)) {
			buildingsInitPos.Add (selectedBuilding, selectedBuilding.transform.position);
			buildingsInitAngle.Add (selectedBuilding, selectedBuilding.transform.rotation.eulerAngles.y);
		}

		// Enregistrement de la situation initiale de la caméra
		if (editionState == EditionStates.NONE_SELECTION) {
			GameObject mainCameraGo = Camera.main.gameObject;
			cameraController.InitPosition = mainCameraGo.transform.position;
			cameraController.InitRotation = mainCameraGo.transform.rotation;
		}

		// Déplacement de la caméra jusqu'au bâtiment sélectionné avec mise à jour de l'état de modification à la fin
		// du déplacement
		editionState = EditionStates.MOVING_TO_OBJECT;
		cameraController.StartCoroutine (
			cameraController.MoveToBuilding(selectedBuilding, () => {
				editionState = EditionStates.READY_TO_EDIT;
			})
		);
	}


	/// <summary>
	/// 	Quitte le bâtiment jusqu'alors en cours de modification en refermant le panneau latéral et en déplaçant la
	/// 	caméra à la position qu'elle occupait avant modification.
	/// </summary>
	public void ExitBuilding() {
		selectedBuilding = null;
		selectedWall = null;

		// Décoloration du bâtiment courant
		buildingsTools.DiscolorAll ();

		// Fermeture du panneau latéral et désactiovation de ce dernier lorsqu'il est fermé
		this.ClosePanel (() => {
			lateralPanel.SetActive (false);
		});


		// Déplacement de la caméra à sa position initiale et réinitialisation de l'état de modification à la fin du
		// déplacement
		editionState = EditionController.EditionStates.MOVING_TO_INITIAL_SITUATION;
		cameraController.StartCoroutine (
			cameraController.MoveToSituation(cameraController.InitPosition, cameraController.InitRotation, () => {
				editionState = EditionController.EditionStates.NONE_SELECTION;
			})
		);
	}


	/// <summary>
	/// 	Inverse l'état d'ouverture du panneau latéral : s'il état fermé, il s'ouvre; s'il était ouvert, il se ferme.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de l'inversion.</param>
	public void TogglePanel(Action finalAction) {
		if (panelState == PanelStates.CLOSED) {
			this.StartCoroutine ( this.SlidePanel (finalAction, -1) );
			panelState = PanelStates.CLOSED_TO_OPEN;
		} else if (panelState == PanelStates.OPEN) {
			this.StartCoroutine ( this.SlidePanel (finalAction, 1) );
			panelState = PanelStates.OPEN_TO_CLOSED;
		}
	}

	/// <summary>
	/// 	Ouvre la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de l'ouverture.</param>
	public void OpenPanel(Action finalAction) {
		this.StartCoroutine ( this.SlidePanel(finalAction, -1) );
		panelState = PanelStates.CLOSED_TO_OPEN;
	}


	/// <summary>
	/// 	Ferme la panneau latéral.
	/// </summary>
	/// <param name="finalAction">Action finale à effectuer à la fin de la fermeture.</param>
	public void ClosePanel(Action finalAction) {
		this.StartCoroutine ( this.SlidePanel(finalAction, 1) );
		panelState = PanelStates.OPEN_TO_CLOSED;
	}


	/// <summary>
	/// 	Fait glisser le panneau latéral d'une configuration à une autre.
	/// </summary>
	/// <returns>Temporisateur servant à générer une animation.</returns>
	/// <param name="finalAction">Action finale à effectuer à la fin de la glissade.</param>
	/// <param name="direction">Direction de la glissade.</param>
	private IEnumerator SlidePanel(Action finalAction, int direction) {
		// Passage de la direction à -1 si elle est strictement négative et à 1 si elle est positive
		direction = direction > 0 ? 1 : -1;

		// Configuration courante du panneau
		Vector3 panelPosition = lateralPanel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)lateralPanel.transform;

		// Situation courante du bouton de contrôle du panneau
		Vector3 panelButtonPosition = slidePanelButton.transform.localPosition;
		Quaternion panelButtonRotation = slidePanelButton.transform.localRotation;

		// Position en X initiale et ciblé du panneau
		float initPanelPosX = panelPosition.x;
		float targetPanelPosX = panelPosition.x + (direction * panelRectTransform.rect.width);

		// Position en X initiale et ciblé du bouton de contrôle
		float panelButtonInitPosX = panelButtonPosition.x;
		float targetPanelButtonPosX = panelButtonPosition.x - (direction * 20);

		// orientation en Z initiale et ciblé du bouton de contrôle
		float panelButtonInitRotZ = panelButtonRotation.eulerAngles.z;
		float targetPanelButtonRotZ = panelButtonRotation.eulerAngles.z + (direction * 180);

		Transform panelTransform = lateralPanel.transform;
		Transform panelButtonTransform = slidePanelButton.transform;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			// Calcul de la situation courante
			float currentPanelPosX = initPanelPosX - (initPanelPosX - targetPanelPosX) * cursor;
			float currentPanelButtonPosX = panelButtonInitPosX - (panelButtonInitPosX - targetPanelButtonPosX) * cursor;
			float currentPanelButtonRotZ = panelButtonInitRotZ - (panelButtonInitRotZ - targetPanelButtonRotZ) * cursor;

			panelTransform.localPosition = new Vector3 (currentPanelPosX, panelPosition.y, panelPosition.z);
			panelButtonTransform.localPosition = new Vector3 (currentPanelButtonPosX, panelButtonPosition.y, panelButtonPosition.z);
			panelButtonTransform.localRotation = Quaternion.Euler (panelButtonRotation.x, panelButtonRotation.y, currentPanelButtonRotZ);

			yield return new WaitForSeconds (0.01F);
		}

		if(panelState == PanelStates.CLOSED_TO_OPEN)
			panelState = PanelStates.OPEN;
		else if (panelState == PanelStates.OPEN_TO_CLOSED)
			panelState = PanelStates.CLOSED;

		// Appel de la tâche finale s'il y en a une
		if(finalAction != null)
			finalAction ();
	}


	/// <summary>
	/// 	Inverse les visibilité des boutons flottants avec un effet de zoom.
	/// </summary>
	/// <returns>The floatting buttons.</returns>
	private IEnumerator ToggleFloattingButtons() {
		// Echelle courante des boutons de valdiation et d'annulation
		Vector3 validateButtonInitScale = validateEditionButton.transform.localScale;
		Vector3 cancelButtonInitScale = cancelEditionButton.transform.localScale;

		// Echelle courante des boutons d'étendue de mur et de bâtiment
//		Vector3 wallRangeInitScale = wallRangeButton.transform.localScale;
//		Vector3 buildingRangeInitScale = buildingRangeButton.transform.localScale;

		// Calcul de l'échelle à atteindre à partir de l'échelle courante
		Vector3 targetScale = Vector3.zero;
		if (validateButtonInitScale == Vector3.one && cancelButtonInitScale == Vector3.one /*&& wallRangeInitScale == Vector3.one && buildingRangeInitScale == Vector3.one*/)
			targetScale = Vector3.zero;
		else if (validateButtonInitScale == Vector3.zero && cancelButtonInitScale == Vector3.zero /*&& wallRangeInitScale == Vector3.zero && buildingRangeInitScale == Vector3.zero*/)
			targetScale = Vector3.one;

		Transform validateButtonTransform = validateEditionButton.transform;
		Transform cancelButtonTransform = cancelEditionButton.transform;

//		Transform wallRangeButtonTransform = wallRangeButton.transform;
//		Transform buildingRangeButtonTransform = buildingRangeButton.transform;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			// Calcul de la situation courante
			if (validateButtonInitScale.x == 0) {
				validateButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
				cancelButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);

//				wallRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
//				buildingRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
			} else if (validateButtonInitScale.x == 1) {
				validateButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
				cancelButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);

//				wallRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
//				buildingRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
			}

			yield return new WaitForSeconds (0.01F);
		}

		// Affectation de l'échelle finale pour éviter les imprécisions
		validateButtonTransform.localScale = targetScale;
		cancelButtonTransform.localScale = targetScale;

//		wallRangeButtonTransform.localScale = targetScale;
//		buildingRangeButtonTransform.localScale = targetScale;
	}


	/// <summary>
	/// 	Change le nom du NodeGroup et du GameObject correspondant au bâtiment et enregistre le nom inital. Cette
	/// 	méthode ne modifie pas le nom dans les fichiers car l'utilisateur n'a pas encore enregistré les
	/// 	modifications
	/// </summary>
	/// <param name="building">Bâtiment dont on veut changer le nom.</param>
	/// <param name="newName">Novueau nom à donner au bâtiment.</param>
	public void RenameBuilding(GameObject building, string newName) {
		// Enregistrement du nom initial si c'est la 1ère fois qu'il est modifié durant la période courante
		// de modification.
		if (!renamedBuildings.ContainsKey (building))
			renamedBuildings.Add (building, building.name);

		// Changement du nom du GameObject et du NodeGroupe correspondant au bâtiment
		NodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup (building);
		buildingNodeGroup.Name = newName;
		building.name = newName;

		// Chagement du nom des murs apparentés au bâtiment
		for (int i = 0; i < building.transform.childCount; i++)
			building.transform.GetChild (i).name = newName + "_mur_" + i;
	}


	/// <summary>
	/// 	Active le mode de déplacement d'un bâtiment en entrant dans un mode de transformation et en initialisant
	/// 	les éditeurs.
	/// </summary>
	public void EnterMovingMode() {
		this.EnterTransformMode ();
		movingEditor.Initialize(selectedWall, selectedBuilding);
		movingEditor.MoveHandler.SetActive (true);
		editionState = EditionStates.MOVING_MODE;
	}

	/// <summary>
	/// 	Active le mode de rotation d'un bâtiment en entrant dans un mode de transformation et en initialisant
	/// 	les éditeurs.
	/// </summary>
	public void EnterTurningMode() {
		this.EnterTransformMode ();
		turningEditor.Initialize(selectedWall, selectedBuilding);
		turningEditor.TurnHandler.SetActive (true);
		editionState = EditionStates.TURNING_MODE;
	}

	/// <summary>
	/// 	Active dans un mode de transformation d'un bâtiment en fermant le panneau latéral et en inversant
	/// 	l'affichage des boutons flottants.
	/// </summary>
	private void EnterTransformMode() {
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleFloattingButtons");
	}


	/// <summary>
	/// 	Désactive le mode de transformation courant d'un bâtiment en effetuant les tâches permettant de revenir à
	/// 	la configuration d'avant la modification.
	/// </summary>
	public void ExitTransformMode() {
		// Désactivation des poignées
		movingEditor.MoveHandler.SetActive (false);
		turningEditor.TurnHandler.SetActive (false);

		// Ouverture du panneau latéral
		if(panelState == PanelStates.CLOSED)
			this.OpenPanel (null);

		// Inversion de l'affichage des boutons latéraux
		this.StartCoroutine ("ToggleFloattingButtons");

		// Mise à jour de la situation de la caméra pour la repositionner au-dessus du bâtiment courant qui aura
		// probablement bougé
		editionState = EditionStates.MOVING_TO_OBJECT;
		cameraController.StartCoroutine (
			cameraController.MoveToBuilding (selectedBuilding, () => {
				editionState = EditionStates.READY_TO_EDIT;
			})
		);
	}


	/// <summary>
	/// 	Valide la transformation d'un objet en l'ajoutant aux objects modifiés et en mettant à jour les neouds
	/// 	correspondants (NodeGroup).
	/// </summary>
	public void ValidateTransform() {
		// [ NON MAINTENU ] Enregistrement du mur courant au dictionnaire des objets déplacés ou tournés
		if (this.WallTransformed ()) {
			if(editionState == EditionStates.MOVING_MODE && !movedObjects.Contains (selectedWall))
				movedObjects.Add (selectedWall);
			else if(editionState == EditionStates.TURNING_MODE && !turnedObjects.Contains (selectedWall))
				turnedObjects.Add (selectedWall);
		}

		// Enregistrement du bâtiment courant au dictionnaire des objets déplacés ou tournés
		if (this.BuildingTransformed ()) {
			if(editionState == EditionStates.MOVING_MODE)
				movedObjects.Add (selectedBuilding);
			else if(editionState == EditionStates.TURNING_MODE && !turnedObjects.Contains (selectedBuilding))
				turnedObjects.Add (selectedBuilding);
			
			buildingsTools.UpdateNodes (selectedBuilding);
		}
	}


	/// <summary>
	/// 	Annule la transformation de l'objet courant à leur affectant la situation qu'ils avaient avant le passage
	/// 	en mode de transformation (MOVING_MODE par ex) et en mettant à jour les neouds correspondant (NodeGroup).
	/// </summary>
	public void CancelTransform() {
		// [ NON MAINTENU ] Affecte au mur courant la position ou la rotation de départ en fonction du mode de
		// 	tranformation courant
		if (this.WallTransformed()) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedWall.transform.position = movingEditor.SelectedWallStartPos;
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedWallRotation = selectedWall.transform.rotation;
				selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.x, turningEditor.SelectedWallStartAngle, selectedWallRotation.z);
			}
		}

		// Affecte au bâtiment courant la position ou la rotation de départ en fonction du mode de tranformation courant
		if (this.BuildingTransformed()) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedBuilding.transform.position = movingEditor.SelectedBuildingStartPos;

				Vector3 buildingPosition = selectedBuilding.transform.position;
				Vector3 buildingNodesGroupPosition = movingEditor.SelectedBuildingNodes.transform.position;

				movingEditor.SelectedBuildingNodes.transform.position = new Vector3 (buildingPosition.x, buildingNodesGroupPosition.y, buildingPosition.z);
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
				selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turningEditor.SelectedBuildingStartAngle, selectedBuildingRotation.z);
			
				float buildingAngle = selectedBuilding.transform.rotation.eulerAngles.y;
				Quaternion buildingNodesGroupRotation = turningEditor.SelectedBuildingNodes.transform.rotation;

				turningEditor.SelectedBuildingNodes.transform.rotation = Quaternion.Euler (buildingNodesGroupRotation.x, buildingAngle, buildingNodesGroupRotation.z);
			}

			buildingsTools.UpdateNodes (selectedBuilding);
		}
	}


	/// <summary>
	/// Indique si un objet est en cours de transformation (en déplacement par ex).
	/// </summary>
	/// <returns><c>true</c>, un objet est en cours de transformation, <c>false</c> sinon.</returns>
	public bool Transforming() {
		return editionState == EditionStates.MOVING_MODE
			|| editionState == EditionStates.TURNING_MODE
			|| editionState == EditionStates.RENAMING_MODE
			|| editionState == EditionStates.CHANGING_HEIGHT_MODE
			|| editionState == EditionStates.CHANGING_COLOR_MDOE;
	}


	/// <summary>
	/// 	Indique si un mur a fait l'objet d'une transformation en vérifiant dans les éditeurs.
	/// </summary>
	/// <returns><c>true</c>, si la transformation a concerné un mur <c>false</c> sinon.</returns>
	private bool WallTransformed() {
		return editionState == EditionStates.MOVING_MODE && movingEditor.WallTransformed
			|| editionState == EditionStates.TURNING_MODE && turningEditor.WallTransformed;
	}

	/// <summary>
	/// 	Indique si un bâtiment a fait l'objet d'une transformation en vérifiant dans les éditeurs.
	/// </summary>
	/// <returns><c>true</c>, si la transformation a concerné un bâtiment <c>false</c> sinon.</returns>
	private bool BuildingTransformed() {
		return editionState == EditionStates.MOVING_MODE && movingEditor.BuildingTransformed
			|| editionState == EditionStates.TURNING_MODE && turningEditor.BuildingTransformed;
	}


	/// <summary>
	/// 	Valide l'édition courante en mettant à jour les groupes de noeuds concernés et en faisant appel à l'instance
	/// 	de BuildingTools pour mettre à jour les objets dans les différents fichiers.
	/// </summary>
	public void ValidateEdit() {
		// Rennomage des bâtiments et de leurs murs dans les fichiers si ceux-ci ont bien changé le nom
		foreach (DictionaryEntry buildingEntry in renamedBuildings) {
			if(buildingEntry.Key.GetType() == typeof(GameObject) && buildingEntry.Value.GetType() == typeof(string)) {
				GameObject renamedBuilding = (GameObject)buildingEntry.Key;
				string oldName = (string)buildingEntry.Value;

				if(!renamedBuilding.name.Equals(oldName))
					buildingsTools.SetName (renamedBuilding, renamedBuilding.name);
			}
		}

		// On stocke les objets modifiés dans un ensemble pour éviter de mettre à jour deux fois les mêmes bâtiments
		// (les HashSet supprimant les doublons)
		HashSet<GameObject> transformedObjects = new HashSet<GameObject> ();

		// Mise à jour des groupes de noeuds correspondant aux bâtiments déplacés
		foreach (KeyValuePair<GameObject, Vector3> buildingPositionEntry in buildingsInitPos) {
			GameObject building = buildingPositionEntry.Key;
			buildingsTools.UpdateNodes (building);
			transformedObjects.Add (building);
		}

		// Mise à jour des groupes de noeuds correspondant aux bâtiments tournés
		foreach (KeyValuePair<GameObject, float> buildingAngleEntry in buildingsInitAngle) {
			GameObject building = buildingAngleEntry.Key;
			buildingsTools.UpdateNodes (building);
			transformedObjects.Add (building);
		}

		// [NON MAINTENU] Mise à jour des groupes de noeuds correspondant aux murs déplacés et tournés
		foreach (KeyValuePair<GameObject, Vector3> wallPositionEntry in wallsInitPos) {}
		foreach (KeyValuePair<GameObject, float> wallAngleEntry in wallsInitAngle) {}

		// Mise à jour, dans les fichiers, des données conernant les bâtiments modifiés
		foreach (GameObject building in transformedObjects) {
			Vector3 buildingInitPos = Vector3.zero;
			float buildingInitAngle = 0F;

			bool positionContained = buildingsInitPos.TryGetValue (building, out buildingInitPos);
			bool angleContained = buildingsInitAngle.TryGetValue (building, out buildingInitAngle);

			if (positionContained && angleContained) {
				if (!building.transform.position.Equals (buildingInitPos) || building.transform.rotation.eulerAngles.y != buildingInitAngle) {
					buildingsTools.SetLocation (building);
				}
			}
		}
			
		// Suppression des situations initiales des objets modifiés
		this.CleanHistory ();
	}


	/// <summary>
	/// 	Annule la modification de tous les bâtiments de la session courante en leur affectant leurs caractéristiques
	/// 	de départ.
	/// </summary>
	public void CancelEdit() {
		// Rennomage des bâtiments et de leurs murs avec leur nom initial
		foreach (DictionaryEntry buildingEntry in renamedBuildings) {
			if (buildingEntry.Key.GetType () == typeof(GameObject) && buildingEntry.Value.GetType () == typeof(String)) {
				GameObject renamedBuilding = (GameObject)buildingEntry.Key;
				string oldName = (string)buildingEntry.Value;

				renamedBuilding.name = oldName;
				for (int i = 0; i < renamedBuilding.transform.childCount; i++)
					renamedBuilding.transform.GetChild (i).name = oldName + "_mur_" + i;
			}
		}

		// Affectation à chaque batiment de la position qu'il avait lorsqu'il a été sélectionné par l'utilisateur
		foreach (KeyValuePair<GameObject, Vector3> buildingPositionEntry in buildingsInitPos) {
			GameObject building = buildingPositionEntry.Key;
			building.transform.position = buildingPositionEntry.Value;

			GameObject buildingNodes = buildingsTools.BuildingToBuildingNodeGroup (building);

			Vector3 buildingPosition = building.transform.position;
			Vector3 buildingNodesGroupPosition = buildingNodes.transform.position;

			buildingNodes.transform.position = new Vector3 (buildingPosition.x, buildingNodesGroupPosition.y, buildingPosition.z);
			buildingsTools.UpdateNodes (building);
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
			buildingsTools.UpdateNodes (building);
		}

		// [NON MAINTENU] Affecte à chaque mur la position qu'il avait lorsqu'il a été sélectionné par l'utilisateur
		foreach (KeyValuePair<GameObject, Vector3> wallPositionEntry in wallsInitPos) {
			GameObject wall = wallPositionEntry.Key;
			wall.transform.position = wallPositionEntry.Value;
		}

		// [NON MAINTENU] Affecte à chaque mur l'angle qu'il avait lorsqu'il a été sélectionné par l'utilisateur
		foreach (KeyValuePair<GameObject, float> wallAngleEntry in wallsInitAngle) {
			GameObject wall = wallAngleEntry.Key;
			Quaternion wallRotation = wall.transform.rotation;
			wall.transform.rotation = wall.transform.rotation = Quaternion.Euler (wallRotation.x, wallAngleEntry.Value, wallRotation.z);
		}

		// Suppression des situations initiales des objets modifiés
		this.CleanHistory ();
	}


	/// <summary>
	///		Supprime la situation initiale de chaque bâtiment et de chaque mur.
	/// </summary>
	private void CleanHistory() {
		wallsInitPos.Clear();
		wallsInitAngle.Clear ();

		buildingsInitPos.Clear();
		buildingsInitAngle.Clear ();
	}


	public EditionStates EditionState {
		get { return editionState; }
		set { editionState = value; }
	}

	public SelectionRanges SelectionRange {
		get { return selectionRange; }
		set { selectionRange = value; }
	}

	public MovingEditor MovingEditor {
		get { return movingEditor; }
		set { movingEditor = value; }
	}

	public TurningEditor TurningEditor {
		get { return turningEditor; }
		set { turningEditor = value; }
	}

	public PanelStates PanelState {
		get { return panelState; }
		set { panelState = value; }
	}

	public Hashtable RenamedBuildings {
		get { return renamedBuildings; }
	}

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public GameObject LateralPanel {
		get { return lateralPanel; }
	}
}