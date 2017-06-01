using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

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
		List<MaterialData> materialsData = this.MaterialsDetails();

		foreach(MaterialData materialData in materialsData) {
			Transform skinSliderTransform = skinPanel.transform.GetChild(0);
			Transform materialsContainerPanelTransform = skinSliderTransform.GetChild(0);

			Transform materialsGridPanelTransform = materialsContainerPanelTransform.GetChild(0);

			GameObject materialItem = new GameObject();
			materialItem.transform.SetParent(materialsGridPanelTransform.transform, false);

			RectTransform itemTransform = materialItem.AddComponent<RectTransform>();
			itemTransform.sizeDelta = new Vector2(60, 60);

			Button itemButton = materialItem.AddComponent<Button>();


			GameObject decoration = new GameObject("Decorations");
			decoration.transform.SetParent(materialItem.transform, false);
			decoration.transform.localPosition = new Vector3(0, 0, -1);

			RectTransform decorationTransform = decoration.AddComponent<RectTransform>();
			decorationTransform.sizeDelta = new Vector2(0, 0);
			decorationTransform.anchorMin = new Vector2(0, 0);
			decorationTransform.anchorMax = new Vector2(1, 1);
			decorationTransform.pivot = new Vector2(0.5F, 0.5F);


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
			Debug.Log((textureSprite != null) + "  " + materialData.SourceTexturePath);
			bodyImage.sprite = textureSprite;

			var test = System.IO.Path.AltDirectorySeparatorChar;

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


			itemButton.targetGraphic = bodyImage;
		}
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
	}

	public void UpdateButtons(GameObject buttonToLight) {
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