using System.Collections;
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
	public GameObject BuildClassicHighway(float posX, float posZ, float length, float width, float angle) {
		// Création et paramétrage de l'objet 3D destiné à former une route classique
		GameObject highway = new GameObject("Highway", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.HIGHWAY_TAG
		};
		highway.transform.position = new Vector3 (posX, 0.002F, posZ);
		highway.transform.rotation = Quaternion.Euler (0, angle, 0);

		// Création, construction et texturing du maillage formant la route
		Mesh mesh = new Mesh() {
			vertices = this.HighwayVertices(length + 0.01F, width),
			triangles = this.HighwayTriangles(),
			uv = this.HighwayUV(length + 0.01F, width),
			normals = this.HighwayNormals()
		};

		// Affectation du maillage à la route pour lui donner la forme voulue
		MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la route pour lui donner la texture voulue
		MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.ROAD) as Material;

		return highway;
	}


	/// <summary>
	/// 	Créé une voie de bus en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouvelle voie de bus.</returns>
	/// <param name="posX">Position en X de la voie.</param>
	/// <param name="posZ">Position en Z de la voie.</param>
	/// <param name="length">Longueur de la voie.</param>
	/// <param name="width">Largeur de la voie.</param>
	/// <param name="angle">Orientation de la voie.</param>
	public GameObject BuildBusLane(float posX, float posZ, float length, float width, float angle) {
		// Création et paramétrage de l'objet 3D destiné à former une voie de bus
		GameObject highway = new GameObject("BusLane", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.BUS_LANE_TAG
		};

		highway.transform.position = new Vector3 (posX, 0, posZ);
		highway.transform.rotation = Quaternion.Euler (0, angle, 0);

		Mesh mesh = new Mesh() {
			// Création, construction et texturing du maillage formant la voie
			vertices = this.HighwayVertices(length, width),
			triangles = this.HighwayTriangles(),
			uv = this.HighwayUV(length, width),
			normals = this.HighwayNormals()
		};

		// Affectation du maillage à la voie pour lui donner la forme voulue
		MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la voie pour lui donner la texture voulue
		MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.BUSWAY) as Material;

		return highway;
	}


	/// <summary>
	/// 	Créé une piste cyclable en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouvelle piste cyclable.</returns>
	/// <param name="posX">Position en X de la piste cyclable.</param>
	/// <param name="posZ">Position en Z de la piste cyclable.</param>
	/// <param name="length">Longueur de la piste cyclable.</param>
	/// <param name="width">Largeur de la piste cyclable.</param>
	/// <param name="angle">Orientation de la piste cyclable.</param>
	public GameObject BuildCycleway(float posX, float posZ, float length, float width, float angle) {
		// Création et paramétrage de l'objet 3D destiné à former une piste cyclable
		GameObject highway = new GameObject("Cycleway", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.CYCLEWAY_TAG
		};

		highway.transform.position = new Vector3 (posX, 0, posZ);
		highway.transform.rotation = Quaternion.Euler (0, angle, 0);

		Mesh mesh = new Mesh() {
			// Création, construction et texturing du maillage formant la piste
			vertices = this.HighwayVertices(length, width),
			triangles = this.HighwayTriangles(),
			uv = this.HighwayUV(length, width),
			normals = this.HighwayNormals()
		};

		// Affectation du maillage à la piste pour lui donner la forme voulue
		MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la piste pour lui donner la texture voulue
		MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.CYCLEWAY) as Material;

		return highway;
	}


	/// <summary>
	/// 	Créé un chemin piéton en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouveau chemin piéton.</returns>
	/// <param name="posX">Position en X du chemin piéton.</param>
	/// <param name="posZ">Position en Z du chemin piéton.</param>
	/// <param name="length">Longueur du chemin piéton.</param>
	/// <param name="width">Largeur du chemin piéton.</param>
	/// <param name="angle">Orientation du chemin piéton.</param>
	public GameObject BuildFootway(float posX, float posZ, float length, float width, float angle) {
		// Création et paramétrage de l'objet 3D destiné à former un chemin piéton
		GameObject highway = new GameObject("Footway", typeof(MeshFilter), typeof(MeshRenderer)) {
			tag = GoTags.FOOTWAY_TAG
		};

		highway.transform.position = new Vector3 (posX, 0, posZ);
		highway.transform.rotation = Quaternion.Euler (0, angle, 0);

		Mesh mesh = new Mesh() {
			// Création, construction et texturing du maillage formant le chemin
			vertices = this.HighwayVertices(length, width),
			triangles = this.HighwayTriangles(),
			uv = this.HighwayUV(length, width),
			normals = this.HighwayNormals()
		};

		// Affectation du maillage au chemin pour lui donner la forme voulue
		MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

		// Affectation du matériau au chemin pour lui donner la texture voulue
		MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.FOOTWAY) as Material;

		return highway;
	}


	/// <summary>
	/// 	Créé une voie maritime toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <returns>Nouvelle voie maritime.</returns>
	/// <param name="posX">Position en X de la voie maritime.</param>
	/// <param name="posZ">Position en Z de la voie maritime.</param>
	/// <param name="length">Longueur de la voie maritime.</param>
	/// <param name="width">Largeur de la voie maritime.</param>
	/// <param name="angle">Orientation de la voie maritime.</param>
	public GameObject BuildWaterway(float posX, float posZ, float length, float width, float angle) {
		// Création et paramétrage de l'objet 3D destiné à former une voie maritime
		GameObject highway = new GameObject ("Waterway", typeof(MeshFilter), typeof(MeshRenderer));
		//highway.tag = "Waterway";

		highway.transform.position = new Vector3 (posX, 0, posZ);
		highway.transform.rotation = Quaternion.Euler(0, angle, 0);

		Mesh mesh = new Mesh() {
			// Création, construction et texturing du maillage formant la voie
			vertices = this.HighwayVertices(length, width),
			triangles = this.HighwayTriangles(),
			uv = this.HighwayUV(length, width),
			normals = this.HighwayNormals()
		};

		// Affectation du maillage à la voie pour lui donner la forme voulue
		MeshFilter meshFilter = highway.GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

		// Affectation du matériau à la voie pour lui donner la texture voulue
		MeshRenderer meshRenderer = highway.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.WATERWAY) as Material;

		return highway;
	}


	/// <summary>
	/// 	Définit les points qui constituront, à plusieurs, les sommets d'au moins un triangle.
	/// </summary>
	/// <returns>Points formant la route.</returns>
	/// <param name="length">Longueur de la route</param>
	/// <param name="width">Largeur de la route</param>
	private Vector3 [] HighwayVertices(float length, float width) {
		Vector3[] res = new Vector3[] {
			new Vector3 (0, 0, -width  / 2F),	 // [x, y, z] = [RIGHT,TOP,FORWARD]
			new Vector3 (length, 0, -width / 2F),
			new Vector3 (length, 0, width / 2F),
			new Vector3 (0, 0, width / 2F)
		};		
		return res;
	}


	/// <summary>
	/// 	Définit les points qui constituront, à plusieurs, les sommets d'au moins un triangle.
	/// </summary>
	/// <returns>Triangles formant un tronçon de route.</returns>
	private int[] HighwayTriangles() {
		int [] res = {
			1, 0, 2, // triangle 1
			2, 0, 3  // triangle 2
		};
		return res;
	}


	/// <summary>
	/// 	Créé les points utilisés pour le placement de la texture sur la route.
	/// </summary>
	/// <returns>Coordonnées pour le mapping.</returns>
	/// <param name="length">Longueur de la route</param>
	/// <param name="width">Largeur de la route</param>
	private Vector2 [] HighwayUV(float length, float width) {
		Vector2[] res = new Vector2[] {
			new Vector2 (0, 0),		// (x, y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, 1),
			new Vector2 (0, 1)
		};
		return res;
	}


	/// <summary>
	/// 	Permet d'obtenir une texture épurée sur la route. Sans ce traitement, le rendu de la texture serait vraiment
	/// 	trop sombre.
	/// </summary>
	/// <returns>Normales de la texture.</returns>
	private Vector3 [] HighwayNormals() {
		Vector3[] res = new Vector3[] {
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		return res;
	}
}
