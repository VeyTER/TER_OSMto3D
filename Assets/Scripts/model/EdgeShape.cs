using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EdgeShape {
	private List<Edge> edges;

	public EdgeShape() {
		this.edges = new List<Edge>();
	}

	public Edge AddEdge(Edge newEdge) {
		edges.Add(newEdge);
		return newEdge;
	}

	public Edge GetEdge(int index) {
		return edges[index];
	}

	public void ReplaceEdge(int index, Edge newEdge) {
		edges[index] = newEdge;
	}

	public void ReplaceEdge(Edge oldEdge, Edge newEdge) {
		int oldEdgeIndex = edges.IndexOf(oldEdge);
		edges[oldEdgeIndex] = newEdge;
	}

	public Edge RemoveEdge(int index) {
		Edge oldEdge = edges[index];
		edges.Remove(oldEdge);
		return oldEdge;
	}

	public Edge RemoveEdge(Edge oldEdge) {
		edges.Remove(oldEdge);
		return oldEdge;
	}

	public void Clear() {
		edges.Clear();
	}

	public int EdgeCount() {
		return edges.Count;
	}

	public Edge PreviousEdge(Edge flagEdge) {
		int flagEdgeIndex = edges.IndexOf(flagEdge);
		int previousEdgeIndex = flagEdgeIndex != 0 ? (flagEdgeIndex - 1) : edges.Count - 1;
		return edges[previousEdgeIndex];
	}

	public Edge PreviousEdge(int index) {
		int previousEdgeIndex = index != 0 ? (index - 1) : edges.Count - 1;
		return edges[previousEdgeIndex];
	}

	public Edge NextEdge(Edge flagEdge) {
		int flagEdgeIndex = edges.IndexOf(flagEdge);
		int nextEdgeIndex = (flagEdgeIndex + 1) % edges.Count;
		return edges[nextEdgeIndex];
	}

	public Edge NextEdge(int index) {
		int nextEdgeIndex = (index + 1) % edges.Count;
		return edges[nextEdgeIndex];
	}

	public Edge SmallestEdge() {
		Edge res = edges[0];
		foreach (Edge edge in edges) {
			if (edge.Length() < res.Length())
				res = edge;
		}
		return res;
	}

	public List<Edge> Edges {
		get { return edges; }
	}
}