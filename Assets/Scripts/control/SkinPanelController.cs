using UnityEngine;
using System.Collections;
using System;

public class SkinPanelController : CommonPanelController {
	private enum SliderStates { RIGHT, RIGHT_TO_LEFT, LEFT, LEFT_TO_RIGHT };
	private SliderStates sliderState;

	private GameObject skinSlider;

	private void Awake() {
		this.skinSlider = GameObject.Find(UiNames.SKIN_SLIDER);
		this.sliderState = SliderStates.RIGHT;
	}

	public void SlideSliderRight() {
		if (sliderState == SliderStates.LEFT) {
			sliderState = SliderStates.LEFT_TO_RIGHT;
			this.StartCoroutine(this.SlideSlider(1));
		}
	}

	public void SlideSliderLeft() {
		if (sliderState == SliderStates.RIGHT) {
			sliderState = SliderStates.RIGHT_TO_LEFT;
			this.StartCoroutine(this.SlideSlider(-1));
		}
	}

	private IEnumerator SlideSlider(int direction) {
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

		if (sliderState == SliderStates.LEFT_TO_RIGHT)
			sliderState = SliderStates.RIGHT;
		else if (sliderState == SliderStates.RIGHT_TO_LEFT)
			sliderState = SliderStates.LEFT;
	}

	public bool IsMotionLess() {
		return sliderState == SliderStates.RIGHT || sliderState == SliderStates.LEFT;
	}
}