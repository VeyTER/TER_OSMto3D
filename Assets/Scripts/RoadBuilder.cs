using System.Collections;
using UnityEngine;

/* Classe qui gère la création des différents types de voie (terrestres et maritime) */

public class RoadBuilder {
	//Fonction qui créé un route classique. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildClassicHighway(float x, float z, float length, float width, float angle) {
		GameObject road = new GameObject ("Highway", typeof(MeshFilter), typeof(MeshRenderer));

		road.tag = NodeTags.HIGHWAY;
		road.transform.position = new Vector3 (x, 0.002f, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);

		Mesh mesh = new Mesh ();
		mesh.vertices = MakeHighwayVertices (length + 0.01f, width);
		mesh.triangles = MakeHighwayTriangles ();
		mesh.uv = MakeHighwayUV (length + 0.01f, width);
		mesh.normals = MakeHighwayNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/route") as Material;

		return road;
	}

	//Fonction qui créé une voie de bus. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildBusLane(float x, float z, float length, float width, float angle) {
		GameObject road = new GameObject ("BusLane", typeof(MeshFilter), typeof(MeshRenderer));

		road.tag = "BusLane";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = MakeHighwayVertices (length, width);
		mesh.triangles = MakeHighwayTriangles ();
		mesh.uv = MakeHighwayUV (length, width);
		mesh.normals = MakeHighwayNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/voieBus") as Material;

		return road;
	}

	//Fonction qui créé une voie cyclable. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildCycleway(float x, float z, float length, float width, float angle) {
		GameObject road = new GameObject ("Cycleway", typeof(MeshFilter), typeof(MeshRenderer));

		width = width / 2f;

		road.tag = NodeTags.CYCLEWAY;
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = MakeHighwayVertices (length, width);
		mesh.triangles = MakeHighwayTriangles ();
		mesh.uv = MakeHighwayUV (length, width);
		mesh.normals = MakeHighwayNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/voieCyclable") as Material;

		return road;
	}

	//Fonction qui créé un chemin piéton. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildFootway(float x, float z, float length, float width, float angle) {
		GameObject road = new GameObject ("Footway", typeof(MeshFilter), typeof(MeshRenderer));

		width = width /1.5f;

		road.tag = NodeTags.FOOTWAY;
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = MakeHighwayVertices (length, width);
		mesh.triangles = MakeHighwayTriangles ();
		mesh.uv = MakeHighwayUV (length, width);
		mesh.normals = MakeHighwayNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/cheminPieton") as Material;

		return road;
	}

	//Fonction qui créé une voie maritime. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildWaterway(float x, float z, float length, float width, float angle) {
		GameObject road = new GameObject ("Waterway", typeof(MeshFilter), typeof(MeshRenderer));

		width = width /1.5f;

		//highway.tag = "Waterway";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = MakeHighwayVertices (length, width);
		mesh.triangles = MakeHighwayTriangles ();
		mesh.uv = MakeHighwayUV (length, width);
		mesh.normals = MakeHighwayNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/waterway") as Material;

		return road;
	}

	//On définit dans cette fonction les points qui constitueront à plusieurs les sommets d'au moins 1 triangle (triangles définis dans makeBgTriangles()
	private Vector3 [] MakeHighwayVertices(float length, float width) {
		Vector3[] vec = new Vector3[] {
			new Vector3 (0, 0, -width/2), //x,y,z=RIGHT,TOP,FORWARD
			new Vector3 (length, 0, -width/2),
			new Vector3 (length, 0, width/2),
			new Vector3 (0, 0, width/2)
		};		
		return(vec);
	}

	//Création de 2 triangles qui vont former à eux deux un morceau de route
	private int[] MakeHighwayTriangles() {
		int [] tri = {
			1, 0, 2, //triangle 1
			2, 0, 3  //triangle 2
		};
		return(tri);
	}

	//Points pour le placement de la texture sur le GO
	private Vector2 [] MakeHighwayUV(float length, float width) {
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, 1),
			new Vector2 (0, 1)
		};
		return(vec);
	}

	//Fonction qui permet d'avoir une texture "claire" sur le GO. Sinon le rendu de la texture est vraiment trop sombre.
	private Vector3 [] MakeHighwayNormals() {
		Vector3[] vec = new Vector3[] {
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		return(vec);
	}
}
