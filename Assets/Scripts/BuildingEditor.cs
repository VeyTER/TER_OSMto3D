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
	public enum TurningStates { MOTIONLESS, TURNING}

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private CameraStates cameraState;

	private PanelStates panelState;

	private MovingStates movingState;
	private TurningStates turningState;

	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private ArrayList editedObjects;
	private ArrayList turnedObjects;

	private GameObject selectedWall;
	private GameObject selectedBuilding;

	private Vector3 selectedWallInitPos;
	private float selectedWallInitAngle;

	private Vector3 selectedBuildingInitPos;
	private float selectedBuildingInitAngle;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitOffset;

	private GameObject turnHandler;
	private float turnHandlerInitOffset;

	private GameObject slidePanelButton;
	private GameObject validateEditionButton;
	private GameObject cancelEditionButton;

	private GameObject wallRangeButton;
	private GameObject buildingRangeButton;

	private GameObject lateralPanel;

	public void Start() {
		this.editionState = EditionStates.NONE_SELECTION;
		this.selectionRange = SelectionRanges.BUILDING;

		this.cameraState = CameraStates.FREE;
		this.panelState = PanelStates.CLOSED;

		this.movingState = MovingStates.MOTIONLESS;
		this.turningState = TurningStates.MOTIONLESS;

		this.objectBuilder = ObjectBuilder.GetInstance ();
		this.buildingsTools = BuildingsTools.GetInstance ();

		this.editedObjects = new ArrayList ();
		this.turnedObjects = new ArrayList ();

		this.moveHandler = GameObject.Find(UINames.MOVE_HANDLER);
		this.moveHandler.SetActive (false);

		this.turnHandler = GameObject.Find(UINames.TURN_HANDLER);
		this.turnHandler.SetActive (false);

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

	private IEnumerator MoveToBuilding(Action finalAction) {
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

	public void ExitBuilding() {
		selectedBuilding = null;
		selectedWall = null;

		editionState = BuildingEditor.EditionStates.MOVING_TO_INITIAL_SITUATION;
		cameraState = BuildingEditor.CameraStates.FLYING;

		buildingsTools.DiscolorAllBuildings ();

		this.StartCoroutine (
			this.MoveToInitSituation(() => {
				editionState = BuildingEditor.EditionStates.NONE_SELECTION;
				cameraState = BuildingEditor.CameraStates.FREE;
			})
		);
		this.ClosePanel (() => {
			lateralPanel.SetActive (false);
		});
	}

	private IEnumerator MoveToInitSituation(Action finalAction) {
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

	public void EnterMovingMode() {
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleFloattingButtons");
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

		if (selectionRange == SelectionRanges.WALL)
			selectedWallInitPos = objectPosition;
		else if (selectionRange == SelectionRanges.BUILDING)
			selectedBuildingInitPos = objectPosition;
	}

	public void ExitMovingMode() {
		moveHandler.SetActive (false);

		if(panelState == PanelStates.CLOSED)
			this.OpenPanel (null);

		this.StartCoroutine ("ToggleFloattingButtons");

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraState = CameraStates.FLYING;

		this.StartCoroutine (this.MoveToBuilding (() => {
				editionState = EditionStates.READY_TO_EDIT;
				cameraState = CameraStates.FIXED;
			})
		);
	}

	public void StartObjectMoving() {
		Vector2 moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;

		this.MoveObject ();

		movingState = MovingStates.MOVING;
	}

	public void UpdateObjectMoving() {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerInitOffset;
		this.MoveObject ();
	}

	private void MoveObject() {
		Camera mainCamera = Camera.main;
		Vector3 moveHandlerPosition = moveHandler.transform.position;

		Vector3 selectedObjectCurrentPos = mainCamera.ScreenToWorldPoint(new Vector3(moveHandlerPosition.x, moveHandlerPosition.y, mainCamera.transform.position.y));
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




















	public void EnterTurningMode() {
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleFloattingButtons");
		turnHandler.SetActive (true);
		editionState = EditionStates.TURNING_MODE;
	}

	public void InitialiseTurningMode() {
		Camera mainCamera = Camera.main;

		Vector3 objectPosition = Vector3.zero;
		float objectAngle = 0;
		Vector3 objectScale = Vector3.zero;

		if (selectionRange == SelectionRanges.WALL) {
			objectPosition = selectedWall.transform.position;
			objectAngle = selectedWall.transform.rotation.y;
			objectScale = selectedWall.transform.localScale;
		} else if (selectionRange == SelectionRanges.BUILDING) {
			objectPosition = selectedBuilding.transform.position;
			objectAngle = selectedBuilding.transform.rotation.y;
			objectScale = selectedBuilding.transform.localScale;
		}

		Vector3 objectScreenPosition = mainCamera.WorldToScreenPoint (objectPosition);
		turnHandler.transform.position = new Vector3 (objectScreenPosition.x, objectScreenPosition.y, 0);

		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		if (selectionRange == SelectionRanges.WALL) {
			selectedWallInitAngle = objectAngle;
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z, 360 - selectedWall.transform.localRotation.eulerAngles.y); 
		} else if (selectionRange == SelectionRanges.BUILDING) {
			selectedBuildingInitAngle = objectAngle;
			turnHandler.transform.rotation = Quaternion.Euler (turnHandlerRotation.x, turnHandlerRotation.z,  360 - selectedBuilding.transform.rotation.eulerAngles.y); 
		}
	}

	public void ExitTurningMode() {
		turnHandler.SetActive (false);

		if(panelState == PanelStates.CLOSED)
			this.OpenPanel (null);

		this.StartCoroutine ("ToggleFloattingButtons");

		editionState = EditionStates.MOVING_TO_BUILDING;
		cameraState = CameraStates.FLYING;

		this.StartCoroutine (this.MoveToBuilding (() => {
				editionState = EditionStates.READY_TO_EDIT;
				cameraState = CameraStates.FIXED;
			})
		);
	}

	public void StartObjectTurning() {
		Vector3 turnHandlerPosition = turnHandler.transform.position;
		float turnHandlerAngle = 360 - selectedBuilding.transform.rotation.eulerAngles.y;

		Vector2 relativeMousePosition = new Vector2(turnHandlerPosition.x, turnHandlerPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI));

		turnHandlerInitOffset = mouseAngle - turnHandlerAngle * Mathf.Deg2Rad;

		this.TurnObject ();

		turningState = TurningStates.TURNING;
	}

	public void UpdateObjectTurning() {
		Vector3 turnHandlerPosition = turnHandler.transform.position;

		Vector2 relativeMousePosition = new Vector2(turnHandlerPosition.x, turnHandlerPosition.y) - new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		float mouseAngle = (float)((2 * Math.PI) - (Math.Atan2 (relativeMousePosition.x, relativeMousePosition.y) + Math.PI));

		Quaternion turnHandlerRotation = turnHandler.transform.rotation;
		turnHandler.transform.rotation = Quaternion.Euler(turnHandlerRotation.eulerAngles.x, turnHandlerRotation.eulerAngles.y, (mouseAngle - turnHandlerInitOffset) * Mathf.Rad2Deg);

		this.TurnObject ();
	}

	private void TurnObject() {
		Camera mainCamera = Camera.main;
		float turnHandlerAngle = 360 - turnHandler.transform.rotation.eulerAngles.z;

		if (selectionRange == SelectionRanges.WALL) {
			Quaternion selectedWallRotation = selectedWall.transform.rotation;
			selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.eulerAngles.x, turnHandlerAngle, selectedWallRotation.eulerAngles.z);
		} else if (selectionRange == SelectionRanges.BUILDING) {
			Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
			selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.eulerAngles.x, turnHandlerAngle, selectedBuildingRotation.eulerAngles.z);
		}
	}

	public void EndObjectTurning() {
		turningState = TurningStates.MOTIONLESS;
	}
























	public void ValidateEdit() {
		if (selectionRange == SelectionRanges.WALL && editedObjects.Contains(selectedWall))
			editedObjects.Add (selectedWall);

		if (selectionRange == SelectionRanges.BUILDING && editedObjects.Contains(SelectedBuilding))
			editedObjects.Add (selectedBuilding);
	}

	public void CancelEdit() {
		if (selectionRange == SelectionRanges.WALL) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedWall.transform.position = selectedWallInitPos;
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedWallRotation = selectedWall.transform.rotation;
				selectedWall.transform.rotation = Quaternion.Euler (selectedWallRotation.eulerAngles.x, selectedWallInitAngle * Mathf.Rad2Deg, selectedWallRotation.eulerAngles.z);
			}
		}

		if (selectionRange == SelectionRanges.BUILDING) {
			if (editionState == EditionStates.MOVING_MODE) {
				selectedBuilding.transform.position = selectedBuildingInitPos;
			} else if (editionState == EditionStates.TURNING_MODE) {
				Quaternion selectedBuildingRotation = selectedBuilding.transform.rotation;
				selectedBuilding.transform.rotation = Quaternion.Euler (selectedBuildingRotation.eulerAngles.x, selectedBuildingInitAngle * Mathf.Rad2Deg, selectedBuildingRotation.eulerAngles.z);
			}
		}
	}

	public EditionStates EditionState {
		get { return editionState; }
		set { editionState = value; }
	}

	public MovingStates MovingState {
		get { return movingState; }
		set { movingState = value; }
	}

	public TurningStates TurningState {
		get { return turningState; }
		set { turningState = value; }
	}

	public SelectionRanges SelectionRange {
		get { return selectionRange; }
		set { selectionRange = value; }
	}

	public CameraStates CameraState {
		get { return cameraState; }
		set { cameraState = value; }
	}

	public PanelStates PanelState {
		get { return panelState; }
		set { panelState = value; }
	}

	public ArrayList MovedObjects {
		get { return editedObjects; }
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