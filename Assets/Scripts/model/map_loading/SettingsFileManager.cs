using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class SettingsFileManager : OsmFileManager {
	/// <summary>
	/// 	Charge les données de zonage des bâtiment. Ces dernières permettent de changer les propriétés des bâtiments en
	/// 	fonction de la zone dans laquelle ils se trouvent.
	/// </summary>
	public void LoadSettingsData() {
		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument();

		if (File.Exists(mapSettingsFilePath)) {
			mapsSettingsDocument.Load(mapSettingsFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode earthNode = mapsSettingsDocument.ChildNodes[1];

			// Mise à jour des groupes de noeuds en fonctions des paramètres extraits si le noeud XML existe bien
			if (earthNode != null) {

				// Récupération du noeud XML contenant des informations sur le noeud XML père
				XmlNode earthInfoNode = earthNode.FirstChild;

				// Mise à jour les groupes de noeuds si le noeud XML d'information existe bien
				if (earthInfoNode != null && earthInfoNode.Name.Equals(XmlTags.INFO)) {
					// Récupération des valeurs par défaut pour les bâtiments se trouvant sur Terre et changement de la
					// valeur des attributs correspondant dans les groupes de noeuds concernés
					string[] earthBuildingInfo = osmTools.BuildingInfo(earthInfoNode);

					foreach (KeyValuePair<string, NodeGroup> nodeGroupEntry in nodeGroupBase.NodeGroups) {
						NodeGroup nodeGroup = nodeGroupEntry.Value;

						this.SetupAreaNodeGroups(nodeGroup, null, new double[] { 0, 0, 0 }, earthBuildingInfo, XmlTags.EARTH);

						// Appel de la fonction récursive mettant à jour les attributs des groupes de noeuds pour le zones
						// indiquées dans le tableau de zones
						this.UpdateNodeGroupsProperties(nodeGroup, earthNode, areasBuildingList, 0);
					}
				}
			}
		}
	}
}