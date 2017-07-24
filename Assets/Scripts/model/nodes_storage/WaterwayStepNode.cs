using UnityEngine;
using UnityEditor;

public class WaterwayStepNode : Node, IStepNode {
	public WaterwayStepNode(double latitude, double longitude) : base(latitude, longitude) { }

	public WaterwayStepNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public WaterwayStepNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public WaterwayStepNode(Node node) : base(node) { }
}