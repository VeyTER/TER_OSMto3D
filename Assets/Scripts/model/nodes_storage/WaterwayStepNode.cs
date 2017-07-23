using UnityEngine;
using UnityEditor;

public class WaterwayNode : Node {
	public WaterwayNode(double latitude, double longitude) : base(latitude, longitude) { }

	public WaterwayNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public WaterwayNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public WaterwayNode(Node node) : base(node) { }
}