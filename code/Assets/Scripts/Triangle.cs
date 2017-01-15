using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Triangle
{
    // Les trois sommets d'un triangle
    public Node noeudA;
    public Node noeudB;
    public Node noeudC;
    public Node centre;

    // Constructeur
    public Triangle (Node noeudA,Node noeudB, Node noeudC)
    {
        this.noeudA = noeudA;
        this.noeudB = noeudB;
        this.noeudC = noeudC;
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
        if((noeud.id==noeudA.id)|| (noeud.id == noeudB.id) || (noeud.id == noeudC.id))
        {
            res = true;
        }
        return res;
    }
    
    public void calculCentre()
    {
        float nalat, nalon, nblat, nblon, nclat, nclon;
        float axc, bxc, dxc,ayc,byc, xc, yc;
        float templat,templon;

        nalat = this.noeudA.latitude;
        nalon = this.noeudA.longitude;
        nblat = this.noeudB.latitude;
        nblon = this.noeudB.longitude;
        nclat = this.noeudC.latitude;
        nclon = this.noeudC.longitude;
 
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



        axc = ((nclon * nclon) - (nblon * nblon)) / (nclat - nblat);

        bxc = (((nalon * nalon) - (nblon * nblon)) / (nalat - nblat)) + (nclat - nalat);

        dxc = (2 * ((nclon - nblon) / (nclat - nblat))) - (2 * ((nalon - nblon) / (nalat - nblat)));
        xc = (axc - bxc) / dxc;
        ayc = -((nblon - nalon) / (nblat - nalat));
        byc = (((nblon * nblon) - (nalon * nalon)) + ((nblat * nblat) - (nalat * nalat))) / (2 * (nblat - nalat));
        yc = ayc * xc + byc;
       /* UnityEngine.Debug.Log("axc " + axc + " bxc " + bxc + " dxc " + dxc);
        UnityEngine.Debug.Log("ayc " + ayc + " byc " + byc);
        UnityEngine.Debug.Log("xc = " + xc + " yc = " + yc);
        UnityEngine.Debug.Log("nalon = " + nalon + " nalat = " + nalat);*/
        this.centre = new Node(yc, xc);

    }

}

