using UnityEngine;
using UnityEditor;

public class HighwayStepNode : Node, IStepNode {
	public HighwayStepNode(double latitude, double longitude) : base(latitude, longitude) { }

	public HighwayStepNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public HighwayStepNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public HighwayStepNode(Node node) : base(node) { }
}