using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*Classe contentant toutes les fonctions de construction de GameObject*/
public class ObjectBuilder {
	private ArrayList nodeGroups;

	private Hashtable buildingIdTable;

	private Hashtable buildingNodeGroupsIdTable;
	private Hashtable buildingNodesIdTable;

	private Hashtable nodeIdTable;

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
		this.nodeGroups = new ArrayList ();

		this.buildingIdTable = new Hashtable ();

		this.buildingNodeGroupsIdTable = new Hashtable ();
		this.buildingNodesIdTable = new Hashtable ();

		this.nodeIdTable = new Hashtable ();

		this.roadBuilder = new RoadBuilder ();
		this.roofBuilder = new RoofBuilder ();
		this.groundBuilder = new GroundBuilder ();

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
					n.Latitude = n.Latitude * Main.SCALE_FACTOR;
					n.Longitude = n.Longitude * Main.SCALE_FACTOR;
					groupsMemo.Add(n);
				}
			}
		}
	}

	// copie des coordonnées en latitude et longitude
	public void SetBounds(double minlat, double minlon, double maxlat, double maxlon) {
		this.minlat = minlat;
		this.minlon = minlon;
		this.maxlat = maxlat;
		this.maxlon = maxlon;
	}

	// place les nodes dans la scène
	public void BuildNodes() {
		buildingNodes = new GameObject(ObjectNames.BUILDING_NODES);
		buildingNodes.transform.parent = cityComponents.transform;

		highwayNodes = new GameObject(ObjectNames.HIGHWAY_NODES);
		highwayNodes.transform.parent = cityComponents.transform;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

		int i;
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.IsBuilding()) {
				GameObject buildingNodeGroup = new GameObject ();;

				// on construit les angles des buildings
				foreach(Node n in ngp.Nodes) {
					GameObject buildingNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
					buildingNode.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
					buildingNode.transform.position = new Vector3((float)n.Longitude, 0, (float)n.Latitude);
					buildingNode.name = n.Reference.ToString();
					buildingNode.tag = NodeTags.BUILDING_NODE_TAG;
					buildingNode.transform.parent = buildingNodeGroup.transform;
					buildingNodesIdTable [n.Reference + "|" + n.Index] = buildingNode.transform.GetInstanceID();
				}

				buildingNodeGroup.transform.parent = buildingNodes.transform;
				buildingNodeGroup.name = ngp.Id.ToString();
				buildingNodeGroupsIdTable [ngp.Id] = buildingNodeGroup.transform.GetInstanceID();

				Vector3 nodeGroupCenter = buildingsTools.BuildingNodesCenter (buildingNodeGroup, ngp);
				buildingNodeGroup.transform.position = nodeGroupCenter;
				foreach (Transform wallTransform in buildingNodeGroup.transform)
					wallTransform.transform.position -= buildingNodeGroup.transform.position;
			}

			if ( (ngp.IsHighway () && ((ngp.IsPrimary() || ngp.IsSecondary() || ngp.IsTertiary() || ngp.IsUnclassified()
			   || ngp.IsResidential ()|| ngp.IsService()) || ngp.IsCycleWay() || ngp.IsFootway())) || ngp.IsWaterway()) {
//				GameObject highwayNodeGroup = new GameObject();

				// on construit les nodes des highways
				i =  - 1;
				foreach (Node n in ngp.Nodes) {
					i++;
					GameObject highwayNode = GameObject.CreatePrimitive (PrimitiveType.Cube);
					highwayNode.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
					highwayNode.transform.position = new Vector3 ((float)n.Longitude, 0, (float)n.Latitude);
					highwayNode.name = n.Reference.ToString();
					highwayNode.tag = NodeTags.HIGHWAY_NODE_TAG;
					highwayNode.transform.parent = highwayNodes.transform;
				}
			}
		}
	}

	// place les murs dans la scène
	public void BuildWalls() {
		wallGroups = new GameObject(ObjectNames.WALLS);
		wallGroups.transform.parent = cityComponents.transform;
		wallGroups.AddComponent<EditionController> ();
		UiManager.editionController = wallGroups.GetComponent<EditionController>();

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();

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
					Node currentNode = ngp.GetNode (i);
					Node nextNode = ngp.GetNode (i + 1);

					// on recup les coordonées utiles
					x = (currentNode.Longitude + nextNode.Longitude) / 2;
					y = (currentNode.Latitude + nextNode.Latitude) / 2;

					length = Math.Sqrt(Math.Pow(nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow(nextNode.Longitude - currentNode.Longitude, 2));
					adj = Math.Abs(nextNode.Latitude - currentNode.Latitude);
					angle = (Math.Acos(adj / length) * 180 / Math.PI);

					// on positionne le mur
					GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
					wall.tag = NodeTags.WALL_TAG;
					etages = ngp.NbFloor;

					wall.transform.localScale = new Vector3((float) length + thickness * 1.5F, floorHeight * etages, thickness);
					wall.transform.position = new Vector3((float) x, (floorHeight / 2F) * (float) etages, (float) y);

					BoxCollider wallBoxColliser = wall.GetComponent<BoxCollider> ();
					wallBoxColliser.isTrigger = true;

					wall.AddComponent<UiManager>();

					wall.transform.parent = wallGroup.transform;

					// on modifie l'angle en fonction de l'ordre des points
					if((currentNode.Latitude > nextNode.Latitude && currentNode.Longitude < nextNode.Longitude) 
					|| (currentNode.Latitude < nextNode.Latitude && currentNode.Longitude > nextNode.Longitude)) {
						wall.transform.localEulerAngles = new Vector3(0, 90 - (float) angle, 0);
					} else {
						wall.transform.localEulerAngles = new Vector3(0, (float) angle + 90, 0);
					}

					// Si on ne connait pas le nom du batiment on utilise la référence
					if(ngp.Name == "unknown")
						wall.name = currentNode.Reference  + "_wall_" + i;
					else
						wall.name = ngp.Name + "_wall_" + i;

					MeshRenderer meshRenderer = wall.GetComponent<MeshRenderer>();
					meshRenderer.material = Resources.Load (Materials.WALL) as Material;
				}
				buildingIdTable [ngp.Id] = wallGroup.transform.GetInstanceID();

				Vector3 wallGroupCenter = buildingsTools.BuildingCenter(wallGroup);
				wallGroup.transform.position = wallGroupCenter;
				foreach (Transform wallTransform in wallGroup.transform)
					wallTransform.transform.position -= wallGroup.transform.position;

				if(ngp.Name == "unknown")
					wallGroup.name = "Bâtiment n°" + ngp.Id.ToString();
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
		roofs = new GameObject(ObjectNames.ROOFS);
		roofs.transform.parent = cityComponents.transform;

		foreach(NodeGroup ngp in nodeGroups) {
			if (ngp.IsBuilding()) {
				triangulation = new DelauneyTriangulation(ngp);
				triangulation.CreateBoundingBox();
				triangulation.Start();

				GameObject newRoof = roofBuilder.BuildRoof(triangulation, ngp.NbFloor, floorHeight);
				newRoof.transform.parent = roofs.transform;
				newRoof.name = ngp.Id.ToString();
			}
		}
	}

	// construction des routes
	public void BuildRoads() {
		highways = new GameObject(ObjectNames.HIGHWAYS);
		highways.transform.parent = cityComponents.transform;

		cycleways = new GameObject(ObjectNames.CYCLEWAYS);
		cycleways.transform.parent = cityComponents.transform;

		footways = new GameObject(ObjectNames.FOOTWAYS);
		footways.transform.parent = cityComponents.transform;

		double x, y;
		double length, width = 0.06;
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
						newHighway.name = currentNode.Reference + " to " + nextNode.Reference;
					} else if (ngp.IsFootway ()) {
						GameObject newFootway = roadBuilder.BuildFootway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newFootway.transform.parent = footways.transform;
						newFootway.name = currentNode.Reference + " to " + nextNode.Reference;
					} else if (ngp.IsCycleWay ()) {
						GameObject newCycleway = roadBuilder.BuildCycleway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newCycleway.transform.parent = cycleways.transform;
						newCycleway.name = currentNode.Reference + " to " + nextNode.Reference;
// 					} else if (ngp.isBusWayLane ()) {
// 						newHighWay = roadBuilder.createBusLane ((float)x, (float)y, (float)length, (float)width, (float)angle);
					} else if (ngp.IsWaterway ()) {
						GameObject newWaterway = roadBuilder.BuildWaterway ((float)x, (float)y, (float)length, (float)width, (float)angle);
						newWaterway.name = currentNode.Reference + " to " + nextNode.Reference;
					}
				}
			}	
		}
	}

	// construction des arbres
	public void BuildTrees() {
		trees = new GameObject(ObjectNames.TREES);
		trees.transform.parent = cityComponents.transform;

		double x, z;
		float height = 0.12f;
		float diameter = 0.08f;

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
				mesh_renderer1.material = Resources.Load (Materials.TREE_TRUNK) as Material;

				MeshRenderer mesh_renderer2 = foliage.GetComponent<MeshRenderer> ();
				mesh_renderer2.material = Resources.Load (Materials.TREE_LEAF) as Material;

				GameObject tree = new GameObject (ngp.Id.ToString());
				trunk.transform.parent = tree.transform;
				foliage.transform.parent = tree.transform;

				tree.transform.parent = trees.transform;
			}
		}	
	}

	// construction des arbres
	public void BuildTrafficLights() {
		double x, z;
		float height = 0.03f;
		float diameter = 0.015f;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.IsHighway() && ngp.IsTrafficLight() ) {
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
					meshRenderer1.material = Resources.Load (Materials.METAL) as Material;

					MeshRenderer meshRenderer2 = lights.GetComponent<MeshRenderer> ();
					meshRenderer2.material = Resources.Load (Materials.TRAFFIC_LIGHT) as Material;

					GameObject trafficLight = new GameObject (ngp.Id.ToString());
					mount.transform.parent = trafficLight.transform;
					lights.transform.parent = trafficLight.transform;
				}
			}
		}	
	}

	// place la caméra dans la scene
	public void BuildMainCamera() {
		// On centre la camera 
		double camLat = (minlat * Main.SCALE_FACTOR + maxlat * Main.SCALE_FACTOR) / 2;
		double camLon = (minlon * Main.SCALE_FACTOR + maxlon * Main.SCALE_FACTOR) / 2;

		GameObject mainCameraGo = Camera.main.gameObject;

		Light mainLight = mainCameraGo.AddComponent<Light> ();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;

		mainCameraGo.AddComponent <CameraController> ();
	}

	// construction du background
	public void BuildGround() {
		double angle, lat, lon, width, length;
		Vector3 node1, node2, diff;

		lat = (minlat * Main.SCALE_FACTOR + maxlat * Main.SCALE_FACTOR) / 2;
		lon = (minlon * Main.SCALE_FACTOR + maxlon * Main.SCALE_FACTOR) / 2;
		width = maxlon * Main.SCALE_FACTOR - minlon * Main.SCALE_FACTOR;
		length =  Math.Sqrt(Math.Pow(maxlat * Main.SCALE_FACTOR - minlat * Main.SCALE_FACTOR, 2) + Math.Pow(maxlon * Main.SCALE_FACTOR - minlon * Main.SCALE_FACTOR, 2));

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

	public Hashtable BuildingIdTable {
		get { return buildingIdTable; }
	}

	public Hashtable BuildingNodeGroupsIdTable {
		get { return buildingNodeGroupsIdTable; }
	}

	public Hashtable BuildingNodesIdTable {
		get { return buildingNodesIdTable; }
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
