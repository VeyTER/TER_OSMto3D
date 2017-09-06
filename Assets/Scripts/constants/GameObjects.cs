/// <summary>
/// 	Contient le chemin des objets 3D instanciables depuis l'arborescence du projet (prefabs).
/// </summary>
public class GameObjects {
	/// <summary>Chemin vers le gestionnaire d'interfaces graphiques.</summary>
	public const string UI_MANAGER_SCRIPT = FilePaths.GAME_OBJECT_LOCAL_FOLER + "UiManagerScript";

	/// <summary>Chemin vers la zone d'affichage du panneau d'édition.</summary>
	public const string EDIT_CANVAS = FilePaths.GAME_OBJECT_LOCAL_FOLER + "MainCanvas";

	/// <summary>Chemin vers le gestionnaire des évènements.</summary>
	public const string EVENT_SYSTEM = FilePaths.GAME_OBJECT_LOCAL_FOLER + "EventSystem";


	/// <summary>Chemin vers la zone d'affichage des données de dispositifs.</summary>
	public const string BUILDING_DATA_CANVAS = FilePaths.GAME_OBJECT_LOCAL_FOLER + "BuildingDataCanvas";

	/// <summary>Chemin vers la boîte affichant les données de dispositifs d'une certaine salle.</summary>
	public const string BUILDING_DATA_BOX = FilePaths.GAME_OBJECT_LOCAL_FOLER + "BuildingDataBox";

	/// <summary>Chemin vers la ligne affichant les données issues d'un capteur.</summary>
	public const string BUILDING_DATA_INDICATOR = FilePaths.GAME_OBJECT_LOCAL_FOLER + "SensorIndicator";

	/// <summary>Chemin vers le bloc de contrôle permettant en théorie d'agir sur un actionneur.</summary>
	public const string ACTUATOR_CONTROL = FilePaths.GAME_OBJECT_LOCAL_FOLER + "ActuatorControl";


	/// <summary>
	///		Chemin vers l'item permettant de sélectionner une texture dans la palette des textures.
	/// </summary>
	public const string MATERIAL_ITEM = FilePaths.GAME_OBJECT_LOCAL_FOLER + "MaterialItem";

	/// <summary>
	///		Chemin vers l'item permettant de sélectionner une couleur dans la palette des couleurs.
	///	</summary>
	public const string COLOR_ITEM = FilePaths.GAME_OBJECT_LOCAL_FOLER + "ColorItem";


	/// <summary>Chemin vers l'objet 3D représentant un feu tricolore.</summary>
	public const string TRAFFIC_SIGNAL = FilePaths.GAME_OBJECT_LOCAL_FOLER + "TrafficSignal";
}