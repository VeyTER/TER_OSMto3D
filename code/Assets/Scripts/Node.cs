using UnityEngine;
using System.Collections;

public class Node {

	public long id;
	public float latitude, longitude;

	public Node(long id, float lon, float lat){
		this.id = id;
		this.latitude = lat * 1000f;
		this.longitude = lon * 1000f;
	}

	public Node(float lat, float lon){
		this.id = 0;
		this.latitude = lat;
		this.longitude = lon;
	}

  
	public string toString(){
		return "Node ["+this.id+"] : ("+this.latitude+" ; "+this.longitude+")"; 
	}

    //Accesseurs de latitutde
    public void setLatitude(float lat)
    {
        this.latitude = lat;
    }

    public float getLatitude()
    {
        return this.latitude;
    }

    //Accesseurs de longitude
    public void setLongitude(float lon)
    {
        this.longitude = lon;
    }

    public float getLongitude()
    {
        return this.longitude;
    }
}
