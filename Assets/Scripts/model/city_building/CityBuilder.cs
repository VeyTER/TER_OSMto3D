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
	private HighwayBuilder highwayBuilder;

	/// <summary>Constructeur de toits.</summary>
	private RoofBuilder roofBuilder;

	/// <summary>Constructeur de sols.</summary>
	private GroundBuilder groundBuilder;


	/// <summary>
	/// 	Object 3D représentant la ville, contient tous les objects de la ville sous forme de sous-groupes.
	/// </summary>
	private GameObject cityComponents;

	/// <summary>
	/// 	Object 3D contenant tous les noeuds de routes.
	/// summary>
	private GameObject highwayNodes;

	/// <summary>
	/// 	Object 3D contenant tous les groupes de murs, vus par l'application comme des bâtiments.
	/// </summary>
	private GameObject buildings;

	/// <summary>
	/// 	Object 3D contenant tous les groupes de portions de routes.
	/// </summary>
	private GameObject highways;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de pistes cyclables.
	/// summary>
	private GameObject cycleways;

	/// <summary>
	/// 	Object 3D contenant toutes les portions de chemins piétons.
	/// summary>
	private GameObject footways;

	/// <summary>
	/// 	Object 3D contenant tous les groupes composants les arbres (tronc + feuille), formant chacun un arbre.
	/// summary>
	private GameObject trees;

	private CityBuilder() {
		this.buildingsTools = BuildingsTools.GetInstance();

		this.nodeGroupBase = NodeGroupBase.GetInstance();

		this.externalObjectBase = new ExternalObjectBase(FilePaths.EXTERNAL_OBJECTS_FILE);
		this.mapBackgroundsBase = new MapBackgroundBase(FilePaths.MAP_BACKGROUNDS_FILE);
		this.sensorsEquippedBuildingBase = new SensorEquippedBuildingBase(FilePaths.SENSOR_EQUIPPED_BUILDINGS_FILE);

		this.wallsBuilder = new WallsBuilder();
		this.highwayBuilder = new HighwayBuilder();
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
	/// 	Place les noeuds 3D dans la scène.
	/// </summary>
	public void BuildNodes() {
		// Récupération de l'objet contenant les groupes de noeuds de routes et ajout de celui-ci à la ville
		highwayNodes = new GameObject(CityObjectNames.HIGHWAY_NODES);
		highwayNodes.transform.parent = cityComponents.transform;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			NodeGroup nodeGroup = nodeGroupEntry.Value;

			if (nodeGroup.GetType() == typeof(BuildingNodeGroup))
				this.BuildSingleHighwayNodeGroup(nodeGroup);
		}
	}

	public void BuildSingleHighwayNodeGroup(NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(HighwayNodeGroup) || nodeGroup.GetType() == typeof(WaterwayNodeGroup)) {
			// Construction des angles de noeuds de routes
			foreach (Node n in nodeGroup.Nodes) {
				// Création et paramétrage de l'objet 3D destiné à former un noeud de route
				GameObject highwayNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
				highwayNode.name = n.Reference;
				highwayNode.tag = GoTags.HIGHWAY_NODE_TAG;
				highwayNode.transform.position = new Vector3((float) n.Longitude, 0, (float) n.Latitude);
				highwayNode.transform.localScale = new Vector3(0.02F, 0.02F, 0.02F);

				// Ajout du noeud dans le groupe de noeuds 
				highwayNode.transform.parent = highwayNodes.transform;
			}
		}
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
				RoofBuilder.BuildRoof(building, buildingNodeGroup, 0);

				this.LoadMatchingBuilding(building, buildingNodeGroup);
			}
		}
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

	public GameObject BuildVirtualFloor(GameObject building, int floorIndex, Material floorMaterial, bool buildRoof) {
		GameObject buildingWalls = building.transform.GetChild(WALLS_INDEX).gameObject;
		GameObject buildingRoof = building.transform.GetChild(ROOF_INDEX).gameObject;

		Vector3 buildingPosition = building.transform.position;

		GameObject virtualFloor = GameObject.Instantiate(buildingWalls);
		virtualFloor.name = building.name + "virtual_stage_" + floorIndex;
		virtualFloor.transform.localPosition = new Vector3(buildingPosition.x, (floorIndex - 1) * Dimensions.FLOOR_HEIGHT, buildingPosition.z);

		buildingsTools.ChangeWallsHeight(virtualFloor, 1);

		MeshRenderer virtualFloorRenderer = virtualFloor.GetComponent<MeshRenderer>();
		virtualFloorRenderer.material = floorMaterial;

		MeshCollider virtualWallCollider = virtualFloor.GetComponent<MeshCollider>();
		MeshFilter virtualFloorMeshFilter = virtualFloor.GetComponent<MeshFilter>();

		virtualWallCollider.sharedMesh = virtualFloorMeshFilter.sharedMesh;

		if (buildRoof) {
			BuildingNodeGroup buildingNodeGroup = buildingsTools.BuildingToNodeGroup(building);
			GameObject virtualRoof = roofBuilder.BuildRoof(building, buildingNodeGroup, 1.1F);

			virtualRoof.transform.SetParent(virtualFloor.transform, false);
			virtualRoof.transform.localPosition = new Vector3(0, Dimensions.FLOOR_HEIGHT, 0);

			MeshRenderer virtualRoofRenderer = virtualRoof.GetComponent<MeshRenderer>();
			virtualRoofRenderer.material = floorMaterial;
		}

		return virtualFloor;
	}

	private GameObject BuildSingleBuildingNodeGroup(GameObject building, NodeGroup nodeGroup) {
		// Création et paramétrage de l'objet 3D destiné à former un groupe de noeuds de bâtiment
		GameObject buildingNodeGroupGo = new GameObject() {
			name = building.name + "_nodes"
		};
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

	private void BuildRoof(GameObject building, NodeGroup nodeGroup) {

	}

	/// <summary>
	/// 	Place les routes dans la scène.
	/// </summary>
	public void BuildRoads() {
		// Récupération de l'objet contenant les routes classiques et ajout de celui-ci à la ville
		highways = new GameObject(CityObjectNames.HIGHWAYS);
		highways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les pistes cyclables et ajout de celui-ci à la ville
		cycleways = new GameObject(CityObjectNames.CYCLEWAYS);
		cycleways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins piétons et ajout de celui-ci à la ville
		footways = new GameObject(CityObjectNames.FOOTWAYS);
		footways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins voies de bus et ajout de celui-ci à la ville
		//		busways = new GameObject(ObjectNames.BUSWAYS);
		//		busways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins voies maritimes et ajout de celui-ci à la ville
		//		waterways = new GameObject(ObjectNames.WATERWAYS);
		//		waterways.transform.parent = cityComponents.transform;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			NodeGroup nodeGroup = nodeGroupEntry.Value;

			if (nodeGroup.GetType() == typeof(HighwayNodeGroup) || nodeGroup.GetType() == typeof(WaterwayNodeGroup)) {
				for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
					Node currentNode = nodeGroup.GetNode(i);
					Node nextNode = nodeGroup.GetNode(i + 1);

					// Calcul des coordonnées du milieu du vectuer formé par les 2 noeuds consécutifs
					Vector3 node1 = new Vector3((float) currentNode.Longitude, 0, (float) currentNode.Latitude);
					Vector3 node2 = new Vector3((float) nextNode.Longitude, 0, (float) nextNode.Latitude);
					Vector3 delta = node2 - node1;

					double posX = 0;
					double posY = 0;

					double length = 0.06;
					double width = Dimensions.ROAD_WIDTH;

					double angle = 0;

					// Calcul de la position de la route, dépendant du signe du delta entre les deux noeuds
					if (delta.z <= 0) {
						posX = nodeGroup.GetNode(i + 1).Longitude;
						posY = nodeGroup.GetNode(i + 1).Latitude;
						length = Math.Sqrt(Math.Pow(nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow(nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double) Vector3.Angle(Vector3.right, delta) + 180;
					} else {
						posX = nodeGroup.GetNode(i).Longitude;
						posY = nodeGroup.GetNode(i).Latitude;
						length = Math.Sqrt(Math.Pow(nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow(nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double) Vector3.Angle(Vector3.right, -delta) + 180;
					}

					if (nodeGroup.GetType() == typeof(HighwayNodeGroup)) {
						HighwayNodeGroup highwayNodeGroup = (HighwayNodeGroup) nodeGroup;

						if (highwayNodeGroup.IsWay()) {
							// Construction et paramétrage de l'objet 3D destiné à former une route classique
							GameObject newClassicHighway = highwayBuilder.BuildClassicHighway((float) posX, (float) posY, (float) length, (float) width, (float) angle);
							newClassicHighway.name = currentNode.Reference + " to " + nextNode.Reference;

							// Ajout de la route au groupe de routes
							newClassicHighway.transform.parent = highways.transform;
						} else if (highwayNodeGroup.IsCycleWay()) {
							// Construction et paramétrage de l'objet 3D destiné à former une piste cyclable
							GameObject newCycleway = highwayBuilder.BuildCycleway((float) posX, (float) posY, (float) length, (float) width / 2F, (float) angle);
							newCycleway.name = currentNode.Reference + " to " + nextNode.Reference;

							// Ajout de la piste cyclable au groupe de pistes cyclables
							newCycleway.transform.parent = cycleways.transform;
						} else if (highwayNodeGroup.IsFootway()) {
							// Construction et paramétrage de l'objet 3D destiné à former un chemin piéton
							GameObject newFootway = highwayBuilder.BuildFootway((float) posX, (float) posY, (float) length, (float) width / 1.5F, (float) angle);
							newFootway.name = currentNode.Reference + " to " + nextNode.Reference;

							// Ajout du chemin piéton au groupe de chemins piétons
							newFootway.transform.parent = footways.transform;
							// 	 } else if (ngp.isBusWayLane ()) {
							// Construction et paramétrage de l'objet 3D destiné à former une voie de bus
							//  newHighWay = roadBuilder.createBusLane ((float)x, (float)y, (float)length, (float)width, (float)angle);

							// Ajout de la voie maritime au groupe de voies de bus
							// newBusways.transform.parent = busways.transform;
						}
					} else if (nodeGroup.GetType() == typeof(WaterwayNodeGroup)) {
						// Construction et paramétrage de l'objet 3D destiné à former une voie de bus
						GameObject newWaterway = highwayBuilder.BuildWaterway((float) posX, (float) posY, (float) length, (float) width / 1.5F, (float) angle);
						newWaterway.name = currentNode.Reference + " to " + nextNode.Reference;

						// Ajout de la voie maritime au groupe de voies maritimes
						// newWaterway.transform.parent = waterways.transform;
					}
				}
			}
		}
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
				trunk.name = "Trunk";
				trunk.tag = GoTags.TREE_TAG;
				trunk.transform.position = new Vector3((float) posX, height / 2F, (float) posZ);
				trunk.transform.localScale = new Vector3(height / 6F, height / 2F, height / 6F);

				// Création et paramétrage de l'objet 3D (sphere) destiné à former un feuillage 
				GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				foliage.name = "Foliage";
				foliage.tag = GoTags.TREE_TAG;
				foliage.transform.position = new Vector3((float) posX, height, (float) posZ);
				foliage.transform.localScale = new Vector3(diameter, diameter, diameter);

				// Affectation du matériau au tronc pour lui donner la texture voulue
				MeshRenderer trunkMeshRenderer = trunk.GetComponent<MeshRenderer>();
				trunkMeshRenderer.material = Resources.Load(Materials.TREE_TRUNK) as Material;

				// Affectation du matériau au feuillage pour lui donner la texture voulue
				MeshRenderer foliageMeshRenderer = foliage.GetComponent<MeshRenderer>();
				foliageMeshRenderer.material = Resources.Load(Materials.TREE_LEAF) as Material;

				GameObject tree = new GameObject(nodeGroup.Id);

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
	public void BuildTraffiSignals() {
		double posX, posZ;
		float height = 0.03F;
		float diameter = 0.015F;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
			if (nodeGroupEntry.Value.GetType() == typeof(HighwayNodeGroup)) {
				HighwayNodeGroup highwayNodeGroup = (HighwayNodeGroup) nodeGroupEntry.Value;

				for (int i = 0; i < highwayNodeGroup.NodeCount(); i++) {
					if (highwayNodeGroup.GetNode(i).GetType() == typeof(HighwayComponentNode)) {
						HighwayComponentNode highwayComponentNode = (HighwayComponentNode) highwayNodeGroup.GetNode(i);

						if (highwayComponentNode.IsTrafficSignal()) {

							posZ = highwayComponentNode.Latitude;
							posX = highwayComponentNode.Longitude;

							// Création et paramétrage de l'objet 3D (cylindre) destiné à former un support du feu tricolore 
							GameObject mount = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
							mount.name = "Support de feu tricolore";
							mount.transform.position = new Vector3((float) posX, height / 2f, (float) posZ);
							mount.transform.localScale = new Vector3(0.005F, 0.015F, 0.005F);

							// Création et paramétrage de l'objet 3D (cube) destiné à former un lumières du feu tricolore 
							GameObject lights = GameObject.CreatePrimitive(PrimitiveType.Cube);
							lights.name = "Lumière de feu tricolore";
							lights.transform.position = new Vector3((float) posX, height, (float) posZ);
							lights.transform.localScale = new Vector3(diameter, diameter * 2F, diameter);

							// Affectation du matériau au support pour lui donner la texture voulue
							MeshRenderer mountMeshRenderer = mount.GetComponent<MeshRenderer>();
							mountMeshRenderer.material = Resources.Load(Materials.METAL) as Material;

							// Affectation du matériau aux lumières pour lui donner la texture voulue
							MeshRenderer lightsMeshRenderer = lights.GetComponent<MeshRenderer>();
							lightsMeshRenderer.material = Resources.Load(Materials.TRAFFIC_LIGHT) as Material;

							// Création de l'objet 3D destiné à former un feu tricolore
							GameObject trafficLight = new GameObject(highwayNodeGroup.Id);

							// Ajout du support et des feux au feu tricolore
							mount.transform.parent = trafficLight.transform;
							lights.transform.parent = trafficLight.transform;
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
		GameObject mainCameraGo = Camera.main.gameObject;
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
			textureExpansion = new Vector2((float) length * 4F, (float) width * 4F);

		// Calcul des coordonnées du milieu du vecteur formé par les 2 noeuds des extrémités
		Vector3 node1 = new Vector3((float) maxLon, 0, (float) maxLat);
		Vector3 node2 = new Vector3((float) minLon, 0, (float) minLat);

		Vector3 diff = node2 - node1;

		// Construction de l'objet 3D (cube) destiné à former un sol 
		groundBuilder.BuildGround((float) length, (float) width, (float) minLat, (float) minLon, groundMaterial, textureExpansion);
	}

	public void HideWalls() {
		this.ChangeBuildingComponentVisibility(false, WALLS_INDEX);
	}
	public void ShowWalls() {
		this.ChangeBuildingComponentVisibility(true, WALLS_INDEX);
	}

	public void HideBuildingNodes() {
		this.ChangeBuildingComponentVisibility(false, BUILDING_NODES_INDEX);
	}
	public void ShowBuildingNodes() {
		this.ChangeBuildingComponentVisibility(true, BUILDING_NODES_INDEX);
	}

	public void HideRoofs() {
		this.ChangeBuildingComponentVisibility(false, ROOF_INDEX);
	}
	public void ShowRoofs() {
		this.ChangeBuildingComponentVisibility(true, ROOF_INDEX);
	}

	private void ChangeBuildingComponentVisibility(bool visibility, int componentIndex) {
		foreach (Transform buildingTransform in buildings.transform)
			buildingTransform.GetChild(componentIndex).gameObject.SetActive(visibility);
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

	public WallsBuilder WallsBuilder {
		get { return wallsBuilder; }
	}

	public HighwayBuilder HighwayBuilder {
		get { return highwayBuilder; }
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