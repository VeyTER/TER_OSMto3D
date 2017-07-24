using UnityEngine;
using UnityEditor;

public abstract class NodeFactory {
	public static BuildingStepNode CreateBuildingStepNode(Node node) {
		BuildingStepNode buildingStepNode = new BuildingStepNode(node) { };
		return buildingStepNode;
	}

	public static HighwayComponentNode CreateHighwayComponentNode(Node node) {
		HighwayComponentNode highwayComponentNode = new HighwayComponentNode(node) { };
		return highwayComponentNode;
	}

	public static HighwayStepNode CreateHighwayStepNode(Node node) {
		HighwayStepNode highwayStepNode = new HighwayStepNode(node) { };
		return highwayStepNode;
	}

	public static NaturalComponentNode CreateNaturalComponentNode(Node node) {
		NaturalComponentNode naturalComponentNode = new NaturalComponentNode(node) { };
		return naturalComponentNode;
	}

	public static WaterwayStepNode CreateWaterwayStepNode(Node node) {
		WaterwayStepNode waterwayStepNode = new WaterwayStepNode(node) { };
		return waterwayStepNode;
	}
}