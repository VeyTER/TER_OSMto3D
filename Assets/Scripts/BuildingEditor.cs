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

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private GameObject selectedWall;
	private GameObject selectedBuilding;
	
	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	private GameObject moveHandler;
	private Vector2 moveHandlerInitPosition;
	private Vector2 moveHandlerInitOffset;

	public void Start() {
		editionState = EditionStates.NONE_SELECTION;
		selectionRange = SelectionRanges.WALL;

		objectBuilder = ObjectBuilder.GetInstance ();
		buildingsTools = BuildingsTools.GetInstance ();

		moveHandler = GameObject.Find(UINames.MOVE_HANDLER);
		moveHandler.SetActive (false);
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
				this.StartCoroutine ("OpenPanel");
			}

			InputField[] textInputs = GameObject.FindObjectsOfType<InputField> ();

			int i = 0;
			for (; i < textInputs.Length && textInputs[i].name.Equals (UINames.BUILDING_NAME_TEXT_INPUT); i++);

			if (i < textInputs.Length)
				textInputs[i].text = identifier;

			this.ChangeBuildingsColor ();
			selectedBuilding = selectedWall.transform.parent.gameObject;
		}

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

	public IEnumerator OpenPanel() {
		Vector3 panelPosition = Main.panel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)Main.panel.transform;

		float initPosition = panelPosition.x;
		float targetPosX = panelPosition.x - panelRectTransform.rect.width;

		Main.panel.SetActive (true);

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			float currentPosX = initPosition - (initPosition - targetPosX) * cursor;
			Main.panel.transform.localPosition = new Vector3 (currentPosX, panelPosition.y, panelPosition.z);

			yield return new WaitForSeconds (0.01F);
		}
	}

	// ATTENTION : BUG SI ON RE-OUVRE ALORS QU'IL SE FERME
	public IEnumerator ClosePanel(Action finalAction) {
		Vector3 panelPosition = Main.panel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)Main.panel.transform;

		float initPosition = panelPosition.x;
		float targetPosX = panelPosition.x + panelRectTransform.rect.width;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			float currentPosX = initPosition - (initPosition - targetPosX) * cursor;
			Main.panel.transform.localPosition = new Vector3 (currentPosX, panelPosition.y, panelPosition.z);

			yield return new WaitForSeconds (0.01F);
		}

		if(finalAction != null)
			finalAction ();
	}

	public void EnterMovingMode() {
		editionState = EditionStates.MOVING_MODE;
		this.StartCoroutine (
			this.ClosePanel(null)
		);
		moveHandler.SetActive (true);
	}

	public void StartMoving() {
		moveHandlerInitPosition = moveHandler.transform.position;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		moveHandlerInitOffset = mousePosition - moveHandlerInitPosition;
	}
	public void UpdateMoving() {
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