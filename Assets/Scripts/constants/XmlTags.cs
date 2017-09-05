/// <summary>
/// 	<para>
/// 		Contient les noms de balises des documents OSM utilisés par l'application.
/// 	</para>
/// 	<para>
/// 		ATTENTION : Certaines de ces balises sont utilisées pour extraire des données des cartes OSM et ne 
/// 		doivent	donc pas être modifiées, sauf si Open Street Map change le nom des balises.
/// 	</para>
///</summary>
public class XmlTags {
	/// <summary>
	/// 	<para>Nom de la balise de description des fichiers XML.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string XML = "xml"; 


	/// <summary>
	/// 	<para>Nom de la balise donnant l'étendue de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string BOUNDS = "bounds";


	/// <summary>
	/// 	<para>Nom de la balise symbolisant un objet fermé (type bâtiment) de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string NODE = "node";

	/// <summary>
	/// 	<para>Nom de la balise symbolisant un objet composite de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string WAY = "way";


	/// <summary>
	/// 	<para>Nom de la balise donnant une propriété d'un objet de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
	public const string TAG = "tag";

	/// <summary>
	/// 	<para>Nom de la balise symbolisant une étape de construction d'un objet de la ville.</para>
	/// 	<para>NE PAS MODIFIER</para>
	///</summary>
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

	public const string LEISURE = "leisure";

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


	/// <summary>Nom de la balise donnant des informations sur une objet (id, nom, coordonnées...)</summary>
	public const string INFO = "info";


	public const string RESULTS = "results";

	public const string SENSOR_DATA = "brd";

	public const string SENSOR_DATA_RECORD = "record";

	public const string UNIT_ID = "record";

	public const string SENSOR_VALUE = "value";

	public const string SUB_ID = "subID";

	public const string DATE_TIME = "datetime";

	public const string INPUT = "input";

	public const string VALUE_UNITS = "value_units";


	public const string COMPONENTS = "devices";

	public const string THRESHOLDS = "thresholds";
	public const string BUILDING_COMPONENTS_GROUP = "building";
	public const string ROOM_COMPONENTS_GROUP = "room";

	public const string ROOM_SENSOR = "sensor";
	public const string FIXED_VALUE_THRESHOLD = "fixed";
	public const string MIN_THRESHOLD = "min";
	public const string MAX_THRESHOLD = "max";

	public const string ROOM_ACTUATOR = "actuator";
	public const string MIN_ACTUATOR_LIMIT = "min";
	public const string MAX_ACTUATOR_LIMIT = "max";
	public const string ACTUATOR_STEP = "step";
}
