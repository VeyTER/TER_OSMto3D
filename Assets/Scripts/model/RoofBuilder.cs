using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère la génération des toits à partir des triangles créés avec la triangulation de Delauney.
/// </summary>
public class RoofBuilder {
	/// <summary>
	/// 	Créé un toit en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouveau toit.</returns>
	/// <param name="posX">Position en X du toit.</param>
	/// <param name="posZ">Position en Z du toit.</param>
	/// <param name="triangulation">Triangulation de Delauney.</param>
	/// <param name="nbFloor">Nombre d'étages du bâtiments sur lequel le toit va être ajouté.</param>
	/// <param name="floorSize">Hauteur des étages du bâtiment sur lequel le toit va être ajouté.</param>
	public GameObject BuildRoof(GameObject building, NodeGroup nodeGroup, float expansionFactor = 1) {
		Triangulation triangulation = new Triangulation(nodeGroup);
		triangulation.Triangulate(nodeGroup.Name);

		float buildingHeight = nodeGroup.NbFloor * Dimensions.FLOOR_HEIGHT;

		// Création et paramétrage de l'objet 3D destiné à former un toit
		GameObject roof = new GameObject(building.name + "_roof", typeof(MeshFilter), typeof(MeshRenderer), typeof(UiManager)) {
			tag = GoTags.ROOF_TAG
		};
		roof.transform.SetParent(building.transform, false);
		roof.transform.localPosition = new Vector3(0, buildingHeight, 0);

		// Création, construction et texturing du maillage formant un toit
		Dictionary<Node, int> nodesAndIndex = new Dictionary<Node, int>();
		Mesh mesh = new Mesh() {
			vertices = this.RoofVertices(triangulation, building, nodesAndIndex, expansionFactor)
		};
		mesh.triangles = this.RoofTriangles(triangulation, mesh.vertices, building, nodesAndIndex);
		mesh.normals = this.RoofNormals(triangulation);

		// Affectation du maillage au toit pour lui donner la forme voulue
		MeshFilter meshFilter = roof.GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la route pour lui donner la texture voulue
		MeshRenderer meshRenderer = roof.GetComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load (Materials.ROOF) as Material;

		MeshCollider meshCollider = roof.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;

		return roof;
	}


	/// <summary>
	/// 	Définit les points qui constitueront, à plusieurs, les sommets d'au moins un triangle (triangles définis
	/// 	dans la méthode RoofTriangles()).
	/// </summary>
	/// <returns>Points formant le toit.</returns>
	/// <param name="triangulation">Triangulation de Dealauney.</param>
	/// <param name="posX">Position en X du toit.</param>
	/// <param name="posZ">Position en Z du toit.</param>
	private Vector3[] RoofVertices(Triangulation triangulation, GameObject building, Dictionary<Node, int> nodesAndIndex, float expansionFactor) {
		int nbVertex = triangulation.Triangles.Count * 3;
		Vector3[] res = new Vector3[nbVertex];

		Vector2 buildingPosition = new Vector2(building.transform.position.x, building.transform.position.z);

		float jitter = 0F;
		int index = 0;
		foreach(Triangle triangle in triangulation.Triangles) {
			if (!nodesAndIndex.ContainsKey(triangle.NodeA)) {
				float posX2 = (float) (triangle.NodeA.Longitude - buildingPosition.x);
				float posZ2 = (float) (triangle.NodeA.Latitude - buildingPosition.y);

				res[index] = new Vector3(posX2, UnityEngine.Random.Range(-jitter / 2F, jitter / 2F), posZ2);
				nodesAndIndex[triangle.NodeA] = index;

				index++;
			}

			if (!nodesAndIndex.ContainsKey(triangle.NodeB)) {
				float posX2 = (float) (triangle.NodeB.Longitude - buildingPosition.x);
				float posZ2 = (float) (triangle.NodeB.Latitude - buildingPosition.y);

				res[index] = new Vector3(posX2, UnityEngine.Random.Range(-jitter / 2F, jitter / 2F), posZ2);
				nodesAndIndex[triangle.NodeB] = index;

				index++;
			}

			if (!nodesAndIndex.ContainsKey(triangle.NodeC)) {
				float posX2 = (float) (triangle.NodeC.Longitude - buildingPosition.x);
				float posZ2 = (float) (triangle.NodeC.Latitude - buildingPosition.y);

				res[index] = new Vector3(posX2, UnityEngine.Random.Range(-jitter / 2F, jitter / 2F), posZ2);
				nodesAndIndex[triangle.NodeC] = index;

				//this.AddTriangleVertices(triangle, buildingPosition, jitter, res, index, nodesAndIndex);

				index++;
			}
		}
		return res;
	}

	private void AddTriangleVertices(Triangle triangle, Vector2 buildingPosition, float jitter, Vector3[] res, int index, Dictionary<Node, int> nodesAndIndex) {
		float posX2 = (float) (triangle.NodeC.Longitude - buildingPosition.x);
		float posZ2 = (float) (triangle.NodeC.Latitude - buildingPosition.y);

		res[index] = new Vector3(posX2, UnityEngine.Random.Range(-jitter / 2F, jitter / 2F), posZ2);
		nodesAndIndex[triangle.NodeC] = index;
	}

	/// <summary>
	/// 	Créé de 2 triangles qui vont former à eux deux une portion de toit.
	/// </summary>
	/// <returns>Triangles sur le toit.</returns>
	private int[] RoofTriangles(Triangulation triangulation, Vector3[] meshVertices, GameObject building, Dictionary<Node, int> nodesAndIndex) {
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

		return verticesIndex.ToArray();
	}

	/// <summary>
	/// 	Créé les points utilisés pour le placement de la texture sur le toit.
	/// </summary>
	/// <returns>Coordonnées pour le mapping.</returns>
	/// <param name="length">Longueur du toit.</param>
	/// <param name="width">Largeur du toit.</param>
	private Vector2[] RoofUV(float length, float width) {
		Vector2[] res = new Vector2[] {
			new Vector2 (0, 0),			// (x, y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, 1),
			new Vector2 (0, 1)
		};
		return res;
	}


	/// <summary>
	/// 	Permet d'obtenir une texture épurée sur le toit. Sans ce traitement, le rendu de la texture serait vraiment
	/// 	trop sombre.
	/// </summary>
	/// <returns>Normales de la texture.</returns>
	private Vector3[] RoofNormals(Triangulation triangulation) {
		int nbVertex = triangulation.Triangles.Count * 3;

		Vector3[] res = new Vector3[nbVertex];
		for (int i = 0; i < triangulation.Triangles.Count * 3; i++)
			res[i] = Vector3.up;

		return res;
	}
}
