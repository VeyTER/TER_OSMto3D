using UnityEngine;

public class ExternalObject {
	private string blendFileName;

	private Vector3 osmPosition;

	private Vector3 position;
	private double orientation;
	private double scale;

	private bool neverUsed;

	public ExternalObject(string[] objectData) {
		if (objectData.Length == 5) {
			this.blendFileName = objectData[0];

			string[] osmPositionDevices = objectData[1].Split(';');
			this.osmPosition = new Vector3(float.Parse(osmPositionDevices[0]), float.Parse(osmPositionDevices[1]), float.Parse(osmPositionDevices[2]));

			string[] positionDevices = objectData[2].Split(';');
			this.position = new Vector3(float.Parse(positionDevices[0]), float.Parse(positionDevices[1]), float.Parse(positionDevices[2]));
			this.orientation = double.Parse(objectData[3]);
			this.scale = double.Parse(objectData[4]);
		}

		this.neverUsed = true;
	}

	public ExternalObject(string blendFileName, Vector3 osmPosition, Vector3 position, double orientation, double scale) {
		this.blendFileName = blendFileName;

		this.osmPosition = osmPosition;

		this.position = position;
		this.orientation = orientation;
		this.scale = scale;

		this.neverUsed = true;
	}

	public string ObjectFileName {
		get { return blendFileName; }
		set { blendFileName = value; }
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