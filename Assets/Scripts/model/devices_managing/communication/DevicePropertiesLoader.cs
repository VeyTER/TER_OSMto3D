using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System;

public class DevicePropertiesLoader {
	private static DevicePropertiesLoader instance;

	public static DevicePropertiesLoader GetInstance() {
		if (instance == null)
			instance = new DevicePropertiesLoader();
		return instance;
	}

	public void LoadDevicesProperties(Dictionary<string, BuildingRoom> buildingRooms, string buildingName, ref bool alertDetected) {
		XmlDocument thresholdDocument = new XmlDocument();

		if (File.Exists(FilePaths.COMPONENTS_PROPERTIES_FILE)) {
			thresholdDocument.Load(FilePaths.COMPONENTS_PROPERTIES_FILE);

			foreach (KeyValuePair<string, BuildingRoom> buildingRoomEntry in buildingRooms) {
				BuildingRoom buildingRoom = buildingRoomEntry.Value;

				foreach (KeyValuePair<SensorData, ActuatorController> devicePair in buildingRoomEntry.Value.DevicePairs) {
					SensorData singleSensorData = devicePair.Key;
					ActuatorController matchingActuatorController = buildingRoom.DevicePairs[singleSensorData];

					string sensorXPath = XmlTags.DEVICES_PROPERTIES + "/";
					sensorXPath += XmlTags.BUILDING_DEVICES_GROUP + "[@" + XmlAttributes.NAME + "=\"" + buildingName + "\"]" + "/";
					sensorXPath += XmlTags.ROOM_DEVICES_GROUP + "[@" + XmlAttributes.NAME + "=\"" + buildingRoom.Name + "\"]" + "/";
					sensorXPath += XmlTags.DEVICES + "[@" + XmlAttributes.NAME + "=\"" + singleSensorData.XmlIdentifier + "\"]" + "/";

					string actuatorXPath = sensorXPath;

					sensorXPath += XmlTags.ROOM_SENSOR;
					XmlNode sensorNode = thresholdDocument.SelectSingleNode(sensorXPath);

					if (sensorNode != null)
						this.UpdateSensorThreshold(sensorNode, singleSensorData, ref alertDetected);

					if (matchingActuatorController != null) {
						actuatorXPath += XmlTags.ROOM_ACTUATOR + "[@" + XmlAttributes.NAME + "=\"" + matchingActuatorController.XmlIdentifier + "\"]";
						XmlNode actuatorNode = thresholdDocument.SelectSingleNode(actuatorXPath);

						if (actuatorNode != null)
							this.UpdateActuatorLimits(actuatorNode, matchingActuatorController);
					}
				}
			}
		}
	}

	private void UpdateSensorThreshold(XmlNode sensorNode, SensorData sensorData, ref bool isUnderAlert) {
		SensorThreshold threshold = new SensorThreshold();
		this.ExtractThresholdCondition(threshold, sensorNode);
		this.ExtractThresholdValues(threshold, sensorNode);

		sensorData.SensorThreshold = threshold;
		if (sensorData.SensorThreshold.ValueOutOfThreshold(sensorData.Value))
			isUnderAlert = true;
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
}