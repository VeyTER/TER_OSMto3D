using System.IO;
using System.Xml;

internal class CustomFileManager :OsmFileManager {
	/// <summary>
	///		Extrait les données du MapCustom des objets et écrase les caractéristiques des objets correspondant dans le
	/// 	fichier map_resumed avec celles de la version personnalisées de l'objet.
	/// </summary>
	public void LoadCustomData() {
		string mapResumedFilePath = FilePaths.MAPS_RESUMED_FOLDER + "map_resumed.osm";
		string mapCustomFilePath = FilePaths.MAPS_CUSTOM_FOLDER + "map_custom.osm";

		XmlDocument mapResumedDocument = new XmlDocument();
		XmlDocument mapCustomDocument = new XmlDocument();

		if (File.Exists(mapResumedFilePath) && File.Exists(mapCustomFilePath)) {
			mapResumedDocument.Load(mapResumedFilePath);
			mapCustomDocument.Load(mapCustomFilePath);

			// Récupération du noeud XML englobant tous les objets terrestres
			XmlNode customEarthNode = mapCustomDocument.ChildNodes[1];

			if (customEarthNode != null) {
				// Récupération de chaque noeud représentant un fichier personnalisé puis mise à jour du noeud
				// correspondant dans le fihcier MapResumed
				XmlNodeList customNodes = customEarthNode.ChildNodes;
				foreach (XmlNode customNode in customNodes) {
					// Extraction du noeud XML d'information de l'objet personnalisé et récupération de son ID
					XmlNode customInfoNode = customNode.FirstChild;
					string buildingId = osmTools.AttributeValue(customInfoNode, XmlAttributes.ID);

					// Récupération du noeud XML d'information de l'objet du fichier map_resumed correspondant à l'objet
					// courant dans le fichier map_custom
					XmlNode matchingResumedInfoNode = mapResumedDocument.SelectSingleNode("//" + XmlTags.INFO + "[@" + XmlAttributes.ID + "=\"" + buildingId + "\"]");

					if (matchingResumedInfoNode != null)
						this.AddOsmBuilding(mapResumedDocument, customInfoNode, matchingResumedInfoNode);

					if (buildingId.StartsWith("+"))
						this.AddUserBuilding(mapResumedDocument, customInfoNode, buildingId);
				}
			}

			mapResumedDocument.Save(mapResumedFilePath);
		}
	}


	private void AddOsmBuilding(XmlDocument mapResumedDocument, XmlNode customInfoNode, XmlNode matchingResumedInfoNode) {
		// Récupération du noeud XML correspondant à l'objet du fichier map_resumed
		XmlNode matchingResumedNode = matchingResumedInfoNode.ParentNode;

		// Remplacement du noeud XML d'information seul dans le fichier résumé si la personnalisation ne
		// concerne que les caractéristiques, sinon, si la personnalisation concerne les sous-noeuds XML
		// de l'objet, ceux-ci sont aussi remplacés
		if (customInfoNode.ParentNode.ChildNodes.Count == 1) {
			// Suppression du noeud XML d'information au niveau de l'objet du fichier map_resumed
			matchingResumedNode.RemoveChild(matchingResumedNode.FirstChild);

			// Ajout du noeud XML d'information du fichier map_custom avant les sous-noeuds XML de
			// l'objet du fichier mmap_resumed
			XmlNode newResumedInfoNode = mapResumedDocument.ImportNode(customInfoNode, true);
			matchingResumedNode.InsertBefore(newResumedInfoNode, matchingResumedNode.FirstChild);
		} else {
			// Suppression de tous les noeuds XML contenus (y compris le noeuds d'information) compris
			// dans le noeud XML de l'objet du fichier map_resumed
			matchingResumedNode.RemoveAll();

			// Ajout de tous les noeuds XML contenus dans l'objet du fichier map_custom dans l'objet
			// correspondant dans le fichier map_resumed
			foreach (XmlNode customChildNode in customInfoNode.ParentNode.ChildNodes) {
				XmlNode newResumedChildNode = mapResumedDocument.ImportNode(customChildNode, true);
				matchingResumedNode.AppendChild(newResumedChildNode);
			}
		}
	}

	private void AddUserBuilding(XmlDocument mapResumedDocument, XmlNode customInfoNode, string buildingId) {
		BuildingNodeGroup buildingNodeGroup = new BuildingNodeGroup(buildingId, null);
		for (int i = 1; i < customInfoNode.ParentNode.ChildNodes.Count; i++) {
			XmlNode ndNode = customInfoNode.ParentNode.ChildNodes[i];
			string id = osmTools.AttributeValue(ndNode, XmlAttributes.ID);
			double[] locationData = osmTools.AttributeLocationInfo(ndNode);
			buildingNodeGroup.AddNode(new BuildingStepNode(id, locationData[0], locationData[1]));
		}

		string mapSettingsFilePath = FilePaths.MAPS_SETTINGS_FOLDER + "map_settings.osm";
		XmlDocument mapsSettingsDocument = new XmlDocument();
		if (File.Exists(mapSettingsFilePath)) {
			mapsSettingsDocument.Load(mapSettingsFilePath);
			XmlNode settingsEarthNode = mapsSettingsDocument.ChildNodes[1];

			this.UpdateNodeGroupsProperties(buildingNodeGroup, settingsEarthNode, areasBuildingList, 0);

			string zoningXPath = buildingNodeGroup.ToZoningXPath();
			XmlNode resumeParentNode = mapResumedDocument.SelectSingleNode(zoningXPath);

			XmlNode resumeBuildingNode = mapResumedDocument.ImportNode(customInfoNode.ParentNode, true);
			resumeParentNode.AppendChild(resumeBuildingNode);
		}
	}
}