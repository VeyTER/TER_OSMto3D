using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public bool wallActive = true;
	public bool roofActive = true;
	public bool highwayActive = true;
	public bool treeActive = true;
	public bool cyclewayActive = true;
	public bool footwayActive = true;
	public bool busLaneActive = true;
	public bool highwayNodeActive = false;
	public bool buildingNodeActive = false;

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

	public void SetRoofActive(){

		GameObject[] roofs = GameObject.FindGameObjectsWithTag("Roof");

		if (roofs.Length == 0) {
			roofs = main.mainRoofs;
		}
		else{
			main.mainRoofs = roofs;
		}

		foreach( GameObject go in roofs){
			if (roofActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		roofActive = !roofActive;
	}

	public void SetHighwayActive(){

		GameObject[] highways = GameObject.FindGameObjectsWithTag("Highway");

		if (highways.Length == 0) {
			highways = main.mainHighways;
		}
		else{
			main.mainHighways = highways;
		}

		foreach( GameObject go in highways){
			if (highwayActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		highwayActive = !highwayActive;
	}

	public void SetBuildingNodeActive(){
		
		GameObject[] nodes = GameObject.FindGameObjectsWithTag("BuildingNode");

//		if (nodes.Length == 0) {
			nodes = main.mainBuildingNodes;
//		}
//		else{
//			main.mainBuildingNodes = nodes;
//		}

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

//		if (nodes.Length == 0) {
			nodes = main.mainHighwayNodes;
//		}
//		else{
//			main.mainHighwayNodes = nodes;
//		}

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

	public void SetFootwayActive(){

		GameObject[] footways = GameObject.FindGameObjectsWithTag("Footway");

		if (footways.Length == 0) {
			footways = main.mainFootways;
		}
		else{
			main.mainFootways = footways;
		}

		foreach( GameObject go in footways){
			if (footwayActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		footwayActive = !footwayActive;
	}

	public void SetCyclewayActive(){

		GameObject[] cycleways = GameObject.FindGameObjectsWithTag("Cycleway");

		if (cycleways.Length == 0) {
			cycleways = main.mainCycleways;
		}
		else{
			main.mainCycleways = cycleways;
		}

		foreach( GameObject go in cycleways){
			if (cyclewayActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		cyclewayActive = !cyclewayActive;
	}

//	public void SetBusLaneActive(){
//
//		GameObject[] busLanes = GameObject.FindGameObjectsWithTag("BusLane");
//
//		if (busLanes.Length == 0) {
//			busLanes = main.mainBusLanes;
//		}
//		else{
//			main.mainBusLanes = busLanes;
//		}
//
//		foreach( GameObject go in busLanes){
//			if (busLaneActive) {
//				go.SetActive(false);
//			} 
//			else {
//				go.SetActive(true);
//			}
//		}
//		busLaneActive = !busLaneActive;
//	}

	public void SetTreeActive(){

		GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");

		if (trees.Length == 0) {
			trees = main.mainTrees;
		}
		else{
			main.mainTrees = trees;
		}

		foreach( GameObject go in trees){
			if (treeActive) {
				go.SetActive(false);
			} 
			else {
				go.SetActive(true);
			}
		}
		treeActive = !treeActive;
	}

	public void SetPanelInnactive(){
		main.panel.SetActive (false);
	}

}
