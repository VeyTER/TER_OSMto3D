/// <summary>
/// 	<para>
/// 		Contient les noms de clés des propriétés des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : les clés sont utilisés pour extraire des données des cartes OSM et ne doivent
/// 		donc pas être modifiés, sauf si Open Street Map change le nom des clés.
/// 	</para>
///</summary>
public class XmlKeys {
	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant le nom d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string NAME = "name";

	public const string BUILDING = "building";

	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant le type d'une route.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string HIGHWAY = "highway";

	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant le type d'une route.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string WATERWAY = "waterway";

	public const string LANES = "lanes";

	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant la limitation de vitesse d'une route.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MAX_SPEED = "maxspeed";


	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant le type de végétal d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string NATURAL = "natural";

	/// <summary>
	/// 	<para>Nom de la clé de la propriété donnant le type de toît d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string ROOF_SHAPE = "roof:shape";
}
