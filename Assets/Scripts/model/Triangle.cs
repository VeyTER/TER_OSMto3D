using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Triangle {
	// Les trois sommets d'un triangle
	private Node nodeA;
	private Node nodeB;
	private Node nodeC;

	// Constructeur
	public Triangle (Node noeudA,Node noeudB, Node noeudC) {
		this.nodeA = noeudA;
		this.nodeB = noeudB;
		this.nodeC = noeudC;
	}

	/// <summary>
	/// Methode cotainNode :
	/// Retourne true si le noeud passe en parametre est dans le triangle
	/// </summary>
	/// <param name="noeud"></param>
	/// <returns></returns>
	public bool ContainNode(Node noeud) {
		bool res = false;
		if((noeud.Reference == nodeA.Reference) || (noeud.Reference == nodeB.Reference) || (noeud.Reference == nodeC.Reference))
			res = true;
		return res;
	}

	/// <summary>
	/// Methode calculCentre :
	/// Calcul le centre du cercle circoncrit d'un triangle
	/// </summary>
	public Node Center() {
		double nalat, nalon, nblat, nblon, nclat, nclon;
		double axc, bxc, dxc,ayc,byc, xc, yc;
		double templat,templon;

		// Récupération des coordonnee des trois sommets du triangle
		nalat = this.nodeA.Latitude;
		nalon = this.nodeA.Longitude;

		nblat = this.nodeB.Latitude;
		nblon = this.nodeB.Longitude;

		nclat = this.nodeC.Latitude;
		nclon = this.nodeC.Longitude;

		// Test si la coordonne y de du noeudB n'est pas egale à une autre
		// sinon on chage l'ordre des sommets du triangle si necessaire
		if ((nblat==nalat) || (nblat == nclat)) {
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
		if ((nblat == nalat) || (nblat == nclat)) {
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
		return new Node(xc, yc);
	}

	/// Getter de l'attribut de noeudA
	public Node NodeA {
		get { return nodeA; }
	}

	/// Getter de l'attribut de noeudB
	public Node NodeB {
		get { return nodeB; }
	}

	/// Getter de l'attribut de noeudB
	public Node NodeC {
		get { return nodeC; }
	}
}
