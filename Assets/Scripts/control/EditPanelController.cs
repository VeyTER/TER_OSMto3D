using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

public class EditPanelController : CommonPanelController {
	/// <summary>Bouton permettant de valider la transformation d'un bâtiment (déplacement par ex).</summary>
	protected GameObject validateEditionButton;

	/// <summary>Bouton permettant d'annuler la transformation d'un bâtiment (déplacement par ex).</summary>
	protected GameObject cancelEditionButton;

	public void Awake() {
		this.panelState = PanelStates.CLOSED;

		this.validateEditionButton = GameObject.Find(UiNames.VALIDATE_EDITION_BUTTON);
		this.cancelEditionButton = GameObject.Find(UiNames.CANCEL_EDITION_BUTTON);

		this.validateEditionButton.transform.localScale = Vector3.zero;
		this.cancelEditionButton.transform.localScale = Vector3.zero;
	}

	public void OpenSlideButton() {
		if(panelState == PanelStates.CLOSED || panelState == PanelStates.CLOSED_TO_OPEN)
			this.StartCoroutine( this.SlidePanelButton(-1) );
	}

	public void CloseSlideButton() {
		if (panelState == PanelStates.OPEN || panelState == PanelStates.OPEN_TO_CLOSED)
			this.StartCoroutine( this.SlidePanelButton(1) );
	}

	private IEnumerator SlidePanelButton(int direction) {
		// Passage de la direction à -1 si elle est strictement négative et à 1 si elle est positive
		direction = direction > 0 ? 1 : -1;

		GameObject slideButton = GameObject.Find(UiNames.SLIDE_BUTTON);

		// Situation courante du bouton de contrôle du panneau
		Vector3 slideButtonPosition = slideButton.transform.localPosition;
		Quaternion slideButtonRotation = slideButton.transform.localRotation;

		// Position en X initiale puis ciblée du bouton de contrôle
		float slideButtonInitPosX = slideButtonPosition.x;
		float slideButtonTargetPosX = slideButtonPosition.x - (direction * 20);

		float slideButtonInitRotZ = slideButtonRotation.eulerAngles.z;
		float slideButtonTargetRotZ = slideButtonRotation.eulerAngles.z + (direction * 180);

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul de la situation courante
			float slideButtonCurrentPosX = slideButtonInitPosX - (slideButtonInitPosX - slideButtonTargetPosX) * cursor;
			float slideButtonCurrentRotZ = slideButtonInitRotZ - (slideButtonInitRotZ - slideButtonTargetRotZ) * cursor;

			slideButton.transform.localPosition = new Vector3(slideButtonCurrentPosX, slideButtonPosition.y, slideButtonPosition.z);
			slideButton.transform.localRotation = Quaternion.Euler(slideButtonRotation.x, slideButtonRotation.y, slideButtonCurrentRotZ);

			yield return new WaitForSeconds(0.01F);
		}

		slideButton.transform.localPosition = new Vector3(slideButtonTargetPosX, slideButtonPosition.y, slideButtonPosition.z);
		slideButton.transform.localRotation = Quaternion.Euler(slideButtonRotation.x, slideButtonRotation.y, slideButtonTargetRotZ);
	}

	public void OpenFloattingButtons() {
		this.StartCoroutine( this.ScaleFloattingButtons(1) );
	}

	public void CloseFloattingButtons() {
		this.StartCoroutine( this.ScaleFloattingButtons(-1) );
	}

	/// <summary>
	/// 	Inverse la visibilité des boutons flottants avec un effet de zoom.
	/// </summary>
	/// <returns>The floatting buttons.</returns>
	private IEnumerator ScaleFloattingButtons(int direction) {
		// Echelle courante des boutons de valdiation et d'annulation
		Vector3 validateButtonInitScale = validateEditionButton.transform.localScale;
		Vector3 cancelButtonInitScale = cancelEditionButton.transform.localScale;

		// Echelle courante des boutons d'étendue de mur et de bâtiment
		//		Vector3 wallRangeInitScale = wallRangeButton.transform.localScale;
		//		Vector3 buildingRangeInitScale = buildingRangeButton.transform.localScale;

		// Calcul de l'échelle à atteindre à partir de l'échelle courante
		Vector3 targetScale = direction >= 0 ? Vector3.one : Vector3.zero;

		Transform validateButtonTransform = validateEditionButton.transform;
		Transform cancelButtonTransform = cancelEditionButton.transform;

//		Transform wallRangeButtonTransform = wallRangeButton.transform;
//		Transform buildingRangeButtonTransform = buildingRangeButton.transform;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul de la situation courante
			if (direction > 0) {
				validateButtonTransform.localScale = new Vector3(cursor, cursor, cursor);
				cancelButtonTransform.localScale = new Vector3(cursor, cursor, cursor);

//				wallRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
//				buildingRangeButtonTransform.localScale = new Vector3 (cursor, cursor, cursor);
			}  else {
				validateButtonTransform.localScale = Vector3.one - new Vector3(cursor, cursor, cursor);
				cancelButtonTransform.localScale = Vector3.one - new Vector3(cursor, cursor, cursor);

//				wallRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
//				buildingRangeButtonTransform.localScale = Vector3.one - new Vector3 (cursor, cursor, cursor);
			}

			yield return new WaitForSeconds(0.01F);
		}

		// Affectation de l'échelle finale pour éviter les imprécisions
		validateButtonTransform.localScale = targetScale;
		cancelButtonTransform.localScale = targetScale;

//		wallRangeButtonTransform.localScale = targetScale;
//		buildingRangeButtonTransform.localScale = targetScale;
	}
}