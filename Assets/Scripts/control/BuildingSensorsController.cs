using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

public class BuildingSensorsController : MonoBehaviour {
	private SensorDataLoader sensorsDataLoader;
	private Dictionary<string, BuildingSubsetData> subsetsData;

	private CityBuilder cityBuilder;
	private UiBuilder uiBuilder;


	private void Start() {
		this.cityBuilder = CityBuilder.GetInstance();
		this.uiBuilder = UiBuilder.GetInstance();

		this.sensorsDataLoader = new SensorDataLoader(cityBuilder.SensoredBuildings[gameObject.name]);
		this.subsetsData = new Dictionary<string, BuildingSubsetData>();

		GameObject displayPanel = uiBuilder.BuildBuildingDataDisplay(gameObject);

		sensorsDataLoader.LaunchDataLoading();
		this.StartCoroutine(this.WaitAndProcessSensorData(sensorsDataLoader, displayPanel));
	}

	private IEnumerator WaitAndProcessSensorData(SensorDataLoader dataLoader, GameObject displayPanel) {
		while (!dataLoader.ReceptionCompleted)
			yield return new WaitForSeconds(0.1F);

		XmlDocument sensorsDataDocument = new XmlDocument();
		XmlNode rootNode = sensorsDataDocument.CreateElement("root");
		rootNode.InnerXml = dataLoader.LastLoadedData;
		sensorsDataDocument.InnerXml = rootNode.InnerText;

		this.ExtractSensorsData(sensorsDataDocument);

		int dataBoxIndex = 0;
		foreach (KeyValuePair<string, BuildingSubsetData> subsetDataPair in this.subsetsData) {
			uiBuilder.BuildBuidingDataBox(displayPanel, subsetDataPair.Value, dataBoxIndex);
			dataBoxIndex++;
		}
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
}
