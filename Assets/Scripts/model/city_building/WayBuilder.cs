using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère la génération des routes.
/// </summary>
public class WayBuilder {
	public GameObject BuildRoad(HighwayNodeGroup highwayNodeGroup) {
		GameObject road = this.BuildWay(highwayNodeGroup, "Road", GoTags.ROAD_TAG, Materials.ROAD, Dimensions.ROAD_WIDTH);

		Vector3 roadPosition = road.transform.position;
		road.transform.position = new Vector3(roadPosition.x, Dimensions.ROAD_ELEVATION, roadPosition.z);

		return road;
	}
	public GameObject BuildCycleway(HighwayNodeGroup highwayNodeGroup) {
		return this.BuildWay(highwayNodeGroup, "Cycleway", GoTags.CYCLEWAY_TAG, Materials.CYCLEWAY, Dimensions.CYCLEWAYS_WIDTH);
	}

	public GameObject BuildFootway(HighwayNodeGroup highwayNodeGroup) {
		return this.BuildWay(highwayNodeGroup, "Footway", GoTags.FOOTWAY_TAG, Materials.FOOTWAY, Dimensions.FOOTWAY_WIDTH);
	}

	public GameObject BuildBusLane(HighwayNodeGroup highwayNodeGroup) {
		return this.BuildWay(highwayNodeGroup, "BusLane", GoTags.BUS_LANE_TAG, Materials.BUSWAY, Dimensions.BUS_LANE_WIDTH);
	}

	public GameObject BuildWaterway(WaterwayNodeGroup waterwayNodeGroup) {
		return this.BuildWay(waterwayNodeGroup, "Waterway", GoTags.WALLS_TAG, Materials.WATERWAY, Dimensions.WTERWAY_WIDTH);
	}

	private GameObject BuildWay(NodeGroup nodeGroup, string wayIdentifier, string tag, string materialName, float width) {
		if (nodeGroup.NodeCount() >= 2) {
			string nameComplement = nodeGroup.GetNode(0).Reference + "-" + nodeGroup.GetNode(nodeGroup.NodeCount() - 1).Reference;

			nodeGroup.Name = wayIdentifier + "_" + nameComplement;
			GameObject way = new GameObject(wayIdentifier + "_" + nameComplement);
			way.transform.position = new Vector3((float) nodeGroup.GetNode(0).Longitude, Dimensions.WAY_ELEVATION, (float) nodeGroup.GetNode(0).Latitude);

			GameObject sections = new GameObject(wayIdentifier + "Sections_" + nameComplement, typeof(MeshFilter), typeof(MeshRenderer)) {
				tag = tag
			};
			sections.transform.SetParent(way.transform, false);
			sections.transform.localPosition = Vector3.zero;

			List<Vector3> sectionsVertices = this.WayVertices(nodeGroup, sections.transform.position, width);
			List<int> sectionsTriangles = this.WayTriangles(sectionsVertices);
			List<Vector2> sectionsUv = this.WayUv(nodeGroup, sectionsVertices, width);
			List<Vector3> sectionsNormals = this.WayNormals(sectionsVertices);

			// Création, construction et texturing du maillage formant la route
			Mesh sectionsMesh = new Mesh() {
				vertices = sectionsVertices.ToArray(),
				triangles = sectionsTriangles.ToArray(),
				uv = sectionsUv.ToArray(),
				normals = sectionsNormals.ToArray()
			};

			// Affectation du maillage à la route pour lui donner la forme voulue
			MeshFilter meshFilter = sections.GetComponent<MeshFilter>();
			meshFilter.mesh = sectionsMesh;

			// Affectation du matériau à la route pour lui donner la texture voulue
			MeshRenderer meshRenderer = sections.GetComponent<MeshRenderer>();
			meshRenderer.material = Resources.Load(materialName) as Material;

			return way;
		} else {
			return null;
		}
	}

	private List<Vector3> WayVertices(NodeGroup nodeGroup, Vector3 position, float width) {
		List<Vector3> vertices = new List<Vector3>();
		for (int i = 0; i < nodeGroup.NodeCount() - 1; i++) {
			Vector2 currentNodePos = nodeGroup.GetNode(i).ToVector();
			Vector2 nextNodePos = nodeGroup.GetNode(i + 1).ToVector();

			double currentSectionLengthDeltaX = nextNodePos.x - currentNodePos.x;
			double currentSectionLengthDeltaY = nextNodePos.y - currentNodePos.y;
			double currentSectionOrientation = Math.Atan2(currentSectionLengthDeltaY, currentSectionLengthDeltaX);

			double widthDeltaX = (width / 2) * Math.Cos(currentSectionOrientation + Math.PI / 2F);
			double widthDeltaY = (width / 2) * Math.Sin(currentSectionOrientation + Math.PI / 2F);

			Vector2 bottomVertex = Vector2.zero;
			Vector2 topVertex = Vector2.zero;

			if (i == 0) {
				bottomVertex = new Vector2((float) (currentNodePos.x - widthDeltaX - position.x), (float) (currentNodePos.y - widthDeltaY - position.z));
				topVertex = new Vector2((float) (currentNodePos.x + widthDeltaX - position.x), (float) (currentNodePos.y + widthDeltaY - position.z));

				vertices.Add(new Vector3(bottomVertex.x, 0, bottomVertex.y));
				vertices.Add(new Vector3(topVertex.x, 0, topVertex.y));
			}

			if (i < nodeGroup.NodeCount() - 2) {
				Node nextNextNode = nodeGroup.GetNode(i + 2);
				Vector2 nextNextNodePos = nextNextNode.ToVector();

				double nextSectionLengthDeltaX = nextNextNodePos.x - nextNodePos.x;
				double nextSectionLengthDeltaY = nextNextNodePos.y - nextNodePos.y;
				double nextSectionOrientation = Math.Atan2(nextSectionLengthDeltaX, nextSectionLengthDeltaY);
				double invertedCurrentSectionOrientation = Math.Atan2(currentNodePos.x - nextNodePos.x, currentNodePos.y - nextNodePos.y);

				double bissectrixOrientation = invertedCurrentSectionOrientation + (nextSectionOrientation - invertedCurrentSectionOrientation) / 2F;
				double linkPointDistance = Math.Tan(-(bissectrixOrientation - (nextSectionOrientation - Math.PI / 2F))) * (width / 2F);

				double linkPointDeltaX = linkPointDistance * Math.Cos(currentSectionOrientation);
				double linkPointDeltaY = linkPointDistance * Math.Sin(currentSectionOrientation);

				bottomVertex = new Vector2((float) (nextNodePos.x - widthDeltaX - linkPointDeltaX - position.x), (float) (nextNodePos.y - widthDeltaY - linkPointDeltaY - position.z));
				topVertex = new Vector2((float) ((nextNodePos.x + widthDeltaX + linkPointDeltaX) - position.x), (float) ((nextNodePos.y + widthDeltaY + linkPointDeltaY) - position.z));

				vertices.Add(new Vector3(bottomVertex.x, 0, bottomVertex.y));
				vertices.Add(new Vector3(topVertex.x, 0, topVertex.y));

				// AFFICHAGE DES POINTS et directions
				//Color cubeColor = new Color(UnityEngine.Random.Range(0, 255) / 255F, UnityEngine.Random.Range(0, 255) / 255F, UnityEngine.Random.Range(0, 255) / 255F);

				//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube.transform.position = new Vector3(nextNodePos.x, 0, nextNodePos.y);
				//cube.transform.localScale = new Vector3(0.005F, 0.1F, (float) (linkPointDistance * 5F));
				//cube.transform.rotation = Quaternion.Euler(0, (float) (bissectrixOrientation * Mathf.Rad2Deg), 0);
				//MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
				//renderer.material.color = cubeColor;

				//GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube1.transform.position = new Vector3(bottomVertexPosX + roadPosition.x, 0, bottomVertexPosY + roadPosition.z);
				//cube1.transform.localScale = new Vector3(0.01F, 0.1F, 0.01F);
				//MeshRenderer renderer1 = cube1.GetComponent<MeshRenderer>();
				//renderer1.material.color = cubeColor;

				//GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube2.transform.position = new Vector3(topVertexPosX + roadPosition.x, 0, topVertexPosY + roadPosition.z);
				//cube2.transform.localScale = new Vector3(0.01F, 0.1F, 0.01F);
				//MeshRenderer renderer2 = cube2.GetComponent<MeshRenderer>();
				//renderer2.material.color = cubeColor;

				//GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube3.transform.position = new Vector3(nextNodePos.x, 0, nextNodePos.y);
				//cube3.transform.localScale = new Vector3(0.01F, 0.1F, 0.01F);

				//GameObject cube4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube4.transform.position = new Vector3((float) (nextNodePos.x - widthDeltaX), 0, (float) (nextNodePos.y - widthDeltaY));
				//cube4.transform.localScale = new Vector3(0.01F, 0.1F, 0.01F);

				//GameObject cube5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//cube5.transform.position = new Vector3((float) (nextNodePos.x + widthDeltaX), 0, (float) (nextNodePos.y + widthDeltaY));
				//cube5.transform.localScale = new Vector3(0.01F, 0.1F, 0.01F);
			} else if (i == nodeGroup.NodeCount() - 2) {
				bottomVertex = new Vector2((float) (nextNodePos.x - widthDeltaX - position.x), (float) (nextNodePos.y - widthDeltaY - position.z));
				topVertex = new Vector2((float) (nextNodePos.x + widthDeltaX - position.x), (float) (nextNodePos.y + widthDeltaY - position.z));

				vertices.Add(new Vector3(bottomVertex.x, 0, bottomVertex.y));
				vertices.Add(new Vector3(topVertex.x, 0, topVertex.y));
			}
		}

		return vertices;
	}

	private List<Vector2> WayUv(NodeGroup nodeGroup, List<Vector3> vertices, float width) {
		List<Vector2> uvVertices = new List<Vector2>();

		float sectionLength = 0;
		//float bottomLengthCursor = 0;
		//float topLengthCursor = 0;

		uvVertices.Add(new Vector2(0, 0));
		uvVertices.Add(new Vector2(0, 1));

		for (int i = 0; i < vertices.Count - 3; i += 2) {
			Vector2 currentNodePos = nodeGroup.GetNode(i / 2).ToVector();
			Vector2 nextNodePos = nodeGroup.GetNode((i + 2) / 2).ToVector();

			Vector3 currentBottomVertex = vertices[i];
			Vector3 currentTopVertex = vertices[i + 1];

			Vector3 nextBottomVertex = vertices[i + 2];
			Vector3 nextTopVertex = vertices[i + 3];

			sectionLength += Math.Abs(Vector2.Distance(currentNodePos, nextNodePos));
			//bottomLengthCursor = Math.Abs(Vector2.Distance(currentBottomVertex, nextBottomVertex));
			//topLengthCursor = Math.Abs(Vector2.Distance(currentTopVertex, nextTopVertex));

			//float bottomDelta = length - bottomLengthCursor;
			//float topDelta = length - topLengthCursor;

			float posU1 = sectionLength * 25;
			float posU2 = sectionLength * 25;

			uvVertices.Add(new Vector2(posU1, 0));
			uvVertices.Add(new Vector2(posU2, 1));

			//bottomLengthCursor -= bottomDelta;
			//topLengthCursor -= topDelta;
		}

		return uvVertices;
	}

	private List<int> WayTriangles(List<Vector3> vertices) {
		List<int> triangles = new List<int>();
		for (int i = 0; i < vertices.Count - 2; i += 2) {
			triangles.AddRange(new int[] { i, i + 2, i + 3 });
			triangles.AddRange(new int[] { i, i + 3, i + 1 });
		}
		triangles.Reverse();
		return triangles;
	}

	/// <summary>
	/// 	Permet d'obtenir une texture épurée sur la route. Sans ce traitement, le rendu de la texture serait vraiment
	/// 	trop sombre.
	/// </summary>
	/// <returns>Normales de la texture.</returns>
	private List<Vector3> WayNormals(List<Vector3> vertices) {
		List<Vector3> normals = new List<Vector3>();
		foreach (Vector3 vertex in vertices)
			normals.Add(Vector3.up);
		return normals;
	}
}
