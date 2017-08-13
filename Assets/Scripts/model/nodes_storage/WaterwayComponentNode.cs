using UnityEngine;
using UnityEditor;

public class WaterwayComponentNode : Node, IComponentNode {
	public WaterwayComponentNode(double latitude, double longitude) : base(latitude, longitude) { }

	public WaterwayComponentNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public WaterwayComponentNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public WaterwayComponentNode(Node node) : base(node) { }
}