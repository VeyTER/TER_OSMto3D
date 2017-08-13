using UnityEngine;
using UnityEditor;

public class NaturalStepNode : Node, IStepNode {
	public NaturalStepNode(double latitude, double longitude) : base(latitude, longitude) { }

	public NaturalStepNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public NaturalStepNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public NaturalStepNode(Node node) : base(node) { }
}