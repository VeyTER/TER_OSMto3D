using UnityEngine;
using UnityEditor;

public class BuildingStepNode : Node {
	public BuildingStepNode(double latitude, double longitude) : base(latitude, longitude) { }

	public BuildingStepNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public BuildingStepNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public BuildingStepNode(Node node) : base(node) { }
}