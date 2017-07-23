using UnityEngine;
using UnityEditor;

public class HighwayComponentNode : Node {
	public HighwayComponentNode(double latitude, double longitude) : base(latitude, longitude) {}

	public HighwayComponentNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public HighwayComponentNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public HighwayComponentNode(Node node) : base(node) { }

	public bool IsTrafficSignal() {
		return tags.ContainsValue("traffic_signals");
	}
}