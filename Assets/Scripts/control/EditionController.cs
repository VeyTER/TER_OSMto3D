using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class EditionController : MonoBehaviour {
	public enum EditionStates {
		NONE_SELECTION,
		MOVING_TO_BUILDING,
		READY_TO_EDIT,
		RENAMING_MODE,
		MOVING_MODE,
		TURNING_MODE,
		CHANGING_HEIGHT_MDOE,
		CHANGING_COLOR_MDOE,
		MOVING_TO_INITIAL_SITUATION
	}

	public enum SelectionRanges { WALL, BUILDING }

	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private PanelStates panelState;

	private MovingEditor movingEditor;
	private TurningEditor turningEditor;

	private BuildingsTools buildingsTools;

	private Hashtable renamedBuildings;
	private List<GameObject> movedObjects;
	private List<GameObject> turnedObjects;

	private GameObject selectedWall;
	private GameObject selectedBuilding;

	private Dictionary<GameObject, Vector3> wallsInitPos;
	private Dictionary<GameObject, Vector3> buildingsInitPos;

	private Dictionary<GameObject, float> wallsInitAngle;
	private Dictionary<GameObject, float> buildingsInitAngle;

	private CameraController cameraController;

	private GameObject slidePanelButton;
	private GameObject validateEditionButton;
	private GameObject cancelEditionButton;

	private GameObject wallRangeButton;
	private GameObject buildingRangeButton;

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

		this.wallRangeButton.transform.localScale = Vector3.zero;
		this.buildingRangeButton.transform.localScale = Vector3.zero;

		Button wallrangeButtonComponent = buildingRangeButton.GetComponent<Button> ();
		wallrangeButtonComponent.interactable = false;

		this.lateralPanel = GameObject.Find (UiNames.LATERAL_PANEL);
		this.lateralPanel.SetActive(false);
	}

	public void SwitchBuilding(GameObject selectedWall) {
		this.selectedWall = selectedWall;
		selectedBuilding = selectedWall.transform.parent.gameObject;

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

		buildingsTools.DiscolorAll ();
		buildingsTools.ColorAsSelected (selectedBuilding);

		if (!wallsInitPos.ContainsKey(this.selectedWall) && !wallsInitAngle.ContainsKey(this.selectedWall)) {
			print (this.selectedWall.transform.position + "  " + this.selectedWall.transform.localPosition);
			wallsInitPos.Add (this.selectedWall, this.selectedWall.transform.position);
			wallsInitAngle.Add (this.selectedWall, this.selectedWall.transform.rotation.eulerAngles.y);
		}

		if (!buildingsInitPos.ContainsKey(selectedBuilding) && !buildingsInitAngle.ContainsKey(selectedBuilding)) {
			buildingsInitPos.Add (selectedBuilding, selectedBuilding.transform.position);
			buildingsInitAngle.Add (selectedBuilding, selectedBuilding.transform.rotation.eulerAngles.y);
		}

		if (editionState == EditionStates.NONE_SELECTION) {
			GameObject mainCameraGo = Camera.main.gameObject;
			cameraController.InitPosition = mainCameraGo.transform.position;
			cameraController.InitRotation = mainCameraGo.transform.rotation;
		}

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraController.StartCoroutine (
			cameraController.MoveToBuilding(selectedBuilding, () => {
				editionState = EditionStates.READY_TO_EDIT;
			})
		);
	}

	public void ExitBuilding() {
		selectedBuilding = null;
		selectedWall = null;

		buildingsTools.DiscolorAll ();

		this.ClosePanel (() => {
			lateralPanel.SetActive (false);
		});

		editionState = EditionController.EditionStates.MOVING_TO_INITIAL_SITUATION;
		cameraController.StartCoroutine (
			cameraController.MoveToSituation(cameraController.InitPosition, cameraController.InitRotation, () => {
				editionState = EditionController.EditionStates.NONE_SELECTION;
			})
		);
	}

	public void TogglePanel(Action finalAction) {
		if (panelState == PanelStates.CLOSED) {
			this.StartCoroutine ( this.SlidePanel (finalAction, -1) );
			panelState = PanelStates.CLOSED_TO_OPEN;
		} else if (panelState == PanelStates.OPEN) {
			this.StartCoroutine ( this.SlidePanel (finalAction, 1) );
			panelState = PanelStates.OPEN_TO_CLOSED;
		}
	}

	public void OpenPanel(Action finalAction) {
		this.StartCoroutine ( this.SlidePanel(finalAction, -1) );
		panelState = PanelStates.CLOSED_TO_OPEN;
	}

	public void ClosePanel(Action finalAction) {
		this.StartCoroutine ( this.SlidePanel(finalAction, 1) );
		panelState = PanelStates.OPEN_TO_CLOSED;
	}

	private IEnumerator SlidePanel(Action finalAction, int direction) {
		direction = direction > 0 ? 1 : -1;

		Vector3 panelPosition = lateralPanel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)lateralPanel.transform;

		Vector3 panelButtonPosition = slidePanelButton.transform.localPosition;
		Quaternion panelButtonRotation = slidePanelButton.transform.localRotation;

		float panelInitPosX = panelPosition.x;
		float targetPanelPosX = panelPosition.x + (direction * panelRectTransform.rect.width);

		float panelButtonInitPosX = panelButtonPosition.x;
		float targetPanelButtonPosX = panelButtonPosition.x - (direction * 20);

		float panelButtonInitRotZ = panelButtonRotation.eulerAngles.z;
		float targetPanelButtonRotZ = panelButtonRotation.eulerAngles.z + (direction * 180);

		Transform panelTransform = lateralPanel.transform;
		Transform panelButtonTransform = slidePanelButton.transform;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			float currentPanelPosX = panelInitPosX - (panelInitPosX - targetPanelPosX) * cursor;
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

		if(finalAction != null)
			finalAction ();
	}

	private IEnumerator ToggleFloattingButtons() {
		Vector3 validateButtonInitScale = validateEditionButton.transform.localScale;
		Vector3 cancelButtonInitScale = cancelEditionButton.transform.localScale;

		Vector3 wallRangeInitScale = wallRangeButton.transform.localScale;
		Vector3 buildingRangeInitScale = buildingRangeButton.transform.localScale;

		Vector3 targetScale = Vector3.zero;

		if (validateButtonInitScale == Vector3.one && cancelButtonInitScale == Vector3.one && wallRangeInitScale == Vector3.one && buildingRangeInitScale == Vector3.one)
			targetScale = Vector3.zero;
		else if (validateButtonInitScale == Vector3.zero && cancelButtonInitScale == Vector3.zero && wallRangeInitScale == Vector3.zero && buildingRangeInitScale == Vector3.zero)
			targetScale = Vector3.one;

		Transform validateButtonTransform = validateEditionButton.transform;
		Transform cancelButtonTransform = cancelEditionButton.transform;

		Transform wallRangeButtonTransform = wallRangeButton.transform;
		Transform buildingRangeButtonTransform = buildingRangeButton.transform;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			if (validateButtonInitScale.x == 0) {
				validateButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
				cancelButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);

				wallRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
				buildingRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
			} else if (validateButtonInitScale.x == 1) {
				validateButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
				cancelButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);

				wallRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
				buildingRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
			}

			yield return new WaitForSeconds (0.01F);
		}

		validateButtonTransform.localScale = targetScale;
		cancelButtonTransform.localScale = targetScale;

		wallRangeButtonTransform.localScale = targetScale;
		buildingRangeButtonTransform.localScale = targetScale;
	}

	public void RenameBuilding(GameObject building, string newName) {
		if (!renamedBuildings.ContainsKey (building))
			renamedBuildings.Add (building, building.name);

		NodeGroup buildingNodeGroup = buildingsTools.GameObjectToNodeGroup (building);
		buildingNodeGroup.Name = newName;
		building.name = newName;
		for (int i = 0; i < building.transform.childCount; i++)
			building.transform.GetChild (i).name = newName + "_mur_" + i;
	}

	public void EnterMovingMode() {
		this.EnterTransformMode ();
		movingEditor.Initialize(selectedWall, selectedBuilding);
		movingEditor.MoveHandler.SetActive (true);
		editionState = EditionStates.MOVING_MODE;
	}
	public void EnterTurningMode() {
		this.EnterTransformMode ();
		turningEditor.Initialize(selectedWall, selectedBuilding);
		turningEditor.TurnHandler.SetActive (true);
		editionState = EditionStates.TURNING_MODE;
	}
	private void EnterTransformMode() {
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleFloattingButtons");
	}

	public void ExitMovingMode() {
		movingEditor.MoveHandler.SetActive (false);
		this.ExitTransformMode ();
	}
	public void ExitTurningMode() {
		turningEditor.TurnHandler.SetActive (false);
		this.ExitTransformMode ();
	}
	private void ExitTransformMode() {
		if(panelState == PanelStates.CLOSED)
			this.OpenPanel (null);

		this.StartCoroutine ("ToggleFloattingButtons");

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraController.StartCoroutine (
			cameraController.MoveToBuilding (selectedBuilding, () => {
				editionState = EditionStates.READY_TO_EDIT;
			})
		);
	}

	public void ValidateTransform() {
		if (this.WallTransformed () && !movedObjects.Contains (selectedWall)) {
			if(editionState == EditionStates.MOVING_MODE && !movedObjects.Contains (selectedWall))
				movedObjects.Add (selectedWall);
			else if(editionState == EditionStates.TURNING_MODE && !turnedObjects.Contains (selectedWall))
				turnedObjects.Add (selectedWall);
		}

		if (this.BuildingTransformed ()) {
			if(editionState == EditionStates.MOVING_MODE && !movedObjects.Contains (selectedBuilding))
				movedObjects.Add (selectedBuilding);
			else if(editionState == EditionStates.TURNING_MODE && !turnedObjects.Contains (selectedBuilding))
				turnedObjects.Add (selectedBuilding);
		}
	}

	public void CancelTransform() {
		if (this.WallTransformed()) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedWall.transform.position = movingEditor.SelectedWallStartPos;
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedWallRotation = selectedWall.transform.rotation;
				selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.x, turningEditor.SelectedWallStartAngle, selectedWallRotation.z);
			}
		}

		if (this.BuildingTransformed()) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedBuilding.transform.position = movingEditor.SelectedBuildingStartPos;
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
				selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.x, turningEditor.SelectedBuildingStartAngle, selectedBuildingRotation.z);
			}
		}
	}

	private bool WallTransformed() {
		return editionState == EditionStates.MOVING_MODE && movingEditor.WallTransformed || editionState == EditionStates.TURNING_MODE && turningEditor.WallTransformed;
	}

	private bool BuildingTransformed() {
		return editionState == EditionStates.MOVING_MODE && movingEditor.BuildingTransformed || editionState == EditionStates.TURNING_MODE && turningEditor.BuildingTransformed;
	}

	public void ValidateEdit() {
		foreach (DictionaryEntry buildingEntry in renamedBuildings) {
			if(buildingEntry.Key.GetType() == typeof(GameObject) && buildingEntry.Value.GetType() == typeof(string)) {
				GameObject renamedBuilding = (GameObject)buildingEntry.Key;
				string oldName = (string)buildingEntry.Value;

				if(!renamedBuilding.name.Equals(oldName))
					buildingsTools.SetName (renamedBuilding, renamedBuilding.name);
			}
		}

		this.CleanHistory ();
	}

	public void CancelEdit() {
		foreach (DictionaryEntry buildingEntry in renamedBuildings) {
			if (buildingEntry.Key.GetType () == typeof(GameObject) && buildingEntry.Value.GetType () == typeof(String)) {
				GameObject renamedBuilding = (GameObject)buildingEntry.Key;
				string oldName = (string)buildingEntry.Value;

				renamedBuilding.name = oldName;
				for (int i = 0; i < renamedBuilding.transform.childCount; i++)
					renamedBuilding.transform.GetChild (i).name = oldName + "_mur_" + i;
			}
		}

		Vector3 buildingPosition = movingEditor.SelectedBuildingNodes.transform.position;
		Vector3 buildingNodePosition = selectedBuilding.transform.position;
		movingEditor.SelectedBuildingNodes.transform.position = new Vector3 (buildingPosition.x, buildingNodePosition.y, buildingPosition.z);

		foreach (KeyValuePair<GameObject, Vector3> buildingPositionEntry in buildingsInitPos) {
			GameObject building = buildingPositionEntry.Key;
			building.transform.position = buildingPositionEntry.Value;
		}
		foreach (KeyValuePair<GameObject, float> buildingAngleEntry in buildingsInitAngle) {
			GameObject building = buildingAngleEntry.Key;
			Quaternion buildingRotation = building.transform.rotation;
			building.transform.rotation = building.transform.rotation = Quaternion.Euler (buildingRotation.x, buildingAngleEntry.Value, buildingRotation.z);
		}

		foreach (KeyValuePair<GameObject, Vector3> wallPositionEntry in wallsInitPos) {
			GameObject wall = wallPositionEntry.Key;
			wall.transform.position = wallPositionEntry.Value;
		}
		foreach (KeyValuePair<GameObject, float> wallAngleEntry in wallsInitAngle) {
			GameObject wall = wallAngleEntry.Key;
			Quaternion wallRotation = wall.transform.rotation;
			wall.transform.rotation = wall.transform.rotation = Quaternion.Euler (wallRotation.x, wallAngleEntry.Value, wallRotation.z);
		}

		this.CleanHistory ();
	}

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