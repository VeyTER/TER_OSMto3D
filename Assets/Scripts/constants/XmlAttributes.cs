/// <summary>
/// 	<para>
/// 		Contient les noms d'attributs des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : Certains de ces attributs sont utilisés pour extraire des données des cartes OSM et ne doivent
/// 		donc pas être modifiés, sauf si Open Street Map change le nom des attributs.
/// 	</para>
///</summary>
public class XmlAttributes {
	/// <summary>
	/// 	<para>Nom de l'attribut donnant l'ID d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string ID = "id";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la référence d'un noeud "nd" de bâtiment ou route.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string REFERENCE = "ref";

	/// <summary>Nom de l'attribut donnant l'index d'un noeud "nd" de bâtiment ou route.</summary>
	public const string INDEX = "index";


	/// <summary>
	/// 	<para>Nom de l'attribut donnant la visibilité d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string VISIBILITY = "visible";


	/// <summary>
	/// 	<para>Nom de l'attribut donnant la clé de la propriété d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string PROPERTY_KEY = "k";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la valeur de la propriété d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string PROPERTY_VALUE = "v";


	/// <summary>
	/// 	<para>Nom de l'attribut donnant la latitude objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string LATITUDE = "lat";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la longitude objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string LONGIUDE = "lon";


	/// <summary>
	/// 	<para>Nom de l'attribut donnant la latitude minimale d'une carte.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MIN_LATITUDE = "minlat";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la longitude minimale d'une carte.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MIN_LONGITUDE = "minlon";


	/// <summary>
	/// 	<para>Nom de l'attribut donnant la latitude maximale d'une carte.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MAX_LATITUDE = "maxlat";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la longitude maximale d'une carte.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MAX_LONGITUDE = "maxlon";


	/// <summary>
	/// 	Nom de l'attribut donnant la distance d'un objet par rapport au centre de la zone dans laquelle il se
	/// 	trouve.
	///</summary>
	public const string DISTANCE = "dist";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant le nom d'un objet.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string NAME = "name";


	/// <summary>Nom de l'attribut donnant la désignation d'une zone (son nom).</summary>
	public const string DESIGNATION = "designation";

	/// <summary>Nom de l'attribut donnant le nombre d'étages d'un bâtiment.</summary>
	public const string NB_FLOOR = "nbfloor";

	/// <summary>Nom de l'attribut donnant l'angle de toît d'un bâtiment.</summary>
	public const string ROOF_ANGLE = "roofangle";

	/// <summary>Nom de l'attribut donnant le type du toît d'un bâtiment.</summary>
	public const string ROOF_TYPE = "rooftype";


	/// <summary>Nom de l'attribut donnant le type d'une route.</summary>
	public const string ROAD_TYPE = "roadtype";

	/// <summary>Nom de l'attribut donnant le nombre de voies d'une route.</summary>
	public const string NB_WAY = "nbway";

	/// <summary>
	/// 	<para>Nom de l'attribut donnant la vitesse maximale d'une route.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string MAX_SPEED = "maxspeed";

	public const string CUSTOM_MATERIAL = "custmat";
	public const string OVERLAY_COLOR = "color";
	public const string TOPIC = "topic";


	public const string THRESHOLD_CONDITION = "condition";
	public const string COMPONENT_PROPERTY_VALUE = "value";
}