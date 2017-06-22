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
	private static float BUILDING_DATA_HEADER_LENGTH = 0.8F;

	private static float BUILDING_DATA_ICON_SIZE = BUILDING_DATA_HEADER_HEIGHT * 3;

	private static float BUILDING_DATA_INDICATOR_HEIGHT = 0.08F;
	private static float BUILDING_DATA_INDICATOR_EXT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.2F;
	private static float BUILDING_DATA_INDICATOR_INT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.1F;

	private static float BUILDING_DATA_INDICATOR_ICON_SIZE = BUILDING_DATA_INDICATOR_HEIGHT * 0.8F;

	private static float BUILDING_DATA_DATABOX_HEIGHT = BUILDING_DATA_HEADER_HEIGHT + 5 * BUILDING_DATA_INDICATOR_HEIGHT + 4 * BUILDING_DATA_INDICATOR_INT_PADDING + BUILDING_DATA_INDICATOR_EXT_PADDING;
	private static float BUILDING_DATA_DATABOX_EXT_PADDING = 0.03F;

	private UiBuilder() { }

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

		Vector2 linkVerticesStartPosition = new Vector2(-BUILDING_DATA_LINK_WIDTH / 2F, 0);
		Vector2 linkDimensions = new Vector2(BUILDING_DATA_LINK_WIDTH, BUILDING_DATA_LINK_HEIGHT);
		GameObject link = this.BuildNew3DRectangle(building, "Link_" + building.name, Vector3.zero, linkDimensions, ThemeColors.BLUE);

		link.transform.parent = building.transform.parent.parent;

		return link;
	}

	private GameObject BuildBuildingDataIcon(GameObject link, string buildingName) {
		Vector3 linkPanelPosition = link.transform.position;

		string iconName = "Icon_" + buildingName;
		Vector3 iconPosition = new Vector3(0, BUILDING_DATA_LINK_HEIGHT + BUILDING_DATA_ICON_SIZE / 2F, 0);
		Vector2 iconVerticesStartPosition = new Vector2(-BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F);
		Vector2 iconDimensions = new Vector2(BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F);
		GameObject icon = this.BuildNew3DRectangle(link, iconName, iconPosition, iconVerticesStartPosition, iconDimensions, IconsAndTextures.BUILDING_DATA_ICON);

		string backgroundName = "IconBackground_" + buildingName;
		Vector3 backgroundPosition = new Vector3(0, 0, 0);
		Vector2 backgroundVerticesStartPosition = new Vector2(-BUILDING_DATA_ICON_SIZE / 2F, -BUILDING_DATA_ICON_SIZE / 2F);
		Vector2 backgroundDimensions = new Vector2(BUILDING_DATA_ICON_SIZE / 2F, BUILDING_DATA_ICON_SIZE / 2F);
		GameObject iconBackground = this.BuildNew3DRectangle(icon, backgroundName, backgroundPosition, backgroundVerticesStartPosition, backgroundDimensions, IconsAndTextures.BUILDING_DATA_ICON_BACKGROUND);

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
		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		Vector2 boxHeaderDimensions = new Vector2(BUILDING_DATA_HEADER_LENGTH, -BUILDING_DATA_HEADER_HEIGHT);
		GameObject dataBoxHeader = this.BuildNew3DRectangle(dataBox, "DataBoxHeader_" + buildingSubsetData.Name, Vector3.zero, boxHeaderDimensions, ThemeColors.BRIGHT_BLUE);

		Vector3 boxHeaderTextname = new Vector3(BUILDING_DATA_HEADER_HEIGHT * 0.8F, -BUILDING_DATA_HEADER_HEIGHT / 2F, -0.01F);
		float characterSize = BUILDING_DATA_HEADER_HEIGHT / 2F;
		GameObject dataBoxHeaderText = this.BuildNew3DText(dataBoxHeader, "DataBoxHeaderText_" + buildingSubsetData.Name, boxHeaderTextname, buildingSubsetData.Name, arialFont, characterSize);

		return dataBoxHeader;
	}

	private GameObject BuildBuildingDataBoxContent(GameObject dataBox, BuildingSubsetData buildingSubsetData) {
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.TEMPERATURE_ICON, "Température", buildingSubsetData.Temperature.ToString(), "°", 0);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.HUMIDITY_ICON, "Humidité", buildingSubsetData.Humidity.ToString(), "%", 1);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.LUMINOSITY_ICON, "Luminosté", buildingSubsetData.Luminosity.ToString(), "lux", 2);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.CO2_ICON, "CO2", buildingSubsetData.Co2.ToString(), "ppm", 3);
		this.BuildBuildingSensorIndicator(dataBox, IconsAndTextures.PRESENCE_ICON, "Présence", buildingSubsetData.Presence ? "Oui" : "Non", "", 4);

		return dataBox;
	}

	private GameObject BuildBuildingSensorIndicator(GameObject dataBox, string iconPath, string sensorName, string sensorValue, string sensorUnit, int index) {
		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		string subsetName = dataBox.name.Split('_')[1];

		float titleLabelWidth = this.TextWidth(sensorName, arialFont) + BUILDING_DATA_INDICATOR_ICON_SIZE;
		float valueLabelWidth = this.TextWidth(sensorValue + sensorUnit, arialFont) - 0.002F;

		float sensorIndicatorPaddingY = BUILDING_DATA_INDICATOR_EXT_PADDING + BUILDING_DATA_HEADER_HEIGHT;
		float sensorIndicatorPosY = -(index * (BUILDING_DATA_INDICATOR_HEIGHT + (index == 0 ? 0 : BUILDING_DATA_INDICATOR_INT_PADDING)) + sensorIndicatorPaddingY);

		float iconCenter = BUILDING_DATA_INDICATOR_HEIGHT * 0.1F + BUILDING_DATA_INDICATOR_ICON_SIZE / 2F;

		GameObject sensorIndicator = new GameObject(sensorName + " Indicator_" + subsetName);
		sensorIndicator.transform.SetParent(dataBox.transform, false);
		sensorIndicator.transform.localPosition = new Vector3(BUILDING_DATA_INDICATOR_EXT_PADDING, sensorIndicatorPosY, 0);

		Vector2 titleLabelDimensions = new Vector2(titleLabelWidth, -BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject sensorTitleLabel = this.BuildNew3DRectangle(sensorIndicator, sensorName + "TitleLabel_" + subsetName, Vector3.zero, titleLabelDimensions, ThemeColors.BRIGHT_BLUE);

		Vector3 titleLabelTextPosition = new Vector3(BUILDING_DATA_INDICATOR_HEIGHT * 0.2F + BUILDING_DATA_INDICATOR_ICON_SIZE, -BUILDING_DATA_INDICATOR_HEIGHT / 2F, -0.01F);
		GameObject sensorTitleLabelText = this.BuildNew3DText(sensorTitleLabel, sensorName + "TitleLabelText_" + subsetName, titleLabelTextPosition, sensorName, arialFont, BUILDING_DATA_INDICATOR_HEIGHT / 2F);

		Vector2 valueLabelDimensions = new Vector2(valueLabelWidth, -BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject sensorValueLabel = this.BuildNew3DRectangle(sensorIndicator, sensorName + "ValueLabe_" + subsetName, new Vector3(titleLabelWidth, 0, 0), valueLabelDimensions, Color.white);

		Vector3 valueLabelTextPosition = new Vector3(BUILDING_DATA_INDICATOR_HEIGHT * 0.2F, -BUILDING_DATA_INDICATOR_HEIGHT / 2F, -0.01F);
		string valueLabelTextName = sensorName + "ValueLabelText_" + subsetName;
		GameObject sensorValueLabelText = this.BuildNew3DText(sensorValueLabel, valueLabelTextName, valueLabelTextPosition, sensorValue + sensorUnit, arialFont, BUILDING_DATA_INDICATOR_HEIGHT / 2F);

		string sensorIconName = sensorName + "Icon_" + subsetName;
		Vector3 sensorIconPosition = new Vector3(iconCenter, -iconCenter, -0.01F);
		Vector2 sensorIconVerticesStartPosition = new Vector2(-BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, -BUILDING_DATA_INDICATOR_ICON_SIZE / 2F);
		Vector2 sensorIconDimensions = new Vector2(BUILDING_DATA_INDICATOR_ICON_SIZE / 2F, BUILDING_DATA_INDICATOR_ICON_SIZE / 2F);
		GameObject sensorIcon = this.BuildNew3DRectangle(sensorIndicator, sensorIconName, sensorIconPosition, sensorIconVerticesStartPosition, sensorIconDimensions, iconPath);

		MeshFilter iconShape = sensorIcon.GetComponent<MeshFilter>();
		Texture newTexture = Resources.Load<Texture>(iconPath);

		float ratio = newTexture.width / (newTexture.height * 1F);
		if (ratio > 1)
			iconShape.mesh.uv = this.NewRectangleUv(new Vector2(0, 0.5F - ratio / 2F), new Vector2(1, 0.5F + ratio / 2F));
		else
			iconShape.mesh.uv = this.NewRectangleUv(new Vector2(0.5F - ratio / 2F, 0), new Vector2(0.5F + ratio / 2F, 1));

		MeshRenderer iconRenderer = sensorIcon.GetComponent<MeshRenderer>();
		iconRenderer.material = this.NewTextureFill(iconPath);
		iconRenderer.material.mainTexture.wrapMode = TextureWrapMode.Clamp;

		return sensorIndicator;
	}

	private GameObject BuildNew3DRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 dimensions, string spritePath) {
		return this.BuildNew3DRectangle(parent, name, localPosition, Vector2.zero, dimensions, spritePath);
	}

	private GameObject BuildNew3DRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 dimensions, Color color) {
		return this.BuildNew3DRectangle(parent, name, localPosition, Vector2.zero, dimensions, color);
	}

	private GameObject BuildNew3DRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 startPosition, Vector2 dimensions, Color color) {
		GameObject newRectangle = this.NewBasic3DRectangle(parent, name, localPosition, startPosition, dimensions);

		MeshRenderer rectangleRenderer = newRectangle.GetComponent<MeshRenderer>();
		rectangleRenderer.material = this.NewColorFill(color);

		return newRectangle;
	}

	private GameObject BuildNew3DRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 startPosition, Vector2 dimensions, string spritePath) {
		GameObject newRectangle = this.NewBasic3DRectangle(parent, name, localPosition, startPosition, dimensions);

		MeshFilter newRectangleShape = newRectangle.GetComponent<MeshFilter>();
		newRectangleShape.mesh.uv = this.NewRectangleUv(Vector2.zero, Vector2.one);

		MeshRenderer rectangleRenderer = newRectangle.GetComponent<MeshRenderer>();
		rectangleRenderer.material = this.NewTextureFill(spritePath);

		return newRectangle;
	}

	private GameObject NewBasic3DRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 startPosition, Vector2 dimensions) {
		GameObject newRectangle = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
		newRectangle.transform.SetParent(parent.transform, false);
		newRectangle.transform.localPosition = localPosition;

		MeshFilter rectangleShape = newRectangle.GetComponent<MeshFilter>();
		rectangleShape.mesh.vertices = this.New3DRectangleAngles(startPosition, dimensions);
		rectangleShape.mesh.triangles = this.New3DRectangleTriangulation();

		return newRectangle;
	}

	private GameObject BuildNew3DText(GameObject parent, string name, Vector3 localPosition, string text, Font textFont, float characterSize) {
		GameObject sensorTitleLabelText = new GameObject(name, typeof(TextMesh));
		sensorTitleLabelText.transform.SetParent(parent.transform, false);
		sensorTitleLabelText.transform.localPosition = localPosition;

		TextMesh titleLabelTextShape = sensorTitleLabelText.GetComponent<TextMesh>();
		titleLabelTextShape.font = textFont;
		titleLabelTextShape.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);
		titleLabelTextShape.anchor = TextAnchor.MiddleLeft;
		titleLabelTextShape.characterSize = characterSize;
		titleLabelTextShape.text = text;

		return sensorTitleLabelText;
	}

	private float TextWidth(string text, Font arialFont) {
		float res = 0;

		GameObject tempObject = new GameObject("temp", typeof(TextMesh));
		tempObject.GetComponent<TextMesh>().characterSize = BUILDING_DATA_INDICATOR_HEIGHT / 2F;
		tempObject.GetComponent<TextMesh>().text = text;

		foreach (char textChar in text) {
			CharacterInfo charaterInfo = new CharacterInfo();
			arialFont.GetCharacterInfo(textChar, out charaterInfo);
			res += charaterInfo.advance;
		}

		res /= (((400F + (res * 2.5F)) * (0.03F / BUILDING_DATA_INDICATOR_HEIGHT)));
		res += BUILDING_DATA_INDICATOR_HEIGHT * 0.2F;

		GameObject.Destroy(tempObject);

		return res;
	}

	private Vector3[] New3DRectangleAngles(Vector2 startPosition, Vector2 dimensions) {
		return new Vector3[] {
			new Vector3(startPosition.x, startPosition.y, 0),
			new Vector3(dimensions.x, startPosition.y, 0),
			new Vector3(dimensions.x, dimensions.y, 0),
			new Vector3(startPosition.x, dimensions.y, 0),
		};
	}

	private int[] New3DRectangleTriangulation() {
		return new int[] {
			1, 0, 2,
			2, 0, 3
		};
	}

	private Vector2[] NewRectangleUv(Vector2 startPosition, Vector2 dimensions) {
		return new Vector2[] {
			new Vector2(startPosition.x, startPosition.y),
			new Vector2(dimensions.x, startPosition.y),
			new Vector2(dimensions.x, dimensions.y),
			new Vector2(startPosition.x, dimensions.y)
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
