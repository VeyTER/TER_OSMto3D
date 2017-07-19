using System.Collections.Generic;
using System.IO;

public class MapBackgroundBase {
	private Dictionary<string, MapBackground> mapBackgrounds;

	public MapBackgroundBase(string filePath) {
		this.mapBackgrounds = new Dictionary<string, MapBackground>();
		this.LoadMapbackground(filePath);
	}

	private void LoadMapbackground(string filePath) {
		if (File.Exists(filePath)) {
			string backgroundsFileContent = File.ReadAllText(filePath);
			string[] lines = backgroundsFileContent.Split('\n');

			foreach (string line in lines) {
				string[] backgroundData = line.Split('\t');
				MapBackground mapBackground = new MapBackground(backgroundData);
				mapBackgrounds[backgroundData[0]] = mapBackground;
			}
		}
	}

	public MapBackground GetMapBackground(string backgroundName) {
		return mapBackgrounds[backgroundName];
	}

	public Dictionary<string, MapBackground> MapBackgrounds {
		get { return mapBackgrounds; }
	}
}