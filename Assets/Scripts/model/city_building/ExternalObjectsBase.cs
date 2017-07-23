using System.Collections.Generic;
using System.IO;

public class ExternalObjectBase {
	private List<ExternalObject> externalObjects;

	public ExternalObjectBase(string filePath) {
		this.externalObjects = new List<ExternalObject>();
		this.LoadExternalObjects(filePath);
	}

	private void LoadExternalObjects(string filePath) {
		if (File.Exists(filePath)) {
			string objectsFileContent = File.ReadAllText(filePath);
			string[] lines = objectsFileContent.Split('\n');

			foreach (string line in lines) {
				string[] objectData = line.Split('\t');
				ExternalObject externalObject = new ExternalObject(objectData);
				externalObjects.Add(externalObject);
			}
		}
	}

	public int ObjectsCount() {
		return externalObjects.Count;
	}

	public ExternalObject GetObject(int objectIndex) {
		return externalObjects[objectIndex];
	}

	public List<ExternalObject> ExternalObjects {
		get { return externalObjects; }
	}
}