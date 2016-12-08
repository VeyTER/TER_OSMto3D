using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public bool wallActive = true;
	public bool nodeActive = true;

	public void SetWallActive(){

		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

		if (walls.Length == 0) {
			walls = main.mainWalls;
		}
		else{
			main.mainWalls = walls;
		}

		foreach( GameObject go in walls){
			if (wallActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		wallActive = !wallActive;
	}

	public void SetNodeActive(){
		
		GameObject[] nodes = GameObject.FindGameObjectsWithTag("Node");

		if (nodes.Length == 0) {
			nodes = main.mainNodes;
		}
		else{
			main.mainNodes = nodes;
		}

		foreach(GameObject go in nodes){
			if (nodeActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		nodeActive = !nodeActive;
	}

	public void SetPanelInnactive(){
		main.panel.SetActive (false);
	}

}
