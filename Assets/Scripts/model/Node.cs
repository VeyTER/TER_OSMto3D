using UnityEngine;

public class Node {
	private string reference;
	private int index;

	private double latitude;
	private double longitude;

	public Node(double latitude, double longitude) {
		this.reference = "0";
		this.index = -1;

		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(string reference, double latitude, double longitude) {
		this.reference = reference;
		this.index = -1;

		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(string reference, int index, double latitude, double longitude) {
		this.reference = reference;
		this.index = index;

		this.latitude = latitude;
		this.longitude = longitude;
	}

	public string GeneratedId() {
		return reference + "/" + index;
	}

	public Vector2 ToVector() {
		return new Vector2((float)longitude, (float)latitude);
	}

	public string Reference {
		get { return reference; }
		set { reference = value; }
	}

	public bool Equals(Node node) {
		return latitude == node.Latitude && longitude == node.Longitude;
	}

	public int Index {
		get { return index; }
		set { index = value; }
	}

	public double Latitude {
		get { return latitude; }
		set { latitude = value; }
	}

	public double Longitude {
		get { return longitude; }
		set { longitude = value; }
	}

	public override string ToString(){
		return "Node [" + reference + "/" + index + "] : (" + longitude + " ; " + latitude + ")";
	}
}
