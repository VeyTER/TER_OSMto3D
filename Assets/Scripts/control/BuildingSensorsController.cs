using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

public class BuildingSensorsController : MonoBehaviour {
	private enum ReceptionStatus { INACTIVE, LOADING, TERMINATED }
	private ReceptionStatus receptionStatus;

	private SensorDataLoader sensorsDataLoader;
	private Dictionary<string, BuildingSubsetData> subsetsData;

	private CityBuilder cityBuilder;
	private UiBuilder uiBuilder;

	private GameObject dataPanel;

	private float timeFlag;

	public void Start() {
		this.receptionStatus = ReceptionStatus.INACTIVE;

		this.cityBuilder = CityBuilder.GetInstance();
		this.uiBuilder = UiBuilder.GetInstance();

		this.sensorsDataLoader = new SensorDataLoader(cityBuilder.SensoredBuildings[gameObject.name]);
		this.subsetsData = new Dictionary<string, BuildingSubsetData>();

		this.dataPanel = uiBuilder.BuildBuildingDataPanel(gameObject, name);
		this.dataPanel.transform.parent.SetParent(uiBuilder.BuildingDataDisplays.transform);

		this.receptionStatus = ReceptionStatus.LOADING;
		sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

		this.timeFlag = Time.time;
	}

	public void Update() {
		if (receptionStatus == ReceptionStatus.TERMINATED) {
			this.receptionStatus = ReceptionStatus.INACTIVE;
			this.RebuildIndicators();
		}

		if (Time.time - timeFlag >= 10) {
			this.receptionStatus = ReceptionStatus.LOADING;
			sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));
			timeFlag = Time.time;
		}

		GameObject dataCanvas = dataPanel.transform.parent.gameObject;

		float deltaX = dataCanvas.transform.position.x - Camera.main.transform.position.x;
		float deltaZ = dataCanvas.transform.position.z - Camera.main.transform.position.z;
		float orientation = (float)Math.Atan2(deltaZ, deltaX) * Mathf.Rad2Deg;

		Quaternion rotation = transform.rotation;
		dataCanvas.transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, -orientation + 90, rotation.eulerAngles.z);
	}

	public void ProcessReceivedData(IAsyncResult asynchronousResult) {
		while (!asynchronousResult.IsCompleted) ;
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

	private void RebuildIndicators () {
		foreach (Transform dataBoxtransform in dataPanel.transform) {
			HorizontalLayoutGroup[] horizontalLayoutsInChildren = dataBoxtransform.GetComponentsInChildren<HorizontalLayoutGroup>();
			foreach (HorizontalLayoutGroup horizontalLayout in horizontalLayoutsInChildren)
				GameObject.Destroy(horizontalLayout);

			VerticalLayoutGroup[] verticalLayoutsInChildren = dataBoxtransform.GetComponentsInChildren<VerticalLayoutGroup>();
			foreach (VerticalLayoutGroup verticalLayout in verticalLayoutsInChildren)
				GameObject.Destroy(verticalLayout);

			GameObject.Destroy(dataBoxtransform.gameObject);
		}

		foreach (KeyValuePair<string, BuildingSubsetData> subsetDataEntry in this.subsetsData)
			uiBuilder.BuildBuidingDataBox(dataPanel, subsetDataEntry.Value);
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
}
