using System;

///<summary>
/// 	Contient tous chemins vers les matérieux utilisés dans le projet. S'il doivent être changés de dossier, il
/// 	suffit ce changer le chemin stocké dans la première variable.
///</summary>
public class Materials {
	///<summary>Chemon vers le dossier des matériaux.</summary>
	public const string MATERIALS_FOLDER = "Materials/";

	///<summary>Chemin vers le matériau des murs.</summary>
//	public const string WALL = MATERIALS_FOLDER + "Wall";
	public const string WALL = MATERIALS_FOLDER + "Bricks";

	///<summary>Chemin vers le matériau des toits.</summary>
	public const string ROOF = MATERIALS_FOLDER + "Roof";

	///<summary>Chemin vers le matériau des routes.</summary>
	public const string ROAD = MATERIALS_FOLDER + "Road";

	///<summary>Chemin vers le matériau des feux tricolores.</summary>
	public const string TRAFFIC_LIGHT = MATERIALS_FOLDER + "TrafficLight";

	///<summary>Chemin vers le matériau des voies de bus.</summary>
	public const string BUSWAY = MATERIALS_FOLDER + "Busway";

	///<summary>Chemin vers le matériau des pistes cyclables.</summary>
	public const string CYCLEWAY = MATERIALS_FOLDER + "Cycleway";

	///<summary>Chemin vers le matériau des chemins piétons.</summary>
	public const string FOOTWAY = MATERIALS_FOLDER + "Footway";

	///<summary>Chemin vers le matériau des voies maritimes.</summary>
	public const string WATERWAY = MATERIALS_FOLDER + "Waterway";

	///<summary>Chemin vers le matériau des troncs d'arbre.</summary>
	public const string TREE_TRUNK = MATERIALS_FOLDER + "TreeTrunk";

	///<summary>Chemin vers le matériau des feuilles.</summary>
	public const string TREE_LEAF = MATERIALS_FOLDER + "TreeLeaf";

	///<summary>Chemin vers le matériau du sol.</summary>
	public const string GROUND = MATERIALS_FOLDER + "Ground";

	///<summary>Chemin vers le matériau métal.</summary>
	public const string METAL = MATERIALS_FOLDER + "Metal";

	///<summary>Chemin vers le matériau des éléments sélectionnés.</summary>
	public const string SELECTED_ELEMENT = MATERIALS_FOLDER + "SelectedElement";
}

