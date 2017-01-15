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
    public float ecart = 0.1f;


    public TRGDelaunay (NodeGroup build)
    {
        foreach (Node nd in build.nodes)
        {
            listNode.Add(nd);
        }
    }

    public bool isWithin(Triangle tri , Node n)
    {
        float A, B;
        float moduleA, moduleB;
        tri.calculCentre();
        //UnityEngine.Debug.Log("noeud A " + tri.noeudA.longitude + " centre " + tri.centre.longitude);
        A = ((tri.noeudA.longitude - tri.centre.getLongitude()) * (tri.noeudA.longitude - tri.centre.getLongitude())) + ((tri.noeudA.latitude - tri.centre.getLatitude()) * (tri.noeudA.latitude - tri.centre.getLatitude()));
        B = ((n.longitude - tri.centre.getLongitude()) * (n.longitude - tri.centre.getLongitude())) + ((n.latitude - tri.centre.getLatitude()) * (n.latitude - tri.centre.getLatitude()));
        moduleA = (float)Math.Sqrt(System.Convert.ToDouble(A));
        moduleB = (float)Math.Sqrt(System.Convert.ToDouble(B));

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

        float maxX, maxY, minX, minY;

        minY = listNode[0].latitude;
        maxY = listNode[0].latitude;
        minX = listNode[0].longitude;
        maxX = listNode[0].longitude;


        foreach(Node nd in listNode)
        {
            if (nd.latitude > maxY)
            {
                maxY = nd.latitude;
            }
            if (nd.latitude < minY)
            {
                minY = nd.latitude;
            }
            if (nd.longitude > maxX)
            {
                maxX = nd.longitude;
            }
            if (nd.longitude < minX)
            {
                minX = nd.longitude;
            }
        }

        // valeur par deffaut = 0.1f
        maxX += ecart;
        maxY += ecart;
        minX -= ecart;
        minY -= ecart;


        //Création des points composant la boite englobante
        Node temp1 = new Node(1, minY / 1000f, minX / 1000f);
        Node temp2 = new Node(2, maxY / 1000f, minX / 1000f);
        Node temp3 = new Node(3, maxY / 1000f, maxX / 1000f);


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