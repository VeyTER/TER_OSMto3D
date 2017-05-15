﻿using System;

/// <summary>
/// 	<para>
/// 		Contient le nom des tags contenus dans objets de type Node.
/// 	</para>
/// 	<para>
/// 		ATTENTION : ces chaines n'ont techniquement aucun lien avec les tags contenus dans les fichiers XML.
/// 	</para>
/// </summary>
public class NodeTags {
	/// <summary> Tag des noeuds représentant un noeud de bâtiment. </summary>
	public const string BUILDING_NODE_TAG = "BuildingNode";

	/// <summary> Tag des noeuds représentant un noeud de route. </summary>
	public const string HIGHWAY_NODE_TAG = "HighwayNode";

	/// <summary> Tag des noeuds représentant un mur. </summary>
	public const string WALL_TAG = "Wall";

	/// <summary> Tag des noeuds représentant un toît. </summary>
	public const string ROOF_TAG = "Roof";

	/// <summary> Tag des noeuds représentant une route. </summary>
	public const string HIGHWAY_TAG = "Highway";

	/// <summary> Tag des noeuds représentant un chemin piéton. </summary>
	public const string FOOTWAY_TAG = "Footway";

	/// <summary> Tag des noeuds représentant une piste cyclable. </summary>
	public const string CYCLEWAY_TAG = "Cycleway";

	/// <summary> Tag des noeuds représentant un arbre. </summary>
	public const string TREE_TAG = "Tree";
}