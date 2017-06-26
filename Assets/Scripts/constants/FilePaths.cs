using System;

///<summary>
/// 	Contient tous chemins vers certains fichiers du projet.
///</summary>
public class FilePaths {
	///<summary>Chemin vers le dossier des supports.</summary>
	public const string ASSETS_FOLDER = @"./Assets/";

	public const string RESOURCES_FOLDER = ASSETS_FOLDER + "Resources/";

	public const string OBJECTS_3D_FOLDER = ASSETS_FOLDER + "3D Objects/";

	public const string EXTERNAL_OBJECTS_FOLDER = RESOURCES_FOLDER + "External objects/";

	public const string TEXTURES_FOLDER = RESOURCES_FOLDER + "Textures/";

	///<summary>Chemin vers le dossier des fichiers en lien avec OSM.</summary>
	public const string OSM_FOLDER = ASSETS_FOLDER + "OSM data/";

	///<summary>Chemin vers les cartes OSM disponibles.</summary>
	public const string MAPS_FOLDER = OSM_FOLDER + "Maps/";

	///<summary>Chemin vers les fichiers de paramètres par zones.</summary>
	public const string MAPS_SETTINGS_FOLDER = OSM_FOLDER + "Maps settings/";

	///<summary>Chemin vers les fichier résumant une ville.</summary>
	public const string MAPS_RESUMED_FOLDER = OSM_FOLDER + "Maps resumed/";

	///<summary>Chemin vers les fichiers personnalisant les objets.</summary>
	public const string MAPS_CUSTOM_FOLDER = OSM_FOLDER + "Maps custom/";


	///<summary>Chemon vers le dossier des matériaux.</summary>
	public const string MATERIALS_FOLDER_LOCAL = "Materials/";

	public const string MATERIALS_FOLDER = RESOURCES_FOLDER + "Materials/";

	public const string MATERIAL_DETAILS_FILE = MATERIALS_FOLDER + "materials_details.txt";


	public const string ICONS_FOLDER_LOCAL = "Icons/";

	public const string EXTERNAL_OBJECTS_FOLDER_LOCAL = "External objects/";

	public const string EXTERNAL_OBJECTS_FILE = EXTERNAL_OBJECTS_FOLDER + "objects_properties.txt";


	public const string SPRITES_FOLDER_LOCAL = "Sprites/";


	public const string TEXTURES_FOLDER_LOCAL = "Textures/";

	public const string MAP_BACKGROUNDS_FILE = FilePaths.TEXTURES_FOLDER + "maps_background_details.txt";
}
