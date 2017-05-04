using System;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;

public class BuildingsTools {
	private ObjectBuilder objectBuilder;

	private BuildingsTools () {
		this.objectBuilder = ObjectBuilder.GetInstance ();
	}

	public static BuildingsTools GetInstance() {
		return BuildingsToolsInstanceHolder.instance;
	}

	public void ChangeBuildingName(string newName) {
		InputField[] textInputs = GameObject.FindObjectsOfType<InputField> ();

		int i = 0;
		for (; i < textInputs.Length && textInputs[i].name.Equals (UINames.BUILDING_NAME_TEXT_INPUT); i++);
		if (i < textInputs.Length)
			textInputs[i].text = newName;
	}

	public void DiscolorAllBuildings() {
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
		Material wallMaterial = Resources.Load ("Materials/Wall") as Material;
		Material selectedElementMaterial = Resources.Load ("Materials/SelectedElement") as Material;

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
		string resumeFilePath = Application.dataPath + @"/Maps Resumed/map_resumed.osm";
		XmlDocument mapResumeDocument = new XmlDocument(); 

		XmlAttribute floorAttribute = this.BuildingAttribute (mapResumeDocument, resumeFilePath, building, "nbFloor");
		if (floorAttribute != null)
			return int.Parse(floorAttribute.Value);
		else
			return -1;
	}

	public void DecrementBuildingHeight(GameObject buildingGo) {
		int nbFloors = GetBuildingHeight(buildingGo);
		this.ChangeBuildingHeight (buildingGo, nbFloors - 1);
	}
	public void IncrementBuildingHeight(GameObject buildingGo) {
		int nbFloors = GetBuildingHeight(buildingGo);
		this.ChangeBuildingHeight (buildingGo, nbFloors + 1);
	}

	public void ChangeBuildingHeight(GameObject building, int nbFloors) {
		string resumeFilePath = Application.dataPath + @"/Maps Resumed/map_resumed.osm";
		XmlDocument mapResumeDocument = new XmlDocument(); 
		
		XmlAttribute floorAttribute = this.BuildingAttribute (mapResumeDocument, resumeFilePath, building, "nbFloor");
		if (floorAttribute != null)
			floorAttribute.Value = Math.Max(nbFloors, 1) + "";

		mapResumeDocument.Save (resumeFilePath);

		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance();
		objectBuilder.EditUniqueBuilding (building, nbFloors);
	}

	public XmlAttribute BuildingAttribute(XmlDocument xmlDocument, string filePath, GameObject building, string attributeKey) {
		string buildingIdentifier = building.name;

		XmlNode res = null;

		if (File.Exists (filePath)) {
			xmlDocument.Load (filePath); 
			XmlNodeList infosList = xmlDocument.GetElementsByTagName ("info");

			foreach (XmlNode infosNode in infosList) {
				string nodeGroupId = infosNode.Attributes.GetNamedItem ("id").InnerText;
				XmlNode nameAttribute = infosNode.Attributes.GetNamedItem ("name");
				if (nameAttribute != null) {
					string nodeGroupName = nameAttribute.InnerText;
					if (nodeGroupId.Equals (buildingIdentifier) || nodeGroupName.Equals (buildingIdentifier)) {
						res = infosNode;
						break;
					}
				}
			}
		}

		return res.Attributes ["nbFloor"];
	}

	public NodeGroup WallToBuildingNodeGroup(GameObject wallGo) {
		string identifier = wallGo.name.Substring (0, wallGo.name.LastIndexOf ("_"));

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

	public Vector3 BuildingCenter(GameObject building) {
		Vector3 positionsSum = new Vector3 (0, 0, 0);

		foreach (Transform wallTransform in building.transform)
			positionsSum += wallTransform.position;
		
		return positionsSum / (building.transform.childCount * 1F);
	}

	public double BuildingRadius(GameObject building) {
		Vector3 buildingCenter = this.BuildingCenter (building);

		double maxDistance = 0;
		foreach (Transform wallTransform in building.transform) {
			double currentDistance = Vector3.Distance (buildingCenter, wallTransform.position);
			if (currentDistance > maxDistance)
				maxDistance = currentDistance;
		}

		return maxDistance;
	}

	public static class BuildingsToolsInstanceHolder {
		public static BuildingsTools instance = new BuildingsTools();
	}
}
