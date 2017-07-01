using System.Collections.Generic;

public class BuildingSubsetData {
	private string name;
	private List<SensorData> sensorsData;

	public BuildingSubsetData(string name) {
		this.Name = name;
		this.SensorsData = new List<SensorData>();
	}

	public void AddSensorData(SensorData sensorData) {
		SensorsData.Add(sensorData);
	}

	public void AddSensorData(string sensorIdentifier, string sensorName, string value, string unit, string iconPath, string gameObjectTag) {
		SensorsData.Add(new SensorData(sensorIdentifier, sensorName, value, unit, iconPath, gameObjectTag, new SensorThreshold()));
	}

	public void RemoveSensorData(SensorData sensorData) {
		SensorsData.Remove(sensorData);
	}

	public List<SensorData> SensorsData {
		get { return sensorsData; }
		set { sensorsData = value; }
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public override string ToString() {
		string res = "";
		foreach (SensorData sensorData in SensorsData)
			res += sensorData.ToString();
		return res;
	}
}