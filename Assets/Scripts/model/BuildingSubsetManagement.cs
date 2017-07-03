using System.Collections.Generic;

public class BuildingSubsetManagement {
	private string name;
	private List<SensorData> sensorData;
	private List<ActuatorController> actuatorControllers;

	public BuildingSubsetManagement(string name) {
		this.Name = name;
		this.SensorData = new List<SensorData>();
	}

	public void AddSensorData(SensorData sensorData) {
		SensorData.Add(sensorData);
	}

	public void AddSensorData(string sensorIdentifier, string sensorName, string value, string unit, string iconPath, string gameObjectTag) {
		SensorData.Add(new SensorData(sensorIdentifier, sensorName, value, unit, iconPath, gameObjectTag, new SensorThreshold()));
	}

	public void RemoveSensorData(SensorData sensorData) {
		SensorData.Remove(sensorData);
	}

	public List<SensorData> SensorData {
		get { return sensorData; }
		set { sensorData = value; }
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public override string ToString() {
		string res = "";
		foreach (SensorData sensorData in SensorData)
			res += sensorData.ToString();
		return res;
	}
}
