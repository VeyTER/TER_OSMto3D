using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

public class MapFileManager : OsmFileManager {
	/// <summary>
	///		Extrait les données du fichier OSM contenant la carte les stocke dans des noeuds eux-même contenus dans des
	/// 	groupes de noeuds.
	/// </summary>
	/// <param name="mapName">Nom du fichier OSM contenant la carte à charger.</param>
	public void LoadOsmData(string mapName) {
		string OSMFilePath = mapName;
		XmlDocument osmDocument = new XmlDocument();

		if (File.Exists(OSMFilePath)) {
			osmDocument.Load(OSMFilePath);

			// Récupération des coordonnées minimales et maximales de la carte
			XmlNodeList boundsNodes = osmDocument.GetElementsByTagName(XmlTags.BOUNDS);
			if (boundsNodes.Count > 0) {
				double minLat = double.Parse(osmTools.AttributeValue(boundsNodes[0], XmlAttributes.MIN_LATITUDE));
				double minLon = double.Parse(osmTools.AttributeValue(boundsNodes[0], XmlAttributes.MIN_LONGITUDE));
				double maxLat = double.Parse(osmTools.AttributeValue(boundsNodes[0], XmlAttributes.MAX_LATITUDE));
				double maxLon = double.Parse(osmTools.AttributeValue(boundsNodes[0], XmlAttributes.MAX_LONGITUDE));
				nodeGroupBase.SetBounds(minLat, minLon, maxLat, maxLon);
			}

			this.ExtractNodes(osmDocument);
		}
	}

	private void ExtractNodes(XmlDocument osmDocument) {
		List<string> usedNodesId = new List<string>();

		Dictionary<string, XmlNode> osmNodes = this.SingleNodes(osmDocument);

		XmlNodeList wayNodes = osmDocument.GetElementsByTagName(XmlTags.WAY);
		foreach (XmlNode wayNode in wayNodes) {
			string nodeGroupId = osmTools.AttributeValue(wayNode, XmlAttributes.ID);
			int index = wayNode.ChildNodes.Count - 1;

			Dictionary<string, string> nodesGroupTags = this.NodesGroupTags(wayNode, ref index);
			this.AddNewNodeGroup(nodesGroupTags, nodeGroupId);
			this.FillCompositeNodeGroups(wayNode, nodesGroupTags, nodeGroupId, ref index, osmNodes, usedNodesId);
		}

		this.CreateSimpleNodeGroups(osmNodes, usedNodesId);
	}

	private Dictionary<string, XmlNode> SingleNodes(XmlDocument osmDocument) {
		Dictionary<string, XmlNode> osmNodes = osmNodes = new Dictionary<string, XmlNode>();
		XmlNodeList nodeNodes = osmDocument.GetElementsByTagName(XmlTags.NODE);
		foreach (XmlNode nodeNode in nodeNodes) {
			string id = osmTools.AttributeValue(nodeNode, XmlAttributes.ID);
			osmNodes[id] = nodeNode;
		}
		return osmNodes;
	}

	private Dictionary<string, string> NodeTags(XmlNode node) {
		Dictionary<string, string> tags = new Dictionary<string, string>();
		foreach (XmlNode tagNode in node.ChildNodes) {
			string key = osmTools.AttributeValue(tagNode, XmlAttributes.PROPERTY_KEY);
			string value = osmTools.AttributeValue(tagNode, XmlAttributes.PROPERTY_VALUE);
			tags[key] = value;
		}
		return tags;
	}

	private Dictionary<string, string> NodesGroupTags(XmlNode wayNode, ref int index) {
		Dictionary<string, string> nodesGroupTags = new Dictionary<string, string>();
		for (; index >= 0 && !wayNode.ChildNodes[index].Name.Equals(XmlTags.ND); index--) {
			if (wayNode.ChildNodes[index].Name.Equals(XmlTags.TAG)) {
				XmlNode tagNode = wayNode.ChildNodes[index];
				string key = osmTools.AttributeValue(tagNode, XmlAttributes.PROPERTY_KEY);
				string value = osmTools.AttributeValue(tagNode, XmlAttributes.PROPERTY_VALUE);
				nodesGroupTags[key] = value;
			}
		}
		return nodesGroupTags;
	}

	private void AddNewNodeGroup(Dictionary<string, string> nodesGroupTags, string nodeGroupId) {
		if (nodesGroupTags.ContainsKey(XmlKeys.BUILDING))
			nodeGroupBase.AddNodeGroup(new BuildingNodeGroup(nodeGroupId, nodesGroupTags[XmlKeys.BUILDING]));
		else if (nodesGroupTags.ContainsKey(XmlKeys.HIGHWAY))
			nodeGroupBase.AddNodeGroup(new HighwayNodeGroup(nodeGroupId, nodesGroupTags[XmlKeys.HIGHWAY]));
		else if (nodesGroupTags.ContainsKey(XmlKeys.WATERWAY))
			nodeGroupBase.AddNodeGroup(new WaterwayNodeGroup(nodeGroupId, nodesGroupTags[XmlKeys.WATERWAY]));
		else if (nodesGroupTags.ContainsKey(XmlKeys.NATURAL))
			nodeGroupBase.AddNodeGroup(new NaturalNodeGroup(nodeGroupId, nodesGroupTags[XmlKeys.NATURAL]));
		else if (nodesGroupTags.ContainsKey(XmlKeys.LEISURE))
			nodeGroupBase.AddNodeGroup(new LeisureNodeGroup(nodeGroupId, nodesGroupTags[XmlKeys.LEISURE]));
	}

	private void FillCompositeNodeGroups(XmlNode wayNode, Dictionary<string, string> nodesGroupTags, string nodeGroupId, ref int index, Dictionary<string, XmlNode> osmNodes, List<string> usedNodesId) {
		if (nodeGroupBase.NodeGroups.ContainsKey(nodeGroupId)) {
			NodeGroup newNodeGroup = nodeGroupBase.GetNodeGroup(nodeGroupId);
			newNodeGroup.Tags = nodesGroupTags;

			if (nodesGroupTags.ContainsKey(XmlKeys.NAME))
				newNodeGroup.Name = nodesGroupTags[XmlKeys.NAME];

			for (; index >= 0; index--) {
				if (wayNode.ChildNodes[index].Name.Equals(XmlTags.ND)) {
					XmlNode nbNode = wayNode.ChildNodes[index];
					string reference = osmTools.AttributeValue(nbNode, XmlAttributes.REFERENCE);

					XmlNode matchingOsmNode = osmNodes[reference];
					string id = osmTools.AttributeValue(matchingOsmNode, XmlAttributes.ID);
					double latitude = double.Parse(osmTools.AttributeValue(matchingOsmNode, XmlAttributes.LATITUDE));
					double longitude = double.Parse(osmTools.AttributeValue(matchingOsmNode, XmlAttributes.LONGIUDE));

					usedNodesId.Add(Node.GenerateId(reference, index));

					Dictionary<string, string> nodeTags = this.NodeTags(matchingOsmNode);

					try {
						newNodeGroup.AddStepNode(reference, index, latitude, longitude, nodeTags);
					} catch (NotImplementedException exception) {
						Debug.LogError(exception.Message);
					}

					if (matchingOsmNode.ChildNodes.Count > 0) {
						try {
							newNodeGroup.AddComponentNode(reference, index, latitude, longitude, nodeTags);
						} catch (NotImplementedException exception) {
							Debug.LogError(exception.Message);
						}
					}
				}
			}
		}
	}

	private void CreateSimpleNodeGroups(Dictionary<string, XmlNode> osmNodes, List<string> usedNodesId) {
		foreach (KeyValuePair<string, XmlNode> osmNodeEntry in osmNodes) {
			string nodeId = osmNodeEntry.Key;
			XmlNode osmNode = osmNodeEntry.Value;

			string reference = osmTools.AttributeValue(osmNode, XmlAttributes.REFERENCE);
			double latitude = double.Parse(osmTools.AttributeValue(osmNode, XmlAttributes.LATITUDE));
			double longitude = double.Parse(osmTools.AttributeValue(osmNode, XmlAttributes.LONGIUDE));

			Dictionary<string, string> nodeTags = this.NodeTags(osmNode);

			if (!usedNodesId.Contains(nodeId)) {
				if (nodeTags.ContainsKey(XmlKeys.NATURAL))
					nodeGroupBase.AddNodeGroup(new NaturalNodeGroup(nodeId, nodeTags[XmlKeys.NATURAL]));
				// else if()

				if (nodeGroupBase.NodeGroups.ContainsKey(nodeId)) {
					NodeGroup newNodeGroup = nodeGroupBase.GetNodeGroup(nodeId);
					newNodeGroup.AddComponentNode(reference, 0, latitude, longitude, nodeTags);
				}
			}
		}
	}
}