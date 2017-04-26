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
				Main.panel.SetActive (true);
				Text buildingNameLabel = GameObject.Find ("Nom batiment").GetComponent<Text> ();
				buildingNameLabel.text = identifier;

				this.ChangeBuildingsColor ();
				buildingsTools.SelectedBuilding = transform.parent.gameObject;
			}

			if (editionState == EditionStates.NONE_SELECTION/* && EventSystem.current.IsPointerOverGameObject()*/) {
				GameObject mainCamera = Camera.main.gameObject;
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
		GameObject mainCamera = Camera.main.gameObject;
		GameObject building = transform.parent.gameObject;

		Vector3 cameraPosition = mainCamera.transform.position;
		Quaternion cameraRotation = mainCamera.transform.rotation;

		cameraInitPosition = new Vector3 (cameraPosition.x, cameraPosition.y, cameraPosition.z);
		cameraInitRotation = new Quaternion (cameraRotation.x, cameraRotation.y, cameraRotation.z, cameraRotation.w);

		Vector3 targetPosition = buildingsTools.BuildingCenter (building);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (90, 90, 0));

		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = building.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius (building);
		float cameraHeight = (float) (buildingHeight + buildingRadius / Math.Tan (cameraFOV)) * 0.8F;

//		GameObject cercle = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
//		cercle.transform.position = new Vector3(buildingsTools.BuildingCenter (building).x, -0.4F, buildingsTools.BuildingCenter (building).z);
//		cercle.transform.localScale = new Vector3 ((float) buildingsTools.BuildingRadius(building) * 2, 0.5F, (float) buildingsTools.BuildingRadius(building) * 2);

		editionState = EditionStates.MOVING_TO_BUILDING;

		for (double i = 0; i <= 1; i += 0.1F) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (cameraInitPosition, new Vector3(targetPosition.x, cameraHeight, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (cameraInitRotation, targetRotation, cursor);

			mainCamera.transform.position = cameraCurrentPosition;
			mainCamera.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCamera.transform.position = new Vector3(targetPosition.x, cameraHeight, targetPosition.z);
		mainCamera.transform.rotation = targetRotation;

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
		GameObject mainCamera = Camera.main.gameObject;
		GameObject building = transform.parent.gameObject;

		Vector3 buildingPosition = mainCamera.transform.position;
		Quaternion buildingRotation = mainCamera.transform.rotation;

		Vector3 targetPosition = cameraInitPosition;
		Quaternion targetRotation = cameraInitRotation;

		editionState = EditionStates.MOVING_TO_INITIAL_SITUATION;

		for (double i = 0; i <= 1; i += 0.1F) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			Vector3 cameraCurrentPosition = Vector3.Lerp (buildingPosition, targetPosition, cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (buildingRotation, targetRotation, cursor);

			mainCamera.transform.position = cameraCurrentPosition;
			mainCamera.transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		mainCamera.transform.position = targetPosition;
		mainCamera.transform.rotation = targetRotation;

		editionState = EditionStates.NONE_SELECTION;
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