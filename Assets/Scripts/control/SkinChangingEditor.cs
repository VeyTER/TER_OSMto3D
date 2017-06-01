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

	private FloorColorController topFloorColorController;
	private FloorColorController bottomFloorColorController;

	public SkinChangingEditor(GameObject skinPanel) {
		this.selectedBuildingStartHeight = -1;

		this.skinPanel = skinPanel;
		this.skinPanel.AddComponent<SkinPanelController>();
		this.skinPanel.SetActive(false);

		this.skinPanelController = this.skinPanel.GetComponent<SkinPanelController>();

		RectTransform editPanelTransform = (RectTransform) this.skinPanelController.transform;
		this.skinPanelController.StartPosX = editPanelTransform.localPosition.x - editPanelTransform.rect.width;
		this.skinPanelController.EndPosX = editPanelTransform.localPosition.x;

		Vector3 panelPosition = this.skinPanel.transform.localPosition;
		this.skinPanel.transform.localPosition = new Vector3(this.skinPanelController.StartPosX, panelPosition.y, panelPosition.z);

		this.BuildMaterialsItems();
	}

	public void BuildMaterialsItems() {
		Transform skinSliderTransform = skinPanel.transform.GetChild(0);
		Transform materialsContainerPanelTransform = skinSliderTransform.GetChild(0);

		Transform materialsGridPanelTransform = materialsContainerPanelTransform.GetChild(0);

		GameObject materialItem = new GameObject();
		materialItem.transform.SetParent(materialsGridPanelTransform.transform, true);

		materialItem.AddComponent<RectTransform>();
		materialItem.AddComponent<Button>();

		RectTransform itemTransform = materialItem.GetComponent<RectTransform>();
		itemTransform.sizeDelta = new Vector2(60, 60);

		Button itemButton = materialItem.GetComponent<Button>();
		// /!\  itemButton.targetGraphic = [ Graphique de l'image interne ];


		GameObject decoration = new GameObject("Decorations");
		decoration.transform.SetParent(materialItem.transform, true);

		decoration.AddComponent<RectTransform>();

		RectTransform decorationTransform = decoration.GetComponent<RectTransform>();
		decorationTransform.sizeDelta = new Vector2(0, 0);
		decorationTransform.anchorMin = new Vector2(0, 0);
		decorationTransform.anchorMax = new Vector2(1, 1);
		decorationTransform.anchoredPosition3D = new Vector3(0, 0, -1);
		decorationTransform.pivot = new Vector2(0.5F, 0.5F);
		//decorationTransform.offsetMin = new Vector2(0, 0);
		//decorationTransform.offsetMax = new Vector2(0, 0);

		GameObject leftRect = new GameObject("LeftDecoration");
		leftRect.transform.SetParent(decoration.transform, true);
		leftRect.transform.position = new Vector3(6.2F, -38.6F, 13.6F);
		leftRect.transform.rotation = Quaternion.Euler(0, -130, -30);
		leftRect.transform.localScale = new Vector3(0.549217F, 1, 1);

		leftRect.AddComponent<RectTransform>();
		leftRect.AddComponent<CanvasRenderer>();
		leftRect.AddComponent<Image>();

		RectTransform leftRectTransform = leftRect.GetComponent<RectTransform>();
		leftRectTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		leftRectTransform.anchorMin = new Vector2(0, 1);
		leftRectTransform.anchorMax = new Vector2(0, 1);
		leftRectTransform.pivot = new Vector2(0.5F, 0.5F);

		Image leftRectImage = leftRect.GetComponent<Image>();
		leftRectImage.color = new Color(52 / 255F, 77 / 255F, 104 / 255F);


		GameObject rightRect = new GameObject("RightDecoration");
		rightRect.transform.SetParent(decoration.transform, true);
		rightRect.transform.position = new Vector3(54.8F, -38.6F, 13.6F);
		rightRect.transform.rotation = Quaternion.Euler(0, -130, 30);
		rightRect.transform.localScale = new Vector3(0.549217F, 1, 1);

		rightRect.AddComponent<RectTransform>();
		rightRect.AddComponent<CanvasRenderer>();
		rightRect.AddComponent<Image>();

		RectTransform rightRectTransform = rightRect.GetComponent<RectTransform>();
		rightRectTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		rightRectTransform.anchorMin = new Vector2(0, 1);
		rightRectTransform.anchorMax = new Vector2(0, 1);
		rightRectTransform.pivot = new Vector2(0.5F, 0.5F);

		Image rightRectImage = rightRect.GetComponent<Image>();
		rightRectImage.color = new Color(52 / 255F, 77 / 255F, 104 / 255F);


		GameObject body = new GameObject("Body");
		body.transform.SetParent(materialItem.transform, true);

		body.AddComponent<RectTransform>();

		RectTransform bodyTransform = body.GetComponent<RectTransform>();
		bodyTransform.sizeDelta = new Vector2(0, 0);
		bodyTransform.anchorMin = new Vector2(0, 0);
		bodyTransform.anchorMax = new Vector2(1, 1);
		bodyTransform.anchoredPosition3D = new Vector3(0, 0, -1);
		bodyTransform.pivot = new Vector2(0.5F, 0.5F);


		GameObject materialSupport = new GameObject("MaterialSupport");
		materialSupport.transform.position = new Vector3(30, -49.7F, -1);
		materialSupport.transform.SetParent(body.transform, true);

		materialSupport.AddComponent<RectTransform>();
		materialSupport.AddComponent<Image>();

		RectTransform materialSupportTransform = materialSupport.GetComponent<RectTransform>();
		bodyTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		bodyTransform.anchorMin = new Vector2(0, 1);
		bodyTransform.anchorMax = new Vector2(1, 1);
		bodyTransform.pivot = new Vector2(0.5F, 0.5F);

		Image materialSupportImage = materialSupport.GetComponent<Image>();

	}

	public void BuildColorsItems() {

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