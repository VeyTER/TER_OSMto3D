using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using System.Text.RegularExpressions;

/// <summary>
///		Contrôle les composants présents sur les bâtiments, c'est à dire les capteurs et les actionneurs. Cette
///		classe lance la récupération des données et actualise l'affichage dès que les données sont récupérées.
/// </summary>
public class BuildingComponentsController : MonoBehaviour {
	private static int RELOADING_PERIOD = 20;

	private enum DisplayState { ICON_ONLY, ICON_ONLY_TO_FULL_DISPLAY, FULL_DISPLAY, FULL_DISPLAY_TO_ICON_ONLY }
	private DisplayState displayState;

	private enum ReceptionStatus { INACTIVE, LOADING, TERMINATED }
	private ReceptionStatus receptionStatus;

	private SensorDataLoader sensorsDataLoader;
	private Dictionary<string, BuildingRoom> buildingRooms;

	private CityBuilder cityBuilder;
	private UiBuilder uiBuilder;

	private GameObject dataPanel;

	private float timeFlag;

	private bool isUnderAlert;

	private readonly object synchronisationLock = new object();

	public void Start() {
		this.displayState = DisplayState.ICON_ONLY;
		this.receptionStatus = ReceptionStatus.INACTIVE;

		this.cityBuilder = CityBuilder.GetInstance();
		this.uiBuilder = UiBuilder.GetInstance();

		this.sensorsDataLoader = new SensorDataLoader(cityBuilder.SensorsEquippedBuildings[gameObject.name]);
		this.buildingRooms = new Dictionary<string, BuildingRoom>();

		this.dataPanel = uiBuilder.BuildBuildingDataPanel(gameObject);
		this.dataPanel.transform.parent.SetParent(uiBuilder.BuildingDataDisplays.transform);

		this.isUnderAlert = false;

		this.ReloadData();
	}

	public void Update() {
		if (Time.time - timeFlag >= RELOADING_PERIOD)
			this.ReloadData();

		if (receptionStatus == ReceptionStatus.TERMINATED)
			this.UpdateDataDisplay();

		this.SetOrientationToCamera();
	}

	private void ReloadData() {
		sensorsDataLoader.StopDataLoading();

		receptionStatus = ReceptionStatus.LOADING;
		sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

		this.StartDataBuildingIconAnimation();

		if(displayState == DisplayState.ICON_ONLY || displayState == DisplayState.FULL_DISPLAY_TO_ICON_ONLY)
			this.StartCoroutine(this.ChangeDisplayIconStatus(IconsTexturesSprites.BUILDING_DATA_LOW_ICON_BACKGROUND, IconsTexturesSprites.BUILDING_DATA_LOW_ICON, ThemeColors.BLUE_BRIGHT));
		else if (displayState == DisplayState.FULL_DISPLAY || displayState == DisplayState.ICON_ONLY_TO_FULL_DISPLAY)
			this.StartCoroutine(this.ChangeDisplayIconStatus(IconsTexturesSprites.BUILDING_DATA_HIGH_ICON_BACKGROUND, IconsTexturesSprites.BUILDING_DATA_HIGH_ICON, ThemeColors.BLUE_BRIGHT));

		this.StartCoroutine(this.ScaleDataBuildingIcon(1));

		timeFlag = Time.time;
	}

	private void UpdateDataDisplay() {
		this.receptionStatus = ReceptionStatus.INACTIVE;

		isUnderAlert = false;
		lock (synchronisationLock) {
			ComponentPropertiesLoader propertiesLoader = ComponentPropertiesLoader.GetInstance();
			propertiesLoader.LoadComponentsProperties(buildingRooms, name, ref isUnderAlert);
		}

		this.StopDataBuildingIconAnimation();
		this.StartCoroutine(this.ScaleDataBuildingIcon(-1));

		if (isUnderAlert) {
			if (displayState == DisplayState.ICON_ONLY || displayState == DisplayState.ICON_ONLY_TO_FULL_DISPLAY) {
				dataPanel.SetActive(true);
				this.StartCoroutine(this.ChangeDisplayHeight(1, null));
				displayState = DisplayState.ICON_ONLY_TO_FULL_DISPLAY;
			}
			this.StartCoroutine(this.ChangeDisplayIconStatus(IconsTexturesSprites.BUILDING_DATA_HIGH_ALERT_ICON_BACKGROUND, IconsTexturesSprites.BUILDING_DATA_HIGH_ALERT_ICON, ThemeColors.RED_BRIGHT));
		}

		Action flipMiddleAction = () => {
			this.StartCoroutine(this.RebuildIndicators());
			if (displayState == DisplayState.ICON_ONLY || displayState == DisplayState.FULL_DISPLAY_TO_ICON_ONLY)
				dataPanel.SetActive(false);
		};
		Action flipEndAction = () => {
			if (displayState == DisplayState.ICON_ONLY || displayState == DisplayState.FULL_DISPLAY_TO_ICON_ONLY)
				this.SetOpacityInHierarchy(0, dataPanel, 0, int.MaxValue);
		};
		this.StartCoroutine(this.FlipDataBoxItems(flipMiddleAction, flipEndAction));
	}

	private void StartDataBuildingIconAnimation() {
		GameObject dataBuildingDecorations = dataPanel.transform.parent.Find(UiNames.BUILDING_DATA_DECORATIONS).gameObject;
		GameObject iconBackgroundPanel = dataBuildingDecorations.transform.Find(UiNames.BUILDING_DATA_ICON_BUTTON).gameObject;
		iconBackgroundPanel.transform.Find(UiNames.BUILDING_DATA_STATIC_ICON).gameObject.SetActive(false);
		iconBackgroundPanel.transform.Find(UiNames.BUILDING_DATA_ANIMATED_ICON).gameObject.SetActive(true);
	}

	private void StopDataBuildingIconAnimation() {
		GameObject dataBuildingDecorations = dataPanel.transform.parent.Find(UiNames.BUILDING_DATA_DECORATIONS).gameObject;
		GameObject iconBackgroundPanel = dataBuildingDecorations.transform.Find(UiNames.BUILDING_DATA_ICON_BUTTON).gameObject;
		iconBackgroundPanel.transform.Find(UiNames.BUILDING_DATA_STATIC_ICON).gameObject.SetActive(true);
		iconBackgroundPanel.transform.Find(UiNames.BUILDING_DATA_ANIMATED_ICON).gameObject.SetActive(false);
	}

	private void SetOrientationToCamera() {
		GameObject dataCanvas = dataPanel.transform.parent.gameObject;

		float deltaX = dataCanvas.transform.position.x - Camera.main.transform.position.x;
		float deltaZ = dataCanvas.transform.position.z - Camera.main.transform.position.z;
		float orientation = (float) Math.Atan2(deltaZ, deltaX) * Mathf.Rad2Deg;

		Quaternion rotation = transform.rotation;
		dataCanvas.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, -orientation + 90, rotation.eulerAngles.z);
	}

	public void ProcessReceivedData(IAsyncResult asynchronousResult) {
		while (!asynchronousResult.IsCompleted);

		SensorDataLoader.RequestState requestState = (SensorDataLoader.RequestState) asynchronousResult.AsyncState;
		string requestResult = requestState.RequestResult();

		requestResult = requestResult.Replace("<pre>", "");
		requestResult = requestResult.Replace("</pre>", "");

		XmlDocument sensorsDataDocument = new XmlDocument();
		XmlNode rootNode = sensorsDataDocument.CreateElement("root");
		rootNode.InnerXml = requestResult;
		sensorsDataDocument.InnerXml = rootNode.InnerText;

		//Debug.Log(sensorsDataDocument.InnerXml);

		this.ExtractSensorsData(sensorsDataDocument);

		this.receptionStatus = ReceptionStatus.TERMINATED;
	}

	private void ExtractSensorsData(XmlDocument sensorsDataDocument) {
		XmlNodeList sensorsData = sensorsDataDocument.GetElementsByTagName(XmlTags.SENSOR_DATA);

		lock (synchronisationLock) {
			buildingRooms.Clear();
			foreach (XmlNode sensorDataNode in sensorsData) {
				if (sensorDataNode.ChildNodes.Count > 0) {
					XmlNode dataRecord = sensorDataNode.FirstChild;
					string sensorPath = sensorDataNode.Attributes[XmlAttributes.TOPIC].Value;

					string subSetIdentifier = sensorPath.Split('/')[1];
					string xmlIdentifier = sensorPath.Split('/')[2];

					BuildingRoom buildingRoom = null;
					if (buildingRooms.ContainsKey(subSetIdentifier)) {
						buildingRoom = buildingRooms[subSetIdentifier];
					} else {
						buildingRoom = new BuildingRoom(subSetIdentifier);
						buildingRooms[subSetIdentifier] = buildingRoom;
					}

					this.UpdateRoomContainer(buildingRoom, sensorsDataDocument, sensorPath, xmlIdentifier);
				}
			}
		}
	}

	private void UpdateRoomContainer(BuildingRoom buildingRoom, XmlDocument sensorsDataDocument, string sensorPath, string xmlIdentifier) {
		string xPath = XmlTags.RESULTS + "/" + XmlTags.SENSOR_DATA + "[@" + XmlAttributes.TOPIC + "=\"" + sensorPath + "\"]" + "/" + XmlTags.SENSOR_DATA_RECORD + "/" + XmlTags.SENSOR_VALUE;
		XmlNode valueNode = sensorsDataDocument.SelectSingleNode(xPath);

		if (valueNode != null) {
			string sensorValue = valueNode.InnerText;

			switch (xmlIdentifier) {
			case Sensors.TEMPERATURE:
				SensorData temperatureSensor = buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, "Température", sensorValue, "°", IconsTexturesSprites.TEMPERATURE_ICON);

				// À changer par l'identifiant du chauffage (changer aussi dans le fichiers XML des propriétés)
				string heatersIdentifier = "heaters";

				ActuatorController heatersController = buildingRoom.AddActuatorController(temperatureSensor, buildingRoom.Name, heatersIdentifier, "0", "°", IconsTexturesSprites.HEATERS_CONTROL_ICON);
				break;
			case Sensors.HUMIDITY:
				buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, "Humidité", sensorValue, "%", IconsTexturesSprites.HUMIDITY_ICON);
				break;
			case Sensors.LUMINOSITY:
				SensorData luminositySensor = buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, "Luminosité", sensorValue, "lux", IconsTexturesSprites.LUMINOSITY_ICON);

				// À changer par l'identifiant des volets (changer aussi dans le fichiers XML des propriétés)
				string shuttersIdentifier = "shutters";

				buildingRoom.AddActuatorController(luminositySensor, buildingRoom.Name, shuttersIdentifier, "0", "%", IconsTexturesSprites.SHUTTERS_CONTROL_ICON);
				break;
			case Sensors.CO2:
				buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, "CO2", sensorValue, "ppm", IconsTexturesSprites.CO2_ICON);
				break;
			case Sensors.PRESENCE:
				buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, "Présence", (sensorValue == "0" ? "Non" : "Oui"), "", IconsTexturesSprites.PRESENCE_ICON);
				break;
			default:
				buildingRoom.AddSensorData(buildingRoom.Name, xmlIdentifier, xmlIdentifier, sensorValue, "unit", IconsTexturesSprites.DEFAULT_SENSOR_ICON);
				break;
			}
		}
	}

	public void ShiftActuatorValue(GameObject inputObject, float factor) {
		InputField valueInput = inputObject.GetComponent<InputField>();
		ActuatorController matchingController = this.TextInputToActuatorController(inputObject);
		ActuatorInputParsingResult parsingResult = this.ExtactActuatorInputValue(valueInput);

		if ((parsingResult.inputValue > matchingController.MinValue || (parsingResult.inputValue <= matchingController.MinValue && factor > 0))
		 && (parsingResult.inputValue < matchingController.MaxValue || (parsingResult.inputValue >= matchingController.MaxValue && factor < 0))) {
			string newActuatorValue = (parsingResult.inputValue + matchingController.DefaultStep * factor).ToString();

			valueInput.text = valueInput.text.Replace(parsingResult.numericalPart, newActuatorValue);
			matchingController.Value = newActuatorValue;
		}
	}

	public void FixActuatorValue(GameObject inputObject) {
		InputField valueInput = inputObject.GetComponent<InputField>();
		ActuatorController matchingController = this.TextInputToActuatorController(inputObject);
		ActuatorInputParsingResult parsingResult = this.ExtactActuatorInputValue(valueInput);

		double newActuatorValue = parsingResult.inputValue;

		if (newActuatorValue == double.NaN)
			newActuatorValue = matchingController.MinValue;

		newActuatorValue = Math.Max(newActuatorValue, matchingController.MinValue);
		newActuatorValue = Math.Min(newActuatorValue, matchingController.MaxValue);
		newActuatorValue = Math.Round(newActuatorValue, 2);

		valueInput.text = newActuatorValue + matchingController.Unit;
		matchingController.Value = newActuatorValue.ToString();
	}

	private ActuatorController TextInputToActuatorController(GameObject inputObject) {
		GameObject uiActuatorControl = inputObject.transform.parent.parent.gameObject;

		string roomName = uiActuatorControl.name.Split('_')[1];
		string rawActuatorIndex = uiActuatorControl.name.Split('_')[2];
		int actuatorIndex = int.Parse(rawActuatorIndex);

		BuildingRoom matchingRoom = buildingRooms[roomName];
		return matchingRoom.GetActuatorController(actuatorIndex);
	}

	private ActuatorInputParsingResult ExtactActuatorInputValue(InputField valueInput) {
		string numericalPart = string.Empty;
		MatchCollection numericalMembers = Regex.Matches(valueInput.text, @"[0-9\,\.\-]");
		foreach (Match numericalMember in numericalMembers)
			numericalPart += numericalMember.Value;

		double inputValue = double.NaN;
		double.TryParse(numericalPart, out inputValue);
		return new ActuatorInputParsingResult(inputValue, numericalPart);
	}

	private IEnumerator RebuildIndicators () {
		this.DestroyIndicators();
		yield return new WaitForSeconds(0.01F);
		this.BuildIndicators();
	}

	private void DestroyIndicators() {
		if (dataPanel.transform.childCount > 0) {
			foreach (Transform dataBoxTransform in dataPanel.transform) {
				HorizontalLayoutGroup[] horizontalLayoutsInChildren = dataBoxTransform.GetComponentsInChildren<HorizontalLayoutGroup>();
				foreach (HorizontalLayoutGroup horizontalLayout in horizontalLayoutsInChildren)
					GameObject.Destroy(horizontalLayout);

				VerticalLayoutGroup[] verticalLayoutsInChildren = dataBoxTransform.GetComponentsInChildren<VerticalLayoutGroup>();
				foreach (VerticalLayoutGroup verticalLayout in verticalLayoutsInChildren)
					GameObject.Destroy(verticalLayout);

				GameObject dataBox = dataBoxTransform.gameObject;
				GameObject.Destroy(dataBox);
			}
		}
	}

	private void BuildIndicators() {
		lock (synchronisationLock) {
			foreach (KeyValuePair<string, BuildingRoom> buildingRoomEntry in buildingRooms) {
				BuildingRoom buildingRoom = buildingRoomEntry.Value;

				uiBuilder.BuildBuidingDataBox(dataPanel, buildingRoom);

				GameObject lastAddedDataBox = dataPanel.transform.GetChild(dataPanel.transform.childCount - 1).gameObject;
				this.UpdateDataBoxTitle(lastAddedDataBox, buildingRoom.Name);

				for (int i = 1; i < lastAddedDataBox.transform.childCount; i++) {
					GameObject indicator = lastAddedDataBox.transform.GetChild(i).gameObject;

					string[] titleComponents = indicator.name.Split('_');
					int indicatorIndex = int.Parse(titleComponents[2]);

					SensorData sensorData = buildingRoom.GetSensorData(indicatorIndex);
					ActuatorController actuatorController = buildingRoom.ComponentPairs[sensorData];

					this.UpdateIndicatorValues(indicator, sensorData);

					if (indicator.transform.childCount == 3) {
						GameObject actuatorControl = indicator.transform.Find(UiNames.ACTUATOR_CONTROL + "_" + buildingRoom.Name + "_" + actuatorController.Index).gameObject;
						this.UpdateControlValues(actuatorControl, actuatorController);
					}
				}
			}
		}
	}

	public void UpdateDataBoxTitle(GameObject dataBox, string title) {
		Text boxTitleText = dataBox.GetComponentInChildren<Text>();
		boxTitleText.text = title;
	}

	public void UpdateIndicatorValues(GameObject sensorIndicator, SensorData singleSensorData) {
		bool sensorUnderAlert = singleSensorData.IsOutOfThreshold();

		GameObject indicatorTitle = sensorIndicator.transform.Find(UiNames.SENSOR_TITLE).gameObject;
		Image titleBackgroundImage = indicatorTitle.GetComponent<Image>();
		titleBackgroundImage.color = sensorUnderAlert ? ThemeColors.RED_BRIGHT : ThemeColors.BLUE;

		GameObject titleIcon = indicatorTitle.transform.Find(UiNames.SENSOR_TITLE_ICON).gameObject;
		Image titleIconImage = titleIcon.GetComponent<Image>();
		titleIconImage.sprite = Resources.Load<Sprite>(singleSensorData.IconPath);
		titleIconImage.color = sensorUnderAlert ? ThemeColors.RED_BRIGHT : ThemeColors.BLUE_BRIGHT;

		GameObject titleText = indicatorTitle.transform.Find(UiNames.SENSOR_TITLE_TEXT).gameObject;
		Text titleTextText = titleText.GetComponentInChildren<Text>();
		titleTextText.text = singleSensorData.SensorName;
		titleTextText.color = sensorUnderAlert ? ThemeColors.RED_TEXT : ThemeColors.GREY_TEXT;

		GameObject indicatorValue = sensorIndicator.transform.Find(UiNames.SENSOR_VALUE).gameObject;
		GameObject valueText = indicatorValue.transform.Find(UiNames.SENSOR_VALUE_TEXT).gameObject;
		Text valueTextText = valueText.GetComponentInChildren<Text>();
		valueTextText.text = singleSensorData.Value + singleSensorData.Unit;
		valueTextText.color = sensorUnderAlert ? ThemeColors.RED_TEXT : ThemeColors.GREY_TEXT;
	}

	public void UpdateControlValues(GameObject actuatorControl, ActuatorController actuatorController) {
		GameObject actuatorActions = actuatorControl.transform.Find(UiNames.ACTUATOR_ACTIONS).gameObject;

		GameObject decreaseButton = actuatorActions.transform.Find(UiNames.ACTUATOR_DECREASE_BUTTON).gameObject;
		GameObject decreaseButtonIcon = decreaseButton.transform.Find(UiNames.ACTUATOR_DECREASE_ICON).gameObject;
		Image decreaseIconImage = decreaseButtonIcon.GetComponent<Image>();
		decreaseIconImage.sprite = Resources.Load<Sprite>(actuatorController.ActuatorButtonsPathPattern.Replace("[mode]", "Decrease"));

		GameObject increaseButton = actuatorActions.transform.Find(UiNames.ACTUATOR_INCREASE_BUTTON).gameObject;
		GameObject increaseButtonIcon = increaseButton.transform.Find(UiNames.ACTUATOR_INCREASE_ICON).gameObject;
		Image increaseIconImage = increaseButtonIcon.GetComponent<Image>();
		increaseIconImage.sprite = Resources.Load<Sprite>(actuatorController.ActuatorButtonsPathPattern.Replace("[mode]", "Increase"));

		GameObject valueInput = actuatorActions.transform.Find(UiNames.ACTUATOR_VALUE_INPUT).gameObject;
		InputField valueText = valueInput.GetComponent<InputField>();
		valueText.text = actuatorController.Value + actuatorController.Unit;
	}

	public void ToggleHeightState() {
		if (displayState == DisplayState.ICON_ONLY) {
			dataPanel.SetActive(true);
			this.StartCoroutine(this.ChangeDisplayHeight(1, null));
			displayState = DisplayState.ICON_ONLY_TO_FULL_DISPLAY;
		} else if (displayState == DisplayState.FULL_DISPLAY) {
			this.StartCoroutine(this.ChangeDisplayHeight(-1, () => {
				dataPanel.SetActive(false);
			}));
			displayState = DisplayState.FULL_DISPLAY_TO_ICON_ONLY;
		}
	}


//// ANIMATIONS ////

	private IEnumerator ChangeDisplayHeight(int direction, Action finalAction) {
		float initOrientation = 0;
		float initOpacity = 1;
		float targetHeight = -1;

		float midEndOrientation = 90;
		float midStartOrientation = -90;
		float midOpacity = 0;

		float targetOrientation = 0;
		float targetOpacity = 1;
		float initHeight = -1;

		if (direction > 0) {
			initHeight = UiBuilder.BUILDING_DATA_CANVAS_LOW_HEIGHT;
			targetHeight = UiBuilder.BUILDING_DATA_CANVAS_HIGH_HEIGHT;
		} else if (direction < 0) {
			initHeight = UiBuilder.BUILDING_DATA_CANVAS_HIGH_HEIGHT;
			targetHeight = UiBuilder.BUILDING_DATA_CANVAS_LOW_HEIGHT;
		}

		GameObject dataDisplay = dataPanel.transform.parent.gameObject;
		RectTransform displayRect = (RectTransform) dataDisplay.transform;

		string iconBackgroundPath = UiNames.BUILDING_DATA_DECORATIONS + "/" + UiNames.BUILDING_DATA_ICON_BUTTON;
		GameObject buildingDataIconBackground = dataPanel.transform.parent.Find(iconBackgroundPath).gameObject;
		GameObject buildingDataStaticIcon = buildingDataIconBackground.transform.Find(UiNames.BUILDING_DATA_STATIC_ICON).gameObject;

		Image iconBackgroundImage = buildingDataIconBackground.GetComponent<Image>();
		Image iconImage = buildingDataStaticIcon.GetComponent<Image>();

		Color iconBackgroundColor = iconBackgroundImage.color;
		Color iconColor = iconImage.color;

		Sprite lowIconBackgroundSprite = null;
		Sprite highIconBackgroundSprite = null;
		Sprite lowIconSprite = null;
		Sprite highIconSprite = null;

		if (!isUnderAlert || receptionStatus == ReceptionStatus.LOADING) {
			lowIconBackgroundSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_LOW_ICON_BACKGROUND);
			highIconBackgroundSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_HIGH_ICON_BACKGROUND);
			lowIconSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_LOW_ICON);
			highIconSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_HIGH_ICON);
		} else {
			lowIconBackgroundSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_LOW_ALERT_ICON_BACKGROUND);
			highIconBackgroundSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_HIGH_ALERT_ICON_BACKGROUND);
			lowIconSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_LOW_ALERT_ICON);
			highIconSprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_HIGH_ALERT_ICON);
		}

		double pCursor = -1;
		for (double i = 0; i <= 1; i += 0.05) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentOrientation = -1;
			float currentOpacity = -1;

			if (cursor < 0.5) {
				currentOrientation = initOrientation + (midEndOrientation - initOrientation) * (cursor * 2);
				currentOpacity = initOpacity + (midOpacity - initOpacity) * (cursor * 2);
			} else {
				currentOrientation = midStartOrientation + (targetOrientation - midStartOrientation) * ((cursor - 0.5F) * 2);
				currentOpacity = midOpacity + (targetOpacity - midOpacity) * ((cursor - 0.5F) * 2);
			}

			float currentHeight = initHeight + (targetHeight - initHeight) * cursor;

			Quaternion iconBackgroundRotation = buildingDataIconBackground.transform.localRotation;
			buildingDataIconBackground.transform.localRotation = Quaternion.Euler(0, currentOrientation, 0);

			this.SetDataItemsOrientation(currentOrientation);

			if ((direction > 0 && cursor >= 0.5) || (direction < 0 && cursor <= 0.5))
				this.SetOpacityInHierarchy(currentOpacity, dataPanel, 0, int.MaxValue);

			iconBackgroundImage.color = new Color(iconBackgroundColor.r, iconBackgroundImage.color.g, iconBackgroundImage.color.b, currentOpacity);
			iconImage.color = new Color(iconColor.r, iconColor.g, iconColor.b, currentOpacity);

			if (pCursor < 0.5 && cursor >= 0.5) {
				if (direction > 0) {
					iconBackgroundImage.sprite = highIconBackgroundSprite;
					iconImage.sprite = highIconSprite;
				} else if (direction < 0) {
					this.SetOpacityInHierarchy(0, dataPanel, 0, int.MaxValue);
					iconBackgroundImage.sprite = lowIconBackgroundSprite;
					iconImage.sprite = lowIconSprite;
				}
			}

			displayRect.sizeDelta = new Vector2(displayRect.sizeDelta.x, currentHeight);

			yield return new WaitForSeconds(0.01F);
			pCursor = cursor;
		}

		if (displayState == DisplayState.ICON_ONLY_TO_FULL_DISPLAY)
			displayState = DisplayState.FULL_DISPLAY;
		else if (displayState == DisplayState.FULL_DISPLAY_TO_ICON_ONLY)
			displayState = DisplayState.ICON_ONLY;

		if (finalAction != null)
			finalAction();
	}

	private IEnumerator ScaleDataBuildingIcon(int direction) {
		float initScale = -1;
		float targetScale = -1;

		if (direction > 0) {
			initScale = 1;
			targetScale = 1.25F;
		} else if (direction < 0) {
			initScale = 1.25F;
			targetScale = 1;
		}

		string iconBackgroundPath = UiNames.BUILDING_DATA_DECORATIONS + "/" + UiNames.BUILDING_DATA_ICON_BUTTON;
		GameObject iconbackground = dataPanel.transform.parent.Find(iconBackgroundPath).gameObject;
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentScale = initScale + (targetScale - initScale) * cursor;
			iconbackground.transform.localScale = new Vector3(currentScale, currentScale, cursor);

			yield return new WaitForSeconds(0.01F);
		}
	}

	private IEnumerator FlipDataBoxItems(Action middleTimeAction, Action finalAction) {
		float initOrientation = 0;
		float initOpacity = 1;

		float midEndOrientation = 90;
		float midStartOrientation = -90;
		float midOpacity = 0;

		float targetOrientation = 0;
		float targetOpacity = 1;

		double pCursor = -1;
		for (double i = 0; i <= 1; i += 0.05) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentOrientation = -1;
			float currentOpacity = -1;

			if (cursor < 0.5) {
				currentOrientation = initOrientation + (midEndOrientation - initOrientation) * (cursor * 2);
				currentOpacity = initOpacity + (midOpacity - initOpacity) * (cursor * 2);
			} else {
				currentOrientation = midStartOrientation + (targetOrientation - midStartOrientation) * ((cursor - 0.5F) * 2);
				currentOpacity = midOpacity + (targetOpacity - midOpacity) * ((cursor - 0.5F) * 2);
			}

			if (pCursor < 0.5 && cursor >= 0.5 && middleTimeAction != null) {
				this.SetOpacityInHierarchy(0, dataPanel, 0, int.MaxValue);
				yield return new WaitForSeconds(0.01F);
				middleTimeAction();
			}

			this.SetDataItemsOrientation(currentOrientation);
			this.SetOpacityInHierarchy(currentOpacity, dataPanel, 0, int.MaxValue);

			yield return new WaitForSeconds(0.01F);
			pCursor = cursor;
		}

		finalAction();
	}

	private void SetDataItemsOrientation(float orientation) {
		foreach (Transform dataBoxTransform in dataPanel.transform) {
			foreach (Transform dataItemTransform in dataBoxTransform) {
				Quaternion dataItemRotation = dataItemTransform.localRotation;
				dataItemTransform.localRotation = Quaternion.Euler(orientation, 0, 0);
			}
		}
	}

	private void SetOpacityInHierarchy(float opacity, GameObject parentElement, int currentDeth, int maxDeth) {
		Image elementImage = parentElement.GetComponent<Image>();
		Text elementText = parentElement.GetComponent<Text>();

		if (elementImage != null) {
			Color imageColor = elementImage.color;
			elementImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, opacity);
		}

		if (elementText != null) {
			Color textColor = elementText.color;
			elementText.color = new Color(textColor.r, textColor.g, textColor.b, opacity);
		}

		if (parentElement.transform.childCount > 0 && currentDeth < maxDeth) {
			foreach (Transform dataElementTransform in parentElement.transform) {
				this.SetOpacityInHierarchy(opacity, dataElementTransform.gameObject, currentDeth + 1, maxDeth);
			}
		}
	}

	private IEnumerator ChangeDisplayIconStatus(string newIconBackgroundPath, string newIconPath, Color newLinkColor) {
		string linkPath = UiNames.BUILDING_DATA_DECORATIONS + "/" + UiNames.BUILDING_DATA_LINK;
		GameObject link = dataPanel.transform.parent.Find(linkPath).gameObject;

		string iconBackgroundPath = UiNames.BUILDING_DATA_DECORATIONS + "/" + UiNames.BUILDING_DATA_ICON_BUTTON;
		GameObject iconBackground = dataPanel.transform.parent.Find(iconBackgroundPath).gameObject;

		GameObject icon = iconBackground.transform.Find(UiNames.BUILDING_DATA_STATIC_ICON).gameObject;

		Image linkImage = link.GetComponent<Image>();
		Image iconBckgroundImage = iconBackground.GetComponent<Image>();
		Image iconImage = icon.GetComponent<Image>();

		Color linkColor = linkImage.color;
		Color iconBckgroundColor = iconBckgroundImage.color;
		Color iconImageColor = iconImage.color;

		Sprite newIconBackgroundSprite = Resources.Load<Sprite>(newIconBackgroundPath);
		Sprite newIconSprite = Resources.Load<Sprite>(newIconPath);

		double pCursor = -1;
		for (double i = 0; i <= 1; i += 0.1) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float[] currentLinkColorLevels = new float[3];
			float currentOpacity = -1;

			currentLinkColorLevels[0] = linkColor.r + (newLinkColor.r - linkColor.r) * cursor;
			currentLinkColorLevels[1] = linkColor.g + (newLinkColor.g - linkColor.g) * cursor;
			currentLinkColorLevels[2] = linkColor.b + (newLinkColor.b - linkColor.b) * cursor;

			if (cursor < 0.5)
				currentOpacity = (float) ((1 - cursor * 2));
			else
				currentOpacity = (float) ((cursor - 0.5F) * 2);

			if (pCursor < 0.5 && cursor >= 0.5) {
				iconBckgroundImage.sprite = newIconBackgroundSprite;
				iconImage.sprite = newIconSprite;
			}

			linkImage.color = new Color(currentLinkColorLevels[0], currentLinkColorLevels[1], currentLinkColorLevels[2]);
			iconBckgroundImage.color = new Color(iconBckgroundColor.r, iconBckgroundColor.g, iconBckgroundColor.b, currentOpacity);
			iconImage.color = new Color(iconImageColor.r, iconImageColor.g, iconImageColor.b, currentOpacity);

			yield return new WaitForSeconds(0.01F);
			pCursor = cursor;
		}
	}

	private class ActuatorInputParsingResult {
		internal double inputValue;
		internal string numericalPart;

		internal ActuatorInputParsingResult(double inputValue, string numericalPart) {
			this.inputValue = inputValue;
			this.numericalPart = numericalPart;
		}
	}
}