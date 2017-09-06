/// <summary>
///		Contient des dimensions de référence, comme la hauteur des étages des bâtiments.
/// </summary>
public class Dimensions {
	/// <summary>Facteur d'échelle permettant de d'aggrandir différents éléments dans la vue 3D.</summary>
	public const float SCALE_FACTOR = 1;

	/// <summary>
	///		Facteur d'échelle permettant de régler l'espacement entre les noeuds au sein de la scène 3D.
	/// </summary>
	public const float NODE_SCALE = 1000;

	/// <summary>Hauteur des étages de bâtiments.</summary>
	public const float FLOOR_HEIGHT = 0.06F * SCALE_FACTOR;

	/// <summary>Hauteur des toits pentus de bâtiments.</summary>
	public const float ROOF_HEIGHT = FLOOR_HEIGHT * 0.65F;

	/// <summary>
	///		Élevation des voies de circulation par rapport au sol, permet d'éviter des problèmes d'affichage
	/// </summary>
	public const float WAY_ELEVATION = 0.002F * SCALE_FACTOR;

	/// <summary>
	///		Élevation des voies de circulation par rapport aux voies de circulation, permet d'éviter des problèmes
	///		d'affichage.
	/// </summary>
	public const float ROAD_ELEVATION = WAY_ELEVATION + 0.001F * SCALE_FACTOR;


	/// <summary>Largeur des routes.</summary>
	public const float ROAD_WIDTH = 0.06F * SCALE_FACTOR;

	/// <summary>Largeur des voies de bus.</summary>
	public const float BUS_LANE_WIDTH = ROAD_WIDTH;

	/// <summary>Largeur des pistes cyclables.</summary>
	public const float CYCLEWAYS_WIDTH = ROAD_WIDTH * 0.5F;

	/// <summary>Largeur des chemins piétoniers.</summary>
	public const float FOOTWAY_WIDTH = ROAD_WIDTH * 0.5F;

	/// <summary>Largeur par défaut des cours d'eau.</summary>
	public const float WATERWAY_WIDTH = ROAD_WIDTH * 3;


	/// <summary>Hauteur des troncs d'arbres.</summary>
	public const float TRUNC_HEIGHT = 0.12F * SCALE_FACTOR;

	/// <summary>Diamètre des troncs d'arbres.</summary>
	public const float TRUNC_DIAMTETER = 0.08F * SCALE_FACTOR;


	/// <summary>
	///		Hauteur, en mode "rétracté", des panneaux d'affichage d'informations sur les bâtiments.
	/// </summary>
	public const float BUILDING_DATA_CANVAS_LOW_HEIGHT = 50;

	/// <summary>
	///		Hauteur, en mode "déployé", des panneaux d'affichage d'informations sur les bâtiments.
	/// </summary>
	public const float BUILDING_DATA_CANVAS_HIGH_HEIGHT = 200;
}
