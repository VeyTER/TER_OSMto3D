using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// 	Contient une suite d'outils permettant la construction des différents objets d'une ville.
/// </summary>
public class CityBuilder {
	/// <summary>
	/// 	Groupes de noeuds contenant toutes les informations sur les objets de la scène 3D.
	/// </summary>
	private List<NodeGroup> nodeGroups;


	/// <summary>Latitude minimale de la ville.</summary>
	private double minLat;

	/// <summary>Longitude minimale de la ville.</summary>
	private double minLon;

	/// <summary>Latitude maximale de la ville.</summary>
	private double maxLat;

	/// <summary>Longitude maximale de la ville.</summary>
	private double maxLon;


	/// <summary>Constructeur de routes.</summary>
	private HighwayBuilder roadBuilder;

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
	private GameObject wallGroups;

	/// <summary>
	/// 	Object 3D contenant tous les groupes de murs, avec un groupe de toit par bâtiment.
	/// </summary>
	private GameObject roofs;

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

	/// <summary>
	/// 	Object 3D contenant tous les groupes de noeuds de murs, vus par l'application comme des noeuds 3D
	/// 	des bâtiments.
	/// summary>
	private GameObject buildingNodes;

	/// <summary>
	/// 	Object 3D contenant tous les noeuds de routes.
	/// summary>
	private GameObject highwayNodes;

	private List<ExternalObject> externalObjects;

	private Dictionary<string, MapBackground> mapBackgrounds;

	private CityBuilder() {
		this.nodeGroups = new List<NodeGroup> ();

		this.roadBuilder = new HighwayBuilder ();
		this.roofBuilder = new RoofBuilder ();
		this.groundBuilder = new GroundBuilder ();

		this.externalObjects = this.LoadExternalObject();
		this.mapBackgrounds = this.LoadMapBackgrounds();
	}

	private List<ExternalObject> LoadExternalObject() {
		List<ExternalObject> externalObjects = new List<ExternalObject>();

		if (File.Exists(FilePaths.EXTERNAL_OBJECTS_FILE)) {
			String objectsFileContent = File.ReadAllText(FilePaths.EXTERNAL_OBJECTS_FILE);
			String[] lines = objectsFileContent.Split('\n');

			foreach (String line in lines) {
				String[] objectData = line.Split('\t');
				ExternalObject materialData = new ExternalObject(objectData);
				externalObjects.Add(materialData);
			}
		}

		return externalObjects;
	}

	private Dictionary<string, MapBackground> LoadMapBackgrounds() {
		Dictionary<string, MapBackground> mapBackgrounds = new Dictionary<string, MapBackground>();

		if (File.Exists(FilePaths.MAP_BACKGROUNDS_FILE)) {
			String backgroundsFileContent = File.ReadAllText(FilePaths.MAP_BACKGROUNDS_FILE);
			String[] lines = backgroundsFileContent.Split('\n');

			foreach (String line in lines) {
				String[] backgroundData = line.Split('\t');
				MapBackground mapBackground = new MapBackground(backgroundData);
				mapBackgrounds[backgroundData[0]] = mapBackground;
			}
		}

		return mapBackgrounds;
	}

	public static CityBuilder GetInstance() {
		return CityBuilderHolder.instance;
	}


	/// <summary>
	/// 	Copie et change l'échelle des groupes de noeuds.
	/// </summary>
	/// <param name="scaleFactor">Facteur d'échelle.</param>
	public void ScaleNodes(double scaleFactor) {
		List<Node> groupsMemo = new List<Node>();
		foreach (NodeGroup nodeGroup in nodeGroups) {
			foreach (Node node in nodeGroup.Nodes) {
				if (!groupsMemo.Contains(node)) {
					node.Latitude = node.Latitude * scaleFactor;
					node.Longitude = node.Longitude * scaleFactor;
					groupsMemo.Add(node);
				}
			}
		}
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
		// Récupération de l'objet contenant les groupes de noeuds de bâtiments et ajout de celui-ci à la ville
		buildingNodes = new GameObject(ObjectNames.BUILDING_NODES);
		buildingNodes.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les groupes de noeuds de routes et ajout de celui-ci à la ville
		highwayNodes = new GameObject(ObjectNames.HIGHWAY_NODES);
		highwayNodes.transform.parent = cityComponents.transform;

		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.IsBuilding())
				this.BuildSingleBuildingNodeGroup(ngp);

			if (ngp.IsHighway ())
				this.BuildSingleHighwayNodeGroup(ngp);
		}
	}

	public GameObject BuildSingleBuildingNodeGroup(NodeGroup ngp) {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance();

		// Création et paramétrage de l'objet 3D destiné à former un groupe de noeuds de bâtiment
		GameObject buildingNodeGroup = new GameObject() {
			name = ngp.Id
		};

		// Ajout du groupe de noeuds à l'objet contenant les groupes de noeuds de bâtiments
		// et ajout d'une entrée dans la table de correspondances
		buildingNodeGroup.transform.parent = buildingNodes.transform;
		buildingsTools.AddBuildingNodeGroupToNodeGroupEntry(buildingNodeGroup, ngp);
		buildingsTools.AddNodeGroupToBuildingNodeGroupEntry(ngp, buildingNodeGroup);

		// Construction des angles de noeuds de bâtiments
		foreach (Node n in ngp.Nodes) {
			// Création et paramétrage de l'objet 3D destiné à former un noeud de bâtiment
			GameObject buildingNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buildingNode.name = n.Reference;
			buildingNode.tag = NodeTags.BUILDING_NODE_TAG;
			buildingNode.transform.position = new Vector3((float) n.Longitude, 0, (float) n.Latitude);
			buildingNode.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

			// Ajout du noeud au groupe de noeuds et ajout d'une entrée dans la table de correspondances
			buildingNode.transform.parent = buildingNodeGroup.transform;
			buildingsTools.AddBuildingNodeToNodeEntry(buildingNode, n);
		}

		// Déplacement des noeuds 3D au sein du groupe pour qu'ils aient une position relative au centre
		Vector3 nodeGroupCenter = buildingsTools.BuildingNodesCenter(buildingNodeGroup, ngp);
		buildingNodeGroup.transform.position = nodeGroupCenter;
		foreach (Transform wallTransform in buildingNodeGroup.transform)
			wallTransform.transform.position -= buildingNodeGroup.transform.position;

		return buildingNodeGroup;
	}

	public void BuildSingleHighwayNodeGroup(NodeGroup ngp) {
		if ((ngp.IsPrimary() || ngp.IsSecondary() || ngp.IsTertiary() || ngp.IsUnclassified() || ngp.IsResidential()
				|| ngp.IsService()) || ngp.IsCycleWay() || ngp.IsFootway() || ngp.IsWaterway()) {
			// Construction des angles de noeuds de routes
			foreach (Node n in ngp.Nodes) {
				// Création et paramétrage de l'objet 3D destiné à former un noeud de route
				GameObject highwayNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
				highwayNode.name = n.Reference;
				highwayNode.tag = NodeTags.HIGHWAY_NODE_TAG;
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
	public void BuildWalls() {
		// Récupération de l'objet contenant les groupes de murs (bâtiments) et ajout de celui-ci à la ville
		wallGroups = new GameObject(ObjectNames.WALLS);
		wallGroups.transform.parent = cityComponents.transform;

		// Ajout d'un gestionnaire d'interface au groupe de bâtiments et affectaton du controlleur de modification,
		// contenu dans ce groupe, à ce gestionnaire
		wallGroups.AddComponent<EditController> ();
		UiManager.editController = wallGroups.GetComponent<EditController>();

		// Construction et ajout des bâtiments
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.IsBuilding()) {
				// Décomposition du bâtiment en triangles
				if(!ngp.RightDecomposition())
					ngp.LeftDecomposition();

				// Création et paramétrage de l'objet 3D destiné à former un bâtiment. Pour cela, chaque mur est
				// construit à partir du noeud courant et du noeud suivant dans le groupe de noeuds courant, puis, il
				// est ajouté au bâtiment
				GameObject wallGroup = this.BuildSingleWallGroup(ngp);
				this.SetupSingleWallGroup(wallGroup, ngp);
			}
		}
	}
	
	public GameObject BuildSingleWallGroup(NodeGroup ngp) {
		GameObject wallGroup = new GameObject();
		for (int i = 0; i < ngp.NodeCount() - 1; i++) {
			Node currentNode = ngp.GetNode(i);
			Node nextNode = ngp.GetNode(i + 1);

			// Récupération des coordonnées utiles
			double posX = (currentNode.Longitude + nextNode.Longitude) / 2;
			double posY = (currentNode.Latitude + nextNode.Latitude) / 2;

			// Calcul de l'orientation du mur courant
			double length = Math.Sqrt(Math.Pow(nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow(nextNode.Longitude - currentNode.Longitude, 2));
			double deltaLat = Math.Abs(nextNode.Latitude - currentNode.Latitude);
			double angle = (Math.Acos(deltaLat / length) * 180 / Math.PI);

			// Création et paramétrage de l'objet 3D destiné à former un mur
			GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
			wall.tag = NodeTags.WALL_TAG;

			// Paramétrage du mur 3D
			int nbFloor = ngp.NbFloor;
			wall.transform.localScale = new Vector3((float) length + Dimensions.WALL_THICKNESS * 1.5F, Dimensions.FLOOR_HEIGHT * nbFloor, Dimensions.WALL_THICKNESS);
			wall.transform.position = new Vector3((float) posX, (Dimensions.FLOOR_HEIGHT / 2F) * (float) nbFloor, (float) posY);

			// Récupération et configuration de la boite de collision du mur
			BoxCollider wallBoxColliser = wall.GetComponent<BoxCollider>();
			wallBoxColliser.isTrigger = true;

			// Ajout d'une instance du gestionnaire d'interface pour que cette dernière soit déclenchée lors
			// d'un clic
			wall.AddComponent<UiManager>();

			// Ajout du mur au bâtiment
			wall.transform.SetParent(wallGroup.transform);

			// Modification de l'angle en fonction de l'ordre des points
			if ((currentNode.Latitude > nextNode.Latitude && currentNode.Longitude < nextNode.Longitude)
			|| (currentNode.Latitude < nextNode.Latitude && currentNode.Longitude > nextNode.Longitude)) {
				wall.transform.localEulerAngles = new Vector3(0, 90 - (float) angle, 0);
			} else {
				wall.transform.localEulerAngles = new Vector3(0, (float) angle + 90, 0);
			}

			// Nommage du mur à partir du nom du bâtiment s'il existe, sinon, utilisation de la référence du mur
			if (ngp.Name == "unknown")
				wall.name = currentNode.Reference + "_wall_" + i;
			else
				wall.name = ngp.Name + "_wall_" + i;

			// Affectation du matériau et de sa couleur au mur pour lui donner la texture voulue
			MeshRenderer meshRenderer = wall.GetComponent<MeshRenderer>();
			meshRenderer.material = ngp.CustomMaterial;
			meshRenderer.materials[0].color = ngp.OverlayColor;
		}
		return wallGroup;
	}

	public void SetupSingleWallGroup(GameObject wallGroup, NodeGroup ngp) {
		BuildingsTools buildingsTools = BuildingsTools.GetInstance();

		// Ajout d'une entrée dans la table de correspondances
		buildingsTools.AddBuildingToNodeGroupEntry(wallGroup, ngp);
		buildingsTools.AddNodeGroupToBuildingEntry(ngp, wallGroup);

		// Déplacement des murs au sein du bâtiment pour qu'ils aient une position relative au centre
		Vector3 wallGroupCenter = buildingsTools.BuildingCenter(wallGroup);
		wallGroup.transform.position = wallGroupCenter;
		foreach (Transform wallTransform in wallGroup.transform)
			wallTransform.transform.position -= wallGroup.transform.position;

		// Nommage du bâtiment avec son nom dans les fichiers de données s'il existe, sinon, utilisation de
		// son ID
		if (ngp.Name == "unknown")
			wallGroup.name = "Bâtiment n°" + ngp.Id;
		else
			wallGroup.name = ngp.Name;

		// Ajout du bâtiment au groupe de bâtiments
		wallGroup.transform.parent = wallGroups.transform;

		ExternalObject externalObject = this.ExternalBuildingAtPosition(wallGroup.transform.position, buildingsTools.BuildingRadius(wallGroup));
		if (externalObject != null) {
			if (externalObject.NeverUsed) {
				GameObject importedObject = (GameObject) GameObject.Instantiate(Resources.Load(FilePaths.EXTERNAL_OBJECTS_FOLDER_LOCAL + externalObject.ObjectFileName));
				importedObject.transform.position = externalObject.Position;
				importedObject.transform.localScale = new Vector3((float) externalObject.Scale, (float) externalObject.Scale, (float) externalObject.Scale);
				importedObject.transform.rotation = Quaternion.Euler(importedObject.transform.rotation.x, (float) externalObject.Orientation, importedObject.transform.rotation.z);
				importedObject.transform.parent = wallGroups.transform;

				externalObject.NeverUsed = false;
			}

			wallGroup.SetActive(false);
		}
	}

	private ExternalObject ExternalBuildingAtPosition(Vector3 position, double radius) {
		const int PRECISION = 2;

		int i = 0;
		for (; i < externalObjects.Count
		&& !(Math.Round(externalObjects[i].OsmPosition.x, PRECISION) == Math.Round(position.x, PRECISION)
		  && Math.Round(externalObjects[i].OsmPosition.y, PRECISION) == Math.Round(position.y, PRECISION)
		  && Math.Round(externalObjects[i].OsmPosition.z, PRECISION) == Math.Round(position.z, PRECISION)); i++) ;

		if (i < externalObjects.Count)
			return externalObjects[i];
		else
			return null;
	}


	/// <summary>
	/// 	Modifie la hauteur et l'orientation d'un bâtiment. Pour cela, la méthode modifie la hauteur et de la
	/// 	position de tous les murs du bâtiment.
	/// </summary>
	/// <param name="wallSetGo">Bâtiment à modifier.</param>
	/// <param name="nbFloor">Nombre d'étages qui doivent former le bâtiment.</param>
	public void RebuildBuilding(GameObject building, int nbFloor) {
		foreach(Transform wallTransform in building.transform) {
			wallTransform.position = new Vector3 (wallTransform.position.x, (Dimensions.FLOOR_HEIGHT / 2F) * (float)nbFloor, wallTransform.position.z);
			wallTransform.localScale = new Vector3(wallTransform.localScale.x, Dimensions.FLOOR_HEIGHT * nbFloor, wallTransform.localScale.z);
		}
	}

	public GameObject BuildVirtualFloor(GameObject building, int floorIndex, Material floorMaterial) {
		List<GameObject> virtualWalls = new List<GameObject>();
		foreach (Transform childTransform in building.transform) {
			GameObject virtualWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
			virtualWall.tag = NodeTags.WALL_TAG;

			virtualWall.transform.position = new Vector3(childTransform.position.x, (Dimensions.FLOOR_HEIGHT) * floorIndex - Dimensions.FLOOR_HEIGHT / 2F, childTransform.position.z);
			virtualWall.transform.rotation = childTransform.rotation;
			virtualWall.transform.localScale = new Vector3(childTransform.localScale.x, Dimensions.FLOOR_HEIGHT, childTransform.localScale.z * 1.5F);

			BoxCollider wallBoxColliser = virtualWall.GetComponent<BoxCollider>();
			wallBoxColliser.isTrigger = true;

			virtualWall.AddComponent<UiManager>();

			MeshRenderer virtualWallRenderer = virtualWall.GetComponent<MeshRenderer>();
			virtualWallRenderer.material = floorMaterial;

			virtualWalls.Add(virtualWall);
		}

		GameObject virtualFloor = new GameObject(building.name + "virtual_stage_" + floorIndex);
		virtualFloor.transform.position = building.transform.position;
		virtualFloor.transform.rotation = building.transform.rotation;

		foreach (GameObject virtualWall in virtualWalls)
			virtualWall.transform.parent = virtualFloor.transform;

		Vector3 buildingScale = building.transform.localScale;
		virtualFloor.transform.localScale = new Vector3(buildingScale.x, virtualFloor.transform.localScale.y, buildingScale.z);

		return virtualFloor;
	}


	/// <summary>
	/// 	Place les toits dans la scène.
	/// </summary>
	public void BuildRoofs() {
		// Récupération de l'objet contenant les toits et ajout de celui-ci à la ville
		roofs = new GameObject(ObjectNames.ROOFS);
		roofs.transform.parent = cityComponents.transform;

		foreach(NodeGroup ngp in nodeGroups) {
			if (ngp.IsBuilding()) {
				// Initialisation d'une triangulation de Delauney
				DelauneyTriangulation triangulation = new DelauneyTriangulation(ngp);
				triangulation.CreateBoundingBox();
				triangulation.Start();

				// Récupération de la position du toit à partir de la triangulation
				float posX = (float)triangulation.Triangles[0].NodeA.Longitude;
				float posZ = (float)triangulation.Triangles[0].NodeA.Latitude;

				// Construction et paramétrage de l'objet 3D destiné à former un toit
				GameObject newRoof = roofBuilder.BuildRoof(posX, posZ, triangulation, ngp.NbFloor, Dimensions.FLOOR_HEIGHT);
				newRoof.name = ngp.Id;

				// Ajout du toit au groupe de toits
				newRoof.transform.parent = roofs.transform;
			}
		}
	}


	/// <summary>
	/// 	Place les routes dans la scène.
	/// </summary>
	public void BuildRoads() {
		// Récupération de l'objet contenant les routes classiques et ajout de celui-ci à la ville
		highways = new GameObject(ObjectNames.HIGHWAYS);
		highways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les pistes cyclables et ajout de celui-ci à la ville
		cycleways = new GameObject(ObjectNames.CYCLEWAYS);
		cycleways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins piétons et ajout de celui-ci à la ville
		footways = new GameObject(ObjectNames.FOOTWAYS);
		footways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins voies de bus et ajout de celui-ci à la ville
//		busways = new GameObject(ObjectNames.BUSWAYS);
//		busways.transform.parent = cityComponents.transform;

		// Récupération de l'objet contenant les chemins voies maritimes et ajout de celui-ci à la ville
//		waterways = new GameObject(ObjectNames.WATERWAYS);
//		waterways.transform.parent = cityComponents.transform;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( (ngp.IsHighway () && (ngp.IsResidential () || ngp.IsPrimary() || ngp.IsSecondary() || ngp.IsTertiary() 
				|| ngp.IsService() || ngp.IsUnclassified() || ngp.IsCycleWay() || ngp.IsFootway())) 
				/*|| ngp.isBusWayLane()*/ || ngp.IsWaterway() ) {

				for (int i = 0; i < ngp.NodeCount () - 1; i++) {
					Node currentNode = ngp.GetNode (i);
					Node nextNode = ngp.GetNode (i + 1);

					// Calcul des coordonnées du milieu du vectuer formé par les 2 noeuds consécutifs
					Vector3 node1 = new Vector3 ((float)currentNode.Longitude, 0, (float)currentNode.Latitude);
					Vector3 node2 = new Vector3 ((float)nextNode.Longitude, 0, (float)nextNode.Latitude);
					Vector3 delta = node2 - node1;

					double posX = 0;
					double posY = 0;

					double length = 0.06;
					double width = Dimensions.ROAD_WIDTH;

					double angle = 0;

					// Calcul de la position de la route, dépendant du signe du delta entre les deux noeuds
					if (delta.z <= 0) {
						posX = ngp.GetNode (i + 1).Longitude;
						posY = ngp.GetNode (i + 1).Latitude;
						length = Math.Sqrt (Math.Pow (nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow (nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double)Vector3.Angle (Vector3.right, delta) + 180;
					} else {
						posX = ngp.GetNode (i).Longitude;
						posY = ngp.GetNode (i).Latitude;
						length = Math.Sqrt (Math.Pow (nextNode.Latitude - currentNode.Latitude, 2) + Math.Pow (nextNode.Longitude - currentNode.Longitude, 2));
						angle = (double)Vector3.Angle (Vector3.right, -delta) + 180;
					}

					if (ngp.IsHighway () && (ngp.IsResidential () || ngp.IsPrimary () || ngp.IsSecondary () || ngp.IsTertiary () || ngp.IsService () || ngp.IsUnclassified ())) {
						// Construction et paramétrage de l'objet 3D destiné à former une route classique
						GameObject newClassicHighway = roadBuilder.BuildClassicHighway ((float)posX, (float)posY, (float)length, (float)width, (float)angle);
						newClassicHighway.name = currentNode.Reference + " to " + nextNode.Reference;

						// Ajout de la route au groupe de routes
						newClassicHighway.transform.parent = highways.transform;
					} else if (ngp.IsCycleWay ()) {
						// Construction et paramétrage de l'objet 3D destiné à former une piste cyclable
						GameObject newCycleway = roadBuilder.BuildCycleway ((float)posX, (float)posY, (float)length, (float)width / 2F, (float)angle);
						newCycleway.name = currentNode.Reference + " to " + nextNode.Reference;

						// Ajout de la piste cyclable au groupe de pistes cyclables
						newCycleway.transform.parent = cycleways.transform;
					} else if (ngp.IsFootway ()) {
						// Construction et paramétrage de l'objet 3D destiné à former un chemin piéton
						GameObject newFootway = roadBuilder.BuildFootway ((float)posX, (float)posY, (float)length, (float)width / 1.5F, (float)angle);
						newFootway.name = currentNode.Reference + " to " + nextNode.Reference;

						// Ajout du chemin piéton au groupe de chemins piétons
						newFootway.transform.parent = footways.transform;
// 					} else if (ngp.isBusWayLane ()) {
						// Construction et paramétrage de l'objet 3D destiné à former une voie de bus
// 						newHighWay = roadBuilder.createBusLane ((float)x, (float)y, (float)length, (float)width, (float)angle);

						// Ajout de la voie maritime au groupe de voies de bus
						// newBusways.transform.parent = busways.transform;
					} else if (ngp.IsWaterway ()) {
						// Construction et paramétrage de l'objet 3D destiné à former une voie de bus
						GameObject newWaterway = roadBuilder.BuildWaterway ((float)posX, (float)posY, (float)length, (float)width / 1.5F, (float)angle);
						newWaterway.name = currentNode.Reference + " to " + nextNode.Reference;

						// Ajout de la voie maritime au groupe de voies maritimes
//						newWaterway.transform.parent = waterways.transform;
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
		trees = new GameObject(ObjectNames.TREES);
		trees.transform.parent = cityComponents.transform;

		float height = 0.12F;
		float diameter = 0.08F;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.IsTree() ) {
				// Récupération de la position de l'arbre depuis le groupe de noeuds correspondant
				double posX = ngp.GetNode (0).Longitude;
				double posZ = ngp.GetNode (0).Latitude;

				// Création et paramétrage de l'objet 3D (cylindre) destiné à former un tronc d'arbre 
				GameObject trunk = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
				trunk.name = "Trunk";
				trunk.tag = NodeTags.TREE_TAG;
				trunk.transform.position = new Vector3 ((float)posX, height / 2F, (float)posZ);
				trunk.transform.localScale = new Vector3 (height / 6F, height / 2F, height / 6F);

				// Création et paramétrage de l'objet 3D (sphere) destiné à former un feuillage 
				GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				foliage.name = "Foliage";
				foliage.tag = NodeTags.TREE_TAG;
				foliage.transform.position = new Vector3 ((float)posX, height, (float)posZ);
				foliage.transform.localScale = new Vector3 (diameter, diameter, diameter);

				// Affectation du matériau au tronc pour lui donner la texture voulue
				MeshRenderer trunkMeshRenderer = trunk.GetComponent<MeshRenderer> ();
				trunkMeshRenderer.material = Resources.Load (Materials.TREE_TRUNK) as Material;

				// Affectation du matériau au feuillage pour lui donner la texture voulue
				MeshRenderer foliageMeshRenderer = foliage.GetComponent<MeshRenderer> ();
				foliageMeshRenderer.material = Resources.Load (Materials.TREE_LEAF) as Material;

				GameObject tree = new GameObject (ngp.Id);

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
	public void BuildTrafficLights() {
		double x, z;
		float height = 0.03F;
		float diameter = 0.015F;

		foreach (NodeGroup ngp in nodeGroups) {
			if ( ngp.IsHighway() && ngp.IsTrafficLight() ) {
				for (int i = 0; i < ngp.NodeCount(); i++) {
					z = ngp.GetNode (i).Latitude;
					x = ngp.GetNode (i).Longitude;

					// Création et paramétrage de l'objet 3D (cylindre) destiné à former un support du feu tricolore 
					GameObject mount = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
					mount.name = "Support de feu tricolore";
					mount.transform.position = new Vector3 ((float)x, height / 2f, (float)z);
					mount.transform.localScale = new Vector3 (0.005F, 0.015F, 0.005F);

					// Création et paramétrage de l'objet 3D (cube) destiné à former un lumières du feu tricolore 
					GameObject lights = GameObject.CreatePrimitive (PrimitiveType.Cube);
					lights.name = "Lumière de feu tricolore";
					lights.transform.position = new Vector3 ((float)x, height, (float)z);
					lights.transform.localScale = new Vector3 (diameter, diameter * 2F, diameter);

					// Affectation du matériau au support pour lui donner la texture voulue
					MeshRenderer mountMeshRenderer = mount.GetComponent<MeshRenderer> ();
					mountMeshRenderer.material = Resources.Load (Materials.METAL) as Material;

					// Affectation du matériau aux lumières pour lui donner la texture voulue
					MeshRenderer lightsMeshRenderer = lights.GetComponent<MeshRenderer> ();
					lightsMeshRenderer.material = Resources.Load (Materials.TRAFFIC_LIGHT) as Material;

					// Création de l'objet 3D destiné à former un feu tricolore
					GameObject trafficLight = new GameObject (ngp.Id);

					// Ajout du support et des feux au feu tricolore
					mount.transform.parent = trafficLight.transform;
					lights.transform.parent = trafficLight.transform;
				}
			}
		}	
	}


	/// <summary>
	/// 	Place la caméra dans la scène.
	/// </summary>
	public void BuildMainCamera() {
		// On centre la camera 
		double camLat = (minLat * Main.SCALE_FACTOR + maxLat * Main.SCALE_FACTOR) / 2F;
		double camLon = (minLon * Main.SCALE_FACTOR + maxLon * Main.SCALE_FACTOR) / 2F;

		// Création de l'objet 3D représenant la caméra
		GameObject mainCameraGo = Camera.main.gameObject;

		// Création de l'objet représentant une lumière et servant à éclairer le champ de vision de la caméra
		Light mainLight = mainCameraGo.AddComponent<Light> ();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;

		// Ajout d'un controlleur de caméra à la caméra
		mainCameraGo.AddComponent <CameraController> ();
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

			MapBackground mapBackground = mapBackgrounds[backgroundName];

			minLat = mapBackground.MinLat;
			minLon = mapBackground.MinLon;

			maxLat = mapBackground.MaxLat;
			maxLon = mapBackground.MaxLon;
		} else {
			groundMaterial = Resources.Load(Materials.GROUND) as Material;
			groundMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
		}

		double lat = (minLat * Main.SCALE_FACTOR + maxLat * Main.SCALE_FACTOR) / 2F;
		double lon = (minLon * Main.SCALE_FACTOR + maxLon * Main.SCALE_FACTOR) / 2F;

		double length = maxLon * Main.SCALE_FACTOR - minLon * Main.SCALE_FACTOR;
		double width = maxLat * Main.SCALE_FACTOR - minLat * Main.SCALE_FACTOR;

		Vector2 textureExpansion = Vector2.one;
		if (backgroundName == null)
			textureExpansion = new Vector2((float)length * 4F, (float)width * 4F);

		// Calcul des coordonnées du milieu du vecteur formé par les 2 noeuds des extrémités
		Vector3 node1 = new Vector3((float)maxLon, 0, (float)maxLat);
		Vector3 node2 = new Vector3((float)minLon, 0, (float)minLat);

		Vector3 diff = node2 - node1;

		// Construction de l'objet 3D (cube) destiné à former un sol 
		groundBuilder.BuildGround ((float)length, (float)width, (float)minLat, (float)minLon, groundMaterial, textureExpansion);
	}

	public List<NodeGroup> NodeGroups {
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

	private class CityBuilderHolder {
		internal static CityBuilder instance = new CityBuilder();
	}
}
