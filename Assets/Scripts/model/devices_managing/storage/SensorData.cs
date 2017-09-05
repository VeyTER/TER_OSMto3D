public class SensorData : RoomDevice {
	private string sensorName;
	private string iconPath;

	private SensorThreshold sensorThreshold;

	public SensorData(uint index, string buildingName, string roomName, string xmlIdentifier, string sensorName, string value, string unit, string iconPath, SensorThreshold sensorThreshold) :
			base(index, buildingName, roomName, xmlIdentifier, value, unit) {

		this.sensorName = sensorName;
		this.iconPath = iconPath;

		this.sensorThreshold = sensorThreshold;
	}

	public bool IsOutOfThreshold() {
		return sensorThreshold.ValueOutOfThreshold(value);
	}

	public string SensorName {
		get { return sensorName; }
		set { sensorName = value; }
	}

	public string IconPath {
		get { return iconPath; }
		set { iconPath = value; }
	}

	public SensorThreshold SensorThreshold {
		get { return sensorThreshold; }
		set { sensorThreshold = value; }
	}

	public override string ToString() {
		return xmlIdentifier + "/" + sensorName + " : " + value + unit + "\nIcon : " + iconPath;
	}
}