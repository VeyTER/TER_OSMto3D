using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class FileManager {
	private ObjectBuilder objectBuilder;

	// compteurs de nodes et groupes de nodes (batiments puis routes) respectivement
	private int counter;
	private int buildingCounter;
	private int highwayCounter;

	// chemin d'acces et nom du fichier par defaut
	private string path = @"./Assets/";

	// Arraylist permettant de stocker les balises
	private ArrayList countries = new ArrayList();
	private ArrayList regions = new ArrayList();
	private ArrayList towns = new ArrayList();
	private ArrayList districts = new ArrayList();

	//creation d'une instance de ModificationPoint
	private PointEditor pointEditor;

	public FileManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();
		this.pointEditor = new PointEditor();
	}

	//Constructeur 
	public FileManager(string path) {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.path = path;

		this.countries = new ArrayList();
		this.regions = new ArrayList();
		this.towns = new ArrayList();
		this.districts = new ArrayList();

		this.pointEditor = new PointEditor();
	}

	/// <summary>
	/// Methode readFileOSM :
	/// Permet d'extraire les données de fichier ".osm" et de les stocker dans des objet "node" et "nodeGroup". 
	/// </summary>
	/// <param name="nameMap"> nom du fichier ".osm" dont on doit extraire les infos </param>
	public void readOSMFile(string mapName, int mapNum) {
		string OSMFilePath = path + "Maps/" + mapName + ".osm";
		XmlDocument OSMDocument = new XmlDocument(); 

		if (File.Exists (OSMFilePath)) {
			OSMDocument.Load (OSMFilePath);

			XmlNodeList boundsNodes = OSMDocument.GetElementsByTagName (XMLTags.BOUNDS);
			XmlNodeList nodeNodes = OSMDocument.GetElementsByTagName (XMLTags.NODE);
			XmlNodeList wayNodes = OSMDocument.GetElementsByTagName (XMLTags.WAY);

			if (boundsNodes.Count > 0) {
				XmlNode minLatAttribute = boundsNodes [0].Attributes.GetNamedItem (XMLAttributes.MIN_LATITUDE);
				XmlNode minLonAttribute = boundsNodes [0].Attributes.GetNamedItem (XMLAttributes.MIN_LONGITUTE);
				XmlNode maxLatAttribute = boundsNodes [0].Attributes.GetNamedItem (XMLAttributes.MAX_LATITUDE);
				XmlNode maxLonAttribute = boundsNodes [0].Attributes.GetNamedItem (XMLAttributes.MAX_LONGIUDE);

				Main.minlat = double.Parse (minLatAttribute.InnerText);
				Main.minlon = double.Parse (minLonAttribute.InnerText);
				Main.maxlat = double.Parse (maxLatAttribute.InnerText);
				Main.maxlon = double.Parse (maxLonAttribute.InnerText);
			}

			ArrayList nodes = new ArrayList();
			foreach (XmlNode nodeNode in nodeNodes) {
				XmlNode idAttribute = nodeNode.Attributes.GetNamedItem (XMLAttributes.ID);
				XmlNode latitudeAttribute = nodeNode.Attributes.GetNamedItem (XMLAttributes.LATITUDE);
				XmlNode longitudeAttribute = nodeNode.Attributes.GetNamedItem (XMLAttributes.LONGIUDE);

				double id = double.Parse (idAttribute.InnerText);
				double latitude = double.Parse(latitudeAttribute.InnerText);
				double longitude = double.Parse(longitudeAttribute.InnerText);

				XmlNodeList tagNodes = nodeNode.ChildNodes;
				for (int i = 0; i < tagNodes.Count; i++) {
					if(tagNodes [i].Name.Equals(XMLTags.TAG)) {
						XmlNode tagNode = tagNodes [i];

						XmlNode keyAttribute = tagNode.Attributes.GetNamedItem (XMLAttributes.KEY);
						XmlNode valueAttribute = tagNode.Attributes.GetNamedItem (XMLAttributes.VALUE);

						string key = keyAttribute.InnerText;
						string value = valueAttribute.InnerText;

						if (key.Equals (XMLKeys.NATURAL) || key.Equals (XMLKeys.HIGHWAY)) {
							NodeGroup nodeGroup = new NodeGroup (id);
							nodeGroup.AddTag (key, value);

							if (nodeGroup.IsTree() || nodeGroup.IsTrafficLight())
								nodeGroup.Name = value;

							nodeGroup.AddNode (new Node (id, longitude, latitude));
							objectBuilder.NodeGroups.Add (nodeGroup);
						}
					}
				}
				nodes.Add(new Node(id, longitude, latitude));
			}

			foreach (XmlNode wayNode in wayNodes) {
				XmlNode idAttribute = wayNode.Attributes.GetNamedItem (XMLAttributes.ID);
				double id = double.Parse (idAttribute.InnerText);

				NodeGroup nodeGroup = new NodeGroup (id);
				XmlNodeList ndNodes = wayNode.ChildNodes;
				for (int i = 0; i < ndNodes.Count; i++) {
					if(ndNodes[i].Name.Equals(XMLTags.ND)) {
						XmlNode ndNode = ndNodes [i];

						XmlNode referenceAttribute = ndNode.Attributes.GetNamedItem (XMLAttributes.REFERENCE);
						double reference = double.Parse (referenceAttribute.InnerText);

						int j = 0;
						for (; j < nodes.Count && ((Node)nodes [j]).Id != reference; j++);
						if(j < nodes.Count)
							nodeGroup.AddNode ((Node)nodes[j]);
					} else if(ndNodes[i].Name.Equals(XMLTags.TAG)) {
						XmlNode tagNode = ndNodes [i];

						XmlNode keyAttribute = tagNode.Attributes.GetNamedItem (XMLAttributes.KEY);
						XmlNode valueAttibute = tagNode.Attributes.GetNamedItem (XMLAttributes.VALUE);

						string key = keyAttribute.InnerText;
						string value = valueAttibute.InnerText;

						if (key.Equals (XMLValues.LANES))
							nodeGroup.NbWay = int.Parse (value);

						if (key.Equals (XMLValues.MAX_SPEED))
							nodeGroup.MaxSpeed = int.Parse (value);

						nodeGroup.AddTag(key, value);

						if (key.Equals(XMLKeys.ROOF_SHAPE))
							nodeGroup.RoofType = value;

						if (key.Equals(XMLKeys.NAME))
							nodeGroup.Name = value;
					}
				}

				if ((nodeGroup.IsBuilding() || nodeGroup.IsHighway()) || nodeGroup.IsWaterway())
					objectBuilder.NodeGroups.Add(nodeGroup);
			}
		}
	}
		
	/// <summary>
	/// Methode readSettingsFile :
	/// Lit un fichier setting
	/// </summary>
	/// <param name="nameFile"> nom du fichier setting a lire </param>
	public void readSettingsFile() {
		string mapSettingsFilePath = path + "Maps Settings/map_settings.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument(); 

		if (File.Exists (mapSettingsFilePath)) {
			mapsSettingsDocument.Load (mapSettingsFilePath);

			XmlNodeList earthNodes = mapsSettingsDocument.GetElementsByTagName (XMLTags.EARTH);
			if (earthNodes.Count > 0) {

				XmlNode earthInfosNode = earthNodes [0].FirstChild;
				if (earthInfosNode != null && earthInfosNode.Name.Equals (XMLTags.INFO)) {
					string[] earthBuildingInfos = this.AttributeBuildingInfo (earthInfosNode);
					this.SetupAreaNodeGroups (null, new double[] {0, 0, 0}, earthBuildingInfos, XMLTags.EARTH);

					string[] areas = new string[] {
						XMLTags.COUNTRY,
						XMLTags.REGION,
						XMLTags.TOWN,
						XMLTags.DISTRICT,
						XMLTags.BUILDING
					};

					this.FillNodeGroups (earthNodes [0], areas, 0);
				}
			}
		}
	}

	private void FillNodeGroups(XmlNode boundingAreaNode, string[] areas, int areaIndex) {
		for (int i = 1; i < boundingAreaNode.ChildNodes.Count; i++) {
			if (boundingAreaNode.ChildNodes[i].Name.Equals (areas[areaIndex])) {
				XmlNode areaNode = boundingAreaNode.ChildNodes [i];

				XmlNode areaDesignationAttribute = areaNode.Attributes.GetNamedItem (XMLAttributes.DESIGNATION);
				string areaDesignation = areaDesignationAttribute.InnerText;

				XmlNode areaInfosNode = areaNode.FirstChild;
				double[] areaLocationInfos = this.AttributeLocationInfo(areaInfosNode);
				string[] areaBuildingInfos = this.AttributeBuildingInfo (areaInfosNode);

				this.SetupAreaNodeGroups (areaDesignation, areaLocationInfos, areaBuildingInfos, areas[areaIndex]);

				if (areaIndex < areas.Length)
					this.FillNodeGroups (areaNode, areas, areaIndex + 1);
			}
		}
	}

	private string[] AttributeBuildingInfo(XmlNode infoNode) {
		string[] res = new string[3];

		XmlNode nbFloorAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.NB_FLOOR);
		XmlNode roofAngleAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.ROOF_ANGLE);
		XmlNode roofTypeAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.ROOF_TYPE);

		res[0] = nbFloorAttribute.InnerText;
		res[1] = roofAngleAttribute.InnerText;
		res[2] = roofTypeAttribute.InnerText.Equals("unknown") ? "" : roofTypeAttribute.InnerText;

		return res;
	}

	private double[] AttributeLocationInfo(XmlNode infoNode) {
		double[] res = new double[3];

		XmlNode latitudeAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.LATITUDE);
		XmlNode longitudeAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.LONGIUDE);
		XmlNode distanceAttribute = infoNode.Attributes.GetNamedItem (XMLAttributes.DISTANCE);

		res[0] = double.Parse (latitudeAttribute.InnerText);
		res[1] = double.Parse (longitudeAttribute.InnerText);
		res[2] = double.Parse (distanceAttribute.InnerText);

		return res;
	}

	private void SetupAreaNodeGroups(string designation, double[] locationData, string[] buildingData, string tagName) {
		foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
			double nodeGroupDistance = Math.Sqrt (Math.Pow (locationData [0] - (nodeGroup.GetNode (0).Latitude), 2) + Math.Pow (locationData [1] - (nodeGroup.GetNode (0).Longitude), 2));
			if (tagName.Equals(XMLTags.EARTH) || nodeGroupDistance < locationData[2]) {
				switch (tagName) {
				case XMLTags.COUNTRY:
					nodeGroup.Country = designation;
					break;
				case XMLTags.REGION:
					nodeGroup.Region = designation;
					break;
				case XMLTags.TOWN:
					nodeGroup.Town = designation;
					break;
				case XMLTags.DISTRICT:
					nodeGroup.District = designation;
					break;
				}

				nodeGroup.NbFloor = int.Parse(buildingData[0]);
				nodeGroup.RoofAngle = int.Parse(buildingData[1]);
				nodeGroup.RoofType = buildingData[2];
			}
		}
	}

	/// <summary>
	/// Methode createResumeFile :
	/// Cree un fichier propre ou les informations sont resumees
	/// </summary>
	/// <param name="nameFile"> nom du fichier ou l'on inscrit les informations </param>
	public void createResumeFile() {
		string mapSettingsFilePath = path + "Maps Settings/map_settings.osm";
		string mapResumedFilePath = path + "Maps Resumed/map_resumed.osm";

		XmlDocument mapSettingsDocument = new XmlDocument (); 
		XmlDocument mapResumedDocument = new XmlDocument (); 

		if (File.Exists (mapSettingsFilePath)) {
			mapSettingsDocument.Load (mapSettingsFilePath);

			XmlDeclaration mapResumedDeclaration = mapResumedDocument.CreateXmlDeclaration ("1.0", "UTF-8", null);
			mapResumedDocument.AppendChild (mapResumedDeclaration);
			this.TransfertNodes (mapResumedDocument, mapSettingsDocument, mapResumedDocument);

			foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
				string xPath = "";
				xPath += "/" + XMLTags.EARTH;
				xPath += "/" + XMLTags.COUNTRY + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
				xPath += "/" + XMLTags.REGION + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
				xPath += "/" + XMLTags.TOWN + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
				xPath += "/" + XMLTags.DISTRICT + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";
//				xPath += "/" + XMLTags.BUILDING;

				XmlNode locationNode = mapResumedDocument.SelectSingleNode (xPath);

				if (locationNode != null) {
					XmlNode objectNode = mapResumedDocument.CreateElement (nodeGroup.ObjectType ());
					XmlNode objectInfoNode = mapResumedDocument.CreateElement (XMLTags.INFO);
					objectNode.AppendChild (objectInfoNode);

					XmlAttribute objectIdAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.ID);
					objectIdAttribute.Value = nodeGroup.Id + "";
					objectInfoNode.Attributes.Append (objectIdAttribute);

					this.AddInternalNodes (mapResumedDocument, nodeGroup, objectNode);

					if (nodeGroup.IsBuilding ())
						this.AddBuildingNodeInfo (mapResumedDocument, nodeGroup, objectNode, objectInfoNode);
					else if (nodeGroup.IsHighway () && !nodeGroup.IsTrafficLight ())
						this.AddHighwayNodeInfo (mapResumedDocument, nodeGroup, objectNode, objectInfoNode);

					locationNode.AppendChild (objectNode);
				}
			}

			mapResumedDocument.Save (mapResumedFilePath);
		}
	}

	private void TransfertNodes (XmlDocument targetDocument, XmlNode mapSettingsBoundingElement, XmlNode mapResumedBoundingElement) {
		foreach (XmlNode mapSettingsNode in mapSettingsBoundingElement) {
			XmlNode mapResumedNode = targetDocument.ImportNode (mapSettingsNode, true);
			if ((!mapSettingsNode.Name.Equals (XMLTags.INFO) || mapSettingsBoundingElement.Name.Equals(XMLTags.BUILDING)) && !mapResumedNode.Name.Equals (XMLTags.XML)) {
				mapResumedBoundingElement.AppendChild (mapResumedNode);
				mapResumedNode.InnerText = "";
				mapResumedNode.InnerXml = "";
				this.TransfertNodes (targetDocument, mapSettingsNode, mapResumedNode);
			}
		}
	}

	private void AddInternalNodes(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode) {
		foreach (Node node in nodeGroup.Nodes) {
			XmlNode objectNd = mapResumedDocument.CreateElement (XMLTags.ND);
			objectNode.AppendChild (objectNd);

			XmlAttribute objectNdIdAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.ID);
			XmlAttribute objectNdLatitudeAttibute = mapResumedDocument.CreateAttribute (XMLAttributes.LATITUDE);
			XmlAttribute objectNdLongitudeAttibute = mapResumedDocument.CreateAttribute (XMLAttributes.LONGIUDE);

			objectNdIdAttribute.Value = node.Id + "";
			objectNdLatitudeAttibute.Value = node.Latitude + "";
			objectNdLongitudeAttibute.Value = node.Longitude + "";

			objectNd.Attributes.Append (objectNdIdAttribute);
			objectNd.Attributes.Append (objectNdLatitudeAttibute);
			objectNd.Attributes.Append (objectNdLongitudeAttibute);
		}
	}

	private void AddBuildingNodeInfo (XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		XmlAttribute buildingNameAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.NAME);
		XmlAttribute buildingNbFloorAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.NB_FLOOR);
		XmlAttribute buildingRoofAngleAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.ROOF_ANGLE);
		XmlAttribute buildingRoofTypeAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.ROOF_TYPE);

		buildingNameAttribute.Value = nodeGroup.Name;
		buildingNbFloorAttribute.Value = nodeGroup.NbFloor + "";
		buildingRoofAngleAttribute.Value = nodeGroup.RoofAngle + "";
		buildingRoofTypeAttribute.Value = nodeGroup.RoofType;

		objectInfoNode.Attributes.Append (buildingNameAttribute);
		objectInfoNode.Attributes.Append (buildingNbFloorAttribute);
		objectInfoNode.Attributes.Append (buildingRoofAngleAttribute);
		objectInfoNode.Attributes.Append (buildingRoofTypeAttribute);
	}

	private void AddHighwayNodeInfo (XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		XmlAttribute highwayNameAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.NAME);
		XmlAttribute highwayTypeAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.ROAD_TYPE);
		XmlAttribute highwayNbWayAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.NB_WAY);
		XmlAttribute highwayMaxSpeedAttribute = mapResumedDocument.CreateAttribute (XMLAttributes.MAX_SPEED);

		highwayNameAttribute.Value = nodeGroup.Name;
		highwayTypeAttribute.Value = nodeGroup.GetTagValue ("highway");
		highwayNbWayAttribute.Value = nodeGroup.NbWay + "";
		highwayMaxSpeedAttribute.Value = nodeGroup.MaxSpeed + "";

		objectInfoNode.Attributes.Append (highwayNameAttribute);
		objectInfoNode.Attributes.Append (highwayTypeAttribute);
		objectInfoNode.Attributes.Append (highwayNbWayAttribute);
		objectInfoNode.Attributes.Append (highwayMaxSpeedAttribute);
	}

	/// <summary>
	/// Metode readSettingFile :
	/// Lit un fichier resume
	/// </summary>
	/// <param name="nameFile"> nom du fichier resume a lire </param>
	public void readResumeFile() {
		ArrayList nodes = new ArrayList();

		objectBuilder.NodeGroups.Clear ();

		string line;
	
		double id = 0d;
		double lat = 0d;
		double lon = 0d;

		//Variables permettant de recuperer les balises
		string strCou="country";
		string strReg="region";
		string strTow="town";
		string strDis="district";

		// Read the file and display it line by line.
		StreamReader file = new StreamReader(path + "Maps Resumed/map_resumed.osm");
		while ((line = file.ReadLine()) != null) {
			
			// Recuperation de limites de la carte
			if (line.Contains("<bounds")) {
				Main.minlat = double.Parse(line.Substring(line.IndexOf("minlat=") + 8, line.IndexOf("\" minlon=") - line.IndexOf("minlat=") - 8));
				Main.maxlat = double.Parse(line.Substring(line.IndexOf("maxlat=") + 8, line.IndexOf("\" maxlon=") - line.IndexOf("maxlat=") - 8));
				Main.minlon = double.Parse(line.Substring(line.IndexOf("minlon=") + 8, line.IndexOf("\" maxlat=") - line.IndexOf("minlon=") - 8));
				Main.maxlon = double.Parse(line.Substring(line.IndexOf("maxlon=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxlon=") - 8));
			}

			// Recuperation des balises
			if (line.Contains("<country"))
				strCou = line.Substring(line.IndexOf(XMLAttributes.DESIGNATION + "=") + 3, line.IndexOf("\">") - line.IndexOf(XMLAttributes.DESIGNATION + "=") - 3);

			if (line.Contains("<region"))
				strReg = line.Substring(line.IndexOf(XMLAttributes.DESIGNATION + "=") + 3, line.IndexOf("\">") - line.IndexOf(XMLAttributes.DESIGNATION + "=") - 3);

			if (line.Contains("<town"))
				strTow = line.Substring(line.IndexOf(XMLAttributes.DESIGNATION + "=") + 3, line.IndexOf("\">") - line.IndexOf(XMLAttributes.DESIGNATION + "=") - 3);

			if (line.Contains("<district"))
				strDis = line.Substring(line.IndexOf(XMLAttributes.DESIGNATION + "=") + 3, line.IndexOf("\">") - line.IndexOf(XMLAttributes.DESIGNATION + "=") - 3);

			//Recuparation des batiments
			if (line.Contains("<building")) {
				line = file.ReadLine();

				// Creation d'un nouveau nodegroup
				NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" name=") - line.IndexOf("id=") - 4)));

				// Ajout des caractéristiques du batiment
				current.Name = line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" nbFloor") - line.IndexOf("name=") - 6);
				current.NbFloor = int.Parse(line.Substring(line.IndexOf("nbFloor=") + 9, line.IndexOf("\" type") - line.IndexOf("nbFloor=") - 9));
				current.RoofType = line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\" angle") - line.IndexOf("type=") - 6);
				current.RoofAngle = int.Parse(line.Substring(line.IndexOf("angle=") + 7, line.IndexOf("\"/>") - line.IndexOf("angle=") - 7));

				// Lecture d'une nouvelle ligne
				line = file.ReadLine();
				while (!line.Contains("</building")) {
					if (line.Contains("<node")) {
						
						// Recuperation des nodes
						id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
						lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
						lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

						nodes.Add(new Node(id, lon, lat));

						// Ajout du nouveau node au nodegroup
						current.AddNode(new Node(id, lon, lat));

					}

					// Changement de ligne
					line = file.ReadLine();
				}

				// Ajout des balises de location dans le nodegroup
				current.Country = strCou;
				current.Region = strReg;
				current.Town = strTow;
				current.District = strDis;

				//Ajout du tag 
				current.AddTag("building", "yes");

				//Ajout du nodeGroup courent à la liste main.nodeGroups
				objectBuilder.NodeGroups.Add(current);
			}

			//Récupération des routes
			if (line.Contains("<highway")) {
				line = file.ReadLine();

				// Creation d'un nouveau nodegroup
				NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" type=") - line.IndexOf("id=") - 4)));

				// Ajout des caractéristiques de la route
				current.AddTag("highway", line.Substring(line.IndexOf("type=") + 6, line.IndexOf("\" name") - line.IndexOf("type=") - 6));
				current.Name = line.Substring(line.IndexOf("name=") + 6, line.IndexOf("\" nbVoie") - line.IndexOf("name=")-6);
				current.NbWay = int.Parse(line.Substring(line.IndexOf("nbVoie=") + 8, line.IndexOf("\" maxspd") - line.IndexOf("nbVoie=") - 8));
				current.MaxSpeed = int.Parse(line.Substring(line.IndexOf("maxspd=") + 8, line.IndexOf("\"/>") - line.IndexOf("maxspd=") - 8));

				// Lecture d'une nouvelle ligne
				line = file.ReadLine();

				while (!line.Contains("</highway")) {
					if (line.Contains("<node")) {
						// Recuperation des nodes
						id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
						lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
						lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

						// Ajout du nouveau node au nodegroup
						current.AddNode(new Node(id, lon, lat));
					}

					// Changement de ligne
					line = file.ReadLine();
				}

				// Ajout des balises de location dans le nodegroup
				current.Country = strCou;
				current.Region = strReg;
				current.Town = strTow;
				current.District = strDis;

				//Ajout du nodeGroup courent à la liste main.nodeGroups
				objectBuilder.NodeGroups.Add(current);
			}

			// Récupération des routes
			if (line.Contains("<waterway")) {
				line = file.ReadLine();

				// Creation d'un nouveau nodegroup
				NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\"/>") - line.IndexOf("id=") - 4)));

				current.AddTag("waterway", "");

				// Lecture d'une nouvelle ligne
				line = file.ReadLine();

				while (!line.Contains("</waterway")) {
					if (line.Contains("<node")) {
						
						// Recuperation des nodes
						id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
						lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
						lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

						// Ajout du nouveau node au nodegroup
						current.AddNode(new Node(id, lon, lat));
					}

					// Changement de ligne
					line = file.ReadLine();
				}

				// Ajout des balises de location dans le nodegroup
				current.Country = strCou;
				current.Region = strReg;
				current.Town = strTow;
				current.District = strDis;

				//Ajout du nodeGroup courent à la liste main.nodeGroups
				objectBuilder.NodeGroups.Add(current);
			}

			//Recuparation des arbres
			if (line.Contains("<tree")) {
				line = file.ReadLine();

				// Creation d'un nouveau nodegroup
				NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\"/>") - line.IndexOf("id=") - 4)));

				// Lecture d'une nouvelle ligne
				line = file.ReadLine();
				while (!line.Contains("</tree")) {
					if (line.Contains("<node")) {
						
						// Recuperation des nodes
						id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
						lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
						lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

						// Ajout du nouveau node au nodegroup
						current.AddNode(new Node(id, lon, lat));
					}

					// Changement de ligne
					line = file.ReadLine();
				}

				// Ajout des balises de location dans le nodegroup
				current.Country = strCou;
				current.Region = strReg;
				current.Town = strTow;
				current.District = strDis;

				//Ajout du tag 
				current.AddTag("natural", "tree");

				//Ajout du nodeGroup courent à la liste main.nodeGroups
				objectBuilder.NodeGroups.Add(current);
			}

			//Recuparation des feux tricolores
			if (line.Contains("<feuTri")) {
				line = file.ReadLine();

				// Creation d'un nouveau nodegroup
				NodeGroup current = new NodeGroup(double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\"/>") - line.IndexOf("id=") -  4)));

				// Lecture d'une nouvelle ligne
				line = file.ReadLine();
				while (!line.Contains("</feuTri")) {
					if (line.Contains("<node")) {
						
						// Recuperation des nodes
						id = double.Parse(line.Substring(line.IndexOf("id=") + 4, line.IndexOf("\" lat=") - line.IndexOf("id=") - 4));
						lat = double.Parse(line.Substring(line.IndexOf("lat=") + 5, line.IndexOf("\" lon=") - line.IndexOf("lat=") - 5));
						lon = double.Parse(line.Substring(line.IndexOf("lon=") + 5, line.IndexOf("\"/>") - line.IndexOf("lon=") - 5));

						// Ajout du nouveau node au nodegroup
						current.AddNode(new Node(id, lon, lat));
					}

					// Changement de ligne
					line = file.ReadLine();
				}

				// Ajout des balises de location dans le nodegroup
				current.Country = strCou;
				current.Region = strReg;
				current.Town = strTow;
				current.District = strDis;

				//Ajout du tag 
				current.AddTag("highway", "traffic_signals");

				//Ajout du nodeGroup courent à la liste main.nodeGroups
				objectBuilder.NodeGroups.Add(current);
			}
		}
	}
}