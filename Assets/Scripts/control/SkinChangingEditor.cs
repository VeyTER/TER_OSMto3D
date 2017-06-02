﻿using UnityEngine;
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

	private GameObject materialsPanel;
	private GameObject colorsPanel;

	private GameObject materialsGridPanel;
	private GameObject colorsGridPanel;

	private Dictionary<int, Material> instanceIdToMaterialTable;

	public SkinChangingEditor(GameObject skinPanel) {
		this.selectedBuildingStartMaterial = null;
		this.selectedBuildingStartColor = Color.white;

		this.skinPanel = skinPanel;
		this.skinPanel.AddComponent<SkinPanelController>();
		this.skinPanel.SetActive(false);

		this.skinPanelController = this.skinPanel.GetComponent<SkinPanelController>();

		RectTransform editPanelTransform = (RectTransform) this.skinPanelController.transform;
		this.skinPanelController.StartPosX = editPanelTransform.localPosition.x - editPanelTransform.rect.width;
		this.skinPanelController.EndPosX = editPanelTransform.localPosition.x;

		Vector3 panelPosition = this.skinPanel.transform.localPosition;
		this.skinPanel.transform.localPosition = new Vector3(this.skinPanelController.StartPosX, panelPosition.y, panelPosition.z);

		this.instanceIdToMaterialTable = new Dictionary<int, Material>();

		Transform skinSliderTransform = skinPanel.transform.GetChild(0);

		Transform materialsContainerPanelTransform = skinSliderTransform.GetChild(0);
		materialsGridPanel = materialsContainerPanelTransform.GetChild(0).gameObject;

		Transform colorsContainerPanelTransform = skinSliderTransform.GetChild(1);
		colorsGridPanel = colorsContainerPanelTransform.GetChild(0).gameObject;

		this.BuildMaterialsItems();
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
		GameObject materialItem = new GameObject("MaterialButton_" + materialData.ReadableName);
		materialItem.transform.SetParent(materialsGridPanel.transform, false);

		RectTransform itemTransform = materialItem.AddComponent<RectTransform>();
		itemTransform.sizeDelta = new Vector2(60, 60);

		materialItem.AddComponent<Button>();

		materialItem.AddComponent<UiManager>();

		Material targetMaterial = Resources.Load(materialData.TargetMaterialPath) as Material;
		instanceIdToMaterialTable.Add(materialItem.GetInstanceID(), targetMaterial);

		return materialItem;
	}

	private GameObject AddDecorations(GameObject materialItem) {
		GameObject decorations = new GameObject("Decorations");
		decorations.transform.SetParent(materialItem.transform, false);
		decorations.transform.localPosition = new Vector3(0, 0, -1);

		RectTransform decorationsTransform = decorations.AddComponent<RectTransform>();
		decorationsTransform.sizeDelta = new Vector2(0, 0);
		decorationsTransform.anchorMin = new Vector2(0, 0);
		decorationsTransform.anchorMax = new Vector2(1, 1);
		decorationsTransform.pivot = new Vector2(0.5F, 0.5F);

		this.AddLeftRect(decorations);
		this.AddRightRect(decorations);
		this.AddSelectionBackground(decorations);

		return decorations;
	}

	private GameObject AddLeftRect(GameObject decoration) {
		GameObject leftRect = new GameObject("LeftDecoration");
		leftRect.transform.SetParent(decoration.transform, false);
		leftRect.transform.localPosition = new Vector3(-23.8F, -8.6F, 13.6F);
		leftRect.transform.rotation = Quaternion.Euler(0, -130, -30);
		leftRect.transform.localScale = new Vector3(0.549217F, 1, 1);

		RectTransform leftRectTransform = leftRect.AddComponent<RectTransform>();
		leftRectTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		leftRectTransform.anchorMin = new Vector2(0, 1);
		leftRectTransform.anchorMax = new Vector2(0, 1);
		leftRectTransform.anchoredPosition = new Vector2(6.2F, -38.6F);
		leftRectTransform.pivot = new Vector2(0.5F, 0.5F);

		Image leftRectImage = leftRect.AddComponent<Image>();
		leftRectImage.color = ThemeColors.DARK_BLUE;

		return leftRect;
	}

	private GameObject AddRightRect(GameObject decoration) {
		GameObject rightRect = new GameObject("RightDecoration");
		rightRect.transform.SetParent(decoration.transform, false);
		rightRect.transform.localPosition = new Vector3(23.7F, -8.6F, 13.6F);
		rightRect.transform.rotation = Quaternion.Euler(0, -130, 30);
		rightRect.transform.localScale = new Vector3(0.549217F, 1, 1);

		RectTransform rightRectTransform = rightRect.AddComponent<RectTransform>();
		rightRectTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		rightRectTransform.anchorMin = new Vector2(0, 1);
		rightRectTransform.anchorMax = new Vector2(0, 1);
		rightRectTransform.anchoredPosition = new Vector2(53.7F, -38.6F);
		rightRectTransform.pivot = new Vector2(0.5F, 0.5F);

		Image rightRectImage = rightRect.AddComponent<Image>();
		rightRectImage.color = ThemeColors.DARK_BLUE;

		return rightRect;
	}

	private GameObject AddSelectionBackground(GameObject decoration) {
		GameObject backgroundRect = new GameObject("SelectionBackground");
		backgroundRect.transform.SetParent(decoration.transform, false);

		RectTransform backgroundRectTransform = backgroundRect.AddComponent<RectTransform>();
		backgroundRectTransform.sizeDelta = new Vector2(4.5F, 4.5F);
		backgroundRectTransform.anchorMin = new Vector2(0, 0);
		backgroundRectTransform.anchorMax = new Vector2(1, 1);
		backgroundRectTransform.anchoredPosition = new Vector2(0, 0);
		backgroundRectTransform.pivot = new Vector2(0.5F, 0.5F);

		Image backgroundRectImage = backgroundRect.AddComponent<Image>();
		backgroundRectImage.color = ThemeColors.LIGHT_BLUE;
		backgroundRectImage.enabled = false;

		return backgroundRect;
	}

	private GameObject AddBody(MaterialData materialData, GameObject materialItem) {
		GameObject body = new GameObject("Body");
		body.transform.SetParent(materialItem.transform, false);
		body.transform.localPosition = new Vector3(0, 0, -1);

		RectTransform bodyTransform = body.AddComponent<RectTransform>();
		bodyTransform.sizeDelta = new Vector2(0, 0);
		bodyTransform.anchorMin = new Vector2(0, 0);
		bodyTransform.anchorMax = new Vector2(1, 1);
		bodyTransform.pivot = new Vector2(0.5F, 0.5F);

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

		RectTransform labelTransform = label.AddComponent<RectTransform>();
		labelTransform.sizeDelta = new Vector2(75.2F, 14.5F);
		labelTransform.anchorMin = new Vector2(0, 1);
		labelTransform.anchorMax = new Vector2(0, 1);
		labelTransform.anchoredPosition = new Vector2(30, -49.7F);
		labelTransform.pivot = new Vector2(0.5F, 0.5F);

		Image labelImage = label.AddComponent<Image>();
		labelImage.color = ThemeColors.LIGHT_BLUE;

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

		RectTransform titleTransform = title.AddComponent<RectTransform>();
		titleTransform.sizeDelta = new Vector2(0, 0);
		titleTransform.anchorMin = new Vector2(0, 0);
		titleTransform.anchorMax = new Vector2(1, 1);
		titleTransform.anchoredPosition = new Vector3(0, 0);
		titleTransform.pivot = new Vector2(0.5F, 0.5F);

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

		for (int i = 0; i < materialsGridPanel.transform.childCount; i++) {
			GameObject itemButton = materialsGridPanel.transform.GetChild(i).gameObject;
			Material itemMaterial = instanceIdToMaterialTable[itemButton.GetInstanceID()];

			string startMaterialName = selectedBuildingStartMaterial.name.Replace(" (Instance)", "");
			if (itemMaterial.name.Equals(startMaterialName)) {
				this.UpdateMaterialItems(itemButton);
				break;
			}
		}
	}

	public void SwitchGrid(GameObject buttonToLight) {
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

				Color selectionOverlay = ThemeColors.LIGHT_BLUE;
				selectionOverlay.a = 0.5F;
				bodyImage.color = selectionOverlay;
			} else {
				selectionBackgroundImage.enabled = false;
				bodyImage.color = Color.white;
			}
		}
	}

	public void ChangeBuildingMaterial(GameObject sourceButton) {
		Material newMaterial = instanceIdToMaterialTable[sourceButton.GetInstanceID()];
		buildingTools.ReplaceMaterial(selectedBuilding, newMaterial);
	}

	public override void ValidateTransform() {
		if (!transformedObjects.Contains(selectedBuilding))
			transformedObjects.Add(selectedBuilding);
	}

	public override void CancelTransform() {
		GameObject firstWall = selectedBuilding.transform.GetChild(0).gameObject;
		buildingTools.ReplaceMaterial(selectedBuilding, selectedBuildingStartMaterial);
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