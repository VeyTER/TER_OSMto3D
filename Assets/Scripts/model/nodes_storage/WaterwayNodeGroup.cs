using System.Collections.Generic;
using System;

public class WaterwayNodeGroup : NodeGroup {
	public WaterwayNodeGroup(string id, string secondaryType) : base(id, "waterway", secondaryType) { }

	public WaterwayNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		this.primaryType = "waterway";
	}

	public WaterwayNodeGroup(string id, string name, string country, string region, string town, string district) :
		base(id, "waterway", name, country, region, town, district) { }

	public override void AddStepNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new WaterwayStepNode(reference, index, latitude, longitude) { Tags = tags });
	}

	public override void AddComponentNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new WaterwayComponentNode(reference, index, latitude, longitude) { Tags = tags });
	}
}