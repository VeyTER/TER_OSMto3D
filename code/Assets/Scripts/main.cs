using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class main : MonoBehaviour {

    // liste des nodes ( structure de donnée )
    public static ArrayList nodes = new ArrayList();
    // liste des groupes de nodes ( structure de donnée )
    public static ArrayList nodeGroups = new ArrayList();

    // coordonnées min et max de la carte
    public static float minlat, maxlat, minlon, maxlon;
	// chemin d'acces et nom du fichier
	private string path = @"./Assets/";
	// "map" est la valeur qu'on met par defaut dans fileName. 
	// Mais celle-ci sera écrasée par la valeur qu'on met dans File Name dans ScriptGameObject (sur Unity)
	public string fileName = "map";
	// listes des gameObjects créés dans la scene
	public static GameObject[] mainWalls;
	public static GameObject[] mainHighwaysCons;
	public static GameObject[] mainBuildingNodes;
	public static GameObject[] mainHighwayNodes;
	public static GameObject panel = null;

    //création d'une instance de GestFile
    public GestFile f = new GestFile();
	//création d'une instance de ObjectBuilding
	public ObjectBuilding ob = new ObjectBuilding();
    //création d'une instance de TRGDelaunay
    public TRGDelaunay tr;

    // Fonction lancée à l'initialisation de la scene
    void Start () {

		SetUpUI ();

        // Si le fichier Resumed n'existe pas on le crée
        if (!System.IO.File.Exists(path + "MapsResumed/" + fileName + "Resumed.osm"))
        {
            f.readFileOSM(fileName);
            f.createResumeFile(fileName);
        }
        else
        {
            f.readResumeFile(fileName);
        }
/*
        // si la carte n'a pas de fichier de parametre on le créé
        if (!System.IO.File.Exists (path + "MapsSettings/" + fileName + "Settings.osm")) {*/
            f.createSettingsFile(fileName);	
		//}
		//else{
            f.readSettingsFile(fileName);
        //  }
        //}

        foreach(NodeGroup ngp in nodeGroups)
        {
            tr = new TRGDelaunay(ngp);
            tr.creaBoiteEnglob();
            tr.start();
            foreach (Triangle tri in tr.listTriangle)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                cube.transform.position = new Vector3(tri.noeudA.latitude, 0, tri.noeudA.longitude);
                cube.name = "" + tri.noeudA.id;


                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                cube.transform.position = new Vector3(tri.noeudB.latitude, 0, tri.noeudB.longitude);
                cube.name = "" + tri.noeudB.id;


                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                cube.transform.position = new Vector3(tri.noeudC.latitude, 0, tri.noeudC.longitude);
                cube.name = "" + tri.noeudC.id;

            }
        }
        
		ob.setNodeGroups(nodeGroups);
	    ob.setLatLong(minlat, maxlat, minlon, maxlon);
		ob.buildNodes();
		ob.buildWalls();
		//ob.buildHighways ();

		// recommandé respecter un ration de interv/taille = 5 avec 0.01 0.002 si pas beaucoup de batiments
		//ob.buildRoofs (0.03f,0.006f); //probleme de format dans la fonction, à commenter pour le moment
		ob.buildMainCameraBG ();

		// on recupere la reference du panneau et on le desactive
		panel = GameObject.Find ("Panneau");
		panel.SetActive(false);  
        foreach (NodeGroup ngp in nodeGroups){

            UnityEngine.Debug.Log(ngp.district);
            UnityEngine.Debug.Log(ngp.name);
        }
	}
		

	// mise en place de l'interface
	public void SetUpUI(){

		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("UIManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("myCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("myEventSystem"));

	}
}
