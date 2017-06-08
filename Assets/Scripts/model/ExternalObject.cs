﻿using UnityEngine;
using UnityEditor;

public class ExternalObject {
	private string blendFilePath;

	private Vector3 osmPosition;

	private Vector3 position;
	private double orientation;
	private double scale;

	private bool neverUsed;

	public ExternalObject(string[] properties) {
		if (properties.Length == 5) {
			this.blendFilePath = properties[0];

			string[] osmPositionComponents = properties[1].Split(';');
			this.osmPosition = new Vector3(float.Parse(osmPositionComponents[0]), float.Parse(osmPositionComponents[1]), float.Parse(osmPositionComponents[2]));

			string[] positionComponents = properties[2].Split(';');
			this.position = new Vector3(float.Parse(positionComponents[0]), float.Parse(positionComponents[1]), float.Parse(positionComponents[2]));
			this.orientation = double.Parse(properties[3]);
			this.scale = double.Parse(properties[4]);
		}

		this.neverUsed = true;
	}

	public ExternalObject(string blendFilePath, Vector3 osmPosition, Vector3 position, double orientation, double scale) {
		this.blendFilePath = blendFilePath;

		this.osmPosition = osmPosition;

		this.position = position;
		this.orientation = orientation;
		this.scale = scale;

		this.neverUsed = true;
	}

	public string ObjectFilePath {
		get { return blendFilePath; }
		set { blendFilePath = value; }
	}

	public Vector3 OsmPosition {
		get { return osmPosition; }
		set { osmPosition = value; }
	}

	public Vector3 Position {
		get { return position; }
		set { position = value; }
	}

	public double Orientation {
		get { return orientation; }
		set { orientation = value; }
	}

	public double Scale {
		get { return scale; }
		set { scale = value; }
	}

	public bool NeverUsed {
		get { return neverUsed; }
		set { neverUsed = value; }
	}
}