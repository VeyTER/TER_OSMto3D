using System.Xml;
using UnityEngine;

public abstract class OsmFileManager {
	protected NodeGroupBase nodeGroupBase;
	protected OsmTools osmTools;

	protected string[] areasBuildingList;
	protected string[] areasMinimalList;

	public OsmFileManager() {
		this.nodeGroupBase = NodeGroupBase.GetInstance();
		this.osmTools = OsmTools.GetInstance();

		this.areasBuildingList = new string[] {
			XmlTags.COUNTRY,
			XmlTags.REGION,
			XmlTags.TOWN,
			XmlTags.DISTRICT,
			XmlTags.BUILDING
		};

		this.areasMinimalList = new string[] {
			XmlTags.COUNTRY,
			XmlTags.REGION,
			XmlTags.TOWN,
			XmlTags.DISTRICT,
		};
	}

	/// <summary>
	/// 	Méthode récursive permettant d'affecter aux groupes de noeuds les caractéristiques propres à la zone dans
	/// 	lequel se trouve le bâtiment correspondant. La méthode se sert du tableau contenant les différentes zones et
	/// 	et son indice associé pour connaitre la "profondeur" à laquelle il se trouve dans le document.
	/// </summary>
	/// <param name="nodeGroup">Groupe de noeuds à mettre à jour.</param>
	/// <param name="boundingAreaNode">Noeud XML de la zone courante.</param>
	/// <param name="areaTypes">Types de zones pouvant être rencontrée et classées par ordre de profondeur.</param>
	/// <param name="areaTypeIndex">Index utilisé par la méthode pour connaître sa profondeur dans le fichier.</param>
	public void UpdateNodeGroupsProperties(NodeGroup nodeGroup, XmlNode boundingAreaNode, string[] areaTypes, int areaTypeIndex) {
		// Traitement pour tous les noeud XML fils
		for (int i = 1; i < boundingAreaNode.ChildNodes.Count; i++) {
			// Mise à jour des attributs des groupes de noeuds correspondant si le noeud XML fils est au niveau
			// inférieur à celui du noeud XML courant
			if (boundingAreaNode.ChildNodes[i].Name.Equals(areaTypes[areaTypeIndex])) {
				// Récupération des noeuds fils du noeud de zone englobant et extraction de la désignation de la
				// zone inférieure
				XmlNode areaNode = boundingAreaNode.ChildNodes[i];
				string areaDesignation = osmTools.AttributeValue(areaNode, XmlAttributes.DESIGNATION);

				// Récupération du premier fils du noeud de zone inférieure, qui est le noeud contenant les
				// caractéristiques par défaut de la zone courante. 
				XmlNode areaInfoNode = areaNode.FirstChild;

				// Extraction de la localisation et des caractéristiques par défaut des bâtiment se trouvant dans la
				// zone inférieure
				double[] areaLocationInfo = osmTools.AttributeLocationInfo(areaInfoNode);
				string[] areaBuildingInfo = osmTools.BuildingInfo(areaInfoNode);

				// Affectation des caractéristiques par défaut aux groupes de noeuds correspondant aux bâtiments
				// se trouvant dans la zone inférieure
				this.SetupAreaNodeGroups(nodeGroup, areaDesignation, areaLocationInfo, areaBuildingInfo, areaTypes[areaTypeIndex]);

				// Appel récursif si l'on ne se trouve pas à la profondeur maximale
				if (areaTypeIndex < areaTypes.Length)
					this.UpdateNodeGroupsProperties(nodeGroup, areaNode, areaTypes, areaTypeIndex + 1);
			}
		}
	}


	/// <summary>
	/// 	Change les caractéristiques des groupes de noeud se trouvant dans une certaine zone avec les valeurs par
	/// 	défaut de cette zone.
	/// </summary>
	/// <param name="nodeGroup">Groupe de noeuds à mettre à jour.</param>
	/// <param name="designation">Nom de la zone (ex : France, Toulouse etc...).</param>
	/// <param name="locationData">Données sur la localisation de la zone.</param>
	/// <param name="buildingData">Caractéristiques par défaut sur les bâtiments se trouvant dans la zone.</param>
	/// <param name="tagName">Type de zone.</param>
	public void SetupAreaNodeGroups(NodeGroup nodeGroup, string designation, double[] locationData, string[] buildingData, string tagName) {
		// Calcul de la distance du bâtiment avec le centre de la zone
		double nodeGroupDistance = Vector2.Distance(new Vector2((float) locationData[0], (float) locationData[1]), new Vector2((float) nodeGroup.GetNode(0).Latitude, (float) nodeGroup.GetNode(0).Longitude));

		// Changement des caractésitiques si l'objet correspondant au groupe de noeuds courant est dans la zone
		if (tagName.Equals(XmlTags.EARTH) || nodeGroupDistance < locationData[2]) {
			switch (tagName) {
			case XmlTags.COUNTRY:
				nodeGroup.Country = designation;
				break;
			case XmlTags.REGION:
				nodeGroup.Region = designation;
				break;
			case XmlTags.TOWN:
				nodeGroup.Town = designation;
				break;
			case XmlTags.DISTRICT:
				nodeGroup.District = designation;
				break;
			}

			// Change les caractéristiques des données relatives aux bâtiments si le groupe de noeuds courant
			// représente un tel objet
			if (nodeGroup.GetType() == typeof(BuildingNodeGroup)) {
				BuildingNodeGroup buildingNodeGroup = (BuildingNodeGroup) nodeGroup;

				buildingNodeGroup.NbFloor = int.Parse(buildingData[0]);
				buildingNodeGroup.RoofAngle = int.Parse(buildingData[1]);
				buildingNodeGroup.RoofShape = buildingData[2];

				Material customMaterial = Resources.Load<Material>(FilePaths.MATERIALS_FOLDER_LOCAL + buildingData[3]);
				if (customMaterial != null)
					buildingNodeGroup.CustomMaterial = customMaterial;

				string[] colorFactors = buildingData[4].Split(';');
				buildingNodeGroup.OverlayColor = new Color(float.Parse(colorFactors[0]), float.Parse(colorFactors[1]), float.Parse(colorFactors[2]));
			}
		}
	}
}