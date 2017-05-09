using UnityEngine;
using System.Collections;

public class Node {
	private double id;
	private double latitude;
	private double longitude;

	public Node(double id, double latitude, double longitude){
		this.id = id;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public Node(double latitude, double longitude){
		this.id = 0;
		this.latitude = latitude;
		this.longitude = longitude;
	}

	public double Id {
		get { return id; }
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
		return "Node ["+this.id+"] : (" + this.longitude+" ; " + this.latitude+")";
	}
}
