using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

public class SkinChangingEditor : ObjectEditor {
	private int selectedBuildingStartHeight;

	private GameObject skinPanel;
	private SkinPanelController skinPanelController;

	private GameObject materialsPanel;
	private GameObject colorsPanel;

	private GameObject materialsScrollBar;
	private GameObject colorsScrollBar;

	private GameObject materialsButton;
	private GameObject colorsButton;

	private FloorColorController topFloorColorController;
	private FloorColorController bottomFloorColorController;

	public SkinChangingEditor(GameObject skinPanel) {
		this.selectedBuildingStartHeight = -1;

		this.skinPanel = skinPanel;
		this.skinPanel.AddComponent<SkinPanelController>();
		this.skinPanel.SetActive(false);

		this.skinPanelController = this.skinPanel.GetComponent<SkinPanelController>();

		int i = 0;
		for (; i < skinPanel.transform.childCount && !skinPanel.transform.GetChild(i).name.Equals(UiNames.SKIN_SLIDER); i++) ;

		if (i < skinPanel.transform.childCount) {
			GameObject skinSlider = skinPanel.transform.GetChild(i).gameObject;

			this.materialsPanel = this.ChildSkinElement(skinSlider, UiNames.MATERIALS_PANEL);
			this.colorsPanel = this.ChildSkinElement(skinSlider, UiNames.COLORS_PANEL);

			this.materialsScrollBar = this.ChildSkinElement(skinSlider, UiNames.MATERIALS_SCROLLBAR);
			this.colorsScrollBar = this.ChildSkinElement(skinSlider, UiNames.COLORS_SCROLLBAR);

			this.materialsButton = this.ChildSkinElement(skinSlider, UiNames.MATERIALS_BUTTON);
			this.colorsButton = this.ChildSkinElement(skinSlider, UiNames.COLORS_BUTTON);
		}


		RectTransform editPanelTransform = (RectTransform) this.skinPanelController.transform;
		this.skinPanelController.StartPosX = editPanelTransform.localPosition.x - editPanelTransform.rect.width;
		this.skinPanelController.EndPosX = editPanelTransform.localPosition.x;

		Vector3 panelPosition = this.skinPanel.transform.localPosition;
		this.skinPanel.transform.localPosition = new Vector3(this.skinPanelController.StartPosX, panelPosition.y, panelPosition.z);
	}

	private GameObject ChildSkinElement(GameObject containter, string elementName) {
		int i = 0;
		for (; i < containter.transform.childCount && !containter.transform.GetChild(i).name.Equals(elementName); i++) ;

		if (i < containter.transform.childCount)
			return containter.transform.GetChild(i).gameObject;
		else
			return null;
	}

	public void InitializeSkinChangingMode() {
		skinPanel.SetActive(true);
		skinPanelController.OpenPanel(null);
	}

	public void UpdateButtons(GameObject buttonToLight) {
		GameObject buttonToDark = null;
		if (buttonToLight.name.Equals(UiNames.MATERIALS_BUTTON))
			buttonToDark = GameObject.Find(UiNames.COLORS_BUTTON);
		else if (buttonToLight.name.Equals(UiNames.COLORS_BUTTON))
			buttonToDark = GameObject.Find(UiNames.MATERIALS_BUTTON);
		else
			throw new Exception("Error in buttons naming : button \"" + buttonToLight.name + "\" not found.");

		RectTransform lightButtonRect = buttonToLight.GetComponent<RectTransform>();
		RectTransform darkButtonRect = buttonToDark.GetComponent<RectTransform>();

		// Erreur car le bouton est désactivé et donc introuvable en passant par le "find"

		lightButtonRect.sizeDelta = new Vector2(lightButtonRect.sizeDelta.x, 30);
		darkButtonRect.sizeDelta = new Vector2(darkButtonRect.sizeDelta.x, 25);

		Button lightButtonComponent = buttonToLight.GetComponent<Button>();
		Button shadowButtonComponent = buttonToDark.GetComponent<Button>();

		lightButtonComponent.interactable = false;
		shadowButtonComponent.interactable = true;
		shadowButtonComponent.OnPointerExit (null);
	}

	public override void ValidateTransform() {
		throw new NotImplementedException();
	}

	public override void CancelTransform() {
		throw new NotImplementedException();
	}

	public GameObject SkinPanel {
		get { return skinPanel; }
		set { skinPanel = value; }
	}

	public SkinPanelController SkinPanelController {
		get { return skinPanelController; }
		set { skinPanelController = value; }
	}
}