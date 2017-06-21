using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

// Attaché au bâtiment
public class BuildingDataDisplayController : MonoBehaviour {
	private SensorDataLoader sensorsDataLoader;
	private Dictionary<string, BuildingSubsetData> subsetsData;

	private CityBuilder cityBuilder;

	private void Start() {
		this.sensorsDataLoader = new SensorDataLoader(cityBuilder.SensoredBuildings[gameObject.name]);
		this.subsetsData = new Dictionary<string, BuildingSubsetData>();

		this.cityBuilder = CityBuilder.GetInstance();

		UiBuilder uiBuilder = UiBuilder.GetInstance();
		uiBuilder.BuildBuildingDataDisplay(gameObject);

		sensorsDataLoader.LaunchDataLoading();
		this.StartCoroutine( this.WaitSensorData(sensorsDataLoader) );

	}

	private IEnumerator WaitSensorData(SensorDataLoader dataLoader) {
		while (!dataLoader.ReceptionCompleted)
			yield return new WaitForSeconds(0.1F);

		XmlDocument sensorsDataDocument = new XmlDocument();
		XmlNode rootNode = sensorsDataDocument.CreateElement("root");
		rootNode.InnerXml = dataLoader.LastLoadedData;
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
}

	private void SetIndicatorValue(int valueIndex, string[] resultsLines, GameObject indicator, string unit) {
		foreach (string line in resultsLines) {
			Debug.Log(line);
		}

		if (valueIndex < resultsLines.Length) {
			string[] lineTerms = resultsLines[valueIndex].Split(',');

			int flagIndex = Array.IndexOf(lineTerms, "inside");

			if (flagIndex == -1)
				flagIndex = Array.IndexOf(lineTerms, "outside");

			if (flagIndex < lineTerms.Length - 1) {
				string sensorValue = lineTerms[flagIndex + 1];
				Text sensorInputField = indicator.GetComponent<Text>();
				sensorInputField.text = sensorValue + unit;
			}
		} else {
			indicator.GetComponent<Text>().text = "Non disp.";
		}
	}

	//private void LoadSensorsData() {
	//	GameObject temperatureLabel = GameObject.Find(UiNames.TEMPERATURE_INDICATOR_INPUT_TEXT);
	//	GameObject humidityLabel = GameObject.Find(UiNames.HUMIDITY_INDICATOR_INPUT_TEXT);

	//	if (cityBuilder.SensoredBuildings.ContainsKey(name)) {
	//		string buildingIdentifier = cityBuilder.SensoredBuildings[name];

	//		temperatureLabel.GetComponent<Text>().text = "En attente";
	//		humidityLabel.GetComponent<Text>().text = "En attente";

	//		this.LaunchSensorDataLoading(SensorDataLoader.Sensors.TEMPERATURE, buildingIdentifier);
	//		this.LaunchSensorDataLoading(SensorDataLoader.Sensors.HUMIDITY, buildingIdentifier);
	//	} else {
	//		temperatureLabel.GetComponent<Text>().text = "N.R";
	//		humidityLabel.GetComponent<Text>().text = "N.R";
	//	}
	//}
}
