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
	/// <summary>Chemin vers le fichier OSM contenant les données de la ville.</summary>
	private static string OSM_FILE_NAME = FilePaths.MAPS_FOLDER + MapNames.CAMPUS + ".osm";

	public void Start() {
		CityBuilder cityBuilder = CityBuilder.GetInstance();
		MapLoader mapLoader = MapLoader.GetInstance ();

		VisibilityController visibilityController = VisibilityController.GetInstance();

		// Paramétrage de la qualité graphique
		QualitySettings.antiAliasing = 8;
		QualitySettings.shadows = ShadowQuality.All;

		this.InstantiateMainObjects ();

		// Si le fichier contennant la carte OSM existe bien, le traitement est effectué
		if (File.Exists(OSM_FILE_NAME)) {
			// Chargement des données OSM
			mapLoader.LoadOsmData(OSM_FILE_NAME);
			mapLoader.LoadSettingsData();
			mapLoader.GenerateResumeFile();
			mapLoader.LoadCustomData();
			mapLoader.LoadResumedData();

			// Réglage de l'échelle et des dimensions
			cityBuilder.NodeGroupBase.ScaleNodes(Dimensions.SCALE_FACTOR);
			cityBuilder.SetBounds(mapLoader.Minlat, mapLoader.Minlon, mapLoader.Maxlat, mapLoader.Maxlon);

			// Construction de la ville
			cityBuilder.CityComponents = new GameObject(CityObjectNames.CITY);
			cityBuilder.BuildBuildings();
			cityBuilder.BuildHighways();
			cityBuilder.BuildTrees();
			cityBuilder.BuildTrafficSignals();
			cityBuilder.BuildMainCamera();
			cityBuilder.BuildGround(/*"CaptitoleBackground"*/);

			GameObject visibilityWheelPanel = GameObject.Find(UiNames.VISIBILITY_WHEEL_PANEL);
			WheelPanelController visibilityPanelContoller = visibilityWheelPanel.GetComponent<WheelPanelController>();
			visibilityPanelContoller.StartPosition = visibilityWheelPanel.transform.parent.localPosition;
			visibilityPanelContoller.EndPosition = new Vector3(110, visibilityWheelPanel.transform.parent.localPosition.y, 0);

			GameObject buildingCreationBoxPanel = GameObject.Find(UiNames.BUILDING_CREATION_BOX_PANEL);
			GameObject buildingCreationButtonIcon = buildingCreationBoxPanel.transform.parent.Find(UiNames.CREATE_BUILDING_ICON).gameObject;
			BoxPanelController buildingCreationPanelContoller = buildingCreationBoxPanel.GetComponent<BoxPanelController>();
			buildingCreationPanelContoller.StartPosition = buildingCreationButtonIcon.transform.localPosition;
			buildingCreationPanelContoller.EndPosition = new Vector3(58.7F, buildingCreationButtonIcon.transform.localPosition.y, 0);

			ControlPanelManager controlPanelManager = ControlPanelManager.GetInstance();
			controlPanelManager.AddPanel(visibilityWheelPanel);
			controlPanelManager.AddPanel(buildingCreationBoxPanel);

			// Désactivation des certains groupes d'objets
			visibilityController.HideBuildingNodes();
			visibilityPanelContoller.DisableButton(GameObject.Find(UiNames.BUILDING_NODES_SWITCH));

			visibilityController.HideRoadsNodes();
			visibilityPanelContoller.DisableButton(GameObject.Find(UiNames.ROADS_NODES_SWITCH));

			// Récupération de la référence du panneau et ajout d'un controlleur
			GameObject editPanel = GameObject.Find(UiNames.EDIT_PANEL);
			editPanel.AddComponent<EditPanelController>();

			// Paramétrage du panneau latéral
			Vector3 panelPosition = editPanel.transform.localPosition;
			RectTransform panelRect = (RectTransform) editPanel.transform;
			editPanel.transform.localPosition = new Vector3(panelPosition.x + panelRect.rect.width, panelPosition.y, panelPosition.z);

			Material greenOverlay = Resources.Load(Materials.GREEN_OVERLAY) as Material;
			Material redOverlay = Resources.Load(Materials.RED_OVERLAY) as Material;

			greenOverlay.SetColor("_EmissionColor", ThemeColors.GREEN);
			redOverlay.SetColor("_EmissionColor", ThemeColors.RED);
		}
	}

	/// <summary>
	/// 	Mise en place de l'interface
	/// </summary>
	public void InstantiateMainObjects() {
		GameObject mainCamera = (GameObject) GameObject.Instantiate(Resources.Load("Game objects/CampusCamera"));
		mainCamera.name = "Camera";

		GameObject.Instantiate (Resources.Load(GameObjects.EDIT_CANVAS));
		GameObject.Instantiate (Resources.Load(GameObjects.UI_MANAGER_SCRIPT));
		GameObject.Instantiate (Resources.Load(GameObjects.EVENT_SYSTEM));

		GameObject.Instantiate(Resources.Load("Game objects/Sun"));
	}
}
