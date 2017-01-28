using System.Collections;
using UnityEngine;
using System;

public class ModificationPoint {

	public ModificationPoint()
	{
	}

	public void modifPoint(ArrayList nodeGroups)
	{
		double xa, xb, xc, ya, yb, yc;
		double xba, yba, xbc, ybc, angleABC;
		double nouvLat, nouvLon, a, d, coef, dist;

		for (int j = 0; j < 5; j++)
		{
			foreach (NodeGroup ngp in nodeGroups)
			{
				if (ngp.isBuilding())
				{
					for (int i = 0; i < ngp.getNbNode() - 2; i++)
					{
						xa = ngp.getNode(i).getLongitude();
						ya = ngp.getNode(i).getLatitude();

						xb = ngp.getNode(i + 1).getLongitude();
						yb = ngp.getNode(i + 1).getLatitude();

						xc = ngp.getNode(i + 2).getLongitude();
						yc = ngp.getNode(i + 2).getLatitude();

						xba = xa - xb;
						yba = ya - yb;
						xbc = xc - xb;
						ybc = yc - yb;

						Vector3 ba = new Vector3((float)xba, 0, (float)yba);
						Vector3 bc = new Vector3((float)xbc, 0, (float)ybc);

						angleABC = Vector3.Angle(ba, bc);



						//Modification des angles avec calcul de vecteur
						/*if (angleABC > 70 && angleABC < 110)
                        {

                            nouvLat = (-((xb - xa) * (xc - xb)) / (yb - ya)) + yb;
                            nouvLon = (-((yb - ya) * (yc - yb)) / (xb - xa)) + xb;
                            a = Math.Sqrt(Math.Pow(xc - xb, 2) + Math.Pow(yc - yb, 2));
                            if ((yc - nouvLat) < (xc - nouvLon))
                            {
                                d = Math.Sqrt(Math.Pow(xc - xb, 2) + Math.Pow(nouvLat - yb, 2));
                                coef = a / d;
                                dist = nouvLat - yb;
                                nouvLat = yb + (dist * coef);
                                dist = xc - xb;
                                xc = xb + (dist * coef);
                                ngp.getNode(i + 2).setLatitude(nouvLat);
                                ngp.getNode(i + 2).setLongitude(xc);
                            }
                            else
                            {
                                d = Math.Sqrt(Math.Pow(nouvLon - xb, 2) + Math.Pow(yc - yb, 2));
                                coef = a / d;
                                dist = nouvLon - xb;
                                nouvLon = xb + (dist * coef);
                                dist = yc - yb;
                                yc = yb + (dist * coef);
                                //Math.Pow(ngp.getNode(i).getLatitude(),2);
                                ngp.getNode(i + 2).setLongitude(nouvLon);
                                ngp.getNode(i + 2).setLatitude(yc);
                            }

                        }
                        else */if (angleABC < 20 || angleABC > 170)
						{
							ngp.removeNode(i + 1);
						}
					}
				}
			}
		}
	}
}
