﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère la génération des toits à partir des triangles créés avec la triangulation de Delauney.
/// </summary>
public class RoofBuilder {
	public GameObject BuildFlatRoof(GameObject building, BuildingNodeGroup buildingNodeGroup, Triangulation triangulation, float expansionFactor = 0) {
		// Création, construction et texturing du maillage formant un toit
		Dictionary<Node, int> nodesAndIndex = new Dictionary<Node, int>();

		List<Vector3> vertices = this.FlatRoofVertices(triangulation, building, nodesAndIndex, expansionFactor);
		List<int> triangles = this.FlatRoofTriangles(triangulation, vertices.ToArray(), building, nodesAndIndex);
		List<Vector3> normals = this.FlatRoofNormals(triangulation);

		Mesh mesh = new Mesh() {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			normals = normals.ToArray()
		};

		// Création et paramétrage de l'objet 3D destiné à former un toit
		GameObject roof = this.BuildRoof(building, buildingNodeGroup, mesh);

		return roof;
	}

	public GameObject BuildHippedRoof(GameObject building, BuildingNodeGroup buildingNodeGroup, Triangulation triangulation, float expansionFactor) {
		List<Vector3> bottomVertices = new List<Vector3>();
		List<Vector3> topVertices = new List<Vector3>();

		Dictionary <Node, int> topNodesAndIndex = new Dictionary<Node, int>();
		Dictionary<Node, int> bottomNodesAndIndex = new Dictionary<Node, int>();

		List<Vector3> vertices = this.HippedRoofVertices(triangulation, building, out bottomVertices, out topVertices, bottomNodesAndIndex, topNodesAndIndex, expansionFactor);
		List<int> triangles = this.HippedRoofTriangles(triangulation, bottomVertices.ToArray(), topVertices.ToArray(), vertices.ToArray(), building, bottomNodesAndIndex, topNodesAndIndex);

		Mesh mesh = new Mesh() {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray()
		};

		// Création et paramétrage de l'objet 3D destiné à former un toit
		GameObject roof = this.BuildRoof(building, buildingNodeGroup, mesh);

		MeshFilter roofMeshFilter = roof.GetComponent<MeshFilter>();
		roofMeshFilter.mesh.RecalculateNormals();
		roofMeshFilter.mesh.RecalculateBounds();

		return roof;
	}

	private GameObject BuildRoof(GameObject building, BuildingNodeGroup buildingNodeGroup, Mesh mesh) {
		float buildingHeight = buildingNodeGroup.NbFloor * Dimensions.FLOOR_HEIGHT;

		// Création et paramétrage de l'objet 3D destiné à former un toit
		GameObject roof = new GameObject(building.name + "_roof", typeof(MeshFilter), typeof(MeshRenderer), typeof(UiManager)) {
			tag = GoTags.ROOF_TAG
		};
		roof.transform.SetParent(building.transform, false);
		roof.transform.localPosition = new Vector3(0, buildingHeight, 0);

		MeshFilter roofMeshFilter = roof.GetComponent<MeshFilter>();
		roofMeshFilter.mesh = mesh;

		// Affectation du matériau à la route pour lui donner la texture voulue
		MeshRenderer meshRenderer = roof.GetComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load(Materials.ROOF) as Material;

		MeshCollider meshCollider = roof.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = roofMeshFilter.mesh;

		return roof;
	}

	private List<Vector3> HippedRoofVertices(Triangulation triangulation, GameObject building, out List<Vector3> bottomVertices, out List<Vector3> topVertices, Dictionary<Node, int> bottomNodesAndIndex, Dictionary<Node, int> topNodesAndIndex, float expansionFactor) {
		List<Vector3> allVertices = new List<Vector3>();

		// Création, construction et texturing du maillage formant un toit
		topVertices = this.FlatRoofVertices(triangulation, building, topNodesAndIndex, expansionFactor);
		for (int i = 0; i < topVertices.Count; i++) {
			Vector3 topVertex = topVertices[i];
			topVertices[i] = new Vector3(topVertex.x, topVertex.y + Dimensions.ROOF_HEIGHT, topVertex.z);
		}

		bottomVertices = this.FlatRoofVertices(triangulation, building, bottomNodesAndIndex, 0);
		for (int i = 0; i < 3; i++)
			allVertices.AddRange(bottomVertices);
		for (int i = 0; i < 3; i++)
			allVertices.AddRange(topVertices);

		return allVertices;
	}

	/// <summary>
	/// 	Définit les points qui constitueront, à plusieurs, les sommets d'au moins un triangle (triangles définis
	/// 	dans la méthode RoofTriangles()).
	/// </summary>
	/// <returns>Points formant le toit.</returns>
	/// <param name="triangulation">Triangulation de Dealauney.</param>
	/// <param name="posX">Position en X du toit.</param>
	/// <param name="posZ">Position en Z du toit.</param>
	private List<Vector3> FlatRoofVertices(Triangulation triangulation, GameObject building, Dictionary<Node, int> nodesAndIndex, float expansionFactor) {
		int nbVertex = triangulation.Triangles.Count * 3;
		Vector3[] roofVertices = new Vector3[nbVertex];

		int index = 0;
		foreach (Triangle triangle in triangulation.Triangles) {
			if (!nodesAndIndex.ContainsKey(triangle.NodeA))
				this.AddTriangleVertices(triangulation, triangle.NodeA, building.transform.position, roofVertices, ref index, nodesAndIndex, expansionFactor);

			if (!nodesAndIndex.ContainsKey(triangle.NodeB))
				this.AddTriangleVertices(triangulation, triangle.NodeB, building.transform.position, roofVertices, ref index, nodesAndIndex, expansionFactor);

			if (!nodesAndIndex.ContainsKey(triangle.NodeC))
				this.AddTriangleVertices(triangulation, triangle.NodeC, building.transform.position, roofVertices, ref index, nodesAndIndex, expansionFactor);
		}
		return new List<Vector3>(roofVertices);
	}

	private void AddTriangleVertices(Triangulation triangulation, Node node, Vector3 buildingPosition, Vector3[] roofVertices, ref int index, Dictionary<Node, int> nodesAndIndex, float expansionFactor) {
		const float JITTER = 0F;

		double posX = node.Longitude - buildingPosition.x;
		double posY = UnityEngine.Random.Range(-JITTER / 2F, JITTER / 2F);
		double posZ = node.Latitude - buildingPosition.z;

		BuildingShape buildingShape = triangulation.BuildingShape;

		Edge currentEdge = buildingShape.GetEdge(node, 1);
		Edge nextEdge = buildingShape.NextEdge(currentEdge);

		double currentEdgeOrientation = currentEdge.InvertedCopy().Orientation();
		double nextEdgeOrientation = nextEdge.Orientation();

		double bissectrixOrientation = Math.PI - (currentEdgeOrientation + (nextEdgeOrientation - currentEdgeOrientation) / 2F) + (Math.PI / 2F);

		float shiftedPosX = 0;
		float shiftedPosZ = 0;
		if (currentEdgeOrientation < nextEdgeOrientation) {
			shiftedPosX = (float) (posX + Math.Sin(bissectrixOrientation) * expansionFactor);
			shiftedPosZ = (float) (posZ + Math.Cos(bissectrixOrientation) * expansionFactor);
		} else {
			shiftedPosX = (float) (posX - Math.Sin(bissectrixOrientation) * expansionFactor);
			shiftedPosZ = (float) (posZ - Math.Cos(bissectrixOrientation) * expansionFactor);
		}

		roofVertices[index] = new Vector3(shiftedPosX, (float) posY, shiftedPosZ);
		nodesAndIndex[node] = index;
		index++;
	}

	/// <summary>
	/// 	Créé de 2 triangles qui vont former à eux deux une portion de toit.
	/// </summary>
	/// <returns>Triangles sur le toit.</returns>
	private List<int> FlatRoofTriangles(Triangulation triangulation, Vector3[] meshVertices, GameObject building, Dictionary<Node, int> nodesAndIndex) {
		Vector2 buildingPosition = new Vector2(building.transform.position.x, building.transform.position.z);

		List<int> verticesIndex = new List<int>();
		foreach (Triangle triangle in triangulation.Triangles) {
			int indexA = nodesAndIndex[triangle.NodeA];
			int indexB = nodesAndIndex[triangle.NodeB];
			int indexC = nodesAndIndex[triangle.NodeC];

			verticesIndex.Add(indexA);
			verticesIndex.Add(indexB);
			verticesIndex.Add(indexC);
		}

		return verticesIndex;
	}

	private List<int> HippedRoofTriangles(Triangulation triangulation, Vector3[] bottomVertices, Vector3[] topVertices, Vector3[] allVertices, GameObject building, Dictionary<Node, int> bottomNodesAndIndex, Dictionary<Node, int> topNodesAndIndex) {
		List<int> bottomTriangles = this.FlatRoofTriangles(triangulation, bottomVertices, building, bottomNodesAndIndex);
		List<int> topTriangles = this.FlatRoofTriangles(triangulation, topVertices, building, topNodesAndIndex);
		for (int i = 0; i < topTriangles.Count; i++)
			topTriangles[i] += allVertices.Length / 2;

		List<int> middleTriangles = new List<int>();
		BuildingShape buildingShape = triangulation.BuildingShape;
		for (int i = 0; i < buildingShape.EdgeCount(); i++) {
			int edgeIndex = i;
			Edge edge = buildingShape.GetEdge(edgeIndex);

			Node currentNode = edge.NodeA;
			Node nextNode = edge.NodeB;

			if (i == buildingShape.EdgeCount() - 1)
				nextNode = buildingShape.GetEdge(0).NodeA;

			int bottomCurrentNodeIndex = bottomNodesAndIndex[currentNode] + bottomVertices.Length * 2;
			int topCurrentNodeIndex = topNodesAndIndex[currentNode] + bottomVertices.Length * 2 + allVertices.Length / 2;

			int bottomNextNodeIndex = bottomNodesAndIndex[nextNode] + bottomVertices.Length;
			int topNextNodeIndex = topNodesAndIndex[nextNode] + bottomVertices.Length + allVertices.Length / 2;

			middleTriangles.Add(bottomCurrentNodeIndex);
			middleTriangles.Add(bottomNextNodeIndex);
			middleTriangles.Add(topNextNodeIndex);

			middleTriangles.Add(bottomCurrentNodeIndex);
			middleTriangles.Add(topNextNodeIndex);
			middleTriangles.Add(topCurrentNodeIndex);
		}

		List<int> roofTriangles = new List<int>();
		roofTriangles.AddRange(topTriangles);
		roofTriangles.AddRange(bottomTriangles);
		roofTriangles.AddRange(middleTriangles);

		return roofTriangles;
	}

	/// <summary>
	/// 	Créé les points utilisés pour le placement de la texture sur le toit.
	/// </summary>
	/// <returns>Coordonnées pour le mapping.</returns>
	/// <param name="length">Longueur du toit.</param>
	/// <param name="width">Largeur du toit.</param>
	private List<Vector2> FlatRoofUV(float length, float width) {
		Vector2[] res = new Vector2[] {
			new Vector2 (0, 0),			// (x, y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, 1),
			new Vector2 (0, 1)
		};
		return new List<Vector2>(res);
	}


	/// <summary>
	/// 	Permet d'obtenir une texture épurée sur le toit. Sans ce traitement, le rendu de la texture serait vraiment
	/// 	trop sombre.
	/// </summary>
	/// <returns>Normales de la texture.</returns>
	private List<Vector3> FlatRoofNormals(Triangulation triangulation) {
		int nbVertex = triangulation.Triangles.Count * 3;

		Vector3[] res = new Vector3[nbVertex];
		for (int i = 0; i < triangulation.Triangles.Count * 3; i++)
			res[i] = Vector3.up;

		return new List<Vector3>(res);
	}
}
