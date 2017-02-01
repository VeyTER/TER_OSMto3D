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
	public static double minlat=0, maxlat=0, minlon=0, maxlon=0;
	public static double minlat2=0, maxlat2=0, minlon2=0, maxlon2=0;
	// chemin d'acces et nom du fichier
	private string path = @"./Assets/";

	//Les fileName sont instancie via l'interfce de Unity
	public string fileName;
	public string fileName2;

	// listes des gameObjects créés dans la scene
	public static GameObject[] mainWalls;
	public static GameObject[] mainRoofs;
	public static GameObject[] mainHighways;
	public static GameObject[] mainTrees;
	public static GameObject[] mainCycleways;
	public static GameObject[] mainBusLanes;
	public static GameObject[] mainFootways;
	public static GameObject[] mainBuildingNodes;
	public static GameObject[] mainHighwayNodes;
	public static GameObject panel = null;

	//création d'une instance de GestFile
	public GestFile f = new GestFile();

	//création d'une instance de ObjectBuilding
	ObjectBuilding ob = new ObjectBuilding();

	// Fonction lancée à l'initialisation de la scene
	void Start() {

		SetUpUI ();
		//Test si un nom de fichier est renseigner sur l'interface de Unity
		if(fileName != "null"){
			f.readFileOSM(fileName,0);
		}

		if(fileName2 != "null"){
			f.readFileOSM(fileName2,1);
		}
		// Test si aucun des nom des fichiers n'est egale a null
		if ((fileName != "null") || (fileName2 != "null")) {
			//Generation du SettingsFiles
			f.createSettingsFile ();
			//Lecture de SettingsFiles
			f.readSettingsFile ();


			classerlonlat ();
			//Creation du ResumeFile
			f.createResumeFile ();
			//Lecture du fichier ResumeFile precedement créé
			f.readResumeFile ();

			ob.setNodeGroups (nodeGroups);
			ob.setLatLong (minlat, maxlat, minlon, maxlon);

			//Contruction des noeuds
			ob.buildNodes ();
			//Contruction des murs
			ob.buildWalls ();
			//Contruction des toits
			ob.buildRoofs ();
			//Contruction des routes
			ob.buildHighways ();
			//Contruction des arbres
			ob.buildTrees ();
			//Contruction des feux tricolores
			ob.buildTrafficSignals ();
			//Mise en place de la camera
			ob.buildMainCameraBG ();
			//Mise en place du background
			ob.buildBackground ();

			// Mise en place de panneaux d'information
			// NE FOCTIONNE PAS POUR LE MOMMENT
			mainBuildingNodes = GameObject.FindGameObjectsWithTag ("BuildingNode");
			mainHighwayNodes = GameObject.FindGameObjectsWithTag ("HighwayNode");
			foreach (GameObject go in mainBuildingNodes) {
				go.SetActive (false);
			}
			foreach (GameObject go in mainHighwayNodes) {
				go.SetActive (false);
			}
		}

		Debug.Log (" ");

		// on recupere la reference du panneau et on le desactive
		panel = GameObject.Find ("Panneau");
		panel.SetActive(false);

	}

	/// <summary>
	/// Methode SetUpUI:
	/// mise en place de l'interface
	/// </summary>
	public void SetUpUI(){

		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("UIManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("myCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("myEventSystem"));

	}

	/// <summary>
	/// Methode classerlonlat:
	/// mise a jour des longitude max et min
	/// mise a jour des latitude max et min
	/// </summary>
	public void classerlonlat(){
		if ( ((minlat > minlat2) && (minlat2!=0)) || (minlat==0) ){
			minlat = minlat2;
		}

		if ( (maxlat < maxlat2) || (maxlat==0) ){
			maxlat = maxlat2;
		}

		if ( ((minlon > minlon2) && (minlon2!=0)) || (minlon==0) ){
			minlon = minlon2;
		}

		if ( (maxlon < maxlon2) || (maxlon==0) ){
			maxlon = maxlon2;
		}
	}
}
