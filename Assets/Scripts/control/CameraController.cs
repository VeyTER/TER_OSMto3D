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
	public enum CameraStates { FREE, FLYING, FIXED, CIRCULARY_CONSTRAINED, TURNING_AROUND }

	/// <summary> Etat courant dans lequel se trouve la caméra. </summary>
	private CameraStates cameraState;
	private CameraStates stateBeforeTuringAround;

	public enum ControlModes { GLOBAL, SEMI_LOCAL, FULL_LOCAL }
	private ControlModes controlMode;

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
		this.stateBeforeTuringAround = CameraStates.CIRCULARY_CONSTRAINED;

		this.controlMode = ControlModes.FULL_LOCAL;

		this.buildingsTools = BuildingsTools.GetInstance();

		this.initPosition = Vector3.zero;
		this.initRotation.eulerAngles = Vector3.zero;

		this.targetBuilding = null;
	}


	public void Update () {
		Vector3 localPosition = transform.localPosition;
		Vector3 position = transform.position;

		Quaternion localRoation = transform.localRotation;

		float cameraHorizontalAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
		float cameraVecticalAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.x;

		float sinVecticalFactor = (float) Math.Sin(cameraVecticalAngle);

		float cosHorizontalOffset = 0.1F * (float) Math.Cos(cameraHorizontalAngle) * (sinVecticalFactor > 0 ? (1 - sinVecticalFactor) : (sinVecticalFactor + 1));
		float sinHorizontalOffset = 0.1F * (float) Math.Sin(cameraHorizontalAngle) * (sinVecticalFactor > 0 ? (1 - sinVecticalFactor) : (sinVecticalFactor + 1));
		float sinVecticalOffset = 0.1F * sinVecticalFactor;

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

			} else if (Input.GetKey(KeyCode.LeftShift)) {
				if (controlMode == ControlModes.GLOBAL || controlMode == ControlModes.SEMI_LOCAL) {
					if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
						transform.localPosition = new Vector3(localPosition.x, localPosition.y + 0.1F, localPosition.z);
					if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
						transform.localPosition = new Vector3(localPosition.x, localPosition.y - 0.1F, localPosition.z);
				}/* else if (controlMode == ControlModes.FULL_LOCAL) {
					if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
						transform.localPosition = new Vector3(localPosition.x + sinVecticalOffset, localPosition.y + cosHorizontalOffset, localPosition.z - sinHorizontalOffset);
					if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
						transform.localPosition = new Vector3(localPosition.x - sinVecticalOffset, localPosition.y - cosHorizontalOffset, localPosition.z + sinHorizontalOffset);
				}*/
			} else {

				switch (controlMode) {
				case ControlModes.GLOBAL:
					if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
						transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z + 0.1F);
					if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
						transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z - 0.1F);
					if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
						transform.localPosition = new Vector3(localPosition.x + 0.1F, localPosition.y, localPosition.z);
					if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
						transform.localPosition = new Vector3(localPosition.x - 0.1F, localPosition.y, localPosition.z);
					break;
				case ControlModes.SEMI_LOCAL:
					if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
						transform.localPosition = new Vector3(localPosition.x + sinHorizontalOffset, localPosition.y, localPosition.z + cosHorizontalOffset);
					if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
						transform.localPosition = new Vector3(localPosition.x - sinHorizontalOffset, localPosition.y, localPosition.z - cosHorizontalOffset);
					if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
						transform.localPosition = new Vector3(localPosition.x - cosHorizontalOffset, localPosition.y, localPosition.z + sinHorizontalOffset);
					if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
						transform.localPosition = new Vector3(localPosition.x + cosHorizontalOffset, localPosition.y, localPosition.z - sinHorizontalOffset);
					break;
				case ControlModes.FULL_LOCAL:
					if (Input.GetKey("up") || Input.GetKey(KeyCode.Z))
						transform.position = new Vector3(position.x + sinHorizontalOffset, position.y - sinVecticalOffset, position.z + cosHorizontalOffset);
					if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
						transform.position = new Vector3(position.x - sinHorizontalOffset, position.y + sinVecticalOffset, position.z - cosHorizontalOffset);
					if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
						transform.position = new Vector3(localPosition.x - cosHorizontalOffset, localPosition.y, localPosition.z + sinHorizontalOffset);
					if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
						transform.position = new Vector3(position.x + cosHorizontalOffset, position.y, position.z - sinHorizontalOffset);
					break;
				}
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

	public IEnumerator TurnAroundBuilding(GameObject building, float verticalOrientation) {
		stateBeforeTuringAround = cameraState;
		cameraState = CameraStates.TURNING_AROUND;

		float horizontalOrientation = this.RelativeOrientation(building);
		while (cameraState == CameraStates.TURNING_AROUND) {
			horizontalOrientation += 1 * Mathf.Deg2Rad;

			Vector3 cameraCurrentPosition = this.RelativePosition(building, horizontalOrientation, verticalOrientation);
			Quaternion cameraCurrentRotation = Quaternion.Euler(new Vector3(verticalOrientation, -horizontalOrientation * Mathf.Rad2Deg + 90, 0));

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds(0.01F);
		}
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
		return (float) (Math.Round(Math.Atan2(buildingPosition.z - localPosition.z, buildingPosition.x - localPosition.x), 3));
	}

	public void StopTurningAround() {
		cameraState = stateBeforeTuringAround;
	}

	public void SwitchToGlobalMode() {
		controlMode = ControlModes.GLOBAL;
	}

	public void SwitchToSemiLocalMode() {
		controlMode = ControlModes.SEMI_LOCAL;
	}

	public void SwitchToFullLocalMode() {
		controlMode = ControlModes.FULL_LOCAL;
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
