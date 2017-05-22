using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>
/// 	Suite d'outils destinés au chargement et au stockage des différents objects d'une carte OSM.
/// </summary>
public class MapLoader {
	/// <summary>
	/// 	Unique instance du singleton ObjectBuilder, servant à construire la ville en 3D à partir des données OSM.
	/// </summary>
	private ObjectBuilder objectBuilder;

	/// <summary>Latitude minimale de la ville.</summary>
	private double minLat;

	/// <summary>Longitude minimale de la ville.</summary>
	private double minLon;

	/// <summary>Latitude maximale de la ville.</summary>
	private double maxLat;

	/// <summary>Longitude maximale de la ville.</summary>
	private double maxLon;

	private MapLoader() {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.minLat = 0;
		this.minLon = 0;
		this.maxLat = 0;
		this.maxLon = 0;
	}

	public static MapLoader GetInstance() {
		return MapLoaderHolder.instance;
	}


	/// <summary>
	///		Extrait les données du fichier OSM contenant la carte les stocke dans des noeuds eux-même contenus dans des
	/// 	groupes de noeuds.
	/// </summary>
	/// <param name="mapName">Nom du fichier OSM contenant la carte à charger.</param>
	public void LoadOsmData(string mapName) {
		string OSMFilePath = mapName;
		XmlDocument OSMDocument = new XmlDocument(); 

		if (File.Exists (OSMFilePath)) {
			OSMDocument.Load (OSMFilePath);

			// Récupération des coordonnées minimales et maximales de la carte
			XmlNodeList boundsNodes = OSMDocument.GetElementsByTagName (XmlTags.BOUNDS);
			if (boundsNodes.Count > 0) {
				minLat = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MIN_LATITUDE));
				minLon = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MIN_LONGITUDE));
				maxLat = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MAX_LATITUDE));
				maxLon = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MAX_LONGITUDE));
			}

			// Extraction et stockage des noeuds décrivant les objets de la ville
			List<Node> extractedNodes = this.ExtractUnfoldedNodes (OSMDocument);
			this.FoldExtractedNodes (OSMDocument, extractedNodes);
		}
	}


	/// <summary>
	/// 	Extrait tous les sous-noeuds XML, qui contiennent des infos sur les noeuds internes des objets simples
	/// 	(un seul noeud) et des objets complexes (plusieurs noeuds). Dans le cas d'objets complexes, les noeuds
	/// 	extraits seront destinés à être rattachés à des noeuds XML (nommées "nd"), comme leurs coordonées.
	/// </summary>
	/// <returns>Noeuds extraits.</returns>
	/// <param name="OSMDocument">Document OSM contenant la carte.</param>
	private List<Node> ExtractUnfoldedNodes(XmlDocument OSMDocument) {
		// Liste des noeuds extraits du fichier OSM
		List<Node> extractedNodes = new List<Node>();

		// Parcours de tous les sous-noeuds XML pour les extraitre et, éventuellement, les rattacher à des
		// objets simples
		XmlNodeList nodeNodes = OSMDocument.GetElementsByTagName (XmlTags.NODE);
		foreach (XmlNode nodeNode in nodeNodes) {
			// Récupération de l'ID et des coordonnées 
			long id = long.Parse (this.AttributeValue (nodeNode, XmlAttributes.ID));
			double latitude = double.Parse(this.AttributeValue (nodeNode, XmlAttributes.LATITUDE));
			double longitude = double.Parse(this.AttributeValue (nodeNode, XmlAttributes.LONGIUDE));

			// Récupération des propriétés sur l'objet en cours de récupération
			XmlNodeList tagNodes = nodeNode.ChildNodes;
			for (int i = 0; i < tagNodes.Count; i++) {
				if(tagNodes [i].Name.Equals(XmlTags.TAG)) {
					XmlNode tagNode = tagNodes [i];

					// Récupération du nom et de la valeur de propriété
					string key = this.AttributeValue (tagNode, XmlAttributes.KEY);
					string value = this.AttributeValue (tagNode, XmlAttributes.VALUE);

					// Si l'objet est un objet naturel ou une voie de route, l'objet est complet, car ne contenant qu'un
					// seul noeud, on peut alors directement créer un groupe de noeuds et y insérer le noeud courant.
					if (key.Equals (XmlKeys.NATURAL) || key.Equals (XmlKeys.HIGHWAY)) {
						// Création et paramétrage du groupe de noeuds correspondant à l'objet extrait
						NodeGroup nodeGroup = new NodeGroup (id);
						nodeGroup.AddTag (key, value);

						// Renommage de l'objet avec la valeur de la propriété si c'est un arbre ou un feu tricolore
						if (nodeGroup.IsTree() || nodeGroup.IsTrafficLight())
							nodeGroup.Name = value;

						// Ajout d'un noeud, correspondant à l'objet simple, au groupe de noeuds courant, et ajout de ce
						// dernier à la liste des groupes de noeuds de l'application
						nodeGroup.AddNode (new Node (id, latitude, longitude));
						objectBuilder.NodeGroups.Add (nodeGroup);
					}
				}
			}

			// Création et ajout d'un nouveau noeud courant à la liste des noeuds extraits
			extractedNodes.Add(new Node(id, latitude, longitude));
		}

		return extractedNodes;
	}


	/// <summary>
	/// 	Extrait toutes les noeuds XML d'objets et met en correspondances les balises internes avec les noeuds XML
	/// 	de sous-noeuds extraits précédemment grâce à la référence de ces derniers. Cette opération permet de
	/// 	savoir à la fois à quel objet complexe appartiennent les sous-noeuds XML et de récupérer les informations
	/// 	relatives à ce sous-noeud XML. Cette boucle va aussi chercher le noeud (objet Node) correspondant à chaque
	/// 	noeud de l'objet complexe courant pour le rattacher au groupe de noeuds correspondant à cet objet.
	/// </summary>
	/// <param name="OSMDocument">Document OSM contenant la carte.</param>
	/// <param name="extractedNodes">Sous-noeuds extraits du fichier OSM.</param>
	private void FoldExtractedNodes(XmlDocument OSMDocument, List<Node> extractedNodes) {
		// Rangement dand des noeuds XML des sous-noeuds XML extraits précédemment. Dans un même temps, stockage des
		// sous-noeuds XML extraits dans des groupes de noeuds
		XmlNodeList wayNodes = OSMDocument.GetElementsByTagName (XmlTags.WAY);
		foreach (XmlNode wayNode in wayNodes) {
			// Extraction de l'ID de l'objet complexe
			long id = long.Parse (this.AttributeValue (wayNode, XmlAttributes.ID));

			// Création, paramétrage et remplissage d'un nouveau groupe de noeuds avec les noeuds associés
			NodeGroup nodeGroup = new NodeGroup (id);
			for (int i = 0; i < wayNode.ChildNodes.Count; i++) {
				XmlNode ndNode = wayNode.ChildNodes [i];

				// Ajout du noeud après avoir trouvé sa correspondance avec un noeud déjà extrait si le noeuds XML
				// contient un sous-noeud XML, sinon, si la balise est un noeud contenant des propriétés sur l'objet,
				// stockage de ces propriétés dans le groupe de noeuds courant.
				if (ndNode.Name.Equals (XmlTags.ND)) {
					// Extraction de la référence du sous-noeud XML
					long reference = long.Parse (this.AttributeValue (ndNode, XmlAttributes.REFERENCE));

					// Recherche du noeud d'information correspondant au noeud déja extrait
					Node node = (Node)extractedNodes [0];
					for (int j = 0; j < extractedNodes.Count && node.Reference != reference; node = (Node)extractedNodes [j], j++);

					// Ajout du noeud correspondant au groupe de noeuds courant
					if (node != null)
						nodeGroup.AddNode (node);
				} else if (ndNode.Name.Equals (XmlTags.TAG)) {
					XmlNode tagNode = ndNode;

					// Récupération du nom et de la valeur de la propriété
					string key = this.AttributeValue (tagNode, XmlAttributes.KEY);
					string value = this.AttributeValue (tagNode, XmlAttributes.VALUE);

					// Changement du nom du groupe de noeuds courant si la propriété représente cette valeur
					if (key.Equals (XmlKeys.NAME))
						nodeGroup.Name = value;
					
					// Changement de du nombre de voies dans le groupe de noeuds courant si la propriété représente
					// cette valeur
					if (key.Equals (XmlValues.LANES))
						nodeGroup.NbWay = int.Parse (value);

					// Changement de la vitesse maximale dans le groupe de noeuds courant si la propriété représente
					// cette valeur
					if (key.Equals (XmlKeys.MAX_SPEED))
						nodeGroup.MaxSpeed = int.Parse (value);

					// Changement de la forme du toit dans le groupe de noeuds courant si la propriété représente
					// cette valeur
					if (key.Equals (XmlKeys.ROOF_SHAPE))
						nodeGroup.RoofType = value;

					// Ajout de la propriété et de sa valeur au groupe de noeuds courant
					nodeGroup.AddTag (key, value);
				}
			}

			// Ajout du groupe de noeuds à la liste globale s'il représente un bâtiment, une route ou une
			// voie maritime
			if ((nodeGroup.IsBuilding () || nodeGroup.IsHighway ()) || nodeGroup.IsWaterway ())
				objectBuilder.NodeGroups.Add (nodeGroup);
		}
	}


	/// <summary>
	/// 	Charge les données de zonage des bâtiment. Ces dernières permettent de changer les propriétés des bâtiments en
	/// 	fonction de la zone dans laquelle ils se trouvent.
	/// </summary>
	public void LoadSettingsData() {
		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument(); 

		if (File.Exists (mapSettingsFilePath)) {
			mapsSettingsDocument.Load (mapSettingsFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];

			// Mise à jour des groupes de noeuds en fonctions des paramètres extraits si le noeud XML existe bien
			if (earthNode != null) {

				// Récupération du noeud XML contenant des informations sur le noeud XML père
				XmlNode earthInfoNode = earthNode.FirstChild;

				// Mise à jour les groupes de noeuds si le noeud XML d'information existe bien
				if (earthInfoNode != null && earthInfoNode.Name.Equals (XmlTags.INFO)) {
					// Récupération des valeurs par défaut pour les bâtiments se trouvant sur Terre et changement de la
					// valeur des attributs correspondant dans les groupes de noeuds concernés
					string[] earthBuildingInfo = this.BuildingInfo (earthInfoNode);
					this.SetupAreaNodeGroups (null, new double[] {0, 0, 0}, earthBuildingInfo, XmlTags.EARTH);

					// Construction d'un tableau contenant les différentes zones. Le niveau d'imbrication des zones est
					// directement lié à leur ordre dans le tableau (les régions étant comprises dans les pays par ex.)
					string[] areas = new string[] {
						XmlTags.COUNTRY,
						XmlTags.REGION,
						XmlTags.TOWN,
						XmlTags.DISTRICT,
						XmlTags.BUILDING
					};

					// Appel de la fonction récursive mettant à jour les attributs des groupes de noeuds pour le zones
					// indiquées dans le tableau de zones
					this.UpdateNodeGroupsProperties (earthNode, areas, 0);
				}
			}
		}
	}


	/// <summary>
	/// 	Méthode récursive permettant d'affecter aux groupes de noeuds les caractéristiques propres à la zone dans
	/// 	lequel se trouve le bâtiment correspondant. La méthode se sert du tableau contenant les différentes zones et
	/// 	et son indice associé pour connaitre la "profondeur" à laquelle il se trouve dans le document.
	/// </summary>
	/// <param name="boundingAreaNode">Noeud XML de la zone courante.</param>
	/// <param name="areaTypes">Types de zones pouvant être rencontrée et classées par ordre de profondeur.</param>
	/// <param name="areaTypeIndex">Index utilisé par la méthode pour connaître sa profondeur dans le fichier.</param>
	private void UpdateNodeGroupsProperties(XmlNode boundingAreaNode, string[] areaTypes, int areaTypeIndex) {
		// Traitement pour tous les noeud XML fils
		for (int i = 1; i < boundingAreaNode.ChildNodes.Count; i++) {

			// Mise à jour des attributs des groupes de noeuds correspondant si le noeud XML fils est au niveau
			// inférieur à celui du noeud XML courant
			if (boundingAreaNode.ChildNodes[i].Name.Equals (areaTypes[areaTypeIndex])) {
				// Récupération des noeuds fils du noeud de zone englobant et extraction de la désignation de la
				// zone inférieure
				XmlNode areaNode = boundingAreaNode.ChildNodes [i];
				string areaDesignation = this.AttributeValue (areaNode, XmlAttributes.DESIGNATION);

				// Récupération du premier fils du noeud de zone inférieure, qui est le noeud contenant les
				// caractéristiques par défaut de la zone courante. 
				XmlNode areaInfoNode = areaNode.FirstChild;

				// Extraction de la localisation et des caractéristiques par défaut des bâtiment se trouvant dans la
				// zone inférieure
				double[] areaLocationInfo = this.AttributeLocationInfo (areaInfoNode);
				string[] areaBuildingInfo = this.BuildingInfo (areaInfoNode);

				// Affectation des caractéristiques par défaut aux groupes de noeuds correspondant aux bâtiments
				// se trouvant dans la zone inférieure
				this.SetupAreaNodeGroups (areaDesignation, areaLocationInfo, areaBuildingInfo, areaTypes[areaTypeIndex]);

				// Appel récursif si l'on ne se trouve pas à la profondeur maximale
				if (areaTypeIndex < areaTypes.Length)
					this.UpdateNodeGroupsProperties (areaNode, areaTypes, areaTypeIndex + 1);
			}
		}
	}


	/// <summary>
	/// 	Change les caractéristiques des groupes de noeud se trouvant dans une certaine zone avec les valeurs par
	/// 	défaut de cette zone.
	/// </summary>
	/// <param name="designation">Nom de la zone (ex : France, Toulouse etc...).</param>
	/// <param name="locationData">Données sur la localisation de la zone.</param>
	/// <param name="buildingData">Caractéristiques par défaut sur les bâtiments se trouvant dans la zone.</param>
	/// <param name="tagName">Type de zone.</param>
	private void SetupAreaNodeGroups(string designation, double[] locationData, string[] buildingData, string tagName) {
		foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
			// Calcul de la distance du bâtiment avec le centre de la zone
			double nodeGroupDistance = Math.Sqrt (Math.Pow (locationData [0] - (nodeGroup.GetNode (0).Latitude), 2) + Math.Pow (locationData [1] - (nodeGroup.GetNode (0).Longitude), 2));

			// Changement des caractésitiques si l'objet correspondant au groupe de noeuds courant est dans la zone
			if (tagName.Equals(XmlTags.EARTH) || nodeGroupDistance < locationData[2]) {
				switch (tagName) {
				case XmlTags.COUNTRY:
					nodeGroup.Country = designation;
					break;
				case XmlTags.REGION:
					nodeGroup.Region = designation;
					break;
				case XmlTags.TOWN:
					nodeGroup.Town = designation;
					break;
				case XmlTags.DISTRICT:
					nodeGroup.District = designation;
					break;
				}

				// Change les caractéristiques des données relatives aux bâtiments si le groupe de noeuds courant
				// représente un tel objet
				if (nodeGroup.IsBuilding ()) {
					nodeGroup.NbFloor = int.Parse (buildingData [0]);
					nodeGroup.RoofAngle = int.Parse (buildingData [1]);
					nodeGroup.RoofType = buildingData [2];
				}
			}
		}
	}


	/// <summary>
	/// 	Extrait les caractéristiques par défaut dans une certaine zone pour tous les bâtiments s'y trouvant.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques par défaut sur les bâtiments.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans les attributs du noeud XML.</param>
	private string[] BuildingInfo(XmlNode infoNode) {
		string[] res = new string[3];

		res [0] = this.AttributeValue(infoNode, XmlAttributes.NB_FLOOR);
		res [1] = this.AttributeValue (infoNode, XmlAttributes.ROOF_ANGLE);

		string roofType = this.AttributeValue (infoNode, XmlAttributes.ROOF_TYPE);
		res [2] = roofType.Equals("unknown") ? "" : roofType;

		return res;
	}


	/// <summary>
	/// 	Extrait les caractéristiques par défaut dans une certaine zone pour toutes les routes s'y trouvant.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques sur les routes.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans ses attributs.</param>
	private string[] HighwayInfo(XmlNode infoNode) {
		string[] res = new string[3];
		res [0] = this.AttributeValue (infoNode, XmlAttributes.ROAD_TYPE);
		res [1] = this.AttributeValue(infoNode, XmlAttributes.NB_WAY);
		res [2] = this.AttributeValue (infoNode, XmlAttributes.MAX_SPEED);
		return res;
	}


	/// <summary>
	/// 	Extrait les caractéristiques de localisation d'une certaine zone.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques de localisation.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans ses attributs.</param>
	private double[] AttributeLocationInfo(XmlNode infoNode) {
		double[] res = new double[3];
		res[0] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.LATITUDE));
		res[1] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.LONGIUDE));
		res[2] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.DISTANCE));
		return res;
	}


	/// <summary>
	/// 	Extrait la valeur d'un attribut identifié par son nom dans un certain noeud XML.
	/// </summary>
	/// <returns>Valeur extraite de l'attribut.</returns>
	/// <param name="containerNode">Noeud XML contenant l'attribut.</param>
	/// <param name="attributeName">Nom de l'attribut dont on veut connaître la valeur.</param>
	private string AttributeValue(XmlNode containerNode, string attributeName) {
		XmlNode attribute = containerNode.Attributes.GetNamedItem (attributeName);
		if (attribute != null)
			return attribute.InnerText;
		else
			return null;
	}


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

		XmlDocument mapSettingsDocument = new XmlDocument (); 
		XmlDocument mapResumedDocument = new XmlDocument (); 

		if (File.Exists (mapSettingsFilePath)) {
			mapSettingsDocument.Load (mapSettingsFilePath);

			// Création de l'en-tête du fichier résumé
			XmlDeclaration mapResumedDeclaration = mapResumedDocument.CreateXmlDeclaration ("1.0", "UTF-8", null);
			mapResumedDocument.AppendChild (mapResumedDeclaration);

			// Transfert des noeuds XML de zone dans le fichier résumé en retirant les noeuds XML d'information
			this.TransfertSettingsToResumed (mapResumedDocument, mapSettingsDocument, mapResumedDocument);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapResumedDocument.ChildNodes[1];

			if (earthNode != null) {
				// Création et ajout d'un nouveau noeud XML contenant les coordonnées minimales et maximales de la carte
				XmlNode boundsNode = this.NewBoundsNode (mapResumedDocument);
				earthNode.InsertBefore (boundsNode, earthNode.FirstChild);

				// Ajout d'un nouveau noeud XML pour chaque groupe de noeuds contenu dans l'application
				foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
					// Construction du chemin xPath vers le noeud XML correspondant à la zone la plus locale au groupe
					// de noeuds et récupération de ce noeud
					string zoningXPath = "";
					zoningXPath += "/" + XmlTags.EARTH;
					zoningXPath += "/" + XmlTags.COUNTRY + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
					zoningXPath += "/" + XmlTags.REGION + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
					zoningXPath += "/" + XmlTags.TOWN + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
					zoningXPath += "/" + XmlTags.DISTRICT + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";
					XmlNode locationNode = mapResumedDocument.SelectSingleNode (zoningXPath);

					if (locationNode != null) {
						// Construction du chemin xPath vers le noeud XML correspondant à la zone la plus locale au groupe
						// de noeuds et récupération de de noeud
						string customObjectXPath = zoningXPath + "/" + XmlTags.BUILDING + "/" + nodeGroup.Type () + "[@" + XmlAttributes.NAME + "=\"" + nodeGroup.Name + "\"]";
						XmlNode customObjectNode = mapResumedDocument.SelectSingleNode (customObjectXPath);

						XmlNode objectNode = null;
						XmlNode objectInfoNode = null;

						// Récupération du noeud XML d'information de l'objet personnalisé courant, soit en le
						// récupérant à partir du noeud XML correspondant à l'objet, soit en le créant si ce sernier
						// n'existe pas
						if (customObjectNode == null) {
							objectNode = mapResumedDocument.CreateElement (nodeGroup.Type ());
							objectInfoNode = mapResumedDocument.CreateElement (XmlTags.INFO);
							objectNode.AppendChild (objectInfoNode);
						} else {
							objectNode = customObjectNode;
							objectInfoNode = customObjectNode.FirstChild;
						}

						// Ajout de l'ID de l'objet au noeud XML d'information correspondant au dernier
						this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ID, nodeGroup.Id.ToString ());

						// Ajout des sous-noeuds XM au noeud XML correspondnant au groupe de noeuds courant
						this.AddInternalNodes (mapResumedDocument, nodeGroup, objectNode);

						// Ajout d'attributs propres à l'objet courant en fonction de son type, contenu dans le groupe
						// de noeuds courant
						if (nodeGroup.IsBuilding ())
							this.AddBuildingNodeAttribute (mapResumedDocument, nodeGroup, objectInfoNode);
						else if (nodeGroup.IsHighway () && !nodeGroup.IsTrafficLight ())
							this.AddHighwayNodeAttribute (mapResumedDocument, nodeGroup, objectInfoNode);

						// Ajout du nouveau noeud XML créé, représentant l'objet, au neud XML correspondant à la zone à
						// laquelle il appartient
						locationNode.AppendChild (objectNode);
					}
				}

				// Suppression des noeuds de zonage inutiles car vides
				this.RemoveUnusedNodes (mapResumedDocument);

				mapResumedDocument.Save (mapResumedFilePath);
			}
		}
	}


	/// <summary>
	/// 	Genère un noeud XML contenant les coordonnées minimales et maximales du terrain.
	/// </summary>
	/// <returns>Noeud XML contenant les coordonnées minimales et maximales du terrain.</returns>
	/// <param name="document">Fichier XML dans lequel ajouter les bornes du terrain.</param>
	private XmlNode NewBoundsNode(XmlDocument document) {
		// Ajout d'un nouveau noeud XML au document et ajout des informations à ce noeud.
		XmlNode res = document.CreateElement (XmlTags.BOUNDS);
		this.AppendAttribute (document, res, XmlAttributes.MIN_LATITUDE, minLat.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MIN_LONGITUDE, minLon.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MAX_LATITUDE, maxLat.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MAX_LONGITUDE, maxLon.ToString());
		return res;
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
	private void TransfertSettingsToResumed (XmlDocument targetDocument, XmlNode sourceParentElement, XmlNode targetParentElement) {
		
		foreach (XmlNode mapSettingsNode in sourceParentElement) {
			XmlNode mapResumedNode = targetDocument.ImportNode (mapSettingsNode, true);
			if ((!mapSettingsNode.Name.Equals (XmlTags.INFO) || sourceParentElement.Name.Equals(XmlTags.BUILDING)) && !mapResumedNode.Name.Equals (XmlTags.XML)) {
				targetParentElement.AppendChild (mapResumedNode);
				mapResumedNode.InnerText = "";
				mapResumedNode.InnerXml = "";
				this.TransfertSettingsToResumed (targetDocument, mapSettingsNode, mapResumedNode);
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
		foreach(XmlNode dataNode in parentElement.ChildNodes) {
			if (!dataNode.Name.Equals(XmlTags.XML) && !dataNode.Name.Equals(XmlTags.BOUNDS)) {

				// Ajout du noeud XML à la liste de suppression d'il remplit les critères et appel récursif sinon
				if (dataNode.ChildNodes.Count > 0 && dataNode.FirstChild.Name.Equals (XmlTags.INFO)) {
					if(dataNode.ChildNodes.Count == 1) {
						oldNodes.Add(dataNode);
					}
				} else {
					this.RemoveUnusedNodes(dataNode);
				}
			}
		}

		// Suppression des noeuds à supprimer
		foreach(XmlNode oldNode in oldNodes)
			parentElement.RemoveChild (oldNode);
	}


	/// <summary>
	/// 	Ajoute des sous-noeuds XML au noeuds XML corredpondant à un groupe de noeuds. La méthode ajoute également
	/// 	des informations d'identification et de localisation aux sous-noeuds XML ajoutés.
	/// </summary>
	/// <param name="mapResumedDocument">Document dans lequel ajouter les sous-noeuds.</param>
	/// <param name="nodeGroup">Groupe de noeuds contenant les noeuds à retranscrire dans le document.</param>
	/// <param name="objectNode">Groupe de noeuds contenant les noeuds à retranscrire en sous-noeuds XML.</param>
	private void AddInternalNodes(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode) {
		// Parcours de tous les noeuds du groupe de noeuds pour les retranscrire en sous-noeuds XML
		for (int i = 0; i < nodeGroup.Nodes.Count; i++) {
			Node node = (Node)nodeGroup.Nodes [i];

			// Affectation à l'index du nouveau noeud de la valeur de l'index servant à parcourir les noeuds du groupe
			// de noeuds
			node.Index = i;

			// Création d'un sous-noeud XML destiné à retranscrire le noeud courant
			XmlNode objectNd = mapResumedDocument.CreateElement (XmlTags.ND);
			objectNode.AppendChild (objectNd);

			// Ajout d'informations au nouveau sous-noeud XML
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.INDEX, node.Index.ToString ());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.REFERENCE, node.Reference.ToString());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
		}
	}


	/// <summary>
	/// 	Ajoute une série d'attributs contenant des information relatives aux bâtiments dans un noeud XML
	/// 	représentant un bâtiment.
	/// </summary>
	/// <param name="mapResumedDocument">Document dans lequel se trouve le noeud XML à modifier.</param>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds contenant les caractéristiques à insérer dans le noeuds XML à modifier.
	/// </param>
	/// <param name="objectInfoNode">Noeud XML d'information de l'objet à modifier.</param>
	private void AddBuildingNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);
	}


	/// <summary>
	/// 	Ajoute une série d'attributs contenant des informations relatives aux routes dans un noeud XML représentant
	/// 	une route.
	/// </summary>
	/// <param name="mapResumedDocument">Document dans lequel se trouve le noeud XML à modifier.</param>
	/// <param name="nodeGroup">
	/// 	Groupe de noeuds contenant les caractéristiques à insérer dans le noeuds XML à modifier.
	/// </param>
	/// <param name="objectInfoNode">Noeud XML d'information de l'objet à modifier.</param>
	private void AddHighwayNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROAD_TYPE, nodeGroup.GetTagValue ("highway"));
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NB_WAY, nodeGroup.NbWay.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.MAX_SPEED, nodeGroup.MaxSpeed.ToString());
	}


	/// <summary>
	/// 	Ajoute un attribut au noeud XML d'un certain document XML.
	/// </summary>
	/// <param name="boundingDocument">Document dans lequel se trouve le noeud XML à modifier.</param>
	/// <param name="containerNode">Noeud XML à modifier.</param>
	/// <param name="attributeName">Nom du nouvel attribut à ajouter.</param>
	/// <param name="attributeValue">Valeur du nouvel attribut à ajouter.</param>
	private void AppendAttribute(XmlDocument boundingDocument, XmlNode containerNode, string attributeName, string attributeValue) {
		// Création d'un nouvel attribut au noeud XML et changement de sa valeur
		XmlAttribute attribute = boundingDocument.CreateAttribute (attributeName);
		attribute.Value = attributeValue;

		// Ajout de cet attribut au noeud XML cible
		containerNode.Attributes.Append (attribute);
	}


	/// <summary>
	/// 	Extrait les informations necessaires à la construction de la ville depuis le fichier map_resumed en appelant
	/// 	notamment la fonction récursive d'extraction d'objets.
	/// </summary>
	public void LoadResumedData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument (); 

		// Suppression de tous les groupes de noeuds jusque là stockés
		objectBuilder.NodeGroups.Clear ();

		if (File.Exists (mapResumedFilePath)) {
			mapsSettingsDocument.Load (mapResumedFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];

			if (earthNode != null) {
				// Récupération des coordonnées minimales et maximales de la carte
				XmlNode boundsNode = earthNode.FirstChild;
				if (boundsNode != null) {
					minLat = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MIN_LATITUDE));
					minLon = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MIN_LONGITUDE));
					maxLat = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MAX_LATITUDE));
					maxLon = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MAX_LONGITUDE));
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
				this.ExtractResumedNodes (earthNode, new string[4], areas, 0);
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
			if (areaTypeIndex < areaTypes.Length) {
				// Extraction du nom de la zone courante et ajout de celle-ci au chemin emprunté
				areaDesignations [areaTypeIndex] = this.AttributeValue (childNode, XmlAttributes.DESIGNATION);

				// Appel récusrsif pour traiter les noeuds XML fils
				this.ExtractResumedNodes (childNode, areaDesignations, areaTypes, areaTypeIndex + 1);
			} else {
				// Récupération du noeud XML contenant les informations sur l'objet courant et récupération de l'ID de
				// cet objet
				XmlNode objectInfoNode = childNode.FirstChild;
				long id = long.Parse(this.AttributeValue (objectInfoNode, XmlAttributes.ID));

				// Création et remplissage d'un nouveau noeud avec les identifiants et coordonnées extraits de chacun
				// des sous-noeuds XML du noeud XML courant
				NodeGroup nodeGroup = new NodeGroup (id);
				for (int i = 1; i < childNode.ChildNodes.Count; i++) {
					XmlNode ndNode = childNode.ChildNodes[i];

					// Extraction des données du sous-noeud courant
					long reference = long.Parse (this.AttributeValue(ndNode, XmlAttributes.REFERENCE));
					int index = int.Parse (this.AttributeValue(ndNode, XmlAttributes.INDEX));
					double latitude = double.Parse (this.AttributeValue(ndNode, XmlAttributes.LATITUDE));
					double longitude = double.Parse (this.AttributeValue(ndNode, XmlAttributes.LONGIUDE));

					// Création d'un nouveau noeud et ajout de clui-ci au groupe de noeuds correspondant à l'objet
					Node node = new Node (reference, index, latitude, longitude);
					nodeGroup.AddNode (node);
				}

				// Stockage du zoning dans le groupe de noeuds correspondant à l'objet courant
				nodeGroup.Country = areaDesignations [0];
				nodeGroup.Region = areaDesignations [1];
				nodeGroup.Town = areaDesignations [2];
				nodeGroup.District = areaDesignations [3];

				// Ajout des caractéristiques et des tags aux groupes de noeuds en fonction du type d'objet extrait
				if (childNode.Name.Equals (XmlTags.BUILDING)) {
					nodeGroup.Name = this.AttributeValue (objectInfoNode, XmlAttributes.NAME);

					string[] areaBuildingInfo = this.BuildingInfo (objectInfoNode);
					// Ajout des caractéristiques aux groupes de noeuds
					nodeGroup.NbFloor = int.Parse(areaBuildingInfo [0]);
					nodeGroup.RoofAngle = int.Parse(areaBuildingInfo [1]);
					nodeGroup.RoofType = areaBuildingInfo [2];

					nodeGroup.AddTag("building", "yes");
				} else if (childNode.Name.Equals (XmlTags.HIGHWAY)) {
					nodeGroup.Name = this.AttributeValue (objectInfoNode, XmlAttributes.NAME);

					// Ajout des caractéristiques aux groupes de noeuds
					string[] areaHighwayInfo = this.HighwayInfo (objectInfoNode);
					nodeGroup.NbWay = int.Parse (areaHighwayInfo[1]);
					nodeGroup.MaxSpeed = int.Parse (areaHighwayInfo[2]);

					nodeGroup.AddTag("highway", areaHighwayInfo[0]);
				} else if (childNode.Name.Equals (XmlTags.WATERWAY)) {

				} else if (childNode.Name.Equals (XmlTags.TREE)) {
					nodeGroup.AddTag("natural", XmlTags.TREE);
				} else if (childNode.Name.Equals (XmlTags.TRAFFIC_LIGHT)) {
					nodeGroup.AddTag("highway", XmlTags.TRAFFIC_LIGHT);
				}

				// Ajout du groupe de noeuds courant à la liste de tous les groupes de noeuds
				objectBuilder.NodeGroups.Add(nodeGroup);
			}
		}
	}


	/// <summary>
	///		Extrait les données du MapCustom des objets et écrase les caractéristiques des objets correspondant dans le
	/// 	fichier map_resumed avec celles de la version personnalisées de l'objet.
	/// </summary>
	public void LoadCustomData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		string mapCustomFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";

		XmlDocument mapResumedDocument = new XmlDocument ();
		XmlDocument mapCustomDocument = new XmlDocument ();

		if (File.Exists (mapResumedFilePath) && File.Exists (mapCustomFilePath)) {
			mapResumedDocument.Load (mapResumedFilePath);
			mapCustomDocument.Load (mapCustomFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapCustomDocument.ChildNodes [1];

			if (earthNode != null) {
				// Récupération de chaque noeud représentant un fichier personnalisé puis mise à jour du noeud
				// correspondant dans le fihcier MapResumed
				XmlNodeList customNodes = earthNode.ChildNodes;
				foreach (XmlNode customNode in customNodes) {
					// Extraction du noeud XML d'information de l'objet personnalisé et récupération de son ID
					XmlNode customInfoNode = customNode.FirstChild;
					string objectId = this.AttributeValue (customInfoNode, XmlAttributes.ID);

					// Récupération du noeud XML d'information de l'objet du fichier map_resumed correspondant à l'objet
					// courant dans le fichier map_custom
					XmlNode matchingResumedInfoNode = mapResumedDocument.SelectSingleNode("//" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + objectId + "\"]");

					if (matchingResumedInfoNode != null) {

						// Récupération du noeud XML correspondant à l'objet du fichier map_resumed
						XmlNode matchingResumedNode = matchingResumedInfoNode.ParentNode;

						// Remplacement du noeud XML d'information seul dans le fichier résumé si la personnalisation ne
						// concerne que les caractéristiques, sinon, si la personnalisation concerne les sous-noeuds XML
						// de l'objet, ceux-ci sont aussi remplacés
						if (customNode.ChildNodes.Count == 1) {
							// Suppression du noeud XML d'information au niveau de l'objet du fichier map_resumed
							matchingResumedNode.RemoveChild (matchingResumedNode.FirstChild);

							// Ajout du noeud XML d'information du fichier map_custom avant les sous-noeuds XML de
							// l'objet du fichier mmap_resumed
							XmlNode newResumedInfoNode = mapResumedDocument.ImportNode (customInfoNode, true);
							matchingResumedNode.InsertBefore (newResumedInfoNode, matchingResumedNode.FirstChild);
						} else {
							// Suppression de tous les noeuds XML contenus (y compris le noeuds d'information) compris
							// dans le noeud XML de l'objet du fichier map_resumed
							matchingResumedNode.RemoveAll ();

							// Ajout de tous les noeuds XML contenus dans l'objet du fichier map_custom dans l'objet
							// correspondant dans le fichier map_resumed
							foreach (XmlNode customChildNode in customNode.ChildNodes) {
								XmlNode newResumedChildNode = mapResumedDocument.ImportNode (customChildNode, true);
								matchingResumedNode.AppendChild (newResumedChildNode);
							}
						}
					}
				}
			}

			mapResumedDocument.Save (mapResumedFilePath);
		}
	}


	public double Minlat {
		get { return minLat; }
		set { minLat = value; }
	}

	public double Minlon {
		get { return minLon; }
		set { minLon = value; }
	}

	public double Maxlat {
		get { return maxLat; }
		set { maxLat = value; }
	}

	public double Maxlon {
		get { return maxLon; }
		set { maxLon = value; }
	}

	private static class MapLoaderHolder {
		public static MapLoader instance = new MapLoader();
	}
}