using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*Classe contentant toutes les fonctions de construction de GameObject*/
public class ObjectBuilder {
	private ArrayList nodeGroups;

	private double minlat;
	private double maxlat;
	private double minlon;
	private double maxlon;

	private RoadBuilder roadBuilder;
	private RoofBuilder roofBuilder;
	private GroundBuilder groundBuilder;
	private DelauneyTriangulation triangulation;

	private GameObject cityComponents;
	private GameObject wallGroups;
	private GameObject roofs;
	private GameObject highways;
	private GameObject cycleways;
	private GameObject footways;
	private GameObject trees;
	private GameObject highwayNodes;
	private GameObject buildingNodes;

	private float floorHeight;

	// constructeur
	private ObjectBuilder() {
		this.nodeGroups = new ArrayList();

		this.roadBuilder = new RoadBuilder();
		this.roofBuilder = new RoofBuilder();
		this.groundBuilder = new GroundBuilder();

		this.floorHeight = 0.08F;
	}

	public static ObjectBuilder GetInstance() {
		return ObjectBuilderHolder.instance;
	}

    // copie d'une liste de groupe de nodes
	public void ScaleNodes(double scaleValue) {
		ArrayList groupsMemo = new ArrayList();
		foreach (NodeGroup ngp in nodeGroups) {
			foreach (Node n in ngp.Nodes) {
				if (!groupsMemo.Contains(n)) {
					n.Latitude = n.Latitude * 1000d;
					n.Longitude = n.Longitude * 1000d;
					groupsMemo.Add(n);
				}
			}
		}
	}

	// copie des coordonnées en latitude et longitude
	public void SetLatLon(double minla, double maxla, double minlo, double maxlo) {
		this.minlat = minla;
		this.maxlat = maxla;
		this.minlon = minlo;
		this.maxlon = maxlo;
	}

	public void InitializeCityGameObject() {
		cityComponents = new GameObject ("City");
	}

	// place les nodes dans la scène
	public void BuildNodes() {
		highwayNodes = new GameObject("Highway nodes");
		highwayNodes.transform.parent = cityComponents.transform;

		buildingNodes = new GameObject("buildings nodes");
		buildingNodes.transform.parent = cityComponents.transform;

		int i;
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.IsBuilding()) {
				// on construit les angles des buildings
				foreach(Node n in ngp.Nodes) {
					GameObject buildingNodeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
					buildingNodeGo.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
					buildingNodeGo.transform.position = new Vector3((float)n.Longitude, 0, (float)n.Latitude);
					buildingNodeGo.name = "" + n.Id;
					buildingNodeGo.tag = NodeTags.BUILDING_NODE_TAG;
					buildingNodeGo.transform.parent = buildingNodes.transform;
				}
			}
			if ( (ngp.IsHighway () && ((ngp.IsPrimary() || ngp.IsSecondary() || ngp.IsTertiary() || ngp.IsUnclassified() || ngp.IsResidential ()|| ngp.IsService()) || ngp.IsCycleWay() || ngp.IsFootway())) || ngp.IsWaterway()) {
				// on construit les nodes des highways
				i =  - 1;
				foreach (Node n in ngp.Nodes) {
					i++;
					GameObject highwayNodeGo = GameObject.CreatePrimitive (PrimitiveType.Cube);
					highwayNodeGo.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
					highwayNodeGo.transform.position = new Vector3 ((float)n.Longitude, 0, (float)n.Latitude);
					highwayNodeGo.name = "" + n.Id;
					highwayNodeGo.tag = NodeTags.HIGHWAY_NODE_TAG;
					highwayNodeGo.transform.parent = highwayNodes.transform;
				}
			}
		}
	}

	// place les murs dans la scène
	public void BuildWalls() {
		wallGroups = new GameObject("Walls groups (buildings)");
		wallGroups.transform.parent = cityComponents.transform;

		float thickness = 0.01f;

		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.IsBuilding()) {

				if(!ngp.DecomposeRight())
					ngp.DecomposeLeft();

				// On créé les murs
				double x, y, length, adj, angle;
				int etages = 1;

				GameObject wallGroup = new GameObject ();

				// Construction du bâtiment courant
				for(int i = 0; i < ngp.NodeCount() - 1; i++) {
					Node curentNode = ngp.GetNode (i);
					Node nextNode = ngp.GetNode (i + 1);

					// on recup les coordonées utiles
					x = (curentNode.Longitude + nextNode.Longitude) / 2;
					y = (curentNode.Latitude + nextNode.Latitude) / 2;

					length = Math.Sqrt(Math.Pow(nextNode.Latitude - curentNode.Latitude, 2) + Math.Pow(nextNode.Longitude - curentNode.Longitude, 2));
					adj = Math.Abs(nextNode.Latitude - curentNode.Latitude);
					angle = (Math.Acos(adj / length) * 180d / Math.PI);

					// on positionne le mur
					GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
					wall.tag = NodeTags.WALL_TAG;
					etages = ngp.NbFloor;

					wall.transform.localScale = new Vector3((float) length + 0.015f, floorHeight * etages, thickness);
					wall.transform.position = new Vector3((float) x, (floorHeight / 2F) * (float) etages, (float) y);

					BoxCollider wallBoxColliser = wall.GetComponent<BoxCollider> ();
					wallBoxColliser.isTrigger = true;

					wall.AddComponent<BuildingEditor>();

					wall.transform.parent = wallGroup.transform;

					// on modifie l'angle en fonction de l'ordre des points
					if((curentNode.Latitude > nextNode.Latitude && curentNode.Longitude < nextNode.Longitude) 
					|| (curentNode.Latitude < nextNode.Latitude && curentNode.Longitude > nextNode.Longitude)) {
						wall.transform.localEulerAngles = new Vector3(0, 90 - (float) angle, 0);
					} else {
						wall.transform.localEulerAngles = new Vector3(0, (float) angle + 90, 0);
					}

					// Si on ne connait pas le nom du batiment on utilise l'id
					if(ngp.Name == "unknown")
						wall.name = ngp.Id  + "_Mur" + i;
					else
						wall.name = ngp.Name + "_Mur" + i;

					MeshRenderer meshRenderer = wall.GetComponent<MeshRenderer>();
					meshRenderer.material = Resources.Load ("Materials/mur") as Material;
				}

				if(ngp.Name == "unknown")
					wallGroup.name = ngp.Id + "";
				else
					wallGroup.name = ngp.Name;

				wallGroup.transform.parent = wallGroups.transform;
			}
		}
	}

	public void EditUniqueBuilding(GameObject wallSetGo, int nbFloor) {
		foreach(Transform wall in wallSetGo.transform) {
			wall.localScale = new Vector3(wall.localScale.x, floorHeight * nbFloor, wall.localScale.z);
			wall.position = new Vector3 (wall.position.x, (floorHeight / 2F) * (float)nbFloor, wall.position.z);
		}
	}

	// construction des toits
	public void BuildRoofs() {
		roofs = new GameObject("Roofs");
		roofs.transform.parent = cityComponents.transform;

		foreach(NodeGroup ngp in nodeGroups) {
			if (ngp.IsBuilding()) {
				triangulation = new DelauneyTriangulation(ngp);
				triangulation.CreateBoundingBox();
				triangulation.Start();

				GameObject newRoof = roofBuilder.BuildRoof(triangulation, ngp.NbFloor, floorHeight);
				newRoof.transform.parent = roofs.transform;
				newRoof.name = ngp.Id + "";
			}
		}
	}

	// construction des routes
	public void BuildRoads() {
		highways = new GameObject("Highways");
		highways.transform.parent = cityComponents.transform;

		cycleways = new GameObject("Cycleways");
		cycleways.transform.parent = cityComponents.transform;

		footways = new GameObject("Footways");
		footways.transform.parent = cityComponents.transform;

		double x, y;
		double length, width = 0.06d;
		double angle;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( (ngp.IsHighway () && (ngp.IsResidential () || ngp.IsPrimary() || ngp.IsSecondary() || ngp.IsTertiary() 
				|| ngp.IsService() || ngp.IsUnclassified() || ngp.IsCycleWay() || ngp.IsFootway())) 
				/*|| ngp.isBusWayLane()*/ || ngp.IsWaterway() ) {

				for (int i = 0; i < ngp.NodeCount () - 1; i++) {
					Node currentNode = ngp.GetNode (i);
					Node nextNode = ngp.GetNode (i + 1);

					// Calcul des coordonées du milieu du vecteur formé par les 2 nodes consécutives
					Vector3 node1 = new Vector3 ((float)currentNode.Longitude, 0, (float)currentNode.Latitude);
					Vector3 node2 = new Vector3 ((float)nextNode.Longitude, 0, (float)nextNode.Latitude);
					Vector3 diff = node2 - node1;

					if (diff.z <= 0) {
						x = ngp.GetNode (i + 1).Longitude;
						y = ngp.GetNode (i + 1).Latitude;
						length = Math.Sqrt (Math.Pow (nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow (nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double)Vector3.Angle (Vector3.right, diff) + 180;
					} else {
						x = ngp.GetNode (i).Longitude;
						y = ngp.GetNode (i).Latitude;
						length = Math.Sqrt (Math.Pow (nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow (nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double)Vector3.Angle (Vector3.right, -diff) + 180;
					}

					if (ngp.IsHighway () && (ngp.IsResidential () || ngp.IsPrimary () || ngp.IsSecondary () || ngp.IsTertiary () || ngp.IsService () || ngp.IsUnclassified ())) {
						GameObject newHighway = roadBuilder.BuildClassicHighway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newHighway.transform.parent = highways.transform;
						newHighway.name = currentNode.Id + " à " + nextNode.Id;
					} else if (ngp.IsFootway ()) {
						GameObject newFootway = roadBuilder.BuildFootway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newFootway.transform.parent = footways.transform;
						newFootway.name = currentNode.Id + " à " + nextNode.Id;
					} else if (ngp.IsCycleWay ()) {
						GameObject newCycleway = roadBuilder.BuildCycleway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newCycleway.transform.parent = cycleways.transform;
						newCycleway.name = currentNode.Id + " à " + nextNode.Id;
// 					} else if (ngp.isBusWayLane ()) {
// 						newHighWay = roadBuilder.createBusLane ((float)x, (float)y, (float)length, (float)width, (float)angle);
					} else if (ngp.IsWaterway ()) {
						GameObject newWaterway = roadBuilder.BuildWaterway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newWaterway.name = currentNode.Id + " à " + nextNode.Id;
					}
				}
			}	
		}
	}

	// construction des arbres
	public void BuildTrees() {
		trees = new GameObject("Trees");
		trees.transform.parent = cityComponents.transform;

		double x, z;
		float height=0.12f, diameter=0.08f;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.IsTree() ) {
				x = ngp.GetNode (0).Longitude;
				z = ngp.GetNode (0).Latitude;

				// Creation du tronc d'arbre (cylindre)
				GameObject trunk = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
				trunk.name = "Trunk";
				trunk.tag = NodeTags.TREE_TAG;
				trunk.transform.position = new Vector3 ((float)x, height / 2f, (float)z);
				trunk.transform.localScale = new Vector3 (height / 6f, height / 2f, height / 6f);

				// Creation du feuillage (sphere)
				GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				foliage.name = "Foliage";
				foliage.tag = NodeTags.TREE_TAG;
				foliage.transform.position = new Vector3 ((float)x, height, (float)z);
				foliage.transform.localScale = new Vector3 (diameter, diameter, diameter);

				MeshRenderer mesh_renderer1 = trunk.GetComponent<MeshRenderer> ();
				mesh_renderer1.material = Resources.Load ("Materials/troncArbre") as Material;
				MeshRenderer mesh_renderer2 = foliage.GetComponent<MeshRenderer> ();
				mesh_renderer2.material = Resources.Load ("Materials/feuillesArbre") as Material;

				GameObject tree = new GameObject (ngp.Id + "");
				trunk.transform.parent = tree.transform;
				foliage.transform.parent = tree.transform;

				tree.transform.parent = trees.transform;
			}
		}	
	}

	// construction des arbres
	public void BuildTrafficSignals() {
		double x, z;
		float height=0.03f, diameter=0.015f;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.IsHighway() && ngp.IsFeuTri() ) {
				for (int i = 0; i < ngp.NodeCount(); i++) {
					z = ngp.GetNode (i).Latitude;
					x = ngp.GetNode (i).Longitude;

					// Création de la barre de métal du feu (cylindre)
					GameObject mount = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
					mount.name = "Support de feu tricolore";
					mount.transform.position = new Vector3 ((float)x, height / 2f, (float)z);
					mount.transform.localScale = new Vector3 (0.005f, 0.015f, 0.005f);

					// Création du cube des feux (cube)
					GameObject lights = GameObject.CreatePrimitive (PrimitiveType.Cube);
					lights.name = "Lumière de feu tricolore";
					lights.transform.position = new Vector3 ((float)x, height, (float)z);
					lights.transform.localScale = new Vector3 (diameter, diameter * 2f, diameter);

					MeshRenderer meshRenderer1 = mount.GetComponent<MeshRenderer> ();
					meshRenderer1.material = Resources.Load ("Materials/metal") as Material;

					MeshRenderer meshRenderer2 = lights.GetComponent<MeshRenderer> ();
					meshRenderer2.material = Resources.Load ("Materials/feuxTricolores") as Material;

					GameObject trafficLight = new GameObject (ngp.Id + "");
					mount.transform.parent = trafficLight.transform;
					lights.transform.parent = trafficLight.transform;
				}
			}
		}	
	}

	// place la caméra dans la scene
	public void BuildMainCameraBG() {
		double CamLat, CamLon;

		GameObject mainCamera = Camera.main.gameObject;

		// On centre la camera 
		CamLat = (minlat * 1000d + maxlat * 1000d) / 2;
		CamLon = (minlon * 1000d + maxlon * 1000d) / 2;

		Light mainLight = mainCamera.AddComponent<Light> ();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;
		mainCamera.AddComponent <CameraController> ();
	}

	// construction du background
	public void BuildGround() {
		double angle, lat, lon, width, length;
		Vector3 node1, node2, diff;

		lat = (minlat * 1000d + maxlat * 1000d) / 2;
		lon = (minlon * 1000d + maxlon * 1000d) / 2;
		width = maxlon*1000d - minlon * 1000d;
		length =  Math.Sqrt(Math.Pow(maxlat * 1000d - minlat * 1000d, 2) + Math.Pow(maxlon * 1000d - minlon * 1000d, 2));

		node1 = new Vector3((float)maxlon, 0, (float)maxlat);
		node2 = new Vector3((float)minlon, 0, (float)minlat);
		diff = node2 - node1;

		if (diff.z <= 0)
			angle = (double)Vector3.Angle (Vector3.right, diff) + 180;
		else
			angle = (double)Vector3.Angle (Vector3.right,  - diff) + 180;

		groundBuilder.BuildGround ((float)lon, (float)lat, (float)length, (float)length, (float)angle, (float) minlat, (float)minlon);
	}

	public ArrayList NodeGroups {
		get { return nodeGroups; }
	}

	public GameObject CityComponents {
		set { cityComponents = value; }
	}

	public GameObject WallGroups {
		get { return wallGroups; }
	}

	public GameObject Roofs {
		get { return roofs; }
	}

	public GameObject Highways {
		get { return highways; }
	}

	public GameObject Trees {
		get { return trees; }
	}

	public GameObject Cycleways {
		get { return cycleways; }
	}

	public GameObject Footways {
		get { return footways; }
	}

	public GameObject HighwayNodes {
		get { return highwayNodes; }
	}

	public GameObject BuildingNodes {
		get { return buildingNodes; }
	}

	private class ObjectBuilderHolder {
		public static ObjectBuilder instance = new ObjectBuilder();
	}
}
