using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectBuilding {

    protected ArrayList nodeGroups;
    protected double minlat, maxlat, minlon, maxlon;
    protected RoadCreation rc;
    protected RoofCreation rfc;
	protected BackgroundCreation bgc;

	// constructeur
	public ObjectBuilding(){
		rc = new RoadCreation();
        rfc = new RoofCreation();
		bgc = new BackgroundCreation ();
	}

    // copie d'une liste de groupe de nodes
    public void setNodeGroups(ArrayList nodeG)
    {
        ArrayList memo = new ArrayList();
        this.nodeGroups = new ArrayList(nodeG);
        foreach (NodeGroup ngp in nodeGroups)
        {
            foreach (Node n in ngp.nodes)
            {
                if (!memo.Contains(n))
                {
                    n.setLatitude(n.getLatitude() * 1000d);
                    n.setLongitude(n.getLongitude() * 1000d);
                    memo.Add(n);
                }
            }
        }
    }

    //copie des coordonnées en latitude et longitude
    public void setLatLong(double minla, double maxla, double minlo, double maxlo){
		this.minlat = minla;
		this.maxlat = maxla;
		this.minlon = minlo;
		this.maxlon = maxlo;
	}

	// place les nodes dans la scène
	public void buildNodes(){
		int j = -1;
		int i;
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){
				// on construit les angles des buildings
				foreach(Node n in ngp.nodes){
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
					cube.transform.position = new Vector3((float)n.getLongitude(),0, (float)n.getLatitude());
					cube.name = ""+n.getID();
					cube.tag = "BuildingNode";
				}
			}
			if (ngp.isHighway ()) {
				//on construit les nodes des highways
				if (ngp.isPrimary() || ngp.isSecondary() || ngp.isTertiary() || ngp.isUnclassified() || ngp.isResidential () || ngp.isService()) {
					j++;
					i = -1;
					foreach (Node n in ngp.nodes) {
						i++;
						GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
						cube.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
						cube.transform.position = new Vector3 ((float)n.getLongitude(), 0, (float)n.getLatitude());
						cube.name = "" + j + "-" + i + "  " + n.getID();
						cube.tag = "HighwayNode";
					}
				}
			}
		}
	}

	//construction des routes
	public void buildHighways(){
		double x,y,length,width=0.06d,angle;
		int j = 0;
		foreach (NodeGroup ngp in nodeGroups) {
			if ( (ngp.isHighway () && (ngp.isResidential () || ngp.isPrimary() || ngp.isSecondary() || ngp.isTertiary() 
				|| ngp.isService() || ngp.isUnclassified() || ngp.isCycleWay() || ngp.isFootway())) 
				|| ngp.isBusWayLane() ) {
				for (int i = 0; i < ngp.nodes.Count-1; i++) {
					//Calcul des coordonées du milieu du vecteur formé par les 2 nodes consécutives
					Vector3 node1 = new Vector3((float)ngp.getNode(i).getLongitude(),0, (float)ngp.getNode(i).getLatitude());
					Vector3 node2 = new Vector3((float)ngp.getNode(i+1).getLongitude(),0, (float)ngp.getNode(i+1).getLatitude());
					Vector3 diff = node2-node1;

					if (diff.z <= 0) {
						x = ngp.getNode (i + 1).getLongitude();
						y = ngp.getNode (i + 1).getLatitude();
						length = Math.Sqrt(Math.Pow(ngp.getNode(i + 1).getLatitude() - ngp.getNode (i).getLatitude(), 2) + Math.Pow(ngp.getNode (i + 1).getLongitude() - ngp.getNode (i).getLongitude(), 2));
						angle = (double)Vector3.Angle(Vector3.right,diff)+180;
					} 
					else {
						x = ngp.getNode(i).getLongitude();
						y = ngp.getNode(i).getLatitude();
						length = Math.Sqrt(Math.Pow(ngp.getNode (i + 1).getLatitude() - ngp.getNode (i).getLatitude(), 2) + Math.Pow(ngp.getNode(i + 1).getLongitude() - ngp.getNode (i).getLongitude(), 2));
						angle = (double)Vector3.Angle(Vector3.right,-diff)+180;
					}
					if (ngp.isHighway () && (ngp.isResidential () || ngp.isPrimary () || ngp.isSecondary () || ngp.isTertiary () || ngp.isService () || ngp.isUnclassified ()))
						rc.createClassicRoad ((float)x, (float)y, (float)length, (float)width, (float)angle, j, i);
					else if (ngp.isFootway ())
						rc.createFootway ((float)x, (float)y, (float)length, (float)width, (float)angle);
					else if (ngp.isCycleWay ())
						rc.createCycleway ((float)x, (float)y, (float)length, (float)width, (float)angle);
//					else if (ngp.isBusWayLane ())
//						rc.createBusLane ((float)x, (float)y, (float)length, (float)width, (float)angle);
				}
				j++;
			}	
		}
	}

	// place les murs dans la scène
	public void buildWalls(){
		
		float epaisseur = 0.01f;

		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){
				if(!ngp.decomposerRight()){
					ngp.decomposerLeft();
				}
				//On créé les murs
				double x, y, length,adj,angle;
				int etages = 5;

				for(int i=0;i<ngp.nodes.Count-1;i++){
					//on recup les coordonées utiles
					x = (ngp.getNode(i).getLongitude() + ngp.getNode(i+1).getLongitude()) /2;
					y = (ngp.getNode(i).getLatitude() + ngp.getNode(i+1).getLatitude()) /2;
					length = Math.Sqrt(Math.Pow(ngp.getNode(i+1).getLatitude() - ngp.getNode(i).getLatitude(),2) + Math.Pow(ngp.getNode(i+1).getLongitude() - ngp.getNode(i).getLongitude(),2));
					adj = Math.Abs(ngp.getNode(i+1).getLatitude() - ngp.getNode(i).getLatitude());
					angle = (Math.Acos(adj/length)*180d/Math.PI);

					//on positionne le mur
					GameObject mur = GameObject.CreatePrimitive(PrimitiveType.Cube);
					mur.tag = "Wall";
					if(ngp.tags.ContainsKey("etages")){
						etages = int.Parse(ngp.GetTagValue("etages"));
					}
					else{
						etages = 1;
					}
					mur.transform.localScale = new Vector3((float)length+0.015f,0.1f*(float)etages,epaisseur);
					mur.transform.position = new Vector3((float)x,0.05f*(float)etages, (float)y);
//					mur.GetComponent<MeshRenderer>().shadowCastingMode = false;
					mur.AddComponent<GetInfos>();

					// on modifie l'angle en fonction de l'ordre des points

					if((ngp.getNode(i).getLatitude() > ngp.getNode(i+1).getLatitude() && ngp.getNode(i).getLongitude() < ngp.getNode(i+1).getLongitude()) 
						|| (ngp.getNode(i).getLatitude() < ngp.getNode(i+1).getLatitude() && ngp.getNode(i).getLongitude() > ngp.getNode(i+1).getLongitude())){
						mur.transform.localEulerAngles = new Vector3(0,90-(float)angle,0);
					}
					else {
						mur.transform.localEulerAngles = new Vector3(0,(float)angle+90,0);
					}

                    //Si on ne connait pas le nom du batiment on utilise l'id
                    if(ngp.getName() == "unknown")
                    {
                        mur.name = ngp.getID() +"_Mur"+ i;
                    }
                    else
                    {
                        mur.name = ngp.getName() + "_Mur" + i;                     
                    }
					MeshRenderer mesh_renderer1 = mur.GetComponent<MeshRenderer> ();
					mesh_renderer1.material = Resources.Load ("Materials/mur") as Material;
					
				}
			}	
		}
	}

	//construction des toits
    public void buildRoofs(TRGDelaunay TRG)
    {
        rfc.createRoof(TRG);
    }

	//construction des arbres
	public void buildTrees(){
		double x, z;
		float height=0.06f,diameter=0.04f;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.isTree() ) {
				x = ngp.getNode (0).getLongitude ();
				z = ngp.getNode (0).getLatitude ();

				GameObject trunk = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
				trunk.name = "Trunk";
				trunk.tag = "Tree";
				trunk.transform.position = new Vector3 ((float)x, height / 2f, (float)z);
				trunk.transform.localScale = new Vector3 (0.01f, 0.03f, 0.01f);
				GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				foliage.name = "Foliage";
				foliage.tag = "Tree";
				foliage.transform.position = new Vector3 ((float)x, height, (float)z);
				foliage.transform.localScale = new Vector3 (diameter, diameter, diameter);

				MeshRenderer mesh_renderer1 = trunk.GetComponent<MeshRenderer> ();
				mesh_renderer1.material = Resources.Load ("Materials/troncArbre") as Material;
				MeshRenderer mesh_renderer2 = foliage.GetComponent<MeshRenderer> ();
				mesh_renderer2.material = Resources.Load ("Materials/feuillesArbre") as Material;
			}
		}	
	}

	//construction des arbres
	public void buildTrafficSignals(){
		double x, z;
		float height=0.03f,diameter=0.015f;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.isHighway() && ngp.isFeuTri() ) {
				for (int i = 0; i < ngp.nodes.Count; i++) {
					Debug.Log ("efjzfijeifjzeofij");
					x = ngp.getNode (i).getLongitude ();
					z = ngp.getNode (i).getLatitude ();

					GameObject trunk = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
					trunk.name = "TrafficSignalsTrunk";
					trunk.transform.position = new Vector3 ((float)x, height / 2f, (float)z);
					trunk.transform.localScale = new Vector3 (0.005f, 0.015f, 0.005f);
					GameObject TSLights = GameObject.CreatePrimitive (PrimitiveType.Cube);
					TSLights.name = "TrafficSignalsLights";
					TSLights.transform.position = new Vector3 ((float)x, height, (float)z);
					TSLights.transform.localScale = new Vector3 (diameter, diameter * 2f, diameter);

					MeshRenderer mesh_renderer1 = trunk.GetComponent<MeshRenderer> ();
					mesh_renderer1.material = Resources.Load ("Materials/metal") as Material;
					MeshRenderer mesh_renderer2 = TSLights.GetComponent<MeshRenderer> ();
					mesh_renderer2.material = Resources.Load ("Materials/feuxTricolores") as Material;
				}
			}
		}	
	}

	// place la caméra dans la scene
	public void buildMainCameraBG(){

		double CamLat, CamLon;

		// On centre la camera 
		CamLat = (minlat * 1000d + maxlat * 1000d) / 2;
		CamLon = (minlon * 1000d + maxlon * 1000d) / 2;
		GameObject mainCam = new GameObject ();
		mainCam.AddComponent <Camera> ();
		Light mainLight = mainCam.AddComponent<Light> ();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;
		mainCam.name = "MainCam";
		mainCam.transform.position = new Vector3 ((float)CamLon, 7, (float)CamLat);
		mainCam.transform.rotation = Quaternion.Euler (90, 0, 360);
		mainCam.AddComponent <CameraController> ();
	}

	public void buildBackground(){
		double angle;
		double lat = (minlat * 1000d + maxlat * 1000d) / 2;
		double lon = (minlon * 1000d + maxlon * 1000d) / 2;
		double width = maxlon*1000d-minlon*1000d;
//		double length = maxlat*1000d-minlat*1000d;
		double length =  Math.Sqrt(Math.Pow(maxlat*1000d - minlat*1000d,2)+Math.Pow(maxlon*1000d - minlon*1000d,2));

		Vector3 node1 = new Vector3((float)maxlon,0, (float)maxlat);
		Vector3 node2 = new Vector3((float)minlon,0, (float)minlat);
		Vector3 diff = node2-node1;

//		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		cube.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
//		cube.transform.position = new Vector3((float)minlon*1000f,0, (float)minlat*1000f);
//		GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		cube2.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
//		cube2.transform.position = new Vector3((float)maxlon*1000f,0, (float)maxlat*1000f);
//		GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		cube3.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
//		cube3.transform.position = new Vector3((float)lon,0, (float)lat);

		if (diff.z <= 0) {
			angle = (double)Vector3.Angle (Vector3.right, diff) + 180;
		} else {
			angle = (double)Vector3.Angle (Vector3.right, -diff) + 180;
		}
//		bgc.createBackground ((float)lon, (float)lat, (float)length, (float)width, (float)angle, (float) minlat, (float)minlon);
		bgc.createBackground ((float)lon, (float)lat, (float)length, (float)length, (float)angle, (float) minlat, (float)minlon);
	}
}
