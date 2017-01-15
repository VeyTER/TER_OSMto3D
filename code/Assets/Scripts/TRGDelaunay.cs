using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

public class TRGDelaunay
{
    public List<Triangle> listTriangle = new List<Triangle>();
    public List<Triangle> listTriangleAdd = new List<Triangle>();
    public List<Triangle> listTriangleSupp = new List<Triangle>();
    public List<Node> listNode = new List<Node>();
    public List<Node> listNodeTemp = new List<Node>();
    public List<Node> listNodeRm = new List<Node>();
    public double ecartement = 0.1d;


    public TRGDelaunay (NodeGroup build)
    {
        foreach (Node nd in build.nodes)
        {
            listNode.Add(nd);
        }
    }

    public bool isWithin(Triangle tri , Node n)
    {
        double A, B;
        double moduleA, moduleB;
        tri.calculCentre();
        //UnityEngine.Debug.Log("noeud A " + tri.noeudA.longitude + " centre " + tri.centre.longitude);
        A = Math.Pow((tri.noeudA.getLongitude() - tri.centre.getLongitude()), 2) + Math.Pow((tri.noeudA.getLatitude() - tri.centre.getLatitude()), 2);
        B = Math.Pow((n.getLongitude() - tri.centre.getLongitude()), 2) + Math.Pow((n.getLatitude() - tri.centre.getLatitude()), 2); 
        moduleA = Math.Sqrt(A);
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

    public void creaBoiteEnglob()
    {

        double maxX, maxY, minX, minY;

        minY = listNode[0].getLatitude();
        maxY = listNode[0].getLatitude();
        minX = listNode[0].getLongitude();
        maxX = listNode[0].getLongitude();


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
        

        // Ajout d'un triangle respectant les conditons de Delaunay
        listTriangle.Add(new Triangle(temp1, temp2, temp3));
        //UnityEngine.Debug.Log("nalat " +listTriangle[0].noeudA.latitude +" nblat "+ listTriangle[0].noeudB.latitude +" nclat " + listTriangle[0].noeudC.latitude);
        //UnityEngine.Debug.Log("nalon " + listTriangle[0].noeudA.longitude + " nblon " + listTriangle[0].noeudB.longitude + " nclon " + listTriangle[0].noeudC.longitude);


    }

    public void start()
    {
        foreach(Node n in listNode)
        {
            foreach(Triangle t1 in listTriangle)
            {
                if (this.isWithin(t1, n))
                {
                    listTriangleSupp.Add(t1);
                }
            }
            
            foreach(Triangle t2 in listTriangleSupp)
            {
                if (!listNodeTemp.Contains(t2.noeudA))
                {
                    listNodeTemp.Add(t2.noeudA);
                }
                if (!listNodeTemp.Contains(t2.noeudB))
                {
                    listNodeTemp.Add(t2.noeudB);
                }
                if (!listNodeTemp.Contains(t2.noeudC))
                {
                    listNodeTemp.Add(t2.noeudC);
                }
            }

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

            foreach (Triangle t3 in listTriangleSupp)
            {
                listTriangle.Remove(t3);
            }
            foreach(Triangle t4 in listTriangleAdd)
            {
                listTriangle.Add(t4);
            }
            listTriangleAdd.Clear();
            listTriangleSupp.Clear();
            listNodeTemp.Clear();
        }
        foreach(Node n in listNodeRm)
        {
            foreach (Triangle t5 in listTriangle)
            {
                if (t5.containNode(n))
                {
                    listTriangleSupp.Add(t5);
                }
            }
        }
        foreach(Triangle t6 in listTriangleSupp)
        {
            listTriangle.Remove(t6);
        }
        listTriangleSupp.Clear();
        UnityEngine.Debug.Log("liste triangle = " + listTriangle.Count);
    }
}