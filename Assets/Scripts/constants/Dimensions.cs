﻿/// <summary>
///		Contient des dimensions de référence, comme la hauteur des étages des bâtiments.
/// </summary>
public class Dimensions {
	/// <summary>Facteur d'échelle pour agrandir différents éléments dans la vue 3D.</summary>
	public const float SCALE_FACTOR = 1000;

	public const float FLOOR_HEIGHT = 0.00006F * SCALE_FACTOR;

	public const float ROAD_WIDTH = 0.00006F * SCALE_FACTOR;

	public const float TRUNC_HEIGHT = 0.00012F * SCALE_FACTOR;
	public const float TRUNC_DIAMTETER = 0.00008F * SCALE_FACTOR;

	public const float BUILDING_DATA_CANVAS_LOW_HEIGHT = 50;
	public const float BUILDING_DATA_CANVAS_HIGH_HEIGHT = 200;
}
