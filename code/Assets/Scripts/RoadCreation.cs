using System.Collections;
using UnityEngine;

public class RoadCreation
{
	//Prefab route, (prefab est un type d'asset, reutilisable à partir du Project View)
	//public GameObject prefabRoad;
	public Material roadMaterial;

	public RoadCreation (Material roadMat)
	{
		roadMaterial = roadMat;
	}
		
	public void createRoad(float x, float y, float length, float width)
	{
		GameObject road = new GameObject("Highway",typeof(MeshFilter),typeof(MeshRenderer));
		road.tag = "Highway";
		road.transform.position = new Vector3 (x,0,y);

		Vector3[] vertices = {
			new Vector3 (0, 0, 0), //x,y,z=RIGHT,TOP,FORWARD
			new Vector3 (0.1f, 0, 0),
			new Vector3 (0.1f, 0, 0.1f),
			new Vector3 (0, 0, 0.1f) 
		};

		int[] triangles = {
			1,0,2, //triangle 1
			2,0,3  //triangle 2
		};

		Mesh mesh = new Mesh ();

		mesh.vertices = vertices;
		mesh.triangles = triangles;

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = roadMaterial;

		/*Vector2[] uv = {
			new Vector2(0,0),
			new Vector2(1,0),
			new Vector2(1,1),
			new Vector2(0,1)
		}*/
	}


		
}

