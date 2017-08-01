using System.Collections.Generic;
using System;

public class LeisureNodeGroup : NodeGroup {
	public LeisureNodeGroup(string id, string secondaryType) : base(id, "leisure", secondaryType) { }

	public LeisureNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		this.mainType = "leisure";
	}

	public LeisureNodeGroup(string id, string name, string country, string region, string town, string district) :
		base(id, "leisure", name, country, region, town, district) { }

	public override void AddStepNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new LeisureStepNode(reference, index, latitude, longitude) { Tags = tags });
	}

	public override void AddComponentNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		throw new NotImplementedException("Tentative d'insertion d'un noeud composant dans un groupe de noeud non prévu pour en accueillir.\n" + reference + "/" + index);
	}

	public bool IsStadium() {
		return tags.ContainsValue("stadium");
	}
}