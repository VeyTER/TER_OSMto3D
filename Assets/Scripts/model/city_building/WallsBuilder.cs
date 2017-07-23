using System;
using System.Collections.Generic;
using UnityEngine;

public class WallsBuilder {
	public GameObject BuildWalls(GameObject building, BuildingNodeGroup buildingNodeGroup) {
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

		MeshFilter wallMeshFilter = walls.GetComponent<MeshFilter>();

		wallMeshFilter.mesh.vertices = this.WallsVertices(buildingNodeGroup, building.transform.position);
		wallMeshFilter.mesh.triangles = this.WallsTriangles(buildingNodeGroup, building.transform.position, wallMeshFilter.mesh.vertices);
		wallMeshFilter.mesh.uv = this.WallsUv(buildingNodeGroup);

		wallMeshFilter.mesh.RecalculateNormals();
		wallMeshFilter.mesh.RecalculateBounds();

		MeshCollider wallBoxColliser = walls.AddComponent<MeshCollider>();

		return walls;
	}

	private Vector3[] WallsVertices(BuildingNodeGroup buildingNodeGroup, Vector3 buildingPosition) {
		List<Vector3> wallVertices = new List<Vector3>();
		for (int i = 0; i < buildingNodeGroup.NodeCount(); i++) {
			Node node = buildingNodeGroup.GetNode(i);

			float wallHeight = Dimensions.FLOOR_HEIGHT * buildingNodeGroup.NbFloor;
			wallVertices.Add(new Vector3((float) (node.Longitude - buildingPosition.x), 0, (float) (node.Latitude - buildingPosition.z)));
			wallVertices.Add(new Vector3((float) (node.Longitude - buildingPosition.x), wallHeight, (float) (node.Latitude - buildingPosition.z)));

			wallVertices.Add(new Vector3((float) (node.Longitude - buildingPosition.x), 0, (float) (node.Latitude - buildingPosition.z)));
			wallVertices.Add(new Vector3((float) (node.Longitude - buildingPosition.x), wallHeight, (float) (node.Latitude - buildingPosition.z)));
		}
		return wallVertices.ToArray();
	}

	private int[] WallsTriangles(NodeGroup nodeGroup, Vector2 buildingCenter, Vector3[] wallVertices) {
		List<int> triangles = new List<int>();
		for (int i = 0; i < wallVertices.Length; i += 2) {
			int index = i % (wallVertices.Length - 2);

			triangles.Add(index);
			triangles.Add(index + 2);
			triangles.Add(index + 1);

			triangles.Add(index + 2);
			triangles.Add(index + 3);
			triangles.Add(index + 1);
		}

		if (BuildingsTools.GetInstance().BuildingArea(nodeGroup) < 0)
			triangles.Reverse();

		return triangles.ToArray();
	}

	private Vector2[] WallsUv(BuildingNodeGroup buildingNodeGroup) {
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

		return uvVertices.ToArray();
	}
}
