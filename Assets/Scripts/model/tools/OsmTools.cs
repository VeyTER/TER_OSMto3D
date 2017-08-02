using UnityEngine;
using UnityEditor;
using System.Xml;

public class OsmTools {
	private static OsmTools instance;

	private OsmTools() { }

	public static OsmTools GetInstance() {
		if (instance == null)
			instance = new OsmTools();
		return instance;
	}

	/// <summary>
	/// 	Genère un noeud XML contenant les coordonnées minimales et maximales du terrain.
	/// </summary>
	/// <returns>Noeud XML contenant les coordonnées minimales et maximales du terrain.</returns>
	/// <param name="document">Fichier XML dans lequel ajouter les bornes du terrain.</param>
	public XmlNode NewBoundsNode(XmlDocument document, double[] bounds) {
		// Ajout d'un nouveau noeud XML au document et ajout des informations à ce noeud.
		XmlNode res = document.CreateElement(XmlTags.BOUNDS);
		this.AppendAttribute(document, res, XmlAttributes.MIN_LATITUDE, bounds[0].ToString());
		this.AppendAttribute(document, res, XmlAttributes.MIN_LONGITUDE, bounds[1].ToString());
		this.AppendAttribute(document, res, XmlAttributes.MAX_LATITUDE, bounds[2].ToString());
		this.AppendAttribute(document, res, XmlAttributes.MAX_LONGITUDE, bounds[3].ToString());
		return res;
	}

	public string MatchingZoningPath(XmlDocument mapResumed, XmlNode parentNode, NodeGroup nodeGroup, string[] areas, int areaLevel, string totalXPath) {
		string areaDesignation = "";

		switch (areas[areaLevel]) {
		case XmlTags.COUNTRY:
			areaDesignation = nodeGroup.Country;
			break;
		case XmlTags.REGION:
			areaDesignation = nodeGroup.Region;
			break;
		case XmlTags.TOWN:
			areaDesignation = nodeGroup.Town;
			break;
		case XmlTags.DISTRICT:
			areaDesignation = nodeGroup.District;
			break;
		}

		string localXPath = "/" + areas[areaLevel] + "[@" + XmlAttributes.DESIGNATION + "=\"" + areaDesignation + "\"]";
		XmlNode zoningNode = mapResumed.SelectSingleNode(totalXPath + localXPath);

		if (zoningNode != null && areaLevel < areas.Length - 1)
			return localXPath + this.MatchingZoningPath(mapResumed, zoningNode, nodeGroup, areas, areaLevel + 1, totalXPath + localXPath);
		else if (zoningNode != null && areaLevel >= areas.Length - 1)
			return localXPath;
		else
			return "";
	}

	/// <summary>
	/// 	Extrait les caractéristiques par défaut dans une certaine zone pour tous les bâtiments s'y trouvant.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques par défaut sur les bâtiments.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans les attributs du noeud XML.</param>
	public string[] BuildingInfo(XmlNode infoNode) {
		string[] res = new string[5];

		res[0] = this.AttributeValue(infoNode, XmlAttributes.NB_FLOOR);
		res[1] = this.AttributeValue(infoNode, XmlAttributes.ROOF_ANGLE);

		string roofShape = this.AttributeValue(infoNode, XmlAttributes.ROOF_SHAPE);
		res[2] = roofShape.Equals("unknown") ? "" : roofShape;

		res[3] = this.AttributeValue(infoNode, XmlAttributes.CUSTOM_MATERIAL);
		res[4] = this.AttributeValue(infoNode, XmlAttributes.OVERLAY_COLOR);

		return res;
	}

	/// <summary>
	/// 	Extrait les caractéristiques par défaut dans une certaine zone pour toutes les routes s'y trouvant.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques sur les routes.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans ses attributs.</param>
	public string[] HighwayInfo(XmlNode infoNode) {
		string[] res = new string[3];
		res[0] = this.AttributeValue(infoNode, XmlAttributes.ROAD_TYPE);
		res[1] = this.AttributeValue(infoNode, XmlAttributes.NB_WAY);
		res[2] = this.AttributeValue(infoNode, XmlAttributes.MAX_SPEED);
		return res;
	}

	public string[] LeisureInfo(XmlNode infoNode) {
		string[] res = new string[1];
		res[0] = this.AttributeValue(infoNode, XmlAttributes.LEISURE_TYPE);
		return res;
	}

	/// <summary>
	/// 	Extrait les caractéristiques de localisation d'une certaine zone.
	/// </summary>
	/// <returns>Tableau contenant les caractéristiques de localisation.</returns>
	/// <param name="infoNode">Noeud XML contenant les informations à extraire dans ses attributs.</param>
	public double[] AttributeLocationInfo(XmlNode infoNode) {
		double[] res = new double[3];

		res[0] = double.Parse(this.AttributeValue(infoNode, XmlAttributes.LATITUDE));
		res[1] = double.Parse(this.AttributeValue(infoNode, XmlAttributes.LONGIUDE));

		string distance = this.AttributeValue(infoNode, XmlAttributes.DISTANCE);
		res[2] = (distance == null) ? double.NaN : double.Parse(distance);

		return res;
	}


	/// <summary>
	/// 	Extrait la valeur d'un attribut identifié par son nom dans un certain noeud XML.
	/// </summary>
	/// <returns>Valeur extraite de l'attribut.</returns>
	/// <param name="containerNode">Noeud XML contenant l'attribut.</param>
	/// <param name="attributeName">Nom de l'attribut dont on veut connaître la valeur.</param>
	public string AttributeValue(XmlNode containerNode, string attributeName) {
		XmlNode attribute = containerNode.Attributes.GetNamedItem(attributeName);
		if (attribute != null)
			return attribute.InnerText;
		else
			return null;
	}

	public void AddCommonNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
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
	public void AddBuildingNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
			BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NAME, nodeGroup.Name);
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NB_FLOOR, buildingNodeGroup.NbFloor.ToString());
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_ANGLE, buildingNodeGroup.RoofAngle.ToString());
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.ROOF_SHAPE, buildingNodeGroup.RoofShape);

			if (buildingNodeGroup.CustomMaterial != null)
				this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.CUSTOM_MATERIAL, buildingNodeGroup.CustomMaterial.name);

			Color overlayColor = buildingNodeGroup.OverlayColor;
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.OVERLAY_COLOR, overlayColor.r + ";" + overlayColor.g + ";" + overlayColor.b);
		}
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
	public void AddHighwayNodeAttribute(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		if (nodeGroup.GetType() == typeof(HighwayNodeGroup)) {
			HighwayNodeGroup highwayNodeGroup = (HighwayNodeGroup) nodeGroup;
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NAME, highwayNodeGroup.Name);
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.ROAD_TYPE, highwayNodeGroup.GetTagValue("highway"));
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NB_WAY, highwayNodeGroup.NbWay.ToString());
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.MAX_SPEED, highwayNodeGroup.MaxSpeed.ToString());
		}
	}

	public void AddLeisureNodeAttributeInfo(XmlDocument mapResumedDocument, NodeGroup nodeGroup, XmlNode objectInfoNode) {
		if (nodeGroup.GetType() == typeof(LeisureNodeGroup)) {
			LeisureNodeGroup highwayNodeGroup = (LeisureNodeGroup) nodeGroup;
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.NAME, highwayNodeGroup.Name);
			this.AppendAttribute(mapResumedDocument, objectInfoNode, XmlAttributes.LEISURE_TYPE, highwayNodeGroup.GetTagValue("leisure"));
		}
	}

	/// <summary>
	/// 	Ajoute un attribut au noeud XML d'un certain document XML.
	/// </summary>
	/// <param name="boundingDocument">Document dans lequel se trouve le noeud XML à modifier.</param>
	/// <param name="containerNode">Noeud XML à modifier.</param>
	/// <param name="attributeName">Nom du nouvel attribut à ajouter.</param>
	/// <param name="attributeValue">Valeur du nouvel attribut à ajouter.</param>
	public void AppendAttribute(XmlDocument boundingDocument, XmlNode containerNode, string attributeName, string attributeValue) {
		// Création d'un nouvel attribut au noeud XML et changement de sa valeur
		XmlAttribute attribute = boundingDocument.CreateAttribute(attributeName);
		attribute.Value = attributeValue;

		// Ajout de cet attribut au noeud XML cible
		containerNode.Attributes.Append(attribute);
	}
}