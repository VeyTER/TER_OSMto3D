using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WheelPanelController : MonoBehaviour {
	private void Start() {
		this.BuildWheel();
	}

	private void BuildWheel() {
		RectTransform wheelTransform = this.GetComponent<RectTransform>();
		RectTransform closeWheelButtonTransform = transform.GetChild(transform.childCount - 1).GetComponent<RectTransform>();

		float remoteness = wheelTransform.sizeDelta.x / 2F - closeWheelButtonTransform.sizeDelta.x / 2F - 3;
		float angleStep = 360 / ((transform.childCount - 1) * 1F);

		float currentAngle = 0;
		for (int i = 0; i < transform.childCount - 1; i++) {
			float localPosX = (float)(Math.Cos(currentAngle * Mathf.Deg2Rad) * remoteness);
			float localPosY = (float)(Math.Sin(currentAngle * Mathf.Deg2Rad) * remoteness);

			Transform wheelSwitchTransform = transform.GetChild(i);
			wheelSwitchTransform.localPosition = new Vector3(localPosX + wheelTransform.sizeDelta.x / 2F, localPosY, 0);

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

	//public IEnumerator OpenWheel(int direction, Action finalAction) {

	//}

	//public IEnumerator CloseWheel(int direction, Action finalAction) {

	//}

	//public IEnumerator MoveWheel(int direction, Action finalAction) {

	//}
}
