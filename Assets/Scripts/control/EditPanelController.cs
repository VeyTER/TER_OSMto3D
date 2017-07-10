using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

public class EditPanelController : CommonPanelController {
	/// <summary>Bouton permettant de valider la transformation d'un bâtiment (déplacement par ex).</summary>
	protected GameObject validateEditButton;

	/// <summary>Bouton permettant d'annuler la transformation d'un bâtiment (déplacement par ex).</summary>
	protected GameObject cancelEditButton;

	public void Awake() {
		this.panelState = PanelStates.CLOSED;

		this.validateEditButton = GameObject.Find(UiNames.VALIDATE_EDIT_BUTTON);
		this.cancelEditButton = GameObject.Find(UiNames.CANCEL_EDIT_BUTTON);

		this.validateEditButton.transform.localScale = Vector3.zero;
		this.cancelEditButton.transform.localScale = Vector3.zero;
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
		Vector3 validateButtonInitScale = validateEditButton.transform.localScale;
		Vector3 cancelButtonInitScale = cancelEditButton.transform.localScale;

		// Calcul de l'échelle à atteindre à partir de l'échelle courante
		Vector3 targetScale = direction >= 0 ? Vector3.one : Vector3.zero;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul de la situation courante
			if (direction > 0) {
				validateEditButton.transform.localScale = new Vector3(cursor, cursor, cursor);
				cancelEditButton.transform.localScale = new Vector3(cursor, cursor, cursor);
			}  else {
				validateEditButton.transform.localScale = Vector3.one - new Vector3(cursor, cursor, cursor);
				cancelEditButton.transform.localScale = Vector3.one - new Vector3(cursor, cursor, cursor);
			}

			yield return new WaitForSeconds(0.01F);
		}

		// Affectation de l'échelle finale pour éviter les imprécisions
		validateEditButton.transform.localScale = targetScale;
		cancelEditButton.transform.localScale = targetScale;
	}
}