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
	private CameraStates previousState;
	private CameraStates currentState;

	public enum ControlModes { GLOBAL, SEMI_LOCAL, FULL_LOCAL }
	private ControlModes controlMode;

	private BuildingsTools buildingsTools;

	private bool[] positionLock;
	private bool[] rotationLock;

	/// <summary>
	/// 	Position initiale de la caméra avant déplacement et immobilisation à une situation donnée.
	/// </summary>
	private Vector3 initPosition;

	/// <summary>
	/// 	Orientation initiale de la caméra avant déplacement et immobilisation à une situation donnée.
	/// </summary>
	private Quaternion initRotation;

	private GameObject targetObject;
	private GameObject pivotPoint;

	public void Start() {
		this.previousState = CameraStates.FREE;
		this.currentState = CameraStates.FREE;

		this.controlMode = ControlModes.SEMI_LOCAL;

		this.buildingsTools = BuildingsTools.GetInstance();

		this.positionLock = new bool[3];
		for (int i = 0; i < positionLock.Length; i++)
			positionLock[i] = true;

		this.rotationLock = new bool[3];
		for (int i = 0; i < positionLock.Length; i++)
			rotationLock[i] = true;

		this.initPosition = Vector3.zero;
		this.initRotation.eulerAngles = Vector3.zero;

		this.targetObject = null;
		this.pivotPoint = null;
	}


	public void Update () {
		Vector3 localPosition = transform.localPosition;
		Vector3 position = transform.position;

		Quaternion localRoation = transform.localRotation;

		float cameraHorizontalAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
		float cameraVecticalAngle = Mathf.Deg2Rad * transform.rotation.eulerAngles.x;

		float sinVecticalFactor = (float) Math.Sin(cameraVecticalAngle);

		float rawCosHorizontalOffset = 0.1F * (float) Math.Cos(cameraHorizontalAngle);
		float rawSinHorizontalOffset = 0.1F * (float) Math.Sin(cameraHorizontalAngle);

		float cosHorizontalOffset = rawCosHorizontalOffset * (sinVecticalFactor > 0 ? (1 - sinVecticalFactor) : (sinVecticalFactor + 1));
		float sinHorizontalOffset = rawSinHorizontalOffset * (sinVecticalFactor > 0 ? (1 - sinVecticalFactor) : (sinVecticalFactor + 1));
		float sinVecticalOffset = 0.1F * sinVecticalFactor;

		// Contrôle de la caméra avec le touches du clavier
		if (currentState == CameraStates.FREE) {

			// Rotation de la caméra
			if (Input.GetKey(KeyCode.LeftControl)) {
				if ((Input.GetKey("up") || Input.GetKey(KeyCode.Z)) && positionLock[0])
					transform.Rotate(new Vector3(-1, 0, 0));
				if ((Input.GetKey("down") || Input.GetKey(KeyCode.S)) && positionLock[0])
					transform.Rotate(new Vector3(1, 0, 0));
				if ((Input.GetKey("left") || Input.GetKey(KeyCode.Q)) && positionLock[2])
					transform.RotateAround(localPosition, Vector3.up, -1);
				if ((Input.GetKey("right") || Input.GetKey(KeyCode.D)) && positionLock[2])
					transform.RotateAround(localPosition, Vector3.up, 1);

			} else if (Input.GetKey(KeyCode.LeftShift)) {
				if (controlMode == ControlModes.GLOBAL || controlMode == ControlModes.SEMI_LOCAL) {
					if ((Input.GetKey("up") || Input.GetKey(KeyCode.Z)) && positionLock[2])
						transform.localPosition = new Vector3(localPosition.x, localPosition.y + 0.1F, localPosition.z);
					if ((Input.GetKey("down") || Input.GetKey(KeyCode.S)) && positionLock[2])
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
					if ((Input.GetKey("up") || Input.GetKey(KeyCode.Z)) && positionLock[1])
						transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z + 0.1F);
					if ((Input.GetKey("down") || Input.GetKey(KeyCode.S)) && positionLock[1])
						transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z - 0.1F);
					if ((Input.GetKey("left") || Input.GetKey(KeyCode.Q)) && positionLock[0])
						transform.localPosition = new Vector3(localPosition.x + 0.1F, localPosition.y, localPosition.z);
					if ((Input.GetKey("right") || Input.GetKey(KeyCode.D)) && positionLock[0])
						transform.localPosition = new Vector3(localPosition.x - 0.1F, localPosition.y, localPosition.z);
					break;
				case ControlModes.SEMI_LOCAL:
					if ((Input.GetKey("up") || Input.GetKey(KeyCode.Z)) && positionLock[1])
						transform.position = new Vector3(position.x + rawSinHorizontalOffset, position.y, position.z + rawCosHorizontalOffset);
					if ((Input.GetKey("down") || Input.GetKey(KeyCode.S)) && positionLock[1])
						transform.position = new Vector3(position.x - rawSinHorizontalOffset, position.y, position.z - rawCosHorizontalOffset);
					if ((Input.GetKey("left") || Input.GetKey(KeyCode.Q)) && positionLock[0])
						transform.position = new Vector3(localPosition.x - rawCosHorizontalOffset, localPosition.y, localPosition.z + rawSinHorizontalOffset);
					if ((Input.GetKey("right") || Input.GetKey(KeyCode.D)) && positionLock[0])
						transform.position = new Vector3(position.x + rawCosHorizontalOffset, position.y, position.z - rawSinHorizontalOffset);
					break;
				case ControlModes.FULL_LOCAL:
					if ((Input.GetKey("up") || Input.GetKey(KeyCode.Z)) && positionLock[1])
						transform.position = new Vector3(position.x + sinHorizontalOffset, position.y - sinVecticalOffset, position.z + cosHorizontalOffset);
					if ((Input.GetKey("down") || Input.GetKey(KeyCode.S)) && positionLock[1])
						transform.position = new Vector3(position.x - sinHorizontalOffset, position.y + sinVecticalOffset, position.z - cosHorizontalOffset);
					if ((Input.GetKey("left") || Input.GetKey(KeyCode.Q)) && positionLock[0])
						transform.position = new Vector3(localPosition.x - rawCosHorizontalOffset, localPosition.y, localPosition.z + rawSinHorizontalOffset);
					if ((Input.GetKey("right") || Input.GetKey(KeyCode.D)) && positionLock[0])
						transform.position = new Vector3(position.x + rawCosHorizontalOffset, position.y, position.z - rawSinHorizontalOffset);
					break;
				}
			}
		} else if (currentState == CameraStates.CIRCULARY_CONSTRAINED) {
			if (targetObject != null && pivotPoint != null) {
				Quaternion pivotRotation = pivotPoint.transform.rotation;

				if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
					pivotPoint.transform.rotation = Quaternion.Euler(pivotRotation.eulerAngles.x, pivotRotation.eulerAngles.y - 3, pivotRotation.eulerAngles.z);
				else if (Input.GetKey("left") || Input.GetKey(KeyCode.Q))
					pivotPoint.transform.rotation = Quaternion.Euler(pivotRotation.eulerAngles.x, pivotRotation.eulerAngles.y + 3, pivotRotation.eulerAngles.z);
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
		this.ChangeState(CameraStates.FLYING);

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

		targetObject = null;

		this.ChangeState(CameraStates.FREE);

		// Appel de la tâche finale s'il y en a une
		if(finalAction != null)
			finalAction ();
	}


	/// <summary>
	/// 	Déplace et oriente la caméra au-dessus d'un building en prenant en compte les dimensions de celui-ci.
	/// </summary>
	/// <returns>Temporisateur servant à générer une animation.</returns>
	/// <param name="targetBuilding">Bâtiment au-dessus duquel se positionner.</param>
	/// <param name="finalAction">Action finale à effectuer à la fin du déplacement.</param>
	public IEnumerator MoveToBuilding(GameObject targetBuilding, bool champTo, Action finalAction, float orientation = 90) {
		this.ChangeState(CameraStates.FLYING);

		Vector3 startPosition = transform.position;
		Quaternion startRotation = transform.rotation;

		Vector3 targetPosition = this.RelativePosition(targetBuilding, 0, orientation);
		Quaternion targetRotation = Quaternion.Euler(new Vector3(orientation, 90, 0));

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul de la situation courante
			Vector3 cameraCurrentPosition = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, targetPosition.y, targetPosition.z), cursor);
			Quaternion cameraCurrentRotation = Quaternion.Lerp(startRotation, targetRotation, cursor);

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			yield return new WaitForSeconds(0.01F);
		}

		// Affectation de la situation finale pour éviter les imprécisions
		transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
		transform.rotation = targetRotation;

		targetObject = targetBuilding;

		if (champTo)
			this.ChangeState(CameraStates.CIRCULARY_CONSTRAINED, targetBuilding);
		else
			this.ChangeState(CameraStates.FIXED);

		// Appel de la tâche finale s'il y en a une
		if (finalAction != null)
			finalAction();
	}

	public void TeleportToBuilding(GameObject targetBuilding, bool champTo, float horizontalOrientation, float verticalOrientation = 90) {
		if (currentState == CameraStates.CIRCULARY_CONSTRAINED)
			this.DisableCircularConstraintMode();

		transform.position = this.RelativePosition(targetBuilding, horizontalOrientation, verticalOrientation);
		transform.rotation = Quaternion.Euler(new Vector3(verticalOrientation, transform.rotation.eulerAngles.y, 0));

		if (champTo)
			ChangeState(CameraStates.CIRCULARY_CONSTRAINED, targetBuilding);
		else
			ChangeState(CameraStates.FIXED);
	}

	public IEnumerator TurnAroundBuilding(GameObject targetBuilding, float verticalOrientation) {
		this.ChangeState(CameraStates.TURNING_AROUND);

		const float MAX_SPEED = 0.35F;
		float rotationSpeed = MAX_SPEED / 1024F;

		float horizontalOrientation = (float) this.RelativeOrientation(targetBuilding);
		while (currentState == CameraStates.TURNING_AROUND) {
			horizontalOrientation += rotationSpeed * Mathf.Deg2Rad;

			Vector3 cameraCurrentPosition = this.RelativePosition(targetBuilding, horizontalOrientation, verticalOrientation);
			Quaternion cameraCurrentRotation = Quaternion.Euler(new Vector3(verticalOrientation, -horizontalOrientation * Mathf.Rad2Deg + 90, 0));

			transform.position = cameraCurrentPosition;
			transform.rotation = cameraCurrentRotation;

			if (rotationSpeed < MAX_SPEED)
				rotationSpeed *= 2F;

			yield return new WaitForSeconds(0.01F);
		}
	}

	private Vector3 RelativePosition(GameObject building, float horizontalOrientation, float verticalOrientation) {
		// Un peu de trigo pour pouvoir être à la bonne hauteur par rapport au à la taille du bâtiment
		float cameraFOV = Camera.main.fieldOfView;
		float buildingHeight = buildingsTools.BuildingHeight(building);
		float buildingRadius = (float) buildingsTools.BuildingRadius(building);
		float targetPosZ = (float) (buildingHeight + (buildingRadius / Math.Tan(cameraFOV)));

		const float MIN_HORIZONTAL_OFFSET = 0.5F;

		// Calcul de la position à adopter par rapport à l'orientation de la caméra
		float horizontalOffset = (float) ((targetPosZ - buildingHeight + MIN_HORIZONTAL_OFFSET) * Math.Cos(verticalOrientation * Mathf.Deg2Rad));
		float verticalOffset = (float) ((targetPosZ - buildingHeight) * Math.Sin(verticalOrientation * Mathf.Deg2Rad));

		Vector3 buildingPosition = building.transform.position;
		Vector3 localPosition = transform.position;

		float cosOffset = (float) (Math.Cos(horizontalOrientation) * horizontalOffset);
		float sinOffset = (float) (Math.Sin(horizontalOrientation) * horizontalOffset);

		// Enregistrement de la situation à atteindre
		Vector3 buildingCenterPosition = buildingsTools.BuildingCenter(building);
		return new Vector3(buildingCenterPosition.x - cosOffset, buildingHeight + verticalOffset + Camera.main.nearClipPlane, buildingCenterPosition.z - sinOffset);
	}

	public double RelativeOrientation(GameObject building) {
		Vector3 position = transform.position;
		Vector3 buildingPosition = building.transform.position;
		return Math.Atan2(buildingPosition.z - position.z, buildingPosition.x - position.x);
	}

	private void EnableCircularConstraintMode(GameObject target) {
		GameObject.Destroy(pivotPoint);
		pivotPoint = new GameObject("Camera Pivot");

		pivotPoint.transform.position = target.transform.position;
		transform.SetParent(pivotPoint.transform, true);
	}

	private void DisableCircularConstraintMode() {
		Vector3 parentedPosition = transform.position;
		Quaternion parentedRotation = transform.rotation;

		transform.SetParent(null, false);

		transform.position = parentedPosition;
		transform.rotation = parentedRotation;

		GameObject.Destroy(pivotPoint);
		pivotPoint = null;
	}

	public bool RestaurePreviousState() {
		if (currentState == CameraStates.CIRCULARY_CONSTRAINED)
			this.DisableCircularConstraintMode();

		bool noEffect = currentState == previousState;
		currentState = previousState;

		return !noEffect;
	}

	private void ChangeState(CameraStates nextState, GameObject target = null) {
		if (currentState == CameraStates.CIRCULARY_CONSTRAINED && pivotPoint != null)
			this.DisableCircularConstraintMode();

		if (nextState == CameraStates.CIRCULARY_CONSTRAINED)
			this.EnableCircularConstraintMode(target);

		previousState = currentState;
		currentState = nextState;
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
		get { return currentState; }
	}

	public Vector3 InitPosition {
		get { return initPosition; }
		set { initPosition = value; }
	}

	public Quaternion InitRotation {
		get { return initRotation; }
		set { initRotation = value; }
	}

	public bool[] TranslationLock {
		get { return positionLock; }
		set { positionLock = value; }
	}

	public bool[] RotationLock {
		get { return rotationLock; }
		set { rotationLock = value; }
	}
}
