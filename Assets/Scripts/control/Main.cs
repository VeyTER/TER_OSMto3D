using UnityEngine;
using System.IO;

/// <summary>
/// 	<para>
/// 		Class principale, considérée comme le point d'entrée du programme. Elle se charge de récupérer les données
/// 		OSM avant de lancer la construction de la ville et d'initialiser l'interface graphique.
/// 	</para>
/// 	<para>
/// 		Cette classe hérite de la classe MonoBehaviour. Comme toutes les autres classes qui héritent de
/// 		MonoBehaviour, elle peut être considérée comme un composant de GameObject. On ne peut pas mentionner de
/// 		GameObject dans son constructeur, on utilisera plutôt la méthode Start() ou Awake() pour initialiser
/// 		les objets.
/// 	</para>
/// </summary>
public class Main : MonoBehaviour {
	/// <summary>Facteur d'échelle pour agrandir différents éléments dans la vue 3D.</summary>
	public static double SCALE_FACTOR = 1000;

	/// <summary>Chemin vers le fichier OSM contenant les données de la ville.</summary>
	private static string OSM_FILE_NAME = FilePaths.MAPS_FOLDER + "capitole" + ".osm";


	/// <summary>Panneau latéral</summary>
	private GameObject lateralPanel;


	/// <summary>
	/// 	Undique isntance du singleton MapLoader servant à charger une carte OSM.
	/// </summary>
	private MapLoader mapLoader;

	/// <summary>
	/// 	Unique instance du singleton ObjectBuilder servant construire la ville en 3D à partir des données OSM.
	/// </summary>
	private ObjectBuilder objectBuilder;


	public void Start() {
		objectBuilder = ObjectBuilder.GetInstance();
		mapLoader = MapLoader.GetInstance ();

		// Paramétrage de la qualité graphique
		QualitySettings.antiAliasing = 8;
		QualitySettings.shadows = ShadowQuality.All;

		this.SetUpUi ();

		// Si le fichier contennant la carte OSM existe bien, le traitement est effectué
		if (File.Exists(OSM_FILE_NAME)) {
			// Chargement des données OSM
			mapLoader.LoadOsmData (OSM_FILE_NAME);
			mapLoader.LoadSettingsData ();
			mapLoader.GenerateResumeFile ();
			mapLoader.LoadCustomData ();
			mapLoader.LoadResumedData ();

			// Réglage de l'échelle et des dimensions
			objectBuilder.ScaleNodes (Main.SCALE_FACTOR);
			objectBuilder.SetBounds (mapLoader.Minlat, mapLoader.Minlon, mapLoader.Maxlat, mapLoader.Maxlon);

			// Construction de la ville
			objectBuilder.CityComponents = new GameObject (ObjectNames.CITY);
			objectBuilder.BuildNodes ();
			objectBuilder.BuildWalls ();
			objectBuilder.BuildRoofs ();
			objectBuilder.BuildRoads ();
			objectBuilder.BuildTrees ();
			objectBuilder.BuildTrafficLights ();
			objectBuilder.BuildMainCamera ();
			objectBuilder.BuildGround ();

			// Désactivation des certains groupes d'objets
			objectBuilder.BuildingNodes.SetActive (false);
			objectBuilder.HighwayNodes.SetActive (false);
			objectBuilder.Roofs.SetActive (false);

			// Récupération de la référence du panneau et désactivation de ce dernier
			lateralPanel = GameObject.Find (UiNames.LATERAL_PANEL);

			// Paramétrage du panneau latéral
			Vector3 panelPosition = lateralPanel.transform.localPosition;
			RectTransform panelRectTransform = (RectTransform)lateralPanel.transform;
			lateralPanel.transform.localPosition = new Vector3 (panelPosition.x + panelRectTransform.rect.width, panelPosition.y, panelPosition.z);
		}
	}

	/// <summary>
	/// 	Mise en place de l'interface
	/// </summary>
	public void SetUpUi() {
		GameObject uiManager = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/UiManagerScript"));
		GameObject canvas = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/MainCanvas"));
		GameObject eventSystem = (GameObject) GameObject.Instantiate (Resources.Load("Game objects/EventSystem"));
	}
}
