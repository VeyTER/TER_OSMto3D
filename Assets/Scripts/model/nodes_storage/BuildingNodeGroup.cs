﻿using UnityEngine;
using UnityEditor;

public class BuildingNodeGroup : NodeGroup {
	private int nbFloor;

	private string roofShape;
	private int roofAngle;

	private Material customMaterial;
	private Color overlayColor;

	public BuildingNodeGroup(string id) : base(id, "building") {
		this.nbFloor = 1;

		this.roofShape = "unknown";
		this.roofAngle = 0;

		this.customMaterial = null;
		this.overlayColor = Color.white;
	}

	public BuildingNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		type = "building";
	}

	public BuildingNodeGroup(string id, string name, string country, string region, string town, string district, string roofShape, int roofAngle, Material customMaterial, Color overlayColor) :
		base(id, "building", name, country, region, town, district) {
		this.nbFloor = 1;

		this.roofAngle = roofAngle;
		this.roofShape = roofShape;

		this.customMaterial = customMaterial;
		this.overlayColor = overlayColor;
	}

	public string ToZoningXPath() {
		string zoningXPath = "/" + XmlTags.EARTH;

		if (country != "unknown")
			zoningXPath += "/" + XmlTags.COUNTRY + "[@" + XmlAttributes.DESIGNATION + "=\"" + country + "\"]";
		if (region != "unknown")
			zoningXPath += "/" + XmlTags.REGION + "[@" + XmlAttributes.DESIGNATION + "=\"" + region + "\"]";
		if (town != "unknown")
			zoningXPath += "/" + XmlTags.TOWN + "[@" + XmlAttributes.DESIGNATION + "=\"" + town + "\"]";
		if (district != "unknown")
			zoningXPath += "/" + XmlTags.DISTRICT + "[@" + XmlAttributes.DESIGNATION + "=\"" + district + "\"]";

		return zoningXPath;
	}

	public string ToFullBuildingXPath() {
		string zoningXPath = this.ToZoningXPath();

		zoningXPath += "/" + XmlTags.BUILDING;
		zoningXPath += "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + id + "\"]";
		return zoningXPath;
	}

	public int NbFloor {
		get { return nbFloor; }
		set { nbFloor = value; }
	}

	public string RoofShape {
		get { return roofShape; }
		set { roofShape = value; }
	}

	public int RoofAngle {
		get { return roofAngle; }
		set { roofAngle = value; }
	}

	public Material CustomMaterial {
		get { return customMaterial; }
		set { customMaterial = value; }
	}

	public Color OverlayColor {
		get { return overlayColor; }
		set { overlayColor = value; }
	}
}