using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Main : MonoBehaviour {
	public static double SCALE_FACTOR = 1000;

	// Les fileName sont instancie via l'interfce de Unity
	private string osmFileName;

	private GameObject lateralPanel;

	// création d'une instance de GestFile
	private MapLoader mapLoader;

	// création d'une instance de ObjectBuilding
	private ObjectBuilder objectBuilder;

	// Fonction lancée à l'initialisation de la scene
	public void Start() {
		objectBuilder = ObjectBuilder.GetInstance();
		mapLoader = new MapLoader();

		this.SetUpUi ();

		osmFileName = "campus";

		// Teste si un nom de fichier est renseigné sur l'interface de Unity
		if (osmFileName != null)
			mapLoader.LoadOsmData (osmFileName, 0);

		// Test si aucun des nom des fichiers n'est egale a null
		if (osmFileName != null) {
			QualitySettings.antiAliasing = 8;

			// Lecture de SettingsFiles
			mapLoader.LoadSettingsData ();

			// Creation du ResumeFile
			mapLoader.GenerateResumeFile ();

			// Lecture du fichier CustomFile avec remplacement des données correspondantes dans le ResumedFile
			mapLoader.LoadCustomData ();

			// Lecture du fichier ResumeFile précédement créé
			mapLoader.LoadResumedData ();

			objectBuilder.ScaleNodes (Main.SCALE_FACTOR);
			objectBuilder.SetLatLon (mapLoader.Minlat, mapLoader.Minlon, mapLoader.Maxlat, mapLoader.Maxlon);

			GameObject cityComponents = new GameObject (ObjectNames.CITY);
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
		lateralPanel = GameObject.Find (UiNames.LATERAL_PANEL);

		Vector3 panelPosition = lateralPanel.transform.localPosition;
		RectTransform panelRectTransform = (RectTransform)lateralPanel.transform;
		lateralPanel.transform.localPosition = new Vector3 (panelPosition.x + panelRectTransform.rect.width, panelPosition.y, panelPosition.z);
	}

	// / <summary>
	// / Methode SetUpUI:
	// / mise en place de l'interface
	// / </summary>
	public void SetUpUi() {
		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/UiManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/MainCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/EventSystem"));
	}
}
