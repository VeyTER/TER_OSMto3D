using System;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public class BuildingsTools {
	private ObjectBuilder objectBuilder;

	private string resumeFilePath;
	private XmlDocument mapResumeDocument;

	private BuildingsTools () {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.resumeFilePath = Application.dataPath + @"/Maps resumed/map_resumed.osm";
		this.mapResumeDocument = new XmlDocument();
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

	public void SetName(GameObject building, string newName) {
		NodeGroup nodeGroup = this.GameObjectToNodeGroup (building);
		XmlAttribute nameAttribute = this.GetNodeGroupAttribute (nodeGroup, XmlAttributes.NAME);

		nodeGroup.Name = newName;
		building.name = newName;
		for (int i = 0; i < building.transform.childCount; i++)
			building.transform.GetChild (i).name = newName + "_mur_" + i;
		
		nameAttribute.Value = newName;
		mapResumeDocument.Save (resumeFilePath);
	}

	public int GetHeight(GameObject building) {
		NodeGroup nodeGroup = this.GameObjectToNodeGroup (building);
		XmlAttribute floorAttribute = this.GetNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
		if (floorAttribute != null)
			return int.Parse(floorAttribute.Value);
		else
			return -1;
	}

	public void DecrementHeight(GameObject buildingGo) {
		int nbFloors = GetHeight(buildingGo);
		this.SetHeight (buildingGo, nbFloors - 1);
	}
	public void IncrementHeight(GameObject buildingGo) {
		int nbFloors = GetHeight(buildingGo);
		this.SetHeight (buildingGo, nbFloors + 1);
	}

	public void SetHeight(GameObject building, int nbFloors) {
		NodeGroup nodeGroup = this.GameObjectToNodeGroup (building);
		XmlAttribute floorAttribute = this.GetNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
		if (floorAttribute != null)
			floorAttribute.Value = Math.Max(nbFloors, 1).ToString();

		mapResumeDocument.Save (resumeFilePath);

		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance();
		objectBuilder.EditUniqueBuilding (building, nbFloors);
	}

	public XmlAttribute GetNodeGroupAttribute(NodeGroup nodeGroup, string attributeName) {
		XmlAttribute res = null;
		if (File.Exists (resumeFilePath)) {
			mapResumeDocument.Load (resumeFilePath); 

			string locationXPath = "";
			locationXPath += "/" + XmlTags.EARTH;
			locationXPath += "/" + XmlTags.COUNTRY + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Country + "\"]";
			locationXPath += "/" + XmlTags.REGION + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Region + "\"]";
			locationXPath += "/" + XmlTags.TOWN + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.Town + "\"]";
			locationXPath += "/" + XmlTags.DISTRICT + "[@" + XmlAttributes.DESIGNATION + "=\"" + nodeGroup.District + "\"]";
			locationXPath += "/" + XmlTags.BUILDING;
			locationXPath += "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";

			XmlNode infoNode = mapResumeDocument.SelectSingleNode (locationXPath);
			res = infoNode.Attributes[attributeName];
		}
		return res;
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
		if (objectBuilder.IdTable [buildingNgp.Id].GetType() == typeof(int)) {
			int buildingGoId = (int)objectBuilder.IdTable [buildingNgp.Id];
			int nbWallGroups = objectBuilder.WallGroups.transform.childCount;

			int i = 0;
			for (; i < nbWallGroups && objectBuilder.WallGroups.transform.GetChild (i).GetInstanceID () != buildingGoId; i++);
			if (i < nbWallGroups)
				return objectBuilder.WallGroups.transform.GetChild (i).gameObject;
			else
				return null;
		} else {
			return null;
		}
	}

	public NodeGroup GameObjectToNodeGroup(GameObject buildingGo) {
		NodeGroup res = null;
		foreach (DictionaryEntry buildingEntry in objectBuilder.IdTable) {
			if (buildingEntry.Key.GetType () == typeof(int) && buildingEntry.Value.GetType () == typeof(int)) {
				int nodeGroupId = (int)buildingEntry.Key;
				int gameObjectId = (int)buildingEntry.Value;

				if (gameObjectId == buildingGo.GetInstanceID()) {
					int nbNodeGroup = objectBuilder.NodeGroups.Count;

					int i = 0;
					for (; i < nbNodeGroup && ((NodeGroup)objectBuilder.NodeGroups[i]).Id != nodeGroupId; i++);
					if (i < nbNodeGroup)
						res = ((NodeGroup)objectBuilder.NodeGroups[i]);
					else
						res = null;
					
					break;
				}
			}
		}

		return res;
	}

	public Vector3 Center(GameObject building) {
		Vector3 positionsSum = new Vector3 (0, 0, 0);

		foreach (Transform wallTransform in building.transform)
			positionsSum += wallTransform.position;
		
		return positionsSum / (building.transform.childCount * 1F);
	}

	public double Radius(GameObject building) {
		Vector3 buildingCenter = this.Center (building);

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
