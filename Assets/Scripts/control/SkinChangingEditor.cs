using UnityEngine;
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

	private UiBuilder uiBuilder;

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

		GameObject skinSlider = skinPanel.transform.Find(UiNames.SKIN_SLIDER).gameObject;

		GameObject materialsContainerPanel = skinSlider.transform.Find(UiNames.MATERIALS_CONTAINER).gameObject;
		materialsGridPanel = materialsContainerPanel.transform.Find(UiNames.MATERIALS_GRID).gameObject;

		GameObject colorsContainerPanel = skinSlider.transform.Find(UiNames.COLORS_CONTAINER).gameObject;
		colorsGridPanel = colorsContainerPanel.transform.Find(UiNames.COLORS_GRID).gameObject;

		this.uiBuilder = UiBuilder.GetInstance();

		this.BuildMaterialsItems();
		this.BuildColorsItems();
	}

	private void BuildMaterialsItems() {
		List<MaterialData> materialsData = this.MaterialsDetails();

		foreach(MaterialData materialData in materialsData)
			uiBuilder.BuildMaterialItem(materialData, materialsGridPanel, buttonIdToMaterialTable);
	}

	private void BuildColorsItems() {
		int nbLevels = 4;
		for (int r = nbLevels; r >= 0; r--) {
			for (int g = nbLevels; g >= 0; g--) {
				for (int b = nbLevels; b >= 0; b--) {
					Color itemColor = new Color(r / (nbLevels * 1F), g / (nbLevels * 1F), b / (nbLevels * 1F));
					uiBuilder.BuildColorsItem(itemColor, colorsGridPanel, buttonIdToColorTable);
				}
			}
		}
	}

	private List<MaterialData> MaterialsDetails() {
		List<MaterialData> materialsData = new List<MaterialData>();

		if (File.Exists(FilePaths.MATERIAL_DETAILS_FILE)) {
			String detailsFileContent = File.ReadAllText(FilePaths.MATERIAL_DETAILS_FILE);
			String[] lines = detailsFileContent.Split('\n');

			foreach (String line in lines) {
				String[] details = line.Split('\t');
				MaterialData materialData = new MaterialData(details);
				materialsData.Add(materialData);
			}
		}

		return materialsData;
	}

	public override void InitializeMode() {
		skinPanel.SetActive(true);
		skinPanelController.OpenPanel(null);

		GameObject walls = selectedBuilding.transform.GetChild(CityBuilder.WALLS_INDEX).gameObject;
		MeshRenderer meshRenderer = walls.GetComponent<MeshRenderer>();
		selectedBuildingStartMaterial = meshRenderer.materials[0];
		selectedBuildingStartColor = selectedBuildingStartMaterial.color;

		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingNodeGroup.CustomMaterial = meshRenderer.materials[0];
		buildingNodeGroup.OverlayColor = selectedBuildingStartMaterial.color;

		int i = 0;
		for (; i < materialsGridPanel.transform.childCount; i++) {
			GameObject materialItem = materialsGridPanel.transform.GetChild(i).gameObject;
			GameObject itemButton = materialItem.transform.Find(UiNames.MATERIAL_ITEM_BUTTON).gameObject;
			GameObject itemBody = itemButton.transform.Find(UiNames.MATERIAL_ITEM_BODY).gameObject;

			string selectedMaterialName = buttonIdToMaterialTable[materialItem.GetInstanceID()].name;

			string textureName = itemBody.GetComponent<Image>().material.name;
			string itemMaterialName = buildingNodeGroup.CustomMaterial.name.Replace(" (Instance)", "");

			if (selectedMaterialName.Equals(itemMaterialName)) {
				this.UpdateMaterialItems(itemButton);
				break;
			}
		}

		i = 0;
		for (; i < colorsGridPanel.transform.childCount; i++) {
			GameObject colorItem = colorsGridPanel.transform.GetChild(i).gameObject;
			GameObject itemButton = colorItem.transform.Find(UiNames.COLOR_ITEM_BUTTON).gameObject;

			string selectedColorName = buttonIdToColorTable[colorItem.GetInstanceID()].ToString();
			string itemColorName = buildingNodeGroup.OverlayColor.ToString();
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

			Button lightButtonDevice = buttonToLight.GetComponent<Button>();
			Button shadowButtonDevice = buttonToDark.GetComponent<Button>();

			lightButtonDevice.interactable = false;
			shadowButtonDevice.interactable = true;
			shadowButtonDevice.OnPointerExit(null);
		}
	}

	public void UpdateMaterialItems(GameObject selectedButton) {
		foreach (Transform materialItemTransform in materialsGridPanel.transform) {
			GameObject body = materialItemTransform.transform.Find(UiNames.MATERIAL_ITEM_BUTTON + "/" + UiNames.MATERIAL_ITEM_BODY).gameObject;
			Image bodyImage = body.GetComponent<Image>();

			GameObject itemButton = materialItemTransform.transform.Find(UiNames.MATERIAL_ITEM_BUTTON).gameObject;
			GameObject decorations = itemButton.transform.Find(UiNames.MATERIAL_ITEM_DECORATION).gameObject;
			GameObject selectionBackground = decorations.transform.Find(UiNames.ITEM_SELECTION_BACKGROUND).gameObject;
			Image selectionBackgroundImage = selectionBackground.GetComponent<Image>();

			bool isSelected = itemButton.gameObject.GetInstanceID() == selectedButton.GetInstanceID();

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
		foreach (Transform colorButtonTransform in colorsGridPanel.transform) {
			GameObject itemButton = colorButtonTransform.Find(UiNames.COLOR_ITEM_BUTTON).gameObject;
			GameObject colorSupport = itemButton.transform.Find(UiNames.COLOR_ITEM_SUPPORT).gameObject;

			GameObject selectionBackground = itemButton.transform.Find(UiNames.ITEM_SELECTION_BACKGROUND).gameObject;
			Image selectionBackgroundImage = selectionBackground.GetComponent<Image>();

			bool isSelected = itemButton.gameObject.GetInstanceID() == selectedButton.GetInstanceID();

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
		Material newMaterial = buttonIdToMaterialTable[sourceButton.transform.parent.gameObject.GetInstanceID()];
		newMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
		buildingsTools.ReplaceMaterial(selectedBuilding, newMaterial);

		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingsTools.ReplaceColor(selectedBuilding, buildingNodeGroup.OverlayColor);
		buildingNodeGroup.CustomMaterial = newMaterial;
	}

	public void ChangeBuildingColor(GameObject sourceButton) {
		Color newColor = buttonIdToColorTable[sourceButton.transform.parent.gameObject.GetInstanceID()];
		buildingsTools.ReplaceColor(selectedBuilding, newColor);

		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingNodeGroup.OverlayColor = newColor;
	}

	public override void ValidateTransform() {
		if (!transformedObjects.Contains(selectedBuilding))
			transformedObjects.Add(selectedBuilding);
	}

	public override void CancelTransform() {
		buildingsTools.ReplaceMaterial(selectedBuilding, selectedBuildingStartMaterial);
		buildingsTools.ReplaceColor(selectedBuilding, selectedBuildingStartColor);

		BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(selectedBuilding);
		buildingNodeGroup.CustomMaterial = selectedBuildingStartMaterial;
		buildingNodeGroup.OverlayColor = selectedBuildingStartColor;
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