using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class BuildingShape {
	private List<Edge> edgesShape;

	private List<Node> convexNodes;
	private List<Node> earTipNodes;
	private List<Node> reflexNodes;

	public BuildingShape() {
		this.edgesShape = new List<Edge>();

		this.convexNodes = new List<Node>();
		this.reflexNodes = new List<Node>();
		this.earTipNodes = new List<Node>();
	}

	public void ReplaceEdges(List<Edge> newEdges) {
		edgesShape.Clear();
		edgesShape.AddRange(newEdges);
		this.LabelVertex();		
	}

	public Edge AddEdge(Edge newEdge, bool labelVertices) {
		edgesShape.Add(newEdge);
		if(labelVertices)
			this.LabelVertex();
		return newEdge;
	}

	public Edge GetEdge(int index) {
		return edgesShape[index];
	}

	public void ReplaceEdge(int index, Edge newEdge, bool labelVertices) {
		edgesShape[index] = newEdge;
		if (labelVertices)
			this.LabelVertex();
	}

	public void ReplaceEdge(Edge oldEdge, Edge newEdge, bool labelVertices) {
		int oldEdgeIndex = edgesShape.IndexOf(oldEdge);
		if (labelVertices)
			this.LabelVertex();
		edgesShape[oldEdgeIndex] = newEdge;
	}

	public Edge RemoveEdge(int index, bool labelVertices) {
		Edge oldEdge = edgesShape[index];
		edgesShape.Remove(oldEdge);
		if (labelVertices)
			this.LabelVertex();
		return oldEdge;
	}

	public Edge RemoveEdge(Edge oldEdge, bool labelVertices) {
		edgesShape.Remove(oldEdge);
		if (labelVertices)
			this.LabelVertex();
		return oldEdge;
	}

	public void Clear() {
		edgesShape.Clear();

		this.convexNodes.Clear();
		this.reflexNodes.Clear();
		this.earTipNodes.Clear();
	}

	public int EdgeCount() {
		return edgesShape.Count;
	}

	public Edge PreviousEdge(Edge flagEdge) {
		int flagEdgeIndex = edgesShape.IndexOf(flagEdge);
		int previousEdgeIndex = flagEdgeIndex != 0 ? (flagEdgeIndex - 1) : edgesShape.Count - 1;
		return edgesShape[previousEdgeIndex];
	}

	public Edge PreviousEdge(int index) {
		int previousEdgeIndex = index != 0 ? (index - 1) : edgesShape.Count - 1;
		return edgesShape[previousEdgeIndex];
	}

	public Edge NextEdge(Edge flagEdge) {
		int flagEdgeIndex = edgesShape.IndexOf(flagEdge);
		int nextEdgeIndex = (flagEdgeIndex + 1) % edgesShape.Count;
		return edgesShape[nextEdgeIndex];
	}

	public Edge NextEdge(int index) {
		int nextEdgeIndex = (index + 1) % edgesShape.Count;
		return edgesShape[nextEdgeIndex];
	}

	private void LabelVertex() {
		for (int i = 0; i < this.EdgeCount(); i++) {
			Edge currentEdge = this.GetEdge(i);
			Edge nextEdge = this.NextEdge(currentEdge);

			if (this.IsVertexConvex(currentEdge.NodeB, currentEdge, nextEdge)) {
				convexNodes.Add(currentEdge.NodeB);

				if (this.IsVertexEarTip(new Triangle(currentEdge.NodeA, currentEdge.NodeB, nextEdge.NodeB)))
					earTipNodes.Add(currentEdge.NodeB);
			} else {
				reflexNodes.Add(currentEdge.NodeB);
			}
		}
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
		for (int i = 0; i < edgesShape.Count; i++) {
			Node testedNode = edgesShape[i].NodeA;
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
		bool test1 = this.PointSign(testedVertex, triangle.NodeA.ToVector(), triangle.NodeB.ToVector()) < 0;
		bool test2 = this.PointSign(testedVertex, triangle.NodeB.ToVector(), triangle.NodeC.ToVector()) < 0;
		bool test3 = this.PointSign(testedVertex, triangle.NodeC.ToVector(), triangle.NodeA.ToVector()) < 0;
		return test1 && test2 && test3;
	}

	private float PointSign(Vector2 testedVertex, Vector2 vertexA, Vector2 vertexB) {
		return (testedVertex.x - vertexB.x) * (vertexA.y - vertexB.y) - (vertexA.x - vertexB.x) * (testedVertex.y - vertexB.y);
	}

	public Edge SmallestEdge() {
		Edge res = edgesShape[0];
		foreach (Edge edge in edgesShape) {
			if (edge.Length() < res.Length())
				res = edge;
		}
		return res;
	}

	public List<Edge> Edges {
		get { return edgesShape; }
	}

	public List<Node> ConvexNodes {
		get { return convexNodes; }
	}

	public List<Node> EarTipNodes {
		get { return earTipNodes; }
	}

	public List<Node> ReflexNodes {
		get { return reflexNodes; }
	}
}