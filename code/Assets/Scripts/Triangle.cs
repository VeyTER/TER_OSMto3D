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
        

}

