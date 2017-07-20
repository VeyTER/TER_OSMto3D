using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 	Suite de méthodes permettant d'effectuer une triangulation de Delaunay pour construire les toits.
/// </summary>
public class Triangulation {
	private enum VertexSides { NONE, RIGHT, LEFT }

	private NodeGroup nodeGroup;

	private BuildingShape edgeShape;
	private List<Triangle> triangles;

	private List<GameObject> testCubes;

	public Triangulation(NodeGroup nodeGroup) {
		this.nodeGroup = nodeGroup;

		this.edgeShape = new BuildingShape();
		this.triangles = new List<Triangle>();

		this.testCubes = new List<GameObject>();
	}

	public void Triangulate(string name) {
		if (nodeGroup.NodeCount() >= 3) {
			this.BuildShapeEdges();
			this.BuildTriangulation();
		}
	}

	private void BuildShapeEdges() {
		float area = BuildingsTools.GetInstance().BuildingArea(nodeGroup);

		List<Edge> newEdges = new List<Edge>();
		for (int i = area >= 0 ? 0 : nodeGroup.NodeCount() - 1; (area >= 0 && i < nodeGroup.NodeCount() - 1) || (area < 0 && i >= 1); i = area >= 0 ? i + 1 : i - 1) {
			Node currentNode = nodeGroup.GetNode(i);
			Node nextNode = nodeGroup.GetNode(i + (area >= 0 ? 1 : -1));

			Edge newEdge = new Edge(currentNode, nextNode, Edge.EdgeTypes.SHAPE);
			newEdges.Add(newEdge);
		}

		edgeShape.ReplaceEdges(newEdges);
	}

	private void BuildTriangulation() {
		while (edgeShape.EdgeCount() > 3 && edgeShape.EarTipNodes.Count > 0) {
			int i = 0;
			for (; i < edgeShape.EdgeCount() && !edgeShape.EarTipNodes.Contains(edgeShape.GetEdge(i).NodeB); i++) ;

			if (i < edgeShape.EdgeCount()) {
				Edge currentEdge = edgeShape.GetEdge(i);
				Edge nextEdge = edgeShape.NextEdge(i);

				Node previousNode = currentEdge.NodeA;
				Node earTipNode = currentEdge.NodeB;
				Node nextNode = nextEdge.NodeB;

				Edge newEdge = new Edge(previousNode, nextNode);

				Triangle triangle = new Triangle(previousNode, earTipNode, nextNode);

				edgeShape.RemoveEdge(currentEdge, true);

				if (!currentEdge.Equals(nextEdge)) {
					edgeShape.ReplaceEdge(nextEdge, newEdge, true);
					triangles.Add(triangle);
				}
			}
		}

		if (edgeShape.EdgeCount() == 3) {
			Triangle triangle = new Triangle(edgeShape.GetEdge(0).NodeA, edgeShape.GetEdge(0).NodeB, edgeShape.GetEdge(1).NodeB);
			triangles.Add(triangle);
		}

		edgeShape.Clear();
	}

	public BuildingShape EdgeShape {
		get { return edgeShape; }
	}

	public List<Triangle> Triangles {
		get { return triangles; }
	}
}