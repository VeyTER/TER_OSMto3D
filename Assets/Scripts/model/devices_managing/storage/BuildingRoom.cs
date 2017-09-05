using System.Collections.Generic;

public class BuildingRoom {
	private static uint currentIndex = 0;

	private string name;
	private Dictionary<SensorData, ActuatorController> devicePairs;

	public BuildingRoom(string name) {
		this.name = name;
		this.devicePairs = new Dictionary<SensorData, ActuatorController>();
	}

	public SensorData AddSensorData(SensorData sensorData) {
		devicePairs[sensorData] = null;
		return sensorData;
	}

	public SensorData AddSensorData(string roomName, string xmlIdentifier, string sensorName, string value, string unit, string iconPath) {
		SensorData newSensorData = new SensorData(currentIndex, name, roomName, xmlIdentifier, sensorName, value, unit, iconPath, new SensorThreshold());
		devicePairs[newSensorData] = null;
		currentIndex++;
		return newSensorData;
	}

	public SensorData RemoveSensorData(SensorData sensorData) {
		devicePairs.Remove(sensorData);
		return sensorData;
	}

	public ActuatorController AddActuatorController(SensorData assosiatedSensorData, ActuatorController actuatorController) {
		devicePairs[assosiatedSensorData] = actuatorController;
		return actuatorController;
	}

	public ActuatorController AddActuatorController(SensorData assosiatedSensorData, string roomName, string xmlIdentifier, string value, string unit, string iconsPathPattern) {
		ActuatorController actuatorController = new ActuatorController(currentIndex, name, roomName, xmlIdentifier, value, unit, iconsPathPattern);
		devicePairs[assosiatedSensorData] = actuatorController;
		return actuatorController;
	}

	public ActuatorController RemoveActuatorController(SensorData assosiatedSensorData, ActuatorController actuatorController) {
		devicePairs[assosiatedSensorData] = null;
		return actuatorController;
	}

	public SensorData GetSensorData(int sensorIndex) {
		IEnumerator<SensorData> sensorsEnum = devicePairs.Keys.GetEnumerator();
		for (; sensorsEnum.MoveNext() && sensorsEnum.Current != null && sensorsEnum.Current.Index != sensorIndex;) ;
		return sensorsEnum.Current;
	}

	public ActuatorController GetActuatorController(int actuatorIndex) {
		IEnumerator<ActuatorController> actuatorsEnum = devicePairs.Values.GetEnumerator();
		for (; actuatorsEnum.MoveNext() && (actuatorsEnum.Current == null || (actuatorsEnum.Current != null && actuatorsEnum.Current.Index != actuatorIndex));) ;
		return actuatorsEnum.Current;
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public Dictionary<SensorData, ActuatorController> DevicePairs {
		get { return devicePairs; }
	}

	public override string ToString() {
		string res = "";
		foreach (KeyValuePair<SensorData, ActuatorController> devicesPair in devicePairs) {
			res += devicesPair.Key;
			if (devicesPair.Value != null)
				res += "=>" + devicesPair.Value;
			res += "\n";
		}
		return res;
	}
}
