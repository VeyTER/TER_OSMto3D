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
			this.LabelVectices(name);

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

	private void LabelVectices(string name) {
		float sum = 0;
		for (int i = 0; i < nodeGroup.NodeCount() - 2; i++) {
			Vector2 currentPoint = nodeGroup.GetNode(i).ToVector();
			Vector2 nextPoint = nodeGroup.GetNode(i + 1).ToVector();

			sum += (nextPoint.x - currentPoint.x) * (nextPoint.y + currentPoint.y);
		}

		for (int i = 0; i < edgeShape.EdgeCount(); i++) {
			Edge currentEdge = edgeShape.GetEdge(i);
			Edge nextEdge = edgeShape.NextEdge(currentEdge);

			if (this.IsVertexConvex(currentEdge.NodeB, currentEdge, nextEdge)) {
				convexVertices.Add(currentEdge.NodeB.ToVector());

				if (this.IsVertexEarTip(new Triangle(currentEdge.NodeA, currentEdge.NodeB, nextEdge.NodeB))) {
					earTipVertices.Add(currentEdge.NodeB.ToVector());
					this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.green);
				} else {
					this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.yellow);
				}
			} else {
				reflexVertices.Add(currentEdge.NodeB.ToVector());
				this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.red);
			}
		}
	}

	private void AddCube(Vector3 position, Color color, float scale = 0.05F, string name = "cube") {
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

		cube.transform.position = position;
		cube.transform.localScale = new Vector3(scale, scale, scale);
		cube.name = name;

		MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
		cubeRenderer.material.color = color;
	}

	private bool IsVertexConvex(Node testedNode, Edge currentEdge, Edge nextEdge) {
		Vector2 previousVertex = currentEdge.NodeA.ToVector();
		Vector2 testedVertex = currentEdge.NodeB.ToVector();
		Vector2 nextVertex = nextEdge.NodeB.ToVector();

		float testOperation = (testedVertex.x - previousVertex.x) * (nextVertex.y - previousVertex.y)
						    - (nextVertex.x - previousVertex.x) * (testedVertex.y - previousVertex.y);

		return testOperation <= 0;
	}

	private bool IsVertexEarTip(Triangle currentTriangle) {
		bool nonePointInsideTriangle = true;
		for (int i = 0; i < edgeShape.EdgeCount(); i++) {
			Node testedNode = edgeShape.GetEdge(i).NodeA;
			Vector2 testedVertex = testedNode.ToVector();

			if (!testedNode.Equals(currentTriangle.NodeA) && !testedNode.Equals(currentTriangle.NodeB) && !testedNode.Equals(currentTriangle.NodeC)
			&& !currentTriangle.NodeA.Equals(currentTriangle.NodeC) && !currentTriangle.NodeB.Equals(currentTriangle.NodeA) && !currentTriangle.NodeB.Equals(currentTriangle.NodeC)) {
				if (this.IsVertexInsideTriangle(testedVertex, currentTriangle)) {
					nonePointInsideTriangle = false;
				}
			}
		}

		return nonePointInsideTriangle;
	}

	private bool IsVertexInsideTriangle(Vector2 testedVertex, Triangle triangle) {
		bool test1 = this.PointSign(testedVertex, triangle.NodeA.ToVector(), triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		bool test2 = this.PointSign(testedVertex, triangle.NodeB.ToVector(), triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		bool test3 = this.PointSign(testedVertex, triangle.NodeC.ToVector(), triangle.NodeA.ToVector(), triangle.NodeB.ToVector()) < 0;
		return test1 && test2 && test3;

		//bool test1 = this.PointSign2(testedVertex, triangle.NodeA.ToVector(), triangle.NodeB.ToVector()) < 0;
		//bool test2 = this.PointSign2(testedVertex, triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		//bool test3 = this.PointSign2(testedVertex, triangle.NodeC.ToVector(), triangle.NodeA.ToVector()) < 0;
		//return test1 && test2 && test3;
	}

	private float PointSign(Vector2 testedVertex, Vector2 vertexA, Vector2 vertexB, Vector2 vertexC) {
		Vector2 deltaAB = vertexB - vertexA;
		Vector2 deltaAT = testedVertex - vertexA;
		Vector2 deltaAC = vertexC - vertexA;

		Vector3 crossProductABT = Vector3.Cross(new Vector3(deltaAB.x, deltaAB.y, 0), new Vector3(deltaAT.x, deltaAT.y, 0));
		Vector3 crossProductABC = Vector3.Cross(new Vector3(deltaAB.x, deltaAB.y, 0), new Vector3(deltaAC.x, deltaAC.y, 0));

		return Vector3.Dot(crossProductABT, crossProductABC);
	}

	//private float PointSign2(Vector2 testedVertex, Vector2 vertexA, Vector2 vertexB) {
	//	return (testedVertex.x - vertexB.x) * (vertexA.y - vertexB.y) - (vertexA.x - vertexB.x) * (testedVertex.y - vertexB.y);
	//}

	private void BuildTriangulation() {
		//Edge startEdge = this.SmallestEdge();

		//Edge previousEdge = edgeShape.PreviousEdge(startEdge);
		//Edge nextEdge = edgeShape.NextEdge(startEdge);

		//float startPreviousAngle = this.AngleBetweenEdges(previousEdge, startEdge);
		//float startNextAngle = this.AngleBetweenEdges(nextEdge, startEdge);

		//Debug.Log("3 => Différence d'angles : " + startPreviousAngle + "  " + startNextAngle);

		//Edge downStreamEdge = null;
		//Edge upHillEdge = null;

		//if (Math.Abs(90 - startPreviousAngle) > Math.Abs(90 - startNextAngle)) {
		//	upHillEdge = startEdge;
		//	downStreamEdge = previousEdge;
		//} else {
		//	downStreamEdge = startEdge;
		//	upHillEdge = nextEdge;
		//}

		//Edge linkEdge = new Edge(downStreamEdge.NodeA, upHillEdge.NodeB);
		//Vector2 linkMedianPoint = linkEdge.MedianPoint();

		//Edge downStreamPreviousEdge = edgeShape.PreviousEdge(downStreamEdge);
		//Edge upHillNextEdge = edgeShape.NextEdge(upHillEdge);

		//Edge upHillFollowingEdge = shapeEdges(downStreamEdgeIndex + 

		//linkMedianPoint
	}
}
