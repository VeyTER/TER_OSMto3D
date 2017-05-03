using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Main : MonoBehaviour {
	// coordonnées min et max de la carte
	public static double minlat = 0, maxlat = 0, minlon = 0, maxlon = 0;
	public static double minlat2 = 0, maxlat2 = 0, minlon2 = 0, maxlon2 = 0;

	// Les fileName sont instancie via l'interfce de Unity
	private string OSMFileName1;
	private string OSMFileName2;

	private GameObject lateralPanel;

	// création d'une instance de GestFile
	private FileManager fileManager;

	// création d'une instance de ObjectBuilding
	private ObjectBuilder objectBuilder;

	// Fonction lancée à l'initialisation de la scene
	public void Start() {
		objectBuilder = ObjectBuilder.GetInstance();
		fileManager = new FileManager();

		this.SetUpUI ();

		OSMFileName1 = "capitole";
		OSMFileName2 = null;

		// Teste si un nom de fichier est renseigné sur l'interface de Unity
		if (OSMFileName1 != null)
			fileManager.readFileOSM (OSMFileName1, 0);

//		if (OSMFileName2 != "null") {
//			gf.readFileOSM (OSMFileName2, 1);
//		}

		// Test si aucun des nom des fichiers n'est egale a null
		if (OSMFileName1 != null || OSMFileName2 != null) {
			QualitySettings.antiAliasing = 8;

			// Generation du SettingsFiles
			fileManager.createSettingsFile ();

			// Lecture de SettingsFiles
			fileManager.readSettingsFile ();

			this.SortLonLat ();

			// Creation du ResumeFile
			fileManager.createResumeFile ();

			// Lecture du fichier ResumeFile précédement créé
			fileManager.readResumeFile ();

			objectBuilder.ScaleNodes (1000D);
			objectBuilder.SetLatLon (minlat, maxlat, minlon, maxlon);

			GameObject cityComponents = new GameObject (CityNames.CITY);
			objectBuilder.CityComponents = cityComponents;

			// Contruction des noeuds
			objectBuilder.BuildNodes ();

			// Contruction des murs
			objectBuilder.BuildWalls ();

			// Contruction des toits
			objectBuilder.BuildRoofs ();

			// Contruction des routes
			objectBuilder.BuildRoads ();

			// Contruction des arbres
			objectBuilder.BuildTrees ();

			// Contruction des feux tricolores
			objectBuilder.BuildTrafficSignals ();

			// Mise en place de la camera
			objectBuilder.BuildMainCameraBG ();

			// Mise en place du background
			objectBuilder.BuildGround ();


			// Mise en place de panneaux d'information
			// NE FOCTIONNE PAS POUR LE MOMMENT
			objectBuilder.BuildingNodes.SetActive (false);
			objectBuilder.HighwayNodes.SetActive (false);
			objectBuilder.Roofs.SetActive (false);
		}

		// on recupere la reference du panneau et on le desactive
		lateralPanel = GameObject.Find (UINames.LATERAL_PANEL);

		Vector3 panelPosition = lateralPanel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)lateralPanel.transform;
		lateralPanel.transform.localPosition = new Vector3 (panelPosition.x + panelRectTransform.rect.width, panelPosition.y, panelPosition.z);
	}

	// / <summary>
	// / Methode SetUpUI:
	// / mise en place de l'interface
	// / </summary>
	public void SetUpUI() {
		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("Game Objects/UIManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("Game Objects/MainCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("Game Objects/EventSystem"));
	}

	// / <summary>
	// / Methode classerlonlat:
	// / mise a jour des longitude max et min
	// / mise a jour des latitude max et min
	// / </summary>
	public void SortLonLat() {
		if ( ((minlat > minlat2) && (minlat2 != 0)) || (minlat == 0) )
			minlat = minlat2;

		if ( (maxlat < maxlat2) || (maxlat == 0) )
			maxlat = maxlat2;

		if ( ((minlon > minlon2) && (minlon2 != 0)) || (minlon == 0) )
			minlon = minlon2;

		if ( (maxlon < maxlon2) || (maxlon == 0) )
			maxlon = maxlon2;
	}
}
