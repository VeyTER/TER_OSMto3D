using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class WheelPanelController : PanelController {
	private void Start() {
		this.panelState = PanelStates.CLOSED;
		this.gameObject.SetActive(false);
		this.BuildWheel();
	}

	private void BuildWheel() {
		RectTransform wheelRect = this.GetComponent<RectTransform>();
		RectTransform closeWheelButtonRect = transform.Find(UiNames.CLOSE_VISIBILITY_WHEEL_BUTTON).GetComponent<RectTransform>();

		float remoteness = wheelRect.sizeDelta.x / 2F - closeWheelButtonRect.sizeDelta.x / 2F - 3;
		float angleStep = 360 / ((transform.childCount - 1) * 1F);

		float currentAngle = 0;
		for (int i = 0; i < transform.childCount - 1; i++) {
			float localPosX = (float) (Math.Cos(currentAngle * Mathf.Deg2Rad) * remoteness);
			float localPosY = (float) (Math.Sin(currentAngle * Mathf.Deg2Rad) * remoteness);

			GameObject wheelSwitch = transform.GetChild(i).gameObject;
			wheelSwitch.transform.localPosition = new Vector3(localPosX + wheelRect.sizeDelta.x / 2F, localPosY, 0);

			currentAngle += angleStep;
		}
	}

	public void DisableButton(GameObject switcher) {
		switcher.transform.GetChild(0).gameObject.SetActive(true);
		switcher.transform.GetChild(1).gameObject.SetActive(false);
	}

	public void EnableButton(GameObject switcher) {
		switcher.transform.GetChild(0).gameObject.SetActive(false);
		switcher.transform.GetChild(1).gameObject.SetActive(true);
	}

	public void DisableButton(string switchName) {
		GameObject matchingSwitch = this.GetSwitchButton(switchName);

		if (matchingSwitch != null) {
			matchingSwitch.transform.GetChild(0).gameObject.SetActive(true);
			matchingSwitch.transform.GetChild(1).gameObject.SetActive(false);
		}
	}

	public void EnableButton(string switchName) {
		GameObject matchingSwitch = this.GetSwitchButton(switchName);

		matchingSwitch.transform.GetChild(0).gameObject.SetActive(false);
		matchingSwitch.transform.GetChild(1).gameObject.SetActive(true);
	}

	private GameObject GetSwitchButton(string switchName) {
		int i = 0;
		for (; i < transform.childCount - 1 && !transform.GetChild(i).name.Equals(switchName); i++) ;

		if (i < transform.childCount - 1)
			return transform.GetChild(i).gameObject;
		else
			return null;
	}

	public override void OpenPanel(Action finalAction) {
		if (panelState == PanelStates.CLOSED) {
			panelState = PanelStates.CLOSED_TO_OPEN;
			this.StartCoroutine(this.MoveWheel(1, finalAction));
		}
	}

	public override void ClosePanel(Action finalAction) {
		if (panelState == PanelStates.OPEN) {
			panelState = PanelStates.OPEN_TO_CLOSED;
			this.StartCoroutine(this.MoveWheel(-1, finalAction));
		}
	}

	private IEnumerator MoveWheel(int direction, Action finalAction) {
		Vector3 initPosition = Vector3.zero;
		Vector3 targetPosition = Vector3.zero;

		float initAngle = 0;
		float targetAngle = 0;

		if (direction > 0) {
			initPosition = StartPosition;
			targetPosition = EndPosition;

			initAngle = 45;
			targetAngle = 0;
		} else if (direction < 0) {
			initPosition = endPosition;
			targetPosition = startPosition;

			initAngle = 0;
			targetAngle = 45;
		}

		Image visibilitylPanelImage = GetComponent<Image>();
		Color visibilitylPanelImageColor = visibilitylPanelImage.color;

		GameObject toggleVisibilityButton = transform.parent.gameObject;
		GameObject toggleVisibilityIcon = toggleVisibilityButton.transform.Find(UiNames.TOGGLE_VISIBILITY_ICON).gameObject;
		Image visibilityIconImage = toggleVisibilityIcon.GetComponent<Image>();
		Color visibilityIconColor = visibilityIconImage.color;

		int childCount = transform.childCount - 1;
		GameObject closeVisibilityWheelButton = transform.Find(UiNames.CLOSE_VISIBILITY_WHEEL_BUTTON).gameObject;
		Image closeButtonImage = closeVisibilityWheelButton.GetComponent<Image>();
		Color closeButtonImageColor = closeButtonImage.color;

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentAlpha = 0;
			float currentAngle = initAngle + (targetAngle - initAngle) * cursor;

			if (direction > 0)
				currentAlpha = cursor;
			else if (direction < 0)
				currentAlpha = 1 - cursor;

			visibilitylPanelImageColor.a = currentAlpha;
			visibilitylPanelImage.color = visibilitylPanelImageColor;

			closeButtonImage.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
			closeButtonImageColor.a = currentAlpha;
			closeButtonImage.color = closeButtonImageColor;

			toggleVisibilityButton.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
			toggleVisibilityButton.transform.localPosition = Vector3.Lerp(initPosition, targetPosition, cursor);

			toggleVisibilityIcon.transform.rotation = Quaternion.Euler(0, 0, currentAngle - 45);
			visibilityIconColor.a = 1 - currentAlpha;
			visibilityIconImage.color = visibilityIconColor;

			this.FadeInHierarchy(currentAlpha);

			yield return new WaitForSeconds(0.01F);
		}

		if (direction > 0)
			panelState = PanelStates.OPEN;
		else if (direction < 0)
			panelState = PanelStates.CLOSED;

		if (finalAction != null)
			finalAction();
	}

	private void FadeInHierarchy(float alpha) {
		for (int i = 0; i < transform.childCount - 1; i++) {
			GameObject switchButton = transform.GetChild(i).gameObject;
			switchButton.transform.rotation = Quaternion.Euler(0, 0, 0);

			Image disabledButtonImage = switchButton.transform.GetChild(0).GetComponent<Image>();
			Image enabledButtonImage = switchButton.transform.GetChild(1).GetComponent<Image>();

			Color disabledButtonImageColor = disabledButtonImage.color;
			Color enabledButtonImageColor = disabledButtonImage.color;

			disabledButtonImageColor.a = alpha;
			enabledButtonImageColor.a = alpha;

			disabledButtonImage.color = disabledButtonImageColor;
			enabledButtonImage.color = disabledButtonImageColor;
		}
	}
}
