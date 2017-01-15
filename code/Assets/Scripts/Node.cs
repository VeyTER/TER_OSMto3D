using UnityEngine;
using System.Collections;

public class Node {

	public double id;
	public double latitude, longitude;

	public Node(double id, double lon, double lat){
		this.id = id;
        this.longitude = lon;
        this.latitude = lat;

	}

	public Node(double lon, double lat){
		this.id = 0;
        this.longitude = lon;
        this.latitude = lat; 
 
	}

  
	public string toString(){
		return "Node ["+this.id+"] : ("+this.longitude+" ; "+this.latitude+")"; 
	}

    //Accesseurs de latitutde
    public void setLatitude(double lat)
    {
        this.latitude = lat;
    }

    public double getLatitude()
    {
        return this.latitude;
    }

    //Accesseurs de longitude
    public void setLongitude(double lon)
    {
        this.longitude = lon;
    }

    public double getLongitude()
    {
        return this.longitude;
    }
}
