using System.Collections;
using UnityEngine;

/*Classe qui gère la création des toits à partir des triangles créés avec la triangulation de Delaunay*/

public class RoofBuilder {
	//Fonction qui créé un toit. Appelle toutes les autres fonctions privées de la classe
	public GameObject BuildRoof(DelauneyTriangulation triangulation, int etage, float sizefloor) {
		float z = (float)triangulation.Triangles[0].NodeA.Latitude;
		float x = (float)triangulation.Triangles[0].NodeA.Longitude;

		//Création du GameObject (GO)
		GameObject roof = new GameObject("Roof", typeof(MeshFilter), typeof(MeshRenderer));

		//Ajout du tag pour l'affichage avec le boutton associé dans Unity
		roof.tag = NodeTags.ROOF_TAG;

		//Position du GO
		roof.transform.position = new Vector3(x, (etage * sizefloor), z);

		//Création d'un Mesh
		Mesh mesh = new Mesh();


		//Définition des points qui constitueront à plusieurs les sommets d'au moins 1 triangle
		mesh.vertices = MakeRoofVertices(triangulation,x,z);

		//On définit maintenant quels points forment chaque triangle
		mesh.triangles = MakeRoofTriangles(triangulation);

		//Reglage de l'étirement de l'image qui sert de texture sur le GO
		//mesh.uv = makeRoofUV();
		//Pour un rendu plus clair au niveau de la texture
		mesh.normals = MakeRoofNormals(triangulation);


		//On associe le mesh créé au GO
		MeshFilter mesh_filter = roof.GetComponent<MeshFilter>();
		mesh_filter.mesh = mesh;

		//On charge le matériel du toit et on l'associe au GO
		MeshRenderer mesh_renderer = roof.GetComponent<MeshRenderer>();
		mesh_renderer.material = Resources.Load ("Materials/toit") as Material;

		return roof;
	}

	//On définit dans cette fonction les points qui constitueront à plusieurs les sommets d'au moins 1 triangle (triangles définis dans makeBgTriangles()
	private Vector3[] MakeRoofVertices(DelauneyTriangulation triangulation, float x, float z) {
		float x2, z2;
		int N = triangulation.Triangles.Count * 3;
		Vector3[] vec = new Vector3[N];

		int i = 0;
		foreach(Triangle tri in triangulation.Triangles) {
			z2 = (float)tri.NodeA.Latitude - z;
			x2 = (float)tri.NodeA.Longitude - x;
			vec[i] = new Vector3(x2, 0, z2); //x,y,z=RIGHT,TOP,FORWARD;

			i++;

			z2 = (float)tri.NodeB.Latitude - z;
			x2 = (float)tri.NodeB.Longitude - x;
			vec[i] = new Vector3(x2, 0, z2); //x,y,z=RIGHT,TOP,FORWARD;

			i++;

			z2 = (float)tri.NodeC.Latitude - z;
			x2 = (float)tri.NodeC.Longitude - x;
			vec[i] = new Vector3(x2, 0, z2);
			i++;
		}
		return vec;
	}

	//Création de 2 triangles qui vont former à eux deux un morceau de toit
	private int[] MakeRoofTriangles(DelauneyTriangulation triangulation) {
		int N = triangulation.Triangles.Count * 3;
		int[] tri = new int[N];

		for (int i = 0; i < triangulation.Triangles.Count * 3; i++)
			tri[i] = i;
		
		return tri;
	}

	//Points pour le placement de la texture sur le GO
	private Vector2[] MakeRoofUV(float length, float width) {
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, 1),
			new Vector2 (0, 1)
		};
		return vec;
	}

	//Fonction qui permet d'avoir une texture "claire" sur le GO. Sinon le rendu de la texture est vraiment trop sombre.
	private Vector3[] MakeRoofNormals(DelauneyTriangulation triangulation) {
		int N = triangulation.Triangles.Count * 3;
		Vector3[] vec = new Vector3[N];

		for (int i = 0; i < triangulation.Triangles.Count * 3; i++)
			vec[i] = Vector3.up;

		return vec;
	}
}
