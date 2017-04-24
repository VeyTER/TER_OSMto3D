using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GetInfos : MonoBehaviour {
	public void OnMouseDown() {
		if (tag.Equals (NodeTags.WALL)) {

			// Récupération du bâtiment correspondant au mur sélectionné
			BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
			NodeGroup buildingNgp = buildingsTools.WallToBuildingNodeGroup (this.gameObject);

			string identifier = name.Substring (0, name.LastIndexOf ("_"));

			// Si le bâtiment n'a pas de nom défini, ajouter un prefixe dans son affichage
			double parsedValue = 0;
			if (double.TryParse (identifier, out parsedValue))
				identifier = "Bâtiment n°" + identifier;

			if (buildingNgp != null) {
				// Renommage de l'étiquette indiquant le nom ou le numéro du bâtiment
				Main.panel.SetActive (true);
				Text buildingNameLabel = GameObject.Find ("Nom batiment").GetComponent<Text> ();
				buildingNameLabel.text = identifier;

				this.ChangeBuildingsColor ();
				buildingsTools.SelectedBuilding = transform.parent.gameObject;
			}
		}

		// Ancien code sauvegardé ici
		//		Text temperature = GameObject.Find("Temperature").GetComponent<Text>();
		//		Text humidite = GameObject.Find("Humidite").GetComponent<Text>();
		//		nomBatiment.text = ngp.GetTagValue ("name");
		//		temperature.text = "Temperature : " + ngp.GetTagValue ("temperature");
		//		humidite.text = "Humidité : " + ngp.GetTagValue ("humidité");
	}

	/// <summary>
	/// Change la couleur du bâtiment pointé.
	/// </summary>
	private void ChangeBuildingsColor () {
		GameObject buildingGo = transform.parent.gameObject;

		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		buildingsTools.DiscolorAll ();
		buildingsTools.ColorAsSelected (buildingGo);
	}
}