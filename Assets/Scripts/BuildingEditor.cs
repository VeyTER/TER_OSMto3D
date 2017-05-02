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

	// TODO : Faire le même chose mais pour la caméra
	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	public enum MovingStates { MOTIONLESS, MOVING}

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private PanelStates panelState;
	private MovingStates movingState;

	private ArrayList movedObjects;
	private ArrayList turnedObjects;

	private GameObject selectedWall;
	private GameObject selectedBuilding;

	private Vector3 selectedWallInitPos;
	private Vector3 selectedWallCurentPos;
	private Quaternion selectedWallInitRot;
	private Quaternion selectedWallCurrentRot;

	private Vector3 selectedBuildingInitPos;
	private Vector3 selectedBuildingCurrentPos;
	private Quaternion selectedBuildingInitRot;
	private Quaternion selectedBuildingCurrentRot;

	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitPosition;
	private Vector2 moveHandlerInitOffset;

	private GameObject validateEditionButton;
	private GameObject cancelEditionButton;

	private GameObject wallRangeButton;
	private GameObject buildingRangeButton;

	private bool wallEdited;
	private bool buildingEdited;

	public void Start() {
		editionState = EditionStates.NONE_SELECTION;
		selectionRange = SelectionRanges.WALL;
		panelState = PanelStates.CLOSED;
		movingState = MovingStates.MOTIONLESS;

		objectBuilder = ObjectBuilder.GetInstance ();
		buildingsTools = BuildingsTools.GetInstance ();

		moveHandler = GameObject.Find(UINames.MOVE_HANDLER);
		moveHandler.SetActive (false);

		validateEditionButton = GameObject.Find(UINames.VALDIATE_EDITION_BUTTON);
		cancelEditionButton = GameObject.Find(UINames.CANCEL_EDITION_BUTTON);

		validateEditionButton.transform.localScale = Vector3.zero;
		cancelEditionButton.transform.localScale = Vector3.zero;

		wallRangeButton = GameObject.Find(UINames.WALL_RANGE_BUTTON);
		buildingRangeButton = GameObject.Find(UINames.BUILDING_RANGE_BUTTON);

		wallRangeButton.transform.localScale = Vector3.zero;
		buildingRangeButton.transform.localScale = Vector3.zero;

		Button wallrangeButtonComponent = buildingRangeButton.GetComponent<Button> ();
		wallrangeButtonComponent.interactable = false;

		movedObjects = new ArrayList ();
		turnedObjects = new ArrayList ();

		wallEdited = false;
		buildingEdited = false;

		Main.panel.SetActive(false);
	}

	public void ChangeBuilding(GameObject selectedWall) {
		this.selectedWall = selectedWall;

		// Récupération du bâtiment correspondant au mur sélectionné
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		NodeGroup buildingNgp = buildingsTools.WallToBuildingNodeGroup (selectedWall);

		string identifier = selectedWall.name.Substring (0, selectedWall.name.LastIndexOf ("_"));

		// Si le bâtiment n'a pas de nom défini, ajouter un prefixe dans son affichage
		double parsedValue = 0;
		if (double.TryParse (identifier, out parsedValue))
			identifier = "Bâtiment n°" + identifier;

		if (buildingNgp != null) {
			// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment

			if (Main.panel.activeInHierarchy == false) {
				Main.panel.SetActive (true);
				this.OpenPanel (null);
			}

			InputField[] textInputs = GameObject.FindObjectsOfType<InputField> ();

			int i = 0;
			for (; i < textInputs.Length && textInputs[i].name.Equals (UINames.BUILDING_NAME_TEXT_INPUT); i++);

			if (i < textInputs.Length)
				textInputs[i].text = identifier;

			this.ChangeBuildingsColor ();
			selectedBuilding = selectedWall.transform.parent.gameObject;

			GameObject mainCameraGo = Camera.main.gameObject;
			UIManager UIManager = FindObjectOfType<UIManager> ();

			if (editionState == EditionStates.NONE_SELECTION) {
				Vector3 cameraPosition = mainCameraGo.transform.position;
				Quaternion cameraRotation = mainCameraGo.transform.rotation;

				cameraInitPosition = new Vector3 (cameraPosition.x, cameraPosition.y, cameraPosition.z);
				cameraInitRotation = new Quaternion (cameraRotation.x, cameraRotation.y, cameraRotation.z, cameraRotation.w);
			}

			editionState = EditionStates.MOVING_TO_BUILDING;
			this.StartCoroutine (
				this.MoveToBuilding(() => {
					editionState = EditionStates.READY_TO_EDIT;
				})
			);
		}
	}

	/// <summary>
	/// Change la couleur du bâtiment pointé.
	/// </summary>
	public void ChangeBuildingsColor () {
		GameObject buildingGo = selectedWall.transform.parent.gameObject;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.DiscolorAll ();
		buildingsTools.ColorAsSelected (buildingGo);
	}

	public IEnumerator MoveToBuilding(Action finalAction) {
		GameObject mainCameraGo = Camera.main.gameObject;
		GameObject building = selectedWall.transform.parent.gameObject;

		Vector3 cameraPosition = mainCameraGo.transform.position;
		Quaternion cameraRotation = mainCameraGo.transform.rotation;

		Vector3 targetPosition = buildingsTools.BuildingCenter (building);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = building.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius (building);
		float cameraHeight = (float) (buildingHeight + buildingRadius / Math.Tan (cameraFOV)) * 0.8F;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (cameraPosition, new Vector3(targetPosition.x, cameraHeight, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (cameraRotation, targetRotation, cursor);

			mainCameraGo.transform.position = cameraCurrentPosition;
			mainCameraGo.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCameraGo.transform.position = new Vector3(targetPosition.x, cameraHeight, targetPosition.z);
		mainCameraGo.transform.rotation = targetRotation;

		if(finalAction != null)
			finalAction ();
	}

	public IEnumerator MoveToInitSituation(Action finalAction) {
		GameObject mainCameraGo = Camera.main.gameObject;
		GameObject buildingGo = selectedWall.transform.parent.gameObject;

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
			panelState = PanelStates.CLOSED_TO_OPEN;
			this.StartCoroutine ( this.SlidePanel (finalAction, -1) );
		} else if (panelState == PanelStates.OPEN) {
			panelState = PanelStates.OPEN_TO_CLOSED;
			this.StartCoroutine ( this.SlidePanel (finalAction, 1) );
		}
	}

	public void OpenPanel(Action finalAction) {
		if(panelState == PanelStates.CLOSED) {
			panelState = PanelStates.CLOSED_TO_OPEN;
			this.StartCoroutine ( this.SlidePanel(finalAction, -1) );
		}
	}

	public void ClosePanel(Action finalAction) {
		if(panelState == PanelStates.OPEN) {
			panelState = PanelStates.OPEN_TO_CLOSED;
			this.StartCoroutine ( this.SlidePanel(finalAction, 1) );
		}
	}

	private IEnumerator SlidePanel(Action finalAction, int direction) {
		direction = direction > 0 ? 1 : -1;

		Vector3 panelPosition = Main.panel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)Main.panel.transform;

		GameObject slidePanelButton = GameObject.Find (UINames.SLIDE_PANEL_BUTTON);
		Vector3 panelButtonPosition = slidePanelButton.transform.localPosition;
		Quaternion panelButtonRotation = slidePanelButton.transform.localRotation;

		float panelInitPosX = panelPosition.x;
		float targetPanelPosX = panelPosition.x + (direction * panelRectTransform.rect.width);

		float panelButtonInitPosX = panelButtonPosition.x;
		float targetPanelButtonPosX = panelButtonPosition.x - (direction * 20);

		float panelButtonInitRotZ = panelButtonRotation.eulerAngles.z;
		float targetPanelButtonRotZ = panelButtonRotation.eulerAngles.z + (direction * 180);

		Transform panelTransform = Main.panel.transform;
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
		editionState = EditionStates.MOVING_MODE;
		this.ClosePanel (null);
		this.StartCoroutine ("ToggleEditionButtons");

		Camera mainCamera = Camera.main;
		Vector3 buildingCenterPosition = buildingsTools.BuildingCenter (selectedBuilding);
		Vector3 buildingCenterScreenPosition = mainCamera.WorldToScreenPoint (buildingCenterPosition);

		moveHandler.transform.position = new Vector3 (buildingCenterScreenPosition.x, buildingCenterScreenPosition.y, 0);
		float buildingHeight = selectedBuilding.transform.localScale.y;
		selectedBuildingInitPos = mainCamera.ScreenToWorldPoint(new Vector3(buildingCenterScreenPosition.x, buildingCenterScreenPosition.y + buildingHeight, mainCamera.transform.position.y));
//		selectedBuildingInitPos = selectedBuilding.transform.localPosition;

		print (selectedBuildingInitPos);

		moveHandler.SetActive (true);
	}

	public void ExitMovingMode() {
		editionState = EditionStates.MOVING_TO_BUILDING;

		this.StartCoroutine (this.MoveToBuilding (() => {
				editionState = EditionStates.READY_TO_EDIT;
			})
		);

		this.OpenPanel (null);
		this.StartCoroutine ("ToggleEditionButtons");

		moveHandler.SetActive (false);
	}

	// TODO : Faire aussi pour les murs
	public void StartBuildingMoving() {
		moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;

		selectedBuildingCurrentPos = selectedBuildingInitPos;

		buildingEdited = true;

		movingState = MovingStates.MOVING;
	}

	public void UpdateBuildingMoving() {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerInitOffset;

		float buildingHeight = selectedBuilding.transform.localScale.y;
		Camera mainCamera = Camera.main;
		Vector3 modeHandlerPosition = moveHandler.transform.position;

		selectedBuildingCurrentPos = mainCamera.ScreenToWorldPoint(new Vector3(modeHandlerPosition.x, modeHandlerPosition.y, mainCamera.transform.position.y));
		selectedBuilding.transform.position = selectedBuildingCurrentPos - selectedBuildingInitPos;
	}

	public void EndBuildingMoving() {
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
		movedObjects.Add (selectedWall);
		movedObjects.Add (selectedBuilding);

		wallEdited = false;
		buildingEdited = false;
	}

	public void CancelEdit() {
		selectedBuilding.transform.position = selectedBuildingInitPos;
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
}