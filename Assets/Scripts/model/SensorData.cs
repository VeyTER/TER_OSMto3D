using UnityEngine;
using UnityEditor;

public class SensorData {
	private string sensorIdentifier;

	private string sensorName;
	private string value;
	private string unit;

	private string iconPath;
	private string indicatorTag;

	public SensorData(string sensorIdentifier, string sensorName, string value, string unit, string iconPath, string indicatorTag) {
		this.sensorIdentifier = sensorIdentifier;

		this.sensorName = sensorName;
		this.Value = value;
		this.Unit = unit;

		this.IconPath = iconPath;
		this.indicatorTag = indicatorTag;
	}

	public string SensorIdentifier {
		get { return sensorIdentifier; }
		set { sensorIdentifier = value; }
	}

	public string SensorName {
		get { return sensorName; }
		set { sensorName = value; }
	}

	public string Value {
		get { return value; }
		set { this.value = value; }
	}

	public string Unit {
		get { return unit; }
		set { unit = value; }
	}

	public string IconPath {
		get { return iconPath; }
		set { iconPath = value; }
	}

	public string IndicatorTag {
		get { return indicatorTag; }
		set { indicatorTag = value; }
	}

	public override string ToString() {
		return sensorIdentifier + "/" + sensorName + " : " + value + unit + "\nIcon : " + iconPath + "\nTag : [" + indicatorTag + "]";
	}
}