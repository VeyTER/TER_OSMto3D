using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 	<para>
/// 		Gère l'état de la caméra en fonction des actions de l'utilisateur. Elle peut se trouver dans 3 modes
/// 		différents :
/// 	</para>
/// 	<para></para>
/// 	<para>
/// 		FREE : La caméra peut être dépacée librement par l'utilisateur avec les touches du clavier.
/// 	</para>
/// 	<para></para>
/// 	<para>
/// 		FLYING : La caméra est en train de se déplacer vers un objectif, elle n'est plus controllable par
/// 		l'utilisateur.
/// 	</para><para>
/// 	</para>
/// 	<para>
/// 		FIXED : La caméra est immobile à une certaine position et n'est pas controllable par l'utilisateur.
/// 	</para>
/// </summary>
public class CameraController : MonoBehaviour {
	/// <summary>
	/// 	Les différents états dans laquelle peut se trouver la caméra (détaillés dans la description de la classe).
	/// </summary>
	public enum CameraStates { FREE, FLYING, FIXED, CIRCULARY_CONSTRAINED }

	/// <summary> Etat courant dans lequel se trouve la caméra. </summary>
	private CameraStates cameraState;

	private BuildingsTools buildingsTools;

	/// <summary>
	/// 	Position initiale de la caméra avant déplacement et immobilisation à une situation donnée.
	/// </summary>
	private Vector3 initPosition;

	/// <summary>
	/// 	Orientation initiale de la caméra avant déplacement et immobilisation à une situation donnée.
	/// </summary>
	private Quaternion initRotation;


	private GameObject targetBuilding;


	public void Start() {
		this.cameraState = CameraStates.FREE;

		this.buildingsTools = BuildingsTools.GetInstance();

		this.initPosition = Vector3.zero;
		this.initRotation.eulerAngles = Vector3.zero;

		this.targetBuilding = null;
	}


	public void Update () {
		Vector3 localPosition = transform.localPosition;
		Quaternion localRoation = transform.localRotation;

		// Contrôle de la caméra avec le touches du clavier
		if (cameraState == CameraStates.FREE) {

			// Rotation de la caméra
			if (Input.GetKey(KeyCode.LeftControl)) {

				if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
					transform.Rotate(new Vector3(-1, 0, 0));

				if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
					transform.Rotate(new Vector3(1, 0, 0));

				if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
					transform.RotateAround(localPosition, Vector3.up, -1);

				if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
					transform.RotateAround(localPosition, Vector3.up, 1);

				// Déplacement vertical de la caméra
			} else if (Input.GetKey(KeyCode.LeftShift)) {
				if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
					transform.localPosition = new Vector3(localPosition.x, localPosition.y + 0.1F, localPosition.z);

				if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
					transform.localPosition = new Vector3(localPosition.x, localPosition.y - 0.1F, localPosition.z);

				// Déplacement horizontal de la caméra
			} else {
				float cameraAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
				float cosOffset = 0.1F * (float) Math.Cos(cameraAngle);
				float sinOffset = 0.1F * (float) Math.Sin(cameraAngle);

				if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
					transform.localPosition = new Vector3(localPosition.x + sinOffset, localPosition.y, localPosition.z + cosOffset);

				if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
					transform.localPosition = new Vector3(localPosition.x - sinOffset, localPosition.y, localPosition.z - cosOffset);

				if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
					transform.localPosition = new Vector3(localPosition.x - cosOffset, localPosition.y, localPosition.z + sinOffset);

				if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
					transform.localPosition = new Vector3(localPosition.x + cosOffset, localPosition.y, localPosition.z - sinOffset);
			}
		} else if (cameraState == CameraStates.CIRCULARY_CONSTRAINED) {
			if (targetBuilding != null) {
				Vector3 buildingPosition = targetBuilding.transform.position;

				float horizontalOrientation = this.RelativeOrientation(targetBuilding);
				float distance = Vector2.Distance(new Vector2(buildingPosition.x, buildingPosition.z), new Vector2(localPosition.x, localPosition.z));

				if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
					horizontalOrientation += 5F * Mathf.Deg2Rad;

				if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
					horizontalOrientation -= 5F * Mathf.Deg2Rad;

				transform.localPosition = this.RelativePosition(targetBuilding, horizontalOrientation, localRoation.eulerAngles.x);
				transform.localRotation = Quaternion.Euler(localRoation.eulerAngles.x, localRoation.y - horizontalOrientation * Mathf.Rad2Deg + 90, localRoation.eulerAngles.z);
			}
		}
	}


	/// <summary>
	/// 	Déplace et oriente la caméra à une certaine situation en effectuant une action lorsque la tâche est
	/// 	terminée.
	/// </summary>
	/// <returns>Temporisateur servant à générer une animation.</returns>
	/// <param name="targetPosition">Position à atteindre pour la caméra.</param>
	/// <param name="targetRotation">Orientation à atteindre pour la caméra.</param>
	/// <param name="finalAction">Action finale à effectuer à la fin du déplacement.</param>
	public IEnumerator MoveToSituation(Vector3 targetPosition, Quaternion targetRotation, Action finalAction) {
		cameraState = CameraStates.FLYING;

		// Enregistrement de la situation initale de la caméra
		Vector3 initPosition = transform.position;
		Quaternion initRotation = transform.rotation;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float)Math.Sin (i * (Math.PI) / 2F);

			// Calcul de la situation courante
			Vector3 cameraCurrentPosition = Vector3.Lerp (initPosition, targetPosition, cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (initRotation, targetRotation, cursor);

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		// Affectation de la situation finale pour éviter les imprécisions
		transform.position = targetPosition;
		transform.rotation = targetRotation;

		targetBuilding = null;

		cameraState = CameraStates.FREE;

		// Appel de la tâche finale s'il y en a une
		if(finalAction != null)
			finalAction ();
	}


	/// <summary>
	/// 	Déplace et oriente la caméra au-dessus d'un building en prenant en compte les dimensions de celui-ci.
	/// </summary>
	/// <returns>Temporisateur servant à générer une animation.</returns>
	/// <param name="building">Bâtiment au-dessus duquel se positionner.</param>
	/// <param name="finalAction">Action finale à effectuer à la fin du déplacement.</param>
	public IEnumerator MoveToBuilding(GameObject building, bool champTo, Action finalAction, float orientation = 90) {
		cameraState = CameraStates.FLYING;

		Vector3 startPosition = transform.position;
		Quaternion startRotation = transform.rotation;

		Vector3 targetPosition = this.RelativePosition(building, 0, orientation);
		Quaternion targetRotation = Quaternion.Euler (new Vector3 (orientation, 90, 0));

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin (i * (Math.PI) / 2F);

			// Calcul de la situation courante
			Vector3 cameraCurrentPosition = Vector3.Lerp (startPosition, new Vector3(targetPosition.x, targetPosition.y, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp (startRotation, targetRotation, cursor);

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds (0.01F);
		}

		// Affectation de la situation finale pour éviter les imprécisions
		transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
		transform.rotation = targetRotation;

		targetBuilding = building;

		if (champTo)
			cameraState = CameraStates.CIRCULARY_CONSTRAINED;
		else
			cameraState = CameraStates.FIXED;

		// Appel de la tâche finale s'il y en a une
		if(finalAction != null)
			finalAction ();
	}

	public void TeleportToBuilding(GameObject building, bool champTo, float horizontalOrientation, float verticalOrientation = 90) {
		transform.position = this.RelativePosition(building, horizontalOrientation, verticalOrientation);
		transform.rotation = Quaternion.Euler(new Vector3(verticalOrientation, transform.rotation.eulerAngles.y, 0));

		if (champTo)
			cameraState = CameraStates.CIRCULARY_CONSTRAINED;
		else
			cameraState = CameraStates.FIXED;
	}

	private Vector3 RelativePosition(GameObject building, float horizontalOrientation, float verticalOrientation) {
		Vector3 res = Vector3.zero;

		GameObject firstWall = building.transform.GetChild(0).gameObject;

		// Un peu de trigo pour pouvoir être à la bonne hauteur par rapport au à la taille du bâtiment
		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = firstWall.transform.localScale.y;
		double buildingRadius = buildingsTools.BuildingRadius(building);
		float targetPosZ = (float) (buildingHeight + (buildingRadius / Math.Tan(cameraFOV)));

		// Calcul de la position à adopter par rapport à l'orientation de la caméra
		double horizontalShift = (targetPosZ - buildingHeight) * Math.Cos(verticalOrientation * Mathf.Deg2Rad);
		double verticalShift = (targetPosZ - buildingHeight) * Math.Sin(verticalOrientation * Mathf.Deg2Rad);

		Vector3 buildingPosition = building.transform.position;
		Vector3 localPosition = transform.position;

		float cosOffset = (float) (Math.Cos(horizontalOrientation) * horizontalShift);
		float sinOffset = (float) (Math.Sin(horizontalOrientation) * horizontalShift);

		// Enregistrement de la situation à atteindre
		Vector3 buildingCenterPosition = buildingsTools.BuildingCenter(building);
		res = new Vector3(buildingCenterPosition.x - cosOffset, buildingHeight + (float) verticalShift, buildingCenterPosition.z - sinOffset);

		return res;
	}

	public float RelativeOrientation(GameObject building) {
		Vector3 localPosition = transform.localPosition;
		Vector3 buildingPosition = building.transform.position;
		float res = (float) (Math.Atan2(buildingPosition.z - localPosition.z, buildingPosition.x - localPosition.x));
		return res;
	}

	public CameraStates CameraState {
		get { return cameraState; }
		set { cameraState = value; }
	}

	public Vector3 InitPosition {
		get { return initPosition; }
		set { initPosition = value; }
	}

	public Quaternion InitRotation {
		get { return initRotation; }
		set { initRotation = value; }
	}
}
