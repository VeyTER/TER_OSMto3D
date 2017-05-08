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

	// coordonnées min et max de la carte
	private double minlat;
	private double minlon;
	private double maxlat;
	private double maxlon;

	//creation d'une instance de ModificationPoint
	private PointEditor pointEditor;

	public FileManager() {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.minlat = 0;
		this.minlon = 0;
		this.maxlat = 0;
		this.maxlon = 0;

		this.pointEditor = new PointEditor();
	}

	//Constructeur 
	public FileManager(string path) {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.path = path;

		this.minlat = 0;
		this.minlon = 0;
		this.maxlat = 0;
		this.maxlon = 0;

		this.pointEditor = new PointEditor();
	}

	/// <summary>
	/// Methode readFileOSM :
	/// Permet d'extraire les données de fichier ".osm" et de les stocker dans des objet "node" et "nodeGroup". 
	/// </summary>
	/// <param name="nameMap"> nom du fichier ".osm" dont on doit extraire les infos </param>
	public void ReadOSMFile(string mapName, int mapNum) {
		string OSMFilePath = path + "Maps/" + mapName + ".osm";
		XmlDocument OSMDocument = new XmlDocument(); 

		if (File.Exists (OSMFilePath)) {
			OSMDocument.Load (OSMFilePath);

			XmlNodeList boundsNodes = OSMDocument.GetElementsByTagName (XMLTags.BOUNDS);
			XmlNodeList nodeNodes = OSMDocument.GetElementsByTagName (XMLTags.NODE);
			XmlNodeList wayNodes = OSMDocument.GetElementsByTagName (XMLTags.WAY);

			if (boundsNodes.Count > 0) {
				minlat = double.Parse (this.AttributeValue (boundsNodes [0], XMLAttributes.MIN_LATITUDE));
				minlon = double.Parse (this.AttributeValue (boundsNodes [0], XMLAttributes.MIN_LONGITUDE));
				maxlat = double.Parse (this.AttributeValue (boundsNodes [0], XMLAttributes.MAX_LATITUDE));
				maxlon = double.Parse (this.AttributeValue (boundsNodes [0], XMLAttributes.MAX_LONGITUDE));
			}

			ArrayList nodes = new ArrayList();
			foreach (XmlNode nodeNode in nodeNodes) {
				double id = double.Parse (this.AttributeValue (nodeNode, XMLAttributes.ID));
				double latitude = double.Parse(this.AttributeValue (nodeNode, XMLAttributes.LATITUDE));
				double longitude = double.Parse(this.AttributeValue (nodeNode, XMLAttributes.LONGIUDE));

				XmlNodeList tagNodes = nodeNode.ChildNodes;
				for (int i = 0; i < tagNodes.Count; i++) {
					if(tagNodes [i].Name.Equals(XMLTags.TAG)) {
						XmlNode tagNode = tagNodes [i];

						string key = this.AttributeValue (tagNode, XMLAttributes.KEY);
						string value = this.AttributeValue (tagNode, XMLAttributes.VALUE);

						if (key.Equals (XMLKeys.NATURAL) || key.Equals (XMLKeys.HIGHWAY)) {
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

			foreach (XmlNode wayNode in wayNodes) {
				double id = double.Parse (this.AttributeValue (wayNode, XMLAttributes.ID));

				NodeGroup nodeGroup = new NodeGroup (id);
				XmlNodeList ndNodes = wayNode.ChildNodes;
				for (int i = 0; i < ndNodes.Count; i++) {
					if(ndNodes[i].Name.Equals(XMLTags.ND)) {
						XmlNode ndNode = ndNodes [i];

						double reference = double.Parse (this.AttributeValue(ndNode, XMLAttributes.REFERENCE));

						int j = 0;
						for (; j < nodes.Count && ((Node)nodes [j]).Id != reference; j++);
						if(j < nodes.Count)
							nodeGroup.AddNode ((Node)nodes[j]);
					} else if(ndNodes[i].Name.Equals(XMLTags.TAG)) {
						XmlNode tagNode = ndNodes [i];

						string key = this.AttributeValue (tagNode, XMLAttributes.KEY);
						string value = this.AttributeValue (tagNode, XMLAttributes.VALUE);

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
	public void ReadSettingsFile() {
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

	private void FillNodeGroups(XmlNode boundingAreaNode, string[] areaTypes, int areaTypeIndex) {
		for (int i = 1; i < boundingAreaNode.ChildNodes.Count; i++) {
			if (boundingAreaNode.ChildNodes[i].Name.Equals (areaTypes[areaTypeIndex])) {
				XmlNode areaNode = boundingAreaNode.ChildNodes [i];
				string areaDesignation = this.AttributeValue (areaNode, XMLAttributes.DESIGNATION);

				XmlNode areaInfosNode = areaNode.FirstChild;
				double[] areaLocationInfos = this.AttributeLocationInfo (areaInfosNode);
				string[] areaBuildingInfos = this.AttributeBuildingInfo (areaInfosNode);

				this.SetupAreaNodeGroups (areaDesignation, areaLocationInfos, areaBuildingInfos, areaTypes[areaTypeIndex]);

				if (areaTypeIndex < areaTypes.Length)
					this.FillNodeGroups (areaNode, areaTypes, areaTypeIndex + 1);
			}
		}
	}

	private string[] AttributeBuildingInfo(XmlNode infoNode) {
		string[] res = new string[3];

		res [0] = this.AttributeValue(infoNode, XMLAttributes.NB_FLOOR);
		res [1] = this.AttributeValue (infoNode, XMLAttributes.ROOF_ANGLE);

		string roofType = this.AttributeValue (infoNode, XMLAttributes.ROOF_TYPE);
		res [2] = roofType.Equals("unknown") ? "" : roofType;

		return res;
	}

	private string[] AttributeHighwayInfo(XmlNode infoNode) {
		string[] res = new string[3];
		res [0] = this.AttributeValue (infoNode, XMLAttributes.ROAD_TYPE);
		res [1] = this.AttributeValue(infoNode, XMLAttributes.NB_WAY);
		res [2] = this.AttributeValue (infoNode, XMLAttributes.MAX_SPEED);
		return res;
	}

	private double[] AttributeLocationInfo(XmlNode infoNode) {
		double[] res = new double[3];
		res[0] = double.Parse (this.AttributeValue(infoNode, XMLAttributes.LATITUDE));
		res[1] = double.Parse (this.AttributeValue(infoNode, XMLAttributes.LONGIUDE));
		res[2] = double.Parse (this.AttributeValue(infoNode, XMLAttributes.DISTANCE));
		return res;
	}

	private string AttributeValue(XmlNode containerNode, string attributeName) {
		XmlNode attribute = containerNode.Attributes.GetNamedItem (attributeName);
		if (attribute != null)
			return attribute.InnerText;
		else
			return null;
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
	public void CreateResumeFile() {
		string mapSettingsFilePath = path + "Maps Settings/map_settings.osm";
		string mapResumedFilePath = path + "Maps Resumed/map_resumed.osm";

		XmlDocument mapSettingsDocument = new XmlDocument (); 
		XmlDocument mapResumedDocument = new XmlDocument (); 

		if (File.Exists (mapSettingsFilePath)) {
			mapSettingsDocument.Load (mapSettingsFilePath);

			XmlDeclaration mapResumedDeclaration = mapResumedDocument.CreateXmlDeclaration ("1.0", "UTF-8", null);
			mapResumedDocument.AppendChild (mapResumedDeclaration);
			this.TransfertNodes (mapResumedDocument, mapSettingsDocument, mapResumedDocument);

			XmlNode earthNode = mapResumedDocument.ChildNodes[1];

			if (earthNode != null) {
				XmlNode boundsNode = this.NewBoundsNode (mapResumedDocument);
				earthNode.InsertBefore (boundsNode, earthNode.FirstChild);

				foreach (NodeGroup nodeGroup in objectBuilder.NodeGroups) {
					string locationXPath = "";
					locationXPath += "/" + XMLTags.EARTH;
					locationXPath += "/" + XMLTags.COUNTRY + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
					locationXPath += "/" + XMLTags.REGION + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
					locationXPath += "/" + XMLTags.TOWN + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
					locationXPath += "/" + XMLTags.DISTRICT + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";

					XmlNode locationNode = mapResumedDocument.SelectSingleNode (locationXPath);

					if (locationNode != null) {
						string customObjectXPath = locationXPath + "/" + nodeGroup.Type () + "[@" + XMLAttributes.DESIGNATION + "=\"" + nodeGroup.Name + "\"]";
						XmlNode customObjectNode = mapResumedDocument.SelectSingleNode (customObjectXPath);

						XmlNode objectNode = null;
						XmlNode objectInfoNode = null;

						if (customObjectNode == null) {
							objectNode = mapResumedDocument.CreateElement (nodeGroup.Type ());
							objectInfoNode = mapResumedDocument.CreateElement (XMLTags.INFO);
							objectNode.AppendChild (objectInfoNode);
						} else {
							objectNode = customObjectNode;
							objectInfoNode = customObjectNode.FirstChild;
						}

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

				this.RemoveUnusedNodes (mapResumedDocument);

				mapResumedDocument.Save (mapResumedFilePath);
			}
		}
	}

	private XmlNode NewBoundsNode(XmlDocument document) {
		XmlNode res = document.CreateElement (XMLTags.BOUNDS);
		this.AppendAttribute (document, res, XMLAttributes.MIN_LATITUDE, minlat + "");
		this.AppendAttribute (document, res, XMLAttributes.MIN_LONGITUDE, minlon + "");
		this.AppendAttribute (document, res, XMLAttributes.MAX_LATITUDE, maxlat + "");
		this.AppendAttribute (document, res, XMLAttributes.MAX_LONGITUDE, maxlon + "");
		return res;
	}

	private void TransfertNodes (XmlDocument targetDocument, XmlNode mapSettingsParentElement, XmlNode mapResumedParentElement) {
		foreach (XmlNode mapSettingsNode in mapSettingsParentElement) {
			XmlNode mapResumedNode = targetDocument.ImportNode (mapSettingsNode, true);
			if ((!mapSettingsNode.Name.Equals (XMLTags.INFO) || mapSettingsParentElement.Name.Equals(XMLTags.BUILDING)) && !mapResumedNode.Name.Equals (XMLTags.XML)) {
				mapResumedParentElement.AppendChild (mapResumedNode);
				mapResumedNode.InnerText = "";
				mapResumedNode.InnerXml = "";
				this.TransfertNodes (targetDocument, mapSettingsNode, mapResumedNode);
			}
		}
	}

	private void RemoveUnusedNodes(XmlNode parentElement) {
		ArrayList oldNodes = new ArrayList(); 
		foreach(XmlNode dataNode in parentElement.ChildNodes) {
			if (!dataNode.Name.Equals(XMLTags.XML) && !dataNode.Name.Equals(XMLTags.BOUNDS)) {
				if (dataNode.ChildNodes.Count > 0 && dataNode.FirstChild.Name.Equals (XMLTags.INFO)) {
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
		foreach (Node node in nodeGroup.Nodes) {
			XmlNode objectNd = mapResumedDocument.CreateElement (XMLTags.ND);
			objectNode.AppendChild (objectNd);

			this.AppendAttribute (mapResumedDocument, objectNd, XMLAttributes.ID, node.Id + "");
			this.AppendAttribute (mapResumedDocument, objectNd, XMLAttributes.LATITUDE, node.Latitude + "");
			this.AppendAttribute (mapResumedDocument, objectNd, XMLAttributes.LONGIUDE, node.Longitude + "");
		}
	}

	private void AddBuildingNodeInfo (XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.NB_FLOOR, nodeGroup.NbFloor + "");
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.ROOF_ANGLE, nodeGroup.RoofAngle + "");
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.ROOF_TYPE, nodeGroup.RoofType);
	}

	private void AddHighwayNodeInfo (XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectNode, XmlNode objectInfoNode) {
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.NAME, nodeGroup.Name);
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.ROAD_TYPE, nodeGroup.GetTagValue ("highway"));
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.NB_WAY, nodeGroup.NbWay + "");
		this.AppendAttribute (mapResumedDocument, objectInfoNode, XMLAttributes.MAX_SPEED, nodeGroup.MaxSpeed + "");
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
	public void ReadResumeFile() {
		string mapResumedFilePath = path + "Maps Resumed/map_resumed.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument (); 

		ArrayList extractedNodes = new ArrayList ();
		objectBuilder.NodeGroups.Clear ();
		if (File.Exists (mapResumedFilePath)) {
			mapsSettingsDocument.Load (mapResumedFilePath);

			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];
			if (earthNode != null) {
				
				XmlNode boundsNode = earthNode.FirstChild;
				if (boundsNode != null) {
					minlat = double.Parse (this.AttributeValue (boundsNode, XMLAttributes.MIN_LATITUDE));
					minlon = double.Parse (this.AttributeValue (boundsNode, XMLAttributes.MIN_LONGITUDE));
					maxlat = double.Parse (this.AttributeValue (boundsNode, XMLAttributes.MAX_LATITUDE));
					maxlon = double.Parse (this.AttributeValue (boundsNode, XMLAttributes.MAX_LONGITUDE));

					string[] areas = new string[4] {
						XMLTags.COUNTRY,
						XMLTags.REGION,
						XMLTags.TOWN,
						XMLTags.DISTRICT,
					};

					earthNode.RemoveChild (earthNode.FirstChild);
					this.ExtractNodes (earthNode, new string[4], areas, 0);
				}		
			}
		}
	}

	private void ExtractNodes(XmlNode parentAreaNode, string[] areaDesignations, string[] areaTypes, int areaTypeIndex) {
		foreach (XmlNode dataNode in parentAreaNode.ChildNodes) {
			if (areaTypeIndex < areaTypes.Length) {
				areaDesignations [areaTypeIndex] = this.AttributeValue (dataNode, XMLAttributes.DESIGNATION);
				this.ExtractNodes (dataNode, areaDesignations, areaTypes, areaTypeIndex + 1);
			} else {
				XmlNode areaInfosNode = dataNode.FirstChild;

				double id = double.Parse(this.AttributeValue (areaInfosNode, XMLAttributes.ID));

				NodeGroup nodeGroup = new NodeGroup (id);
				for (int i = 1; i < dataNode.ChildNodes.Count; i++) {
					XmlNode ndNode = dataNode.ChildNodes[i];

					double latitude = double.Parse (this.AttributeValue(ndNode, XMLAttributes.LATITUDE));
					double longitude = double.Parse (this.AttributeValue(ndNode, XMLAttributes.LONGIUDE));
					Node node = new Node (id, latitude, longitude);
					nodeGroup.AddNode (node);
				}

				nodeGroup.Country = areaDesignations [0];
				nodeGroup.Region = areaDesignations [1];
				nodeGroup.Town = areaDesignations [2];
				nodeGroup.District = areaDesignations [3];

				if (dataNode.Name.Equals (XMLTags.BUILDING)) {
					nodeGroup.Name = this.AttributeValue (areaInfosNode, XMLAttributes.NAME);

					string[] areaBuildingInfos = this.AttributeBuildingInfo (areaInfosNode);
					nodeGroup.NbFloor = int.Parse(areaBuildingInfos [0]);
					nodeGroup.RoofAngle = int.Parse(areaBuildingInfos [1]);
					nodeGroup.RoofType = areaBuildingInfos [2];

					nodeGroup.AddTag("building", "yes");
				} else if (dataNode.Name.Equals (XMLTags.HIGHWAY)) {
					nodeGroup.Name = this.AttributeValue (areaInfosNode, XMLAttributes.NAME);

					string[] areaHighwayInfos = this.AttributeHighwayInfo (areaInfosNode);
					nodeGroup.NbWay = int.Parse (areaHighwayInfos[1]);
					nodeGroup.MaxSpeed = int.Parse (areaHighwayInfos[2]);

					nodeGroup.AddTag("highway", areaHighwayInfos[0]);
				} else if (dataNode.Name.Equals (XMLTags.WATERWAY)) {

				} else if (dataNode.Name.Equals (XMLTags.TREE)) {
					nodeGroup.AddTag("natural", XMLTags.TREE);
				} else if (dataNode.Name.Equals (XMLTags.TRAFFIC_LIGHT)) {
					nodeGroup.AddTag("highway", XMLTags.TRAFFIC_LIGHT);
				}

				objectBuilder.NodeGroups.Add(nodeGroup);
			}
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
}