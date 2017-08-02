using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NodeGroupBase {
	private static NodeGroupBase instance;

	/// <summary>
	/// 	Groupes de noeuds contenant toutes les informations sur les objets de la scène 3D.
	/// </summary>
	private Dictionary<string, NodeGroup> nodeGroups;

	/// <summary>Latitude minimale de la ville.</summary>
	private double minLat;

	/// <summary>Longitude minimale de la ville.</summary>
	private double minLon;

	/// <summary>Latitude maximale de la ville.</summary>
	private double maxLat;

	/// <summary>Longitude maximale de la ville.</summary>
	private double maxLon;

	private NodeGroupBase() {
		this.nodeGroups = new Dictionary<string, NodeGroup>();

		this.minLat = 0;
		this.minLon = 0;
		this.maxLat = 0;
		this.maxLon = 0;
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
		minLat = minLat * Dimensions.SCALE_FACTOR * scaleFactor;
		minLon = minLon * Dimensions.SCALE_FACTOR * scaleFactor;
		maxLat = maxLat * Dimensions.SCALE_FACTOR * scaleFactor;
		maxLon = maxLon * Dimensions.SCALE_FACTOR * scaleFactor;

		foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroups) {
			foreach (Node node in nodeGroupEntry.Value.Nodes) {
				node.Latitude = node.Latitude * Dimensions.SCALE_FACTOR * scaleFactor;
				node.Longitude = node.Longitude * Dimensions.SCALE_FACTOR * scaleFactor;
			}
		}
	}

	public void SetBounds(double minLat, double minLon, double maxLat, double maxLon) {
		this.minLat = minLat;
		this.minLon = minLon;
		this.maxLat = maxLat;
		this.maxLon = maxLon;
	}

	public double[] GetBounds() {
		return new double[] {
			minLat,
			minLon,
			maxLat,
			maxLon
		};
	}

	public Dictionary<string, NodeGroup> NodeGroups {
		get { return nodeGroups; }
	}

	public double MinLat {
		get { return minLat; }
	}

	public double MinLon {
		get { return minLon; }
	}

	public double MaxLat {
		get { return maxLat; }
	}

	public double MaxLon {
		get { return maxLon; }
	}
}