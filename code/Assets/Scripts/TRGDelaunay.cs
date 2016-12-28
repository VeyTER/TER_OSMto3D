using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TRGDelaunay
{
    // Matrisse pour utilise pour vérifier si un point D se trouve dans le cercle circonscrit à A, B et C
    public float[,] matrisse = new float[3, 3];  

    public TRGDelaunay()
    {
    }

    public void rempMatrisse(Node noeudA, Node noeudB, Node noeudC, Node noeudD)
    {
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
        this.matrisse[2, 2] = ((noeudB.getLongitude() * noeudB.getLongitude()) - (noeudD.getLongitude() * noeudD.getLongitude())) + ((noeudB.getLatitude() * noeudB.getLatitude()) - (noeudD.getLatitude() * noeudD.getLatitude()));
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
}