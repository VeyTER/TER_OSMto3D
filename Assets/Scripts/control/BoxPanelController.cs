using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

/// <summary>
///		Contrôle les panneaux de type "boîte" (rectangulaires).
/// </summary>
public class BoxPanelController : PanelController {
	private void Start() {
		this.panelState = PanelStates.CLOSED;
		this.gameObject.SetActive(false);
	}

	public override void OpenPanel(Action finalAction) {
		if (panelState == PanelStates.CLOSED) {
			panelState = PanelStates.CLOSED_TO_OPEN;
			this.StartCoroutine(this.MoveBox(1, finalAction));
		}
	}

	public override void ClosePanel(Action finalAction) {
		if (panelState == PanelStates.OPEN) {
			panelState = PanelStates.OPEN_TO_CLOSED;
			this.StartCoroutine(this.MoveBox(-1, finalAction));
		}
	}

	private IEnumerator MoveBox(int direction, Action finalAction) {
		Vector3 initPosition = Vector3.zero;
		Vector3 targetPosition = Vector3.zero;

		float initPanelScale = 0;
		float targetPanelScale = 0;

		float initCloseButtonDeltaFactor = 0;
		float targetCloseButtonDeltaFactor = 0;

		if (direction > 0) {
			initPosition = StartPosition;
			targetPosition = EndPosition;

			initPanelScale = 0.75F;
			targetPanelScale = 1;

			initCloseButtonDeltaFactor = 0;
			targetCloseButtonDeltaFactor = 20;
		} else if (direction < 0) {
			initPosition = endPosition;
			targetPosition = startPosition;

			initPanelScale = 1;
			targetPanelScale = 0.75F;

			initCloseButtonDeltaFactor = 20;
			targetCloseButtonDeltaFactor = 0;
		}

		Image visibilitylPanelImage = GetComponent<Image>();
		Color visibilitylPanelImageColor = visibilitylPanelImage.color;

		GameObject createBuildingButton = transform.parent.gameObject;

		GameObject createBuildingIcon = createBuildingButton.transform.Find(UiNames.CREATE_BUILDING_ICON).gameObject;
		Image buildingCreationIconImage = createBuildingIcon.GetComponent<Image>();
		Color buildingCreationIconColor = buildingCreationIconImage.color;

		GameObject validateBuildingCreationButton = createBuildingButton.transform.Find(UiNames.VALIDATE_BUILDING_CREATION_BUTTON).gameObject;
		Vector3 validateButtonPosition = validateBuildingCreationButton.transform.localPosition;
		Image validateButtonImage = validateBuildingCreationButton.GetComponent<Image>();
		Color validateButtonColor = validateButtonImage.color;

		GameObject cancelBuildingCreationButton = createBuildingButton.transform.Find(UiNames.CANCEL_BUILDING_CREATION_BUTTON).gameObject;
		Vector3 cancelButtonPosition = cancelBuildingCreationButton.transform.localPosition;
		Image cancelButtonImage = cancelBuildingCreationButton.GetComponent<Image>();
		Color cancelButtonColor = cancelButtonImage.color;

		if (panelState == PanelStates.CLOSED_TO_OPEN) {
			validateBuildingCreationButton.SetActive(true);
			cancelBuildingCreationButton.SetActive(true);
		}

		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentAlpha = 0;
			float currentPanelScale = initPanelScale + (targetPanelScale - initPanelScale) * cursor;
			float currentCloseButtonDeltaFactor = initCloseButtonDeltaFactor + (targetCloseButtonDeltaFactor - initCloseButtonDeltaFactor) * cursor;

			if (direction > 0)
				currentAlpha = cursor;
			else if (direction < 0)
				currentAlpha = 1 - cursor;

			visibilitylPanelImageColor.a = currentAlpha;
			visibilitylPanelImage.color = visibilitylPanelImageColor;

			transform.localScale = new Vector3(currentPanelScale, currentPanelScale, currentPanelScale);

			createBuildingIcon.transform.localPosition = Vector3.Lerp(initPosition, targetPosition, cursor);
			Vector3 positionOffset = new Vector3(0, currentCloseButtonDeltaFactor, 0);
			validateBuildingCreationButton.transform.localPosition = Vector3.Lerp(initPosition + positionOffset, targetPosition + positionOffset, cursor);
			cancelBuildingCreationButton.transform.localPosition = Vector3.Lerp(initPosition - positionOffset, targetPosition - positionOffset, cursor);

			buildingCreationIconColor.a = 1 - currentAlpha;
			buildingCreationIconImage.color = buildingCreationIconColor;

			validateButtonColor.a = currentAlpha;
			validateButtonImage.color = validateButtonColor;

			cancelButtonColor.a = currentAlpha;
			cancelButtonImage.color = cancelButtonColor;

			this.FadeInHierarchy(gameObject, currentAlpha);

			yield return new WaitForSeconds(0.01F);
		}

		if (direction > 0) {
			panelState = PanelStates.OPEN;
		} else if (direction < 0) {
			validateBuildingCreationButton.SetActive(false);
			cancelBuildingCreationButton.SetActive(false);
			panelState = PanelStates.CLOSED;
		}

		if (finalAction != null)
			finalAction();
	}

	private void FadeInHierarchy(GameObject parentElement, float alpha) {
		foreach (Transform childElementTransform in parentElement.transform) {
			Image elementImage = childElementTransform.GetComponent<Image>();
			Text elementText = childElementTransform.GetComponent<Text>();

			if (elementImage != null) {
				Color elementImageColor = elementImage.color;
				elementImageColor.a = alpha;

				if (childElementTransform.name.Contains("Input"))
					elementImageColor.a = alpha * 0.6F;
				else
					elementImageColor.a = alpha;

				elementImage.color = elementImageColor;
			}

			if (elementText != null) {
				Color elementTextColor = elementText.color;

				if(childElementTransform.name.Contains("Placeholder"))
					elementTextColor.a = alpha / 2F;
				else
					elementTextColor.a = alpha;

				elementText.color = elementTextColor;
			}

			this.FadeInHierarchy(childElementTransform.gameObject, alpha);
		}
	}
}