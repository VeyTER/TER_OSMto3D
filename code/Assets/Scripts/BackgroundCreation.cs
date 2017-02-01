﻿using System.Collections;
using UnityEngine;

/*Cette classe concerne uniquement la création du background de la carte (sol vert de la ville).*/

public class BackgroundCreation
{
	//Constructeur
	public BackgroundCreation ()
	{
	}

	//Fonction qui créé le background. Appelle toutes les autres fonctions privées de la classe
	public void createBackground(float x, float z, float length, float width, float angle, float minlat, float minlon)
	{
		//Création du GameObject (GO)
		GameObject bg = new GameObject ("Background", typeof(MeshFilter), typeof(MeshRenderer));

		//Position et rotation du GO
		bg.transform.position = new Vector3 (minlon*1000f, -0.02f, minlat*1000f);
		bg.transform.rotation = Quaternion.Euler (0, angle, 0);

		//Création d'un Mesh
		Mesh mesh = new Mesh ();

		//Définition des points qui constitueront à plusieurs les sommets d'au moins 1 triangle
		mesh.vertices = makeBgVertices (length, width); 
		//On définit maintenant quels points forment chaque triangle
		mesh.triangles = makeBgTriangles ();
		//Reglage de l'étirement de l'image qui sert de texture sur le GO
		mesh.uv = makeBgUV (length, width);
		//Pour un rendu plus clair au niveau de la texture
		mesh.normals = makeBgNormals ();

		//On associe le mesh créé au GO
		MeshFilter mesh_filter = bg.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		//On charge le matériel du background et on l'associe au GO
		MeshRenderer mesh_renderer = bg.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/bg") as Material;
	}

	//On définit dans cette fonction les points qui constitueront à plusieurs les sommets d'au moins 1 triangle (triangles définis dans makeBgTriangles()
	private Vector3 [] makeBgVertices(float length, float width){
		Vector3[] vec = new Vector3[] {
			new Vector3 (0, 0, -width/2), //x,y,z=RIGHT,TOP,FORWARD
			new Vector3 (length, 0, -width/2),
			new Vector3 (length, 0, width/2),
			new Vector3 (0, 0, width/2)
		};		
		return(vec);
	}

	//Création de 2 triangles qui vont former à eux deux un morceau de route
	private int[] makeBgTriangles(){
		int [] tri = {
			1,0,2, //triangle 1
			2,0,3  //triangle 2
		};
		return(tri);
	}

	//Points pour le placement de la texture sur le GO
	private Vector2 [] makeBgUV(float length, float width){
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length*20, 0),
			new Vector2 (length*20, width*10),
			new Vector2 (0, width*10)
		};
		return(vec);
	}

	//Fonction qui permet d'avoir une texture "claire" sur le GO. Sinon le rendu de la texture est vraiment trop sombre.
	private Vector3 [] makeBgNormals(){
		Vector3[] vec = new Vector3[] {
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		return(vec);
	}

}
