using UnityEngine;
using System.Collections;

public class Node {
	private double reference;
	private double latitude;
	private double longitude;

	public Node(double reference, double latitude, double longitude){
		this.reference = reference;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(double latitude, double longitude){
		this.reference = 0;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public double Reference {
		get { return reference; }
		set { reference = value; }
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
		return "Node [" + reference + "] : (" + longitude + " ; " + latitude + ")";
	}
}
