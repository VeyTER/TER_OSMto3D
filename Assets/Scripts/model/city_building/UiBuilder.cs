using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBuilder {
	private static UiBuilder instance;

	private BuildingsTools buildingsTools;

	private GameObject sensorsDisplays;

	private UiBuilder() {
		this.buildingsTools = BuildingsTools.GetInstance();
		this.sensorsDisplays = new GameObject("SensorsData");
	}

	public static UiBuilder GetInstance() {
		if (instance == null)
			instance = new UiBuilder();
		return instance;
	}

	public GameObject BuildBuildingDataPanel(GameObject building) {
		GameObject buildBuildingDataCanvas = this.BuildBuildingDataCanvas(building);

		buildingsTools.AddBuildingAndDataDisplayEntryPair(building, buildBuildingDataCanvas);

		GameObject decorationPanel = buildBuildingDataCanvas.transform.Find(UiNames.BUILDING_DATA_DECORATIONS).gameObject;
		decorationPanel.name = decorationPanel.name;

		GameObject dataPanel = buildBuildingDataCanvas.transform.Find(UiNames.BUILDING_DATA_BODY).gameObject;
		dataPanel.name = dataPanel.name;

		return dataPanel;
	}

	private GameObject BuildBuildingDataCanvas(GameObject building) {
		float buildingHeight = buildingsTools.BuildingHeight(building);
		float scale = 0.01F * Dimensions.SCALE_FACTOR;

		GameObject buildingDataCanvas = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_CANVAS));
		buildingDataCanvas.name = buildingDataCanvas.name.Replace("(Clone)", "") + "_" + building.name;

		buildingDataCanvas.transform.SetParent(building.transform, false);
		buildingDataCanvas.transform.localPosition = new Vector3(0, buildingHeight / 2F, 0);
		buildingDataCanvas.transform.localScale = Vector3.one * scale;

		return buildingDataCanvas;
	}

	public GameObject BuildBuidingDataBox(GameObject dataPanel, BuildingRoom buildingRoom) {
		GameObject dataBox = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_BOX));
		dataBox.name = dataBox.name.Replace("(Clone)", "") + "_" + buildingRoom.Name;
		dataBox.transform.SetParent(dataPanel.transform, false);
		this.BuildBuildingDataBoxContent(dataBox, buildingRoom);
		return dataBox;
	}

	private GameObject BuildBuildingDataBoxContent(GameObject dataBox, BuildingRoom buildingRoom) {
		foreach (KeyValuePair<SensorData, ActuatorController> componentPair in buildingRoom.ComponentPairs) {
			SensorData sensorData = componentPair.Key;
			ActuatorController actuatorController = componentPair.Value;

			GameObject sensorIndicator = this.BuildBuildingSensorIndicator(dataBox, sensorData.Index);
			if (actuatorController != null)
				this.BuildBuildingActuatorControl(sensorIndicator, actuatorController.Index);
		}
		return dataBox;
	}

	private GameObject BuildBuildingSensorIndicator(GameObject dataBox, uint sensorIndex) {
		string roomName = dataBox.name.Split('_')[1];
		GameObject sensorIndicator = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.BUILDING_DATA_INDICATOR));
		sensorIndicator.name = sensorIndicator.name.Replace("(Clone)", "") + "_" + roomName + "_" + sensorIndex;
		sensorIndicator.transform.SetParent(dataBox.transform, false);
		return sensorIndicator;
	}

	private GameObject BuildBuildingActuatorControl(GameObject sensorIndicator, uint actuatorIndex) {
		string roomName = sensorIndicator.name.Split('_')[1];
		GameObject actuatorControl = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.ACTUATOR_CONTROL));
		actuatorControl.name = actuatorControl.name.Replace("(Clone)", "") + "_" + roomName + "_" + actuatorIndex;
		actuatorControl.transform.SetParent(sensorIndicator.transform, false);
		actuatorControl.transform.SetAsFirstSibling();

		return actuatorControl;
	}

	public GameObject BuildMaterialItem(MaterialData materialData, GameObject materialsGridPanel, Dictionary<int, Material> buttonIdToMaterialTable) {
		GameObject materialItem = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(GameObjects.MATERIAL_ITEM));
		materialItem.name = materialItem.name + "_" + materialData.ReadableName;
		materialItem.transform.SetParent(materialsGridPanel.transform, false);

		GameObject materialButton = materialItem.transform.Find(UiNames.MATERIAL_ITEM_BUTTON).gameObject;
		GameObject itemBody = materialButton.transform.Find(UiNames.MATERIAL_ITEM_BODY).gameObject;

		Image bodyImage = itemBody.GetComponent<Image>();
		Sprite textureSprite = Resources.Load<Sprite>(materialData.SourceTexturePath);
		bodyImage.sprite = textureSprite;

		Material targetMaterial = Resources.Load(materialData.TargetMaterialPath) as Material;
		buttonIdToMaterialTable.Add(materialItem.GetInstanceID(), targetMaterial);

		Text titleText = itemBody.GetComponentInChildren<Text>();
		titleText.text = materialData.ReadableName;

		return materialItem;
	}

	public void BuildColorsItem(Color itemColor, GameObject colorsGridPanel, Dictionary<int, Color> buttonIdToColorTable) {
		GameObject colorItem = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(GameObjects.COLOR_ITEM));
		colorItem.name = colorItem.name + "_" + itemColor.ToString();
		colorItem.transform.SetParent(colorsGridPanel.transform, false);

		GameObject colorButton = colorItem.transform.Find(UiNames.COLOR_ITEM_BUTTON).gameObject;
		GameObject itemColorSupport = colorButton.transform.Find(UiNames.COLOR_ITEM_SUPPORT).gameObject;

		Image colorSupportImage = itemColorSupport.GetComponent<Image>();
		if (!itemColor.Equals(Color.white)) {
			colorSupportImage.sprite = null;
			colorSupportImage.color = itemColor;
		}

		buttonIdToColorTable.Add(colorItem.GetInstanceID(), itemColor);
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
}