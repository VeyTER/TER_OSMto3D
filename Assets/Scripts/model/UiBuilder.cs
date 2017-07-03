using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBuilder {
	public static float BUILDING_DATA_CANVAS_LOW_HEIGHT = 50;
	public static float BUILDING_DATA_CANVAS_HIGH_HEIGHT = 200;

	private GameObject sensorsDisplays;

	private UiBuilder() {
		this.sensorsDisplays = new GameObject("SensorsData");
	}

	public static UiBuilder GetInstance() {
		return UiBuilderHolder.instance;
	}

	public GameObject BuildBuildingDataPanel(GameObject building) {
		GameObject buildBuildingDataCanvas = this.BuildBuildingDataCanvas(building);

		BuildingsTools buildingTools = BuildingsTools.GetInstance();
		buildingTools.AddBuildingToDataDisplayEntry(building, buildBuildingDataCanvas);
		buildingTools.AddDataDisplayEntryToBuilding(buildBuildingDataCanvas, building);

		GameObject decorationPanel = buildBuildingDataCanvas.transform.GetChild(1).gameObject;
		decorationPanel.name = decorationPanel.name + "_" + building.name;

		GameObject dataPanel = buildBuildingDataCanvas.transform.GetChild(0).gameObject;
		dataPanel.name = dataPanel.name + "_" + building.name;

		return dataPanel;
	}

	private GameObject BuildBuildingDataCanvas(GameObject building) {
		Transform buildingFirstWallTransform = building.transform.GetChild(0);
		float buildingHeight = buildingFirstWallTransform.localScale.y;

		GameObject buildBuildingDataCanvas = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_CANVAS));
		buildBuildingDataCanvas.name = buildBuildingDataCanvas.name + "_" + building.name;

		buildBuildingDataCanvas.transform.SetParent(building.transform, false);
		buildBuildingDataCanvas.transform.localPosition = new Vector3(0, buildingHeight / 2F, 0);

		return buildBuildingDataCanvas;
	}

	public GameObject BuildBuidingDataBox(GameObject dataPanel, BuildingSubsetManagement subsetData) {
		GameObject dataBox = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_BOX));
		dataBox.name = dataBox.name + "_" + dataPanel.name.Split('_')[1];
		dataBox.transform.SetParent(dataPanel.transform, false);

		this.BuildBuildingDataBoxHeader(dataBox, subsetData, dataPanel.name);
		this.BuildBuildingDataBoxContent(dataBox, subsetData);

		return dataBox;
	}

	private GameObject BuildBuildingDataBoxHeader(GameObject dataBox, BuildingSubsetManagement buildingSubsetData, string dataPanelName) {
		GameObject header = dataBox.transform.GetChild(0).gameObject;
		header.name = header.name + dataBox.name.Split('_')[1];
		return header;
	}

	private GameObject BuildBuildingDataBoxContent(GameObject dataBox, BuildingSubsetManagement buildingSubsetData) {
		foreach (SensorData singleSensorData in buildingSubsetData.SensorData) {
			GameObject sensorIndicator = this.BuildBuildingSensorIndicator(dataBox);
			this.SetIndicatorValues(sensorIndicator, singleSensorData);
		}
		return dataBox;
	}

	private GameObject BuildBuildingSensorIndicator(GameObject dataBox) {
		string subsetName = dataBox.name.Split('_')[1];
		GameObject sensorIndicator = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_INDICATOR));
		sensorIndicator.transform.SetParent(dataBox.transform, false);
		return sensorIndicator;
	}

	public void SetIndicatorValues(GameObject sensorIndicator, SensorData singleSensorData) {
		bool sensorUnderAlert = singleSensorData.IsOutOfThreshold();

		GameObject indicatorTitle = sensorIndicator.transform.GetChild(0).gameObject;
		Image titleBackgroundImage = indicatorTitle.GetComponent<Image>();
		titleBackgroundImage.color = sensorUnderAlert ? ThemeColors.RED_BRIGHT : ThemeColors.BLUE;

		GameObject titleIcon = indicatorTitle.transform.GetChild(0).gameObject;
		Image titleIconImage = titleIcon.GetComponent<Image>();
		titleIconImage.sprite = Resources.Load<Sprite>(singleSensorData.IconPath);
		titleIconImage.color = sensorUnderAlert ? ThemeColors.RED_BRIGHT : ThemeColors.BLUE_BRIGHT;

		GameObject titleText = indicatorTitle.transform.GetChild(1).gameObject;
		Text titleTextText = titleText.GetComponentInChildren<Text>();
		titleTextText.text = singleSensorData.SensorName;
		titleTextText.color = sensorUnderAlert ? ThemeColors.RED_TEXT : ThemeColors.GREY_TEXT;

		GameObject indicatorValue = sensorIndicator.transform.GetChild(1).gameObject;
		GameObject valueText = indicatorValue.transform.GetChild(0).gameObject;
		Text valueTextText = valueText.GetComponentInChildren<Text>();
		valueTextText.text = singleSensorData.Value + singleSensorData.Unit;
		valueTextText.color = sensorUnderAlert ? ThemeColors.RED_TEXT : ThemeColors.GREY_TEXT;
	}


	private GameObject NewUiRectangle(GameObject parent, string name, RectTransform rectTransform, Color color) {
		GameObject newRectangle = this.NewUiRectangle(parent, name, Vector3.zero, rectTransform.sizeDelta, rectTransform.pivot, rectTransform.anchorMin, rectTransform.anchorMax, color);
		return newRectangle;
	}

	private GameObject NewUiRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax) {
		GameObject newRectangle = this.NewBaseUiRectangle(parent, name, localPosition);
		newRectangle.AddComponent<RectTransform>();

		this.AddNewRectangleRectangleTransform(newRectangle, size, pivot, anchorMin, anchorMax);
		return newRectangle;
	}

	private GameObject NewUiRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Color color) {
		GameObject newRectangle = this.NewBaseUiRectangle(parent, name, localPosition);
		newRectangle.AddComponent<RectTransform>();

		this.AddNewRectangleImage(newRectangle, color);
		this.AddNewRectangleRectangleTransform(newRectangle, size, pivot, anchorMin, anchorMax);
		return newRectangle;
	}

	private GameObject NewUiRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, string imagePath) {
		GameObject newRectangle = this.NewBaseUiRectangle(parent, name, localPosition);
		newRectangle.AddComponent<RectTransform>();

		this.AddNewRectangleImage(newRectangle, imagePath);
		this.AddNewRectangleRectangleTransform(newRectangle, size, pivot, anchorMin, anchorMax);
		return newRectangle;
	}

	private GameObject NewUiRectangle(GameObject parent, string name, Vector3 localPosition, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, string text, int textSize) {
		GameObject newRectangle = this.NewBaseUiRectangle(parent, name, localPosition);
		newRectangle.AddComponent<RectTransform>();

		this.AddNewRectangleText(newRectangle, text, textSize);
		this.AddNewRectangleRectangleTransform(newRectangle, size, pivot, anchorMin, anchorMax);
		return newRectangle;
	}

	private GameObject NewBaseUiRectangle(GameObject parent, string name, Vector3 localPosition) {
		GameObject newRectangle = new GameObject(name);
		if(parent != null)
			newRectangle.transform.SetParent(parent.transform, false);
		newRectangle.transform.localPosition = localPosition;
		newRectangle.AddComponent<CanvasRenderer>();
		return newRectangle;
	}

	private Image AddNewRectangleImage(GameObject rectangle, Color color) {
		Image rectangleImage = rectangle.AddComponent<Image>();
		rectangleImage.color = color;
		return rectangleImage;
	}

	private Image AddNewRectangleImage(GameObject rectangle, string imagePath) {
		Image rectangleImage = rectangle.AddComponent<Image>();
		Sprite imageSprite = Resources.Load<Sprite>(imagePath);
		rectangleImage.sprite = imageSprite;
		rectangleImage.preserveAspect = true;
		return rectangleImage;
	}

	private Text AddNewRectangleText(GameObject rectangle, string text, int textSize) {
		Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

		Text rectangleText = rectangle.AddComponent<Text>();
		rectangleText.text = text;
		rectangleText.fontSize = textSize;
		rectangleText.font = arialFont;
		rectangleText.alignment = TextAnchor.MiddleLeft;
		rectangleText.color = ThemeColors.GREY_TEXT;

		return rectangleText;
	}

	private RectTransform AddNewRectangleRectangleTransform(GameObject rectangle, Vector2 size, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax) {
		RectTransform rectangleRect = (RectTransform) rectangle.transform;
		rectangleRect.sizeDelta = size;
		rectangleRect.pivot = pivot;
		rectangleRect.anchorMin = anchorMin;
		rectangleRect.anchorMax = anchorMax;
		return rectangleRect;
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
		titleLabelTextShape.color = ThemeColors.GREY_TEXT;
		titleLabelTextShape.anchor = TextAnchor.MiddleLeft;
		titleLabelTextShape.characterSize = characterSize;
		titleLabelTextShape.text = text;

		return sensorTitleLabelText;
	}

	private float Text3DWidth(string text, Font arialFont, float referenceSize) {
		float res = 0;

		GameObject tempObject = new GameObject("temp", typeof(TextMesh));
		tempObject.GetComponent<TextMesh>().characterSize = referenceSize / 2F;
		tempObject.GetComponent<TextMesh>().text = text;

		foreach (char textChar in text) {
			CharacterInfo charaterInfo = new CharacterInfo();
			arialFont.GetCharacterInfo(textChar, out charaterInfo);
			res += charaterInfo.advance;
		}

		res /= (((400F + (res * 2.5F)) * (0.03F / referenceSize)));
		res += referenceSize * 0.2F;

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
		return new Material(Shader.Find("Standard")) {
			color = newColor
		};
	}

	private Material NewTextureFill(string texturePath) {
		Texture newTexture = Resources.Load<Texture>(texturePath);
		return new Material(Shader.Find("Sprites/Default")) {
			mainTexture = newTexture
		};
	}

	public GameObject BuildingDataDisplays {
		get { return sensorsDisplays; }
		set { sensorsDisplays = value; }
	}

	private class UiBuilderHolder {
		internal static UiBuilder instance = new UiBuilder();
	}
}
