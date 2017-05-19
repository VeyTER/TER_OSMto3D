﻿using System.Collections;
using UnityEngine;

/// <summary>
/// 	Générateur de sol, représenté par un sol vert.
/// </summary>
public class GroundBuilder {
	
	/// <summary>
	/// 	Créé le sol en appelant toutes les autres fonctions privées de la classe.
	/// </summary>
	/// <param name="length">Longueur du sol.</param>
	/// <param name="width">Largeur du sol.</param>
	/// <param name="angle">Orientation du sol sur l'axe Y.</param>
	/// <param name="minlat">Latitude maximale du sol.</param>
	/// <param name="minlon">Longitude minimale du sol.</param>
	public void BuildGround(float length, float width, float angle, float minLat, float minLon) {
		// Création de l'objet 3D destiné à former le sol
		GameObject ground = new GameObject ("Ground", typeof(MeshFilter), typeof(MeshRenderer));

		// Positionnement et orientation du sol
		ground.transform.position = new Vector3 ((float)(minLon * Main.SCALE_FACTOR), -0.02F, (float)(minLat * Main.SCALE_FACTOR));
		ground.transform.rotation = Quaternion.Euler (0, angle, 0);

		// Création du maillage du sol
		Mesh groundMesh = new Mesh ();

		// Définition des points qui constitueront à plusieurs les sommets d'au moins 1 triangle
		groundMesh.vertices = GroundVertices (length, width); 

		// Définition des points formant chaque triangle
		groundMesh.triangles = GroundTriangles ();

		// Reglage de l'étirement de l'image qui sert de texture sur l'objet 3D
		groundMesh.uv = GroundUV (length, width);

		// Correction du rendu de la texture
		groundMesh.normals = GroundNormals ();

		// Affectation du maillage au sol pour lui donner la forme voulue
		MeshFilter meshFilter = ground.GetComponent<MeshFilter> ();
		meshFilter.mesh = groundMesh;

		// Affectation du matériau au sol pour lui donner la texture voulue
		MeshRenderer meshRenderer = ground.GetComponent<MeshRenderer> ();
		meshRenderer.material = Resources.Load (Materials.GROUND) as Material;
	}


	/// <summary>
	/// 	Définit les points qui constitueront, à plusieurs, les sommets d'au moins un triangle (triangles définis
	/// 	dans la méthode GroundTriangles()).
	/// </summary>
	/// <returns>Points formant le sol.</returns>
	/// <param name="length">Longueur du sol.</param>
	/// <param name="width">Largeur du sol.</param>
	private Vector3 [] GroundVertices(float length, float width) {
		Vector3[] res = new Vector3[] {
			new Vector3 (0, 0, -width/2F),		// [x, y, z] = [RIGHT, TOP, FORWARD]
			new Vector3 (length, 0, -width/2F),
			new Vector3 (length, 0, width/2F),
			new Vector3 (0, 0, width/2F)
		};		
		return res;
	}


	/// <summary>
	/// 	Créé de 2 triangles qui vont former à eux deux une portion du sol.
	/// </summary>
	/// <returns>Triangles sur le sol.</returns>
	private int[] GroundTriangles() {
		int [] res = {
			1, 0, 2, // Triangle 1
			2, 0, 3  // Triangle 2
		};
		return res;
	}


	/// <summary>
	/// 	Créé les points utilisés pour le placement de la texture sur le sol.
	/// </summary>
	/// <returns>Coordonnées pour le mapping.</returns>
	/// <param name="length">Longueur du sol.</param>
	/// <param name="width">Largeur du sol.</param>
	private Vector2 [] GroundUV(float length, float width) {
		Vector2[] res = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length * 20, 0),
			new Vector2 (length * 20, width * 10),
			new Vector2 (0, width * 10)
		};
		return res;
	}


	/// <summary>
	/// 	Permet d'obtenir une texture épurée sur le sol. Sans ce traitement, le rendu de la texture serait vraiment
	/// 	trop sombre.
	/// </summary>
	/// <returns>Normales de la texture.</returns>
	private Vector3 [] GroundNormals() {
		Vector3[] res = new Vector3[] {
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		return res;
	}
}