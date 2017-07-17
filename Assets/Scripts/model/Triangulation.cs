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

	private List<Node> convexNodes;
	private List<Node> earTipNodes;
	private List<Node> reflexNodes;

	private EdgeShape edgeShape;
	private List<Triangle> triangles;

	private List<GameObject> testCubes;

	public Triangulation(NodeGroup nodeGroup) {
		this.nodeGroup = nodeGroup;

		this.convexNodes = new List<Node>();
		this.reflexNodes = new List<Node>();
		this.earTipNodes = new List<Node>();

		this.edgeShape = new EdgeShape();
		this.triangles = new List<Triangle>();

		this.testCubes = new List<GameObject>();
	}

	public void Triangulate(string name) {
		if (nodeGroup.NodeCount() >= 3) {
			this.BuildShapeEdges();

			this.LabelVectices();
			this.BuildTriangulation();
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

	private void LabelVectices() {
		convexNodes.Clear();
		earTipNodes.Clear();
		reflexNodes.Clear();

		foreach (GameObject cube in testCubes)
			GameObject.Destroy(cube);
		testCubes.Clear();

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
				convexNodes.Add(currentEdge.NodeB);

				if (this.IsVertexEarTip(new Triangle(currentEdge.NodeA, currentEdge.NodeB, nextEdge.NodeB))) {
					earTipNodes.Add(currentEdge.NodeB);
					this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.green);
				} else {
					this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.yellow);
				}
			} else {
				reflexNodes.Add(currentEdge.NodeB);
				this.AddCube(new Vector3((float) currentEdge.NodeB.Longitude, 0, (float) currentEdge.NodeB.Latitude), Color.red);
			}
		}
	}

	private void AddCube(Vector3 position, Color color, float scale = 0.05F, string name = "cube") {
		//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		//testCubes.Add(cube);

		//cube.transform.position = position;
		//cube.transform.localScale = new Vector3(scale, scale, scale);
		//cube.name = name;

		//MeshRenderer cubeRenderer = cube.GetComponent<MeshRenderer>();
		//cubeRenderer.material.color = color;
	}

	private bool IsVertexConvex(Node testedNode, Edge currentEdge, Edge nextEdge) {
		Vector2 previousVertex = currentEdge.NodeA.ToVector();
		Vector2 testedVertex = currentEdge.NodeB.ToVector();
		Vector2 nextVertex = nextEdge.NodeB.ToVector();

		float testOperation = (testedVertex.x - previousVertex.x) * (nextVertex.y - previousVertex.y)
						    - (nextVertex.x - previousVertex.x) * (testedVertex.y - previousVertex.y);

		return testOperation <= 0;
	}

	private bool IsVertexEarTip(Triangle triangle) {
		bool nonePointInside = true;
		for (int i = 0; i < edgeShape.EdgeCount(); i++) {
			Node testedNode = edgeShape.GetEdge(i).NodeA;
			Vector2 testedVertex = testedNode.ToVector();

			if (!testedNode.Equals(triangle.NodeA) && !testedNode.Equals(triangle.NodeB) && !testedNode.Equals(triangle.NodeC)
			&& !triangle.NodeA.Equals(triangle.NodeC) && !triangle.NodeB.Equals(triangle.NodeA) && !triangle.NodeB.Equals(triangle.NodeC)) {
				if (this.IsVertexInsideTriangle(testedVertex, triangle)) {
					nonePointInside = false;
				}
			}
		}

		return nonePointInside;
	}

	private bool IsVertexInsideTriangle(Vector2 testedVertex, Triangle triangle) {
		bool test1 = this.PointSign2(testedVertex, triangle.NodeA.ToVector(), triangle.NodeB.ToVector()) < 0;
		bool test2 = this.PointSign2(testedVertex, triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		bool test3 = this.PointSign2(testedVertex, triangle.NodeC.ToVector(), triangle.NodeA.ToVector()) < 0;
		return test1 && test2 && test3;
	}

	private float PointSign2(Vector2 testedVertex, Vector2 vertexA, Vector2 vertexB) {
		return (testedVertex.x - vertexB.x) * (vertexA.y - vertexB.y) - (vertexA.x - vertexB.x) * (testedVertex.y - vertexB.y);
	}

	private void BuildTriangulation() {
		int cpt = 100;

		while (edgeShape.EdgeCount() > 3/* && cpt > 0*/ && earTipNodes.Count > 0) {
			this.LabelVectices();

			if (earTipNodes.Count == 0) {
				//Debug.Log(nodeGroup.Id + "  " + nodeGroup.Name + " | cpt : " + cpt + " | edge count : " + edgeShape.EdgeCount() + " | convex count : " + convexNodes.Count + " | reflex count : " + reflexNodes.Count);
				Debug.Log("inversion");

				earTipNodes.AddRange(reflexNodes);
				reflexNodes.Clear();
			}

			int i = 0;
			for (; i < edgeShape.EdgeCount() && !earTipNodes.Contains(edgeShape.GetEdge(i).NodeB); i++) ;

			if (i < edgeShape.EdgeCount()) {
				Edge currentEdge = edgeShape.GetEdge(i);
				Edge nextEdge = edgeShape.NextEdge(i);

				//Debug.Log("nb : " + edgeShape.EdgeCount() + " | " + edgeShape.Edges.IndexOf(currentEdge) + " | " + edgeShape.Edges.IndexOf(nextEdge));

				Node previousNode = currentEdge.NodeA;
				Node earTipNode = currentEdge.NodeB;
				Node nextNode = nextEdge.NodeB;

				Edge newEdge = new Edge(previousNode, nextNode);

				Triangle triangle = new Triangle(previousNode, earTipNode, nextNode);

				edgeShape.RemoveEdge(currentEdge);

				if (!currentEdge.Equals(nextEdge)) {
					edgeShape.ReplaceEdge(nextEdge, newEdge);
					triangles.Add(triangle);
				}
			}

			cpt--;
		}

		if (edgeShape.EdgeCount() == 3) {
			Triangle triangle = new Triangle(edgeShape.GetEdge(0).NodeA, edgeShape.GetEdge(0).NodeB, edgeShape.GetEdge(1).NodeB);
			triangles.Add(triangle);
		}

		edgeShape.Clear();
	}

	public List<Triangle> Triangles {
		get { return triangles; }
	}
}
