using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

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
			this.LoadComponentsProperties();
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
		Transform dataBuildingDecorations = dataPanel.transform.parent.GetChild(1);
		GameObject iconBackgroundPanel = dataBuildingDecorations.GetChild(1).gameObject;
		iconBackgroundPanel.transform.GetChild(0).gameObject.SetActive(false);
		iconBackgroundPanel.transform.GetChild(1).gameObject.SetActive(true);
	}

	private void StopDataBuildingIconAnimation() {
		Transform dataBuildingDecorations = dataPanel.transform.parent.GetChild(1);
		GameObject iconBackgroundPanel = dataBuildingDecorations.GetChild(1).gameObject;
		iconBackgroundPanel.transform.GetChild(0).gameObject.SetActive(true);
		iconBackgroundPanel.transform.GetChild(1).gameObject.SetActive(false);
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

	private void LoadComponentsProperties() {
		XmlDocument thresholdDocument = new XmlDocument();

		if (File.Exists(FilePaths.COMPONENTS_PROPERTIES_FILE)) {
			thresholdDocument.Load(FilePaths.COMPONENTS_PROPERTIES_FILE);

			foreach (KeyValuePair<string, BuildingRoom> buildingRoomEntry in buildingRooms) {
				BuildingRoom buildingRoom = buildingRoomEntry.Value;

				foreach (KeyValuePair<SensorData, ActuatorController> componentPair in buildingRoomEntry.Value.ComponentPairs) {
					SensorData singleSensorData = componentPair.Key;
					ActuatorController matchingActuatorController = buildingRoom.ComponentPairs[singleSensorData];

					string sensorXPath = XmlTags.THRESHOLDS + "/";
					sensorXPath += XmlTags.BUILDING_COMPONENTS_GROUP + "[@" + XmlAttributes.NAME + "=\"" + name + "\"]" + "/";
					sensorXPath += XmlTags.ROOM_COMPONENTS_GROUP + "[@" + XmlAttributes.NAME + "=\"" + buildingRoom.Name + "\"]" + "/";
					sensorXPath += XmlTags.COMPONENTS + "[@" + XmlAttributes.NAME + "=\"" + singleSensorData.XmlIdentifier + "\"]" + "/";

					string actuatorXPath = sensorXPath;

					sensorXPath += XmlTags.ROOM_SENSOR;
					XmlNode sensorNode = thresholdDocument.SelectSingleNode(sensorXPath);

					if (sensorNode != null)
						this.UpdateSensorThreshold(sensorNode, singleSensorData);

					if (matchingActuatorController != null) {
						actuatorXPath += XmlTags.ROOM_ACTUATOR + "[@" + XmlAttributes.NAME + "=\"" + matchingActuatorController.XmlIdentifier + "\"]";
						XmlNode actuatorNode = thresholdDocument.SelectSingleNode(actuatorXPath);

						if(actuatorNode != null)
							this.UpdateActuatorLimits(actuatorNode, matchingActuatorController);
					}
				}
			}
		}
	}

	private void UpdateSensorThreshold(XmlNode sensorNode, SensorData sensorData) {
		SensorThreshold threshold = new SensorThreshold();
		this.ExtractThresholdCondition(threshold, sensorNode);
		this.ExtractThresholdValues(threshold, sensorNode);

		sensorData.SensorThreshold = threshold;
		if (sensorData.SensorThreshold.ValueOutOfThreshold(sensorData.Value)) {
			isUnderAlert = true;
		}
	}

	private void ExtractThresholdCondition(SensorThreshold threshold, XmlNode sensorNode) {
		XmlAttribute conditionAttribute = sensorNode.Attributes[XmlAttributes.THRESHOLD_CONDITION];
		string condition = conditionAttribute.Value;

		switch (condition) {
		case XmlValues.THRESHOLD_EQUALS_CONDITION:
			threshold.Condition = ThresholdConditions.EQUALS;
			break;
		case XmlValues.THRESHOLD_NOT_EQUALS_CONDITION:
			threshold.Condition = ThresholdConditions.NOT_EQUALS;
			break;
		case XmlValues.THRESHOLD_IN_CONDITION:
			threshold.Condition = ThresholdConditions.IN;
			break;
		case XmlValues.THRESHOLD_OUT_CONDITION:
			threshold.Condition = ThresholdConditions.OUT;
			break;
		}
	}

	private void ExtractThresholdValues(SensorThreshold threshold, XmlNode sensorNode) {
		foreach (XmlNode thresholdValueNode in sensorNode.ChildNodes) {
			XmlAttribute thresholdValueAttribute = thresholdValueNode.Attributes[XmlAttributes.COMPONENT_PROPERTY_VALUE];
			string thresholdValue = thresholdValueAttribute.Value;
			bool thresholdParsingOutcome = false;

			switch (thresholdValueNode.Name) {
			case XmlTags.MIN_THRESHOLD:
				double minValue = double.MinValue;
				thresholdParsingOutcome = double.TryParse(thresholdValue, out minValue);
				if (thresholdParsingOutcome)
					threshold.MinValue = minValue;
				else
					throw new InvalidCastException("La valeur minimale de seuil doit être un nombre.");
				break;
			case XmlTags.MAX_THRESHOLD:
				double maxValue = double.MaxValue;
				thresholdParsingOutcome = double.TryParse(thresholdValue, out maxValue);
				if (thresholdParsingOutcome)
					threshold.MaxValue = maxValue;
				else
					throw new InvalidCastException("La valeur maximale de seuil doit être un nombre.");
				break;
			case XmlTags.FIXED_VALUE_THRESHOLD:
				threshold.AddFixedValue(thresholdValue);
				break;
			}
		}
	}

	private void UpdateActuatorLimits(XmlNode actuatorNode, ActuatorController actuatorController) {
		foreach (XmlNode propertyNode in actuatorNode.ChildNodes) {
			XmlAttribute propertyValueAttribute = propertyNode.Attributes[XmlAttributes.COMPONENT_PROPERTY_VALUE];
			string propertyRawValue = propertyValueAttribute.Value;

			double propertyValue = double.NaN;
			bool parsingOutcome = double.TryParse(propertyRawValue, out propertyValue);
			if (!parsingOutcome) {
				Debug.LogError("La valeur d'une propriété n'est pas un nombre.");
				continue;
			}

			switch (propertyNode.Name) {
			case XmlTags.MIN_ACTUATOR_LIMIT:
				actuatorController.MinValue = propertyValue;
				break;
			case XmlTags.MAX_ACTUATOR_LIMIT:
				actuatorController.MaxValue = propertyValue;
				break;
			case XmlTags.ACTUATOR_STEP:
				actuatorController.DefaultStep = propertyValue;
				break;
			}
		}
	}

	public void ShiftUnitSuffixedValue(GameObject inputObject, float factor) {
		InputField valueInput = inputObject.GetComponent<InputField>();

		string numericalPart = string.Empty;
		MatchCollection numericalMembers = Regex.Matches(valueInput.text, @"[0-9,.-]");
		foreach (Match numericalMember in numericalMembers)
			numericalPart += numericalMember.Value;

		double value = double.Parse(numericalPart);

		GameObject actuatorControl = inputObject.transform.parent.parent.gameObject;

		string roomName = actuatorControl.name.Split('_')[1];
		string rawActuatorIndex = actuatorControl.name.Split('_')[2];
		int actuatorIndex = int.Parse(rawActuatorIndex);

		BuildingRoom matchingRoom = buildingRooms[roomName];
		ActuatorController matchingController = matchingRoom.GetActuatorController(actuatorIndex);

		if ((value > matchingController.MinValue || (value <= matchingController.MinValue && factor > 0))
		 && (value < matchingController.MaxValue || (value >= matchingController.MaxValue && factor < 0))) {
			valueInput.text = valueInput.text.Replace(numericalPart, (value + matchingController.DefaultStep * factor).ToString());
		}
	}

	private IEnumerator RebuildIndicators () {
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

		yield return new WaitForSeconds(0.01F);

		lock (synchronisationLock) {
			foreach (KeyValuePair<string, BuildingRoom> buildingRoomEntry in buildingRooms) {
				uiBuilder.BuildBuidingDataBox(dataPanel, buildingRoomEntry.Value);
			}
		}
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

		GameObject buildingDataIconBackground = dataPanel.transform.parent.GetChild(1).GetChild(1).gameObject;
		GameObject buildingDataIcon = buildingDataIconBackground.transform.GetChild(0).gameObject;

		Image iconBackgroundImage = buildingDataIconBackground.GetComponent<Image>();
		Image iconImage = buildingDataIcon.GetComponent<Image>();

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

		GameObject iconbackground = dataPanel.transform.parent.GetChild(1).GetChild(1).gameObject;
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
		GameObject link = dataPanel.transform.parent.GetChild(1).GetChild(0).gameObject;
		GameObject iconBckground = dataPanel.transform.parent.GetChild(1).GetChild(1).gameObject;
		GameObject icon = iconBckground.transform.GetChild(0).gameObject;

		Image linkImage = link.GetComponent<Image>();
		Image iconBckgroundImage = iconBckground.GetComponent<Image>();
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
}