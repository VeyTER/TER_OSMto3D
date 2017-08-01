using System.Collections.Generic;

public class HighwayNodeGroup : NodeGroup {
	private int nbWay;
	private int maxSpeed;

	public HighwayNodeGroup(string id, string secondaryType) : base(id, "highway", secondaryType) {
		this.nbWay = 1;
		this.maxSpeed = 50;
	}

	public HighwayNodeGroup(NodeGroup nodeGroup) : base(nodeGroup) {
		mainType = "highway";
	}

	public HighwayNodeGroup(string id, string type, string name, string country, string region, string town, string district, int nbWay, int maxSpeed) :
		base(id, type, name, country, region, town, district) {

		this.nbWay = nbWay;
		this.maxSpeed = maxSpeed;
	}

	public override void AddStepNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new HighwayStepNode(reference, index, latitude, longitude) { Tags = tags });
	}

	public override void AddComponentNode(string reference, int index, double latitude, double longitude, Dictionary<string, string> tags) {
		this.AddNode(new HighwayComponentNode(reference, index, latitude, longitude) { Tags = tags });
	}

	public bool IsBusWayLane() {
		return tags.ContainsKey("bus") && tags.ContainsValue("yes");
	}

	public bool IsPrimary() {
		return tags.ContainsValue("primary");
	}

	public bool IsSecondary() {
		return tags.ContainsValue("secondary");
	}

	public bool IsTertiary() {
		return tags.ContainsValue("tertiary");
	}

	public bool IsUnclassified() {
		return tags.ContainsValue("unclassified");
	}

	public bool IsResidential() {
		return tags.ContainsValue("residential");
	}

	public bool IsService() {
		return tags.ContainsValue("service");
	}

	public bool IsFootway() {
		return tags.ContainsValue("footway");
	}

	public bool IsCycleWay() {
		return tags.ContainsValue("cycleway");
	}

	public bool IsRoad() {
		return (this.IsResidential()
			 || this.IsPrimary()
			 || this.IsSecondary()
			 || this.IsTertiary()
			 || this.IsService()
		);
	}

	public int NbWay {
		get { return nbWay; }
		set { nbWay = value; }
	}

	public int MaxSpeed {
		get { return maxSpeed; }
		set { maxSpeed = value; }
	}
}