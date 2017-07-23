using UnityEngine;
using UnityEditor;

public class NaturalComponentNode : Node {
	public NaturalComponentNode(double latitude, double longitude) : base(latitude, longitude) { }

	public NaturalComponentNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public NaturalComponentNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public NaturalComponentNode(Node node) : base(node) { }

	public bool IsTree() {
		return tags.ContainsValue("tree");
	}
}