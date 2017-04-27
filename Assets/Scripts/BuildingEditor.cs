using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildingEditor : MonoBehaviour, IPointerUpHandler  {
	private enum EditionStates { NONE_SELECTION, MOVING_TO_BUILDING, READY_TO_EDIT, RENAMING, TRANSLATING, TURNING, VERTICAL_SCALING, MOVING_TO_INITIAL_SITUATION }
	private enum SelectionRanges { WALL, BUILDING }

	private EditionStates editionState;
	private SelectionRanges selectionRange;

	private ObjectBuilder objectBuilder;
	private BuildingsTools buildingsTools;

	private Vector3 cameraInitPosition;
	private Quaternion cameraInitRotation;

	public void Start() {
		this.editionState = EditionStates.NONE_SELECTION;
		this.selectionRange = SelectionRanges.WALL;

		this.objectBuilder = ObjectBuilder.GetInstance ();
		this.buildingsTools = BuildingsTools.GetInstance ();
	}

	public void OnPointerUp(PointerEventData eventData) {
		Debug.Log("Dans OnPointerUp de BuildingEditor");
	}

	public void OnMouseUp() {
		if (tag.Equals (NodeTags.WALL_TAG) && !EventSystem.current.IsPointerOverGameObject ()) {

			// Récupération du bâtiment correspondant au mur sélectionné
			BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
			NodeGroup buildingNgp = buildingsTools.WallToBuildingNodeGroup (this.gameObject);

			string identifier = name.Substring (0, name.LastIndexOf ("_"));

			// Si le bâtiment n'a pas de nom défini, ajouter un prefixe dans son affichage
			double parsedValue = 0;
			if (double.TryParse (identifier, out parsedValue))
				identifier = "Bâtiment n°" + identifier;

			if (buildingNgp != null) {
				// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment

				if (Main.panel.activeInHierarchy == false)
					this.StartCoroutine ("SlidePanelLeft");
				//				93.7


				InputField[] textInputs = GameObject.FindObjectsOfType<InputField> ();

				int i = 0;
				for (; i < textInputs.Length && textInputs[i].name.Equals (UINames.BUILDING_NAME_TEXT_INPUT); i++);

				if (i < textInputs.Length)
					textInputs[i].text = identifier;

				this.ChangeBuildingsColor ();
				buildingsTools.SelectedBuilding = transform.parent.gameObject;
			}

			if (editionState == EditionStates.NONE_SELECTION) {
				GameObject mainCameraGo = Camera.main.gameObject;
				UIManager UIManager = FindObjectOfType<UIManager> ();
				UIManager.BuildingEditor = this;
				this.StartCoroutine ("MoveToBuilding");
			}
		}
	}

	/// <summary>
	/// Change la couleur du bâtiment pointé.
	/// </summary>
	private void ChangeBuildingsColor () {
		GameObject buildingGo = transform.parent.gameObject;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.DiscolorAll ();
		buildingsTools.ColorAsSelected (buildingGo);
	}

	public IEnumerator MoveToBuilding() {
		GameObject mainCameraGo = Camera.main.gameObject;
		GameObject building = transform.parent.gameObject;

		Vector3 cameraPosition = mainCameraGo.transform.position;
		Quaternion cameraRotation = mainCameraGo.transform.rotation;

		cameraInitPosition = new Vector3 (cameraPosition.x, cameraPosition.y, cameraPosition.z);
		cameraInitRotation = new Quaternion (cameraRotation.x, cameraRotation.y, cameraRotation.z, cameraRotation.w);

		Vector3 targetPosition = buildingsTools.BuildingCenter (building);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = building.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius (building);
		float cameraHeight = (float) (buildingHeight + buildingRadius / Math.Tan (cameraFOV)) * 0.8F;

		editionState = EditionStates.MOVING_TO_BUILDING;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (cameraInitPosition, new Vector3(targetPosition.x, cameraHeight, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (cameraInitRotation, targetRotation, cursor);

			mainCameraGo.transform.position = cameraCurrentPosition;
			mainCameraGo.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCameraGo.transform.position = new Vector3(targetPosition.x, cameraHeight, targetPosition.z);
		mainCameraGo.transform.rotation = targetRotation;

		this.GetPreviousInitSituation ();

		editionState = EditionStates.READY_TO_EDIT;
	}

	private void GetPreviousInitSituation() {
		GameObject wallGroups = objectBuilder.WallGroups;
		BuildingEditor[] buildingEditors = wallGroups.GetComponentsInChildren<BuildingEditor> ();
		foreach (BuildingEditor buildingEditor in buildingEditors) {
			if (buildingEditor.InUse ()) {
				cameraInitPosition = buildingEditor.CameraInitPosition;
				cameraInitRotation = buildingEditor.CameraInitRotation;
				buildingEditor.setInactive ();
			}
		}
	}

	public IEnumerator MoveToInitSituation() {
		GameObject mainCameraGo = Camera.main.gameObject;
		GameObject building = transform.parent.gameObject;

		Vector3 buildingPosition = mainCameraGo.transform.position;
		Quaternion buildingRotation = mainCameraGo.transform.rotation;

		Vector3 targetPosition = cameraInitPosition;
		Quaternion targetRotation = cameraInitRotation;

		editionState = EditionStates.MOVING_TO_INITIAL_SITUATION;

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

		editionState = EditionStates.NONE_SELECTION;
	}

	public IEnumerator SlidePanelLeft() {
		Vector3 panelPosition = Main.panel.transform.localPosition;
		Main.panel.transform.localPosition = new Vector3 (-93.7F, panelPosition.y, panelPosition.z);

		Main.panel.SetActive (true);

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			float currentPosX = (93.7F * 2) * cursor - 93.7F;
			Main.panel.transform.localPosition = new Vector3 (currentPosX, panelPosition.y, panelPosition.z);

			yield return new WaitForSeconds (0.01F);
		}
	}

	// ATTENTION : BUG SI ON RE-OUVRE ALORS QU4IL SE FERME !!!
	public IEnumerator SlidePanelRight() {
		Vector3 panelPosition = Main.panel.transform.localPosition;
		Main.panel.transform.position = new Vector3 (-93.7F, panelPosition.y, panelPosition.z);

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			float currentPosX = 93.7F - (93.7F * 2) * cursor;
			Main.panel.transform.localPosition = new Vector3 (currentPosX, panelPosition.y, panelPosition.z);

			yield return new WaitForSeconds (0.01F);
		}

		Main.panel.SetActive (false);
	}

	public bool InUse() {
		return editionState != EditionStates.NONE_SELECTION;
	}

	public void setInactive() {
		editionState = EditionStates.NONE_SELECTION;
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