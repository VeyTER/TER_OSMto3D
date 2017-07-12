using UnityEngine;
using UnityEditor;
using System;

public class Edge {
	public enum EdgeTypes { UNSPECIFIED, SHAPE, LINK}

	private Node nodeA;
	private Node nodeB;

	private EdgeTypes edgeType;

	public Edge(Node nodeA, Node nodeB) {
		this.nodeA = nodeA;
		this.nodeB = nodeB;

		this.edgeType = EdgeTypes.UNSPECIFIED;
	}

	public Edge(Node nodeA, Node nodeB, EdgeTypes edgeType) {
		this.nodeA = nodeA;
		this.nodeB = nodeB;

		this.edgeType = edgeType;
	}

	public float Length() {
		Vector2 nodeALocation = new Vector2((float) nodeA.Latitude, (float) nodeA.Longitude);
		Vector2 nodeBLocation = new Vector2((float) nodeA.Latitude, (float) nodeA.Longitude);

		if (Vector2.Distance(nodeALocation, nodeBLocation) < 0)
			Debug.Log("1 => Problème : distance négative entre deux sommets.");

		return Vector2.Distance(nodeALocation, nodeBLocation);
	}

	public double Orientation() {
		float deltaX = (float) (nodeA.Latitude - nodeB.Latitude);
		float deltaY = (float) (nodeA.Longitude - nodeB.Longitude);
		return Math.Atan2(deltaY, deltaX);
	}

	public Vector2 MedianPoint() {
		Vector2 nodeALocation = new Vector2((float) nodeA.Latitude, (float) nodeA.Longitude);
		Vector2 nodeBLocation = new Vector2((float) nodeA.Latitude, (float) nodeA.Longitude);
		return nodeALocation + (nodeBLocation - nodeALocation) / 2F;
	}

	public Edge GetCopy() {
		return new Edge(nodeA, nodeB);
	}

	public Edge GetInvertedCopy() {
		return new Edge(nodeB, nodeA);
	}

	public bool IsShapeEdge() {
		return edgeType == EdgeTypes.SHAPE;
	}

	public bool IsLinkEdge() {
		return edgeType == EdgeTypes.LINK;
	}

	public Node NodeA {
		get { return nodeA; }
		set { nodeA = value; }
	}

	public Node NodeB {
		get { return nodeB; }
		set { nodeB = value; }
	}
}