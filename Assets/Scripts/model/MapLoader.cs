using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class MapLoader {
	private ObjectBuilder objectBuilder;

	// coordonnées min et max de la carte
	private double minlat;
	private double minlon;
	private double maxlat;
	private double maxlon;

	private MapLoader() {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.minlat = 0;
		this.minlon = 0;
		this.maxlat = 0;
		this.maxlon = 0;
	}

	public static MapLoader GetInstance() {
		return MapLoaderHolder.instance;
	}

	/// <summary>
	/// Methode readFileOSM :
	/// Permet d'extraire les données de fichier ".osm" et de les stocker dans des objet "node" et "nodeGroup". 
	/// </summary>
	/// <param name="nameMap"> nom du fichier ".osm" dont on doit extraire les infos </param>
	public void LoadOsmData(string mapName) {
		string OSMFilePath = mapName;
		XmlDocument OSMDocument = new XmlDocument(); 

		if (File.Exists (OSMFilePath)) {
			OSMDocument.Load (OSMFilePath);

			XmlNodeList boundsNodes = OSMDocument.GetElementsByTagName (XmlTags.BOUNDS);
			if (boundsNodes.Count > 0) {
				minlat = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MIN_LATITUDE));
				minlon = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MIN_LONGITUDE));
				maxlat = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MAX_LATITUDE));
				maxlon = double.Parse (this.AttributeValue (boundsNodes [0], XmlAttributes.MAX_LONGITUDE));
			}

			// Extraction de toutes les nodes nodes, contenant des infos sur les sous-nodes
			ArrayList nodes = new ArrayList();
			XmlNodeList nodeNodes = OSMDocument.GetElementsByTagName (XmlTags.NODE);
			foreach (XmlNode nodeNode in nodeNodes) {
				long id = long.Parse (this.AttributeValue (nodeNode, XmlAttributes.ID));
				double latitude = double.Parse(this.AttributeValue (nodeNode, XmlAttributes.LATITUDE));
				double longitude = double.Parse(this.AttributeValue (nodeNode, XmlAttributes.LONGIUDE));

				XmlNodeList tagNodes = nodeNode.ChildNodes;
				for (int i = 0; i < tagNodes.Count; i++) {
					if(tagNodes [i].Name.Equals(XmlTags.TAG)) {
						XmlNode tagNode = tagNodes [i];

						string key = this.AttributeValue (tagNode, XmlAttributes.KEY);
						string value = this.AttributeValue (tagNode, XmlAttributes.VALUE);

						if (key.Equals (XmlKeys.NATURAL) || key.Equals (XmlKeys.HIGHWAY)) {
							NodeGroup nodeGroup = new NodeGroup (id);
							nodeGroup.AddTag (key, value);

							if (nodeGroup.IsTree() || nodeGroup.IsTrafficLight())
								nodeGroup.Name = value;

							nodeGroup.AddNode (new Node (id, latitude, longitude));
							objectBuilder.NodeGroups.Add (nodeGroup);
						}
					}

				}
				nodes.Add(new Node(id, latitude, longitude));
			}

			// Extraction de tous les sous-nodes en recherchant à chque fois le node contenant des infos dessus
			XmlNodeList wayNodes = OSMDocument.GetElementsByTagName (XmlTags.WAY);
			foreach (XmlNode wayNode in wayNodes) {
				long id = long.Parse (this.AttributeValue (wayNode, XmlAttributes.ID));

				NodeGroup nodeGroup = new NodeGroup (id);
				for (int i = 0; i < wayNode.ChildNodes.Count; i++) {
					XmlNode ndNode = wayNode.ChildNodes[i];
					if(ndNode.Name.Equals(XmlTags.ND)) {
						long reference = long.Parse (this.AttributeValue(ndNode, XmlAttributes.REFERENCE));

						Node infoNode = (Node)nodes [0];
						for (int j = 0; j < nodes.Count && infoNode.Reference != reference; infoNode = (Node)nodes [j], j++);
						if (infoNode != null)
							nodeGroup.AddNode (infoNode);
					} else if(ndNode.Name.Equals(XmlTags.TAG)) {
						XmlNode tagNode = ndNode;

						string key = this.AttributeValue (tagNode, XmlAttributes.KEY);
						string value = this.AttributeValue (tagNode, XmlAttributes.VALUE);

						if (key.Equals (XmlValues.LANES))
							nodeGroup.NbWay = int.Parse (value);

// 						TODO Tester, MAX_SPEED étant une valeur et non une clé...
//						if (key.Equals (XmlValues.MAX_SPEED))
//							nodeGroup.MaxSpeed = int.Parse (value);

						nodeGroup.AddTag(key, value);

						if (key.Equals(XmlKeys.ROOF_SHAPE))
							nodeGroup.RoofType = value;

						if (key.Equals(XmlKeys.NAME))
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
	public void LoadSettingsData() {
		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument(); 

		if (File.Exists (mapSettingsFilePath)) {
			mapsSettingsDocument.Load (mapSettingsFilePath);

			XmlNodeList earthNodes = mapsSettingsDocument.GetElementsByTagName (XmlTags.EARTH);
			if (earthNodes.Count > 0) {

				XmlNode earthInfosNode = earthNodes [0].FirstChild;
				if (earthInfosNode != null && earthInfosNode.Name.Equals (XmlTags.INFO)) {
					string[] earthBuildingInfos = this.AttributeBuildingInfo (earthInfosNode);
					this.SetupAreaNodeGroups (null, new double[] {0, 0, 0}, earthBuildingInfos, XmlTags.EARTH);

					string[] areas = new string[] {
						XmlTags.COUNTRY,
						XmlTags.REGION,
						XmlTags.TOWN,
						XmlTags.DISTRICT,
						XmlTags.BUILDING
					};

					this.CreateNodeGroups (earthNodes [0], areas, 0);
				}
			}
		}
	}

	private void CreateNodeGroups(XmlNode boundingAreaNode, string[] areaTypes, int areaTypeIndex) {
		for (int i = 1; i < boundingAreaNode.ChildNodes.Count; i++) {
			if (boundingAreaNode.ChildNodes[i].Name.Equals (areaTypes[areaTypeIndex])) {
				XmlNode areaNode = boundingAreaNode.ChildNodes [i];
				string areaDesignation = this.AttributeValue (areaNode, XmlAttributes.DESIGNATION);

				XmlNode areaInfosNode = areaNode.FirstChild;
				double[] areaLocationInfos = this.AttributeLocationInfo (areaInfosNode);
				string[] areaBuildingInfos = this.AttributeBuildingInfo (areaInfosNode);

				this.SetupAreaNodeGroups (areaDesignation, areaLocationInfos, areaBuildingInfos, areaTypes[areaTypeIndex]);

				if (areaTypeIndex < areaTypes.Length)
					this.CreateNodeGroups (areaNode, areaTypes, areaTypeIndex + 1);
			}
		}
	}

	private void SetupAreaNodeGroups(string designation, double[] locationData, string[] buildingData, string tagName) {
		foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
			double nodeGroupDistance = Math.Sqrt (Math.Pow (locationData [0] - (nodeGroup.GetNode (0).Latitude), 2) + Math.Pow (locationData [1] - (nodeGroup.GetNode (0).Longitude), 2));
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

				nodeGroup.NbFloor = int.Parse(buildingData[0]);
				nodeGroup.RoofAngle = int.Parse(buildingData[1]);
				nodeGroup.RoofType = buildingData[2];
			}
		}
	}

	private string[] AttributeBuildingInfo(XmlNode infoNode) {
		string[] res = new string[3];

		res [0] = this.AttributeValue(infoNode, XmlAttributes.NB_FLOOR);
		res [1] = this.AttributeValue (infoNode, XmlAttributes.ROOF_ANGLE);

		string roofType = this.AttributeValue (infoNode, XmlAttributes.ROOF_TYPE);
		res [2] = roofType.Equals("unknown") ? "" : roofType;

		return res;
	}

	private string[] AttributeHighwayInfo(XmlNode infoNode) {
		string[] res = new string[3];
		res [0] = this.AttributeValue (infoNode, XmlAttributes.ROAD_TYPE);
		res [1] = this.AttributeValue(infoNode, XmlAttributes.NB_WAY);
		res [2] = this.AttributeValue (infoNode, XmlAttributes.MAX_SPEED);
		return res;
	}

	private double[] AttributeLocationInfo(XmlNode infoNode) {
		double[] res = new double[3];
		res[0] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.LATITUDE));
		res[1] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.LONGIUDE));
		res[2] = double.Parse (this.AttributeValue(infoNode, XmlAttributes.DISTANCE));
		return res;
	}

	private string AttributeValue(XmlNode containerNode, string attributeName) {
		XmlNode attribute = containerNode.Attributes.GetNamedItem (attributeName);
		if (attribute != null)
			return attribute.InnerText;
		else
			return null;
	}

	/// <summary>
	/// Methode createResumeFile :
	/// Cree un fichier propre ou les informations sont resumees
	/// </summary>
	/// <param name="nameFile"> nom du fichier ou l'on inscrit les informations </param>
	public void GenerateResumeFile() {
		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";

		XmlDocument mapSettingsDocument = new XmlDocument (); 
		XmlDocument mapResumedDocument = new XmlDocument (); 

		if (File.Exists (mapSettingsFilePath)) {
			mapSettingsDocument.Load (mapSettingsFilePath);

			XmlDeclaration mapResumedDeclaration = mapResumedDocument.CreateXmlDeclaration ("1.0", "UTF-8", null);
			mapResumedDocument.AppendChild (mapResumedDeclaration);
			this.TransfertSettingsToResumed (mapResumedDocument, mapSettingsDocument, mapResumedDocument);

			XmlNode earthNode = mapResumedDocument.ChildNodes[1];
			if (earthNode != null) {
				XmlNode boundsNode = this.NewBoundsNode (mapResumedDocument);
				earthNode.InsertBefore (boundsNode, earthNode.FirstChild);

				foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
					string locationXPath = "";
					locationXPath += "/" + XmlTags.EARTH;
					locationXPath += "/" + XmlTags.COUNTRY + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
					locationXPath += "/" + XmlTags.REGION + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
					locationXPath += "/" + XmlTags.TOWN + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
					locationXPath += "/" + XmlTags.DISTRICT + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";

					XmlNode locationNode = mapResumedDocument.SelectSingleNode (locationXPath);

					if (locationNode != null) {
						string customObjectXPath = locationXPath + "/" + XmlTags.BUILDING + "/" + nodeGroup.Type () + "[@" + XmlAttributes.NAME + "=\"" + nodeGroup.Name + "\"]";
						XmlNode customObjectNode = mapResumedDocument.SelectSingleNode (customObjectXPath);

						XmlNode objectNode = null;
						XmlNode objectInfoNode = null;

						if (customObjectNode == null) {
							objectNode = mapResumedDocument.CreateElement (nodeGroup.Type ());
							objectInfoNode = mapResumedDocument.CreateElement (XmlTags.INFO);
							objectNode.AppendChild (objectInfoNode);
						} else {
							objectNode = customObjectNode;
							objectInfoNode = customObjectNode.FirstChild;
						}

						this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ID, nodeGroup.Id.ToString ());
						this.AddInternalNodes (mapResumedDocument, nodeGroup, objectNode);

						if (nodeGroup.IsBuilding ())
							this.AddBuildingNodeAttribute (mapResumedDocument, nodeGroup, objectNode, objectInfoNode);
						else if (nodeGroup.IsHighway () && !nodeGroup.IsTrafficLight ())
							this.AddHighwayNodeAttribute (mapResumedDocument, nodeGroup, objectNode, objectInfoNode);

						locationNode.AppendChild (objectNode);
					}
				}

				this.RemoveUnusedNodes (mapResumedDocument);

				mapResumedDocument.Save (mapResumedFilePath);
			}
		}
	}

	private XmlNode NewBoundsNode(XmlDocument document) {
		XmlNode res = document.CreateElement (XmlTags.BOUNDS);
		this.AppendAttribute (document, res, XmlAttributes.MIN_LATITUDE, minlat.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MIN_LONGITUDE, minlon.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MAX_LATITUDE, maxlat.ToString());
		this.AppendAttribute (document, res, XmlAttributes.MAX_LONGITUDE, maxlon.ToString());
		return res;
	}

	private void TransfertSettingsToResumed (XmlDocument targetDocument, XmlNode mapSettingsParentElement, XmlNode mapResumedParentElement) {
		foreach (XmlNode mapSettingsNode in mapSettingsParentElement) {
			XmlNode mapResumedNode = targetDocument.ImportNode (mapSettingsNode, true);
			if ((!mapSettingsNode.Name.Equals (XmlTags.INFO) || mapSettingsParentElement.Name.Equals(XmlTags.BUILDING)) && !mapResumedNode.Name.Equals (XmlTags.XML)) {
				mapResumedParentElement.AppendChild (mapResumedNode);
				mapResumedNode.InnerText = "";
				mapResumedNode.InnerXml = "";
				this.TransfertSettingsToResumed (targetDocument, mapSettingsNode, mapResumedNode);
			}
		}
	}

	private void RemoveUnusedNodes(XmlNode parentElement) {
		ArrayList oldNodes = new ArrayList(); 
		foreach(XmlNode dataNode in parentElement.ChildNodes) {
			if (!dataNode.Name.Equals(XmlTags.XML) && !dataNode.Name.Equals(XmlTags.BOUNDS)) {
				if (dataNode.ChildNodes.Count > 0 && dataNode.FirstChild.Name.Equals (XmlTags.INFO)) {
					if(dataNode.ChildNodes.Count == 1) {
						oldNodes.Add(dataNode);
					}
				} else {
					this.RemoveUnusedNodes(dataNode);
				}
			}
		}
		foreach(XmlNode oldNode in oldNodes)
			parentElement.RemoveChild (oldNode);
	}

	private void AddInternalNodes(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode) {
		for (int i = 0; i < nodeGroup.Nodes.Count; i++) {
			Node node = (Node)nodeGroup.Nodes [i];
			node.Index = i;

			XmlNode objectNd = mapResumedDocument.CreateElement (XmlTags.ND);
			objectNode.AppendChild (objectNd);

			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.INDEX, node.Index.ToString ());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.REFERENCE, node.Reference.ToString());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.LATITUDE, node.Latitude.ToString());
			this.AppendAttribute (mapResumedDocument, objectNd, XmlAttributes.LONGIUDE, node.Longitude.ToString());
		}
	}

	private void AddBuildingNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);
	}

	private void AddHighwayNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.ROAD_TYPE, nodeGroup.GetTagValue ("highway"));
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.NB_WAY, nodeGroup.NbWay.ToString());
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XmlAttributes.MAX_SPEED, nodeGroup.MaxSpeed.ToString());
	}

	private void AppendAttribute(XmlDocument boundingDocument, XmlNode containerNode, string attributeName, string attributeValue) {
		XmlAttribute attribute = boundingDocument.CreateAttribute (attributeName);
		attribute.Value = attributeValue;
		containerNode.Attributes.Append (attribute);
	}

	/// <summary>
	/// Metode readSettingFile :
	/// Lit un fichier resume
	/// </summary>
	/// <param name="nameFile"> nom du fichier resume a lire </param>
	public void LoadResumedData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument (); 

		ArrayList extractedNodes = new ArrayList ();
		objectBuilder.NodeGroups.Clear ();

		if (File.Exists (mapResumedFilePath)) {
			mapsSettingsDocument.Load (mapResumedFilePath);

			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];
			if (earthNode != null) {
				
				XmlNode boundsNode = earthNode.FirstChild;
				if (boundsNode != null) {
					minlat = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MIN_LATITUDE));
					minlon = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MIN_LONGITUDE));
					maxlat = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MAX_LATITUDE));
					maxlon = double.Parse (this.AttributeValue (boundsNode, XmlAttributes.MAX_LONGITUDE));

					string[] areas = new string[4] {
						XmlTags.COUNTRY,
						XmlTags.REGION,
						XmlTags.TOWN,
						XmlTags.DISTRICT,
					};

					earthNode.RemoveChild (earthNode.FirstChild);
					this.ExtractResumedNodes (earthNode, new string[4], areas, 0);
				}		
			}
		}
	}

	private void ExtractResumedNodes(XmlNode parentAreaNode, string[] areaDesignations, string[] areaTypes, int areaTypeIndex) {
		foreach (XmlNode dataNode in parentAreaNode.ChildNodes) {
			if (areaTypeIndex < areaTypes.Length) {
				areaDesignations [areaTypeIndex] = this.AttributeValue (dataNode, XmlAttributes.DESIGNATION);
				this.ExtractResumedNodes (dataNode, areaDesignations, areaTypes, areaTypeIndex + 1);
			} else {
				XmlNode areaInfosNode = dataNode.FirstChild;

				long id = long.Parse(this.AttributeValue (areaInfosNode, XmlAttributes.ID));

				NodeGroup nodeGroup = new NodeGroup (id);
				for (int i = 1; i < dataNode.ChildNodes.Count; i++) {
					XmlNode ndNode = dataNode.ChildNodes[i];

					long reference = long.Parse (this.AttributeValue(ndNode, XmlAttributes.REFERENCE));
					int index = int.Parse (this.AttributeValue(ndNode, XmlAttributes.INDEX));
					double latitude = double.Parse (this.AttributeValue(ndNode, XmlAttributes.LATITUDE));
					double longitude = double.Parse (this.AttributeValue(ndNode, XmlAttributes.LONGIUDE));
					Node node = new Node (reference, index, latitude, longitude);
					nodeGroup.AddNode (node);
				}

				nodeGroup.Country = areaDesignations [0];
				nodeGroup.Region = areaDesignations [1];
				nodeGroup.Town = areaDesignations [2];
				nodeGroup.District = areaDesignations [3];

				if (dataNode.Name.Equals (XmlTags.BUILDING)) {
					nodeGroup.Name = this.AttributeValue (areaInfosNode, XmlAttributes.NAME);

					string[] areaBuildingInfos = this.AttributeBuildingInfo (areaInfosNode);
					nodeGroup.NbFloor = int.Parse(areaBuildingInfos [0]);
					nodeGroup.RoofAngle = int.Parse(areaBuildingInfos [1]);
					nodeGroup.RoofType = areaBuildingInfos [2];

					nodeGroup.AddTag("building", "yes");
				} else if (dataNode.Name.Equals (XmlTags.HIGHWAY)) {
					nodeGroup.Name = this.AttributeValue (areaInfosNode, XmlAttributes.NAME);

					string[] areaHighwayInfos = this.AttributeHighwayInfo (areaInfosNode);
					nodeGroup.NbWay = int.Parse (areaHighwayInfos[1]);
					nodeGroup.MaxSpeed = int.Parse (areaHighwayInfos[2]);

					nodeGroup.AddTag("highway", areaHighwayInfos[0]);
				} else if (dataNode.Name.Equals (XmlTags.WATERWAY)) {

				} else if (dataNode.Name.Equals (XmlTags.TREE)) {
					nodeGroup.AddTag("natural", XmlTags.TREE);
				} else if (dataNode.Name.Equals (XmlTags.TRAFFIC_LIGHT)) {
					nodeGroup.AddTag("highway", XmlTags.TRAFFIC_LIGHT);
				}

				objectBuilder.NodeGroups.Add(nodeGroup);
			}
		}
	}

	public void LoadCustomData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		string mapCustomFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";

		XmlDocument mapResumedDocument = new XmlDocument ();
		XmlDocument mapCustomDocument = new XmlDocument ();

		if (File.Exists (mapResumedFilePath) && File.Exists (mapCustomFilePath)) {
			mapResumedDocument.Load (mapResumedFilePath);
			mapCustomDocument.Load (mapCustomFilePath);

			XmlNode earthNode = mapCustomDocument.ChildNodes [1];
			if (earthNode != null) {
				XmlNodeList customNodes = earthNode.ChildNodes;
				foreach (XmlNode customNode in customNodes) {
					XmlNode customInfoNode = customNode.FirstChild;
					string objectId = this.AttributeValue (customInfoNode, XmlAttributes.ID);

					XmlNode matchingResumedInfoNode = mapResumedDocument.SelectSingleNode("//" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + objectId + "\"]");
					if (matchingResumedInfoNode != null) {
						XmlNode matchingResumedNode = matchingResumedInfoNode.ParentNode;
						if (customNode.ChildNodes.Count == 1) {
							matchingResumedNode.RemoveChild (matchingResumedNode.FirstChild);
							XmlNode newResumedInfoNode = mapResumedDocument.ImportNode (customInfoNode, true);
							matchingResumedNode.InsertBefore (newResumedInfoNode, matchingResumedNode.FirstChild);
						} else {
							matchingResumedNode.RemoveAll ();
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
		get { return minlat; }
		set { minlat = value; }
	}

	public double Minlon {
		get { return minlon; }
		set { minlon = value; }
	}

	public double Maxlat {
		get { return maxlat; }
		set { maxlat = value; }
	}

	public double Maxlon {
		get { return maxlon; }
		set { maxlon = value; }
	}

	private static class MapLoaderHolder {
		public static MapLoader instance = new MapLoader();
	}
}