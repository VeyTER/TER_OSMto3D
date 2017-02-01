using System.Collections;
using UnityEngine;

/*Classe qui gère la création des toits à partir des triangles créés avec la triangulation de Delaunay*/

public class RoofCreation
{
	//Constructeur
    public RoofCreation()
    {
    }

	//Fonction qui créé un toit. Appelle toutes les autres fonctions privées de la classe
	public void createRoof(TRGDelaunay TRG,int etage,float sizefloor)
	{
		
		float x = (float)TRG.listTriangle[0].getNoeudA().getLongitude();
		float z = (float)TRG.listTriangle[0].getNoeudA().getLatitude();

		//Création du GameObject (GO)
		GameObject roof = new GameObject("Roof", typeof(MeshFilter), typeof(MeshRenderer));
		//Ajout du tag pour l'affichage avec le boutton associé dans Unity
		roof.tag = "Roof";
		//Position du GO
		roof.transform.position = new Vector3(x, (etage * sizefloor), z);

		//Création d'un Mesh
		Mesh mesh = new Mesh();

		//Définition des points qui constitueront à plusieurs les sommets d'au moins 1 triangle
		mesh.vertices = makeRoofVertices(TRG,x,z);
		//On définit maintenant quels points forment chaque triangle
		mesh.triangles = makeRoofTriangles(TRG);
		//Reglage de l'étirement de l'image qui sert de texture sur le GO
		//mesh.uv = makeRoofUV();
		//Pour un rendu plus clair au niveau de la texture
		mesh.normals = makeRoofNormals(TRG);

		//On associe le mesh créé au GO
		MeshFilter mesh_filter = roof.GetComponent<MeshFilter>();
		mesh_filter.mesh = mesh;

		//On charge le matériel du toit et on l'associe au GO
		MeshRenderer mesh_renderer = roof.GetComponent<MeshRenderer>();
		mesh_renderer.material = Resources.Load ("Materials/toit") as Material;
	
	}

	//On définit dans cette fonction les points qui constitueront à plusieurs les sommets d'au moins 1 triangle (triangles définis dans makeBgTriangles()
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

	//Création de 2 triangles qui vont former à eux deux un morceau de toit
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

	//Points pour le placement de la texture sur le GO
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

	//Fonction qui permet d'avoir une texture "claire" sur le GO. Sinon le rendu de la texture est vraiment trop sombre.
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
