using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Suite de méthodes permettant d'effectuer une triangulation de Delaunay pour construire les toits.
/// </summary>
public class Triangulation {
	private enum VertexSides { NONE, RIGHT, LEFT }

	private NodeGroup nodeGroup;

	private List<Vector2> convexVertices;
	private List<Vector2> reflexVertices;
	private List<Vector2> earTipVertices;

	private EdgeShape edgeShape;

	public Triangulation(NodeGroup nodeGroup) {
		this.nodeGroup = nodeGroup;

		this.convexVertices = new List<Vector2>();
		this.reflexVertices = new List<Vector2>();
		this.earTipVertices = new List<Vector2>();

		this.edgeShape = new EdgeShape();
	}

	public void Triangulate(string name) {
		if (nodeGroup.NodeCount() >= 3) {
			this.BuildShapeEdges();
			this.FilterVectices(name);

			//Debug.Log(convexVertices.Count + "  " + reflexVertices.Count + "  " + earTipVertices.Count);

			//this.BuildTriangulation();
		}
	}

	private void BuildShapeEdges() {
		float shapeArea = 0;
		for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
			Vector2 currentPoint = nodeGroup.GetNode(i).ToVector();
			Vector2 nextPoint = nodeGroup.GetNode(i + 1).ToVector();

			shapeArea += (nextPoint.x - currentPoint.x) * (nextPoint.y + currentPoint.y);
		}

		for (int i = shapeArea >= 0 ? 0 : nodeGroup.NodeCount() - 1; (shapeArea >= 0 && i < nodeGroup.NodeCount() - 1) || (shapeArea < 0 && i >= 1); i = shapeArea >= 0 ? i + 1 : i - 1) {
			Node currentNode = nodeGroup.GetNode(i);
			Node nextNode = nodeGroup.GetNode(i + (shapeArea >= 0 ? 1 : -1));

			Edge newEdge = new Edge(currentNode, nextNode, Edge.EdgeTypes.SHAPE);
			edgeShape.AddEdge(newEdge);
		}
	}

	private void FilterVectices(string name) {
		float sum = 0;
		for (int i = 0; i < nodeGroup.NodeCount() - 2; i++) {
			Vector2 currentPoint = nodeGroup.GetNode(i).ToVector();
			Vector2 nextPoint = nodeGroup.GetNode(i + 1).ToVector();

			sum += (nextPoint.x - currentPoint.x) * (nextPoint.y + currentPoint.y);
		}

		for(int i = 0; i < edgeShape.EdgeCount(); i++) {
			Edge currentEdge = edgeShape.GetEdge(i);
			Edge nextEdge = edgeShape.NextEdge(currentEdge);

			if (this.IsConvex(currentEdge.NodeB, currentEdge, nextEdge)) {
				convexVertices.Add(currentEdge.NodeB.ToVector());

				if (this.IsEarTip(currentEdge.NodeB))
					earTipVertices.Add(currentEdge.NodeB.ToVector());
			} else {
				reflexVertices.Add(currentEdge.NodeB.ToVector());

				GameObject objj = GameObject.CreatePrimitive(PrimitiveType.Cube);

				objj.transform.position = new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude);
				objj.transform.localScale = new Vector3(0.05F, 0.05F, 0.05F);
			}
		}
	}

	private bool IsConvex(Node testedNode, Edge currentEdge, Edge nextEdge) {
		Vector2 previousVertex = currentEdge.NodeA.ToVector();
		Vector2 testedVertex = currentEdge.NodeB.ToVector();
		Vector2 nextVertex = nextEdge.NodeB.ToVector();

		float testOperation = (testedVertex.x - previousVertex.x) * (nextVertex.y - previousVertex.y)
						    - (nextVertex.x - previousVertex.x) * (testedVertex.y - previousVertex.y);

		return testOperation <= 0;

	}

	private bool IsEarTip(Node testedNode) {
		Vector2 testedVertex = testedNode.ToVector();

		bool pointInsideTriangle = false;
		for (int i = 1; i < nodeGroup.NodeCount() - 1 && !pointInsideTriangle; i++) {
			Node previousNode = nodeGroup.GetNode(i - 1);
			Node currentNode = nodeGroup.GetNode(i);
			Node nextNode = nodeGroup.GetNode(i + 1);

			Triangle currentTriangle = new Triangle(previousNode, currentNode, nextNode);

			if (this.InsideTriangle(testedVertex, currentTriangle))
				pointInsideTriangle = true;
		}

		return pointInsideTriangle;
	}

	private bool InsideTriangle(Vector2 testedVertex, Triangle triangle) {
		bool test1 = this.PointSign(testedVertex, triangle.NodeA.ToVector(), triangle.NodeB.ToVector()) < 0;
		bool test2 = this.PointSign(testedVertex, triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		bool test3 = this.PointSign(testedVertex, triangle.NodeC.ToVector(), triangle.NodeA.ToVector()) < 0;
		return test1 == test2 && test2 == test3;
	}

	private float PointSign(Vector2 testedVertex, Vector2 vertexA, Vector2 vertexB) {
		return (testedVertex.x - vertexB.x) * (vertexA.y - vertexB.y) - (vertexA.x - vertexB.x) * (testedVertex.y - vertexB.y);
	}

	private Edge SmallestEdge() {
		Edge res = edgeShape.GetEdge(0);
		foreach(Edge edge in edgeShape.Edges) {
			if (edge.Length() < res.Length())
				res = edge;
		}
		return res;
	}

	private float AngleBetweenEdges(Edge edgeA, Edge edgeB) {
		Debug.Log("2 => Vérification angle : " + Math.Abs(edgeA.Orientation() - edgeB.Orientation()));

		return (float)Math.Abs(edgeA.Orientation() - edgeB.Orientation());
	}

	private void BuildTriangulation() {
		Edge startEdge = this.SmallestEdge();

		Edge previousEdge = edgeShape.PreviousEdge(startEdge);
		Edge nextEdge = edgeShape.NextEdge(startEdge);

		float startPreviousAngle = this.AngleBetweenEdges(previousEdge, startEdge);
		float startNextAngle = this.AngleBetweenEdges(nextEdge, startEdge);

		Debug.Log("3 => Différence d'angles : " + startPreviousAngle + "  " + startNextAngle);

		Edge downStreamEdge = null;
		Edge upHillEdge = null;

		if (Math.Abs(90 - startPreviousAngle) > Math.Abs(90 - startNextAngle)) {
			upHillEdge = startEdge;
			downStreamEdge = previousEdge;
		} else {
			downStreamEdge = startEdge;
			upHillEdge = nextEdge;
		}

		Edge linkEdge = new Edge(downStreamEdge.NodeA, upHillEdge.NodeB);
		Vector2 linkMedianPoint = linkEdge.MedianPoint();

		Edge downStreamPreviousEdge = edgeShape.PreviousEdge(downStreamEdge);
		Edge upHillNextEdge = edgeShape.NextEdge(upHillEdge);

		//Edge upHillFollowingEdge = shapeEdges(downStreamEdgeIndex + 

		//linkMedianPoint
	}
}
