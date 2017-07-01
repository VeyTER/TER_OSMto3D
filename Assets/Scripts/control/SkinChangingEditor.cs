using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SkinChangingEditor : ObjectEditor {
	private Material selectedBuildingStartMaterial;
	private Color selectedBuildingStartColor;

	private GameObject skinPanel;
	private SkinPanelController skinPanelController;

	private GameObject materialsGridPanel;
	private GameObject colorsGridPanel;

	private Dictionary<int, Material> buttonIdToMaterialTable;
	private Dictionary<int, Color> buttonIdToColorTable;

	public SkinChangingEditor(GameObject skinPanel) {
		this.selectedBuildingStartMaterial = null;
		this.selectedBuildingStartColor = Color.white;

		this.skinPanel = skinPanel;
		this.skinPanel.AddComponent<SkinPanelController>();
		this.skinPanel.SetActive(false);

		this.skinPanelController = this.skinPanel.GetComponent<SkinPanelController>();

		RectTransform editPanelRect = (RectTransform) this.skinPanelController.transform;
		this.skinPanelController.StartPosition = new Vector3(editPanelRect.localPosition.x - editPanelRect.rect.width, 0, 0);
		this.skinPanelController.EndPosition = new Vector3(editPanelRect.localPosition.x, 0, 0);

		Vector3 panelPosition = this.skinPanel.transform.localPosition;
		this.skinPanel.transform.localPosition = new Vector3(this.skinPanelController.StartPosition.x, panelPosition.y, panelPosition.z);

		this.buttonIdToMaterialTable = new Dictionary<int, Material>();
		this.buttonIdToColorTable = new Dictionary<int, Color>();

		Transform skinSliderTransform = skinPanel.transform.GetChild(0);

		Transform materialsContainerPanelTransform = skinSliderTransform.GetChild(0);
		materialsGridPanel = materialsContainerPanelTransform.GetChild(0).gameObject;

		Transform colorsContainerPanelTransform = skinSliderTransform.GetChild(1);
		colorsGridPanel = colorsContainerPanelTransform.GetChild(0).gameObject;

		this.BuildMaterialsItems();
		this.BuildColorsItems();
	}

	public void BuildMaterialsItems() {
		List<MaterialData> materialsData = this.MaterialsDetails();

		foreach(MaterialData materialData in materialsData) {
			GameObject materialItem = AddMaterialItem(materialData);
			this.AddDecorations(materialItem);
			GameObject body = this.AddBody(materialData, materialItem);

			Button itemButton = materialItem.GetComponent<Button>();
			Image bodyImage = body.GetComponent<Image>();
			itemButton.targetGraphic = bodyImage;
		}
	}

	private GameObject AddMaterialItem(MaterialData materialData) {
		GameObject materialItem = new GameObject(UiNames.MATERIAL_ITEM_BUTTON + "_" + materialData.ReadableName);
		materialItem.transform.SetParent(materialsGridPanel.transform, false);

		RectTransform itemRect = materialItem.AddComponent<RectTransform>();
		itemRect.sizeDelta = new Vector2(60, 60);

		materialItem.AddComponent<Button>();

		materialItem.AddComponent<UiManager>();

		Material targetMaterial = Resources.Load(materialData.TargetMaterialPath) as Material;
		buttonIdToMaterialTable.Add(materialItem.GetInstanceID(), targetMaterial);

		return materialItem;
	}

	private GameObject AddDecorations(GameObject materialItem) {
		GameObject decorations = new GameObject("Decorations");
		decorations.transform.SetParent(materialItem.transform, false);
		decorations.transform.localPosition = new Vector3(0, 0, -1);

		RectTransform decorationsRect = decorations.AddComponent<RectTransform>();
		decorationsRect.sizeDelta = new Vector2(0, 0);
		decorationsRect.anchorMin = new Vector2(0, 0);
		decorationsRect.anchorMax = new Vector2(1, 1);
		decorationsRect.pivot = new Vector2(0.5F, 0.5F);

		this.AddLeftRect(decorations);
		this.AddRightRectangle(decorations);
		this.AddSelectionBackground(decorations);

		return decorations;
	}

	private GameObject AddLeftRect(GameObject decoration) {
		GameObject leftRect = new GameObject("LeftDecoration");
		leftRect.transform.SetParent(decoration.transform, false);
		leftRect.transform.localPosition = new Vector3(-23.8F, -8.6F, 13.6F);
		leftRect.transform.rotation = Quaternion.Euler(0, -130, -30);
		leftRect.transform.localScale = new Vector3(0.549217F, 1, 1);

		RectTransform leftRectRect = leftRect.AddComponent<RectTransform>();
		leftRectRect.sizeDelta = new Vector2(75.2F, 14.5F);
		leftRectRect.anchorMin = new Vector2(0, 1);
		leftRectRect.anchorMax = new Vector2(0, 1);
		leftRectRect.anchoredPosition = new Vector2(6.2F, -38.6F);
		leftRectRect.pivot = new Vector2(0.5F, 0.5F);

		Image leftRectImage = leftRect.AddComponent<Image>();
		leftRectImage.color = ThemeColors.BLUE_DARK;

		return leftRect;
	}

	private GameObject AddRightRectangle(GameObject decoration) {
		GameObject rightRectangle = new GameObject("RightDecoration");
		rightRectangle.transform.SetParent(decoration.transform, false);
		rightRectangle.transform.localPosition = new Vector3(23.7F, -8.6F, 13.6F);
		rightRectangle.transform.rotation = Quaternion.Euler(0, -130, 30);
		rightRectangle.transform.localScale = new Vector3(0.549217F, 1, 1);

		RectTransform rightRectRect = rightRectangle.AddComponent<RectTransform>();
		rightRectRect.sizeDelta = new Vector2(75.2F, 14.5F);
		rightRectRect.anchorMin = new Vector2(0, 1);
		rightRectRect.anchorMax = new Vector2(0, 1);
		rightRectRect.anchoredPosition = new Vector2(53.7F, -38.6F);
		rightRectRect.pivot = new Vector2(0.5F, 0.5F);

		Image rightRectImage = rightRectangle.AddComponent<Image>();
		rightRectImage.color = ThemeColors.BLUE_DARK;

		return rightRectangle;
	}

	private GameObject AddBody(MaterialData materialData, GameObject materialItem) {
		GameObject body = new GameObject("Body");
		body.transform.SetParent(materialItem.transform, false);
		body.transform.localPosition = new Vector3(0, 0, -1);

		RectTransform bodyRect = body.AddComponent<RectTransform>();
		bodyRect.sizeDelta = new Vector2(0, 0);
		bodyRect.anchorMin = new Vector2(0, 0);
		bodyRect.anchorMax = new Vector2(1, 1);
		bodyRect.pivot = new Vector2(0.5F, 0.5F);

		Image bodyImage = body.AddComponent<Image>();
		Sprite textureSprite = Resources.Load<Sprite>(materialData.SourceTexturePath);
		bodyImage.sprite = textureSprite;

		GameObject label = this.AddLabel(body);
		this.AddTitle(materialData, label);

		return body;
	}

	private GameObject AddLabel(GameObject body) {
		GameObject label = new GameObject("MaterialLabel");
		label.transform.SetParent(body.transform, false);
		label.transform.localPosition = new Vector3(0, -19.7F, -1);

		RectTransform labelRect = label.AddComponent<RectTransform>();
		labelRect.sizeDelta = new Vector2(75.2F, 14.5F);
		labelRect.anchorMin = new Vector2(0, 1);
		labelRect.anchorMax = new Vector2(0, 1);
		labelRect.anchoredPosition = new Vector2(30, -49.7F);
		labelRect.pivot = new Vector2(0.5F, 0.5F);

		Image labelImage = label.AddComponent<Image>();
		labelImage.color = ThemeColors.BLUE_BRIGHT;

		Shadow labelTopShadow = label.AddComponent<Shadow>();
		labelTopShadow.effectDistance = new Vector2(0.71F, 1.15F);
		labelTopShadow.effectColor = new Color(0, 0, 0, 60 / 255F);

		Shadow labelBottomShadow = label.AddComponent<Shadow>();
		labelBottomShadow.effectDistance = new Vector2(0.77F, -1);
		labelBottomShadow.effectColor = new Color(0, 0, 0, 60 / 255F);

		return label;
	}

	private GameObject AddTitle(MaterialData materialData, GameObject label) {
		GameObject title = new GameObject("Title");
		title.transform.SetParent(label.transform, false);

		RectTransform titleRect = title.AddComponent<RectTransform>();
		titleRect.sizeDelta = new Vector2(0, 0);
		titleRect.anchorMin = new Vector2(0, 0);
		titleRect.anchorMax = new Vector2(1, 1);
		titleRect.anchoredPosition = new Vector3(0, 0);
		titleRect.pivot = new Vector2(0.5F, 0.5F);

		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

		Text titleText = title.AddComponent<Text>();
		titleText.text = materialData.ReadableName;
		titleText.font = arialFont;
		titleText.fontSize = 10;
		titleText.color = Color.black;
		titleText.alignment = TextAnchor.MiddleCenter;

		return title;
	}

	public void BuildColorsItems() {
		int nbLevels = 4;

		for (int r = nbLevels; r >= 0; r--) {
			for (int v = nbLevels; v >= 0; v--) {
				for (int b = nbLevels; b >= 0; b--) {
					Color color = new Color(r / (nbLevels * 1F), v / (nbLevels * 1F), b / (nbLevels * 1F));

					GameObject colorItem = new GameObject(UiNames.COLOR_ITEM_BUTTON + "_" + color.ToString());
					colorItem.transform.SetParent(colorsGridPanel.transform, false);

					colorItem.AddComponent<Image>();

					Button supportButton = colorItem.AddComponent<Button>();

					colorItem.AddComponent<UiManager>();

					this.AddSelectionBackground(colorItem);
					GameObject colorSupport = this.AddColorSupport(colorItem, color);

					Image supportImage = colorSupport.GetComponent<Image>();
					supportButton.targetGraphic = supportImage;

					buttonIdToColorTable.Add(colorItem.GetInstanceID(), color);
				}
			}
		}
	}

	private GameObject AddColorSupport(GameObject colorItem, Color color) {
		GameObject colorSupport = new GameObject("ColorSupport");
		colorSupport.transform.SetParent(colorItem.transform, false);

		RectTransform colorSupportRect = colorSupport.AddComponent<RectTransform>();
		colorSupportRect.sizeDelta = new Vector2(0, 0);
		colorSupportRect.anchorMin = new Vector2(0, 0);
		colorSupportRect.anchorMax = new Vector2(1, 1);
		colorSupportRect.anchoredPosition = new Vector2(0, 0);
		colorSupportRect.pivot = new Vector2(0.5F, 0.5F);

		Image supportImage = colorSupport.AddComponent<Image>();
		if (color.r == 1 && color.g == 1 && color.b == 1) {
			Sprite noneColorIcon = Resources.Load<Sprite>(IconsTexturesSprites.NONE_COLOR);
			supportImage.sprite = noneColorIcon;
		} else {
			supportImage.color = color;
		}

		return colorSupport;
	}

	private GameObject AddSelectionBackground(GameObject colorItem) {
		GameObject backgroundRectangle = new GameObject("SelectionBackground");
		backgroundRectangle.transform.SetParent(colorItem.transform, false);

		RectTransform backgroundRectangleRect = backgroundRectangle.AddComponent<RectTransform>();
		backgroundRectangleRect.sizeDelta = new Vector2(4.5F, 4.5F);
		backgroundRectangleRect.anchorMin = new Vector2(0, 0);
		backgroundRectangleRect.anchorMax = new Vector2(1, 1);
		backgroundRectangleRect.anchoredPosition = new Vector2(0, 0);
		backgroundRectangleRect.pivot = new Vector2(0.5F, 0.5F);

		Image backgroundRectImage = backgroundRectangle.AddComponent<Image>();
		backgroundRectImage.color = ThemeColors.BLUE_BRIGHT;
		backgroundRectImage.enabled = false;

		return backgroundRectangle;
	}

	private List<MaterialData> MaterialsDetails() {
		List<MaterialData> materialsData = new List<MaterialData>();

		if (File.Exists(FilePaths.MATERIAL_DETAILS_FILE)) {
			String detailsFileContent = System.IO.File.ReadAllText(FilePaths.MATERIAL_DETAILS_FILE);
			String[] lines = detailsFileContent.Split('\n');

			foreach (String line in lines) {
				String[] details = line.Split('\t');
				MaterialData materialData = new MaterialData(details);
				materialsData.Add(materialData);
			}
		}

		return materialsData;
	}

	public void InitializeSkinChangingMode() {
		skinPanel.SetActive(true);
		skinPanelController.OpenPanel(null);

		GameObject firstWall = selectedBuilding.transform.GetChild(0).gameObject;
		MeshRenderer meshRenderer = firstWall.GetComponent<MeshRenderer>();
		selectedBuildingStartMaterial = meshRenderer.materials[0];
		selectedBuildingStartColor = selectedBuildingStartMaterial.color;

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		nodeGroup.CustomMaterial = meshRenderer.materials[0];
		nodeGroup.OverlayColor = selectedBuildingStartMaterial.color;

		int i = 0;
		for (; i < materialsGridPanel.transform.childCount; i++) {
			GameObject itemButton = materialsGridPanel.transform.GetChild(i).gameObject;
			GameObject itemBody = itemButton.transform.GetChild(1).gameObject;

			string selectedMaterialName = buttonIdToMaterialTable[itemButton.GetInstanceID()].name;

			string textureName = itemBody.GetComponent<Image>().material.name;
			string itemMaterialName = nodeGroup.CustomMaterial.name.Replace(" (Instance)", "");

			if (selectedMaterialName.Equals(itemMaterialName)) {
				this.UpdateMaterialItems(itemButton);
				break;
			}
		}

		i = 0;
		for (; i < colorsGridPanel.transform.childCount; i++) {
			GameObject itemButton = colorsGridPanel.transform.GetChild(i).gameObject;

			string selectedColorName = buttonIdToColorTable[itemButton.GetInstanceID()].ToString();
			string itemColorName = nodeGroup.OverlayColor.ToString();
			if (selectedColorName.Equals(itemColorName)) {
				this.UpdateColorItems(itemButton);
				break;
			}
		}
	}

	public void SwitchPallet(GameObject buttonToLight) {
		if (skinPanelController.IsMotionLess()) {
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
			shadowButtonComponent.OnPointerExit(null);
		}
	}

	public void UpdateMaterialItems(GameObject selectedButton) {
		foreach (Transform materialButtonTransform in selectedButton.transform.parent.transform) {
			Transform decorationsTrasform = materialButtonTransform.transform.GetChild(0);

			GameObject body = materialButtonTransform.transform.GetChild(1).gameObject;
			Image bodyImage = body.GetComponent<Image>();

			GameObject selectionBackground = decorationsTrasform.GetChild(2).gameObject;
			Image selectionBackgroundImage = selectionBackground.GetComponent<Image>();

			bool isSelected = materialButtonTransform.gameObject.GetInstanceID() == selectedButton.GetInstanceID();

			if (isSelected) {
				selectionBackgroundImage.enabled = true;

				Color selectionOverlay = ThemeColors.BLUE_BRIGHT;
				selectionOverlay.a = 0.5F;
				bodyImage.color = selectionOverlay;
			} else {
				selectionBackgroundImage.enabled = false;
				bodyImage.color = Color.white;
			}
		}
	}

	public void UpdateColorItems(GameObject selectedButton) {
		foreach (Transform colorButtonTransform in selectedButton.transform.parent.transform) {
			GameObject colorSupport = colorButtonTransform.transform.GetChild(1).gameObject;

			GameObject selectionBackground = colorButtonTransform.GetChild(0).gameObject;
			Image selectionBackgroundImage = selectionBackground.GetComponent<Image>();

			bool isSelected = colorButtonTransform.gameObject.GetInstanceID() == selectedButton.GetInstanceID();

			if (isSelected) {
				selectionBackgroundImage.enabled = true;

				Color selectionOverlay = ThemeColors.BLUE_BRIGHT;
				selectionOverlay.a = 0.5F;
			} else {
				selectionBackgroundImage.enabled = false;
			}
		}
	}

	public void ChangeBuildingMaterial(GameObject sourceButton) {
		Material newMaterial = buttonIdToMaterialTable[sourceButton.GetInstanceID()];
		buildingsTools.ReplaceMaterial(selectedBuilding, newMaterial);

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ReplaceColor(selectedBuilding, nodeGroup.OverlayColor);
		nodeGroup.CustomMaterial = newMaterial;
	}

	public void ChangeBuildingColor(GameObject sourceButton) {
		Color newColor = buttonIdToColorTable[sourceButton.GetInstanceID()];
		buildingsTools.ReplaceColor(selectedBuilding, newColor);

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		nodeGroup.OverlayColor = newColor;
	}

	public override void ValidateTransform() {
		if (!transformedObjects.Contains(selectedBuilding))
			transformedObjects.Add(selectedBuilding);
	}

	public override void CancelTransform() {
		GameObject firstWall = selectedBuilding.transform.GetChild(0).gameObject;
		buildingsTools.ReplaceMaterial(selectedBuilding, selectedBuildingStartMaterial);
		buildingsTools.ReplaceColor(selectedBuilding, selectedBuildingStartColor);

		NodeGroup nodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		nodeGroup.CustomMaterial = selectedBuildingStartMaterial;
		nodeGroup.OverlayColor = selectedBuildingStartColor;
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

/*

foreach (Transform wall in selectedBuilding.transform) {
	MeshRenderer meshRenderer = wall.GetComponent<MeshRenderer>();
	meshRenderer.materials[0].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
}

*/