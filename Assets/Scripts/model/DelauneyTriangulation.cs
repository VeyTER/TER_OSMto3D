using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

/// <summary>
/// 	Suite de méthodes permettant d'effectuer une triangulation de Delaunay pour construire les toits.
/// </summary>
public class DelauneyTriangulation {
	/// <summary>Triangles produit par la triangulation et destinés à être affichés.</summary>
	private List<Triangle> triangles;


	/// <summary>Triangles destiné à être supprimés de la liste finale car ne respectant pas les conditions.</summary>
	private List<Triangle> oldTriangles;

	/// <summary>Triangles destiné à être ajoutés de la liste finale.</summary>
	private List<Triangle> newTriangles;


	/// <summary>Neods à partir desquels construire la triangulation.</summary>
	private List<Node> nodes = new List<Node>();

	/// <summary>Neods à partir desquels construire la triangulation.</summary>
	private List<Node> nodesTemp = new List<Node>();


	public DelauneyTriangulation (NodeGroup buildingNodeGroup) {
		this.triangles = new List<Triangle>();

		this.oldTriangles = new List<Triangle>();
		this.newTriangles = new List<Triangle>();

		foreach (Node node in buildingNodeGroup.Nodes)
			nodes.Add(node);
		
		nodes.RemoveAt(nodes.Count - 1);
	}


	/// <summary>
	///		<para>
	/// 		Créé un triangle dont le cercle circonscrit englobe tout le batiment.
	/// 	</para>
	/// 	<para>
	/// 		ATTENTION : Cette fonction ne créé pas de boîte englobante, actuellement, elle ne sert qu'à la création
	/// 		de triangles pour débuter une triangulation de Delauney.
	/// 	</para>
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

		// Création des triangles
		for (int i = 0; i < nodes.Count; i++) {
			if (i < (nodes.Count - 2))
				newTriangles.Add(new Triangle(nodes[i], nodes[i + 1], nodes[i + 2]));
			else if (i < (nodes.Count - 1))
				newTriangles.Add(new Triangle(nodes[i], nodes[i + 1], nodes[0]));
			else
				newTriangles.Add(new Triangle(nodes[i], nodes[0], nodes[1]));
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


	public void Start() {
		// Parcours de la liste des noeud
		foreach(Node node in nodes) {

			// Parcours de la liste des triangles
			foreach(Triangle triangle1 in triangles) {
				// Ajout du triangle à la liste des triangles à supprimer de la liste des triangles si celui-ci ne
				// respecte pas les conditions
				if (this.IsWithin(triangle1, node))
					oldTriangles.Add(triangle1);
			}

			// Ajout de tout les noeuds differents des triangles à supprimer dans la liste des noeuds temporaire
			foreach(Triangle triangle2 in oldTriangles) {
				if (!nodesTemp.Contains(triangle2.NodeA))
					nodesTemp.Add(triangle2.NodeA);

				if (!nodesTemp.Contains(triangle2.NodeB))
					nodesTemp.Add(triangle2.NodeB);

				if (!nodesTemp.Contains(triangle2.NodeC))
					nodesTemp.Add(triangle2.NodeC);
			}

			// Creation des triangles à partir de deux points dans la liste des noeuds temporaire et le noeud courant
			for(int i = 0; i < nodesTemp.Count; i++) {
				if (i < (nodesTemp.Count - 1))
					newTriangles.Add(new Triangle(nodesTemp[i], nodesTemp[i + 1], node));
				else
					newTriangles.Add(new Triangle(nodesTemp[i], nodesTemp[0], node));
			}

			// Suppression les triangle à supprimer de la liste des triangles 
			foreach (Triangle triangle3 in oldTriangles)
				triangles.Remove(triangle3);

			// Ajout des triangles à ajouter à la liste des triangles
			foreach(Triangle triangle4 in newTriangles)
				triangles.Add(triangle4);

			// Suppression des listes temporaires
			newTriangles.Clear();
			oldTriangles.Clear();
			nodesTemp.Clear();
		}
	}


	/// <summary>
	/// 	Détermine si un point est a l'interieur du cercle circoncrit d'un triangle.
	/// </summary>
	/// <param name="tri">Triangle à tester.</param>
	/// <param name="n">Point à traiter.</param>
	/// <returns><c>true</c>, si le point est à l'interieur du triangle, <c>false</c> sinon.</returns>
	public bool IsWithin(Triangle triangle, Node node) {
		Node triangleCenter = triangle.Center ();

		// Calcul du rayon du cercle circonscri au triangle
		double quadraticSumA = Math.Pow((triangle.NodeA.Longitude - triangleCenter.Longitude), 2) + Math.Pow((triangle.NodeA.Latitude - triangleCenter.Latitude), 2);
		double moduleA = Math.Sqrt(quadraticSumA);

		// Calcul de la distance entre le centre du cercle circoncrit et le point en paramètre
		double quadraticSumB = Math.Pow((node.Longitude - triangleCenter.Longitude), 2) + Math.Pow((node.Latitude - triangleCenter.Latitude), 2);
		double moduleB = Math.Sqrt(quadraticSumB);

		if (moduleA > moduleB)
			return true;
		else
			return false;
	}


	public List<Triangle> Triangles {
		get { return triangles; }
	}
}
