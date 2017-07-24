using System.Collections.Generic;
using UnityEngine;

public class Node {
	protected string reference;
	protected int index;

	protected double latitude;
	protected double longitude;

	protected Dictionary<string, string> tags;

	public Node(double latitude, double longitude) {
		this.reference = "0";
		this.index = -1;

		this.latitude = latitude;
		this.longitude = longitude;

		this.tags = new Dictionary<string, string>();
	}

	public Node(string reference, double latitude, double longitude) {
		this.reference = reference;
		this.index = -1;

		this.latitude = latitude;
		this.longitude = longitude;

		this.tags = new Dictionary<string, string>();
	}

	public Node(string reference, int index, double latitude, double longitude) {
		this.reference = reference;
		this.index = index;

		this.latitude = latitude;
		this.longitude = longitude;

		this.tags = new Dictionary<string, string>();
	}

	public Node(Node node) {
		this.reference = node.reference;
		this.index = node.index;

		this.latitude = node.latitude;
		this.longitude = node.longitude;

		this.tags = node.tags;
	}

	public static string GenerateId(Node node) {
		return node.Reference + "/" + node.Index;
	}

	public static string GenerateId(string reference, int index) {
		return reference + "/" + index;
	}

	public void AddTag(string key, string value) {
		tags.Add(key, value);
	}

	public string GetTag(string key) {
		return tags[key];
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

	public Dictionary<string, string> Tags {
		get { return tags; }
		set { tags = value; }
	}

	public override string ToString(){
		return "Node [" + reference + "/" + index + "] : (" + longitude + " ; " + latitude + ")";
	}
}
