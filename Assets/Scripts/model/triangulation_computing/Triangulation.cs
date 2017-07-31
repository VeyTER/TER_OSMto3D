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

	private BuildingShape buildingShape;
	private List<Triangle> triangles;

	public Triangulation(NodeGroup nodeGroup) {
		this.nodeGroup = nodeGroup;

		this.buildingShape = new BuildingShape();
		this.triangles = new List<Triangle>();
	}

	public void Triangulate() {
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

		buildingShape.ReplaceEdges(newEdges);
	}

	private void BuildTriangulation() {
		BuildingShape shapeClone = buildingShape.Clone();

		while (shapeClone.EdgeCount() > 3 && shapeClone.EarTipNodes.Count > 0) {
			int i = 0;
			for (; i < shapeClone.EdgeCount() && !shapeClone.EarTipNodes.Contains(shapeClone.GetEdge(i).NodeB); i++) ;

			if (i < shapeClone.EdgeCount()) {
				Edge currentEdge = shapeClone.GetEdge(i);
				Edge nextEdge = shapeClone.NextEdge(i);

				Node previousNode = currentEdge.NodeA;
				Node earTipNode = currentEdge.NodeB;
				Node nextNode = nextEdge.NodeB;

				Edge newEdge = new Edge(previousNode, nextNode);

				Triangle triangle = new Triangle(previousNode, earTipNode, nextNode);

				shapeClone.RemoveEdge(currentEdge, true);

				if (!currentEdge.Equals(nextEdge)) {
					shapeClone.ReplaceEdge(nextEdge, newEdge, true);
					triangles.Add(triangle);
				}
			}
		}

		if (shapeClone.EdgeCount() == 3) {
			Triangle triangle = new Triangle(shapeClone.GetEdge(0).NodeA, shapeClone.GetEdge(0).NodeB, shapeClone.GetEdge(1).NodeB);
			triangles.Add(triangle);
		}

		shapeClone.Clear();
	}

	public BuildingShape BuildingShape {
		get { return buildingShape; }
	}

	public List<Triangle> Triangles {
		get { return triangles; }
	}
}