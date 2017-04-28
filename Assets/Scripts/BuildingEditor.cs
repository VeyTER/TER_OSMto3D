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

	public enum PanelStates { CLOSED, CLOSED_TO_OPEN, OPEN, OPEN_TO_CLOSED }

	private EditionStates editionState;
	private SelectionRanges selectionRange;
	private PanelStates panelState;

	private GameObject selectedWall;
	private GameObject selectedBuilding;
	
	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitPosition;
	private Vector2 moveHandlerInitOffset;

	private GameObject validateEditionButton;
	private GameObject cancelEditionButton;

	public void Start() {
		editionState = EditionStates.NONE_SELECTION;
		selectionRange = SelectionRanges.WALL;

		objectBuilder = ObjectBuilder.GetInstance ();
		buildingsTools = BuildingsTools.GetInstance ();

		moveHandler = GameObject.Find(UINames.MOVE_HANDLER);
		moveHandler.SetActive (false);

		validateEditionButton = GameObject.Find(UINames.VALDIATE_EDITION_BUTTON);
		cancelEditionButton = GameObject.Find(UINames.CANCEL_EDITION_BUTTON);

		validateEditionButton.SetActive (false);
		cancelEditionButton.SetActive (false);

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

	// ATTENTION : BUG SI ON RE-OUVRE ALORS QU'IL SE FERME ==> AJOUTER UNE MAE POUR LE PANNEAU (OUVERT, EN_FERMETURE, FERME, EN_OUVERTURE)
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

	public void EnterMovingMode() {
		editionState = EditionStates.MOVING_MODE;
		this.ClosePanel (null);
		moveHandler.SetActive (true);
	}

	public void StartMovingBuilding() {
		moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;
	}
	public void UpdateMovingBuilding() {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandler.transform.position = mousePosition - moveHandlerInitOffset;
	}

	public EditionStates EditionState {
		get { return editionState; }
		set { editionState = value; }
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