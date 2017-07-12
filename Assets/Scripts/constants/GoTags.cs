/// <summary>
/// 	<para>
/// 		Contient les tags permettant d'identifier le types des objets 3D et des noeuds.
/// 	</para>
/// 	<para>
/// 		ATTENTION : ces chaines n'ont techniquement aucun lien avec les tags contenus dans les fichiers XML.
/// 	</para>
///</summary>
public class GoTags {
	/// <summary>Tag des noeuds représentant un noeud de bâtiment.</summary>
	public const string BUILDING_NODE_TAG = "BuildingNode";

	/// <summary>Tag des noeuds représentant un noeud de route.</summary>
	public const string HIGHWAY_NODE_TAG = "HighwayNode";

	/// <summary>Tag des noeuds représentant un mur.</summary>
	public const string WALL_TAG = "Wall";

	/// <summary>Tag des noeuds représentant un toît.</summary>
	public const string ROOF_TAG = "Roof";

	/// <summary>Tag des noeuds représentant une route.</summary>
	public const string HIGHWAY_TAG = "Highway";

	/// <summary>Tag des noeuds représentant un chemin piéton.</summary>
	public const string FOOTWAY_TAG = "Footway";

	/// <summary>Tag des noeuds représentant une piste cyclable.</summary>
	public const string CYCLEWAY_TAG = "Cycleway";

	/// <summary>Tag des noeuds représentant une voie de bus.</summary>
	public const string BUS_LANE_TAG = "BusLane";

	/// <summary>Tag des noeuds représentant un arbre.</summary>
	public const string TREE_TAG = "Tree";


	public const string DATA_CANVAS = "DataCanvas";

	//public const string HUMIDITY_INDICATOR = "HumidityIndicator";

	//public const string LUMINOSITY_INDICATOR = "LuminosityIndicator";

	//public const string CO2_INDICATOR = "Co2Indicator";

	//public const string PRESENCE_INDICATOR = "PresenceIndicator";

	//public const string UNKNOWN_INDICATOR = "UknownIndicator";
}