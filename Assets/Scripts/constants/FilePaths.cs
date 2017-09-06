/// <summary>
/// 	Contient tous chemins vers les fichiers et dossiers et fichiers du projet.
/// </summary>
public class FilePaths {
	/// <summary>Chemin vers le dossier des supports de l'application.</summary>
	public const string ASSETS_FOLDER = @"./Assets/";

	/// <summary>Chemin vers le dossier des objets 3D externes bruts.</summary>
	public const string OBJECTS_3D_FOLDER = ASSETS_FOLDER + "3D Objects/";

	/// <summary>Chemin vers le dossier des ressources.</summary>
	public const string RESOURCES_FOLDER = ASSETS_FOLDER + "Resources/";

	/// <summary>Chemin vers le dossier des textures.</summary>
	public const string GAME_OBJECT_LOCAL_FOLER = "Game objects/";


	/// <summary>
	///		Chemin, depuis le dossier des ressources, vers le dossier des objets 3D externes Blender.
	/// </summary>
	public const string EXTERNAL_OBJECTS_FOLDER_LOCAL = "External objects/";

	/// <summary>Chemin vers le dossier des objets 3D externes Blender.</summary>
	public const string EXTERNAL_OBJECTS_FOLDER = RESOURCES_FOLDER + "External objects/";


	/// <summary>Chemin vers le dossier des fichiers en lien avec OSM.</summary>
	public const string OSM_FOLDER = ASSETS_FOLDER + "OSM data/";

	/// <summary>Chemin vers les cartes OSM disponibles.</summary>
	public const string MAPS_FOLDER = OSM_FOLDER + "Maps/";

	/// <summary>Chemin vers les fichiers de paramètres par zones.</summary>
	public const string MAPS_SETTINGS_FOLDER = OSM_FOLDER + "Maps settings/";

	/// <summary>Chemin vers les fichiers résumant une ville.</summary>
	public const string MAPS_RESUMED_FOLDER = OSM_FOLDER + "Maps resumed/";

	/// <summary>Chemin vers les fichiers de personnalisation les objets.</summary>
	public const string MAPS_CUSTOM_FOLDER = OSM_FOLDER + "Maps custom/";


	/// <summary>Chemin vers le dossier contenant les données utiles aux scripts.</summary>
	public const string SCRIPT_DATA_FOLDER = ASSETS_FOLDER + "Scripts data/";

	/// <summary>Chemin vers le fichier contenant les propriétés physiques des différents composants.</summary>
	public const string COMPONENTS_PROPERTIES_FILE = SCRIPT_DATA_FOLDER + "devices_properties.xml";

	/// <summary>Chemin vers le fichier contenant les identifiants d'accès à l'API neOCampus.</summary>
	public const string API_LOGIN_FILE = SCRIPT_DATA_FOLDER + "api_login.txt";

	/// <summary>Chemin vers le fichier listant les bâtiments équipés de capteurs.</summary>
	public const string SENSOR_EQUIPPED_BUILDINGS_FILE = SCRIPT_DATA_FOLDER + "sensors_equipped_buildings.txt";


	/// <summary>Chemin, depuis le dossier des ressources, vers le dossier des matériaux.</summary>
	public const string MATERIALS_FOLDER_LOCAL = "Materials/";

	/// <summary>Chemin vers le dossier des matériaux.</summary>
	public const string MATERIALS_FOLDER = RESOURCES_FOLDER + "Materials/";


	/// <summary>Chemin, depuis le dossier des ressources, vers le dossier des images statiques.</summary>
	public const string STATIC_SPRITES_FOLDER_LOCAL = "Static sprites/";

	/// <summary>Chemin, depuis le dossier des ressources, vers le dossier des images animées.</summary>
	public const string ANIMATED_SPRITES_FOLDER_LOCAL = "Animated sprites/";


	/// <summary>Chemin vers le dossier des icônes.</summary>
	public const string ICONS_FOLDER_LOCAL = "Icons/";


	/// <summary>Chemin, depuis le dossier des ressources, vers le dossier des textures.</summary>
	public const string TEXTURES_FOLDER_LOCAL = "Textures/";

	/// <summary>Chemin vers le dossier des textures.</summary>
	public const string TEXTURES_FOLDER = RESOURCES_FOLDER + "Textures/";


	/// <summary>Chemin vers le dossier métadonnées concernant divers éléments de l'application.</summary>
	public const string METADATA_FOLDER = RESOURCES_FOLDER + "Meta data/";

	/// <summary>
	///		Chemin vers le fichier contenant les propriétés des objets externes (position, orientation etc...).
	/// </summary>
	public const string EXTERNAL_OBJECTS_FILE = METADATA_FOLDER + "objects_properties.txt";

	/// <summary>
	///		Chemin vers le fichier contenant des informatiosn supplémentaires sur les matériaux applicables aux
	///		bâtiments. Il permet de faire le lien entre une texture, le matériaux sur lequel est appliquée la
	///		texture et le nom affiché de la texture dans la palette.
	/// </summary>
	public const string MATERIAL_DETAILS_FILE = METADATA_FOLDER + "materials_details.txt";

	/// <summary>
	///		Chemin vers le fichier listant les captures d'écran de cartes à afficher sous la ville ainsi que leurs
	///		coordonnées.
	/// </summary>
	public const string MAP_BACKGROUNDS_FILE = METADATA_FOLDER + "maps_background_details.txt";


	/// <summary>
	///		Chemin vers le dossier contenant toutes les données utilisées par l'IA des capteurs et actionneurs.
	/// </summary>
	public const string AI_DATA_FOLDER = ASSETS_FOLDER + "Ai data/";

	/// <summary>
	///		Chemin vers le dossier contenant les tables d'apprentissage de l'IA des capteurs et actionneurs.
	/// </summary>
	public const string GRADIENTS_FOLDER = AI_DATA_FOLDER + "gradients/";
}