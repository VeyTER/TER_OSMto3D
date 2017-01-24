using System.Collections;
using UnityEngine;

public class BackgroundCreation
{
	//Possibilité d'utiliser un prefab dans cette classe si on préfère faire des 
	//modifs sur les morceaux de routes via Unity plutôt que via le code
	public BackgroundCreation ()
	{
	}

	public void createBackground(float x, float z, float length, float width, float angle, float minlat, float minlon)
	{
		
		GameObject bg = new GameObject ("Background", typeof(MeshFilter), typeof(MeshRenderer));

		bg.transform.position = new Vector3 (minlon*1000f, -0.02f, minlat*1000f);
		bg.transform.rotation = Quaternion.Euler (0, angle, 0);

		Mesh mesh = new Mesh ();

		mesh.vertices = makeBgVertices (length, width);
		mesh.triangles = makeBgTriangles ();
		mesh.uv = makeBgUV (length, width);
		mesh.normals = makeBgNormals ();

		MeshFilter mesh_filter = bg.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = bg.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/bg") as Material;


	}

	private Vector3 [] makeBgVertices(float length, float width){
		Vector3[] vec = new Vector3[] {
			new Vector3 (0, 0, -width/2), //x,y,z=RIGHT,TOP,FORWARD
			new Vector3 (length, 0, -width/2),
			new Vector3 (length, 0, width/2),
			new Vector3 (0, 0, width/2)
//			new Vector3 (0, 0, -width/2+2.5f), //x,y,z=RIGHT,TOP,FORWARD
//			new Vector3 (length+5f, 0, -width/2+2.5f),
//			new Vector3 (length+5f, 0, width/2+2.5f),
//			new Vector3 (0, 0, width/2+2.5f)
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

	//Points pour le placement de la texture (roadMaterial) sur le GO
	private Vector2 [] makeBgUV(float length, float width){
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length*20, 0),
			new Vector2 (length*20, width*10),
			new Vector2 (0, width*10)
		};
		return(vec);
	}

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
