using UnityEngine;
using UnityEditor;

public class BuildingSubsetData {
	private string name;

	private float temperature;
	private float humidity;
	private float luminosity;
	private float co2;
	private bool presence;

	public BuildingSubsetData(string name) {
		this.name = name;

		this.temperature = 0;
		this.humidity = 0;
		this.luminosity = 0;
		this.co2 = 0;
		this.presence = false;
	}

	public BuildingSubsetData(string name, float temperatuere, float humidity, float luminosity, float co2, bool presence) {
		this.name = name;

		this.temperature = temperatuere;
		this.humidity = humidity;
		this.luminosity = luminosity;
		this.co2 = co2;
		this.presence = presence;
	}

	public string Name {
		get { return name; }
		set { name = value; }
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
		return name + " : [" + temperature + "° | " + humidity + "% | " + luminosity + "lux | " + co2 + "ppm | " + (presence ? "Somebody present" : "Nobody present") + "]";
	}
}