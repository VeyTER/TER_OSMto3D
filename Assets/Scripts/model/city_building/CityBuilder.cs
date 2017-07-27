using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// 	Contient une suite d'outils permettant la construction des différents objets d'une ville.
/// </summary>
public class CityBuilder {
	public const int WALLS_INDEX = 0;
	public const int BUILDING_NODES_INDEX = 1;
	public const int ROOF_INDEX = 2;

	public const int ROAD_SECTIONS_INDEX = 0;
	public const int ROAD_NODES_INDEX = 1;

	private static CityBuilder instance;

	private BuildingsTools buildingsTools;
	private NodeGroupBase nodeGroupBase;

	/// <summary>Latitude minimale de la ville.</summary>
	private double minLat;

	/// <summary>Longitude minimale de la ville.</summary>
	private double minLon;

	/// <summary>Latitude maximale de la ville.</summary>
	private double maxLat;

	/// <summary>Longitude maximale de la ville.</summary>
	private double maxLon;

	private ExternalObjectBase externalObjectBase;
	private MapBackgroundBase mapBackgroundsBase;
	private SensorEquippedBuildingBase sensorsEquippedBuildingBase;

	private WallsBuilder wallsBuilder;

	/// <summary>Constructeur de routes.</summary>
	private WayBuilder wayBuilder;

	/// <summary>Constructeur de toits.</summary>
	private RoofBuilder roofBuilder;

	/// <summary>Constructeur de sols.</summary>
	private GroundBuilder groundBuilder;


	/// <summary>
	/// 	Object 3D représentant la ville, contient tous les objects de la ville sous forme de sous-groupes.
	/// </summary>
	private GameObject cityComponents;

	/// <summary>
	/// 	Object 3D contenant tous les groupes de murs, vus par l'application comme des bâtiments.
	/// </summary>
	private GameObject buildings;

	/// <summary>
	/// 	Object 3D contenant tous les groupes de portions de routes.
	/// </summary>
	private GameObject roads;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de pistes cyclables.
	/// summary>
	private GameObject cycleways;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de chemins piétons.
	/// summary>
	private GameObject footways;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de voies de bus.
	/// summary>
	private GameObject busLanes;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de cours d'eau.
	/// summary>
	private GameObject waterways;

	/// <summary>
	/// 	Object 3D contenant tous les groupes composants les arbres (tronc + feuille), formant chacun un arbre.
	/// summary>
	private GameObject trees;

	/// <summary>
	/// 	Object 3D contenant toutes les feux tricolores.
	/// summary>
	private GameObject trafficSignals;

	private CityBuilder() {
		this.buildingsTools = BuildingsTools.GetInstance();

		this.nodeGroupBase = NodeGroupBase.GetInstance();

		this.externalObjectBase = new ExternalObjectBase(FilePaths.EXTERNAL_OBJECTS_FILE);
		this.mapBackgroundsBase = new MapBackgroundBase(FilePaths.MAP_BACKGROUNDS_FILE);
		this.sensorsEquippedBuildingBase = new SensorEquippedBuildingBase(FilePaths.SENSOR_EQUIPPED_BUILDINGS_FILE);

		this.wallsBuilder = new WallsBuilder();
		this.wayBuilder = new WayBuilder();
		this.roofBuilder = new RoofBuilder();
		this.groundBuilder = new GroundBuilder();
	}

	public static CityBuilder GetInstance() {
		if (instance == null)
			instance = new CityBuilder();
		return instance;
	}


	/// <summary>
	/// 	Récupération les coordonnées minimales et maximales.
	/// </summary>
	/// <param name="minlat">Latitude minimale de la ville.</param>
	/// <param name="minlon">Longitude minimale de la ville.</param>
	/// <param name="maxlat">Latitude maximale de la ville.</param>
	/// <param name="maxlon">Longitude maximale de la ville.</param>
	public void SetBounds(double minlat, double minLon, double maxLat, double maxLon) {
		this.minLat = minlat;
		this.minLon = minLon;
		this.maxLat = maxLat;
		this.maxLon = maxLon;
	}


	/// <summary>
	/// 	Place les murs dans la scène.
	/// </summary>
	public void BuildBuildings() {
		// Récupération de l'objet contenant les groupes de murs (bâtiments) et ajout de celui-ci à la ville
		buildings = new GameObject(CityObjectNames.WALLS);
		buildings.transform.parent = cityComponents.transform;

		// Ajout d'un gestionnaire d'interface au groupe de bâtiments et affectaton du controlleur de modification,
		// contenu dans ce groupe, à ce gestionnaire
		buildings.AddComponent<EditController>();
		UiManager.editController = buildings.GetComponent<EditController>();

		// Construction et ajout des bâtiments
		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			if (nodeGroupEntry.Value.GetType() == typeof(BuildingNodeGroup)) {
				BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroupEntry.Value;
				this.BuildSingleBuilding(buildingNodeGroup);
			}
		}
	}

	public GameObject BuildSingleBuilding(BuildingNodeGroup buildingNodeGroup) {
		GameObject building = new GameObject(buildingNodeGroup.Name == "unknown" ? buildingNodeGroup.Id : buildingNodeGroup.Name);
		building.transform.SetParent(buildings.transform, false);
		buildingsTools.AddBuildingAndNodeGroupPair(building, buildingNodeGroup);

		Vector2 buildingCenter = buildingsTools.BuildingCenter(buildingNodeGroup);
		building.transform.position = new Vector3(buildingCenter.x, 0, buildingCenter.y);

		// Création et paramétrage de l'objet 3D destiné à former un bâtiment. Pour cela, chaque mur est
		// construit à partir du noeud courant et du noeud suivant dans le groupe de noeuds courant, puis, il
		// est ajouté au bâtiment
		wallsBuilder.BuildWalls(building, buildingNodeGroup);
		this.BuildSingleBuildingNodeGroup(building, buildingNodeGroup);

		switch (buildingNodeGroup.RoofShape) {
		case "flat":
			roofBuilder.BuildFlatRoof(building, buildingNodeGroup);
			break;
		case "hipped":
			roofBuilder.BuildHippedRoof(building, buildingNodeGroup, -0.035F);
			break;
		default:
			roofBuilder.BuildFlatRoof(building, buildingNodeGroup);
			break;
		}

		this.LoadMatchingBuilding(building, buildingNodeGroup);

		return building;
	}

	public void LoadMatchingBuilding(GameObject building, NodeGroup nodeGroup) {
		ExternalObject externalObject = this.IsExternalBuildingAtPosition(building.transform.position);
		if (externalObject != null) {
			if (externalObject.NeverUsed) {
				GameObject importedObject = (GameObject) GameObject.Instantiate(Resources.Load(FilePaths.EXTERNAL_OBJECTS_FOLDER_LOCAL + externalObject.ObjectFileName));
				importedObject.transform.position = externalObject.Position;
				importedObject.transform.rotation = Quaternion.Euler(importedObject.transform.rotation.x, (float) externalObject.Orientation, importedObject.transform.rotation.z);
				importedObject.transform.localScale = new Vector3((float) externalObject.Scale, (float) externalObject.Scale, (float) externalObject.Scale);
				importedObject.transform.parent = buildings.transform;

				externalObject.NeverUsed = false;
			}

			building.SetActive(false);
		}

		if (sensorsEquippedBuildingBase.SensorsEquippedBuildings.ContainsKey(building.name))
			building.AddComponent<BuildingComponentsController>();
	}

	private ExternalObject IsExternalBuildingAtPosition(Vector3 position) {
		const int PRECISION = 2;

		int i = 0;
		for (; i < externalObjectBase.ObjectsCount()
		&& !(Math.Round(externalObjectBase.GetObject(i).OsmPosition.x, PRECISION) == Math.Round(position.x, PRECISION)
		  && Math.Round(externalObjectBase.GetObject(i).OsmPosition.y, PRECISION) == Math.Round(position.y, PRECISION)
		  && Math.Round(externalObjectBase.GetObject(i).OsmPosition.z, PRECISION) == Math.Round(position.z, PRECISION)); i++) ;

		if (i < externalObjectBase.ObjectsCount())
			return externalObjectBase.GetObject(i);
		else
			return null;
	}

	public GameObject BuildVirtualLevel(GameObject building, int floorIndex, Material floorMaterial, bool buildRoof) {
		GameObject buildingWalls = building.transform.GetChild(WALLS_INDEX).gameObject;
		GameObject buildingRoof = building.transform.GetChild(ROOF_INDEX).gameObject;

		Vector3 buildingPosition = building.transform.position;

		GameObject virtualLevel = GameObject.Instantiate(buildingWalls);
		virtualLevel.name = building.name + "virtual_stage_" + floorIndex;
		virtualLevel.transform.localPosition = new Vector3(buildingPosition.x, (floorIndex - 1) * Dimensions.FLOOR_HEIGHT, buildingPosition.z);

		buildingsTools.ChangeWallsHeight(virtualLevel, 1);

		MeshRenderer virtualLevelRenderer = virtualLevel.GetComponent<MeshRenderer>();
		virtualLevelRenderer.material = floorMaterial;

		MeshCollider virtualWallCollider = virtualLevel.GetComponent<MeshCollider>();
		MeshFilter virtualFloorMeshFilter = virtualLevel.GetComponent<MeshFilter>();

		virtualWallCollider.sharedMesh = virtualFloorMeshFilter.sharedMesh;

		if (buildRoof) {
			BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(building);
			GameObject virtualRoof = roofBuilder.BuildFlatRoof(building, buildingNodeGroup);

			virtualRoof.transform.SetParent(virtualLevel.transform, false);
			virtualRoof.transform.localPosition = new Vector3(0, Dimensions.FLOOR_HEIGHT, 0);

			MeshRenderer virtualRoofRenderer = virtualRoof.GetComponent<MeshRenderer>();
			virtualRoofRenderer.material = floorMaterial;
		}

		return virtualLevel;
	}

	private GameObject BuildSingleBuildingNodeGroup(GameObject building, NodeGroup nodeGroup) {
		// Création et paramétrage de l'objet 3D destiné à former un groupe de noeuds de bâtiment
		GameObject buildingNodeGroupGo = new GameObject(building.name + "_nodes");
		buildingNodeGroupGo.transform.SetParent(building.transform, false);

		// Construction des angles de noeuds de bâtiments
		foreach (Node node in nodeGroup.Nodes) {
			// Création et paramétrage de l'objet 3D destiné à former un noeud de bâtiment
			GameObject buildingNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buildingNode.name = node.Reference;
			buildingNode.tag = GoTags.BUILDING_NODE_TAG;
			buildingNode.transform.position = new Vector3((float) node.Longitude, 0, (float) node.Latitude);
			buildingNode.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

			// Ajout du noeud au groupe de noeuds et ajout d'une entrée dans la table de correspondances
			buildingNode.transform.parent = buildingNodeGroupGo.transform;
			buildingsTools.AddBuildingNodeAndNodeEntryPair(buildingNode, node);
		}

		return buildingNodeGroupGo;
	}

	/// <summary>
	/// 	Place les routes dans la scène.
	/// </summary>
	public void BuildWays() {
		// Récupération de l'objet contenant les routes classiques et ajout de celui-ci à la ville
		roads = new GameObject(CityObjectNames.ROADS);
		roads.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les pistes cyclables et ajout de celui-ci à la ville
		cycleways = new GameObject(CityObjectNames.CYCLEWAYS);
		cycleways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins piétons et ajout de celui-ci à la ville
		footways = new GameObject(CityObjectNames.FOOTWAYS);
		footways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins piétons et ajout de celui-ci à la ville
		busLanes = new GameObject(CityObjectNames.BUS_LANES);
		busLanes.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins piétons et ajout de celui-ci à la ville
		waterways = new GameObject(CityObjectNames.WATERWAYS);
		waterways.transform.parent = cityComponents.transform;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			NodeGroup nodeGroup = nodeGroupEntry.Value;
			if (nodeGroup.GetType() == typeof(HighwayNodeGroup) || nodeGroup.GetType() == typeof(WaterwayNodeGroup)) {
				if (nodeGroup.GetType() == typeof(HighwayNodeGroup)) {
					HighwayNodeGroup highwayNodeGroup = (HighwayNodeGroup) nodeGroup;
					if (highwayNodeGroup.IsRoad()) {
						GameObject road = wayBuilder.BuildRoad(highwayNodeGroup);
						road.transform.parent = roads.transform;

						GameObject newWayNodeGroup = this.BuildSingleWayNodeGroup(road, nodeGroup);
						newWayNodeGroup.transform.SetParent(road.transform, false);
					} else if (highwayNodeGroup.IsCycleWay()) {
						GameObject cycleway = wayBuilder.BuildCycleway(highwayNodeGroup);
						cycleway.transform.parent = cycleways.transform;
					} else if (highwayNodeGroup.IsFootway()) {
						GameObject footway = wayBuilder.BuildFootway(highwayNodeGroup);
						footway.transform.parent = footways.transform;
					} else if (highwayNodeGroup.IsBusWayLane()) {
						GameObject busLane = wayBuilder.BuildBusLane(highwayNodeGroup);
						busLane.transform.parent = busLanes.transform;
					}
				} else if (nodeGroup.GetType() == typeof(WaterwayNodeGroup)) {
					WaterwayNodeGroup waterwayNodeGroup = (WaterwayNodeGroup) nodeGroup;
					GameObject waterway = wayBuilder.BuildWaterway(waterwayNodeGroup);
					waterway.transform.parent = waterways.transform;
				}
			}
		}
	}

	public GameObject BuildSingleWayNodeGroup(GameObject way, NodeGroup nodeGroup) {
		GameObject roadNodeGroupGo = new GameObject("WaysNodes_" + way.name.Split('_')[1]);
		roadNodeGroupGo.transform.SetParent(way.transform, false);

		// Construction des angles de noeuds de routes
		foreach (Node node in nodeGroup.Nodes) {
			// Création et paramétrage de l'objet 3D destiné à former un noeud de route
			GameObject roadNodeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
			roadNodeGo.name = "Node_" + Node.GenerateId(node);
			roadNodeGo.tag = GoTags.ROAD_NODE_TAG;

			roadNodeGo.transform.position = new Vector3((float) node.Longitude, 0, (float) node.Latitude);
			roadNodeGo.transform.localScale = new Vector3(0.02F, 0.02F, 0.02F);

			// Ajout du noeud dans le groupe de noeuds 
			roadNodeGo.transform.parent = roadNodeGroupGo.transform;
		}

		return roadNodeGroupGo;
	}

	/// <summary>
	/// 	Place les arbres dans la scène.
	/// </summary>
	public void BuildTrees() {
		// Récupération de l'objet contenant les arbres et ajout de celui-ci à la ville
		trees = new GameObject(CityObjectNames.TREES);
		trees.transform.parent = cityComponents.transform;

		float heightJitter = Dimensions.TRUNC_HEIGHT * 0.4F;
		float diameterJitter = Dimensions.TRUNC_DIAMTETER * 0.2F;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			NodeGroup nodeGroup = nodeGroupEntry.Value;

			float height = Dimensions.TRUNC_HEIGHT + UnityEngine.Random.Range(-heightJitter / 2F, heightJitter / 2F);
			float diameter = Dimensions.TRUNC_DIAMTETER + UnityEngine.Random.Range(-diameterJitter / 2F, diameterJitter / 2F);

			if (nodeGroup.GetType() == typeof(NaturalNodeGroup)) {
				// Récupération de la position de l'arbre depuis le groupe de noeuds correspondant
				double posX = nodeGroup.GetNode(0).Longitude;
				double posZ = nodeGroup.GetNode(0).Latitude;

				// Création et paramétrage de l'objet 3D (cylindre) destiné à former un tronc d'arbre 
				GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				trunk.name = "Trunk_" + nodeGroup.Id;
				trunk.tag = GoTags.TREE_TAG;
				trunk.transform.position = new Vector3((float) posX, height / 2F, (float) posZ);
				trunk.transform.localScale = new Vector3(height / 6F, height / 2F, height / 6F);

				// Création et paramétrage de l'objet 3D (sphere) destiné à former un feuillage 
				GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				foliage.name = "Foliage" + nodeGroup.Id;
				foliage.tag = GoTags.TREE_TAG;
				foliage.transform.position = new Vector3((float) posX, height, (float) posZ);
				foliage.transform.localScale = new Vector3(diameter, diameter, diameter);

				// Affectation du matériau au tronc pour lui donner la texture voulue
				MeshRenderer trunkMeshRenderer = trunk.GetComponent<MeshRenderer>();
				trunkMeshRenderer.material = Resources.Load(Materials.TREE_TRUNK) as Material;

				// Affectation du matériau au feuillage pour lui donner la texture voulue
				MeshRenderer foliageMeshRenderer = foliage.GetComponent<MeshRenderer>();
				foliageMeshRenderer.material = Resources.Load(Materials.TREE_LEAF) as Material;

				GameObject tree = new GameObject("Tree_" + nodeGroup.Id);
				nodeGroup.Name = tree.name;

				// Ajout du tronc et du feuillage à l'arbre
				trunk.transform.parent = tree.transform;
				foliage.transform.parent = tree.transform;

				// Ajout de l'arbre au groupe d'arbres
				tree.transform.parent = trees.transform;
			}
		}
	}


	/// <summary>
	/// 	Place les feux tricolores dans la scène.
	/// </summary>
	public void BuildTrafficSignals() {
		float posX = 0;
		float posZ = 0;

		// Récupération de l'objet contenant les feux de signalisation et ajout de celui-ci à la ville
		trafficSignals = new GameObject(CityObjectNames.TRAFFIC_SIGNALS);
		trafficSignals.transform.parent = cityComponents.transform;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			if (nodeGroupEntry.Value.GetType() == typeof(HighwayNodeGroup)) {
				HighwayNodeGroup highwayNodeGroup = (HighwayNodeGroup) nodeGroupEntry.Value;

				for (int i = 0; i < highwayNodeGroup.NodeCount(); i++) {
					if (highwayNodeGroup.GetNode(i).GetType() == typeof(HighwayComponentNode)) {
						HighwayComponentNode highwayComponentNode = (HighwayComponentNode) highwayNodeGroup.GetNode(i);

						if (highwayComponentNode.IsTrafficSignal()) {
							posZ = (float)highwayComponentNode.Latitude;
							posX = (float)highwayComponentNode.Longitude;

							GameObject trafficSignal = GameObject.Instantiate(Resources.Load<GameObject>(GameObjects.TRAFFIC_SIGNAL));
							trafficSignal.transform.SetParent(trafficSignals.transform, false);
							trafficSignal.transform.position = new Vector3(posX, 0, posZ);
							trafficSignal.transform.localScale = new Vector3(0.001F * Dimensions.SCALE_FACTOR, 0.001F * Dimensions.SCALE_FACTOR, 0.001F * Dimensions.SCALE_FACTOR);
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// 	Place la caméra dans la scène.
	/// </summary>
	public void BuildMainCamera() {
		// On centre la camera 
		double camLat = (minLat * Dimensions.SCALE_FACTOR + maxLat * Dimensions.SCALE_FACTOR) / 2F;
		double camLon = (minLon * Dimensions.SCALE_FACTOR + maxLon * Dimensions.SCALE_FACTOR) / 2F;

		// Création de l'objet 3D représenant la caméra
		GameObject mainCamera = Camera.main.gameObject;
	}


	/// <summary>
	/// 	Place le sol dans la scène.
	/// </summary>
	public void BuildGround(string backgroundName = null) {
		Material groundMaterial = null;

		if (backgroundName != null) {
			Texture backgroundTexture = Resources.Load<Texture>(FilePaths.TEXTURES_FOLDER_LOCAL + backgroundName);

			groundMaterial = GameObject.Instantiate(new Material(Shader.Find("Standard"))) as Material;
			groundMaterial.mainTexture = backgroundTexture;

			MapBackground mapBackground = mapBackgroundsBase.GetMapBackground(backgroundName);

			minLat = mapBackground.MinLat;
			minLon = mapBackground.MinLon;

			maxLat = mapBackground.MaxLat;
			maxLon = mapBackground.MaxLon;
		} else {
			groundMaterial = Resources.Load(Materials.GROUND) as Material;
			groundMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
		}

		double lat = (minLat * Dimensions.SCALE_FACTOR + maxLat * Dimensions.SCALE_FACTOR) / 2F;
		double lon = (minLon * Dimensions.SCALE_FACTOR + maxLon * Dimensions.SCALE_FACTOR) / 2F;

		double length = maxLon * Dimensions.SCALE_FACTOR - minLon * Dimensions.SCALE_FACTOR;
		double width = maxLat * Dimensions.SCALE_FACTOR - minLat * Dimensions.SCALE_FACTOR;

		Vector2 textureExpansion = Vector2.one;
		if (backgroundName == null)
			textureExpansion = new Vector2((float) length * 40F, (float) width * 40F);

		// Calcul des coordonnées du milieu du vecteur formé par les 2 noeuds des extrémités
		Vector3 node1 = new Vector3((float) maxLon, 0, (float) maxLat);
		Vector3 node2 = new Vector3((float) minLon, 0, (float) minLat);

		Vector3 diff = node2 - node1;

		// Construction de l'objet 3D (cube) destiné à former un sol 
		groundBuilder.BuildGround((float) length, (float) width, (float) minLat, (float) minLon, groundMaterial, textureExpansion);
	}

	public NodeGroupBase NodeGroupBase {
		get { return nodeGroupBase; }
	}

	public GameObject CityComponents {
		set { cityComponents = value; }
	}

	public GameObject Buildings {
		get { return buildings; }
	}

	public GameObject Roads {
		get { return roads; }
	}

	public GameObject Cycleways {
		get { return cycleways; }
	}

	public GameObject Footways {
		get { return footways; }
	}

	public GameObject BusLanes {
		get { return busLanes; }
	}

	public GameObject Waterways {
		get { return waterways; }
	}

	public GameObject Trees {
		get { return trees; }
	}

	public GameObject TrafficSignals {
		get { return trafficSignals; }
	}

	public WallsBuilder WallsBuilder {
		get { return wallsBuilder; }
	}

	public WayBuilder HighwayBuilder {
		get { return wayBuilder; }
	}

	public RoofBuilder RoofBuilder {
		get { return roofBuilder; }
	}

	public GroundBuilder GroundBuilder {
		get { return groundBuilder; }
	}

	public ExternalObjectBase ExternalObjectBase {
		get { return externalObjectBase; }
	}

	public MapBackgroundBase MapBackgroundsBase {
		get { return mapBackgroundsBase; }
	}

	public SensorEquippedBuildingBase SensorsEquippedBuildingBase {
		get { return sensorsEquippedBuildingBase; }
	}
}