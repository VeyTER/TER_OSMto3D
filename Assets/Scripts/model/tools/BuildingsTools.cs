﻿using System;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 	Suite d'outils en rapport avec la modification des bâtiments. Certaines des méthodes vont modifier le bâtiment
/// 	dans les fichiers de données.
/// </summary>
public class BuildingsTools {
	private static BuildingsTools instance;

	/// <summary>Chemin vers le fichier résumant la carte OSM.</summary>
	private string resumeFilePath;

	/// <summary>Document XML représentant le résumé de la carte OSM.</summary>
	private XmlDocument mapResumeDocument;


	/// <summary>Chemin vers le fichier contenant les bâtiments personnalisés la carte OSM.</summary>
	private string customFilePath;

	/// <summary>Document XML représentant les bâtiments personnalisés la carte OSM.</summary>
	private XmlDocument mapCustomDocument;

	private OsmTools osmTools;

	/// <summary>
	/// 	Table faisant la correspondance entre les de bâtiments 3D et les groupes de noeuds correspondant.
	/// </summary>
	private Dictionary<GameObject, string> buildingToNodeGroupTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les groupes de noeuds et les bâtiments 3D correspondant.
	/// </summary>
	private Dictionary<string, GameObject> nodeGroupToBuildingTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les noeuds 3D et les leurs noeuds 3D correspondant.
	/// </summary>
	private Dictionary<GameObject, string> buildingNodeToNodeTable;

	private Dictionary<GameObject, GameObject> buildingToDataDisplayTable;
	private Dictionary<GameObject, GameObject> dataDisplayToBuildingTable;

	private BuildingsTools () {
		this.resumeFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		this.mapResumeDocument = new XmlDocument();

		this.customFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";
		this.mapCustomDocument = new XmlDocument ();

		this.osmTools = OsmTools.GetInstance();

		this.buildingToNodeGroupTable = new Dictionary<GameObject, string> ();
		this.nodeGroupToBuildingTable = new Dictionary<string, GameObject> ();

		this.buildingNodeToNodeTable = new Dictionary<GameObject, string> ();

		this.buildingToDataDisplayTable = new Dictionary<GameObject, GameObject>();
		this.dataDisplayToBuildingTable = new Dictionary<GameObject, GameObject>();
	}

	public static BuildingsTools GetInstance() {
		if (instance == null)
			instance = new BuildingsTools();
		return instance;
	}

	public void ReplaceMaterial(GameObject building, Material newMaterial) {
		GameObject walls = building.transform.GetChild(CityBuilder.WALLS_INDEX).gameObject;
		Renderer meshRenderer = walls.GetComponent<Renderer>();

		if (meshRenderer != null) {
			meshRenderer.materials = new Material[] {
				newMaterial,
			};
		}
	}

	public void ReplaceColor(GameObject building, Color newColor) {
		GameObject walls = building.transform.GetChild(CityBuilder.WALLS_INDEX).gameObject;
		Renderer meshRenderer = walls.GetComponent<Renderer>();

		if (meshRenderer != null) {
			Material mainMaterial = meshRenderer.materials[0];
			mainMaterial.color = newColor;
		}
	}

	/// <summary>
	/// 	Ajoute une couche de couleur à un bâtiment pour le marquer comme sélectionné.
	/// </summary>
	/// <param name="building">Bâtiment à colorier.</param>
	public void ColorAsSelected(GameObject building) {
		Material wallMaterial = Resources.Load (Materials.WALL_DEFAULT) as Material;
		Material selectedElementMaterial = Resources.Load (Materials.BLUE_OVERLAY) as Material;

		// Supperposition du matériau de base avec celui de la couleur de sélection pour le bâtiment sélectionné
		foreach (Transform builginPartTransform in building.transform) {
			Renderer meshRenderer = builginPartTransform.GetComponent<Renderer> ();
			if (meshRenderer != null) {
				// Ecrasement de stock de matériaux du mur avec un nouveau stock contenant son matériau de base et le
				// matériau de sélection
				meshRenderer.materials = new Material[] {
					meshRenderer.materials[0],
					selectedElementMaterial
				};
			}
		}
	}


	/// <summary>
	/// 	Ajoute une couche de couleur à un bâtiment pour le marquer comme sélectionné.
	/// </summary>
	/// <param name="building">Bâtiment à colorier.</param>
	public void DiscolorAsSelected(GameObject building) {
		Material wallMaterial = Resources.Load(Materials.WALL_DEFAULT) as Material;

		// Supperposition du matériau de base avec celui de la couleur de sélection pour le bâtiment sélectionné
		foreach (Transform builginPartTransform in building.transform) {
			Renderer meshRenderer = builginPartTransform.GetComponent<Renderer>();
			if (meshRenderer != null) {
				// Ecrasement de stock de matériaux du mur avec un nouveau stock contenant son matériau de base et le
				// matériau de sélection
				meshRenderer.materials = new Material[] {
					meshRenderer.materials[0],
				};
			}
		}
	}

	/// <summary>
	/// 	Modifie la hauteur et l'orientation d'un bâtiment. Pour cela, la méthode modifie la hauteur et de la
	/// 	position de tous les murs du bâtiment.
	/// </summary>
	/// <param name="wallSetGo">Bâtiment à modifier.</param>
	/// <param name="nbFloor">Nombre d'étages qui doivent former le bâtiment.</param>
	public void ChangeBuildingHeight(GameObject building, int nbFloor) {
		GameObject buildingWalls = building.transform.GetChild(CityBuilder.WALLS_INDEX).gameObject;
		GameObject buildingRoof = building.transform.GetChild(CityBuilder.ROOF_INDEX).gameObject;

		this.ChangeWallsHeight(buildingWalls, nbFloor);

		float newBuildingHeight = nbFloor * Dimensions.FLOOR_HEIGHT;
		Vector3 roofPosition = buildingRoof.transform.localPosition;
		buildingRoof.transform.localPosition = new Vector3(roofPosition.x, newBuildingHeight, roofPosition.z);
	}

	public void ChangeWallsHeight(GameObject walls, int nbFloor) {
		float newBuildingHeight = nbFloor * Dimensions.FLOOR_HEIGHT;

		MeshFilter wallMeshFilter = walls.GetComponent<MeshFilter>();
		List<Vector3> newVertices = new List<Vector3>();
		wallMeshFilter.mesh.GetVertices(newVertices);
		for (int i = 1; i < wallMeshFilter.mesh.vertexCount; i += 2) {
			Vector3 vertexPosition = wallMeshFilter.mesh.vertices[i];
			newVertices[i] = new Vector3(vertexPosition.x, newBuildingHeight, vertexPosition.z);
		}
		wallMeshFilter.mesh.SetVertices(newVertices);

		for (int i = 1; i < wallMeshFilter.mesh.uv.Length; i += 2) {
			Vector2 uvVertexPosition = wallMeshFilter.mesh.uv[i];
			wallMeshFilter.mesh.uv[i] = new Vector2(uvVertexPosition.x, nbFloor);
		}

		MeshCollider wallsMeshCollider = walls.GetComponent<MeshCollider>();
		wallsMeshCollider.sharedMesh = wallMeshFilter.sharedMesh;
	}

	/// <summary>
	/// 	Change le nom d'un bâtiment dans l'application et dans les fichiers MapResumed et MapCustom.
	/// </summary>
	/// <param name="building">Bâtiment à renommer.</param>
	/// <param name="newName">Nouveau nom à donner au bâtiment.</param>
	public void UpdateName(GameObject building) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		BuildingNodeGroup buldingNodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(buldingNodeGroup.Id))
			this.AppendCustomBuilding(buldingNodeGroup);

		if (File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			// Récupération de l'attribut de nom du bâtiment dans les fichiers MapResumed et MapCustom
			XmlAttribute resumeNameAttribute = this.ResumeNodeGroupAttribute (buldingNodeGroup, XmlAttributes.NAME);
			XmlAttribute customNameAttribute = this.CustomNodeGroupAttribute (buldingNodeGroup, XmlAttributes.NAME);

			// Changement du nom du groupe de noeuds correspondant au bâtiment
			buldingNodeGroup.Name = building.name;

			// Changement du nom des attributs de nom dans les fichiers
			resumeNameAttribute.Value = building.name;
			customNameAttribute.Value = building.name;

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}
	}


	/// <summary>
	/// 	Met à jour la position des noeuds d'un bâtiment dans l'application et dans les fichiers MapResumed
	/// 	et MapCustom.
	/// </summary>
	/// <param name="building">Bâtiment à déplacer.</param>
	public void UpdateLocation(GameObject building) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		BuildingNodeGroup buldingNodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(buldingNodeGroup.Id))
			this.AppendCustomBuilding(buldingNodeGroup);

		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);
			
			// Récupération des noeuds correspondant au bâtiment dans les fichiers MapResumed et MapCustom
			string xPath = "/" + XmlTags.EARTH + "//" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + buldingNodeGroup.Id + "\"]";

			XmlNode resumeBuildingNode = mapResumeDocument.SelectSingleNode (xPath).ParentNode;
			XmlNode customBuildingNode = mapCustomDocument.SelectSingleNode (xPath).ParentNode;

			// Suppression des noeuds fils dans le noeud trouvé à l'intérieur du fichier map_custom en vue de
			// leur remplacement
			for (int i = 0; i < customBuildingNode.ChildNodes.Count;) {
				XmlNode customBuildingChildNode = customBuildingNode.ChildNodes [i];
				if (!customBuildingChildNode.Name.Equals (XmlTags.INFO))
					customBuildingNode.RemoveChild (customBuildingChildNode);
				else
					i++;
			}

			// Importation dans le fichier map_custom de chaque nd du fichier map_resumedd correspondant au noeud courant
			// du groupe de noeud correspondant au bâtiment
			XmlNodeList resumedBuildingNd = resumeBuildingNode.ChildNodes;
			foreach(Node node in buldingNodeGroup.Nodes) {

				// Recherche du nd correspondant au noeud courant à partir le la référence et de l'index du noeud.
				// En effet, deux nd peuvent avoir la même référence à l'interieur d'un même noeud dans un fichier
				// (pour faire boucler les bâtiments), c'est pourquoi, un index a été ajouté pour cibler précisément le
				// nd voulu
				int i = 1;
				for (; i < resumedBuildingNd.Count && (!resumedBuildingNd [i].Attributes [XmlAttributes.REFERENCE].Value.Equals (node.Reference)
													|| !resumedBuildingNd [i].Attributes [XmlAttributes.INDEX].Value.Equals (node.Index.ToString ())); i++);

				// Mise à jour de la position du nd s'il a été trouvé
				if (i < resumedBuildingNd.Count) {
					// Mise à jour de la position
					resumedBuildingNd [i].Attributes [XmlAttributes.LATITUDE].Value = (node.Latitude / Dimensions.NODE_SCALE).ToString();
					resumedBuildingNd [i].Attributes [XmlAttributes.LONGIUDE].Value = (node.Longitude / Dimensions.NODE_SCALE).ToString();

					// Importation du nd dans le fichier map_resumedd
					XmlNode customBuildingNd = mapCustomDocument.ImportNode (resumedBuildingNd [i], true);
					customBuildingNode.AppendChild (customBuildingNd);
				}
			}

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}
	}


	/// <summary>
/// 	Change la hauteur d'un bâtiment d'un étage à la fois au niveau du groupe de noeuds correspondant, de la 
/// 	scène 3D et dans les fichiers MapResumed et MapCustom. 
	/// </summary>
	/// <param name="building">Bâtiment à modifier.</param>
	/// <param name="nbFloors">nombre d'étage que doit avoir le bâtiment.</param>
	public void UpdateHeight(GameObject building, int nbFloor) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		BuildingNodeGroup buldingNodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(buldingNodeGroup.Id))
			this.AppendCustomBuilding(buldingNodeGroup);

		if(File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);

			buldingNodeGroup.NbFloor = nbFloor;

			// Modification de l'attribut contenant le nombre d'étages du bâtiment dans le fichier map_resumedd
			XmlAttribute resumefloorAttribute = this.ResumeNodeGroupAttribute (buldingNodeGroup, XmlAttributes.NB_FLOOR);
			XmlAttribute customfloorAttribute = this.CustomNodeGroupAttribute (buldingNodeGroup, XmlAttributes.NB_FLOOR);

			resumefloorAttribute.Value = Math.Max(nbFloor, 1).ToString();
			customfloorAttribute.Value = Math.Max(nbFloor, 1).ToString();

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}

		// Modification de la hauteur du bâtiment 3D
		this.ChangeBuildingHeight (building, nbFloor);
	}

	public void UpdateMaterial(GameObject building, Material newMaterial) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		BuildingNodeGroup buldingNodeGroup = this.BuildingToNodeGroup(building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(buldingNodeGroup.Id))
			this.AppendCustomBuilding(buldingNodeGroup);

		if (File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			buldingNodeGroup.CustomMaterial = newMaterial;

			XmlNode resumeInfoNode = this.GetResumeInfoNode(buldingNodeGroup);
			XmlNode customInfoNode = this.GetCustomInfoNode(buldingNodeGroup);

			string materialName = newMaterial.name.Replace(" (Instance)", "");
			if (resumeInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL] == null)
				osmTools.AppendAttribute(mapResumeDocument, resumeInfoNode, XmlAttributes.CUSTOM_MATERIAL, materialName);
			else
				resumeInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL].Value = materialName;

			if (customInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL] == null)
				osmTools.AppendAttribute(mapCustomDocument, customInfoNode, XmlAttributes.CUSTOM_MATERIAL, materialName);
			else
				customInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL].Value = materialName;

			mapResumeDocument.Save(resumeFilePath);
			mapCustomDocument.Save(customFilePath);
		}
	}

	public void UpdateColor(GameObject building, Color newColor) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		BuildingNodeGroup buildingNodeGroup = this.BuildingToNodeGroup(building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(buildingNodeGroup.Id))
			this.AppendCustomBuilding(buildingNodeGroup);

		if (File.Exists(resumeFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			buildingNodeGroup.OverlayColor = newColor;

			XmlNode resumeInfoNode = this.GetResumeInfoNode(buildingNodeGroup);
			XmlNode customInfoNode = this.GetCustomInfoNode(buildingNodeGroup);

			String colorName = newColor.r + ";" + newColor.g + ";" + newColor.b;
			if (resumeInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR] == null)
				osmTools.AppendAttribute(mapResumeDocument, resumeInfoNode, XmlAttributes.OVERLAY_COLOR, colorName);
			else
				resumeInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR].Value = colorName;

			if (customInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR] == null)
				osmTools.AppendAttribute(mapCustomDocument, customInfoNode, XmlAttributes.OVERLAY_COLOR, colorName);
			else
				customInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR].Value = colorName;

			mapResumeDocument.Save(resumeFilePath);
			mapCustomDocument.Save(customFilePath);
		}
	}

	public void AppendResumeBuilding(NodeGroup nodeGroup) {
		if (File.Exists(resumeFilePath) && nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			mapResumeDocument.Load(resumeFilePath);

			BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;

			string zoningXPath = buildingNodeGroup.ToZoningXPath();

			XmlNode containerNode = mapResumeDocument.SelectSingleNode(zoningXPath);
			XmlNode buildingNode = mapResumeDocument.CreateElement(XmlTags.BUILDING);
			XmlNode buildingInfoNode = mapResumeDocument.CreateElement(XmlTags.INFO);

			containerNode.AppendChild(buildingNode);
			buildingNode.AppendChild(buildingInfoNode);

			osmTools.AppendAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.ID, buildingNodeGroup.Id);
			osmTools.AddBuildingNodeAttribute(mapResumeDocument, buildingNodeGroup, buildingInfoNode);

			foreach (Node node in buildingNodeGroup.Nodes) {
				XmlNode newNd = mapResumeDocument.CreateElement(XmlTags.ND);
				osmTools.AppendAttribute(mapResumeDocument, newNd, XmlAttributes.REFERENCE, node.Reference);
				osmTools.AppendAttribute(mapResumeDocument, newNd, XmlAttributes.INDEX, node.Index.ToString());
				osmTools.AppendAttribute(mapResumeDocument, newNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
				osmTools.AppendAttribute(mapResumeDocument, newNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
				buildingNode.AppendChild(newNd);
			}

			mapResumeDocument.Save(resumeFilePath);
		}
	}

	/// <summary>
	/// 	Ajoute une entrée représentant un bâtiment dans le fichier map_custom.
	/// </summary>
	/// <param name="nodeGroup">Groupe de noeuds correspondant au bâtiment.</param>
	public void AppendCustomBuilding(NodeGroup nodeGroup) {
		if (File.Exists(customFilePath) && nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			mapCustomDocument.Load(customFilePath);

			BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;

			// Récupération du noeud XML englobant tous les objets terrestres et ajout de celui-ci au fichier map_custom
			// s'il n'est pas présent
			XmlNodeList earthNodes = mapCustomDocument.GetElementsByTagName(XmlTags.EARTH);
			if (earthNodes.Count == 0)
				mapCustomDocument.AppendChild(mapCustomDocument.CreateElement(XmlTags.EARTH));

			// Création d'un noeud XML d'information pour le noeud XML du bâtiment
			XmlNode earthNode = earthNodes[0];
			XmlNode buildingNode = mapCustomDocument.CreateElement(XmlTags.BUILDING);
			XmlNode buildingInfoNode = mapCustomDocument.CreateElement(XmlTags.INFO);

			// Ajout du noeud XML d'information dans le fichier map_custom
			earthNode.AppendChild(buildingNode);
			buildingNode.AppendChild(buildingInfoNode);

			// Ajout des attributs et de la valeurs dans le noeud XML d'information
			osmTools.AppendAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.ID, buildingNodeGroup.Id);
			osmTools.AddBuildingNodeAttribute(mapCustomDocument, buildingNodeGroup, buildingInfoNode);

			mapCustomDocument.Save(customFilePath);
		}
	}


	/// <summary>
	/// 	Indique si une entrée correspondant à un certain bâtiment existe dans le fichier map_custom.
	/// </summary>
	/// <returns><c>true</c>, si l'entrée existe, <c>false</c> sinon.</returns>
	/// <param name="nodeGroupId">ID du bâtiment dont on veut vérifier l'existance.</param>
	private bool CustomBuildingExists(string nodeGroupId) {
		if (File.Exists(customFilePath)) {
			mapCustomDocument.Load(customFilePath);
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroupId + "\"]";
			return mapCustomDocument.SelectSingleNode(xPath) != null;
		} else {
			return false;
		}
	}


	private XmlNode GetResumeInfoNode(NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;
			string zoningXPath = buildingNodeGroup.ToFullBuildingXPath();
			return mapResumeDocument.SelectSingleNode(zoningXPath);
		} else {
			return null;
		}
	}

	private XmlNode GetCustomInfoNode(NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";
			return mapCustomDocument.SelectSingleNode(xPath);
		} else {
			return null;
		}
	}


	/// <summary>
	/// 	Renvoie l'attribut d'un groupe de noeuds présent dans le fichier map_resumedd et identifié par le zonage de
	/// 	ce groupe de noeuds et l'ID de ce dernier.
	/// </summary>
	/// <returns>Attribut trouvé dans le fichier map_resumedd.</returns>
	/// <param name="nodeGroup">Groupe de noeuds contenant l'attribut.</param>
	/// <param name="attributeName">Nom de l'attribut à trouver.</param>
	private XmlAttribute ResumeNodeGroupAttribute(NodeGroup nodeGroup, string attributeName) {
		XmlAttribute res = null;
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;

			string zoningXPath = buildingNodeGroup.ToFullBuildingXPath();

			// Récupération du noeud XML d'information et récupération de l'attribut ciblé
			XmlNode infoNode = mapResumeDocument.SelectSingleNode(zoningXPath);
			res = infoNode.Attributes[attributeName];

		}
		return res;
	}


	/// <summary>
	/// 	Renvoie l'attribut d'un groupe de noeuds présents dans le fichier map_custom et identifié l'ID de ce groupe
	/// 	de noeuds.
	/// </summary>
	/// <returns>Attribut trouvé dans le fichier map_custom.</returns>
	/// <param name="nodeGroupeId">ID du groupe de noeuds contenant l'attribut.</param>
	/// <param name="attributeName">Nom de l'attribut à trouver.</param>
	private XmlAttribute CustomNodeGroupAttribute(NodeGroup nodeGroup, string attributeName) {
		XmlAttribute res = null;
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";
			XmlNode infoNode = mapCustomDocument.SelectSingleNode(xPath);
			res = infoNode.Attributes[attributeName];
		}
		return res;
	}


	/// <summary>
	/// 	Met à jour la position des noeuds correspondant à un bâtiment 3D pour que la position des sommets de ce
	/// 	dernier corresponde à la position de ses noeuds.
	/// </summary>
	/// <param name="building">Bâtiment, de type GameObject, dont on veut mettre à jour les noeuds.</param>
	public void UpdateNodesPosition(GameObject building) {
		// Récupération du groupe de noeuds correspondant au bâtiment qui est aussi le noeud parent du noeud a trouver
		BuildingNodeGroup parentNodeGroup = this.BuildingToNodeGroup (building);

		// Récupération du groupe de noeuds 3D correspondant au bâtiment
		GameObject buildingNodeGroupGo = this.BuildingToBuildingNodeGroup (building);

		// Affectation de la position des noeuds 3D de bâtiments aux noeuds correspondants
		foreach(Transform buildingNodeTransform in buildingNodeGroupGo.transform) {
			Node node = this.BuildingNodeToNode (buildingNodeTransform.gameObject, parentNodeGroup);
			node.Latitude = buildingNodeTransform.position.z;
			node.Longitude = buildingNodeTransform.position.x;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un bâtiment 3D en faisant la moyenne de la position de ses murs.
	/// </summary>
	/// <returns>Centre du batiment.</returns>
	/// <param name="building">Batiment, de type GameObject, dont on veut calculer le centre.</param>
	public Vector2 BuildingCenter(NodeGroup nodeGroup) {
		// Somme des coordonnées des murs de bâtiments
		if (nodeGroup != null && (nodeGroup.GetType() == typeof(BuildingNodeGroup) || nodeGroup.GetType() == typeof(LeisureNodeGroup))) {
			Vector2 positionSum = Vector2.zero;
			for (int i = 0; i < nodeGroup.NodeCount() - 1; i++)
				positionSum += nodeGroup.GetNode(i).ToVector();

			return positionSum / ((nodeGroup.Nodes.Count - 1) * 1F);
		} else {
			return Vector2.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un bâtiment 3D en faisant la moyenne de la position de ses murs.
	/// </summary>
	/// <returns>Centre du batiment.</returns>
	/// <param name="building">Batiment, de type GameObject, dont on veut calculer le centre.</param>
	public Vector3 BuildingCenter(GameObject building) {
		BuildingNodeGroup buildingNodeGroup = this.BuildingToNodeGroup(building);

		// Somme des coordonnées des murs de bâtiments
		if (buildingNodeGroup != null) {
			Vector3 positionSum = Vector3.zero;
			for(int i = 0; i < buildingNodeGroup.Nodes.Count - 1; i++) {
				Node buildingNode = buildingNodeGroup.Nodes [i];
				Vector3 nodePosition = new Vector3((float) buildingNode.Longitude, this.BuildingHeight(building), (float)buildingNode.Latitude);
				positionSum += nodePosition;
			}

			return positionSum / ((buildingNodeGroup.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un groupe de noeuds 3D d'un bâtiment en faisant la moyenne de la position
	/// 	de ces noeuds 3D.
	/// </summary>
	/// <returns>Centre du groupe de noeuds 3D.</returns>
	/// <param name="buildingNodeGroupGo">Groupe de noeuds 3D, de type GameObject, dont on veut calculer le centre.</param>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds, de type NodeGroup, correspondant au groupe de noeuds 3D.
	/// </param>
	public Vector3 BuildingNodesCenter(GameObject buildingNodeGroupGo, NodeGroup nodeGroup) {
		Vector3 positionSum = new Vector3(0, 0, 0);

		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			// Somme des coordonnées des noeuds 3D de bâtiments
			for (int i = 0; i < nodeGroup.Nodes.Count - 1; i++) {
				Node buildingNode = nodeGroup.Nodes[i];
				Vector3 nodePosition = new Vector3((float) buildingNode.Longitude, (float) buildingNodeGroupGo.transform.position.y, (float) buildingNode.Latitude);
				positionSum += nodePosition;
			}
			return positionSum / ((nodeGroup.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le "rayon" d'un bâtiment, c'est à dire la distance entre le sommet le plus éloigné du
	/// 	centre du bâtiment et ce dernier.
	/// </summary>
	/// <returns>Rayon du bâtiment.</returns>
	/// <param name="walls">Bâtiment dont on veut calculer le rayon.</param>
	public double BuildingRadius(GameObject walls) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup(walls);

		// Calcul du centre du bâtiment
		Vector3 buildingCenter = this.BuildingCenter(walls);

		// Calcul du rayon si le groupe de noeuds a bien été trouvé
		if (nodeGroup != null && nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			// Somme des distances de chaque sommet par rapport au centre avec mise à jour du maximum
			double maxDistance = 0;
			for (int i = 0; i < nodeGroup.Nodes.Count - 1; i++) {
				Node buildingNode = nodeGroup.Nodes[i];
				Vector2 nodePosition = new Vector2((float) buildingNode.Longitude, (float) buildingNode.Latitude);
				Vector2 buildingCenter2D = new Vector2(buildingCenter.x, buildingCenter.z);

				double currentDistance = Vector2.Distance(buildingCenter2D, nodePosition);
				if (currentDistance > maxDistance)
					maxDistance = currentDistance;
			}

			return maxDistance;
		} else {
			return -1;
		}
	}

	public float BuildingArea(NodeGroup nodeGroup) {
		float area = 0;
		for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
			Vector2 currentPoint = nodeGroup.GetNode(i).ToVector();
			Vector2 nextPoint = nodeGroup.GetNode(i + 1).ToVector();

			area += (nextPoint.x - currentPoint.x) * (nextPoint.y + currentPoint.y);
		}
		return area;
	}

	public float BuildingHeight(GameObject building) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup(building);
		return this.BuildingHeight(nodeGroup);
	}

	public float BuildingHeight(NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup))
			return ((BuildingNodeGroup) nodeGroup).NbFloor * Dimensions.FLOOR_HEIGHT;
		else
			return float.NaN;
	}

	public bool IsInfoAttributeValueUsed(string attributeName, string value) {
		if (File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			string xPath = "//" + XmlTags.INFO + "[@" + attributeName + "=\"" + value + "\"]";

			XmlNode resumeInfoNode = mapResumeDocument.SelectSingleNode(xPath);
			XmlNode customInfoNode = mapCustomDocument.SelectSingleNode(xPath);

			return resumeInfoNode != null || customInfoNode != null;
		} else {
			return false;
		}
	}

	public BuildingNodeGroup NewMinimalNodeGroup(Vector3 centerPosition, Vector2 dimensions, Material material) {
		string newBuildingId = this.NewIdOrReference(10);

		BuildingNodeGroup buildingNodeGroup = new BuildingNodeGroup(newBuildingId, null) {
			Name = "Bâtiment n°" + newBuildingId,
			NbFloor = 3,
			CustomMaterial = material
		};

		float halfLength = dimensions.x / 2F;
		float halfWidth = dimensions.y / 2F;

		Node topLeftWallNode = this.NewWallNode(0, centerPosition, new Vector2(-halfLength, -halfWidth));
		Node topRightWallNode = this.NewWallNode(1, centerPosition, new Vector2(halfLength, -halfWidth));
		Node bottomRightWallNode = this.NewWallNode(2, centerPosition, new Vector2(halfLength, halfWidth));
		Node bottomLeftWallNode = this.NewWallNode(3, centerPosition, new Vector2(-halfLength, halfWidth));
		Node topLeftDupliWallNode = this.NewWallNode(4, centerPosition, new Vector2(-halfLength, -halfWidth));

		buildingNodeGroup.AddNode(topLeftWallNode);
		buildingNodeGroup.AddNode(topRightWallNode);
		buildingNodeGroup.AddNode(bottomRightWallNode);
		buildingNodeGroup.AddNode(bottomLeftWallNode);
		buildingNodeGroup.AddNode(topLeftDupliWallNode);

		return buildingNodeGroup;
	}

	private Node NewWallNode(int index, Vector3 buildingCenter, Vector2 localPosition) {
		string newNodeReference = this.NewIdOrReference(10);
		return new BuildingStepNode(newNodeReference, index, buildingCenter.z + localPosition.x, buildingCenter.x + localPosition.y);
	}

	private string NewIdOrReference(int nbDigits) {
		string res = "+";
		System.Random randomGenerator = new System.Random();
		for (int i = 0; i < nbDigits; i++)
			res += Math.Floor((double) randomGenerator.Next(0, 10));
		return res;
	}

	/// <summary>
	/// 	Ajout une entrée dans la table de correspondance entre les bâtiments 3D et leurs groupes de noeuds
	/// 	correspondant.
	/// </summary>
	/// <param name="building">Bâtiment 3D en temps que clé.</param>
	/// <param name="nodeGroup">Groupe de noeuds en temps que valeur.</param>
	public void AddBuildingAndNodeGroupPair(GameObject building, NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			nodeGroupToBuildingTable[nodeGroup.Id] = building;
			buildingToNodeGroupTable[building] = nodeGroup.Id;
		}
	}

	/// <summary>
	/// 	Ajout une entrée dans la table de correspondance entre les noeuds 3D et leurs noeuds correspondant.
	/// </summary>
	/// <param name="buildingNode">Noeuds 3D en temps que clé.</param>
	/// <param name="node">Noeud en temps que valeur.</param>
	public void AddBuildingNodeAndNodeEntryPair(GameObject buildingNode, Node node) {
		buildingNodeToNodeTable[buildingNode] = Node.GenerateId(node);
	}

	public void AddBuildingAndDataDisplayEntryPair(GameObject building, GameObject dataDisplay) {
		buildingToDataDisplayTable[building] = dataDisplay;
		dataDisplayToBuildingTable[dataDisplay] = building;
	}

	/// <summary>
	/// 	Renvoie le groupe de noeuds correspondant à un bâtiment 3D grâce à une table de correspondance.
	/// </summary>
	/// <returns>Groupe de noeuds correspondant à un bâtiment 3D.</returns>
	/// <param name="building">
	/// 	Bâtiment 3D  dont on veut récupérer le Groupe de noeuds correspondant.
	/// </param>
	public BuildingNodeGroup BuildingToNodeGroup(GameObject building) {
		string nodeGroupId = buildingToNodeGroupTable[building];
		NodeGroup nodeGroup = NodeGroupBase.GetInstance().GetNodeGroup(nodeGroupId);

		if (nodeGroup.GetType() == typeof(BuildingNodeGroup))
			return (BuildingNodeGroup) nodeGroup;
		else
			return null;
	}

	/// <summary>
	/// 	Renvoie le bâtiment 3D correspondant à un groupe de noeuds grâce à une table de correspondance.
	/// </summary>
	/// <returns>Bâtiment 3D correspondant à un groupe de noeuds.</returns>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds dont on veut récupérer le bâtiment 3D correspondant.
	/// </param>
	public GameObject NodeGroupToBuilding(NodeGroup nodeGroup) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup))
			return nodeGroupToBuildingTable[nodeGroup.Id];
		else
			return null;
	}

	/// <summary>
	/// 	Renvoie le noeud correspondant à un noeud 3D grâce à une table de correspondance.
	/// </summary>
	/// <returns>Noeud correspondant à un noeud 3D.</returns>
	/// <param name="buildingNode">
	/// 	Noeud 3D dont on veut récupérer le noeud correspondant.
	/// </param>
	public Node BuildingNodeToNode(GameObject buildingNode, NodeGroup parentNodeGroup) {
		if (parentNodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			string nodeId = buildingNodeToNodeTable[buildingNode];
			return parentNodeGroup.GetNode(nodeId);
		} else {
			return null;
		}
	}

	public GameObject BuildingToDataDisplay(GameObject building) {
		return buildingToDataDisplayTable[building];
	}

	public GameObject DataDisplayToBuilding(GameObject dataDisplay) {
		return dataDisplayToBuildingTable[dataDisplay];
	}

	/// <summary>
	/// 	Renvoie le bâtiment 3D correspondant à un groupe de noeuds 3D.
	/// </summary>
	/// <returns>Objet 3D correspondant au groupe de noeuds 3D.</returns>
	/// <param name="building">Bâtiment 3D dont on veut récupérer le groupe de noeuds 3D corresondant.</param>
	public GameObject BuildingToBuildingNodeGroup(GameObject building) {
		BuildingNodeGroup buildingNodeGroup = this.BuildingToNodeGroup(building);
		GameObject buildingNodeGroupGo = building.transform.GetChild(CityBuilder.BUILDING_NODES_INDEX).gameObject;
		return buildingNodeGroupGo;
	}
}
