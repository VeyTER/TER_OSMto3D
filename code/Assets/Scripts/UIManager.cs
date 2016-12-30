﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public bool wallActive = true;
	public bool highwayNodeActive = true;
	public bool buildingNodeActive = true;

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

	public void SetBuildingNodeActive(){
		
		GameObject[] nodes = GameObject.FindGameObjectsWithTag("BuildingNode");

		if (nodes.Length == 0) {
			nodes = main.mainBuildingNodes;
		}
		else{
			main.mainBuildingNodes = nodes;
		}

		foreach(GameObject go in nodes){
			if (buildingNodeActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		buildingNodeActive = !buildingNodeActive;
	}

	public void SetHighwayNodeActive(){

		GameObject[] nodes = GameObject.FindGameObjectsWithTag("HighwayNode");

		if (nodes.Length == 0) {
			nodes = main.mainHighwayNodes;
		}
		else{
			main.mainHighwayNodes = nodes;
		}

		foreach(GameObject go in nodes){
			if (highwayNodeActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		highwayNodeActive = !highwayNodeActive;
	}

	public void SetPanelInnactive(){
		main.panel.SetActive (false);
	}

}
