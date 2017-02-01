using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Triangle
{
	// Les trois sommets d'un triangle
	protected Node noeudA;
	protected Node noeudB;
	protected Node noeudC;
	// Le centre du cercle circonscrit
	protected Node centre;


	// Constructeur
	public Triangle (Node noeudA,Node noeudB, Node noeudC)
	{
		this.noeudA = noeudA;
		this.noeudB = noeudB;
		this.noeudC = noeudC;
		// Appel à la fonction qui calcul le centre du cercle circoncrit au triangle
		this.calculCentre();
		//UnityEngine.Debug.Log("noeud A " + tri.noeudA.longitude + " centre " + tri.centre.longitude);
	}

	// Accesseurs de l'attribut de noeudA
	public void setNoeudA(Node n)
	{
		this.noeudA = n;
	}
	public Node getNoeudA()
	{
		return this.noeudA;
	}

	// Accesseurs de l'attribut de noeudB
	public void setNoeudB(Node n)
	{
		this.noeudB = n;
	}
	public Node getNoeudB()
	{
		return this.noeudB;
	}

	// Accesseurs de l'attribut de noeudC
	public void setNoeudC(Node n)
	{
		this.noeudC = n;
	}
	public Node getNoeudC()
	{
		return this.noeudC;
	}

	// Accesseurs de l'attribut de centre
	public void setCentre(Node n)
	{
		this.centre = n;
	}
	public Node getCentre()
	{
		return this.centre;
	}


	/// <summary>
	/// Methode cotainNode :
	/// Retourne true si le noeud passe en parametre est dans le triangle
	/// </summary>
	/// <param name="noeud"></param>
	/// <returns></returns>
	public bool containNode(Node noeud)
	{
		bool res = false;
		if((noeud.getID() ==noeudA.getID()) || (noeud.getID() == noeudB.getID()) || (noeud.getID() == noeudC.getID()))
		{
			res = true;
		}
		return res;
	}

	/// <summary>
	/// Methode calculCentre :
	/// Calcul le centre du cercle circoncrit d'un triangle
	/// </summary>
	public void calculCentre()
	{
		double nalat, nalon, nblat, nblon, nclat, nclon;
		double axc, bxc, dxc,ayc,byc, xc, yc;
		double templat,templon;

		// Récupération des coordonnee des trois sommets du triangle
		nalat = this.noeudA.getLatitude();
		nalon = this.noeudA.getLongitude();
		nblat = this.noeudB.getLatitude();
		nblon = this.noeudB.getLongitude();
		nclat = this.noeudC.getLatitude();
		nclon = this.noeudC.getLongitude();

		// Test si la coordonne y de du noeudB n'est pas egale à une autre
		// sinon on chage l'ordre des sommets du triangle si necessaire
		if ((nblat==nalat) || (nblat == nclat)){
			templat = nalat;
			templon = nalon;
			nalat = nblat;
			nalon = nblon;
			nblat = nclat;
			nblon = nclon;
			nclat = templat;
			nclon = templon;
		}

		// Test si la coordonne y de du noeudB n'est pas egale à une autre
		// Sinon on chage l'ordre des sommets du triangle une deuxieme fois si necessaire
		if ((nblat == nalat) || (nblat == nclat)){
			templat = nalat;
			templon = nalon;
			nalat = nblat;
			nalon = nblon;
			nblat = nclat;
			nblon = nclon;
			nclat = templat;
			nclon = templon;
		}

		// Calcul des coordonnee du centre du cercle circonscrit
		axc = ((nclon * nclon) - (nblon * nblon)) / (nclat - nblat);
		bxc = (((nalon * nalon) - (nblon * nblon)) / (nalat - nblat)) + (nclat - nalat);
		dxc = (2 * ((nclon - nblon) / (nclat - nblat))) - (2 * ((nalon - nblon) / (nalat - nblat)));
		xc = (axc - bxc) / dxc;

		ayc = -((nblon - nalon) / (nblat - nalat));
		byc = (Math.Pow(nblon, 2) - Math.Pow(nalon, 2) + Math.Pow(nblat, 2) - Math.Pow(nalat, 2)) / (2 * (nblat - nalat));

		yc = (ayc * xc) + byc;

		// Mise a jour de l'attribut centre
		this.centre = new Node(xc, yc);

	}

}
