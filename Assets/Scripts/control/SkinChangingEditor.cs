using UnityEngine;
using UnityEditor;
using System;

public class SkinChangingEditor : ObjectEditor {
	private int selectedBuildingStartHeight;

	private GameObject skinPanel;

	private GameObject materialsPanel;
	private GameObject colorsPanel;

	private GameObject materialsScrollBar;
	private GameObject colorsScrollBar;

	private GameObject materialsButton;
	private GameObject colorsButton;

	private FloorColorController topFloorColorController;
	private FloorColorController bottomFloorColorController;

	private SkinPanelController skinPanelController;

	public SkinChangingEditor(GameObject skinPanel) {
		this.selectedBuildingStartHeight = -1;

		this.skinPanel = skinPanel;
		this.skinPanel.AddComponent<SkinPanelController>();

		this.materialsPanel = this.ChildSkinElement(UiNames.MATERIALS_PANEL);
		this.colorsPanel = this.ChildSkinElement(UiNames.COLORS_PANEL);

		this.materialsScrollBar = this.ChildSkinElement(UiNames.MATERIALS_SCROLLBAR);
		this.colorsScrollBar = this.ChildSkinElement(UiNames.COLORS_SCROLLBAR);

		this.materialsButton = this.ChildSkinElement(UiNames.MATERIALS_BUTTON);
		this.colorsButton = this.ChildSkinElement(UiNames.COLORS_BUTTON);

		this.skinPanelController = this.skinPanel.GetComponent<SkinPanelController>();
	}

	private GameObject ChildSkinElement(string elementName) {
		int i = 0;
		for (; i < skinPanel.transform.childCount && !skinPanel.transform.GetChild(i).name.Equals(elementName); i++) ;

		if (i < skinPanel.transform.childCount)
			return skinPanel.transform.GetChild(i).gameObject;
		else
			return null;
	}

	public override void ValidateTransform() {
		throw new NotImplementedException();
	}

	public override void CancelTransform() {
		throw new NotImplementedException();
	}
}