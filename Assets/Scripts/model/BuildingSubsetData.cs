using UnityEngine;
using UnityEditor;

public class BuildingSubsetData {
	private string subsetName;

	private float temperature;
	private float humidity;
	private float luminosity;
	private float co2;
	private bool presence;

	public BuildingSubsetData(string subsetName) {
		this.subsetName = subsetName;

		this.temperature = 0;
		this.humidity = 0;
		this.luminosity = 0;
		this.co2 = 0;
		this.presence = false;
	}

	public BuildingSubsetData(string subsetName, float temperatuere, float humidity, float luminosity, float co2, bool presence) {
		this.subsetName = subsetName;

		this.temperature = temperatuere;
		this.humidity = humidity;
		this.luminosity = luminosity;
		this.co2 = co2;
		this.presence = presence;
	}

	public string SubsetName {
		get { return subsetName; }
		set { subsetName = value; }
	}

	public float Temperature {
		get { return temperature; }
		set { temperature = value; }
	}

	public float Humidity {
		get { return humidity; }
		set { humidity = value; }
	}

	public float Luminosity {
		get { return luminosity; }
		set { luminosity = value; }
	}

	public float Co2 {
		get { return co2; }
		set { co2 = value; }
	}

	public bool Presence {
		get { return presence; }
		set { presence = value; }
	}

	public override string ToString() {
		return subsetName + " : [" + temperature + "° | " + humidity + "% | " + luminosity + "lux | " + co2 + "ppm | " + (presence ? "Somebody present" : "Nobody present") + "]";
	}
}