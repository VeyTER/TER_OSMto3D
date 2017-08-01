using UnityEngine;
using UnityEditor;

public class LeisureStepNode : Node, IStepNode {
	public LeisureStepNode(double latitude, double longitude) : base(latitude, longitude) { }

	public LeisureStepNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public LeisureStepNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public LeisureStepNode(Node node) : base(node) { }
}