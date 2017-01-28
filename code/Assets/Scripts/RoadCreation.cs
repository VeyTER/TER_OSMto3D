﻿using System.Collections;
using UnityEngine;

public class RoadCreation
{
    //Possibilité d'utiliser un prefab dans cette classe si on préfère faire des 
    //modifs sur les morceaux de routes via Unity plutôt que via le code
	public RoadCreation ()
	{
	}

	public void createClassicRoad(float x, float z, float length, float width, float angle)
	{
			GameObject road = new GameObject ("Highway", typeof(MeshFilter), typeof(MeshRenderer));
			road.tag = "Highway";
			road.transform.position = new Vector3 (x, 0.002f, z);
			road.transform.rotation = Quaternion.Euler (0, angle, 0);
			Mesh mesh = new Mesh ();
	
			mesh.vertices = makeRoadVertices (length+0.01f, width);
			mesh.triangles = makeRoadTriangles ();
			mesh.uv = makeRoadUV (length+0.01f, width);
			mesh.normals = makeRoadNormals ();

			MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
			mesh_filter.mesh = mesh;

			MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
//			mesh_renderer.shadowCastingMode = false;
			mesh_renderer.material = Resources.Load ("Materials/route") as Material;
	}

	public void createBusLane(float x, float z, float length, float width, float angle)
	{
		GameObject road = new GameObject ("BusLane", typeof(MeshFilter), typeof(MeshRenderer));
		road.tag = "BusLane";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = makeRoadVertices (length, width);
		mesh.triangles = makeRoadTriangles ();
		mesh.uv = makeRoadUV (length, width);
		mesh.normals = makeRoadNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/voieBus") as Material;
	}

	public void createCycleway(float x, float z, float length, float width, float angle)
	{
		width = width /2f;
		GameObject road = new GameObject ("Cycleway", typeof(MeshFilter), typeof(MeshRenderer));
		road.tag = "Cycleway";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = makeRoadVertices (length, width);
		mesh.triangles = makeRoadTriangles ();
		mesh.uv = makeRoadUV (length, width);
		mesh.normals = makeRoadNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/voieCyclable") as Material;
	}

	public void createFootway(float x, float z, float length, float width, float angle)
	{
		width = width /1.5f;
		GameObject road = new GameObject ("Footway", typeof(MeshFilter), typeof(MeshRenderer));
		road.tag = "Footway";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = makeRoadVertices (length, width);
		mesh.triangles = makeRoadTriangles ();
		mesh.uv = makeRoadUV (length, width);
		mesh.normals = makeRoadNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/cheminPieton") as Material;
	}

	public void createWaterway(float x, float z, float length, float width, float angle)
	{
		width = width /1.5f;
		GameObject road = new GameObject ("Waterway", typeof(MeshFilter), typeof(MeshRenderer));
		//road.tag = "Waterway";
		road.transform.position = new Vector3 (x, 0, z);
		road.transform.rotation = Quaternion.Euler (0, angle, 0);
		Mesh mesh = new Mesh ();

		mesh.vertices = makeRoadVertices (length, width);
		mesh.triangles = makeRoadTriangles ();
		mesh.uv = makeRoadUV (length, width);
		mesh.normals = makeRoadNormals ();

		MeshFilter mesh_filter = road.GetComponent<MeshFilter> ();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = road.GetComponent<MeshRenderer> ();
		mesh_renderer.material = Resources.Load ("Materials/waterway") as Material;
	}

	private Vector3 [] makeRoadVertices(float length, float width){
		Vector3[] vec = new Vector3[] {
			new Vector3 (0, 0, -width/2), //x,y,z=RIGHT,TOP,FORWARD
			new Vector3 (length, 0, -width/2),
			new Vector3 (length, 0, width/2),
			new Vector3 (0, 0, width/2)
		};		
		return(vec);
	}

	//Création de 2 triangles qui vont former à eux deux un morceau de route
	private int[] makeRoadTriangles(){
		int [] tri = {
			1,0,2, //triangle 1
			2,0,3  //triangle 2
		};
		return(tri);
	}

	//Points pour le placement de la texture (roadMaterial) sur le GO
	private Vector2 [] makeRoadUV(float length, float width){
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length*20, 0),
			new Vector2 (length*20, 1),
			new Vector2 (0, 1)
		};
		return(vec);
	}

	private Vector3 [] makeRoadNormals(){
		Vector3[] vec = new Vector3[] {
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		return(vec);
	}
		
}
