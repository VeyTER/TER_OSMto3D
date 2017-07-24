/// <summary>
/// 	<para>
/// 		Contient les valeurs propriétés des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : les valeurs sont utilisés pour extraire des données des cartes OSM et ne doivent
/// 		donc pas être modifiés, sauf si Open Street Map change le nom des valeurs.
/// 	</para>
///</summary>
public class XmlValues {
	public const string THRESHOLD_EQUALS_CONDITION = "equals";
	public const string THRESHOLD_NOT_EQUALS_CONDITION = "not equals";

	public const string THRESHOLD_IN_CONDITION = "in";
	public const string THRESHOLD_OUT_CONDITION = "out";

	public const string TRAFFIC_SIGNALS = "traffic_signals";
	public const string TREE = "tree";
}
