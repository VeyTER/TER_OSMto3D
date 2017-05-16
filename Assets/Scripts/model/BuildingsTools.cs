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

	private string customFilePath;
	private XmlDocument mapCustomDocument;

	private BuildingsTools () {
		this.objectBuilder = ObjectBuilder.GetInstance ();

		this.resumeFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		this.mapResumeDocument = new XmlDocument();

		this.customFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";
		this.mapCustomDocument = new XmlDocument ();
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

	public void ColorAsSelected(GameObject buildingGo) {
		Material wallMaterial = Resources.Load ("Materials/Wall") as Material;
		Material selectedElementMaterial = Resources.Load ("Materials/SelectedElement") as Material;

		foreach (Transform wallGo in buildingGo.transform) {
			Renderer meshRenderer = wallGo.GetComponent<Renderer> ();
			if (meshRenderer != null) {
				meshRenderer.materials = new Material[] {
					wallMaterial,
					selectedElementMaterial
				};
			}
		}
	}

	public void SetName(GameObject buildingGo, string newName) {
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);
		if (!this.CustomBuildingExists (buildingNgp))
			this.AppendCustomBuilding (buildingNgp);
		
		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);

			XmlAttribute resumeNameAttribute = this.ResumeNodeGroupAttribute (buildingNgp, XmlAttributes.NAME);
			XmlAttribute customNameAttribute = this.CustomNodeGroupAttribute (buildingNgp, XmlAttributes.NAME);

			buildingNgp.Name = newName;
			buildingGo.name = newName;
			for (int i = 0; i < buildingGo.transform.childCount; i++)
				buildingGo.transform.GetChild (i).name = newName + "_mur_" + i;
			
			resumeNameAttribute.Value = newName;
			customNameAttribute.Value = newName;

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}
	}

	public void SetLocation(GameObject buildingGo) {
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);
		if (!this.CustomBuildingExists (buildingNgp))
			this.AppendCustomBuilding (buildingNgp);

		if (File.Exists (resumeFilePath) && File.Exists (customFilePath)) {
			mapResumeDocument.Load (resumeFilePath);
			mapCustomDocument.Load (customFilePath);

			string xPath = "/" + XmlTags.EARTH + "//" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + buildingNgp.Id + "\"]";
			XmlNode resumeBuildingNode = mapResumeDocument.SelectSingleNode (xPath).ParentNode;
			XmlNode customBuildingNode = mapCustomDocument.SelectSingleNode (xPath).ParentNode;

			for (int i = 0; i < customBuildingNode.ChildNodes.Count;) {
				XmlNode customBuildingChildNode = customBuildingNode.ChildNodes [i];
				if (!customBuildingChildNode.Name.Equals (XmlTags.INFO)) {
					customBuildingNode.RemoveChild (customBuildingChildNode);
				} else {
					i++;
				}
			}

			XmlNodeList resumedBuildingNd = resumeBuildingNode.ChildNodes;

			foreach(Node node in buildingNgp.Nodes) {
				long nodeReference = node.Reference;
				int nodeIndex = node.Index;

				int i = 1;
				for (; i < resumedBuildingNd.Count && (!resumedBuildingNd [i].Attributes [XmlAttributes.REFERENCE].Value.Equals (nodeReference.ToString ())
				|| !resumedBuildingNd [i].Attributes [XmlAttributes.INDEX].Value.Equals (nodeIndex.ToString ())); i++);

				if (i < resumedBuildingNd.Count) {
					resumedBuildingNd [i].Attributes [XmlAttributes.LATITUDE].Value = (node.Latitude / Main.SCALE_FACTOR).ToString();
					resumedBuildingNd [i].Attributes [XmlAttributes.LONGIUDE].Value = (node.Longitude / Main.SCALE_FACTOR).ToString();

					XmlNode customBuildingNd = mapCustomDocument.ImportNode (resumedBuildingNd [i], true);
					customBuildingNode.AppendChild (customBuildingNd);
				}
			}

			mapResumeDocument.Save (resumeFilePath);
			mapCustomDocument.Save (customFilePath);
		}
	}

	public void AppendCustomBuilding(NodeGroup nodeGroup) {
		if (File.Exists (customFilePath)) {
			mapCustomDocument.Load (customFilePath);

			XmlNodeList earthNodes = mapCustomDocument.GetElementsByTagName (XmlTags.EARTH);

			if (earthNodes.Count == 0)
				mapCustomDocument.AppendChild (mapCustomDocument.CreateElement (XmlTags.EARTH));

			XmlNode earthNode = earthNodes [0];
			XmlNode buildingNode = mapCustomDocument.CreateElement (XmlTags.BUILDING);
			XmlNode buildingInfoNode = mapCustomDocument.CreateElement (XmlTags.INFO);

			earthNode.AppendChild (buildingNode);
			buildingNode.AppendChild (buildingInfoNode);
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ID, nodeGroup.Id.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.NAME, nodeGroup.Name);
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.NB_FLOOR, nodeGroup.NbFloor.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_ANGLE, nodeGroup.RoofAngle.ToString ());
			this.AppendNodeGroupAttribute (mapCustomDocument, buildingInfoNode, XmlAttributes.ROOF_TYPE, nodeGroup.RoofType);

			mapCustomDocument.Save (customFilePath);
		}
	}

	public bool CustomBuildingExists(NodeGroup buildingNgp) {
		if (File.Exists (customFilePath)) {
			mapCustomDocument.Load (customFilePath);
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + buildingNgp.Id + "\"]";
			mapCustomDocument.Save (customFilePath);
			return mapCustomDocument.SelectSingleNode (xPath) != null;
		} else {
			return false;
		}
	}

	private void AppendNodeGroupAttribute(XmlDocument boundingDocument, XmlNode containerNode, string attributeName, string attributeValue) {
		XmlAttribute attribute = boundingDocument.CreateAttribute (attributeName);
		attribute.Value = attributeValue;
		containerNode.Attributes.Append (attribute);
	}

	public int GetHeight(GameObject building) {
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);
		XmlAttribute floorAttribute = this.ResumeNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
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
		NodeGroup nodeGroup = this.BuildingToNodeGroup (building);
		XmlAttribute floorAttribute = this.ResumeNodeGroupAttribute (nodeGroup, XmlAttributes.NB_FLOOR);
		if (floorAttribute != null)
			floorAttribute.Value = Math.Max(nbFloors, 1).ToString();

		mapResumeDocument.Save (resumeFilePath);

		ObjectBuilder objectBuilder = ObjectBuilder.GetInstance();
		objectBuilder.EditUniqueBuilding (building, nbFloors);
	}

	private XmlAttribute ResumeNodeGroupAttribute(NodeGroup nodeGroup, string attributeName) {
		XmlAttribute res = null;
//		if (File.Exists (resumeFilePath)) {
//			mapResumeDocument.Load (resumeFilePath); 

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

//			mapResumeDocument.Save (resumeFilePath); 
//		}
		return res;
	}

	private XmlAttribute CustomNodeGroupAttribute(NodeGroup nodeGroup, string attributeName) {
		XmlAttribute res = null;
//		if (File.Exists (customFilePath)) {
//			mapCustomDocument.Load (customFilePath);
			string xPath = "/" + XmlTags.EARTH + "/" + XmlTags.BUILDING + "/" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + nodeGroup.Id + "\"]";
			XmlNode infoNode = mapCustomDocument.SelectSingleNode(xPath);
			res = infoNode.Attributes[attributeName];
//			mapCustomDocument.Save (customFilePath); 
//		}
		return res;
	}

	public GameObject NodeGroupToBuilding(NodeGroup buildingNgp) {
		if (objectBuilder.BuildingIdTable [buildingNgp.Id].GetType() == typeof(int)) {
			int buildingGoId = (int)objectBuilder.BuildingIdTable [buildingNgp.Id];
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

	public NodeGroup BuildingToNodeGroup(GameObject buildingGo) {
		return this.ObjectToNodeGroup (buildingGo, objectBuilder.BuildingIdTable);
	}

	public NodeGroup BuildingNodeGroupToNodeGroup(GameObject buildingNodeGroupGo) {
		return this.ObjectToNodeGroup (buildingNodeGroupGo, objectBuilder.BuildingNodeGroupsIdTable);
	}

	public NodeGroup ObjectToNodeGroup(GameObject gameObject, Hashtable idTable) {
		NodeGroup res = null;

		foreach (DictionaryEntry objectEntry in idTable) {
			if (objectEntry.Key.GetType () == typeof(long) && objectEntry.Value.GetType () == typeof(int)) {
				long nodeGroupId = (long)objectEntry.Key;
				int currentGameObjectId = (int)objectEntry.Value;

				if (currentGameObjectId == gameObject.transform.GetInstanceID()) {
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

	public Node BuildingNodeToNode(GameObject buildingNodeGo) {
		NodeGroup parentNodeGoup = this.BuildingNodeGroupToNodeGroup (buildingNodeGo.transform.parent.gameObject);
		return this.BuildingNodeToNode (buildingNodeGo, parentNodeGoup);
	}

	public Node BuildingNodeToNode(GameObject buildingNodeGo, NodeGroup parentNodeGoup) {
		Node res = null;

		foreach (DictionaryEntry objectEntry in objectBuilder.BuildingNodesIdTable) {
			if (objectEntry.Key.GetType () == typeof(string) && objectEntry.Value.GetType () == typeof(int)) {
				string nodeId = (string)objectEntry.Key;
				int currentGameObjectId = (int)objectEntry.Value;

				string[] nodeIdArraySplit = nodeId.Split ('|');
				long nodeReference = long.Parse (nodeIdArraySplit[0]);
				int nodeIndex = int.Parse (nodeIdArraySplit[1]);

				if (currentGameObjectId == buildingNodeGo.transform.GetInstanceID()) {
					int nbNode = parentNodeGoup.Nodes.Count;

					int i = 0;
					for (; i < nbNode && (((Node)parentNodeGoup.GetNode(i)).Reference != nodeReference || ((Node)parentNodeGoup.GetNode(i)).Index != nodeIndex); i++);
					if (i < nbNode)
						res = ((Node)parentNodeGoup.GetNode (i));
					else
						res = null;

					break;
				}
			}
		}

		return res;
	}

	public GameObject BuildingToBuildingNodeGroup(GameObject buildingGo) {
		// [GameObject]-building  =>  [NodeGroup]-buildingNodeGroup
		NodeGroup buildingNodeGroup = this.BuildingToNodeGroup (buildingGo);

		// 							  [NodeGroup]-buildingNodeGroup ID  =>  [GameObject]-BuildingNode ID
		int buildingNodeGroupId = (int)objectBuilder.BuildingNodeGroupsIdTable[buildingNodeGroup.Id];

		// 							 										[GameObject]-BuildingNode ID  =>  [GameObject]-BuildingNode
		Transform buildingNodesTransform = objectBuilder.BuildingNodes.transform;
		int i = 0;
		for (; i < buildingNodesTransform.childCount && buildingNodesTransform.GetChild(i).GetInstanceID() != buildingNodeGroupId; i++);
		if (i < buildingNodesTransform.childCount)
			return buildingNodesTransform.GetChild (i).gameObject;
		else
			return null;
	}

	public void UpdateNodes(GameObject building) {
		NodeGroup parentNodeGroup = this.BuildingToNodeGroup (building);
		GameObject buildingNodeGroup = this.BuildingToBuildingNodeGroup (building);

		foreach(Transform buildingNodeTransform in buildingNodeGroup.transform) {
			Node node = this.BuildingNodeToNode (buildingNodeTransform.gameObject, parentNodeGroup);

			node.Latitude = buildingNodeTransform.position.z;
			node.Longitude = buildingNodeTransform.position.x;
		}
	}

	public Vector3 BuildingCenter(GameObject buildingGo) {
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		if (buildingNgp != null) {
			Vector3 positionSum = Vector3.zero;
			GameObject firstWall = buildingGo.transform.GetChild (0).gameObject;
			for(int i = 0; i < buildingNgp.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)buildingNgp.Nodes [i];
				Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)firstWall.transform.position.y, (float)buildingNode.Latitude);
				positionSum += nodePosition;
			}

			return positionSum / ((buildingNgp.Nodes.Count - 1) * 1F);
		} else {
			return Vector3.zero;
		}
	}

	public Vector3 BuildingNodesCenter(GameObject buildingGo, NodeGroup buildingNgp) {
		Vector3 positionSum = new Vector3 (0, 0, 0);

		for(int i = 0; i < buildingNgp.Nodes.Count - 1; i++) {
			Node buildingNode = (Node)buildingNgp.Nodes [i];
			Vector3 nodePosition = new Vector3 ((float)buildingNode.Longitude, (float)buildingGo.transform.position.y, (float)buildingNode.Latitude);
			positionSum += nodePosition;
		}

		return positionSum / ((buildingNgp.Nodes.Count - 1) * 1F);
	}

	public double BuildingRadius(GameObject buildingGo) {
		Vector3 buildingCenter = this.BuildingCenter (buildingGo);
		NodeGroup buildingNgp = this.BuildingToNodeGroup (buildingGo);

		if (buildingNgp != null) {
			double maxDistance = 0;
			for(int i = 0; i < buildingNgp.Nodes.Count - 1; i++) {
				Node buildingNode = (Node)buildingNgp.Nodes [i];
				Vector2 nodePosition = new Vector2 ((float)buildingNode.Longitude, (float)buildingNode.Latitude);
				Vector2 buildingCenter2D = new Vector2 (buildingCenter.x, buildingCenter.z);

				double currentDistance = Vector2.Distance (buildingCenter2D, nodePosition);
				if (currentDistance > maxDistance)
					maxDistance = currentDistance;
			}

			return maxDistance;
		} else {
			return -1;
		}
	}

	public static class BuildingsToolsInstanceHolder {
		public static BuildingsTools instance = new BuildingsTools();
	}
}
