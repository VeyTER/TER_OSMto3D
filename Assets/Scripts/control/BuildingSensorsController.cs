using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

public class BuildingSensorsController : MonoBehaviour {
	private static int RELOADING_FREQUENCY = 30;

	private enum DisplayState { ICON_ONLY, ICON_ONLY_TO_FULL_DISPLAY, FULL_DISPLAY, FULL_DISPLAY_TO_ICON_ONLY }
	private DisplayState displayState;

	private enum ReceptionStatus { INACTIVE, LOADING, TERMINATED }
	private ReceptionStatus receptionStatus;

	private SensorDataLoader sensorsDataLoader;
	private Dictionary<string, BuildingSubsetData> subsetsData;

	private CityBuilder cityBuilder;
	private UiBuilder uiBuilder;

	private GameObject dataPanel;

	private float timeFlag;

	private readonly object synchronisationLock = new object();

	public void Start() {
		this.displayState = DisplayState.ICON_ONLY;
		this.receptionStatus = ReceptionStatus.INACTIVE;

		this.cityBuilder = CityBuilder.GetInstance();
		this.uiBuilder = UiBuilder.GetInstance();

		this.sensorsDataLoader = new SensorDataLoader(cityBuilder.SensoredBuildings[gameObject.name]);
		this.subsetsData = new Dictionary<string, BuildingSubsetData>();

		this.dataPanel = uiBuilder.BuildBuildingDataPanel(gameObject, name);
		this.dataPanel.transform.parent.SetParent(uiBuilder.BuildingDataDisplays.transform);

		this.receptionStatus = ReceptionStatus.LOADING;
		sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

		this.StartDataBuildingIconAnimation();
		this.StartCoroutine(this.ScaleDataBuildingIcon(1));

		this.timeFlag = Time.time;
	}

	public void Update() {
		if (Time.time - timeFlag >= RELOADING_FREQUENCY && this.displayState == DisplayState.FULL_DISPLAY)
			this.ReloadData();

		if (receptionStatus == ReceptionStatus.TERMINATED)
			this.EndDataReloading();

		this.SetOrientationToCamera();
	}

	private void ReloadData() {
		sensorsDataLoader.StopDataLoading();

		receptionStatus = ReceptionStatus.LOADING;
		sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

		this.StartDataBuildingIconAnimation();
		this.StartCoroutine(this.ScaleDataBuildingIcon(1));

		timeFlag = Time.time;
	}

	private void EndDataReloading() {
		this.receptionStatus = ReceptionStatus.INACTIVE;

		this.StopDataBuildingIconAnimation();
		this.StartCoroutine(this.ScaleDataBuildingIcon(-1));

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

	private void SetOrientationToCamera() {
		GameObject dataCanvas = dataPanel.transform.parent.gameObject;

		float deltaX = dataCanvas.transform.position.x - Camera.main.transform.position.x;
		float deltaZ = dataCanvas.transform.position.z - Camera.main.transform.position.z;
		float orientation = (float) Math.Atan2(deltaZ, deltaX) * Mathf.Rad2Deg;

		Quaternion rotation = transform.rotation;
		dataCanvas.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, -orientation + 90, rotation.eulerAngles.z);
	}

	private void StartDataBuildingIconAnimation() {
		Transform dataBuildingDecorations = dataPanel.transform.parent.GetChild(0);
		GameObject iconBackgroundPanel = dataBuildingDecorations.GetChild(1).gameObject;
		iconBackgroundPanel.transform.GetChild(0).gameObject.SetActive(false);
		iconBackgroundPanel.transform.GetChild(1).gameObject.SetActive(true);
	}

	private void StopDataBuildingIconAnimation() {
		Transform dataBuildingDecorations = dataPanel.transform.parent.GetChild(0);
		GameObject iconBackgroundPanel = dataBuildingDecorations.GetChild(1).gameObject;
		iconBackgroundPanel.transform.GetChild(0).gameObject.SetActive(true);
		iconBackgroundPanel.transform.GetChild(1).gameObject.SetActive(false);
	}

	public void ProcessReceivedData(IAsyncResult asynchronousResult) {
		while (!asynchronousResult.IsCompleted);
		this.receptionStatus = ReceptionStatus.TERMINATED;

		SensorDataLoader.RequestState requestState = (SensorDataLoader.RequestState) asynchronousResult.AsyncState;
		string requestResult = requestState.RequestResult();

		requestResult = requestResult.Replace("<pre>", "");
		requestResult = requestResult.Replace("</pre>", "");

		XmlDocument sensorsDataDocument = new XmlDocument();
		XmlNode rootNode = sensorsDataDocument.CreateElement("root");
		rootNode.InnerXml = requestResult;

		sensorsDataDocument.InnerXml = rootNode.InnerText;

		this.ExtractSensorsData(sensorsDataDocument);
	}

	private void ExtractSensorsData(XmlDocument sensorsDataDocument) {
		XmlNodeList sensorsData = sensorsDataDocument.GetElementsByTagName(XmlTags.SENSOR_DATA);

		lock (synchronisationLock) {
			subsetsData.Clear();
			foreach (XmlNode sensorData in sensorsData) {
				if (sensorData.ChildNodes.Count > 0) {
					XmlNode dataRecord = sensorData.FirstChild;
					string sensorPath = sensorData.Attributes[XmlAttributes.TOPIC].Value;

					string subSetIdentifier = sensorPath.Split('/')[1];
					string sensorIdentifier = sensorPath.Split('/')[2];

					BuildingSubsetData subsetData = null;
					if (subsetsData.ContainsKey(subSetIdentifier)) {
						subsetData = subsetsData[subSetIdentifier];
					} else {
						subsetData = new BuildingSubsetData(subSetIdentifier);
						subsetsData[subSetIdentifier] = subsetData;
					}

					this.UpdateSubsetDataContainer(subsetData, sensorsDataDocument, sensorPath, sensorIdentifier);
				}
			}
		}
	}

	private void UpdateSubsetDataContainer(BuildingSubsetData subsetData, XmlDocument sensorsDataDocument, string sensorPath, string sensorIdentifier) {
		string xPath = XmlTags.RESULTS + "/" + XmlTags.SENSOR_DATA + "[@" + XmlAttributes.TOPIC + "=\"" + sensorPath + "\"]" + "/" + XmlTags.SENSOR_DATA_RECORD + "/" + XmlTags.VALUE;
		XmlNode valueNode = sensorsDataDocument.SelectSingleNode(xPath);
		string sensorValue = valueNode.InnerText;

		switch (sensorIdentifier) {
		case Sensors.TEMPERATURE:
			subsetData.AddSensorData(sensorIdentifier, "Températue", sensorValue, "°", IconsTexturesSprites.TEMPERATURE_ICON, GoTags.TEMPERATURE_INDICATOR);
			break;
		case Sensors.HUMIDITY:
			subsetData.AddSensorData(sensorIdentifier, "Humidité", sensorValue, "%", IconsTexturesSprites.HUMIDITY_ICON, GoTags.HUMIDITY_INDICATOR);
			break;
		case Sensors.LUMINOSITY:
			subsetData.AddSensorData(sensorIdentifier, "Luminosité", sensorValue, "lux", IconsTexturesSprites.LUMINOSITY_ICON, GoTags.LUMINOSITY_INDICATOR);
			break;
		case Sensors.CO2:
			subsetData.AddSensorData(sensorIdentifier, "CO2", sensorValue, "ppm", IconsTexturesSprites.CO2_ICON, GoTags.CO2_INDICATOR);
			break;
		case Sensors.PRESENCE:
			subsetData.AddSensorData(sensorIdentifier, "Présence", sensorValue, "", IconsTexturesSprites.PRESENCE_ICON, GoTags.PRESENCE_INDICATOR);
			break;
		default:
			subsetData.AddSensorData(sensorIdentifier, sensorIdentifier, sensorValue, "unit", IconsTexturesSprites.PRESENCE_ICON, GoTags.UNKNOWN_INDICATOR);
			break;
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
			foreach (KeyValuePair<string, BuildingSubsetData> subsetDataEntry in subsetsData) {
				uiBuilder.BuildBuidingDataBox(dataPanel, subsetDataEntry.Value);
				//yield return new WaitForSeconds(0.01F);
			}
		}
	}

	private void UpdateIndicators() {
		foreach (Transform dataBox in dataPanel.transform) {
			string subsetName = dataBox.name.Split('_')[1];
			BuildingSubsetData subsetData = subsetsData[subsetName];

			for (int i = 0; i < subsetData.SensorsData.Count; i++) {
				GameObject indicator = dataBox.GetChild(i + i).gameObject;

				SensorData sensorData = subsetData.SensorsData[i];
				string indicatorText = sensorData.Value + sensorData.Unit;

				Text indicatorValueText = indicator.transform.GetChild(1).GetComponentInChildren<Text>();
				indicatorValueText.text = indicatorText;
			}
		}
	}

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

		GameObject buildingDataIconBackground = dataPanel.transform.parent.GetChild(0).GetChild(1).gameObject;
		GameObject buildingDataIcon = buildingDataIconBackground.transform.GetChild(0).gameObject;

		Image iconBackgroundImage = buildingDataIconBackground.GetComponent<Image>();
		Image iconImage = buildingDataIcon.GetComponent<Image>();

		Color iconBackgroundColor = iconBackgroundImage.color;
		Color iconColor = iconImage.color;

		float pCursor = -1;
		for (double i = 0; i <= 1; i += 0.05) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentOrientation = -1;
			float currentOpacity = -1;

			if (cursor < 0.5F) {
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

			if ((direction > 0 && cursor >= 0.5F) || (direction < 0 && cursor <= 0.5F))
				this.SetOpacityInHierarchy(currentOpacity, dataPanel, 0, int.MaxValue);

			iconBackgroundImage.color = new Color(iconBackgroundColor.r, iconBackgroundImage.color.g, iconBackgroundImage.color.b, currentOpacity);
			iconImage.color = new Color(iconColor.r, iconColor.g, iconColor.b, currentOpacity);

			if (pCursor < 0.5F && cursor >= 0.5F) {
				if (direction > 0) {
					iconBackgroundImage.sprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_ICON_BACKGROUND);
					iconImage.sprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_DATA_ICON);
				} else if (direction < 0) {
					this.SetOpacityInHierarchy(0, dataPanel, 0, int.MaxValue);
					iconBackgroundImage.sprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_INFO_BUTTON_ICON_BACKGROUND);
					iconImage.sprite = Resources.Load<Sprite>(IconsTexturesSprites.BUILDING_INFO_BUTTON_ICON);
				}

				yield return new WaitForSeconds(0.01F);
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

		GameObject iconbackground = dataPanel.transform.parent.GetChild(0).GetChild(1).gameObject;
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

		float pCursor = -1;
		for (double i = 0; i <= 1; i += 0.05) {
			float cursor = (float) Math.Sin(i * (Math.PI) / 2F);

			float currentOrientation = -1;
			float currentOpacity = -1;

			if (cursor < 0.5F) {
				currentOrientation = initOrientation + (midEndOrientation - initOrientation) * (cursor * 2);
				currentOpacity = initOpacity + (midOpacity - initOpacity) * (cursor * 2);
			} else {
				currentOrientation = midStartOrientation + (targetOrientation - midStartOrientation) * ((cursor - 0.5F) * 2);
				currentOpacity = midOpacity + (targetOpacity - midOpacity) * ((cursor - 0.5F) * 2);
			}

			if (pCursor < 0.5F && cursor >= 0.5F && middleTimeAction != null) {
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
}