using UnityEngine;
using UnityEditor;
using System;

public abstract class NodeGroupFactory {
	public static BuildingNodeGroup CreateBuildingNodeGroup(NodeGroup nodeGroup) {
		BuildingNodeGroup buildingNodeGroup = new BuildingNodeGroup(nodeGroup) {
			NbFloor = 1,

			RoofShape = "unknown",
			RoofAngle = 0,

			CustomMaterial = null,
			OverlayColor = Color.white
		};

		return buildingNodeGroup;
	}

	public static HighwayNodeGroup CreateHighwayNodeGroup(NodeGroup nodeGroup) {
		HighwayNodeGroup highwayNodeGroup = new HighwayNodeGroup(nodeGroup) {
			NbWay = 1,
			MaxSpeed = 50
		};

		return highwayNodeGroup;
	}

	public static WaterwayNodeGroup CreateWaterwayNodeGroup(NodeGroup nodeGroup) {
		WaterwayNodeGroup waterwayNodeGroup = new WaterwayNodeGroup(nodeGroup) { };

		return waterwayNodeGroup;
	}

	public static NaturalNodeGroup CreateNaturalNodeGroup(NodeGroup nodeGroup) {
		NaturalNodeGroup naturalNodeGroup = new NaturalNodeGroup(nodeGroup) { };

		return naturalNodeGroup;
	}
}