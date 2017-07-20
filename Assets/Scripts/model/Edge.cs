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

	public float Slope() {
		float deltaX = (float) (nodeB.Longitude - nodeA.Longitude);
		float deltaY = (float) (nodeB.Latitude - nodeA.Latitude);
		return deltaY / deltaX;
	}

	public float Orientation() {
		float deltaX = (float) (nodeB.Longitude - nodeA.Longitude);
		float deltaY = (float) (nodeB.Latitude - nodeA.Latitude);
		return (float)Math.Atan2(deltaY, deltaX);
	}

	public Vector2 MedianPoint() {
		return nodeA.ToVector() + (nodeB.ToVector() - nodeA.ToVector()) / 2F;
	}

	public override bool Equals(object obj) {
		if (obj.GetType() == typeof(Edge)) {
			Edge edge = (Edge) obj;
			return nodeA.Equals(edge.NodeA) && nodeB.Equals(edge.NodeB);
		} else {
			return false;
		}
	}

	public Edge InvertedCopy() {
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