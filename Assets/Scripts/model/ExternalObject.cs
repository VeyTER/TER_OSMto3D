using UnityEngine;
using UnityEditor;

public class ExternalObject {
	private string blendFilePath;
	private Vector3 position;

	private bool neverUsed;

	public ExternalObject(string[] properties) {
		if (properties.Length == 2) {
			this.blendFilePath = properties[0];

			string[] positionComponents = properties[1].Split(';');
			this.position = new Vector3(float.Parse(positionComponents[0]), float.Parse(positionComponents[1]), float.Parse(positionComponents[2]));
		}

		this.neverUsed = true;
	}

	public ExternalObject(string blendFilePath, Vector3 position) {
		this.blendFilePath = blendFilePath;
		this.position = position;

		this.neverUsed = true;
	}

	public string BlendFilePath {
		get { return blendFilePath; }
		set { blendFilePath = value; }
	}

	public Vector3 Position {
		get { return position; }
		set { position = value; }
	}

	public bool NeverUsed {
		get { return neverUsed; }
		set { neverUsed = value; }
	}
}