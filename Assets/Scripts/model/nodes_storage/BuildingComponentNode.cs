using System.Collections.Generic;

internal class BuildingComponentNode : Node, IComponentNode {
	public BuildingComponentNode(double latitude, double longitude) : base(latitude, longitude) { }

	public BuildingComponentNode(string reference, double latitude, double longitude) : base(reference, latitude, longitude) { }

	public BuildingComponentNode(string reference, int index, double latitude, double longitude) : base(reference, index, latitude, longitude) { }

	public BuildingComponentNode(Node node) : base(node) { }
}