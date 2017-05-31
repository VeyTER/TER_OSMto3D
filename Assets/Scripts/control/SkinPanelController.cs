using UnityEngine;
using System.Collections;
using System;

public class SkinPanelController : PanelController {
	private enum SlidingStates { RIGHT, RIGHT_TO_LEFT, LEFT, LEFT_TO_RIGHT };
	private SlidingStates slidingState;

	private GameObject skinSlider;

	private void Awake() {
		this.skinSlider = GameObject.Find(UiNames.SKIN_SLIDER);
		this.slidingState = SlidingStates.RIGHT;
	}

	public void SlideSliderRight() {
		if (slidingState == SlidingStates.LEFT) {
			slidingState = SlidingStates.LEFT_TO_RIGHT;
			this.StartCoroutine(this.SlidePanel(1));
		}
	}

	public void SlideSliderLeft() {
		if (slidingState == SlidingStates.RIGHT) {
			slidingState = SlidingStates.RIGHT_TO_LEFT;
			this.StartCoroutine(this.SlidePanel(-1));
		}
	}

	private IEnumerator SlidePanel (int direction) {
		// Configuration courante du panneau
		Vector3 sliderPosition = skinSlider.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform) transform;

		// Position en X initiale puis ciblée du panneau
		float sliderInitPosX = sliderPosition.x;

		float sliderTargetPosX = sliderInitPosX;
		if (direction < 0)
			sliderTargetPosX = -panelRectTransform.rect.width;
		else if (direction > 0)
			sliderTargetPosX = 0;

		// Génération de l'animation
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			// Calcul et affectation de la situation courante
			float sliderCurrentPosX = sliderInitPosX - (sliderInitPosX - sliderTargetPosX) * cursor;
			skinSlider.transform.localPosition = new Vector3(sliderCurrentPosX, sliderPosition.y, sliderPosition.z);

			yield return new WaitForSeconds(0.01F);
		}

		skinSlider.transform.localPosition = new Vector3(sliderTargetPosX, sliderPosition.y, sliderPosition.z);

		if (slidingState == SlidingStates.LEFT_TO_RIGHT)
			slidingState = SlidingStates.RIGHT;
		else if (slidingState == SlidingStates.RIGHT_TO_LEFT)
			slidingState = SlidingStates.LEFT;
	}
}
