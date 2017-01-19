using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class main : MonoBehaviour {

    // liste des nodes ( structure de donnée )
    public static ArrayList nodes = new ArrayList();
    // liste des groupes de nodes ( structure de donnée )
    public static ArrayList nodeGroups = new ArrayList();

    // coordonnées min et max de la carte
    public static double minlat, maxlat, minlon, maxlon;
	// chemin d'acces et nom du fichier
	private string path = @"./Assets/";
	// "map" est la valeur qu'on met par defaut dans fileName. 
	// Mais celle-ci sera écrasée par la valeur qu'on met dans File Name dans ScriptGameObject (sur Unity)
	public string fileName;
	// listes des gameObjects créés dans la scene
	public static GameObject[] mainWalls;
    public static GameObject[] mainRoofs;
    public static GameObject[] mainHighways;
	public static GameObject[] mainBuildingNodes;
	public static GameObject[] mainHighwayNodes;
	public static GameObject panel = null;
	public Material roadMaterial;
    public Material roofMaterial;
    //création d'une instance de GestFile
    public GestFile f = new GestFile();

    //création d'une instance de TRGDelaunay
    public TRGDelaunay tr;

    // Fonction lancée à l'initialisation de la scene
    void Start() {
        
        //création d'une instance de ObjectBuilding
        ObjectBuilding ob = new ObjectBuilding(roadMaterial, roofMaterial);
        SetUpUI ();
        // Si le fichier Resumed n'existe pas on le crée
        if (!System.IO.File.Exists(path + "MapsResumed/" + fileName + "Resumed.osm"))
        {
            f.readFileOSM(fileName);

            f.createSettingsFile(fileName);
            f.readSettingsFile(fileName);

            f.createResumeFile(fileName);
        }
        else
        {
            f.readResumeFile(fileName);
        }
        
        foreach(NodeGroup ngp in nodeGroups)
        {
            if (ngp.isBuilding())
            {
                tr = new TRGDelaunay(ngp);
                tr.creaBoiteEnglob();
                tr.start();
                
                /*foreach (Triangle item in tr.listTriangle)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    cube.transform.position = new Vector3((float)item.getNoeudA().getLongitude() * 1000f, 0, (float)item.getNoeudA().getLatitude() * 1000f);
                    cube.name = "" + item.getNoeudA().getID();
                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    cube.transform.position = new Vector3((float)item.getNoeudB().getLongitude()*1000f, 0, (float)item.getNoeudB().getLatitude() * 1000f);
                    cube.name = "" + item.getNoeudB().getID();
                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    cube.transform.position = new Vector3((float)item.getNoeudC().getLongitude() * 1000f, 0, (float)item.getNoeudC().getLatitude() * 1000f);
                    cube.name = "" + item.getNoeudC().getID(); 
                }*/
                ob.buildRoofs(tr);
            }
        }
        ob.setNodeGroups(nodeGroups);
	    ob.setLatLong(minlat, maxlat, minlon, maxlon);
		ob.buildNodes();
		ob.buildWalls();
		ob.buildHighways ();
		ob.buildMainCameraBG ();

		// on recupere la reference du panneau et on le desactive
		panel = GameObject.Find ("Panneau");
		panel.SetActive(false);  

        foreach (NodeGroup ngp in nodeGroups){

            UnityEngine.Debug.Log(ngp.getDistrict());
            UnityEngine.Debug.Log(ngp.getName());
        


        }
	}
		

	// mise en place de l'interface
	public void SetUpUI(){

		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("UIManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("myCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("myEventSystem"));

	}

    public void modifPoint(NodeGroup ngp)
    {
        double nouvLat, nouvLon;
        double xa, xb, xc, ya, yb, yc;
        double a, d;
        double coef, dist;
        double xba, yba, xbc, ybc, angleABC;

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


            //angleABC = ((xba * xbc) + (yba * ybc)) / ((float)(Math.Sqrt(Math.Pow((double)xbc, 2) + Math.Pow((double)ybc, 2)) * (Math.Sqrt(Math.Pow((double)xbc, 2) + Math.Pow((double)ybc, 2)))));
            //angleABC = (float)Math.Acos((double)angleABC);
            UnityEngine.Debug.Log("angle : " + angleABC + "  Xa : " + xa + "  Xb : " + xb + "  Xc : " + xc);
            if (angleABC > 70 && angleABC < 110)
            //if(((xb-xa)/(xc-xb))-((yb-ya)/(yc-yb)) < 0.2)
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
            /*else if (angleABC >0.4&& angleABC<0.6 || angleABC<-0.4 && angleABC>-0.6)
            {

            }*/

            else if (angleABC < 20 || angleABC > 170)
            {
                //if (i < ngp.nbNode - 3)
                ngp.removeNode(i + 2);
            }

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

			ba = new Vector3((float)xba, 0, (float)yba);
			bc = new Vector3((float)xbc, 0, (float)ybc);

            angleABC = Vector3.Angle(ba, bc);
            UnityEngine.Debug.Log("nouvel angle : " + angleABC + "  Xa : " + xa + "  Xb : " + xb + "  Xc : " + xc);
        }
        /*
        xa = ngp.getNode(0).getLongitude();
        ya = ngp.getNode(0).getLatitude();

        xb = ngp.getNode(ngp.nbNode - 1).getLongitude();
        yb = ngp.getNode(ngp.nbNode - 1).getLatitude();
        

        xc = ngp.getNode(ngp.nbNode-2).getLongitude();
        yc = ngp.getNode(ngp.nbNode-2).getLatitude();
        
        
        
        
        nouvLat = ((ya / 2) + (yc / 2)) - (float)Math.Sqrt((4 * xa * xb - 4 * xb * xc - 2 * ya * yc - 4 * (float)(Math.Pow(xb, 2)) + (float)((Math.Pow(ya, 2)) + Math.Pow(yc, 2))) / 2);
        //(-((yb - ya) * (yc - yb)) / (xc - xb));
        yb1 = ((ya / 2) + (yc / 2)) + (float)Math.Sqrt((4 * xa * xb - 4 * xb * xc - 2 * ya * yc - 4 * (float)(Math.Pow(xb, 2)) + (float)((Math.Pow(ya, 2)) + Math.Pow(yc, 2))) / 2);
        nouvLon = (xa / 2) + (xc / 2) - (ya / 2) + (yc / 2);
        //(-((xb - xa) * (xc - xb)) / (yc - yb));
        xb1 = (xa / 2) + (xc / 2) + (ya / 2) - (yc / 2);

        //ngp.getNode(ngp.nbNode - 2).setLongitude(nouvLon);
        ngp.getNode(1).setLongitude(nouvLon);

        a = Math.Sqrt(Math.Pow(xc - xb, 2) + Math.Pow(yc - yb, 2));
        if (yb - nouvLat > yb - yb1)
        {
            nouvLat = yb1;
        }
        if (xb - nouvLon > xb - xb1)
        {
            nouvLon = xb1;
        }
        ngp.getNode(ngp.nbNode-1).setLatitude(40000);
        ngp.getNode(0).setLatitude(40000);
        if ((yb - nouvLat) < (xb - nouvLon))
        {
            d = Math.Sqrt(Math.Pow(xc - xb, 2) + Math.Pow(yc - nouvLat, 2));
            coef = (float)(a / d);
            dist = nouvLat - yb;
            nouvLat = yc + (dist * coef);
            dist = xb - xc;
            xb = xc + (dist * coef);
            ngp.getNode(0).setLatitude(nouvLat);
            ngp.getNode(0).setLongitude(xb);
            ngp.getNode(ngp.nbNode-1).setLatitude(nouvLat);
            ngp.getNode(ngp.nbNode-1).setLongitude(xb);
        }
        else
        {
            d = Math.Sqrt(Math.Pow(xc - nouvLon, 2) + Math.Pow(yc - yb, 2));
            coef = (float)(a / d);
            dist = nouvLon - xb;
            nouvLon = xc + (dist * coef);
            dist = yb - yc;
            yb = yc + (dist * coef);
            ngp.getNode(0).setLongitude(nouvLon);
            ngp.getNode(0).setLatitude(yb);
            ngp.getNode(ngp.nbNode-1).setLongitude(nouvLon);
            ngp.getNode(ngp.nbNode-1).setLatitude(yb);
        }     */
    }

}
