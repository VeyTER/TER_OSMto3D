﻿using System;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class BuildingsTools {
	private GameObject selectedBuilding;

	private ObjectBuilder objectBuilder;

	private BuildingsTools () {
		this.selectedBuilding = null;

		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public static BuildingsTools GetInstance() {
		return BuildingsToolsInstanceHolder.instance;
	}

	public void DiscolorAll() {
		foreach (Transform currentBuildingGo in objectBuilder.WallGroups.transform) {
			foreach (Transform currentWallGo in currentBuildingGo.transform) {
				Renderer meshRenderer = currentWallGo.GetComponent<Renderer> ();
				if (meshRenderer.materials.Length != 1) {
					Material firstMaterial = meshRenderer.materials [0];
					meshRenderer.materials = new Material[] {
						firstMaterial
					};
				}
			}
		}
	}

	public void ColorAsSelected(GameObject building) {
		Material wallMaterial = Resources.Load ("Materials/mur") as Material;
		Material selectedElementMaterial = Resources.Load ("Materials/element_selectionne") as Material;

		foreach (Transform wallGo in building.transform) {
			Renderer meshRenderer = wallGo.GetComponent<Renderer> ();
			if (meshRenderer != null) {
				meshRenderer.materials = new Material[] {
					wallMaterial,
					selectedElementMaterial
				};
			}
		}
	}

	public int GetBuildingHeight(GameObject building) {
		string resumeFilePath = Application.dataPath + @"/MapsResumed/mapResumed.osm";
		XmlDocument mapResumeDocument = new XmlDocument(); 

		XmlAttribute floorAttribute = this.BuildingAttribute (ref mapResumeDocument, resumeFilePath, building, "nbFloor");
		if (floorAttribute != null)
			return int.Parse(floorAttribute.Value);
		else
			return -1;
	}

	public void DecrementSelectedBuildingHeight() {
		int nbFloors = GetBuildingHeight(SelectedBuilding);
		this.ChangeBuildingHeight (SelectedBuilding, nbFloors - 1);
	}
	public void IncrementSelectedBuildingHeight() {
		int nbFloors = GetBuildingHeight(selectedBuilding);
		this.ChangeBuildingHeight (SelectedBuilding, nbFloors + 1);
	}

	public void ChangeSelectedBuildingHeight(int nbFloors) {
		this.ChangeBuildingHeight (selectedBuilding, nbFloors);
	}

	public void ChangeBuildingHeight(GameObject building, int nbFloors) {
		string resumeFilePath = Application.dataPath + @"/MapsResumed/mapResumed.osm";
		XmlDocument mapResumeDocument = new XmlDocument(); 
		
		XmlAttribute floorAttribute = this.BuildingAttribute (ref mapResumeDocument, resumeFilePath, building, "nbFloor");
		if (floorAttribute != null)
			floorAttribute.Value = Math.Max(nbFloors, 1) + "";

		mapResumeDocument.Save (resumeFilePath);

		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance();
		objectBuilder.EditUniqueBuilding (building, nbFloors);
	}

	public XmlAttribute BuildingAttribute(ref XmlDocument xmlDocument, string filePath, GameObject building, string attributeKey) {
		string buildingIdentifier = building.name;

		XmlNode buildingInfoNode = null;
		if (File.Exists (filePath)) {
			xmlDocument.Load (filePath); 
			XmlNodeList infosList = xmlDocument.GetElementsByTagName ("Info");

			foreach (XmlNode infosNode in infosList) {
				string nodeGroupId = infosNode.Attributes.GetNamedItem ("id").InnerText;
				XmlNode nameAttribute = infosNode.Attributes.GetNamedItem ("name");
				if (nameAttribute != null) {
					string nodeGroupName = nameAttribute.InnerText;
					if (nodeGroupId.Equals (buildingIdentifier) || nodeGroupName.Equals (buildingIdentifier)) {
						buildingInfoNode = infosNode;
						break;
					}
				}
			}
		}

		return buildingInfoNode.Attributes ["nbFloor"];
	}

	public NodeGroup WallToBuildingNodeGroup(GameObject wallGo) {
		string identifier = wallGo.name.Substring (0, wallGo.name.LastIndexOf ("_"));
		NodeGroup buildingNgp = null;

		double numIdentifier;
		bool parsingSuccess = double.TryParse (identifier, out numIdentifier);

		foreach (NodeGroup ngp in objectBuilder.NodeGroups) {
			if ((!parsingSuccess && ngp.Name.Equals(identifier)) || (parsingSuccess && ngp.Id == numIdentifier)) {
				buildingNgp = ngp;
				break;
			}
		}

		return buildingNgp;
	}

	public GameObject NodeGroupToGameObject(NodeGroup buildingNgp) {
		string identifier = buildingNgp.Name;

		GameObject res = null;

		foreach (Transform buildingGo in objectBuilder.WallGroups.transform) {
			if (buildingGo.name.Equals(identifier)) {
				res = buildingGo.gameObject;
				break;
			}
		}

		return res;
	}

	public NodeGroup GameObjectToNodeGroup(GameObject buildingGo) {
		string identifier = buildingGo.name;

		NodeGroup res = null;

		double numIdentifier;
		bool parsingSuccess = double.TryParse (identifier, out numIdentifier);

		foreach (NodeGroup ngp in objectBuilder.NodeGroups) {
			if ((!parsingSuccess && ngp.Name.Equals(identifier)) || (parsingSuccess && ngp.Id == numIdentifier)) {
				res = ngp;
				break;
			}
		}

		return res;
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public static class BuildingsToolsInstanceHolder {
		public static BuildingsTools instance = new BuildingsTools();
	}
}
