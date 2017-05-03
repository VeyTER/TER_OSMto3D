using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildingEditor : MonoBehaviour {
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

	public enum CameraStates { FREE, FIXED, FLYING}
	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	public enum MovingStates { MOTIONLESS, MOVING}

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private CameraStates cameraState;

	private PanelStates panelState;

	private MovingStates movingState;

	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private ArrayList movedObjects;
	private ArrayList turnedObjects;

	private GameObject selectedWall;
	private GameObject selectedBuilding;

	private Vector3 selectedWallInitPos;
	private Quaternion selectedWallInitRot;

	private Vector3 selectedBuildingInitPos;
	private Quaternion selectedBuildingInitRot;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitPosition;
	private Vector2 moveHandlerInitOffset;

	private GameObject slidePanelButton;
	private GameObject validateEditionButton;
	private GameObject cancelEditionButton;

	private GameObject wallRangeButton;
	private GameObject buildingRangeButton;

	private GameObject lateralPanel;

	private bool wallEdited;
	private bool buildingEdited;

	public void Start() {
		this.editionState = EditionStates.NONE_SELECTION;
		this.selectionRange = SelectionRanges.BUILDING;

		this.cameraState = CameraStates.FREE;
		this.panelState = PanelStates.CLOSED;

		this.movingState = MovingStates.MOTIONLESS;

		this.objectBuilder = ObjectBuilder.GetInstance ();
		this.buildingsTools = BuildingsTools.GetInstance ();

		this.movedObjects = new ArrayList ();
		this.turnedObjects = new ArrayList ();

		this.moveHandler = GameObject.Find(UINames.MOVE_HANDLER);
		this.moveHandler.SetActive (false);

		this.validateEditionButton = GameObject.Find(UINames.VALDIATE_EDITION_BUTTON);
		this.cancelEditionButton = GameObject.Find(UINames.CANCEL_EDITION_BUTTON);

		this.slidePanelButton = GameObject.Find (UINames.SLIDE_PANEL_BUTTON);
		this.validateEditionButton.transform.localScale = Vector3.zero;
		this.cancelEditionButton.transform.localScale = Vector3.zero;

		this.wallRangeButton = GameObject.Find(UINames.WALL_RANGE_BUTTON);
		this.buildingRangeButton = GameObject.Find(UINames.BUILDING_RANGE_BUTTON);

		this.wallRangeButton.transform.localScale = Vector3.zero;
		this.buildingRangeButton.transform.localScale = Vector3.zero;

		Button wallrangeButtonComponent = buildingRangeButton.GetComponent<Button> ();
		wallrangeButtonComponent.interactable = false;

		this.lateralPanel = GameObject.Find (UINames.LATERAL_PANEL);
		this.lateralPanel.SetActive(false);

		this.wallEdited = false;
		this.buildingEdited = false;
	}

	public void SwitchBuilding(GameObject selectedWall) {
		this.selectedWall = selectedWall;
		selectedBuilding = selectedWall.transform.parent.gameObject;

		// Récupération du bâtiment correspondant au mur sélectionné
		NodeGroup buildingNgp = buildingsTools.WallToBuildingNodeGroup (selectedWall);
		string identifier = selectedWall.name.Substring (0, selectedWall.name.LastIndexOf ("_"));

		// Si le bâtiment n'a pas de nom défini, ajouter un prefixe dans son affichage
		double parsedValue = 0;
		if (double.TryParse (identifier, out parsedValue))
			identifier = "Bâtiment n°" + identifier;

		if (lateralPanel.activeInHierarchy == false) {
			lateralPanel.SetActive (true);
			this.OpenPanel (null);
		}

		// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment
		buildingsTools.ChangeBuildingName(identifier);

		buildingsTools.DiscolorAllBuildings ();
		buildingsTools.ColorAsSelected (selectedBuilding);

		if (editionState == EditionStates.NONE_SELECTION) {
			GameObject mainCameraGo = Camera.main.gameObject;
			cameraInitPosition = mainCameraGo.transform.position;
			cameraInitRotation = mainCameraGo.transform.rotation;
		}

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraState = CameraStates.FLYING;

		this.StartCoroutine (
			this.MoveToBuilding(() => {
				editionState = EditionStates.READY_TO_EDIT;
				cameraState = CameraStates.FIXED;
			})
		);
	}

	public IEnumerator MoveToBuilding(Action finalAction) {
		GameObject mainCameraGo = Camera.main.gameObject;

		Vector3 cameraPosition = mainCameraGo.transform.position;
		Quaternion cameraRotation = mainCameraGo.transform.rotation;

		Vector3 targetPosition = selectedBuilding.transform.position;
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = selectedBuilding.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius (selectedBuilding);
		float cameraPosZ = (float) (buildingHeight + buildingRadius / Math.Tan (cameraFOV)) * 0.8F;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (cameraPosition, new Vector3(targetPosition.x, cameraPosZ, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (cameraRotation, targetRotation, cursor);

			mainCameraGo.transform.position = cameraCurrentPosition;
			mainCameraGo.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCameraGo.transform.position = new Vector3(targetPosition.x, cameraPosZ, targetPosition.z);
		mainCameraGo.transform.rotation = targetRotation;

		if(finalAction != null)
			finalAction ();
	}

	public IEnumerator MoveToInitSituation(Action finalAction) {
		GameObject mainCameraGo = Camera.main.gameObject;

		Vector3 buildingPosition = mainCameraGo.transform.position;
		Quaternion buildingRotation = mainCameraGo.transform.rotation;

		Vector3 targetPosition = cameraInitPosition;
		Quaternion targetRotation = cameraInitRotation;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (buildingPosition, targetPosition, cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (buildingRotation, targetRotation, cursor);

			mainCameraGo.transform.position = cameraCurrentPosition;
			mainCameraGo.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCameraGo.transform.position = targetPosition;
		mainCameraGo.transform.rotation = targetRotation;

		if(finalAction != null)
			finalAction ();
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
		if(panelState == PanelStates.CLOSED) {
			this.StartCoroutine ( this.SlidePanel(finalAction, -1) );
			panelState = PanelStates.CLOSED_TO_OPEN;
		}
	}

	public void ClosePanel(Action finalAction) {
		if(panelState == PanelStates.OPEN) {
			this.StartCoroutine ( this.SlidePanel(finalAction, 1) );
			panelState = PanelStates.OPEN_TO_CLOSED;
		}
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

	private IEnumerator ToggleEditionButtons() {
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

			if (validateButtonInitScale.x == 1) {
				validateButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
				cancelButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);

				wallRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
				buildingRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
			} else if (validateButtonInitScale.x == 0) {
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

	public void EnterMovingMode() {
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleEditionButtons");
		moveHandler.SetActive (true);
		editionState = EditionStates.MOVING_MODE;
	}

	public void InitialiseMovingMode() {
		Camera mainCamera = Camera.main;

		Vector3 objectPosition = Vector3.zero;
		Vector3 objectScale = Vector3.zero;

		if (selectionRange == SelectionRanges.WALL) {
			objectPosition = selectedWall.transform.position;
			objectScale = selectedWall.transform.localScale;
		} else if (selectionRange == SelectionRanges.BUILDING) {
			objectPosition = selectedBuilding.transform.position;
			objectScale = selectedBuilding.transform.localScale;
		}

		Vector3 objectScreenPosition = mainCamera.WorldToScreenPoint (objectPosition);
		moveHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);

		float objectHeight = objectScale.y;
		if (selectionRange == SelectionRanges.WALL)
			selectedWallInitPos = objectPosition;
		else if (selectionRange == SelectionRanges.BUILDING)
			selectedBuildingInitPos = objectPosition;
	}

	public void ExitMovingMode() {
		moveHandler.SetActive (false);

		this.OpenPanel (null);
		this.StartCoroutine ("ToggleEditionButtons");

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraState = CameraStates.FLYING;

		this.StartCoroutine (this.MoveToBuilding (() => {
				editionState = EditionStates.READY_TO_EDIT;
				cameraState = CameraStates.FIXED;
			})
		);
	}

	public void StartObjectMoving() {
		moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;

		wallEdited = selectionRange == SelectionRanges.WALL;
		buildingEdited = selectionRange == SelectionRanges.BUILDING;

		this.MoveObject ();

		movingState = MovingStates.MOVING;
	}

	public void UpdateObjectMoving() {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerInitOffset;
		this.MoveObject ();
	}

	private void MoveObject() {
		float objectHeight = -1;
		if (selectionRange == SelectionRanges.WALL)
			objectHeight = selectedWall.transform.localScale.y;
		else if(selectionRange == SelectionRanges.BUILDING)
			objectHeight = selectedBuilding.transform.localScale.y;

		Camera mainCamera = Camera.main;
		Vector3 modeHandlerPosition = moveHandler.transform.position;

		Vector3 selectedObjectCurrentPos = mainCamera.ScreenToWorldPoint(new Vector3(modeHandlerPosition.x, modeHandlerPosition.y, mainCamera.transform.position.y));
		if (selectionRange == SelectionRanges.WALL)
			selectedWall.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedWall.transform.position.y, selectedObjectCurrentPos.z);
		else if (selectionRange == SelectionRanges.BUILDING)
			selectedBuilding.transform.position = new Vector3 (selectedObjectCurrentPos.x, selectedBuilding.transform.position.y, selectedObjectCurrentPos.z);
	}

	public void EndObjectMoving() {
		movingState = MovingStates.MOTIONLESS;
	}

	public void ShiftCamera() {
		float cameraAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
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

	public void ValidateEdit() {
		if (wallEdited && movedObjects.Contains(selectedWall))
			movedObjects.Add (selectedWall);

		if (buildingEdited && movedObjects.Contains(SelectedBuilding))
			movedObjects.Add (selectedBuilding);

		wallEdited = false;
		buildingEdited = false;
	}

	public void CancelEdit() {
		if(wallEdited)
			selectedWall.transform.position = selectedWallInitPos;

		if(buildingEdited)
			selectedBuilding.transform.rotation = selectedBuildingInitRot;

		wallEdited = false;
		buildingEdited = false;
	}

	public EditionStates EditionState {
		get { return editionState; }
		set { editionState = value; }
	}

	public MovingStates MovingState {
		get { return movingState; }
		set { movingState = value; }
	}

	public SelectionRanges SelectionRange {
		get { return selectionRange; }
		set { selectionRange = value; }
	}

	public CameraStates CameraState {
		get { return cameraState; }
		set { cameraState = value; }
	}

	public ArrayList MovedObjects {
		get { return movedObjects; }
	}

	public ArrayList TurnedObjects {
		get { return turnedObjects; }
	}

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public Vector3 CameraInitPosition {
		get { return cameraInitPosition; }
		set { cameraInitPosition = value; }
	}

	public Quaternion CameraInitRotation {
		get { return cameraInitRotation; }
		set { cameraInitRotation = value; }
	}

	public GameObject LateralPanel {
		get { return lateralPanel; }
	}
}