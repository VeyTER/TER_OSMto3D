using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class ResumeFileManager : OsmFileManager {
	/// <summary>
	///     Génère un fichier de données (MapResumed) contenant les objets de la ville rangés dans les zones auxquelles
	///     ils appartiennent. Contrairement au fichier OSM, les sous-noeuds d'objets sont maintenant contenus à
	/// 	l'intérieur des noeuds XML représentant ces objets. Les objets et leurs sous-noeuds XML conservent néanmoins
	/// 	leurs identifiants (ID pour les objets et référence pour les sous-noeuds XML) d'origine, même si les
	/// 	sous-noeuds se sont vu ajouter un index pour les identifier au sein de l'objet auquel ils appartiennent.
	/// 	Enfin, les bâtiments et les routes (et seulement eux pour l'instant), ont dans ce fichier les
	/// 	caractéristiques par défaut de leur zone.
	/// </summary>
	public void GenerateResumeFile() {
		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";

		XmlDocument mapSettingsDocument = new XmlDocument();
		XmlDocument mapResumedDocument = new XmlDocument();

		if (File.Exists(mapSettingsFilePath)) {
			mapSettingsDocument.Load(mapSettingsFilePath);

			// Création de l'en-tête du fichier résumé
			XmlDeclaration mapResumedDeclaration = mapResumedDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			mapResumedDocument.AppendChild(mapResumedDeclaration);

			// Transfert des noeuds XML de zone dans le fichier résumé en retirant les noeuds XML d'information
			this.TransfertSettingsToResumed(mapResumedDocument, mapSettingsDocument, mapResumedDocument);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapResumedDocument.ChildNodes[1];

			if (earthNode != null) {
				// Création et ajout d'un nouveau noeud XML contenant les coordonnées minimales et maximales de la carte
				XmlNode boundsNode = osmTools.NewBoundsNode(mapResumedDocument, nodeGroupBase.GetBounds());
				earthNode.InsertBefore(boundsNode, earthNode.FirstChild);

				// Ajout d'un nouveau noeud XML pour chaque groupe de noeuds contenu dans l'application
				foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
					NodeGroup nodeGroup = nodeGroupEntry.Value;

					// Appel de la fonction récursive mettant à jour les attributs des groupes de noeuds pour le zones
					// indiquées dans le tableau de zones
					string zoningXPath = "/" + XmlTags.EARTH + osmTools.MatchingZoningPath(mapResumedDocument, earthNode, nodeGroup, areasMinimalList, 0, "/" + XmlTags.EARTH);
					XmlNode locationNode = mapResumedDocument.SelectSingleNode(zoningXPath);

					if (locationNode != null) {
						// Construction du chemin xPath vers le noeud XML correspondant à la zone la plus locale au groupe
						// de noeuds et récupération de de noeud
						string customObjectXPath = zoningXPath + "/" + XmlTags.BUILDING + "/" + nodeGroup.PrimaryType + "[@" + XmlAttributes.NAME + "=\"" + nodeGroup.Name + "\"]";
						XmlNode customObjectNode = mapResumedDocument.SelectSingleNode(customObjectXPath);

						XmlNode objectNode = null;
						XmlNode objectInfoNode = null;

						// Récupération du noeud XML d'information de l'objet personnalisé courant, soit en le
						// récupérant à partir du noeud XML correspondant à l'objet, soit en le créant si ce sernier
						// n'existe pas
						if (customObjectNode == null) {
							objectNode = mapResumedDocument.CreateElement(nodeGroup.PrimaryType);
							objectInfoNode = mapResumedDocument.CreateElement(XmlTags.INFO);
							objectNode.AppendChild(objectInfoNode);
						} else {
							objectNode = customObjectNode;
							objectInfoNode = customObjectNode.FirstChild;
						}

						// Ajout de l'ID de l'objet au noeud XML d'information correspondant au dernier
						osmTools.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.ID, nodeGroup.Id);

						// Ajout des sous-noeuds XM au noeud XML correspondnant au groupe de noeuds courant
						this.AddInternalStepNodes(mapResumedDocument, nodeGroup, objectNode);

						// Ajout des sous-noeuds XM au noeud XML correspondnant au groupe de noeuds courant
						this.AddComponentNodes(mapResumedDocument, nodeGroup, locationNode);

						// Ajout d'attributs propres à l'objet courant en fonction de son type, contenu dans le groupe
						// de noeuds courant
						if (nodeGroup.GetType() == typeof(BuildingNodeGroup))
							osmTools.AddBuildingNodeAttribute(mapResumedDocument, nodeGroup, objectInfoNode);
						else if (nodeGroup.GetType() == typeof(HighwayNodeGroup))
							osmTools.AddHighwayNodeAttribute(mapResumedDocument, nodeGroup, objectInfoNode);
						else if (nodeGroup.GetType() == typeof(LeisureNodeGroup))
							osmTools.AddLeisureNodeAttributeInfo(mapResumedDocument, nodeGroup, objectInfoNode);

						// Ajout du nouveau noeud XML créé, représentant l'objet, au neud XML correspondant à la zone à
						// laquelle il appartient
						locationNode.AppendChild(objectNode);
					}
				}

				// Suppression des noeuds de zonage inutiles car vides
				this.RemoveUnusedNodes(mapResumedDocument);

				mapResumedDocument.Save(mapResumedFilePath);
			}
		}
	}


	/// <summary>
	/// 	Transfère les noeuds XML de zones (autre que ceux d'information) depuis un document XML vers un autre.
	/// </summary>
	/// <param name="targetDocument">Document XML destinée à recevoir les noeuds XML.</param>
	/// <param name="sourceParentElement">
	/// 	Document XML ou noeud XML contenant les noeuds XML fils à transvaser.
	/// </param>
	/// <param name="targetParentElement">
	/// 	Document XML ou noeud XML destiné à recevoir les noeuds extraits depuis le document XML source.
	/// </param>
	private void TransfertSettingsToResumed(XmlDocument targetDocument, XmlNode sourceParentElement, XmlNode targetParentElement) {
		foreach (XmlNode mapSettingsNode in sourceParentElement) {
			XmlNode mapResumedNode = targetDocument.ImportNode(mapSettingsNode, true);
			if ((!mapSettingsNode.Name.Equals(XmlTags.INFO) || sourceParentElement.Name.Equals(XmlTags.BUILDING)) && !mapResumedNode.Name.Equals(XmlTags.XML)) {
				targetParentElement.AppendChild(mapResumedNode);
				mapResumedNode.InnerText = "";
				mapResumedNode.InnerXml = "";
				this.TransfertSettingsToResumed(targetDocument, mapSettingsNode, mapResumedNode);
			}
		}
	}


	/// <summary>
	/// 	Méthode récursive qui supprime tous les noeuds XML de zone vides pour alléger le fichier map_resumed en
	/// 	explorant récursivement toutes les zones.
	/// </summary>
	/// <param name="parentElement">Noeud XML ou document XML contenant les noeuds XML à explorer.param>
	private void RemoveUnusedNodes(XmlNode parentElement) {
		// Création de la liste des noeuds à supprimer pour ce niveau
		List<XmlNode> oldNodes = new List<XmlNode>();

		// Ajout des noeuds XML à la liste des noeuds XML à supprimer pour chaque noeud XML fils si ce dernier est vide
		// et que ce n'est pas un noeuds important ou d'information
		foreach (XmlNode dataNode in parentElement.ChildNodes) {
			if (!dataNode.Name.Equals(XmlTags.XML) && !dataNode.Name.Equals(XmlTags.BOUNDS)) {
				// Ajout du noeud XML à la liste de suppression d'il remplit les critères et appel récursif sinon
				if (dataNode.ChildNodes.Count > 0 && dataNode.FirstChild.Name.Equals(XmlTags.INFO)) {
					if (dataNode.ChildNodes.Count == 1) {
						oldNodes.Add(dataNode);
					}
				} else {
					this.RemoveUnusedNodes(dataNode);
				}
			}
		}

		// Suppression des noeuds à supprimer
		foreach (XmlNode oldNode in oldNodes)
			parentElement.RemoveChild(oldNode);
	}


	/// <summary>
	/// 	Ajoute des sous-noeuds XML au noeuds XML corredpondant à un groupe de noeuds. La méthode ajoute également
	/// 	des informations d'identification et de localisation aux sous-noeuds XML ajoutés.
	/// </summary>
	/// <param name="mapResumedDocument">Document dans lequel ajouter les sous-noeuds.</param>
	/// <param name="nodeGroup">Groupe de noeuds contenant les noeuds à retranscrire dans le document.</param>
	/// <param name="objectNode">Groupe de noeuds contenant les noeuds à retranscrire en sous-noeuds XML.</param>
	private void AddInternalStepNodes(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode) {
		// Parcours de tous les noeuds du groupe de noeuds pour les retranscrire en sous-noeuds XML
		for (int i = 0; i < nodeGroup.Nodes.Count; i++) {
			Node node = nodeGroup.Nodes[i];

			if (node is IStepNode) {
				// Affectation à l'index du nouveau noeud de la valeur de l'index servant à parcourir les noeuds du groupe
				// de noeuds
				node.Index = i;

				// Création d'un sous-noeud XML destiné à retranscrire le noeud courant
				XmlNode objectNd = mapResumedDocument.CreateElement(XmlTags.ND);
				objectNode.AppendChild(objectNd);

				// Ajout d'informations au nouveau sous-noeud XML
				osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.REFERENCE, node.Reference);
				osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.INDEX, node.Index.ToString());
				osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
				osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
			}
		}
	}

	private void AddComponentNodes(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode locationNode) {
		// Parcours de tous les noeuds du groupe de noeuds pour les retranscrire en sous-noeuds XML
		for (int i = 0; i < nodeGroup.Nodes.Count; i++) {
			Node node = nodeGroup.Nodes[i];

			if (node is IComponentNode) {
				// Affectation à l'index du nouveau noeud de la valeur de l'index servant à parcourir les noeuds du groupe
				// de noeuds
				node.Index = i;

				XmlNode objectNode = null;
				if (node.GetType() == typeof(HighwayComponentNode)) {
					if (((HighwayComponentNode) node).IsTrafficSignal())
						objectNode = mapResumedDocument.CreateElement(XmlTags.TRAFFIC_SIGNALS);
				} else if (node.GetType() == typeof(NaturalComponentNode)) {
					if (((NaturalComponentNode) node).IsTree())
						objectNode = mapResumedDocument.CreateElement(XmlTags.TREE);
				}

				if (objectNode != null) {
					XmlNode objectInfoNode = mapResumedDocument.CreateElement(XmlTags.INFO);
					osmTools.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.ID, Node.GenerateId(node));
					osmTools.AddCommonNodeAttribute(mapResumedDocument, nodeGroup, objectInfoNode);

					objectNode.AppendChild(objectInfoNode);

					XmlNode objectNd = mapResumedDocument.CreateElement(XmlTags.ND);
					objectNode.AppendChild(objectNd);

					// Création d'un sous-noeud XML destiné à retranscrire le noeud courant
					locationNode.AppendChild(objectNode);

					// Ajout d'informations au nouveau sous-noeud XML
					osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.REFERENCE, node.Reference);
					osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.INDEX, node.Index.ToString());
					osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
					osmTools.AppendAttribute(mapResumedDocument, objectNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
				}
			}
		}
	}

	/// <summary>
	/// 	Extrait les informations necessaires à la construction de la ville depuis le fichier map_resumed en appelant
	/// 	notamment la fonction récursive d'extraction d'objets.
	/// </summary>
	public void LoadResumedData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument();

		if (File.Exists(mapResumedFilePath)) {
			mapsSettingsDocument.Load(mapResumedFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];

			if (earthNode != null) {
				// Récupération des coordonnées minimales et maximales de la carte
				XmlNode boundsNode = earthNode.FirstChild;
				if (boundsNode != null) {
					double minLat = double.Parse(osmTools.AttributeValue(boundsNode, XmlAttributes.MIN_LATITUDE));
					double minLon = double.Parse(osmTools.AttributeValue(boundsNode, XmlAttributes.MIN_LONGITUDE));
					double maxLat = double.Parse(osmTools.AttributeValue(boundsNode, XmlAttributes.MAX_LATITUDE));
					double maxLon = double.Parse(osmTools.AttributeValue(boundsNode, XmlAttributes.MAX_LONGITUDE));
					nodeGroupBase.SetBounds(minLat, minLon, maxLat, maxLon);
				}

				// Construction d'un tableau contenant les différentes zones. Le niveau d'imbrication des zones est
				// directement lié à leur ordre dans le tableau (les régions étant comprises dans les pays par ex.)
				string[] areas = new string[4] {
					XmlTags.COUNTRY,
					XmlTags.REGION,
					XmlTags.TOWN,
					XmlTags.DISTRICT,
				};

				// Appel de la fonction résusive d'extraction des objets
				this.ExtractResumedNodes(earthNode, new string[4], areas, 0);
			}
		}
	}


	/// <summary>
	/// 	Fonction récursive qui ajoute à la liste de tous les groupes de noeuds de l'application les noeuds XML
	/// 	extraits depuis un noeud XML parent. La méthode va se positionner à chaque fois au niveau du noeud XML fils
	/// 	inférieur tant que ce dernier n'est pas un objet.
	/// </summary>
	/// <param name="parentAreaNode">Noeud XML parent (forcément un noeud de zone).</param>
	/// <param name="areaDesignations">
	/// 	Nom des zones empruntées lors de la navigation récursive dans les sous-noeuds XML.
	/// </param>
	/// <param name="areaTypes">Types de zones pouvant être rencontrée et classées par ordre de profondeur.</param>
	/// <param name="areaTypeIndex">Index utilisé par la méthode pour connaître sa profondeur dans le fichier.</param>
	private void ExtractResumedNodes(XmlNode parentAreaNode, string[] areaDesignations, string[] areaTypes, int areaTypeIndex) {
		// Poursuite du parcours récursif si les noeuds XML fils sont des noeuds XML de zones, extraction et stockage
		// des noeuds XML fils sinon

		foreach (XmlNode childNode in parentAreaNode.ChildNodes) {
			if (areaTypeIndex < areaTypes.Length && areaTypes[areaTypeIndex].Contains(childNode.Name)) {
				// Extraction du nom de la zone courante et ajout de celle-ci au chemin emprunté
				areaDesignations[areaTypeIndex] = osmTools.AttributeValue(childNode, XmlAttributes.DESIGNATION);

				// Appel récusrsif pour traiter les noeuds XML fils
				this.ExtractResumedNodes(childNode, areaDesignations, areaTypes, areaTypeIndex + 1);
			} else if (!childNode.Name.Equals(XmlTags.BOUNDS)) {
				// Récupération du noeud XML contenant les informations sur l'objet courant et récupération de l'ID de
				// cet objet
				XmlNode objectInfoNode = childNode.FirstChild;
				string id = osmTools.AttributeValue(objectInfoNode, XmlAttributes.ID);

				// Création et remplissage d'un nouveau noeud avec les identifiants et coordonnées extraits de chacun
				// des sous-noeuds XML du noeud XML courant
				List<Node> newNodes = new List<Node>();
				for (int i = 1; i < childNode.ChildNodes.Count; i++) {
					XmlNode ndNode = childNode.ChildNodes[i];

					// Extraction des données du sous-noeud courant
					string reference = osmTools.AttributeValue(ndNode, XmlAttributes.REFERENCE);
					int index = int.Parse(osmTools.AttributeValue(ndNode, XmlAttributes.INDEX));
					double latitude = double.Parse(osmTools.AttributeValue(ndNode, XmlAttributes.LATITUDE));
					double longitude = double.Parse(osmTools.AttributeValue(ndNode, XmlAttributes.LONGIUDE));

					// Création d'un nouveau noeud et ajout de clui-ci au groupe de noeuds correspondant à l'objet
					Node node = new Node(reference, index, latitude, longitude);
					newNodes.Add(node);
				}

				NodeGroup newNodeGroup = null;

				switch (childNode.Name) {
				case XmlTags.BUILDING:
					newNodeGroup = this.NewBuildingNodeGroup(id, objectInfoNode, newNodes);
					break;
				case XmlTags.HIGHWAY:
					newNodeGroup = this.NewHighwayNodeGroup(id, objectInfoNode, newNodes);
					break;
				case XmlTags.WATERWAY:
					newNodeGroup = this.NewWaterwayNodeGroup(id, objectInfoNode, newNodes);
					break;
				case XmlTags.LEISURE:
					newNodeGroup = this.NewLeisureNodeGroup(id, objectInfoNode, newNodes);
					break;
				case XmlTags.TREE:
					newNodeGroup = this.NewTreeNodeGroup(id, objectInfoNode, newNodes);
					break;
				case XmlTags.TRAFFIC_SIGNALS:
					newNodeGroup = this.NewTrafficSignalNodeGroup(id, objectInfoNode, newNodes);
					break;
				}

				if (newNodeGroup != null) {
					newNodeGroup.Name = osmTools.AttributeValue(objectInfoNode, XmlAttributes.NAME);
					newNodeGroup.Country = areaDesignations[0];
					newNodeGroup.Region = areaDesignations[1];
					newNodeGroup.Town = areaDesignations[2];
					newNodeGroup.District = areaDesignations[3];

					nodeGroupBase.AddNodeGroup(newNodeGroup);
				}
			}
		}
	}

	private BuildingNodeGroup NewBuildingNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		BuildingNodeGroup buildingNodeGroup = new BuildingNodeGroup(id, null);

		string[] areaBuildingInfo = osmTools.BuildingInfo(objectInfoNode);

		// Ajout des caractéristiques aux groupes de noeuds
		buildingNodeGroup.NbFloor = int.Parse(areaBuildingInfo[0]);
		buildingNodeGroup.RoofAngle = int.Parse(areaBuildingInfo[1]);
		buildingNodeGroup.RoofShape = areaBuildingInfo[2];

		if (areaBuildingInfo[3] != null) {
			Material customMaterial = Resources.Load<Material>(FilePaths.MATERIALS_FOLDER_LOCAL + areaBuildingInfo[3]);
			buildingNodeGroup.CustomMaterial = customMaterial;
		} else {
			buildingNodeGroup.CustomMaterial = Resources.Load<Material>(Materials.WALL_DEFAULT);
		}

		if (areaBuildingInfo[4] != null) {
			String[] colorDevices = areaBuildingInfo[4].Split(';');
			Color overlayColor = new Color(float.Parse(colorDevices[0]), float.Parse(colorDevices[1]), float.Parse(colorDevices[2]));
			buildingNodeGroup.OverlayColor = overlayColor;
		} else {
			buildingNodeGroup.OverlayColor = new Color(1, 1, 1);
		}

		this.FillNodeGroup(buildingNodeGroup, nodes, new KeyValuePair<string, string>(XmlTags.BUILDING, null), typeof(IStepNode));

		return buildingNodeGroup;
	}

	private HighwayNodeGroup NewHighwayNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		HighwayNodeGroup highwayNodeGroup = new HighwayNodeGroup(id, null);

		string[] areaHighwayInfo = osmTools.HighwayInfo(objectInfoNode);
		highwayNodeGroup.NbWay = int.Parse(areaHighwayInfo[1]);
		highwayNodeGroup.MaxSpeed = int.Parse(areaHighwayInfo[2]);

		this.FillNodeGroup(highwayNodeGroup, nodes, new KeyValuePair<string, string>(XmlTags.HIGHWAY, areaHighwayInfo[0]), typeof(IStepNode));

		return highwayNodeGroup;
	}

	private WaterwayNodeGroup NewWaterwayNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		WaterwayNodeGroup buildingNodeGroup = new WaterwayNodeGroup(id, null);
		this.FillNodeGroup(buildingNodeGroup, nodes, new KeyValuePair<string, string>(XmlTags.WATERWAY, null), typeof(IStepNode));
		return buildingNodeGroup;
	}

	private LeisureNodeGroup NewLeisureNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		LeisureNodeGroup leisureNodeGroup = new LeisureNodeGroup(id, null);

		string[] areaLeisureInfo = osmTools.LeisureInfo(objectInfoNode);
		this.FillNodeGroup(leisureNodeGroup, nodes, new KeyValuePair<string, string>("leisure", areaLeisureInfo[0]), typeof(IStepNode));

		return leisureNodeGroup;
	}

	private NaturalNodeGroup NewTreeNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		NaturalNodeGroup treeNodeGroup = new NaturalNodeGroup(id, null);

		this.FillNodeGroup(treeNodeGroup, nodes, new KeyValuePair<string, string>("natural", XmlValues.TREE), typeof(IComponentNode));
		nodes[0].AddTag("natural", XmlValues.TREE);

		return treeNodeGroup;
	}

	private HighwayNodeGroup NewTrafficSignalNodeGroup(string id, XmlNode objectInfoNode, List<Node> nodes) {
		HighwayNodeGroup trafficSignalNodeGroup = new HighwayNodeGroup(id, null);

		this.FillNodeGroup(trafficSignalNodeGroup, nodes, new KeyValuePair<string, string>("highway", XmlValues.TRAFFIC_SIGNALS), typeof(IComponentNode));
		nodes[0].AddTag("highway", XmlValues.TRAFFIC_SIGNALS);

		return trafficSignalNodeGroup;
	}

	private void FillNodeGroup(NodeGroup nodeGroup, List<Node> nodes, KeyValuePair<string, string> tag, Type nodesType) {
		foreach (Node node in nodes) {
			if (nodesType == typeof(IStepNode))
				nodeGroup.AddStepNode(node.Reference, node.Index, node.Latitude, node.Longitude, node.Tags);
			else if (nodesType == typeof(IComponentNode))
				nodeGroup.AddComponentNode(node.Reference, node.Index, node.Latitude, node.Longitude, node.Tags);
		}
		nodeGroup.AddTag(tag.Key, tag.Value);
	}
}