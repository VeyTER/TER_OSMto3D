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
    protected Node centre;


    // Constructeur
    public Triangle (Node noeudA,Node noeudB, Node noeudC)
    {
        this.noeudA = noeudA;
        this.noeudB = noeudB;
        this.noeudC = noeudC;
    }

    public void setNoeudA(Node n)
    {
        this.noeudA = n;
    }
    public Node getNoeudA()
    {
        return this.noeudA;
    }
    public void setNoeudB(Node n)
    {
        this.noeudB = n;
    }
    public Node getNoeudB()
    {
        return this.noeudB;
    }
    public void setNoeudC(Node n)
    {
        this.noeudC = n;
    }
    public Node getNoeudC()
    {
        return this.noeudC;
    }
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
    
    public void calculCentre()
    {
        double nalat, nalon, nblat, nblon, nclat, nclon;
        double axc, bxc, dxc,ayc,byc, xc, yc;
        double templat,templon;

        nalat = this.noeudA.getLatitude();
        nalon = this.noeudA.getLongitude();
        nblat = this.noeudB.getLatitude();
        nblon = this.noeudB.getLongitude();
        nclat = this.noeudC.getLatitude();
        nclon = this.noeudC.getLongitude();
 
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
        this.centre = new Node(xc, yc);

    }

}

