using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class NaturalNodeGroup : NodeGroup {
	public NaturalNodeGroup(string id, string secondaryType) : base(id, "natural", secondaryType) { }

	public NaturalNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		this.primaryType = "natural";
	}

	public NaturalNodeGroup(string id, string name, string country, string region, string town, string district) :
		base(id, "natural", name, country, region, town, district) { }

	public override void AddStepNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new NaturalStepNode(reference, index, latitude, longitude) { Tags = tags });
	}

	public override void AddComponentNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new NaturalComponentNode(reference, index, latitude, longitude) { Tags = tags });
	}
}