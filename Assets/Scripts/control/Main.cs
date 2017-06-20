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
	private static string OSM_FILE_NAME = FilePaths.MAPS_FOLDER + MapNames.CAMPUS + ".osm";

	private GameObject editPanel;


	/// <summary>
	/// 	Undique instance du singleton MapLoader servant à charger une carte OSM.
	/// </summary>
	private MapLoader mapLoader;

	/// <summary>
	/// 	Unique instance du singleton CityBuilder servant construire la ville en 3D à partir des données OSM.
	/// </summary>
	private CityBuilder cityBuilder;


	public void Start() {
		this.cityBuilder = CityBuilder.GetInstance();
		this.mapLoader = MapLoader.GetInstance ();

		// Paramétrage de la qualité graphique
		QualitySettings.antiAliasing = 8;
		QualitySettings.shadows = ShadowQuality.All;

		this.SetUpUi ();

		// Si le fichier contennant la carte OSM existe bien, le traitement est effectué
		if (File.Exists(OSM_FILE_NAME)) {
			// Chargement des données OSM
			this.mapLoader.LoadOsmData(OSM_FILE_NAME);
			this.mapLoader.LoadSettingsData();
			this.mapLoader.GenerateResumeFile();
			this.mapLoader.LoadCustomData();
			this.mapLoader.LoadResumedData();

			// Réglage de l'échelle et des dimensions
			this.cityBuilder.ScaleNodes(Main.SCALE_FACTOR);
			this.cityBuilder.SetBounds(mapLoader.Minlat, mapLoader.Minlon, mapLoader.Maxlat, mapLoader.Maxlon);

			// Construction de la ville
			this.cityBuilder.CityComponents = new GameObject(ObjectNames.CITY);
			this.cityBuilder.BuildNodes();
			this.cityBuilder.BuildWalls();
			this.cityBuilder.BuildRoofs();
			this.cityBuilder.BuildRoads();
			this.cityBuilder.BuildTrees();
			this.cityBuilder.BuildTrafficLights();
			this.cityBuilder.BuildMainCamera();
			this.cityBuilder.BuildGround(/*"CaptitoleBackground"*/);

			GameObject visibilityWheelPanel = GameObject.Find(UiNames.VISIBILITY_WHEEL_PANEL);
			WheelPanelController visibilityPanelContoller = visibilityWheelPanel.GetComponent<WheelPanelController>();
			visibilityPanelContoller.StartPosition = visibilityWheelPanel.transform.parent.localPosition;
			visibilityPanelContoller.EndPosition = new Vector3(110, visibilityWheelPanel.transform.parent.localPosition.y, 0);

			GameObject buildingCreationBoxPanel = GameObject.Find(UiNames.BUILDING_CREATION_BOX_PANEL);
			BoxPanelController buildingCreationPanelContoller = buildingCreationBoxPanel.GetComponent<BoxPanelController>();
			buildingCreationPanelContoller.StartPosition = buildingCreationBoxPanel.transform.parent.GetChild(0).localPosition;
			buildingCreationPanelContoller.EndPosition = new Vector3(58.7F, buildingCreationBoxPanel.transform.parent.GetChild(0).localPosition.y, 0);

			ControlPanelManager controlPanelManager = ControlPanelManager.GetInstance();
			controlPanelManager.AddPanel(visibilityWheelPanel);
			controlPanelManager.AddPanel(buildingCreationBoxPanel);

			// Désactivation des certains groupes d'objets
			this.cityBuilder.BuildingNodes.SetActive(false);
			visibilityPanelContoller.DisableButton(GameObject.Find(UiNames.BUILDING_NODES_SWITCH));

			this.cityBuilder.HighwayNodes.SetActive(false);
			visibilityPanelContoller.DisableButton(GameObject.Find(UiNames.HIGHWAY_NODES_SWITCH));

			this.cityBuilder.Roofs.SetActive(false);
			visibilityPanelContoller.DisableButton(GameObject.Find(UiNames.ROOFS_SWITCH));

			// Récupération de la référence du panneau et ajout d'un controlleur
			this.editPanel = GameObject.Find(UiNames.EDIT_PANEL);
			this.editPanel.AddComponent<EditPanelController>();

			// Paramétrage du panneau latéral
			Vector3 panelPosition = editPanel.transform.localPosition;
			RectTransform panelRectTransform = (RectTransform) editPanel.transform;
			this.editPanel.transform.localPosition = new Vector3(panelPosition.x + panelRectTransform.rect.width, panelPosition.y, panelPosition.z);

			Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
			Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

			greenOverlay.SetColor("_EmissionColor", ThemeColors.GREEN);
			redOverlay.SetColor("_EmissionColor", ThemeColors.RED);
		}
	}

	/// <summary>
	/// 	Mise en place de l'interface
	/// </summary>
	public void SetUpUi() {
		GameObject mainCamera = (GameObject) GameObject.Instantiate(Resources.Load("Game objects/Camera Campus"));
		mainCamera.name = "Camera";

		GameObject.Instantiate (Resources.Load("Game objects/UiManagerScript"));
		GameObject.Instantiate (Resources.Load("Game objects/MainCanvas"));
		GameObject.Instantiate (Resources.Load("Game objects/EventSystem"));
	}
}
