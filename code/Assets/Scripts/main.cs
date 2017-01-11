using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class main : MonoBehaviour {

    // liste des nodes ( structure de donnée )
	public static ArrayList nodes = new ArrayList();
	// liste des groupes de nodes ( structure de donnée )
	public static ArrayList nodeGroups = new ArrayList ();
	// compteur de nodes et groupe de nodes respectivement
	private int counter;
	private int buildingCounter;
	// coordonnées min et max de la carte
	private float minlat, maxlat, minlon, maxlon;
    /*
    public static ArrayList nodes = new ArrayList();
    // liste des groupes de nodes ( structure de donnée )
    public static ArrayList nodeGroups = new ArrayList();

    // coordonnées min et max de la carte
    private float minlat, maxlat, minlon, maxlon;
    */
	// chemin d'acces et nom du fichier
	private string path = @"./Assets/";
	public string fileName = "map";
	// listes des gameObjects créés dans la scene
	public static GameObject[] mainWalls;
	public static GameObject[] mainNodes;
	public static GameObject panel = null;

    //test fonction de gestFile
    public GestFile f = new GestFile();
 
	// Fonction lancée à l'initialisation de la scene
	void Start () {

		SetUpUI ();
		f.readFileOSM(fileName);
		f.createResumeFile(fileName);
        // si la carte n'a pas de fichier de parametre on le créé
        /*if (!System.IO.File.Exists (path + "MapsSettings/" + fileName + "Settings.osm")) {
			f.createSettingsFile (fileName);	
		}
		else{
            f.readSettingsFile (fileName);
		}*/
        f.createSettingsFile(fileName);
        //test//

        f.readSettingsFile(fileName);
        
        foreach (NodeGroup ngp in nodeGroups)
        {

            UnityEngine.Debug.Log(ngp.getDistrict());
            UnityEngine.Debug.Log(ngp.getName());

        }
        /*
		buildNodes ();
		buildWalls ();
		// recommandé respecter un ration de interv/taille = 5 avec 0.01 0.002 si pas beaucoup de batiments
		buildRoofs (0.03f,0.006f);
		buildMainCameraBG ();

		// on recupere la reference du panneau et on le desactive
		panel = GameObject.Find ("Panneau");
		panel.SetActive(false);
        */
	}

	// place les nodes dans la scène
	public void buildNodes(){
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){
				// on construit les angles des buildings
				foreach(Node n in ngp.nodes){
					
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
					cube.transform.position = new Vector3(n.latitude,0, n.longitude);
					cube.name = ""+n.id;
					cube.tag = "Node";
				}
			}	
		}
	}

	// place les murs dans la scène
	public void buildWalls(){
		foreach (NodeGroup ngp in nodeGroups) {
			if(ngp.isBuilding()){


				if(!ngp.decomposerRight()){
					ngp.decomposerLeft();
				}
				/*
				// affichage de la decomposition
				int a = 0;
				foreach(NodeGroup ngpb in ngp.decomposition){
					Debug.Log("decomposition " + a);
					a++;
					foreach (Node n in ngpb.nodes){
						Debug.Log(n.toString());
					}
				}*/
    

				//On créé les murs
				float x, y, length,adj,angle;
				int etages = 5;

				for(int i=0;i<ngp.nodes.Count-1;i++){
					//on recup les coordonées utiles
					x = (ngp.getNode(i).latitude + ngp.getNode(i+1).latitude)/2;
					y = (ngp.getNode(i).longitude + ngp.getNode(i+1).longitude)/2;
					length = Mathf.Sqrt(Mathf.Pow(ngp.getNode(i+1).latitude - ngp.getNode(i).latitude,2) + Mathf.Pow(ngp.getNode(i+1).longitude - ngp.getNode(i).longitude,2));
					adj = Mathf.Abs(ngp.getNode(i+1).longitude - ngp.getNode(i).longitude);
					angle = (Mathf.Acos(adj/length)*180f/Mathf.PI);
					
					//on positionne le mur
					GameObject mur = GameObject.CreatePrimitive(PrimitiveType.Cube);
					mur.tag = "Wall";
					if(ngp.tags.ContainsKey("etages")){
						etages = int.Parse(ngp.GetTagValue("etages"));
					}
					else{
						etages = 1;
					}
					mur.transform.localScale = new Vector3(length+0.015f,0.1f*etages,0.02f);
					mur.transform.position = new Vector3(x,-0.05f*etages, y);
					mur.AddComponent<GetInfos>();
					
					// on modifie l'angle en fonction de l'ordre des points
					
					if((ngp.getNode(i).latitude > ngp.getNode(i+1).latitude && ngp.getNode(i).longitude < ngp.getNode(i+1).longitude) 
					   || (ngp.getNode(i).latitude < ngp.getNode(i+1).latitude && ngp.getNode(i).longitude > ngp.getNode(i+1).longitude)){
						mur.transform.localEulerAngles = new Vector3(0,90-angle,0);
					}
					else {
						mur.transform.localEulerAngles = new Vector3(0,angle+90,0);
					}

					mur.name = ngp.id + "_" + ngp.GetTagValue("name") +"_Mur"+ i;
				}
			}	
		}
	}

	// place la caméra et le background dans la scene
	public void buildMainCameraBG(){

		float CamLat, CamLon;

		// On centre la camera 
		CamLat = (minlat + maxlat) / 2;
		CamLon = (minlon + maxlon) / 2;
		GameObject mainCam = new GameObject ();
		mainCam.AddComponent <Camera>();
		Light mainLight = mainCam.AddComponent<Light>();
		mainLight.range = 30;
		mainLight.intensity = 0.5f;
		mainCam.name = "MainCam";
		mainCam.transform.position = new Vector3(CamLat,-5, CamLon);
		mainCam.transform.localEulerAngles = new Vector3(-90,270,0);
		mainCam.AddComponent <CameraController>();

		GameObject background = GameObject.CreatePrimitive(PrimitiveType.Plane);
		background.name = "Background";
		background.transform.position = new Vector3(CamLat,0.5f, CamLon);
		background.transform.localScale = new Vector3(10,10,10);
		background.transform.localEulerAngles = new Vector3(0,0,180);
		background.GetComponent<Renderer>().material.mainTexture = Resources.Load ("bg") as Texture;
		background.GetComponent<Renderer>().material.name = "Texture_Background";
		background.GetComponent<Renderer>().material.color = Color.gray;
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (500,500);
	}

	//  coustruction des toits
	public void buildRoofs(float interv, float taille){

		Node n = new Node (0, 0);
		float x, y;
		foreach (NodeGroup ng in nodeGroups) {
			if(ng.isBuilding()){
				ng.setBoundaries();
				for(x = ng.minLat; x < ng.maxLat; x += interv){
					for(y = ng.minLon; y < ng.maxLon; y += interv){

						n.latitude = x;
						n.longitude = y;

						if(ng.appartient(n)){
							int etages = int.Parse(ng.GetTagValue("etages"));
							GameObject toit = GameObject.CreatePrimitive(PrimitiveType.Plane);
							toit.transform.position = new Vector3(x,-0.1f*etages,y);
							toit.transform.localScale = new Vector3(taille,taille,taille);
							toit.transform.localEulerAngles = new Vector3(0,0,180);
						}
					}
				}
			}
		}
	}

	// mise en place de l'interface
	public void SetUpUI(){

		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("UIManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("myCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("myEventSystem"));
 
	}
}
