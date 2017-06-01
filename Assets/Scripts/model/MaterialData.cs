using UnityEngine;
using UnityEditor;

public class MaterialData {
	private string sourceTexturePath;
	private string targetMaterialPath;
	private string readableName;

	public MaterialData(string[] data) {
		if (data.Length == 3) {
			this.sourceTexturePath = data[0];
			this.targetMaterialPath = data[1];
			this.readableName = data[2];
		}
	}

	public MaterialData(string sourceTexturePath, string targetMaterialPath, string readableName) {
		this.sourceTexturePath = sourceTexturePath;
		this.targetMaterialPath = targetMaterialPath;
		this.readableName = readableName;
	}

	public string SourceTexturePath {
		get { return sourceTexturePath; }
		set { sourceTexturePath = value; }
	}

	public string TargetMaterialPath {
		get { return targetMaterialPath; }
		set { targetMaterialPath = value; }
	}

	public string ReadableName {
		get { return readableName; }
		set { readableName = value; }
	}
}