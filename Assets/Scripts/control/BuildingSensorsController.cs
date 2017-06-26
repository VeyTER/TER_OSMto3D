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

		this.receptionStatus = ReceptionStatus.LOADING;
		sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

		this.timeFlag = Time.time;
	}

	public void Update() {
		if (receptionStatus == ReceptionStatus.TERMINATED) {
			this.receptionStatus = ReceptionStatus.INACTIVE;

			if (dataPanel.transform.childCount == 0)
				this.BuildIndicators();

			this.UpdateIndicators();
		}

		if (Time.time - timeFlag >= 10) {
			this.receptionStatus = ReceptionStatus.LOADING;
			sensorsDataLoader.LaunchDataLoading(new AsyncCallback(this.ProcessReceivedData));

			timeFlag = Time.time;
		}
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
			subsetData.Temperature = float.Parse(sensorValue);
			break;
		case Sensors.HUMIDITY:
			subsetData.Humidity = float.Parse(sensorValue);
			break;
		case Sensors.LUMINOSITY:
			subsetData.Luminosity = float.Parse(sensorValue);
			break;
		case Sensors.CO2:
			subsetData.Co2 = float.Parse(sensorValue);
			break;
		case Sensors.PRESENCE:
			subsetData.Presence = !sensorValue.Equals("0");
			break;
		}
	}

	private void BuildIndicators () {
		int dataBoxIndex = 0;
		foreach (KeyValuePair<string, BuildingSubsetData> subsetDataPair in this.subsetsData) {
			uiBuilder.BuildBuidingDataBox(dataPanel, subsetDataPair.Value, dataBoxIndex);
			dataBoxIndex++;
		}
	}

	private void UpdateIndicators() {
		foreach(Transform dataBox in dataPanel.transform) {
			string subsetName = dataBox.name.Split('_')[1];
			BuildingSubsetData subsetData = subsetsData[subsetName];

			for (int i = 1; i < dataBox.childCount; i++) {
				GameObject indicator = dataBox.GetChild(i).gameObject;

				string indiatorValue = null;
				switch (indicator.tag) {
				case GoTags.TEMPERATURE:
					indiatorValue = subsetData.Temperature.ToString() + "°";
					break;
				case GoTags.HUMIDITY:
					indiatorValue = subsetData.Humidity.ToString() + "%";
					break;
				case GoTags.LUMINOSITY:
					indiatorValue = subsetData.Luminosity.ToString() + "lux";
					break;
				case GoTags.CO2:
					indiatorValue = subsetData.Co2.ToString() + "ppm";
					break;
				case GoTags.PRESENCE:
					indiatorValue = subsetData.Presence ? "oui" : "Non";
					break;
				}

				Text indicatorValueText = indicator.transform.GetChild(1).GetComponentInChildren<Text>();
				indicatorValueText.text = indiatorValue;
			}
		}
	}
}
