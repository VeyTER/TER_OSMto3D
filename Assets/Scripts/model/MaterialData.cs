using UnityEngine;
using UnityEditor;

public class MaterialData {
	private string sourceTexturePath;
	private string targetMaterialPath;
	private string readableName;

	public MaterialData(string[] materialData) {
		if (materialData.Length == 3) {
			this.sourceTexturePath = materialData[0];
			this.targetMaterialPath = materialData[1];
			this.readableName = materialData[2];
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