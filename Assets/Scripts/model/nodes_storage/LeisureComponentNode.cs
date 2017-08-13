using UnityEngine;
using UnityEditor;

public class LeisureComponentNode : Node, IComponentNode {
	public LeisureComponentNode(double latitude, double longitude) : base(latitude, longitude) { }

	public LeisureComponentNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public LeisureComponentNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public LeisureComponentNode(Node node) : base(node) { }
}