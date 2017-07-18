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
	public GameObject BuildRoof(float posX, float posZ, Triangulation triangulation, int nbFloor, float floorSize) {
		// Création et paramétrage de l'objet 3D destiné à former un toit
		GameObject roof = new GameObject("Roof", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.ROOF_TAG
		};
		roof.transform.position = new Vector3(posX, (nbFloor * floorSize), posZ);

		// Création, construction et texturing du maillage formant un toit
		Mesh mesh = new Mesh() {
			vertices = this.RoofVertices(triangulation, posX, posZ),
			triangles = this.RoofTriangles(triangulation),
			normals = this.RoofNormals(triangulation)
		};

		// Affectation du maillage au toit pour lui donner la forme voulue
		MeshFilter meshFilter = roof.GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la route pour lui donner la texture voulue
		MeshRenderer meshRenderer = roof.GetComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load (Materials.ROOF) as Material;

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
	private Vector3[] RoofVertices(Triangulation triangulation, float posX, float posZ) {
		int nbVertex = triangulation.Triangles.Count * 3;
		Vector3[] res = new Vector3[nbVertex];

		float jitter = 0.0F;
		int i = 0;
		foreach(Triangle triangle in triangulation.Triangles) {
			float posX2 = (float)(triangle.NodeA.Longitude - posX);
			float posZ2 = (float)(triangle.NodeA.Latitude - posZ);
			res[i] = new Vector3(posX2, Random.Range(-jitter / 2F, jitter / 2F), posZ2);        // [x, y, z] = [RIGHT, TOP, FORWARD];
			i++;

			posX2 = (float)(triangle.NodeB.Longitude - posX);
			posZ2 = (float)(triangle.NodeB.Latitude - posZ);
			res[i] = new Vector3(posX2, Random.Range(-jitter / 2F, jitter / 2F), posZ2);		// [x, y, z] = [RIGHT, TOP, FORWARD];
			i++;

			posX2 = (float)(triangle.NodeC.Longitude - posX);
			posZ2 = (float)(triangle.NodeC.Latitude - posZ);
			res[i] = new Vector3(posX2, Random.Range(-jitter / 2F, jitter / 2F), posZ2);
			i++;
		}
		return res;
	}


	/// <summary>
	/// 	Créé de 2 triangles qui vont former à eux deux une portion de toit.
	/// </summary>
	/// <returns>Triangles sur le toit.</returns>
	private int[] RoofTriangles(Triangulation triangulation) {
		int nbVertex = triangulation.Triangles.Count * 3;

		int[] res = new int[nbVertex];
		for (int i = 0; i < triangulation.Triangles.Count * 3; i++)
			res[i] = i;
		
		return res;
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
