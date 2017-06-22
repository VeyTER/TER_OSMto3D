using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

public class UiBuilder {
	private static float BUILDING_DATA_LINK_WIDTH = 0.01F;
	private static float BUILDING_DATA_LINK_HEIGHT = 1.7F;

	private static float BUILDING_DATA_DATA_AREA_EXT_PADDING = 0.03F;

	private static float BUILDING_DATA_HEADER_HEIGHT = 0.1F;
	private static float BUILDING_DATA_HEADER_LENGTH = 0.3F;

	private static float BUILDING_DATA_ICON_SIZE = BUILDING_DATA_HEADER_HEIGHT * 3;

	private static float BUILDING_DATA_INDICATOR_HEIGHT = 0.08F;
	private static float BUILDING_DATA_INDICATOR_EXT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.2F;
	private static float BUILDING_DATA_INDICATOR_INT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.1F;

	private static float BUILDING_DATA_INDICATOR_ICON_SIZE = BUILDING_DATA_INDICATOR_HEIGHT * 0.8F;

	private static float BUILDING_DATA_DATABOX_HEIGHT = BUILDING_DATA_HEADER_HEIGHT + 5 * BUILDING_DATA_INDICATOR_HEIGHT + 4 * BUILDING_DATA_INDICATOR_INT_PADDING + BUILDING_DATA_INDICATOR_EXT_PADDING;
	private static float BUILDING_DATA_DATABOX_EXT_PADDING = 0.03F;

	private UiBuilder() {}

	public static UiBuilder GetInstance() {
		return UiBuilderHolder.instance;
	}

	public GameObject BuildBuildingDataDisplay(GameObject building) {
		GameObject linkPanel = this.BuildBuildingDataLink(building);

		this.BuildBuildingDataIcon(linkPanel, building.name);
		GameObject dataPanel = this.BuildDataDisplayPanel(linkPanel, building.name);

		return dataPanel;
	}

	private GameObject BuildBuildingDataLink(GameObject building) {
		Transform buildingFirstWallTransform = building.transform.GetChild(0);
		Vector3 buildingPosition = building.transform.position;
		float buildingHeight = buildingFirstWallTransform.localScale.y;

		GameObject link = new GameObject("Link_" + building.name, typeof(MeshFilter), typeof(MeshRenderer));
		link.transform.SetParent(building.transform, false);

		link.transform.position = new Vector3(buildingPosition.x, buildingHeight, buildingPosition.z);

		MeshFilter linkMeshRenderer = link.GetComponent<MeshFilter>();
		linkMeshRenderer.mesh.vertices = new Vector3[] {
			new Vector3(-BUILDING_DATA_LINK_WIDTH / 2F, 0, 0),
			new Vector3(-BUILDING_DATA_LINK_WIDTH / 2F, BUILDING_DATA_LINK_HEIGHT, 0),
			new Vector3(BUILDING_DATA_LINK_WIDTH, BUILDING_DATA_LINK_HEIGHT, 0),
			new Vector3(BUILDING_DATA_LINK_WIDTH, 0, 0),
		};
		linkMeshRenderer.mesh.triangles = this.NewRectTriangulation();

		MeshRenderer linkRenderer = link.GetComponent<MeshRenderer>();
		linkRenderer.material = this.NewColorFill(ThemeColors.BLUE);

		link.transform.parent = building.transform.parent.parent;

		return link;
	}

	private GameObject BuildBuildingDataIcon(GameObject link, string buildingName) {
		Vector3 linkPanelPosition = link.transform.position;

		GameObject icon = new GameObject("Icon_" + buildingName, typeof(MeshFilter), typeof(MeshRenderer));
		icon.transform.SetParent(link.transform, false);
		icon.transform.rotation = Quaternion.Euler(180, 0, -90);

		GameObject iconBackground = new GameObject("IconBackground_" + buildingName, typeof(MeshFilter), typeof(MeshRenderer));
		iconBackground.transform.SetParent(icon.transform, false);
		icon.transform.position = new Vector3(linkPanelPosition.x, linkPanelPosition.y + BUILDING_DATA_LINK_HEIGHT + BUILDING_DATA_ICON_SIZE / 2F, linkPanelPosition.z);


		MeshFilter iconBackgroundShape = iconBackground.GetComponent<MeshFilter>();
		iconBackgroundShape.mesh.vertices = new Vector3[] {
			new Vector3(-BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(-BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F, 0),
		};
		iconBackgroundShape.mesh.triangles = this.NewRectTriangulation();
		iconBackgroundShape.mesh.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (1, 1),
			new Vector2 (0, 1)
		};

		MeshRenderer iconBackgroundRenderer = iconBackground.GetComponent<MeshRenderer>();
		iconBackgroundRenderer.material = this.NewTextureFill(IconsAndTextures.BUILDING_DATA_ICON_BACKGROUND);

		MeshFilter iconShape = icon.GetComponent<MeshFilter>();
		iconShape.mesh.vertices = new Vector3[] {
			new Vector3(-BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(-BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F, 0),
		};
		iconShape.mesh.triangles = this.NewRectTriangulation();
		iconShape.mesh.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (1, 1),
			new Vector2 (0, 1)
		};

		MeshRenderer iconRenderer = icon.GetComponent<MeshRenderer>();
		iconRenderer.material = this.NewTextureFill(IconsAndTextures.BUILDING_DATA_ICON);

		return iconBackground;
	}

	private GameObject BuildDataDisplayPanel(GameObject link, string buildingName) {
		Vector3 linkPosition = link.transform.position;

		GameObject displayPanel = new GameObject("DisplayPanel_" + buildingName);
		displayPanel.transform.SetParent(link.transform, false);
		displayPanel.transform.position = new Vector3(linkPosition.x, linkPosition.y + BUILDING_DATA_LINK_HEIGHT, linkPosition.z);

		return displayPanel;
	}

	public GameObject BuildBuidingDataBox(GameObject displayPanel, BuildingSubsetData buildingSubsetData, int index) {
		GameObject dataBox = new GameObject("DataBox_" + buildingSubsetData.Name);
		dataBox.transform.SetParent(displayPanel.transform, false);
		dataBox.transform.localPosition = new Vector3(0, -(index * (BUILDING_DATA_DATABOX_HEIGHT + BUILDING_DATA_DATABOX_EXT_PADDING) + BUILDING_DATA_LINK_WIDTH + BUILDING_DATA_DATA_AREA_EXT_PADDING), 0);

		this.BuildBuildingDataBoxHeader(dataBox, buildingSubsetData);
		this.BuildBuildingDataBoxContent(dataBox, buildingSubsetData);

		return null;
	}

	private GameObject BuildBuildingDataBoxHeader(GameObject dataBox, BuildingSubsetData buildingSubsetData) {
		GameObject dataBoxHeader = new GameObject("DataBoxHeader_" + buildingSubsetData.Name, typeof(MeshFilter), typeof(MeshRenderer));
		dataBoxHeader.transform.SetParent(dataBox.transform, false);

		GameObject dataBoxHeaderText = new GameObject("DataBoxHeaderText_" + buildingSubsetData.Name, typeof(MeshRenderer), typeof(TextMesh));
		dataBoxHeaderText.transform.SetParent(dataBoxHeader.transform, false);
		dataBoxHeaderText.transform.transform.localPosition = new Vector3(BUILDING_DATA_HEADER_HEIGHT * 0.8F, -BUILDING_DATA_HEADER_HEIGHT / 2F, -0.01F);

		MeshFilter headerShape = dataBoxHeader.GetComponent<MeshFilter>();
		headerShape.mesh.vertices = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(BUILDING_DATA_HEADER_LENGTH, 0, 0),
			new Vector3(BUILDING_DATA_HEADER_LENGTH, -BUILDING_DATA_HEADER_HEIGHT, 0),
			new Vector3(0, -BUILDING_DATA_HEADER_HEIGHT, 0),
		};
		headerShape.mesh.triangles = this.NewRectTriangulation();

		MeshRenderer headerRenderer = dataBoxHeader.GetComponent<MeshRenderer>();
		headerRenderer.material = this.NewColorFill(ThemeColors.BRIGHT_BLUE);

		TextMesh textShape = dataBoxHeaderText.GetComponent<TextMesh>();
		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		textShape.font = arialFont;
		textShape.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);
		textShape.anchor = TextAnchor.MiddleLeft;
		textShape.characterSize = BUILDING_DATA_HEADER_HEIGHT / 2F;
		textShape.text = buildingSubsetData.Name;

		//MeshRenderer labelRenderer = dataBoxHeaderLabel.GetComponent<MeshRenderer>();
		//labelRenderer.material = new Material(Shader.Find("Sprites/Default")) {
		//	color = ThemeColors.BRIGHT_BLUE
		//};

		return dataBoxHeader;
	}

	private GameObject BuildBuildingDataBoxContent(GameObject dataBox, BuildingSubsetData buildingSubsetData) {
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.TEMPERATURE_ICON, "Température", buildingSubsetData.Temperature.ToString(), "°", 0);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.HUMIDITY_ICON, "Humidité", buildingSubsetData.Humidity.ToString(), "%", 1);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.LUMINOSITY_ICON, "Luminosté", buildingSubsetData.Luminosity.ToString(), "lux", 2);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.CO2_ICON, "CO2", buildingSubsetData.Co2.ToString(), "ppm", 3);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.PRESENCE_ICON, "Présence", buildingSubsetData.Presence ? "Oui" : "Non", "°", 4);

		return dataBox;
	}

	private GameObject BuildBuildingSensorIndicator(GameObject dataBox, string iconPath, string sensorName, string sensorValue, string sensorUnit, int index) {
		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		string subsetName = dataBox.name.Split('_')[1];

		float sensorIndicatorPaddingY = BUILDING_DATA_INDICATOR_EXT_PADDING + BUILDING_DATA_HEADER_HEIGHT;
		float sensorIndicatorPosY = -(index * (BUILDING_DATA_INDICATOR_HEIGHT + (index == 0 ? 0 : BUILDING_DATA_INDICATOR_INT_PADDING)) + sensorIndicatorPaddingY);

		float iconCenter = BUILDING_DATA_INDICATOR_HEIGHT * 0.1F + BUILDING_DATA_INDICATOR_ICON_SIZE / 2F;

		GameObject sensorIndicator = new GameObject(sensorName + " Indicator_" + subsetName);
		sensorIndicator.transform.SetParent(dataBox.transform, false);
		sensorIndicator.transform.localPosition = new Vector3(BUILDING_DATA_INDICATOR_EXT_PADDING, sensorIndicatorPosY, 0);

		GameObject sensorTitleLabel = new GameObject(sensorName + "TitleLabel_" + subsetName, typeof(MeshFilter), typeof(MeshRenderer));
		sensorTitleLabel.transform.SetParent(sensorIndicator.transform, false);

		GameObject sensorIcon = new GameObject(sensorName + "Icon_" + subsetName, typeof(MeshFilter), typeof(MeshRenderer));
		sensorIcon.transform.SetParent(sensorIndicator.transform, false);
		sensorIcon.transform.localPosition = new Vector3(iconCenter, -iconCenter, -0.01F);
		sensorIcon.transform.rotation = Quaternion.Euler(180, 0, -90);

		GameObject sensorTitleLabelText = new GameObject(sensorName + "TitleLabelText_" + subsetName, typeof(TextMesh));
		sensorTitleLabelText.transform.SetParent(sensorTitleLabel.transform, false);
		sensorTitleLabelText.transform.localPosition = new Vector3(BUILDING_DATA_INDICATOR_HEIGHT * 0.2F + BUILDING_DATA_INDICATOR_ICON_SIZE, -BUILDING_DATA_INDICATOR_HEIGHT / 2F, -0.01F);

		TextMesh titleLabelTextShape = sensorTitleLabelText.GetComponent<TextMesh>();
		titleLabelTextShape.font = arialFont;
		titleLabelTextShape.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);
		titleLabelTextShape.anchor = TextAnchor.MiddleLeft;
		titleLabelTextShape.characterSize = BUILDING_DATA_INDICATOR_HEIGHT / 2F;
		titleLabelTextShape.text = sensorName;

		float titleLabelWidth = this.TextWidth(sensorName, arialFont, titleLabelTextShape.fontStyle) + BUILDING_DATA_INDICATOR_ICON_SIZE;

		MeshFilter iconShape = sensorIcon.GetComponent<MeshFilter>();
		iconShape.mesh.vertices = new Vector3[] {
			new Vector3(-BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, -BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, 0),
			new Vector3(-BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, 0),
			new Vector3(BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, -BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, 0),
		};
		iconShape.mesh.triangles = this.NewRectTriangulation();





		Texture newTexture = Resources.Load<Texture>(iconPath);
		float ratio = newTexture.width / (newTexture.height * 1F);
		if (ratio > 1) {
			iconShape.mesh.uv = new Vector2[] {
				new Vector2 (0, 0.5F - ratio / 2F),
				new Vector2 (1, 0.5F - ratio / 2F),
				new Vector2 (1, 0.5F + ratio / 2F),
				new Vector2 (0, 0.5F + ratio / 2F)
			};
		} else {
			iconShape.mesh.uv = new Vector2[] {
				new Vector2 (0.5F - ratio / 2F, 0),
				new Vector2 (0.5F + ratio / 2F, 0),
				new Vector2 (0.5F + ratio / 2F, 1),
				new Vector2 (0.5F - ratio / 2F, 1)
			};
		}




		MeshRenderer iconRenderer = sensorIcon.GetComponent<MeshRenderer>();
		iconRenderer.material = this.NewTextureFill(iconPath);

		iconRenderer.material.mainTexture.wrapMode = TextureWrapMode.Clamp;

		GameObject sensorValueLabel = new GameObject(sensorName + "ValueLabe_" + subsetName, typeof(MeshFilter), typeof(MeshRenderer));
		sensorValueLabel.transform.SetParent(sensorIndicator.transform, false);
		sensorValueLabel.transform.localPosition = new Vector3(titleLabelWidth, 0, 0);

		GameObject sensorValueLabelText = new GameObject(sensorName + "ValueLabelText_" + subsetName, typeof(TextMesh));
		sensorValueLabelText.transform.SetParent(sensorValueLabel.transform, false);
		sensorValueLabelText.transform.localPosition = new Vector3(BUILDING_DATA_INDICATOR_HEIGHT * 0.2F, -BUILDING_DATA_INDICATOR_HEIGHT / 2F, -0.01F);

		TextMesh valueLabelTextShape = sensorValueLabelText.GetComponent<TextMesh>();
		valueLabelTextShape.font = arialFont;
		valueLabelTextShape.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);
		valueLabelTextShape.anchor = TextAnchor.MiddleLeft;
		valueLabelTextShape.characterSize = BUILDING_DATA_INDICATOR_HEIGHT / 2F;
		valueLabelTextShape.text = sensorValue + sensorUnit;

		float valueLabelWidth = this.TextWidth(sensorValue + sensorUnit, arialFont, valueLabelTextShape.fontStyle) - 0.002F;


		MeshFilter titleLabelShape = sensorTitleLabel.GetComponent<MeshFilter>();
		titleLabelShape.mesh.vertices = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(titleLabelWidth, 0, 0),
			new Vector3(titleLabelWidth, -BUILDING_DATA_INDICATOR_HEIGHT, 0),
			new Vector3(0, -BUILDING_DATA_INDICATOR_HEIGHT, 0),
		};
		titleLabelShape.mesh.triangles = this.NewRectTriangulation();

		MeshRenderer titleLabelRenderer = sensorTitleLabel.GetComponent<MeshRenderer>();
		titleLabelRenderer.material = this.NewColorFill(ThemeColors.BRIGHT_BLUE);


		MeshFilter valueLabelShape = sensorValueLabel.GetComponent<MeshFilter>();
		valueLabelShape.mesh.vertices = new Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(valueLabelWidth, 0, 0),
			new Vector3(valueLabelWidth, -BUILDING_DATA_INDICATOR_HEIGHT, 0),
			new Vector3(0, -BUILDING_DATA_INDICATOR_HEIGHT, 0),
		};
		valueLabelShape.mesh.triangles = this.NewRectTriangulation();

		MeshRenderer valueLabelRenderer = sensorValueLabel.GetComponent<MeshRenderer>();
		valueLabelRenderer.material = this.NewColorFill(Color.white);

		return sensorIndicator;
	}

	private float TextWidth(string text, Font arialFont, FontStyle fontStyle) {
		float res = 0;
		foreach (char textChar in text) {
			CharacterInfo charaterInfo = new CharacterInfo();
			arialFont.GetCharacterInfo(textChar, out charaterInfo);
			res += charaterInfo.advance;
		}


		res /= (((400F + (res * 2.5F)) * (0.03F / BUILDING_DATA_INDICATOR_HEIGHT)));
		res += BUILDING_DATA_INDICATOR_HEIGHT * 0.2F;

		return res;
	}

	private int[] NewRectTriangulation() {
		return new int[] {
			1, 0, 2,
			2, 0, 3
		};
	}

	private Material NewColorFill(Color newColor) {
		return new Material(Shader.Find("Sprites/Default")) {
			color = newColor
		};
	}

	private Material NewTextureFill(string texturePath) {
		Texture newTexture = Resources.Load<Texture>(texturePath);
		return new Material(Shader.Find("Sprites/Default")) {
			mainTexture = newTexture
		};
	}

	private class UiBuilderHolder {
		internal static UiBuilder instance = new UiBuilder();
	}
}
