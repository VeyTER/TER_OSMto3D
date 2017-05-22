using System;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 	Suite d'outils en rapport avec la modification des bâtiments. Certaines des méthodes vont modifier le bâtiment
/// 	dans les fichiers de données.
/// </summary>
public class BuildingsTools {
	/// <summary>
	/// 	Unique instance du singleton ObjectBuilder, servant à construire la ville en 3D à partir des données OSM.
	/// </summary>
	private ObjectBuilder objectBuilder;


	/// <summary>Chemin vers le fichier résumant la carte OSM.</summary>
	private string resumeFilePath;

	/// <summary>Document XML représentant le résumé de la carte OSM.</summary>
	private XmlDocument mapResumeDocument;


	/// <summary>Chemin vers le fichier contenant les bâtiments personnalisés la carte OSM.</summary>
	private string customFilePath;

	/// <summary>Document XML représentant les bâtiments personnalisés la carte OSM.</summary>
	private XmlDocument mapCustomDocument;


	private BuildingsTools () {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.resumeFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		this.mapResumeDocument = new XmlDocument();

		this.customFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";
		this.mapCustomDocument = new XmlDocument ();
	}

	public static BuildingsTools GetInstance() {
		return BuildingsToolsInstanceHolder.instance;
	}


	/// <summary>
	/// 	Enlève la surcouche de couleur de tous les bâtiments.
	/// </summary>
	public void DiscolorAll() {
		// Suppression de tous les autres matériaux que que celui de base pour chaque mur
		foreach (Transform currentBuildingGo in objectBuilder.WallGroups.transform) {
			foreach (Transform currentWallGo in currentBuildingGo.transform) {
				Renderer meshRenderer = currentWallGo.GetComponent<Renderer> ();

				// Ecrasement du stock de matériaux du mur avec un nouveau stock contenant uniquement son premier
				// matériau s'il contient bien au moins un matériau
				if (meshRenderer.materials.Length != 1) {
					Material firstMaterial = meshRenderer.materials [0];
					meshRenderer.materials = new Material[] {
						firstMaterial
					};
				}
			}
		}
	}


	/// <summary>
	/// 	Ajoute une couche de couleur à un bâtiment pour le marquer comme sélectionné.
	/// </summary>
	/// <param name="buildingGo">Bâtiment à colorier.</param>
	public void ColorAsSelected(GameObject buildingGo) {
		Material wallMaterial = Resources.Load (Materials.WALL) as Material;
		Material selectedElementMaterial = Resources.Load (Materials.SELECTED_ELEMENT) as Material;

		// Supperposition du matériau de base avec celui de la couleur de sélection pour le bâtiment sélectionné
		foreach (Transform wallGo in buildingGo.transform) {
			Renderer meshRenderer = wallGo.GetComponent<Renderer> ();
			if (meshRenderer != null) {

				// Ecrasement de stock de matériaux du mur avec un nouveau stock contenant son matériau de base et le
				// matériau de sélection
				meshRenderer.materials = new Material[] {
					wallMaterial,
					selectedElementMaterial
				};
			}
		}
	}


	/// <summary>
	/// 	Change le nom d'un bâtiment dans l'application et dans les fichiers MapResumed et MapCustom.
	/// </summary>
	/// <param name="buildingGo">Bâtiment à renommer.</param>
	/// <param name="newName">Nouveau nom à donner au bâtiment.</param>
	public void UpdateName(GameObject buildingGo) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);
			
			//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
			if (!this.CustomBuildingExists (buildingNgp.Id))
				this.AppendCustomBuilding (buildingNgp);

			// Récupération de l'attribut de nom du bâtiment dans les fichiers MapResumed et MapCustom
			XmlAttribute resumeNameAttribute = this.ResumeNodeGroupAttribute (buildingNgp, XmlAttributes.NAME);
			XmlAttribute customNameAttribute = this.CustomNodeGroupAttribute (buildingNgp.Id, XmlAttributes.NAME);

			// Changement du nom du groupe de noeuds correspondant au bâtiment
			buildingNgp.Name = buildingGo.name;

			// Changement du nom des attributs de nom dans les fichiers
			resumeNameAttribute.Value = buildingGo.name;
			customNameAttribute.Value = buildingGo.name;

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}
	}


	/// <summary>
	/// 	Met à jour la position des noeuds d'un bâtiment dans l'application et dans les fichiers MapResumed
	/// 	et MapCustom.
	/// </summary>
	/// <param name="buildingGo">Bâtiment à déplacer.</param>
	public void UpdateLocation(GameObject buildingGo) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);

			//  S'il n'y a pas encore d'entrée pour le bâtiment dans le fichier map_custom, l'ajouter
			if (!this.CustomBuildingExists (buildingNgp.Id))
				this.AppendCustomBuilding (buildingNgp);
			
			// Récupération des noeuds correspondant au bâtiment dans les fichiers MapResumed et MapCustom
			string xPath = "/" + XmlTags.EARTH + "//" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + buildingNgp.Id + "\"]";
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
			foreach(Node node in buildingNgp.Nodes) {
				// Recherche du nd correspondant au noeud courant à partir le la référence et de l'index du noeud.
				// En effet, deux nd peuvent avoir la même référence à l'interieur d'un même noeud dans un fichier
				// (pour faire boucler les bâtiments), c'est pourquoi, un index a été ajouté pour cibler précisément le
				// nd voulu
				int i = 1;
				for (; i < resumedBuildingNd.Count && (!resumedBuildingNd [i].Attributes [XmlAttributes.REFERENCE].Value.Equals (node.Reference.ToString ())
															|| !resumedBuildingNd [i].Attributes [XmlAttributes.INDEX].Value.Equals (node.Index.ToString ())); i++);

				// Mise à jour de la position du nd s'il a été trouvé
				if (i < resumedBuildingNd.Count) {
					// Mise à jour de la position
					resumedBuildingNd [i].Attributes [XmlAttributes.LATITUDE].Value = (node.Latitude / Main.SCALE_FACTOR).ToString();
					resumedBuildingNd [i].Attributes [XmlAttributes.LONGIUDE].Value = (node.Longitude / Main.SCALE_FACTOR).ToString();

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
	/// 	Ajoute une entrée représentant un bâtiment dans le fichier map_custom.
	/// </summary>
	/// <param name="nodeGroup">Groupe de noeuds correspondant au bâtiment.</param>
	public void AppendCustomBuilding(NodeGroup nodeGroup) {
		if (File.Exists (customFilePath)) {
			// Récupération du noeud XML englobant tous les objets terrestres et ajout de celui-ci au fichier MapCutom
			// s'il n'est pas présent
			XmlNodeList earthNodes = mapCustomDocument.GetElementsByTagName (XmlTags.EARTH);
			if (earthNodes.Count == 0) 
				mapCustomDocument.AppendChild (mapCustomDocument.CreateElement (XmlTags.EARTH));

			// Création d'un noeud XML d'information pour le noeud XML du bâtiment
			XmlNode earthNode = earthNodes [0];
			XmlNode buildingNode = mapCustomDocument.CreateElement (XmlTags.BUILDING);
			XmlNode buildingInfoNode = mapCustomDocument.CreateElement (XmlTags.INFO);

			// Ajout du noeud XML d'information dans le fichier map_custom
			earthNode.AppendChild (buildingNode);
			buildingNode.AppendChild (buildingInfoNode);

			// Ajout des attributs et de la valeurs dans le noeud XML d'information
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ID, nodeGroup.Id.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.NAME, nodeGroup.Name);
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);
		}
	}


	/// <summary>
	/// 	Indique si une entrée correspondant à un certain bâtiment existe dans le fichier map_custom.
	/// </summary>
	/// <returns><c>true</c>, si l'entrée existe, <c>false</c> sinon.</returns>
	/// <param name="buildingId">ID du bâtiment dont on veut vérifier l'existance.</param>
	public bool CustomBuildingExists(long nodeGroupId) {
		if (File.Exists (customFilePath)) {
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroupId + "\"]";
			return mapCustomDocument.SelectSingleNode (xPath) != null;
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
		XmlAttribute attribute = boundingDocument.CreateAttribute (attributeName);
		attribute.Value = attributeValue;
		containerNode.Attributes.Append (attribute);
	}


	/// <summary>
	/// 	Récupère en revoie la hauteur d'un bâtiment à partir de son entrée dans le fichier map_resumedd.
	/// </summary>
	/// <returns>Hauteur du bâtiment en nombre d'étages.</returns>
	/// <param name="building">Bâtiment dont on veut connaître la hauteur.</param>
	public int GetHeight(GameObject building) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);
		XmlAttribute floorAttribute = this.ResumeNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
		if (floorAttribute != null)
			return int.Parse(floorAttribute.Value);
		else
			return -1;
	}


	/// <summary>
/// 	Diminue la hauteur d'un bâtiment d'un étage à la fois au niveau du groupe de noeuds correspondant, de la 
/// 	scène 3D et dans les fichiers MapResumed et MapCustom. 
	/// </summary>
	/// <param name="buildingGo">Bâtiment à modifier.</param>
	public void DecrementHeight(GameObject buildingGo) {
		int nbFloors = GetHeight(buildingGo);
		this.UpdateHeight (buildingGo, nbFloors - 1);
	}

	/// <summary>
/// 	Augmente la hauteur d'un bâtiment d'un étage à la fois au niveau du groupe de noeuds correspondant, de la 
/// 	scène 3D et dans les fichiers MapResumed et MapCustom. 
	/// </summary>
	/// <param name="buildingGo">Bâtiment à modifier.</param>
	public void IncrementHeight(GameObject buildingGo) {
		int nbFloors = GetHeight(buildingGo);
		this.UpdateHeight (buildingGo, nbFloors + 1);
	}


	/// <summary>
/// 	Change la hauteur d'un bâtiment d'un étage à la fois au niveau du groupe de noeuds correspondant, de la 
/// 	scène 3D et dans les fichiers MapResumed et MapCustom. 
	/// </summary>
	/// <param name="buildingGo">Bâtiment à modifier.</param>
	public void UpdateHeight(GameObject building, int nbFloors) {
		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup buildingNgp = this.BuildingToNodeGroup (building);

		// Modification de l'attribut contenant le nombre d'étages du bâtiment dans le fichier map_resumedd
		XmlAttribute floorAttribute = this.ResumeNodeGroupAttribute (buildingNgp, XmlAttributes.NB_FLOOR);
		if (floorAttribute != null)
			floorAttribute.Value = Math.Max(nbFloors, 1).ToString();

		mapResumeDocument.Save (resumeFilePath);

		// Modification de la hauteur du bâtiment 3D
		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance();
		objectBuilder.EditUniqueBuilding (building, nbFloors);
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
		// Construction de d'adresse du noeud XML d'information du bâtiment à partir de son zonage
		string zoningXPath = "";
		zoningXPath += "/" + XmlTags.EARTH;
		zoningXPath += "/" + XmlTags.COUNTRY + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
		zoningXPath += "/" + XmlTags.REGION + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
		zoningXPath += "/" + XmlTags.TOWN + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
		zoningXPath += "/" + XmlTags.DISTRICT + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";
		zoningXPath += "/" + XmlTags.BUILDING;
		zoningXPath += "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";

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
	private XmlAttribute CustomNodeGroupAttribute(long nodeGroupeId, string attributeName) {
		XmlAttribute res = null;
		string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroupeId + "\"]";
		XmlNode infoNode = mapCustomDocument.SelectSingleNode(xPath);
		res = infoNode.Attributes[attributeName];
		return res;
	}


	/// <summary>
	/// 	Renvoie le bâtiment de type GameObject correspondant à un groupe de noeuds grâce à l'ID de ce dernier et à
	/// 	une table d'ID.
	/// </summary>
	/// <returns>Objet 3D correspondant au groupe de noeuds.</returns>
	/// <param name="buildingGoId">Groupe de noeuds de type NodeGroup.</param>
	public GameObject NodeGroupToBuilding(NodeGroup nodeGroup) {
		if (objectBuilder.BuildingIdTable [nodeGroup.Id].GetType() == typeof(int)) {
			// Récupération de l'ID du bâtiment 3D correspondant à l'ID du groupe de noeuds
			int buildingGoId = (int)objectBuilder.BuildingIdTable [nodeGroup.Id];

			int nbBuildings = objectBuilder.WallGroups.transform.childCount;
			
			// Recherche du bâtiment 3D correspondant à partir de l'ID récupéré
			int i = 0;
			for (; i < nbBuildings && objectBuilder.WallGroups.transform.GetChild (i).GetInstanceID () != buildingGoId; i++);

			// Retour du bâtiment 3D s'il a été trouvé
			if (i < nbBuildings)
				return objectBuilder.WallGroups.transform.GetChild (i).gameObject;
			else
				return null;
		} else {
			return null;
		}
	}


	/// <summary>
	/// 	Renvoie le groupe de noeuds de type NodeGroup correspondant à un bâtiment de type GameObject grâce à l'ID
	/// 	de ce dernier et à une table d'ID.
	/// </summary>
	/// <returns>Groupe de noeuds correspondant au bâtiment.</returns>
	/// <param name="buildingGoId">Bâtiment 3D de type GameObject.</param>
	public NodeGroup BuildingToNodeGroup(GameObject buildingGo) {
		return this.ObjectToNodeGroup (buildingGo, objectBuilder.BuildingIdTable);
	}

	/// <summary>
	/// 	Renvoie le groupe de noeuds de type NodeGroup correspondant à groupe de noeuds 3D de type  GameObject grâce
	/// 	à l'ID de ce dernier et à une table d'ID.
	/// </summary>
	/// <returns>Groupe de noeuds correspondant au bâtiment.</returns>
	/// <param name="buildingNodeGroupGo">Groupe de noeuds 3D de type GameObject</param>
	public NodeGroup BuildingNodeGroupToNodeGroup(GameObject buildingNodeGroupGo) {
		return this.ObjectToNodeGroup (buildingNodeGroupGo, objectBuilder.BuildingNodeGroupsIdTable);
	}


	/// <summary>
	/// 	Renvoie le groupe de noeuds de type NodeGroup correspondant au bâtiment ou groupe de noeuds 3D de type
	/// 	GameObject grâce à l'ID de ce dernier et à une table d'ID.
	/// </summary>
	/// <returns>Groupe de noeuds correspondant au bâtiment.</returns>
	/// <param name="buildingNodeGroupGo">Bâtiment ou groupe de noeuds 3D de type GameObject</param>
	public NodeGroup ObjectToNodeGroup(GameObject gameObject, Dictionary<long, int> idTable) {
		NodeGroup res = null;

		// Recherche du groupe de noeuds correspondant au groupe de noeuds 3D ou au bâtiment 3D en entrée
		// en se servant de chaque association entre l'ID du groupe de noeuds et l'ID de l'objet correspondant
		foreach (KeyValuePair<long, int> objectEntry in idTable) {
			// Récupération des ID
			long nodeGroupId = objectEntry.Key;
			int currentGameObjectId = objectEntry.Value;

			// Recherche et renvoi du bon groupe de noeuds si l'ID de bâtiment 3D récupéré correspond à l'ID du
			// bâtiment en entrée
			if (currentGameObjectId == gameObject.transform.GetInstanceID()) {
				int nbNodeGroup = objectBuilder.NodeGroups.Count;

				// Recherche du groupe de noeud correspondant au bâtiment trouvé
				int i = 0;
				for (; i < nbNodeGroup && ((NodeGroup)objectBuilder.NodeGroups[i]).Id != nodeGroupId; i++);

				// Retour du groupe de noeuds s'il a été trouvé
				if (i < nbNodeGroup)
					res = ((NodeGroup)objectBuilder.NodeGroups[i]);
				else
					res = null;
				
				break;
			}
		}

		return res;
	}


	/// <summary>
	/// 	Renvoie le noeud correspondant à l'un des noeuds 3D d'un bâtiment grâce à l'ID de ce dernier et à une
	/// 	table d'ID.
	/// </summary>
	/// <returns>Noeud correspondant au noeud 3D.</returns>
	/// <param name="buildingNodeGo">Noeud 3D de type Gameobject.</param>
	public Node BuildingNodeToNode(GameObject buildingNodeGo) {
		NodeGroup parentNodeGoup = this.BuildingNodeGroupToNodeGroup (buildingNodeGo.transform.parent.gameObject);
		return this.BuildingNodeToNode (buildingNodeGo, parentNodeGoup);
	}


	/// <summary>
	/// 	Renvoie le noeud correspondant à l'un des noeuds 3D d'un bâtiment grâce à l'ID de ce dernier, au groupe de
	/// 	noeuds correspondant au bâtiment 3D contenant les noeuds d'un bâtiment 3D et à une table d'ID.
	/// </summary>
	/// <returns>Noeud correspondant au noeud 3D.</returns>
	/// <param name="buildingNodeGo">Noeud 3D de type Gameobject.</param>
	public Node BuildingNodeToNode(GameObject buildingNodeGo, NodeGroup parentNodeGoup) {
		Node res = null;

		// Recherche du noeud correspondant au noeud 3D en entrée en se servant de chaque association entre l'ID du
		// noeud et l'ID du noeud 3D
		foreach (KeyValuePair<string, int> objectEntry in objectBuilder.BuildingNodesIdTable) {
			string nodeId = objectEntry.Key;
			int currentGameObjectId = objectEntry.Value;

			// Récupération de la référence et de l'index du noeud courant par décomposition de son ID dans la table
			string[] nodeIdArraySplit = nodeId.Split ('|');
			long nodeReference = long.Parse (nodeIdArraySplit[0]);
			int nodeIndex = int.Parse (nodeIdArraySplit[1]);

			// Recherche et renvoi du bon noeud si l'ID de noeud récupéré correspond à l'ID du noeud 3D en entrée
			if (currentGameObjectId == buildingNodeGo.transform.GetInstanceID()) {
				int nbNode = parentNodeGoup.Nodes.Count;

				// Recherche du noeud correspondant au noeud 3D trouvé
				int i = 0;
				for (; i < nbNode && (((Node)parentNodeGoup.GetNode(i)).Reference != nodeReference || ((Node)parentNodeGoup.GetNode(i)).Index != nodeIndex); i++);

				// Retour du noeud s'il a été trouvé
				if (i < nbNode)
					res = ((Node)parentNodeGoup.GetNode (i));
				else
					res = null;

				break;
			}
		}

		return res;
	}

	/// <summary>
	/// 	Renvoie le Groupe de noeuds 3D correspondant à un bâtiment 3D grâce à l'ID de ce dernier et à une table
	/// 	d'ID.
	/// </summary>
	/// <returns>Groupe de noeuds 3D correspondant à un bâtiment 3D.</returns>
	/// <param name="buildingNodeGo">
	/// 	Bâtiment 3D, de typeGameObject, dont on veut récupérer le noeud 3D correspondant.
	/// </param>
	public GameObject BuildingToBuildingNodeGroup(GameObject buildingGo) {
		// [GameObject]-building  =>  [NodeGroup]-buildingNodeGroup
		NodeGroup buildingNodeGroup = this.BuildingToNodeGroup (buildingGo);

		// 							  [NodeGroup]-buildingNodeGroup ID  =>  [GameObject]-BuildingNode ID
		int buildingNodeGroupId = (int)objectBuilder.BuildingNodeGroupsIdTable[buildingNodeGroup.Id];

		// 							 										[GameObject]-BuildingNode ID  =>  [GameObject]-BuildingNode
		Transform buildingNodesTransform = objectBuilder.BuildingNodes.transform;

		// Recherche du groupe de noeuds 3D correspondant au groupe de noeuds 3D trouvé
		int i = 0;
		for (; i < buildingNodesTransform.childCount && buildingNodesTransform.GetChild(i).GetInstanceID() != buildingNodeGroupId; i++);

		// Retourne le groupe de noeuds 3D s'il a été trouvé
		if (i < buildingNodesTransform.childCount)
			return buildingNodesTransform.GetChild (i).gameObject;
		else
			return null;
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
	/// <param name="buildingGo">Batiment, de type GameObject, dont on veut calculer le centre.</param>
	public Vector3 BuildingCenter(GameObject buildingGo) {
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		// Somme des coordonnées des murs de bâtiments
		if (buildingNgp != null) {
			Vector3 positionSum = Vector3.zero;
			GameObject firstWall = buildingGo.transform.GetChild (0).gameObject;
			for(int i = 0; i < buildingNgp.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)buildingNgp.Nodes [i];
				Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)firstWall.transform.position.y, (float)buildingNode.Latitude);
				positionSum += nodePosition;
			}

			return positionSum / ((buildingNgp.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}


	/// <summary>
	/// 	Calcule et renvoie le centre d'un groupe de noeuds 3D d'un bâtiment en faisant la moyenne de la position
	/// 	de ces noeuds 3D.
	/// </summary>
	/// <returns>Centre du groupe de noeuds 3D.</returns>
	/// <param name="buildingGo">Groupe de noeuds 3D, de type GameObject, dont on veut calculer le centre.</param>
	/// <param name="buildingNodeGroup">
	/// 	Groupe de noeuds, de type NodeGroup, correspondant au groupe de noeuds 3D.
	/// </param>
	public Vector3 BuildingNodesCenter(GameObject buildingNodeGroupGo, NodeGroup buildingNodeGroup) {
		Vector3 positionSum = new Vector3 (0, 0, 0);

		// Somme des coordonnées des noeuds 3D de bâtiments
		for(int i = 0; i < buildingNodeGroup.Nodes.Count - 1; i++) {
			Node buildingNode = (Node)buildingNodeGroup.Nodes [i];
			Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)buildingNodeGroupGo.transform.position.y, (float)buildingNode.Latitude);
			positionSum += nodePosition;
		}

		return positionSum / ((buildingNodeGroup.Nodes.Count - 1) * 1F);
	}


	/// <summary>
	/// 	Calcule et renvoie le "rayon" d'un bâtiment, c'est à dire la distance entre le sommet le plus éloigné du
	/// 	centre du bâtiment et ce dernier.
	/// </summary>
	/// <returns>Rayon du bâtiment.</returns>
	/// <param name="buildingGo">Bâtiment dont on veut calculer le rayon.</param>
	public double BuildingRadius(GameObject buildingGo) {
		// Calcul du centre du bâtiment
		Vector3 buildingCenter = this.BuildingCenter (buildingGo);

		// Récupération du groupe de noeuds correspondant au bâtiment
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		// Calcul du rayon si le groupe de noeuds a bien été trouvé
		if (buildingNgp != null) {

			// Somme des distances de chaque sommet par rapport au centre avec mise à jour du maximum
			double maxDistance = 0;
			for(int i = 0; i < buildingNgp.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)buildingNgp.Nodes [i];
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


	public static class BuildingsToolsInstanceHolder {
		public static BuildingsTools instance = new BuildingsTools();
	}
}
