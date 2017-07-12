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

	public void Triangulate() {
		if (nodeGroup.NodeCount() >= 3) {
			this.BuildShapeEdges();
			this.FilterVectices();

			Debug.Log(convexVertices.Count + "  " + reflexVertices.Count + "  " + earTipVertices.Count);

			//this.BuildTriangulation();
		}
	}

	private void BuildShapeEdges() {
		for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
			Node currentNode = nodeGroup.GetNode(i);
			Node nextNode = nodeGroup.GetNode(i + 1);

			Edge newEdge = new Edge(currentNode, nextNode, Edge.EdgeTypes.SHAPE);
			edgeShape.AddEdge(newEdge);
		}
	}

	private void FilterVectices() {
		foreach (Edge currentEdge in edgeShape.Edges) {
			if (this.IsConvex(currentEdge.NodeB, currentEdge, edgeShape.NextEdge(currentEdge))) {
				convexVertices.Add(currentEdge.NodeB.ToVector());

				if (this.IsEarTip(currentEdge.NodeB))
					earTipVertices.Add(currentEdge.NodeB.ToVector());
			} else {
				reflexVertices.Add(currentEdge.NodeB.ToVector());
				GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				obj.transform.position = new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude);
				obj.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
			}
		}
	}

	private bool IsConvex(Node testedNode, Edge currentEdge, Edge nextEdge) {
		//Vector2 testedVertex = testedNode.ToVector();
		//VertexSides previousSide = VertexSides.NONE;

		//for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
		//	if (!testedNode.Equals(nodeGroup.GetNode(i))) {
		//		Vector2 currentVertex = nodeGroup.GetNode(i).ToVector();
		//		Vector2 nextVertex = nodeGroup.GetNode(i + 1).ToVector();

		//		Vector2 affineSegment = this.Affine(nextVertex, currentVertex);
		//		Vector2 affinePoint = this.Affine(testedVertex, currentVertex);

		//		VertexSides currentSide = this.VertexSide(affineSegment, affinePoint);

		//		if (currentSide == VertexSides.NONE) {
		//			//Debug.Log("ok 1");
		//			return false;
		//		} else if (previousSide == VertexSides.NONE) {
		//			//Debug.Log("ok 2");
		//			previousSide = currentSide;
		//		} else if (previousSide != currentSide) {
		//			//Debug.Log("ok 3");
		//			return false;
		//		}
		//	}
		//}

		//Debug.Log("ok 4");

		Debug.Log((currentEdge.Orientation() - nextEdge.Orientation()) / (2F * Math.PI) * 360);

		return Math.Abs(currentEdge.Orientation() - nextEdge.Orientation()) / (2F * Math.PI) * 360 < 180;
	}

	private Vector2 Affine(Vector2 vertexA, Vector2 vertexB) {
		return new Vector2(vertexA.x - vertexB.x, vertexA.y - vertexB.y);
	}

	private VertexSides VertexSide(Vector2 affineA, Vector2 affineB) {
		if ((affineA.x == 0 && affineB.y == 0) || (affineB.y == 0 && affineB.x == 0))
			Debug.Log("Aïe aïe aïe");

		float xProduct = this.XProduct(affineA, affineB);

		Debug.Log("( " + affineA.x + " ; " + affineA.y + " ) - ( " + affineB.x + " ; " + affineB.y + " )  ==>" + xProduct);

		if (xProduct < 0)
			return VertexSides.LEFT;
		else if (xProduct > 0)
			return VertexSides.RIGHT;
		else
			return VertexSides.NONE;
	}

	private float XProduct(Vector2 affineA, Vector2 affineB) {
		return affineA.x * affineB.y - affineA.y * affineB.x;
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
