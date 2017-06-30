using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBuilder {
	public static float BUILDING_DATA_CANVAS_SCALE = 0.01F;

	public static float BUILDING_DATA_CANVAS_LOW_HEIGHT = 50;
	public static float BUILDING_DATA_CANVAS_HIGH_HEIGHT = 200;

	public static float BUILDING_DATA_LINK_WIDTH = 1F;

	public static float BUILDING_DATA_HEADER_HEIGHT = 8F;
	public static float BUILDING_DATA_HEADER_LENGTH = 50F;

	public static float BUILDING_DATA_ICON_SIZE = BUILDING_DATA_HEADER_HEIGHT * 2.5F;

	public static float BUILDING_DATA_INDICATOR_HEIGHT = 8F;
	public static float BUILDING_DATA_INDICATOR_RECT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.2F;
	public static float BUILDING_DATA_INDICATOR_TEXT_PADDING = BUILDING_DATA_INDICATOR_HEIGHT * 0.45F;

	public static float BUILDING_DATA_INDICATOR_ICON_SIZE = BUILDING_DATA_INDICATOR_HEIGHT * 0.5F;

	private GameObject sensorsDisplays;

	private UiBuilder() {
		this.sensorsDisplays = new GameObject("SensorsData");
	}

	public static UiBuilder GetInstance() {
		return UiBuilderHolder.instance;
	}

	public GameObject BuildBuildingDataPanel(GameObject building, string buildingName) {
		GameObject BuildBuildingDataCanvas = this.BuildBuildingDataCanvas(building);

		BuildingsTools buildingTools = BuildingsTools.GetInstance();
		buildingTools.AddBuildingToDataDisplayEntry(building, BuildBuildingDataCanvas);
		buildingTools.AddDataDisplayEntryToBuilding(BuildBuildingDataCanvas, building);

		GameObject decorationPanel = this.BuildBuildingDataDecoration(BuildBuildingDataCanvas, buildingName);

		Vector2 dataPanelLocPosition = new Vector3(BUILDING_DATA_LINK_WIDTH, -BUILDING_DATA_ICON_SIZE, 0);
		GameObject dataPanel = this.NewUiRectangle(BuildBuildingDataCanvas, "Data_" + buildingName, dataPanelLocPosition, Vector2.zero, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1));

		VerticalLayoutGroup dataPanelVertLayoutGroup = dataPanel.AddComponent<VerticalLayoutGroup>();
		dataPanelVertLayoutGroup.childControlWidth = false;
		dataPanelVertLayoutGroup.childControlHeight = true;
		dataPanelVertLayoutGroup.childForceExpandWidth = false;
		dataPanelVertLayoutGroup.childForceExpandHeight = false;
		dataPanelVertLayoutGroup.padding = new RectOffset(0, 0, (int)BUILDING_DATA_INDICATOR_RECT_PADDING * 2, 0);

		return dataPanel;
	}

	private GameObject BuildBuildingDataCanvas(GameObject building) {
		Transform buildingFirstWallTransform = building.transform.GetChild(0);
		float buildingHeight = buildingFirstWallTransform.localScale.y;

		GameObject newCanvas = new GameObject("Canvas_" + building.name);
		newCanvas.transform.SetParent(building.transform, false);
		newCanvas.transform.localPosition = new Vector3(0, buildingHeight / 2F, 0);
		newCanvas.transform.localScale = Vector3.one * BUILDING_DATA_CANVAS_SCALE;

		Canvas canvasSettings = newCanvas.AddComponent<Canvas>();
		canvasSettings.renderMode = RenderMode.WorldSpace;

		CanvasScaler canvasScaler = newCanvas.AddComponent<CanvasScaler>();
		canvasScaler.dynamicPixelsPerUnit = 6;

		newCanvas.AddComponent<GraphicRaycaster>();

		RectTransform newCanvasRect = (RectTransform) newCanvas.transform;
		newCanvasRect.sizeDelta = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_CANVAS_LOW_HEIGHT);
		newCanvasRect.pivot = new Vector2(0, 0);

		return newCanvas;
	}

	private GameObject BuildBuildingDataDecoration(GameObject dataCanvas, string buildingName) {
		RectTransform parentRect = (RectTransform) dataCanvas.transform;

		Vector2 decorationsSize = new Vector2(BUILDING_DATA_ICON_SIZE, 0);
		GameObject decorationsPanel = this.NewUiRectangle(dataCanvas, "Decorations_" + buildingName, Vector3.zero, decorationsSize, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1));

		Vector2 linkSize = new Vector2(BUILDING_DATA_LINK_WIDTH, -BUILDING_DATA_ICON_SIZE);
		GameObject link = this.NewUiRectangle(decorationsPanel, "Link_" + buildingName, Vector3.zero, linkSize, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 1), ThemeColors.BRIGHT_BLUE);

		string iconBackgroundName = UiNames.BUILDING_DATA_ICON_BUTTON + "_" + buildingName;
		Vector3 iconBackgroundLocPosition = new Vector3(BUILDING_DATA_LINK_WIDTH / 2F, -BUILDING_DATA_ICON_SIZE, 0);
		Vector2 iconBackgroundSize = new Vector2(BUILDING_DATA_ICON_SIZE, BUILDING_DATA_ICON_SIZE);
		string backgroundPath = IconsTexturesSprites.BUILDING_DATA_LOW_ICON_BACKGROUND;
		GameObject dataBuildingIconBackground = this.NewUiRectangle(decorationsPanel, iconBackgroundName, iconBackgroundLocPosition, iconBackgroundSize, new Vector2(0.5F, 0), new Vector2(0, 1), new Vector2(0, 1), backgroundPath);

		dataBuildingIconBackground.AddComponent<Button>();
		dataBuildingIconBackground.AddComponent<UiManager>();

		Vector2 staticIconSize = new Vector2(-BUILDING_DATA_ICON_SIZE * 0.2F, -BUILDING_DATA_ICON_SIZE * 0.2F);
		string staticIconName = "StaticIcon_" + buildingName;
		string staticIconPath = IconsTexturesSprites.BUILDING_DATA_HIGH_ICON;
		GameObject dataBuildingIcon = this.NewUiRectangle(dataBuildingIconBackground, staticIconName, Vector3.zero, staticIconSize, new Vector2(0.5F, 0.5F), new Vector2(0, 0), new Vector2(1, 1), staticIconPath);
		dataBuildingIcon.transform.localScale = new Vector3(0.75F, 0.75F, 0.75F);

		Vector2 animatedIconSize = new Vector2(-BUILDING_DATA_ICON_SIZE * 0.2F, -BUILDING_DATA_ICON_SIZE * 0.2F);;
		string animatedIconName = "AnimatedIcon_" + buildingName;
		GameObject dataBuildingAnimatedIcon = this.NewUiRectangle(dataBuildingIconBackground, animatedIconName, Vector3.zero, animatedIconSize, new Vector2(0.5F, 0.5F), new Vector2(0, 0), new Vector2(1, 1));
		dataBuildingAnimatedIcon.transform.localScale = new Vector3(2.5F, 2.5F, 2.5F);

		SpriteRenderer animatedIconSpriteRenderer = dataBuildingAnimatedIcon.AddComponent<SpriteRenderer>();

		Animator animatedIconAnimator = dataBuildingAnimatedIcon.AddComponent<Animator>();
		animatedIconAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controllers/BuildingDataLoadingIconController");
		animatedIconAnimator.Play("New Animation");

		dataBuildingAnimatedIcon.SetActive(false);

		return decorationsPanel;
	}

	public GameObject BuildBuidingDataBox(GameObject dataPanel, BuildingSubsetData subsetData) {
		GameObject dataBox = this.NewUiRectangle(dataPanel, "DataBox_" + subsetData.Name, Vector3.zero, Vector2.zero, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
		dataBox.transform.SetParent(dataPanel.transform, false);

		VerticalLayoutGroup dataBoxVertLayoutGroup = dataBox.AddComponent<VerticalLayoutGroup>();
		dataBoxVertLayoutGroup.childControlWidth = false;
		dataBoxVertLayoutGroup.childControlHeight = true;
		dataBoxVertLayoutGroup.childForceExpandWidth = false;
		dataBoxVertLayoutGroup.childForceExpandHeight = false;
		dataBoxVertLayoutGroup.padding = new RectOffset(0, 0, 0, (int)BUILDING_DATA_INDICATOR_TEXT_PADDING);

		this.BuildBuildingDataBoxHeader(dataBox, subsetData, dataPanel.name);
		this.BuildBuildingDataBoxContent(dataBox, subsetData);

		return dataBox;
	}

	private GameObject BuildBuildingDataBoxHeader(GameObject dataBox, BuildingSubsetData buildingSubsetData, string dataPanelName) {
		string headerName = "DataBoxHeader_" + buildingSubsetData.Name;
		Vector2 headerSize = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_HEADER_HEIGHT);
		GameObject header = this.NewUiRectangle(dataBox, headerName, Vector3.zero, headerSize, new Vector2(0, 0.5F), new Vector2(0, 1), new Vector2(1, 1), ThemeColors.BRIGHT_BLUE);
		header.transform.SetParent(dataBox.transform, false);

		HorizontalLayoutGroup headerHorizLayoutGroup = header.AddComponent<HorizontalLayoutGroup>();
		headerHorizLayoutGroup.childControlWidth = true;
		headerHorizLayoutGroup.childControlHeight = true;
		headerHorizLayoutGroup.childForceExpandWidth = true;
		headerHorizLayoutGroup.childForceExpandHeight = false;
		headerHorizLayoutGroup.padding = new RectOffset((int)BUILDING_DATA_INDICATOR_TEXT_PADDING, 0, 0, 0);

		string headerTitleName = dataPanelName + "HeaderText_" + buildingSubsetData.Name;
		GameObject headerTitle = this.NewUiRectangle(dataBox, headerTitleName, Vector3.zero, Vector2.zero, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1), buildingSubsetData.Name, 7);
		headerTitle.transform.SetParent(header.transform, false);

		ContentSizeFitter headerTitleFitter = headerTitle.AddComponent<ContentSizeFitter>();
		headerTitleFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		LayoutElement headerTitleLayout = headerTitle.AddComponent<LayoutElement>();
		headerTitleLayout.minWidth = BUILDING_DATA_HEADER_LENGTH;
		headerTitleLayout.minHeight = BUILDING_DATA_HEADER_HEIGHT * 1.2F;
		headerTitleLayout.preferredWidth = BUILDING_DATA_HEADER_LENGTH;
		headerTitleLayout.preferredHeight = BUILDING_DATA_HEADER_HEIGHT * 1.2F;

		return header;
	}

	private GameObject BuildBuildingDataBoxContent(GameObject dataBox, BuildingSubsetData buildingSubsetData) {
		foreach (SensorData sensorData in buildingSubsetData.SensorsData)
			this.BuildBuildingSensorIndicator(dataBox, sensorData);
		return dataBox;
	}

	private GameObject BuildBuildingSensorIndicator(GameObject dataBox, SensorData sensorData) {
		string subsetName = dataBox.name.Split('_')[1];

		GameObject sensorIndicator = this.BuildBuildingDataIndicatorContainer(dataBox, subsetName, sensorData.IndicatorTag);

		GameObject indicatorTitle = this.BuildBuildingDataIndicatorTitle(sensorIndicator, subsetName, sensorData);
		this.BuildBuildingDataIndicatorValue(sensorIndicator, subsetName, sensorData);

		return sensorIndicator;
	}

	private GameObject BuildBuildingDataIndicatorContainer(GameObject dataBox, string subsetName, string indicatorTag) {
		string indicatorName = "Indicator_" + subsetName;
		Vector3 indicatorPosition = new Vector3(BUILDING_DATA_INDICATOR_RECT_PADDING, 0, 0);
		Vector2 indicatorSize = new Vector2(BUILDING_DATA_HEADER_LENGTH * 3, BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject sensorIndicator = this.NewUiRectangle(dataBox, indicatorName, indicatorPosition, indicatorSize, new Vector2(0, 0.5F), new Vector2(0, 1), new Vector2(0, 1));
		sensorIndicator.transform.SetParent(dataBox.transform, false);
		sensorIndicator.tag = indicatorTag;

		HorizontalLayoutGroup indicatorHorizLayoutGroup = sensorIndicator.AddComponent<HorizontalLayoutGroup>();
		indicatorHorizLayoutGroup.childControlWidth = true;
		indicatorHorizLayoutGroup.childControlHeight = false;
		indicatorHorizLayoutGroup.childForceExpandWidth = true;
		indicatorHorizLayoutGroup.childForceExpandHeight = false;
		indicatorHorizLayoutGroup.padding = new RectOffset((int) BUILDING_DATA_INDICATOR_RECT_PADDING, 0, (int) BUILDING_DATA_INDICATOR_RECT_PADDING, 0);

		ContentSizeFitter indicatorFitter = sensorIndicator.AddComponent<ContentSizeFitter>();
		indicatorFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		return sensorIndicator;
	}

	private GameObject BuildBuildingDataIndicatorTitle(GameObject sensorIndicator, string subsetName, SensorData sensorData) {
		string titleName = "IndicatorTitle_" + sensorData.SensorName;
		Vector2 titleSize = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject indicatorTitle = this.NewUiRectangle(sensorIndicator, titleName, Vector3.zero, titleSize, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), ThemeColors.BLUE);

		string iconName = "IndicatorIcon_" + sensorData.SensorName;
		GameObject indicatorIcon = this.NewUiRectangle(indicatorTitle, iconName, Vector3.zero, Vector2.zero, new Vector2(0.5F, 0.5F), new Vector2(0, 1), new Vector2(0, 1), sensorData.IconPath);
		indicatorIcon.transform.localScale = new Vector3(0.8F, 0.8F, 0.8F);

		LayoutElement iconlayout = indicatorIcon.AddComponent<LayoutElement>();
		iconlayout.minWidth = 10;
		iconlayout.minHeight = 0;
		iconlayout.preferredWidth = 0;
		iconlayout.preferredHeight = 0;

		string titleTextName = "IndicatorTitleText_" + sensorData.SensorName;
		Vector2 titleTextSize = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject indicatorTitleText = this.NewUiRectangle(indicatorTitle, titleTextName, Vector3.zero, titleTextSize, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), sensorData.SensorName, 6);
		indicatorTitleText.transform.SetParent(indicatorTitle.transform, false);

		HorizontalLayoutGroup titleHorizLayoutGroup = indicatorTitle.AddComponent<HorizontalLayoutGroup>();
		titleHorizLayoutGroup.padding = new RectOffset(0, (int) BUILDING_DATA_INDICATOR_TEXT_PADDING, 0, 0);

		ContentSizeFitter titleFitter = indicatorTitle.AddComponent<ContentSizeFitter>();
		titleFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		return indicatorTitle;
	}

	private GameObject BuildBuildingDataIndicatorValue(GameObject sensorIndicator, string subsetName, SensorData sensorData) {
		string valueName = "IndicatorValue_" + sensorData.SensorName;
		Vector2 valueSize = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_INDICATOR_HEIGHT);
		GameObject indicatorValue = this.NewUiRectangle(sensorIndicator, valueName, Vector3.zero, valueSize, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), Color.white);

		string valueTextName = "IndicatorValueText_" + sensorData.SensorName;
		Vector2 valueTextSize = new Vector2(BUILDING_DATA_HEADER_LENGTH, BUILDING_DATA_INDICATOR_HEIGHT);
		string valueTextText = sensorData.Value + sensorData.Unit;
		GameObject indicatorValueText = this.NewUiRectangle(indicatorValue, valueTextName, Vector3.zero, valueTextSize, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), valueTextText, 6);

		HorizontalLayoutGroup valueHorizLayoutGroup = indicatorValue.AddComponent<HorizontalLayoutGroup>();
		valueHorizLayoutGroup.padding = new RectOffset((int) (BUILDING_DATA_INDICATOR_TEXT_PADDING), (int) (BUILDING_DATA_INDICATOR_TEXT_PADDING), 0, 0);

		ContentSizeFitter valueFitter = indicatorValue.AddComponent<ContentSizeFitter>();
		valueFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		return indicatorValue;
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
		rectangleText.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);

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
		titleLabelTextShape.color = new Color(50 / 255F, 50 / 255F, 50 / 255F);
		titleLabelTextShape.anchor = TextAnchor.MiddleLeft;
		titleLabelTextShape.characterSize = characterSize;
		titleLabelTextShape.text = text;

		return sensorTitleLabelText;
	}

	private float Text3DWidth(string text, Font arialFont) {
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
