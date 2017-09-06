/// <summary>
/// 	<para>
/// 		Contient les valeurs propriétés des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : les valeurs sont utilisées pour extraire des données des cartes OSM et ne doivent
/// 		donc pas être modifiées, sauf si OpenStreetMap change le nom des valeurs.
/// 	</para>
/// </summary>
public class XmlValues {
	/// <summary>Mode de comparaison par égalité.</summary>
	public const string THRESHOLD_EQUALS_CONDITION = "equals";

	/// <summary>Mode de comparaison par inégalité.</summary>
	public const string THRESHOLD_NOT_EQUALS_CONDITION = "not equals";


	/// <summary>Mode de comparaison par inclusion.</summary>
	public const string THRESHOLD_IN_CONDITION = "in";

	/// <summary>Mode de comparaison par exclusion.</summary>
	public const string THRESHOLD_OUT_CONDITION = "out";


	/// <summary>Feux de signalisation.</summary>
	public const string TRAFFIC_SIGNALS = "traffic_signals";

	/// <summary>Arbres.</summary>
	public const string TREE = "tree";
}
