using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRoom {
	private static uint currentIndex = 0;

	private string name;
	private Dictionary<SensorData, ActuatorController> componentPairs;

	public BuildingRoom(string name) {
		this.name = name;
		this.componentPairs = new Dictionary<SensorData, ActuatorController>();
	}

	public SensorData AddSensorData(SensorData sensorData) {
		componentPairs[sensorData] = null;
		return sensorData;
	}

	public SensorData AddSensorData(string roomName, string xmlIdentifier, string sensorName, string value, string unit, string iconPath) {
		SensorData newSensorData = new SensorData(currentIndex, roomName, xmlIdentifier, sensorName, value, unit, iconPath, new SensorThreshold());
		componentPairs[newSensorData] = null;
		currentIndex++;
		return newSensorData;
	}

	public SensorData RemoveSensorData(SensorData sensorData) {
		componentPairs.Remove(sensorData);
		return sensorData;
	}

	public ActuatorController AddActuatorController(SensorData assosiatedSensorData, ActuatorController actuatorController) {
		componentPairs[assosiatedSensorData] = actuatorController;
		return actuatorController;
	}

	public ActuatorController AddActuatorController(SensorData assosiatedSensorData, string roomName, string xmlIdentifier, string value, string unit, string iconsPathPattern) {
		ActuatorController actuatorController = new ActuatorController(currentIndex, roomName, xmlIdentifier, value, unit, iconsPathPattern);
		componentPairs[assosiatedSensorData] = actuatorController;
		return actuatorController;
	}

	public ActuatorController RemoveActuatorController(SensorData assosiatedSensorData, ActuatorController actuatorController) {
		componentPairs[assosiatedSensorData] = null;
		return actuatorController;
	}

	public SensorData GetSensorData(int sensorIndex) {
		IEnumerator<SensorData> sensorsEnumerator = componentPairs.Keys.GetEnumerator();
		for (sensorsEnumerator.MoveNext(); sensorsEnumerator.MoveNext() && sensorsEnumerator.Current.Index != sensorIndex;) ;
		return sensorsEnumerator.Current;
	}

	public ActuatorController GetActuatorController(int actuatorIndex) {
		IEnumerator<ActuatorController> actuatorsEnumerator = componentPairs.Values.GetEnumerator();

		Debug.Log(componentPairs.Values.Count);

		for (actuatorsEnumerator.MoveNext(); actuatorsEnumerator.MoveNext() && (actuatorsEnumerator.Current == null || actuatorsEnumerator.Current.Index != actuatorIndex);) ;
		return actuatorsEnumerator.Current;
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public Dictionary<SensorData, ActuatorController> ComponentPairs {
		get { return componentPairs; }
	}

	public override string ToString() {
		string res = "";
		foreach (KeyValuePair<SensorData, ActuatorController> componentsPair in componentPairs) {
			res += componentsPair.Key;
			if (componentsPair.Value != null)
				res += "=>" + componentsPair.Value;
			res += "\n";
		}
		return res;
	}
}
