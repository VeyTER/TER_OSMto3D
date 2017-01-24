using System.Collections;
using UnityEngine;

public class RoofCreation
{
    public RoofCreation()
    {
    }

	public void createRoof(TRGDelaunay TRG,int etg,float sizefloor)
	{
		int etage = etg;
		float x = (float)TRG.listTriangle[0].getNoeudA().getLongitude();
		float z = (float)TRG.listTriangle[0].getNoeudA().getLatitude();

		GameObject roof = new GameObject("Roof", typeof(MeshFilter), typeof(MeshRenderer));
		roof.tag = "Roof";
		roof.transform.position = new Vector3(x, (etage * sizefloor), z);

		Mesh mesh = new Mesh();

		mesh.vertices = makeRoofVertices(TRG,x,z);
		mesh.triangles = makeRoofTriangles(TRG);
		//mesh.uv = makeRoofUV();
		mesh.normals = makeRoofNormals(TRG);

		MeshFilter mesh_filter = roof.GetComponent<MeshFilter>();
		mesh_filter.mesh = mesh;

		MeshRenderer mesh_renderer = roof.GetComponent<MeshRenderer>();
		mesh_renderer.material = Resources.Load ("Materials/toit") as Material;
	
	}

	private Vector3[] makeRoofVertices(TRGDelaunay TRG, float x, float z)
	{
		float x2, z2;
		int N = TRG.listTriangle.Count * 3;
		Vector3[] vec = new Vector3[N];

		int i = 0;
		foreach(Triangle tri in TRG.listTriangle)
		{
			x2 = (float)tri.getNoeudA().getLongitude() - x;
			z2 = (float)tri.getNoeudA().getLatitude() - z;
			vec[i] = new Vector3(x2, 0, z2); //x,y,z=RIGHT,TOP,FORWARD;
			i++;
			x2 = (float)tri.getNoeudB().getLongitude() - x;
			z2 = (float)tri.getNoeudB().getLatitude() - z;
			vec[i] = new Vector3(x2, 0, z2); //x,y,z=RIGHT,TOP,FORWARD;
			i++;
			x2 = (float)tri.getNoeudC().getLongitude() - x;
			z2 = (float)tri.getNoeudC().getLatitude() - z;
			vec[i] = new Vector3(x2, 0, z2);
			i++;
		}
		return (vec);
	}

	//Création de triangles
	private int[] makeRoofTriangles(TRGDelaunay TRG)
	{
		int N = TRG.listTriangle.Count*3;
		int[] tri = new int[N];

		for (int i = 0; i < TRG.listTriangle.Count*3; i++)
		{
			tri[i] = i;
		}
		return (tri);
	}

	//Points pour le placement de la texture (roofMaterial) sur le GO
	private Vector2[] makeRoofUV(float length, float width)
	{
		Vector2[] vec = new Vector2[] {
			new Vector2 (0, 0), //(x,y)
			new Vector2 (length*20, 0),
			new Vector2 (length*20, 1),
			new Vector2 (0, 1)
		};
		return (vec);
	}

	private Vector3[] makeRoofNormals(TRGDelaunay TRG)
	{
		int N = TRG.listTriangle.Count * 3;
		Vector3[] vec = new Vector3[N];
		for (int i = 0; i < TRG.listTriangle.Count * 3; i++)
		{
			vec[i] = Vector3.up;
		}
		return (vec);
	}

}
