using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

public class DelauneyTriangulation {
	private List<Triangle> triangles = new List<Triangle>();
	private List<Triangle> trianglesAdd = new List<Triangle>();
	private List<Triangle> trianglesSuppr = new List<Triangle>();

	private List<Node> nodes = new List<Node>();
	private List<Node> nodesTemp = new List<Node>();
	private List<Node> nodesRm = new List<Node>();

	private double spacing = 0.0001d;

	// Constructeur
	public DelauneyTriangulation (NodeGroup build) {
		int temp;
		foreach (Node nd in build.Nodes)
			nodes.Add(nd);
		temp = nodes.Count;
		nodes.RemoveAt(temp - 1);
	}

	/// <summary>
	/// Methode start:
	/// Triangule les points du batiment pour construire les toits
	/// </summary>
	public void Start() {
		// On parcour la liste des noeud
		foreach(Node n in nodes) {

			// On parcour la liste des triangles
			foreach(Triangle t1 in triangles) {
				// Si le triangle ne respecte pas les conditions on l'ajout à la liste des triangles à supprimer de la liste des triangles
				if (this.IsWithin(t1, n)) {
					trianglesSuppr.Add(t1);
				}
			}

			// Ajout de tout les noeuds differents des triangles à supprimer dans la liste des noeuds temporaire
			foreach(Triangle t2 in trianglesSuppr) {
				if (!nodesTemp.Contains(t2.NodeA))
					nodesTemp.Add(t2.NodeA);

				if (!nodesTemp.Contains(t2.NodeB))
					nodesTemp.Add(t2.NodeB);

				if (!nodesTemp.Contains(t2.NodeC))
					nodesTemp.Add(t2.NodeC);
			}

			// Creation des triangles à partir de deux points dans la liste des noeuds temporaire et le noeud courant
			for(int i = 0; i<nodesTemp.Count;i++) {
				if (i < (nodesTemp.Count - 1))
					trianglesAdd.Add(new Triangle(nodesTemp[i], nodesTemp[i + 1], n));
				else
					trianglesAdd.Add(new Triangle(nodesTemp[i], nodesTemp[0], n));
			}

			// On retire de la liste des triangles les triangle à supprimer
			foreach (Triangle t3 in trianglesSuppr)
				triangles.Remove(t3);

			// On ajoute à la liste des triangles les triangles à ajouter
			foreach(Triangle t4 in trianglesAdd)
				triangles.Add(t4);

			// On efface les listes temporaires
			trianglesAdd.Clear();
			trianglesSuppr.Clear();
			nodesTemp.Clear();
		}
	}

	/// <summary>
	/// Methode isWithin :
	/// Determine si un point est a l'interieur du cercle circoncrit d'un triangle
	/// </summary>
	/// <param name="tri"> Triangle </param>
	/// <param name="n"> Point à traiter </param>
	/// <returns> True : Si le point est à l'iterieur </returns>
	public bool IsWithin(Triangle tri , Node n) {
		double A, B;
		double moduleA, moduleB;

		Node triangleCenter = tri.Center ();

		// Calcul rayon du cercle circonscri au triangle
		A = Math.Pow((tri.NodeA.Longitude - triangleCenter.Longitude), 2) + Math.Pow((tri.NodeA.Latitude - triangleCenter.Latitude), 2);
		moduleA = Math.Sqrt(A);

		// Calcul de la distance entre le centre du cercle circoncrit et le point en parametre
		B = Math.Pow((n.Longitude - triangleCenter.Longitude), 2) + Math.Pow((n.Latitude - triangleCenter.Latitude), 2);
		moduleB = Math.Sqrt(B);

		if (moduleA > moduleB)
			return true;
		else
			return false;
	}

	/// <summary>
	/// Methode creaBoiteEnglob:
	/// Creer un triangle dont le cercle circonscrit englobe tout le batiment
	/// CETTE FONCTION NE CREER PAS DE BOITE ENGLOBANTE
	/// ACTUELLEMENT ELLE NE SERT QU'A LA CREATION DE TRIANGLES POUR DEBUTER
	/// UNE TRIAGULATION DE DELAUNAY
	/// </summary>
	public void CreateBoundingBox() {
		/*double maxX, maxY, minX, minY;

        minY = listNode[0].Latitude;
        maxY = listNode[0].Latitude;
        minX = listNode[0].Longitude();
        maxX = listNode[0].Longitude();

        // Parcours de la liste de node pour la mise a jours des latitude/longitude min et max
        foreach(Node nd in listNode)
        {
            if (nd.Latitude > maxY)
            {
                maxY = nd.Latitude;
            }
            if (nd.Latitude < minY)
            {
                minY = nd.Latitude;
            }
            if (nd.Longitude() > maxX)
            {
                maxX = nd.Longitude();
            }
            if (nd.Longitude() < minX)
            {
                minX = nd.Longitude();
            }
        }

        // valeur par deffaut = 0.1f
        maxX += ecartement;
        maxY += ecartement;
        minX -= ecartement;
        minY -= ecartement;

        //Création des points composant la boite englobante
        Node temp1 = new Node(1, minX, minY);
        Node temp2 = new Node(2, minX, maxY);
        Node temp3 = new Node(3, maxX, maxY);

        // Ajout de ces points dans la listes a en
        listNodeRm.Add(temp1);
        listNodeRm.Add(temp2);
        listNodeRm.Add(temp3);

        // Ajout d'un triangle respectant les conditons de Delaunay à la liste des triangles
        listTriangle.Add(new Triangle(temp1, temp2, temp3));
        */
		// CREATION DES TRIANGLES
		for (int i = 0; i < nodes.Count; i++) {
			if (i < (nodes.Count - 2))
				trianglesAdd.Add(new Triangle(nodes[i], nodes[i + 1], nodes[i + 2]));
			else if (i < (nodes.Count - 1))
				trianglesAdd.Add(new Triangle(nodes[i], nodes[i + 1], nodes[0]));
			else
				trianglesAdd.Add(new Triangle(nodes[i], nodes[0], nodes[1]));
		}
		/*for (int i = 0; i < listNode.Count; i++)
		{
			if (i < (listNode.Count - 3))
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[i + 1], listNode[i + 3]));
			}
			else if (i < (listNode.Count - 2))
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[i + 1], listNode[0]));
			}
			else if (i < (listNode.Count - 1))
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[i + 1], listNode[1]));
			}
			else
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[0], listNode[2]));
			}
		}*/

	}

	public List<Triangle> Triangles {
		get { return triangles; }
	}

	public double Spacing {
		get { return spacing; }
		set { spacing = value; }
	}
}
