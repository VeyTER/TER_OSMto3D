using System.Collections.Generic;
using System.IO;

public class SensorEquippedBuildingBase {
	private Dictionary<string, string> sensorsEquippedBuildings;

	public SensorEquippedBuildingBase(string filePath) {
		this.sensorsEquippedBuildings = new Dictionary<string, string>();
		this.LoadSensorEquippedBuildings(filePath);
	}

	private void LoadSensorEquippedBuildings(string filePath) {
		if (File.Exists(filePath)) {
			StreamReader sensorsEquippedBuildingsFile = new StreamReader(File.Open(filePath, FileMode.Open));

			string lines = sensorsEquippedBuildingsFile.ReadToEnd();
			string[] linesArray = lines.Split('\n');

			foreach (string line in linesArray) {
				string[] lineDevices = line.Split('\t');
				string buildingName = lineDevices[0];
				string buildingIdentifier = lineDevices[1].Replace("\r", "");
				sensorsEquippedBuildings[buildingName] = buildingIdentifier;
			}

			sensorsEquippedBuildingsFile.Close();
		}
	}

	public bool ContainsName(string buildingName) {
		return sensorsEquippedBuildings.ContainsKey(buildingName);
	}

	public string GetEquippedBuilding(string buildingName) {
		return sensorsEquippedBuildings[buildingName];
	}

	public Dictionary<string, string> SensorsEquippedBuildings {
		get { return sensorsEquippedBuildings; }
	}
}