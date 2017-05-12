using UnityEngine;
using System.Collections;

public class Node {
	private long reference;
	private int index;
	private double latitude;
	private double longitude;

	public Node(double latitude, double longitude) {
		this.reference = 0;
		this.index = -1;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(long reference, double latitude, double longitude) {
		this.reference = reference;
		this.index = -1;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(long reference, int index, double latitude, double longitude) {
		this.reference = reference;
		this.index = index;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public long Reference {
		get { return reference; }
		set { reference = value; }
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

	public string toString(){
		return "Node [" + reference + "/" + index + "] : (" + longitude + " ; " + latitude + ")";
	}
}
