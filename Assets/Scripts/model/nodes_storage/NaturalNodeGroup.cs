using UnityEngine;
using UnityEditor;

public class NaturalNodeGroup : NodeGroup {
	public NaturalNodeGroup(string id) : base(id, "natural") { }

	public NaturalNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		this.type = "natural";
	}

	public NaturalNodeGroup(string id, string name, string country, string region, string town, string district) :
		base(id, "natural", name, country, region, town, district) { }
}