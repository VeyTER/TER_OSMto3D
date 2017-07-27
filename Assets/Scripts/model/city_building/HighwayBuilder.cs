using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 	Gère la génération des routes.
/// </summary>
public class HighwayBuilder {
	/// <summary>
	/// 	Créé une route classique en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouvelle route classique.</returns>
	/// <param name="posX">Position en X de la route.</param>
	/// <param name="posZ">Position en Z de la route.</param>
	/// <param name="length">Longueur de la route.</param>
	/// <param name="width">Largeur de la route.</param>
	/// <param name="angle">Orientation de la route.</param>
	public GameObject BuildRoad(GameObject road, HighwayNodeGroup highwayNodeGroup) {
		if (highwayNodeGroup.NodeCount() >= 2) {
			road.transform.position = new Vector3((float) highwayNodeGroup.GetNode(0).Longitude, 0.01F, (float) highwayNodeGroup.GetNode(0).Latitude);

			string sectionsName = "RoadSections_" + highwayNodeGroup.GetNode(0).Reference + "-" + highwayNodeGroup.GetNode(highwayNodeGroup.NodeCount() - 1).Reference;
			GameObject sections = new GameObject(sectionsName, typeof(MeshFilter), typeof(MeshRenderer)) {
				tag = GoTags.ROAD_TAG
			};
			sections.transform.SetParent(road.transform, false);
			sections.transform.localPosition = Vector3.zero;

			List<Vector3> sectionsVertices = this.HighwayVertices(highwayNodeGroup, sections.transform.position, Dimensions.ROAD_WIDTH);
			List<int> sectionsTriangles = this.HighwayTriangles(sectionsVertices);
			List<Vector2> sectionsUv = this.HighwayUv(highwayNodeGroup, sectionsVertices, Dimensions.ROAD_WIDTH);
			List<Vector3> sectionsNormals = this.HighwayNormals(sectionsVertices);

			Debug.Log(sectionsVertices.Count + "  " + sectionsUv.Count);

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
			meshRenderer.material = Resources.Load(Materials.ROAD) as Material;

			return sections;
		} else {
			return null;
		}
	}


	///// <summary>
	///// 	Créé une voie de bus en appelant toutes les autres fonctions privées de la classe.
	///// </summary>
	///// <returns>Nouvelle voie de bus.</returns>
	///// <param name="posX">Position en X de la voie.</param>
	///// <param name="posZ">Position en Z de la voie.</param>
	///// <param name="length">Longueur de la voie.</param>
	///// <param name="width">Largeur de la voie.</param>
	///// <param name="angle">Orientation de la voie.</param>
	//public GameObject BuildBusLane(float posX, float posZ, float length, float width, float angle) {
	//	// Création et paramétrage de l'objet 3D destiné à former une voie de bus
	//	GameObject highway = new GameObject("BusLane", typeof(MeshFilter), typeof(MeshRenderer)) {
	//		tag = GoTags.BUS_LANE_TAG
	//	};

	//	highway.transform.position = new Vector3 (posX, 0.002F, posZ);
	//	highway.transform.rotation = Quaternion.Euler (0, angle, 0);

	//	Mesh mesh = new Mesh() {
	//		// Création, construction et texturing du maillage formant la voie
	//		vertices = this.HighwayVertices(length, width),
	//		triangles = this.HighwayTriangles(),
	//		uv = this.HighwayUV(length, width),
	//		normals = this.HighwayNormals()
	//	};

	//	// Affectation du maillage à la voie pour lui donner la forme voulue
	//	MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
	//	meshFilter.mesh = mesh;

	//	// Affectation du matériau à la voie pour lui donner la texture voulue
	//	MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
	//	meshRenderer.material = Resources.Load (Materials.BUSWAY) as Material;

	//	return highway;
	//}


	///// <summary>
	///// 	Créé une piste cyclable en appelant toutes les autres fonctions privées de la classe.
	///// </summary>
	///// <returns>Nouvelle piste cyclable.</returns>
	///// <param name="posX">Position en X de la piste cyclable.</param>
	///// <param name="posZ">Position en Z de la piste cyclable.</param>
	///// <param name="length">Longueur de la piste cyclable.</param>
	///// <param name="width">Largeur de la piste cyclable.</param>
	///// <param name="angle">Orientation de la piste cyclable.</param>
	//public GameObject BuildCycleway(float posX, float posZ, float length, float width, float angle) {
	//	// Création et paramétrage de l'objet 3D destiné à former une piste cyclable
	//	GameObject highway = new GameObject("Cycleway", typeof(MeshFilter), typeof(MeshRenderer)) {
	//		tag = GoTags.CYCLEWAY_TAG
	//	};

	//	highway.transform.position = new Vector3 (posX, 0.002F, posZ);
	//	highway.transform.rotation = Quaternion.Euler (0, angle, 0);

	//	Mesh mesh = new Mesh() {
	//		// Création, construction et texturing du maillage formant la piste
	//		vertices = this.HighwayVertices(length, width),
	//		triangles = this.HighwayTriangles(),
	//		uv = this.HighwayUV(length, width),
	//		normals = this.HighwayNormals()
	//	};

	//	// Affectation du maillage à la piste pour lui donner la forme voulue
	//	MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
	//	meshFilter.mesh = mesh;

	//	// Affectation du matériau à la piste pour lui donner la texture voulue
	//	MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
	//	meshRenderer.material = Resources.Load (Materials.CYCLEWAY) as Material;

	//	return highway;
	//}


	///// <summary>
	///// 	Créé un chemin piéton en appelant toutes les autres fonctions privées de la classe.
	///// </summary>
	///// <returns>Nouveau chemin piéton.</returns>
	///// <param name="posX">Position en X du chemin piéton.</param>
	///// <param name="posZ">Position en Z du chemin piéton.</param>
	///// <param name="length">Longueur du chemin piéton.</param>
	///// <param name="width">Largeur du chemin piéton.</param>
	///// <param name="angle">Orientation du chemin piéton.</param>
	//public GameObject BuildFootway(float posX, float posZ, float length, float width, float angle) {
	//	// Création et paramétrage de l'objet 3D destiné à former un chemin piéton
	//	GameObject highway = new GameObject("Footway", typeof(MeshFilter), typeof(MeshRenderer)) {
	//		tag = GoTags.FOOTWAY_TAG
	//	};

	//	highway.transform.position = new Vector3 (posX, 0.002F, posZ);
	//	highway.transform.rotation = Quaternion.Euler (0, angle, 0);

	//	Mesh mesh = new Mesh() {
	//		// Création, construction et texturing du maillage formant le chemin
	//		vertices = this.HighwayVertices(length, width),
	//		triangles = this.HighwayTriangles(),
	//		uv = this.HighwayUV(length, width),
	//		normals = this.HighwayNormals()
	//	};

	//	// Affectation du maillage au chemin pour lui donner la forme voulue
	//	MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
	//	meshFilter.mesh = mesh;

	//	// Affectation du matériau au chemin pour lui donner la texture voulue
	//	MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
	//	meshRenderer.material = Resources.Load (Materials.FOOTWAY) as Material;

	//	return highway;
	//}


	///// <summary>
	///// 	Créé une voie maritime toutes les autres fonctions privées de la classe.
	///// </summary>
	///// <returns>Nouvelle voie maritime.</returns>
	///// <param name="posX">Position en X de la voie maritime.</param>
	///// <param name="posZ">Position en Z de la voie maritime.</param>
	///// <param name="length">Longueur de la voie maritime.</param>
	///// <param name="width">Largeur de la voie maritime.</param>
	///// <param name="angle">Orientation de la voie maritime.</param>
	//public GameObject BuildWaterway(float posX, float posZ, float length, float width, float angle) {
	//	// Création et paramétrage de l'objet 3D destiné à former une voie maritime
	//	GameObject highway = new GameObject ("Waterway", typeof(MeshFilter), typeof(MeshRenderer));
	//	//highway.tag = "Waterway";

	//	highway.transform.position = new Vector3 (posX, 0.002F, posZ);
	//	highway.transform.rotation = Quaternion.Euler(0, angle, 0);

	//	Mesh mesh = new Mesh() {
	//		// Création, construction et texturing du maillage formant la voie
	//		vertices = this.HighwayVertices(length, width),
	//		triangles = this.HighwayTriangles(),
	//		uv = this.HighwayUV(length, width),
	//		normals = this.HighwayNormals()
	//	};

	//	// Affectation du maillage à la voie pour lui donner la forme voulue
	//	MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
	//	meshFilter.mesh = mesh;

	//	// Affectation du matériau à la voie pour lui donner la texture voulue
	//	MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
	//	meshRenderer.material = Resources.Load (Materials.WATERWAY) as Material;

	//	return highway;
	//}


	private List<Vector3> HighwayVertices(HighwayNodeGroup highwayNodeGroup, Vector3 roadPosition, float highwayWidth) {
		List<Vector3> vertices = new List<Vector3>();
		for (int i = 0; i < highwayNodeGroup.NodeCount() - 1; i++) {
			Vector2 currentNodePos = highwayNodeGroup.GetNode(i).ToVector();
			Vector2 nextNodePos = highwayNodeGroup.GetNode(i + 1).ToVector();

			double currentSectionLengthDeltaX = nextNodePos.x - currentNodePos.x;
			double currentSectionLengthDeltaY = nextNodePos.y - currentNodePos.y;
			double currentSectionOrientation = Math.Atan2(currentSectionLengthDeltaY, currentSectionLengthDeltaX);

			double widthDeltaX = (highwayWidth / 2) * Math.Cos(currentSectionOrientation + Math.PI / 2F);
			double widthDeltaY = (highwayWidth / 2) * Math.Sin(currentSectionOrientation + Math.PI / 2F);

			Vector2 bottomVertex = Vector2.zero;
			Vector2 topVertex = Vector2.zero;

			if (i == 0) {
				bottomVertex = new Vector2((float) (currentNodePos.x - widthDeltaX - roadPosition.x), (float) (currentNodePos.y - widthDeltaY - roadPosition.z));
				topVertex = new Vector2((float) (currentNodePos.x + widthDeltaX - roadPosition.x), (float) (currentNodePos.y + widthDeltaY - roadPosition.z));

				vertices.Add(new Vector3(bottomVertex.x, 0, bottomVertex.y));
				vertices.Add(new Vector3(topVertex.x, 0, topVertex.y));
			}

			if (i < highwayNodeGroup.NodeCount() - 2) {
				Node nextNextNode = highwayNodeGroup.GetNode(i + 2);
				Vector2 nextNextNodePos = nextNextNode.ToVector();

				double nextSectionLengthDeltaX = nextNextNodePos.x - nextNodePos.x;
				double nextSectionLengthDeltaY = nextNextNodePos.y - nextNodePos.y;
				double nextSectionOrientation = Math.Atan2(nextSectionLengthDeltaX, nextSectionLengthDeltaY);
				double invertedCurrentSectionOrientation = Math.Atan2(currentNodePos.x - nextNodePos.x, currentNodePos.y - nextNodePos.y);

				double bissectrixOrientation = invertedCurrentSectionOrientation + (nextSectionOrientation - invertedCurrentSectionOrientation) / 2F;
				double linkPointDistance = Math.Tan(-(bissectrixOrientation - (nextSectionOrientation - Math.PI / 2F))) * (highwayWidth / 2F);

				double linkPointDeltaX = linkPointDistance * Math.Cos(currentSectionOrientation);
				double linkPointDeltaY = linkPointDistance * Math.Sin(currentSectionOrientation);

				bottomVertex = new Vector2((float) (nextNodePos.x - widthDeltaX - linkPointDeltaX - roadPosition.x), (float) (nextNodePos.y - widthDeltaY - linkPointDeltaY - roadPosition.z));
				topVertex = new Vector2((float) ((nextNodePos.x + widthDeltaX + linkPointDeltaX) - roadPosition.x), (float) ((nextNodePos.y + widthDeltaY + linkPointDeltaY) - roadPosition.z));

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
			} else if (i == highwayNodeGroup.NodeCount() - 2) {
				bottomVertex = new Vector2((float) (nextNodePos.x - widthDeltaX - roadPosition.x), (float) (nextNodePos.y - widthDeltaY - roadPosition.z));
				topVertex = new Vector2((float) (nextNodePos.x + widthDeltaX - roadPosition.x), (float) (nextNodePos.y + widthDeltaY - roadPosition.z));

				vertices.Add(new Vector3(bottomVertex.x, 0, bottomVertex.y));
				vertices.Add(new Vector3(topVertex.x, 0, topVertex.y));
			}
		}

		return vertices;
	}

	private List<Vector2> HighwayUv(HighwayNodeGroup highwayNodeGroup, List<Vector3> vertices, float highwayWidth) {
		List<Vector2> uvVertices = new List<Vector2>();

		float length = 0;
		float bottomLengthCursor = 0;
		float topLengthCursor = 0;

		uvVertices.Add(new Vector2(0, 0));
		uvVertices.Add(new Vector2(0, 1));

		for (int i = 0; i < vertices.Count - 3; i += 2) {
			Vector2 currentNodePos = highwayNodeGroup.GetNode(i / 2).ToVector();
			Vector2 nextNodePos = highwayNodeGroup.GetNode((i + 2) / 2).ToVector();

			Vector3 currentBottomVertex = vertices[i];
			Vector3 currentTopVertex = vertices[i + 1];

			Vector3 nextBottomVertex = vertices[i + 2];
			Vector3 nextTopVertex = vertices[i + 3];

			length += Math.Abs(Vector2.Distance(currentNodePos, nextNodePos));
			//bottomLengthCursor = Math.Abs(Vector2.Distance(currentBottomVertex, nextBottomVertex));
			//topLengthCursor = Math.Abs(Vector2.Distance(currentTopVertex, nextTopVertex));

			//float bottomDelta = length - bottomLengthCursor;
			//float topDelta = length - topLengthCursor;

			float posU1 = length * 25;
			float posU2 = length * 25;

			uvVertices.Add(new Vector2(posU1, 0));
			uvVertices.Add(new Vector2(posU2, 1));

			//bottomLengthCursor -= bottomDelta;
			//topLengthCursor -= topDelta;
		}

		return uvVertices;
	}

	private List<int> HighwayTriangles(List<Vector3> vertices) {
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
	private List<Vector3> HighwayNormals(List<Vector3> vertices) {
		List<Vector3> normals = new List<Vector3>();
		foreach (Vector3 vertex in vertices)
			normals.Add(Vector3.up);
		return normals;
	}
}
