﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuilding {

	private ArrayList nodeGroups;
	private float minlat, maxlat, minlon, maxlon;
	private RoadCreation rc;

	// constructeur
	public ObjectBuilding(Material roadMat){
		rc = new RoadCreation(roadMat);
	}

	// copie d'une liste de groupe de nodes
	public void setNodeGroups(ArrayList nodeG){
		this.nodeGroups = new ArrayList(nodeG);
	}

	//copie des coordonnées en latitude et longitude
	public void setLatLong(float minla, float maxla, float minlo, float maxlo){
		this.minlat = minla;
		this.maxlat = maxla;
		this.minlon = minlo;
		this.maxlon = maxlo;
	}

	// place les nodes dans la scène
	public void buildNodes(){
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){
				// on construit les angles des buildings
				foreach(Node n in ngp.nodes){
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
					cube.transform.position = new Vector3(n.latitude,0, n.longitude);
					cube.name = ""+n.id;
					cube.tag = "BuildingNode";
				}
			}
			if (ngp.isHighway ()) {
				//on construit les nodes des highways
				if(/*ngp.isPrimary() || ngp.isSecondary() || ngp.isTertiary() || ngp.isUnclassified() || */ngp.isResidential())/* || ngp.isService()) *///|| ngp.isFootway())
					foreach (Node n in ngp.nodes) {
						GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
						cube.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
						cube.transform.position = new Vector3 (n.latitude, 0, n.longitude);
						cube.name = "" + n.id;
						cube.tag = "HighwayNode";
					}
			}
		}
	}

	//construction des routes
	public void buildHighways(){
		float x,y,length,width=0,adj,angle;
		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.isHighway () && (ngp.isResidential () /*|| ngp.isPrimary() || ngp.isSecondary() || ngp.isService() || ngp.isUnclassified()*/) ) {
				for (int i = 0; i < ngp.nodes.Count - 1; i++) {
					//on recup les coordonées utiles
					//Calcul des coordonées du milieu du vecteur formé par les 2 nodes consécutives
					x = (ngp.getNode (i).latitude + ngp.getNode (i + 1).latitude) / 2;
					y = (ngp.getNode (i).longitude + ngp.getNode (i + 1).longitude) / 2;
					//Calcul de la norme de ce vecteur (sera la longueur du morceau de route)
					length = Mathf.Sqrt (Mathf.Pow (ngp.getNode (i + 1).latitude - ngp.getNode (i).latitude, 2) + Mathf.Pow (ngp.getNode (i + 1).longitude - ngp.getNode (i).longitude, 2));
					//Calcul de l'angle
					adj = Mathf.Abs(ngp.getNode(i+1).longitude - ngp.getNode(i).longitude);
					angle = (Mathf.Acos(adj/length)*180f/Mathf.PI);

					rc.createRoad (x, y, length, width);
//					GameObject route = GameObject.CreatePrimitive (PrimitiveType.Cube);
//					route.tag = "Highway";
//					route.transform.localScale = new Vector3 (length, 0.002f, 0.1f);
//					route.transform.position = new Vector3 (x, 0, y);
//					route.transform.localEulerAngles = new Vector3 (0, angle+90, 0);
//					Renderer rendRoute = route.GetComponent<Renderer> ();
//					rendRoute.material.mainTexture = Resources.Load ("route") as Texture;
//					route.AddComponent<GetInfos> (); //à voir plus tard si c'est utile...

				}
			}	
		}
	}

	// place les murs dans la scène
	public void buildWalls(){
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){

				if(!ngp.decomposerRight()){
					ngp.decomposerLeft();
				}

				//On créé les murs
				float x, y, length,adj,angle;
				int etages = 5;

				for(int i=0;i<ngp.nodes.Count-1;i++){
					//on recup les coordonées utiles
					x = (ngp.getNode(i).latitude + ngp.getNode(i+1).latitude)/2;
					y = (ngp.getNode(i).longitude + ngp.getNode(i+1).longitude)/2;
					length = Mathf.Sqrt(Mathf.Pow(ngp.getNode(i+1).latitude - ngp.getNode(i).latitude,2) + Mathf.Pow(ngp.getNode(i+1).longitude - ngp.getNode(i).longitude,2));
					adj = Mathf.Abs(ngp.getNode(i+1).longitude - ngp.getNode(i).longitude);
					angle = (Mathf.Acos(adj/length)*180f/Mathf.PI);

					//on positionne le mur
					GameObject mur = GameObject.CreatePrimitive(PrimitiveType.Cube);
					mur.tag = "Wall";
					if(ngp.tags.ContainsKey("etages")){
						etages = int.Parse(ngp.GetTagValue("etages"));
					}
					else{
						etages = 1;
					}
					mur.transform.localScale = new Vector3(length+0.015f,0.1f*etages,0.02f);
					mur.transform.position = new Vector3(x,0.05f*etages, y);
					mur.AddComponent<GetInfos>();

					// on modifie l'angle en fonction de l'ordre des points

					if((ngp.getNode(i).latitude > ngp.getNode(i+1).latitude && ngp.getNode(i).longitude < ngp.getNode(i+1).longitude) 
						|| (ngp.getNode(i).latitude < ngp.getNode(i+1).latitude && ngp.getNode(i).longitude > ngp.getNode(i+1).longitude)){
						mur.transform.localEulerAngles = new Vector3(0,90-angle,0);
					}
					else {
						mur.transform.localEulerAngles = new Vector3(0,angle+90,0);
					}

					mur.name = ngp.id + "_" + ngp.GetTagValue("name") +"_Mur"+ i;
				}
			}	
		}
	}


	// place la caméra et le background dans la scene
	public void buildMainCameraBG(){

		float CamLat, CamLon;

		// On centre la camera 
		CamLat = (minlat + maxlat) / 2;
		CamLon = (minlon + maxlon) / 2;
		GameObject mainCam = new GameObject ();
		mainCam.AddComponent <Camera>();
		Light mainLight = mainCam.AddComponent<Light>();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;
		mainCam.name = "MainCam";
		mainCam.transform.position = new Vector3(CamLat,-5, CamLon);
		mainCam.transform.localEulerAngles = new Vector3(-90,270,0);
		mainCam.AddComponent <CameraController>();

		GameObject background = GameObject.CreatePrimitive(PrimitiveType.Plane);
		background.name = "Background";
		background.transform.position = new Vector3(CamLat,0.5f, CamLon);
		background.transform.localScale = new Vector3(10,10,10);
		background.transform.localEulerAngles = new Vector3(0,0,180);
		background.GetComponent<Renderer>().material.mainTexture = Resources.Load ("bg") as Texture;
		background.GetComponent<Renderer>().material.name = "Texture_Background";
		background.GetComponent<Renderer>().material.color = Color.gray;
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (500,500);
	}
}
