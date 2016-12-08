using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GetInfos : MonoBehaviour {

	void OnMouseDown() {
		long id = long.Parse(this.gameObject.name.Substring(0,this.gameObject.name.IndexOf("_")));

		NodeGroup ngp = null;

		foreach (NodeGroup ng in main.nodeGroups){

			if(ng.id == id){
				ngp = ng;
			}
		}

		main.panel.SetActive(true);
		Text nomBatiment = GameObject.Find("NomBatiment").GetComponent<Text>();
		Text temperature = GameObject.Find("Temperature").GetComponent<Text>();
		Text humidite = GameObject.Find("Humidite").GetComponent<Text>();

		nomBatiment.text = ngp.GetTagValue ("name");
		temperature.text = "Temperature : " + ngp.GetTagValue ("temperature");
		humidite.text = "Humidité : " + ngp.GetTagValue ("humidité");

	}
}
