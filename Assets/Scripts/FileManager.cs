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
		string mapSettingsFilePath = path + "Maps Resumed/map_settings.osm";
		string mapResumedFilePath = path + "Maps Resumed/map_resumed.osm";

		XmlDocument mapSettingsDocument = new XmlDocument(); 
		XmlDocument mapResumedDocument = new XmlDocument(); 

		if (File.Exists (mapSettingsFilePath) && File.Exists (mapResumedFilePath)) {
			mapSettingsDocument.Load (mapSettingsFilePath);
			mapResumedDocument.Load (mapResumedFilePath);

			foreach (XmlNode mapSettingsNode in mapSettingsDocument.ChildNodes) {
				if (!mapSettingsNode.Name.Equals (XMLTags.INFO)) {
					XmlNode mapResumedNode = mapSettingsNode.Clone;
					mapResumedDocument.AppendChild (mapResumedNode, mapResumedNode);

					this.Transfert (mapSettingsNode);
				}
			}
		}





















//		// listing des balises de location "country,region,town,district"
//		foreach (NodeGroup ngp in objectBuilder.NodeGroups) {
//			if (!countries.Contains(ngp.Country))
//				countries.Add(ngp.Country);
//
//			if (!regions.Contains(ngp.Region))
//				regions.Add(ngp.Region);
//
//			if (!towns.Contains(ngp.Town))
//				towns.Add(ngp.Town);
//
//			if (!districts.Contains(ngp.District))
//				districts.Add(ngp.District);
//		}
//
//		string pathString = path + "Maps Resumed/map_resumed.osm";
//		string buildingName;
//		string highwayName;
//		string typeRoof;
//		int angleRoof;
//		int nbFloor;
//		double ID;
//		string typeRoute;
//		int nbVoie;
//		int maxspeed;
//
//		//on créé le fichier et on va écrire dedans
//		System.IO.StreamWriter file = new System.IO.StreamWriter(pathString);
//		file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
//
//		file.WriteLine("<root>");
//
//		file.WriteLine("\t<bounds minlat=\"" +Main.minlat + "\" minlon=\"" + Main.minlon + "\" maxlat=\"" + Main.maxlat + "\" maxlon=\"" + Main.maxlon + "\"/>");
//
//		//Ecriture premiere balise earth
//		file.WriteLine("\t<earth>");
//
//
//
//		//ecriture des locations
//		foreach (string str1 in countries) {
//			file.WriteLine("\t\t<country " + XMLAttributes.DESIGNATION + "=\"" + str1 + "\">");
//
//			foreach (string str2 in regions) {
//				file.WriteLine("\t\t\t<region " + XMLAttributes.DESIGNATION + "=\"" + str2 + "\">");
//
//				foreach (string str3 in towns) {
//					file.WriteLine("\t\t\t\t<town " + XMLAttributes.DESIGNATION + "=\"" + str3 + "\">");
//
//					foreach (string str4 in districts) {
//						file.WriteLine("\t\t\t\t\t<district " + XMLAttributes.DESIGNATION + "=\"" + str4 + "\">");
//
//						foreach (NodeGroup ngp in objectBuilder.NodeGroups) {
//							if ((ngp.Country == str1) && (ngp.Region == str2) && (ngp.Town == str3) && (ngp.District== str4)) {
//								//Si c'est un bâtiment 
//								if (ngp.IsBuilding()) {
//									//on récupère le nom du batiment et ses caractéristiques
//									buildingName = ngp.Name;
//									nbFloor = ngp.NbFloor;
//									typeRoof = ngp.RoofType;
//									angleRoof = ngp.RoofAngle;
//									ID = ngp.Id;
//
//									//On écrit ces infos sur le ...Resumed
//									file.WriteLine("\t\t\t\t\t\t<building>");
//									file.WriteLine("\t\t\t\t\t\t\t<info id=\"" + ID + "\" name=\"" + buildingName + "\" nbFloor=\"" + nbFloor + "\" type=\"" + typeRoof + "\" angle=\"" + angleRoof + "\"/>");
//
//									//ecriture des nodes
//									foreach (Node n in ngp.Nodes)
//										file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.Id + "\" lat=\"" + n.Latitude + "\" lon=\"" + n.Longitude + "\"/>");
//
//									//ecriture balise fin de building
//									file.WriteLine("\t\t\t\t\t\t</building>");
//								}
//
//								//Si c'est un arbre
//								if (ngp.IsTree()) {
//									//On récupère les caractéristiques de l'arbre
//									ID = ngp.Id;
//
//									//On écrit ces infos sur le ...Resumed
//									file.WriteLine("\t\t\t\t\t\t<tree>");
//									file.WriteLine("\t\t\t\t\t\t\t<info id=\"" + ID + "\"/>");
//
//									//ecriture des nodes
//									foreach (Node n in ngp.Nodes)
//										file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.Id + "\" lat=\"" + n.Latitude + "\" lon=\"" + n.Longitude + "\"/>");
//
//									//ecriture balise fin d'arbre
//									file.WriteLine("\t\t\t\t\t\t</tree>");
//								}
//
//								//Si c'est un feu tricolore
//								if (ngp.IsTrafficLight()) {
//									//On récupère les caractéristiques du feu tricolore
//									ID = ngp.Id;
//
//									//On écrit ces infos sur le ...Resumed
//									file.WriteLine("\t\t\t\t\t\t<feuTri>");
//									file.WriteLine("\t\t\t\t\t\t\t<info id=\"" + ID + "\"/>");
//
//									//ecriture des nodes
//									foreach (Node n in ngp.Nodes)
//										file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.Id + "\" lat=\"" + n.Latitude + "\" lon=\"" + n.Longitude + "\"/>");
//
//									//ecriture balise fin d'arbre
//									file.WriteLine("\t\t\t\t\t\t</feuTri>");
//								}
//
//								//Si c'est une voie d'eau
//								if (ngp.IsWaterway()) {
//									
//									//On récupère les caractéristiques de la voie d'eau
//									ID = ngp.Id;
//
//									//On écrit ces infos sur le ...Resumed
//									file.WriteLine("\t\t\t\t\t\t<waterway>");
//									file.WriteLine("\t\t\t\t\t\t\t<info id=\"" + ID + "\"/>");
//
//									//ecriture des nodes
//									foreach (Node n in ngp.Nodes)
//										file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.Id + "\" lat=\"" + n.Latitude + "\" lon=\"" + n.Longitude + "\"/>");
//
//									//ecriture balise fin d'arbre
//									file.WriteLine("\t\t\t\t\t\t</waterway>");
//								}
//
//								// Si c'est une route
//								if (ngp.IsHighway() && !(ngp.IsTrafficLight())){
//									//on recupère les données sur la route
//									ID = ngp.Id;
//									typeRoute = ngp.GetTagValue("highway");
//									highwayName = ngp.Name;
//									nbVoie = ngp.NbWay;
//									maxspeed = ngp.MaxSpeed;
//
//									//On écrit ces infos sur le ...Resumed
//									file.WriteLine("\t\t\t\t\t\t<highway>");
//									file.WriteLine("\t\t\t\t\t\t\t<info id=\"" + ID + "\" type=\"" + typeRoute + "\" name=\"" + highwayName + "\" nbVoie=\"" + nbVoie + "\" maxspd=\"" + maxspeed + "\"/>");
//
//									//ecriture des nodes
//									foreach (Node n in ngp.Nodes)
//										file.WriteLine("\t\t\t\t\t\t\t<node id=\"" + n.Id + "\" lat=\"" + n.Latitude + "\" lon=\"" + n.Longitude + "\"/>");
//
//									//ecriture balise fin de la route
//									file.WriteLine("\t\t\t\t\t\t</highway>");
//								}
//							}
//						}
//						file.WriteLine("\t\t\t\t\t</district>");
//					}
//					file.WriteLine("\t\t\t\t</town>");
//				}
//				file.WriteLine("\t\t\t</region>");
//			}
//			file.WriteLine("\t\t</country>");
//		}
//
//		//Fermeture de le balise earth
//		file.WriteLine("\t</earth>");
//
//		// AJOUT
//		file.WriteLine("</root>");
//
//		// Fermeture du fichier 
//		file.Close();
	}

	private void Transfert (XmlNode mapSettingsBoundingNode, XmlNode mapResumedBoundingNode) {
		foreach (XmlNode mapSettingsNode in mapSettingsBoundingNode) {
			if (!mapSettingsNode.Name.Equals (XMLTags.INFO)) {
				XmlNode mapResumedNode = mapSettingsNode.Clone;
				mapResumedBoundingNode.AppendChild (mapResumedNode);

				this.Transfert (mapSettingsNode, mapResumedNode);
			}
		}
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