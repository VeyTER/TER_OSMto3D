using System;
using System.Collections.Generic;
using UnityEngine;

public class WallsBuilder {
	public GameObject BuildWalls(GameObject building, BuildingNodeGroup buildingNodeGroup, Triangulation triangulation, float expansionFactor = 0) {
		// Création et paramétrage de l'objet 3D destiné à former un mur
		GameObject walls = new GameObject(building.name + "_walls", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.WALLS_TAG,
		};
		walls.transform.SetParent(building.transform, false);
		walls.transform.localPosition = Vector3.zero;

		// Ajout d'une instance du gestionnaire d'interface pour que cette dernière soit déclenchée lors
		// d'un clic
		walls.AddComponent<UiManager>();

		// Affectation du matériau et de sa couleur au mur pour lui donner la texture voulue
		MeshRenderer meshRenderer = walls.GetComponent<MeshRenderer>();
		meshRenderer.material = buildingNodeGroup.CustomMaterial;
		meshRenderer.materials[0].color = buildingNodeGroup.OverlayColor;
		meshRenderer.material.mainTexture.wrapMode = TextureWrapMode.Repeat;

		List<Vector3> vertices = this.WallsVertices(buildingNodeGroup, building.transform.position, triangulation, expansionFactor);
		List<int> triangles = this.WallsTriangles(buildingNodeGroup, building.transform.position, vertices);
		List<Vector2> uvs = this.WallsUv(buildingNodeGroup);

		Mesh newMesh = new Mesh() {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			uv = uvs.ToArray()
		};

		newMesh.RecalculateNormals();
		newMesh.RecalculateBounds();

		MeshFilter meshFilter = walls.GetComponent<MeshFilter>();
		meshFilter.mesh = newMesh;

		MeshCollider wallBoxColliser = walls.AddComponent<MeshCollider>();

		return walls;
	}

	private List<Vector3> WallsVertices(BuildingNodeGroup buildingNodeGroup, Vector3 buildingPosition, Triangulation triangulation, float expansionFactor) {
		List<Vector3> wallVertices = new List<Vector3>();

		BuildingShape buildingShape = triangulation.BuildingShape;
		foreach (Node node in buildingNodeGroup.Nodes) {
			Edge currentEdge = buildingShape.GetEdge(node, 1);
			Edge nextEdge = buildingShape.NextEdge(currentEdge);

			double posX = node.Longitude - buildingPosition.x;
			double posZ = node.Latitude - buildingPosition.z;

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

			float wallHeight = Dimensions.FLOOR_HEIGHT * buildingNodeGroup.NbFloor;
			wallVertices.Add(new Vector3(shiftedPosX, 0, shiftedPosZ));
			wallVertices.Add(new Vector3(shiftedPosX, wallHeight, shiftedPosZ));

			wallVertices.Add(new Vector3(shiftedPosX, 0, shiftedPosZ));
			wallVertices.Add(new Vector3(shiftedPosX, wallHeight, shiftedPosZ));
		}
		return wallVertices;
	}

	private List<int> WallsTriangles(NodeGroup nodeGroup, Vector2 buildingCenter, List<Vector3> wallVertices) {
		List<int> triangles = new List<int>();
		for (int i = 0; i < wallVertices.Count; i += 2) {
			int index = i % (wallVertices.Count - 2);

			triangles.Add(index);
			triangles.Add(index + 2);
			triangles.Add(index + 1);

			triangles.Add(index + 2);
			triangles.Add(index + 3);
			triangles.Add(index + 1);
		}

		if (BuildingsTools.GetInstance().BuildingArea(nodeGroup) < 0)
			triangles.Reverse();

		return triangles;
	}

	private List<Vector2> WallsUv(BuildingNodeGroup buildingNodeGroup) {
		float cursorX = 0;
		List<Vector2> uvVertices = new List<Vector2>();
		for (int i = 0; i < buildingNodeGroup.NodeCount(); i++) {
			Node currentNode = buildingNodeGroup.GetNode(i);
			Node nextNode = buildingNodeGroup.GetNode((i + 1) % buildingNodeGroup.NodeCount());

			float posU = cursorX / Dimensions.FLOOR_HEIGHT;
			float posV = 0;

			uvVertices.Add(new Vector2(posU, posV));
			uvVertices.Add(new Vector2(posU, posV + buildingNodeGroup.NbFloor));

			uvVertices.Add(new Vector2(posU, posV));
			uvVertices.Add(new Vector2(posU, posV + buildingNodeGroup.NbFloor));

			float wallLength = Math.Abs(Vector2.Distance(currentNode.ToVector(), nextNode.ToVector()));
			cursorX += wallLength;
		}

		return uvVertices;
	}
}
