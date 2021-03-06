﻿/// <summary>
/// 	<para>
/// 		Contient les noms de balises des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : Certaines de ces balises sont utilisées pour extraire des données des cartes OSM et ne 
/// 		doivent	donc pas être modifiées, sauf si OpenStreetMap change le nom des balises.
/// 	</para>
/// </summary>
public class XmlTags {
	/// <summary>
	/// 	<para>Nom de la balise de description des fichiers XML.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string XML = "xml"; 


	/// <summary>
	/// 	<para>Nom de la balise donnant l'étendue de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string BOUNDS = "bounds";


	/// <summary>
	/// 	<para>Nom de la balise symbolisant un objet fermé (type bâtiment) de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string NODE = "node";

	/// <summary>
	/// 	<para>Nom de la balise symbolisant un objet composite de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string WAY = "way";


	/// <summary>
	/// 	<para>Nom de la balise donnant une propriété d'un objet de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string TAG = "tag";

	/// <summary>
	/// 	<para>Nom de la balise symbolisant une étape de construction d'un objet de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	/// </summary>
	public const string ND = "nd";


	/// <summary>Nom de la balise symbolisant la planète courante.</summary>
	public const string EARTH = "earth";

	/// <summary>Nom de la balise symbolisant le pays courant.</summary>
	public const string COUNTRY = "country";

	/// <summary>Nom de la balise symbolisant la région courante.</summary>
	public const string REGION = "region";

	/// <summary>Nom de la balise symbolisant la ville courante.</summary>
	public const string TOWN = "town";

	/// <summary>Nom de la balise symbolisant le district courant.</summary>
	public const string DISTRICT = "district";


	/// <summary>Nom de la balise symbolisant le bâtiment courant.</summary>
	public const string BUILDING = "building";

	/// <summary>Nom de la balise symbolisant l'arbre courant.</summary>
	public const string TREE = "tree";

	/// <summary>Nom de la balise symbolisant le feux de croisement courant.</summary>
	public const string TRAFFIC_SIGNALS = "traffic_signals";

	/// <summary>Nom de la balise symbolisant la route courante.</summary>
	public const string HIGHWAY = "highway";

	/// <summary>Nom de la balise symbolisant la voie maritime courante.</summary>
	public const string WATERWAY = "waterway";

	/// <summary>Nom de la balise symbolisant le lieu de loisir.</summary>
	public const string LEISURE = "leisure";

	/// <summary>Nom de la balise symbolisant le Stadium.</summary>
	public const string STADIUM = "stadium";

	/// <summary>Nom de la balise symbolisant la voie de bus courante.</summary>
	public const string BUS_LANE_WAY = "buswaylane";

	/// <summary>Nom de la balise symbolisant le chemin piéton courant.</summary>
	public const string FOOTWAY = "footway";

	/// <summary>Nom de la balise symbolisant la piste cyclable courante.</summary>
	public const string CYCLEWAY = "cycleway";

	/// <summary>Nom de la balise symbolisant la route principale courante.</summary>
	public const string PRIMARY = "primary";

	/// <summary>Nom de la balise symbolisant la route secondaire courante.</summary>
	public const string SECONDARY = "secondary";

	/// <summary>Nom de la balise symbolisant la route tertiaire courante.</summary>
	public const string TERTIARY = "tertiary";

	/// <summary>Nom de la balise symbolisant la route de service courante.</summary>
	public const string SERVICE = "service";

	/// <summary>Nom de la balise symbolisant l'objet non classifié courant.</summary>
	public const string UNCLASSIFIED = "unclassified";


	/// <summary>Nom de la balise donnant des informations sur une objet (id, nom, coordonnées...).</summary>
	public const string INFO = "info";


	/// <summary>Nom de la balise contenant les résultats d'un relevé de mesures pour un bâtiment.</summary>
	public const string RESULTS = "results";

	/// <summary>???.</summary>
	public const string SENSOR_DATA = "brd";

	/// <summary>???.</summary>
	public const string SENSOR_DATA_RECORD = "record";

	/// <summary>Nom de la balise contenant les résultats d'un relevé de mesures pour une salle.</summary>
	//public const string UNIT_ID = "unitID"; // /!\ Peut être incorrecte

	/// <summary>Nom de la balise contenant la valeur d'un capteur.</summary>
	public const string SENSOR_VALUE = "value";

	/// <summary>???</summary>
	public const string SUB_ID = "subID";

	/// <summary>Nom de la balise contenant la date d'enregistrement d'un capteur.</summary>
	public const string DATE_TIME = "datetime";

	/// <summary>???</summary>
	public const string INPUT = "input";

	/// <summary>Nom de la balise contenant les unités d'un capteur.</summary>
	public const string VALUE_UNITS = "value_units";


	/// <summary>Nom de la balise contenant toutes les propriétés de chaque dispositif.</summary>
	public const string DEVICES_PROPERTIES = "properties";

	/// <summary>Nom de la balise contenant les propriétés des dispositifs d'un bâtiment.</summary>
	public const string BUILDING_DEVICES_GROUP = "building";

	/// <summary>Nom de la balise contenant les propriétés des dispositifs d'une pièce.</summary>
	public const string ROOM_DEVICES_GROUP = "room";

	/// <summary>
	///		Nom de la balise contenant les propriétés d'un capteur seul ou d'un capteur et d'un actionneur.
	///	</summary>
	public const string DEVICES = "devices";


	/// <summary>Nom de la balise contenant les propriétés d'un capteur.</summary>
	public const string ROOM_SENSOR = "sensor";

	/// <summary>Nom de la balise contenant la valeur d'un seuil ponctuel.</summary>
	public const string FIXED_VALUE_THRESHOLD = "fixed";

	/// <summary>Nom de la balise contenant la valeur minimale d'un seuil d'intervalle.</summary>
	public const string MIN_THRESHOLD = "min";

	/// <summary>Nom de la balise contenant la valeur maximale d'un seuil d'intervalle.</summary>
	public const string MAX_THRESHOLD = "max";


	/// <summary>Nom de la balise contenant les propriétés d'un actionneur.</summary>
	public const string ROOM_ACTUATOR = "actuator";

	/// <summary>Nom de la balise contenant la valeur minimale physique d'un actionneur.</summary>
	public const string MIN_ACTUATOR_LIMIT = "min";

	/// <summary>Nom de la balise contenant la valeur maximale physique d'un actionneur.</summary>
	public const string MAX_ACTUATOR_LIMIT = "max";

	/// <summary>Nom de la balise contenant la valeur de pas d'un actionneur.</summary>
	public const string ACTUATOR_STEP = "step";
}
