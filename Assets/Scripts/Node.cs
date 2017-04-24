using UnityEngine;
using System.Collections;

public class Node {
	private double id;
	private double latitude;
	private double longitude;

	//Constructeur
	public Node(double id, double lon, double lat){
		this.id = id;
		this.longitude = lon;
		this.latitude = lat;
	}

	//Constructeur
	public Node(double lon, double lat){
		this.id = 0;
		this.longitude = lon;
		this.latitude = lat;
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
