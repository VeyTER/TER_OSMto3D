/// <summary>
///		Contient des dimensions de référence, comme la hauteur des étages des bâtiments.
/// </summary>
public class Dimensions {
	/// <summary>Facteur d'échelle pour agrandir différents éléments dans la vue 3D.</summary>
	public const float SCALE_FACTOR = 1;
	public const float NODE_SCALE = 1000;

	public const float FLOOR_HEIGHT = 0.06F * SCALE_FACTOR;
	public const float ROOF_HEIGHT = FLOOR_HEIGHT * 0.65F;

	public const float WAY_ELEVATION = 0.002F * SCALE_FACTOR;

	public const float ROAD_WIDTH = 0.06F * SCALE_FACTOR;
	public const float BUS_LANE_WIDTH = ROAD_WIDTH;
	public const float CYCLEWAYS_WIDTH = ROAD_WIDTH * 0.5F;
	public const float FOOTWAY_WIDTH = ROAD_WIDTH * 0.5F;
	public const float WTERWAY_WIDTH = ROAD_WIDTH * 3;

	public const float TRUNC_HEIGHT = 0.12F * SCALE_FACTOR;
	public const float TRUNC_DIAMTETER = 0.08F * SCALE_FACTOR;

	public const float BUILDING_DATA_CANVAS_LOW_HEIGHT = 50;
	public const float BUILDING_DATA_CANVAS_HIGH_HEIGHT = 200;
}
