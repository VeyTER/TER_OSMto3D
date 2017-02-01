using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

public class TRGDelaunay
{
	public List<Triangle> listTriangle = new List<Triangle>();
	protected List<Triangle> listTriangleAdd = new List<Triangle>();
	protected List<Triangle> listTriangleSupp = new List<Triangle>();
	protected List<Node> listNode = new List<Node>();
	protected List<Node> listNodeTemp = new List<Node>();
	protected List<Node> listNodeRm = new List<Node>();
	protected double ecartement = 0.0001d;

	// Constructeur
	public TRGDelaunay (NodeGroup build)
	{
		int temp;
		foreach (Node nd in build.nodes)
		{
			listNode.Add(nd);
		}
		temp = listNode.Count;
		listNode.RemoveAt(temp-1);
	}


	/// <summary>
	/// Methode isWithin :
	/// Determine si un point est a l'interieur du cercle circoncrit d'un triangle
	/// </summary>
	/// <param name="tri"> Triangle </param>
	/// <param name="n"> Point à traiter </param>
	/// <returns> True : Si le point est à l'iterieur </returns>
	public bool isWithin(Triangle tri , Node n)
	{
		double A, B;
		double moduleA, moduleB;

		// Calcul rayon du cercle circonscri au triangle
		A = Math.Pow((tri.getNoeudA().getLongitude() - tri.getCentre().getLongitude()), 2) + Math.Pow((tri.getNoeudA().getLatitude() - tri.getCentre().getLatitude()), 2);
		moduleA = Math.Sqrt(A);

		// Calcul de la distance entre le centre du cercle circoncrit et le point en parametre
		B = Math.Pow((n.getLongitude() - tri.getCentre().getLongitude()), 2) + Math.Pow((n.getLatitude() - tri.getCentre().getLatitude()), 2);
		moduleB = Math.Sqrt(B);

		if (moduleA > moduleB)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Methode creaBoiteEnglob:
	/// Creer un triangle dont le cercle circonscrit englobe tout le batiment
	/// CETTE FONCTION NE CREER PAS DE BOITE ENGLOBANTE
	/// ACTUELLEMENT ELLE NE SERT QU'A LA CREATION DE TRIANGLES POUR DEBUTER
	/// UNE TRIAGULATION DE DELAUNAY
	/// </summary>
	public void creaBoiteEnglob()
	{
		/*double maxX, maxY, minX, minY;

        minY = listNode[0].getLatitude();
        maxY = listNode[0].getLatitude();
        minX = listNode[0].getLongitude();
        maxX = listNode[0].getLongitude();

        // Parcours de la liste de node pour la mise a jours des latitude/longitude min et max
        foreach(Node nd in listNode)
        {
            if (nd.getLatitude() > maxY)
            {
                maxY = nd.getLatitude();
            }
            if (nd.getLatitude() < minY)
            {
                minY = nd.getLatitude();
            }
            if (nd.getLongitude() > maxX)
            {
                maxX = nd.getLongitude();
            }
            if (nd.getLongitude() < minX)
            {
                minX = nd.getLongitude();
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
		for (int i = 0; i < listNode.Count; i++)
		{
			if (i < (listNode.Count - 2))
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[i + 1], listNode[i + 2]));
			}
			else if (i < (listNode.Count - 1))
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[i + 1], listNode[0]));
			}
			else
			{
				listTriangleAdd.Add(new Triangle(listNode[i], listNode[0], listNode[1]));
			}
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


	/// <summary>
	/// Methode start:
	/// Triangule les points du batiment pour construire les toits
	/// </summary>
	public void start()
	{
		// On parcour la liste des noeud
		foreach(Node n in listNode)
		{

			// On parcour la liste des triangles
			foreach(Triangle t1 in listTriangle)
			{
				// Si le triangle ne respecte pas les conditions on l'ajout à la liste des triangles à supprimer de la liste des triangles
				//UnityEngine.Debug.Log("is " + this.isWithin(t1, n));
				if (this.isWithin(t1, n))
				{
					listTriangleSupp.Add(t1);
				}
			}

			// Ajout de tout les noeuds differents des triangles à supprimer dans la liste des noeuds temporaire
			foreach(Triangle t2 in listTriangleSupp)
			{
				if (!listNodeTemp.Contains(t2.getNoeudA()))
				{
					listNodeTemp.Add(t2.getNoeudA());
				}
				if (!listNodeTemp.Contains(t2.getNoeudB()))
				{
					listNodeTemp.Add(t2.getNoeudB());
				}
				if (!listNodeTemp.Contains(t2.getNoeudC()))
				{
					listNodeTemp.Add(t2.getNoeudC());
				}
			}

			// Creation des triangles à partir de deux points dans la liste des noeuds temporaire et le noeud courant
			for(int i = 0; i<listNodeTemp.Count;i++)
			{
				if (i < (listNodeTemp.Count - 1))
				{
					listTriangleAdd.Add(new Triangle(listNodeTemp[i], listNodeTemp[i + 1], n));
				}
				else
				{
					listTriangleAdd.Add(new Triangle(listNodeTemp[i], listNodeTemp[0], n));
				}
			}

			// On retire de la liste des triangles les triangle à supprimer
			foreach (Triangle t3 in listTriangleSupp)
			{
				listTriangle.Remove(t3);
			}

			// On ajoute à la liste des triangles les triangles à ajouter
			foreach(Triangle t4 in listTriangleAdd)
			{
				listTriangle.Add(t4);
			}

			// On efface les listes temporaires
			listTriangleAdd.Clear();
			listTriangleSupp.Clear();
			listNodeTemp.Clear();
		}

	}

	// Accesseur de l'atribut ecartement
	public void setEcartement(double ecart)
	{
		this.ecartement = ecart;
	}
	public double getEcartement()
	{
		return this.ecartement;
	}
}
