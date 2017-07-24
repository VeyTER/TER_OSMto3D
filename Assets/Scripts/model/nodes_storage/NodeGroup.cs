using UnityEngine;
using System.Collections.Generic;

public class NodeGroup {
	protected string id;
	protected string name;
	protected string type;

	protected List<Node> nodes;

	protected Dictionary<string, string> tags;

	protected double minLon;
	protected double maxLon;
	protected double minLat;
	protected double maxLat;

	protected string country;
	protected string region;
	protected string town;
	protected string district;

	public NodeGroup(string id, string type) {
		this.id = id;
		this.name = "unknown";
		this.type = type;

		this.nodes = new List<Node> ();

		this.tags = new Dictionary<string, string> ();

		this.minLon = 0;
		this.maxLon = 0;
		this.minLat = 0;
		this.maxLat = 0;

		this.country = "unknown";
		this.region = "unknown";
		this.town = "unknown";
		this.district = "unknown";
	}

	public NodeGroup(string id, string type, string name, string country, string region, string town, string district) {
		this.id = id;
		this.name = name;
		this.type = type;

		this.nodes = new List<Node>();

		this.tags = new Dictionary<string, string>();

		this.minLon = 0;
		this.maxLon = 0;
		this.minLat = 0;
		this.maxLat = 0;

		this.country = country;
		this.region = region;
		this.town = town;
		this.district = district;
	}

	public NodeGroup(NodeGroup nodeGroup) {
		this.id = nodeGroup.id;
		this.name = nodeGroup.Name;
		this.type = nodeGroup.type;

		this.nodes = nodeGroup.Nodes;

		this.tags = nodeGroup.Tags;

		this.minLon = nodeGroup.MinLon;
		this.maxLon = nodeGroup.MaxLon;
		this.minLat = nodeGroup.MinLat;
		this.maxLat = nodeGroup.MaxLat;

		this.country = nodeGroup.Country;
		this.region = nodeGroup.Region;
		this.town = nodeGroup.Town;
		this.district = nodeGroup.District;
	}

	public Node AddNode(Node newNode) {
		nodes.Add(newNode);
		return newNode;
	}

	public Node GetNode(int nodeOrder) {
		return nodes[nodeOrder];
	}

	public Node GetNode(string nodeId) {
		int i = 0;
		for (; i < nodes.Count && !Node.GenerateId(nodes[i]).Equals(nodeId); i++);
		if (i < nodes.Count)
			return nodes[i];
		else
			return null;
	}

	public void ReplaceNode(Node oldNode, Node newNode) {
		int oldNodeIndex = nodes.IndexOf(oldNode);
		if(oldNodeIndex > -1)
			nodes[oldNodeIndex] = newNode;
	}

	public void RemoveNode(int nodeOrder) {
		nodes.RemoveAt(nodeOrder);
	}

	public int NodeCount() {
		return nodes.Count;
	}

	// ajoute un tag au NodeGroup
	public void AddTag(string key, string value) {
		tags.Add (key, value);
	}

	// teste l'egalité de deux NodeGroup
	public bool Equals (NodeGroup nodeGroup) {
		return id.Equals(nodeGroup.id);
	}

	// renvoie la valeur du tag si la clé existe
	public string GetTagValue(string key) {
		if (tags.ContainsKey(key))
			return tags [key].ToString ();
		else
			return key + "_unknown";
	}

	// met a jour la longitude et Latitude min et max
	public void SetBoundaries(){
		this.minLon = this.GetNode(0).Longitude;
		this.maxLon = this.GetNode(0).Longitude;
		this.minLat = this.GetNode(0).Latitude;
		this.maxLat = this.GetNode(0).Latitude;

		foreach (Node n in this.nodes) {
			if(n.Latitude > this.maxLat)
				maxLat = n.Latitude;
			
			if(n.Latitude < this.minLat)
				minLat = n.Latitude;
			
			if(n.Longitude > this.maxLon)
				maxLon = n.Longitude;
			
			if(n.Longitude < this.minLon)
				minLon = n.Longitude;
		}
	}

	public string Id {
		get { return id; }
		set { id = value; }
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public string Type {
		get { return type; }
		set { type = value; }
	}

	public List<Node> Nodes {
		get { return nodes; }
	}

	public double MinLon {
		get { return minLon; }
		set { minLon = value; }
	}

	public double MaxLon {
		get { return maxLon; }
		set { maxLon = value; }
	}

	public double MinLat {
		get { return minLat; }
		set { minLat = value; }
	}

	public double MaxLat {
		get { return maxLat; }
		set { maxLat = value; }
	}

	public string Country {
		get { return country; }
		set { country = value; }
	}

	public string Region {
		get { return region; }
		set { region = value; }
	}

	public string Town {
		get { return town; }
		set { town = value; }
	}

	public string District {
		get { return district; }
		set { district = value; }
	}

	public Dictionary<string, string> Tags {
		get { return tags; }
		set { tags = value; }
	}
}