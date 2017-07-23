using UnityEngine;
using UnityEditor;

public class WaterwayNodeGroup : NodeGroup {
	public WaterwayNodeGroup(string id) : base(id, "waterway") { }

	public WaterwayNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		this.type = "waterway";
	}

	public WaterwayNodeGroup(string id, string name, string country, string region, string town, string district) :
		base(id, "waterway", name, country, region, town, district) { }
}