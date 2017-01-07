using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

public class TRGDelaunay
{
    // Matrisse pour utilise pour vérifier si un point D se trouve dans le cercle circonscrit à A, B et C
    public float[,] matrisse = new float[3, 3];
    public List<Triangle> listTriangle = new List<Triangle>();
    public ArrayList listNode = new ArrayList();
    public List<Node> listNodeTemp = new List<Node>();

    public TRGDelaunay(NodeGroup build)
    {
        foreach (Node nd in build.nodes)
        {
            listNode.Add(nd);
        }
    }

    public void rempMatrisse(Node noeudA, Node noeudB, Node noeudC, Node noeudD)
    {
        // calcul vecteur AB
        double xa = System.Convert.ToDouble(noeudA.getLongitude());
        double xb = System.Convert.ToDouble(noeudB.getLongitude());
        double ya = System.Convert.ToDouble(noeudA.getLatitude());
        double yb = System.Convert.ToDouble(noeudB.getLatitude());
        double[] vectAB = new double[2] { (xb - xa), (yb - ya) };
        // calcul vecteur AC
        double xc = System.Convert.ToDouble(noeudC.getLongitude());
        double yc = System.Convert.ToDouble(noeudC.getLatitude());
        double[] vectAC = new double[2] { (xc - xa), (yc - ya) };

        // calcul signe angle orienté
        // cela revient a calculer regarder le signe du determinant de vectAB, vectAC
        double res = (vectAB[0] * vectAC[1]) - (vectAB[1] * vectAC[0]);

        // Le critere marche que si A B et C sont dans le sens trigo
        if (res < 0)
        {
            //1,1
            this.matrisse[0, 0] = noeudA.getLongitude() - noeudD.getLongitude();
            //1,2
            this.matrisse[0, 1] = noeudA.getLatitude() - noeudD.getLatitude();
            //1,3
            this.matrisse[0, 2] = ((noeudA.getLongitude() * noeudA.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) 
                + ((noeudA.getLatitude() * noeudA.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
            //2,1
            this.matrisse[1, 0] = noeudC.getLatitude() - noeudD.getLatitude();
            //2,2
            this.matrisse[1, 1] = noeudC.getLatitude() - noeudD.getLatitude();
            //2,3
            this.matrisse[1, 2] = ((noeudC.getLongitude() * noeudC.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) 
                + ((noeudC.getLatitude() * noeudC.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
            //3,1
            this.matrisse[2, 0] = noeudB.getLatitude() - noeudD.getLatitude();
            //3,2
            this.matrisse[2, 1] = noeudB.getLatitude() - noeudD.getLatitude();
            //3,3
            this.matrisse[2, 2] = ((noeudB.getLongitude() * noeudB.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) 
                + ((noeudB.getLatitude() * noeudB.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));

        }
        //1,1
        this.matrisse[0, 0] = noeudA.getLongitude() - noeudD.getLongitude();
        //1,2
        this.matrisse[0, 1] = noeudA.getLatitude() - noeudD.getLatitude();
        //1,3
        this.matrisse[0, 2] = ((noeudA.getLongitude() * noeudA.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) + ((noeudA.getLatitude() * noeudA.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
        //2,1
        this.matrisse[1, 0] = noeudB.getLatitude() - noeudD.getLatitude();
        //2,2
        this.matrisse[1, 1] = noeudB.getLatitude() - noeudD.getLatitude();
        //2,3
        this.matrisse[1, 2] = ((noeudB.getLongitude() * noeudB.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) + ((noeudB.getLatitude() * noeudB.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
        //3,1
        this.matrisse[2, 0] = noeudC.getLatitude() - noeudD.getLatitude();
        //3,2
        this.matrisse[2, 1] = noeudC.getLatitude() - noeudD.getLatitude();
        //3,3
        this.matrisse[2, 2] = ((noeudC.getLongitude() * noeudC.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) + ((noeudC.getLatitude() * noeudC.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
    }

    public float detMatrisse()
    {
        float res,res1,res2,res3;
        res1 = matrisse[0, 0] * (matrisse[1, 1] * matrisse[2, 2] - matrisse[2, 1] * matrisse[1, 2]);
        res2 = matrisse[0, 1] * (matrisse[1, 0] * matrisse[2, 2] - matrisse[2, 0] * matrisse[1, 2]);
        res3 = matrisse[0, 2] * (matrisse[1, 0] * matrisse[2, 1] - matrisse[2, 0] * matrisse[1, 1]);
        res = res1 - res2 + res3;
        return res;
    }

    public bool isWithin()
    {
        if (detMatrisse() > 0)
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
        minY = listNodeTemp[0].getLatitude();
        maxY = listNodeTemp[0].getLatitude();
        minX = listNodeTemp[0].getLongitude();
        maxX = listNodeTemp[0].getLongitude();


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
                minY = nd.getLongitude();
            }
        }

        // valeur par deffaut
        maxX += 50;
        maxY += 50;
        minX -= 50;
        minY -= 50;

        //Création des points composant la boite englobante
        Node temp1 = new Node(1, minY, minX);
        Node temp2 = new Node(2, minY, maxX);
        Node temp3 = new Node(3, maxY, minX);
        Node temp4 = new Node(4, maxY, maxX);

        // Ajout de ces points dans la listes des point déja traiter
        listNodeTemp.Add(temp1);
        listNodeTemp.Add(temp2);
        listNodeTemp.Add(temp3);
        listNodeTemp.Add(temp4);

        // Ajout de deux triangles respectant les conditons de Delaunay
        listTriangle.Add(new Triangle(temp1, temp2, temp3));
        listTriangle.Add(new Triangle(temp4, temp3, temp2));

    }
}