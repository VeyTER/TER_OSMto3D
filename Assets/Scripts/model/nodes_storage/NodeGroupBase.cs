using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NodeGroupBase {
	private static NodeGroupBase instance;

	/// <summary>
	/// 	Groupes de noeuds contenant toutes les informations sur les objets de la scène 3D.
	/// </summary>
	private Dictionary<string, NodeGroup> nodeGroups;

	private NodeGroupBase() {
		this.nodeGroups = new Dictionary<string, NodeGroup>();
	}

	public static NodeGroupBase GetInstance() {
		if (instance == null)
			instance = new NodeGroupBase();
		return instance;
	}

	public NodeGroup AddNodeGroup(NodeGroup newNodeGroup) {
		nodeGroups[newNodeGroup.Id] = newNodeGroup;
		return newNodeGroup;
	}

	public NodeGroup GetNodeGroup(string nodeGroupId) {
		return nodeGroups[nodeGroupId];
	}

	public NodeGroup RemoveNodeGroup(NodeGroup oldNodeGroup) {
		nodeGroups.Remove(oldNodeGroup.Id);
		return oldNodeGroup;
	}

	/// <summary>
	/// 	Copie et change l'échelle des groupes de noeuds.
	/// </summary>
	/// <param name="scaleFactor">Facteur d'échelle.</param>
	public void ScaleNodes(double scaleFactor) {
		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroups) {
			foreach (Node node in nodeGroupEntry.Value.Nodes) {
				node.Latitude = node.Latitude * scaleFactor;
				node.Longitude = node.Longitude * scaleFactor;
			}
		}
	}

	public Dictionary<string, NodeGroup> NodeGroups {
		get { return nodeGroups; }
	}
}