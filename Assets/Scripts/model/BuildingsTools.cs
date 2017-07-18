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
	/// <summary>Chemin vers le fichier résumant la carte OSM.</summary>
	private string resumeFilePath;

	/// <summary>Document XML représentant le résumé de la carte OSM.</summary>
	private XmlDocument mapResumeDocument;


	/// <summary>Chemin vers le fichier contenant les bâtiments personnalisés la carte OSM.</summary>
	private string customFilePath;

	/// <summary>Document XML représentant les bâtiments personnalisés la carte OSM.</summary>
	private XmlDocument mapCustomDocument;


	/// <summary>
	/// 	Table faisant la correspondance entre les de bâtiments 3D et les groupes de noeuds correspondant.
	/// </summary>
	private Dictionary<GameObject, string> buildingToNodeGroupTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les groupes de noeuds et les bâtiments 3D correspondant.
	/// </summary>
	private Dictionary<string, GameObject> nodeGroupToBuildingTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les groupes de noeuds 3D et les groupes de noeuds correspondant.
	/// </summary>
	private Dictionary<GameObject, string> buildingNodeGroupToNodeGroupTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les groupes de noeuds et les groupes de noeuds 3D correspondant.
	/// </summary>
	private Dictionary<string, GameObject> nodeGroupToBuildingNodeGroupTable;

	/// <summary>
	/// 	Table faisant la correspondance entre les noeuds 3D et les leurs noeuds 3D correspondant.
	/// </summary>
	private Dictionary<GameObject, string> buildingNodeToNodeTable;

	private Dictionary<GameObject, GameObject> buildingToDataDisplayTable;
	private Dictionary<GameObject, GameObject> dataDisplayToBuildingTable;

	private CityBuilder cityBuilder;

	private BuildingsTools () {
		this.resumeFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		this.mapResumeDocument = new XmlDocument();

		this.customFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";
		this.mapCustomDocument = new XmlDocument ();

		this.buildingToNodeGroupTable = new Dictionary<GameObject, string> ();
		this.nodeGroupToBuildingTable = new Dictionary<string, GameObject> ();

		this.buildingNodeGroupToNodeGroupTable = new Dictionary<GameObject, string> ();
		this.nodeGroupToBuildingNodeGroupTable = new Dictionary<string, GameObject> ();
		
		this.buildingNodeToNodeTable = new Dictionary<GameObject, string> ();

		this.buildingToDataDisplayTable = new Dictionary<GameObject, GameObject>();
		this.dataDisplayToBuildingTable = new Dictionary<GameObject, GameObject>();

		this.cityBuilder = CityBuilder.GetInstance();
	}

	public static BuildingsTools GetInstance() {
		return BuildingsToolsInstanceHolder.instance;
	}

	public void ReplaceMaterial(GameObject building, Material newMaterial) {
		foreach (Transform wallTransform in building.transform) {
			Renderer meshRenderer = wallTransform.GetComponent<Renderer>();

			if (meshRenderer != null) {
				meshRenderer.materials = new Material[] {
					newMaterial,
				};
			}
		}
	}

	public void ReplaceColor(GameObject building, Color newColor) {
		foreach (Transform wallTransform in building.transform) {
			Renderer meshRenderer = wallTransform.GetComponent<Renderer>();

			if (meshRenderer != null) {
				Material mainMaterial = meshRenderer.materials[0];
				mainMaterial.color = newColor;
			}
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
		foreach (Transform wallTransform in building.transform) {
			Renderer meshRenderer = wallTransform.GetComponent<Renderer> ();
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
		foreach (Transform wallTransform in building.transform) {
			Renderer meshRenderer = wallTransform.GetComponent<Renderer>();
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
	/// 	Change le nom d'un bâtiment dans l'application et dans les fichiers MapResumed et MapCustom.
	/// </summary>
	/// <param name="building">Bâtiment à renommer.</param>
	/// <param name="newName">Nouveau nom à donner au bâtiment.</param>
	public void UpdateName(GameObject building) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(nodeGroup.Id))
			this.AppendCustomBuilding(nodeGroup);

		if (File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			// Récupération de l'attribut de nom du bâtiment dans les fichiers MapResumed et MapCustom
			XmlAttribute resumeNameAttribute = this.ResumeNodeGroupAttribute (nodeGroup, XmlAttributes.NAME);
			XmlAttribute customNameAttribute = this.CustomNodeGroupAttribute (nodeGroup, XmlAttributes.NAME);

			// Changement du nom du groupe de noeuds correspondant au bâtiment
			nodeGroup.Name = building.name;

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
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(nodeGroup.Id))
			this.AppendCustomBuilding(nodeGroup);

		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);
			
			// Récupération des noeuds correspondant au bâtiment dans les fichiers MapResumed et MapCustom
			string xPath = "/" + XmlTags.EARTH + "//" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";

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
			foreach(Node node in nodeGroup.Nodes) {

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
					resumedBuildingNd [i].Attributes [XmlAttributes.LATITUDE].Value = (node.Latitude / Dimensions.SCALE_FACTOR).ToString();
					resumedBuildingNd [i].Attributes [XmlAttributes.LONGIUDE].Value = (node.Longitude / Dimensions.SCALE_FACTOR).ToString();

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
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(nodeGroup.Id))
			this.AppendCustomBuilding(nodeGroup);

		if(File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);

			nodeGroup.NbFloor = nbFloor;

			// Modification de l'attribut contenant le nombre d'étages du bâtiment dans le fichier map_resumedd
			XmlAttribute resumefloorAttribute = this.ResumeNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
			XmlAttribute customfloorAttribute = this.CustomNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);

			resumefloorAttribute.Value = Math.Max(nbFloor, 1).ToString();
			customfloorAttribute.Value = Math.Max(nbFloor, 1).ToString();

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}

		// Modification de la hauteur du bâtiment 3D
		CityBuilder cityBuilder = CityBuilder.GetInstance();
		cityBuilder.RebuildBuilding (building, nbFloor);
	}

	public void UpdateMaterial(GameObject building, Material newMaterial) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup nodeGroup = this.BuildingToNodeGroup(building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(nodeGroup.Id))
			this.AppendCustomBuilding(nodeGroup);

		if (File.Exists(resumeFilePath) && File.Exists(customFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			nodeGroup.CustomMaterial = newMaterial;

			XmlNode resumeInfoNode = this.GetResumeInfoNode(nodeGroup);
			XmlNode customInfoNode = this.GetCustomInfoNode(nodeGroup);

			string materialName = newMaterial.name.Replace(" (Instance)", "");
			if (resumeInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL] == null)
				this.AppendNodeGroupAttribute(mapResumeDocument, resumeInfoNode, XmlAttributes.CUSTOM_MATERIAL, materialName);
			else
				resumeInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL].Value = materialName;

			if (customInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL] == null)
				this.AppendNodeGroupAttribute(mapCustomDocument, customInfoNode, XmlAttributes.CUSTOM_MATERIAL, materialName);
			else
				customInfoNode.Attributes[XmlAttributes.CUSTOM_MATERIAL].Value = materialName;

			mapResumeDocument.Save(resumeFilePath);
			mapCustomDocument.Save(customFilePath);
		}
	}

	public void UpdateColor(GameObject building, Color newColor) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup nodeGroup = this.BuildingToNodeGroup(building);

		//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
		if (!this.CustomBuildingExists(nodeGroup.Id))
			this.AppendCustomBuilding(nodeGroup);

		if (File.Exists(resumeFilePath)) {
			mapResumeDocument.Load(resumeFilePath);
			mapCustomDocument.Load(customFilePath);

			nodeGroup.OverlayColor = newColor;

			XmlNode resumeInfoNode = this.GetResumeInfoNode(nodeGroup);
			XmlNode customInfoNode = this.GetCustomInfoNode(nodeGroup);

			String colorName = newColor.r + ";" + newColor.g + ";" + newColor.b;
			if (resumeInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR] == null)
				this.AppendNodeGroupAttribute(mapResumeDocument, resumeInfoNode, XmlAttributes.OVERLAY_COLOR, colorName);
			else
				resumeInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR].Value = colorName;

			if (customInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR] == null)
				this.AppendNodeGroupAttribute(mapCustomDocument, customInfoNode, XmlAttributes.OVERLAY_COLOR, colorName);
			else
				customInfoNode.Attributes[XmlAttributes.OVERLAY_COLOR].Value = colorName;

			mapResumeDocument.Save(resumeFilePath);
			mapCustomDocument.Save(customFilePath);
		}
	}

	public void AppendResumeBuilding(NodeGroup nodeGroup) {
		if (File.Exists(resumeFilePath)) {
			mapResumeDocument.Load(resumeFilePath);

			string zoningXPath = nodeGroup.ToZoningXPath();

			XmlNode containerNode = mapResumeDocument.SelectSingleNode(zoningXPath);
			XmlNode buildingNode = mapResumeDocument.CreateElement(XmlTags.BUILDING);
			XmlNode buildingInfoNode = mapResumeDocument.CreateElement(XmlTags.INFO);

			containerNode.AppendChild(buildingNode);
			buildingNode.AppendChild(buildingInfoNode);

			this.AppendNodeGroupAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.ID, nodeGroup.Id);
			this.AppendNodeGroupAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.NAME, nodeGroup.Name);
			this.AppendNodeGroupAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString());
			this.AppendNodeGroupAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString());
			this.AppendNodeGroupAttribute(mapResumeDocument, buildingInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);

			foreach (Node node in nodeGroup.Nodes) {
				XmlNode newNd = mapResumeDocument.CreateElement(XmlTags.ND);
				this.AppendNodeGroupAttribute(mapResumeDocument, newNd, XmlAttributes.REFERENCE, node.Reference);
				this.AppendNodeGroupAttribute(mapResumeDocument, newNd, XmlAttributes.INDEX, node.Index.ToString());
				this.AppendNodeGroupAttribute(mapResumeDocument, newNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
				this.AppendNodeGroupAttribute(mapResumeDocument, newNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
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
		if (File.Exists(customFilePath)) {
			mapCustomDocument.Load(customFilePath);

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
			this.AppendNodeGroupAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.ID, nodeGroup.Id);
			this.AppendNodeGroupAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.NAME, nodeGroup.Name);
			this.AppendNodeGroupAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString());
			this.AppendNodeGroupAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString());
			this.AppendNodeGroupAttribute(mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);

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


	/// <summary>
	/// 	Ajoute un attribut en rapport avec un groupe de noeuds dans un noeud XML. L'attribut aura le nom et la
	/// 	valeur spécifiés en entrée.
	/// </summary>
	/// <param name="boundingDocument">Document dans lequel ajouter l'attribut.</param>
	/// <param name="containerNode">Noeud XML dans laquelle ajouter l'attribut.</param>
	/// <param name="attributeName">Nom à donner au nouvel attribut.</param>
	/// <param name="attributeValue">Valeur à donner au nouvel attribut.</param>
	private void AppendNodeGroupAttribute(XmlDocument boundingDocument, XmlNode containerNode, string attributeName, string attributeValue) {
		XmlAttribute attribute = boundingDocument.CreateAttribute(attributeName);
		attribute.Value = attributeValue;
		containerNode.Attributes.Append(attribute);
	}

	private XmlNode GetResumeInfoNode(NodeGroup nodeGroup) {
		string zoningXPath = nodeGroup.ToFullBuildingXPath();
		return mapResumeDocument.SelectSingleNode(zoningXPath);
	}

	private XmlNode GetCustomInfoNode(NodeGroup nodeGroup) {
		string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";
		return mapCustomDocument.SelectSingleNode(xPath);
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

		string zoningXPath = nodeGroup.ToFullBuildingXPath();

		// Récupération du noeud XML d'information et récupération de l'attribut ciblé
		XmlNode infoNode = mapResumeDocument.SelectSingleNode (zoningXPath);
		res = infoNode.Attributes[attributeName];

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
		string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";
		XmlNode infoNode = mapCustomDocument.SelectSingleNode(xPath);
		res = infoNode.Attributes[attributeName];
		return res;
	}


	/// <summary>
	/// 	Met à jour la position des noeuds correspondant à un bâtiment 3D pour que la position des sommets de ce
	/// 	dernier corresponde à la position de ses noeuds.
	/// </summary>
	/// <param name="building">Bâtiment, de type GameObject, dont on veut mettre à jour les noeuds.</param>
	public void UpdateNodesPosition(GameObject building) {
		// Récupération du groupe de noeuds correspondant au bâtiment qui est aussi le noeud parent du noeud a trouver
		NodeGroup parentNodeGroup = this.BuildingToNodeGroup (building);

		// Récupération du groupe de noeuds 3D correspondant au bâtiment
		GameObject buildingNodeGroup = this.BuildingToBuildingNodeGroup (building);

		// Affectation de la position des noeuds 3D de bâtiments aux noeuds correspondants
		foreach(Transform buildingNodeTransform in buildingNodeGroup.transform) {
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
		if (nodeGroup != null) {
			Vector2 positionSum = Vector3.zero;
			foreach(Node node in nodeGroup.Nodes) {
				positionSum += node.ToVector();
			}
			return positionSum / ((nodeGroup.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un bâtiment 3D en faisant la moyenne de la position de ses murs.
	/// </summary>
	/// <returns>Centre du batiment.</returns>
	/// <param name="building">Batiment, de type GameObject, dont on veut calculer le centre.</param>
	public Vector3 BuildingCenter(GameObject building) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup(building);

		// Somme des coordonnées des murs de bâtiments
		if (nodeGroup != null) {
			Vector3 positionSum = Vector3.zero;
			GameObject firstWall = building.transform.GetChild (0).gameObject;
			for(int i = 0; i < nodeGroup.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)nodeGroup.Nodes [i];
				Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)firstWall.transform.position.y, (float)buildingNode.Latitude);
				positionSum += nodePosition;
			}

			return positionSum / ((nodeGroup.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un groupe de noeuds 3D d'un bâtiment en faisant la moyenne de la position
	/// 	de ces noeuds 3D.
	/// </summary>
	/// <returns>Centre du groupe de noeuds 3D.</returns>
	/// <param name="buildingNodeGroup">Groupe de noeuds 3D, de type GameObject, dont on veut calculer le centre.</param>
	/// <param name="NodeGroup">
	/// 	Groupe de noeuds, de type NodeGroup, correspondant au groupe de noeuds 3D.
	/// </param>
	public Vector3 BuildingNodesCenter(GameObject buildingNodeGroup, NodeGroup NodeGroup) {
		Vector3 positionSum = new Vector3 (0, 0, 0);

		// Somme des coordonnées des noeuds 3D de bâtiments
		for(int i = 0; i < NodeGroup.Nodes.Count - 1; i++) {
			Node buildingNode = (Node)NodeGroup.Nodes [i];
			Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)buildingNodeGroup.transform.position.y, (float)buildingNode.Latitude);
			positionSum += nodePosition;
		}

		return positionSum / ((NodeGroup.Nodes.Count - 1) * 1F);
	}


	/// <summary>
	/// 	Calcule et renvoie le "rayon" d'un bâtiment, c'est à dire la distance entre le sommet le plus éloigné du
	/// 	centre du bâtiment et ce dernier.
	/// </summary>
	/// <returns>Rayon du bâtiment.</returns>
	/// <param name="building">Bâtiment dont on veut calculer le rayon.</param>
	public double BuildingRadius(GameObject building) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup(building);

		// Calcul du centre du bâtiment
		Vector3 buildingCenter = this.BuildingCenter (building);

		// Calcul du rayon si le groupe de noeuds a bien été trouvé
		if (nodeGroup != null) {

			// Somme des distances de chaque sommet par rapport au centre avec mise à jour du maximum
			double maxDistance = 0;
			for(int i = 0; i < nodeGroup.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)nodeGroup.Nodes [i];
				Vector2 nodePosition = new Vector2 ((float)buildingNode.Longitude, (float)buildingNode.Latitude);
				Vector2 buildingCenter2D = new Vector2 (buildingCenter.x, buildingCenter.z);

				double currentDistance = Vector2.Distance (buildingCenter2D, nodePosition);
				if (currentDistance > maxDistance)
					maxDistance = currentDistance;
			}

			return maxDistance;
		} else {
			return -1;
		}
	}


	/// <summary>
	/// 	Ajout une entrée dans la table de correspondance entre les bâtiments 3D et leurs groupes de noeuds
	/// 	correspondant.
	/// </summary>
	/// <param name="building">Bâtiment 3D en temps que clé.</param>
	/// <param name="nodeGroup">Groupe de noeuds en temps que valeur.</param>
	public void AddBuildingAndNodeGroupPair(GameObject building, NodeGroup nodeGroup) {
		nodeGroupToBuildingTable [nodeGroup.Id] = building;
		buildingToNodeGroupTable [building] = nodeGroup.Id;
	}

	/// <summary>
	/// 	Ajout une entrée dans la table de correspondance entre les groupes de noeuds 3D et leurs groupes de noeuds
	/// 	correspondant.
	/// </summary>
	/// <param name="buildingNodeGroup">Groupe de noeuds 3D en temps que clé.</param>
	/// <param name="nodeGroup">Groupe de noeuds en temps que valeur.</param>
	public void AddBuildingNodeGroupAndNodeGroupPair(GameObject buildingNodeGroup, NodeGroup nodeGroup) {
		buildingNodeGroupToNodeGroupTable [buildingNodeGroup] = nodeGroup.Id;
		nodeGroupToBuildingNodeGroupTable [nodeGroup.Id] = buildingNodeGroup;
	}

	/// <summary>
	/// 	Ajout une entrée dans la table de correspondance entre les noeuds 3D et leurs noeuds correspondant.
	/// </summary>
	/// <param name="buildingNode">Noeuds 3D en temps que clé.</param>
	/// <param name="node">Noeud en temps que valeur.</param>
	public void AddBuildingNodeAndNodeEntryPair(GameObject buildingNode, Node node) {
		buildingNodeToNodeTable[buildingNode] = node.GeneratedId();
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
	public NodeGroup BuildingToNodeGroup(GameObject building) {
		string nodeGroupId = buildingToNodeGroupTable[building];
		return cityBuilder.GetNodeGroup(nodeGroupId);
	}

	/// <summary>
	/// 	Renvoie le bâtiment 3D correspondant à un groupe de noeuds grâce à une table de correspondance.
	/// </summary>
	/// <returns>Bâtiment 3D correspondant à un groupe de noeuds.</returns>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds dont on veut récupérer le bâtiment 3D correspondant.
	/// </param>
	public GameObject NodeGroupToBuilding(NodeGroup nodeGroup) {
		return nodeGroupToBuildingTable [nodeGroup.Id];
	}

	/// <summary>
	/// 	Renvoie le groupe de noeuds correspondant à un groupe de noeuds 3D grâce à une table de correspondance.
	/// </summary>
	/// <returns>Groupe de noeuds correspondant à un groupe de noeuds 3D.</returns>
	/// <param name="buildingNodeGroup">
	/// 	Groupe de noeuds 3D dont on veut récupérer le groupe de noeuds correspondant.
	/// </param>
	public NodeGroup BuildingNodeGroupToNodeGroup(GameObject buildingNodeGroup) {
		string nodeGroupId = buildingNodeGroupToNodeGroupTable [buildingNodeGroup];
		return cityBuilder.GetNodeGroup(nodeGroupId);
	}

	/// <summary>
	/// 	Renvoie le groupe de noeuds 3D correspondant à un groupe de noeuds grâce à une table de correspondance.
	/// </summary>
	/// <returns>Groupe de noeuds 3D correspondant à un groupe de noeuds.</returns>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds dont on veut récupérer le groupe de noeuds 3D correspondant.
	/// </param>
	public GameObject NodeGroupToBuildingNodeGroup(NodeGroup nodeGroup) {
		return nodeGroupToBuildingNodeGroupTable [nodeGroup.Id];
	}

	/// <summary>
	/// 	Renvoie le noeud correspondant à un noeud 3D grâce à une table de correspondance.
	/// </summary>
	/// <returns>Noeud correspondant à un noeud 3D.</returns>
	/// <param name="buildingNode">
	/// 	Noeud 3D dont on veut récupérer le noeud correspondant.
	/// </param>
	public Node BuildingNodeToNode(GameObject buildingNode, NodeGroup parentNodeGroup) {
		string nodeId = buildingNodeToNodeTable[buildingNode];
		return parentNodeGroup.GetNode(nodeId);
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
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);
		GameObject buildingNodeGroup = this.NodeGroupToBuildingNodeGroup(nodeGroup);
		return buildingNodeGroup;
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

	public NodeGroup NewBasicNodeGroup(Vector3 centerPosition, Vector2 dimensions, Material material) {
		string newBuildingId = this.NewIdOrReference(10);

		NodeGroup nodeGroup = new NodeGroup(newBuildingId) {
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

		nodeGroup.AddNode(topLeftWallNode);
		nodeGroup.AddNode(topRightWallNode);
		nodeGroup.AddNode(bottomRightWallNode);
		nodeGroup.AddNode(bottomLeftWallNode);
		nodeGroup.AddNode(topLeftDupliWallNode);

		return nodeGroup;
	}

	private Node NewWallNode(int index, Vector3 buildingCenter, Vector2 localPosition) {
		string newNodeReference = this.NewIdOrReference(10);
		return new Node(newNodeReference, index, buildingCenter.z + localPosition.x, buildingCenter.x + localPosition.y);
	}

	private string NewIdOrReference(int nbDigits) {
		string res = "+";
		System.Random randomGenerator = new System.Random();
		for (int i = 0; i < nbDigits; i++)
			res += System.Math.Floor((double) randomGenerator.Next(0, 10));
		return res;
	}

	public static class BuildingsToolsInstanceHolder {
		internal static BuildingsTools instance = new BuildingsTools();
	}
}
