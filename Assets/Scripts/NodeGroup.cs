using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeGroup {
	private double id;
	private string name;

	private ArrayList nodes;
	private ArrayList decomposition;
	private Hashtable tags;

	private double minLon;
	private double maxLon;
	private double minLat;
	private double maxLat;

	private string country;
	private string region;
	private string town;
	private string district;

	private double temperature;
	private int nbFloors;

	private string roofType;
	private int roofAngle;

	private int nbWay;
	private int maxSpeed;

	//constructeur
	public NodeGroup(double id) {
		this.id = id;
		this.name = "unknown";

		this.nodes = new ArrayList ();
		this.decomposition = new ArrayList ();
		this.tags = new Hashtable ();

		this.minLon = 0;
		this.maxLon = 0;
		this.minLat = 0;
		this.maxLat = 0;

		this.country = "unknown";
		this.region = "unknown";
		this.town = "unknown";
		this.district = "unknown";

		this.temperature = 0;
		this.nbFloors = 1;

		this.roofType = "unknown";
		this.roofAngle = 0;

		this.nbWay = 1;
		this.maxSpeed = 50;
	}

	//Surcharge du constructeur
	public NodeGroup(double id, string name, string country, string region, string town, string district, string roofType, int roofAngle, int nbWay, int maxSpeed) {
		this.id = id;
		this.name = name;

		this.nodes = new ArrayList();
		this.decomposition = new ArrayList();
		this.tags = new Hashtable();

		this.minLon = 0;
		this.maxLon = 0;
		this.minLat = 0;
		this.maxLat = 0;

		this.country = country;
		this.region = region;
		this.town = town;
		this.district = district;

		this.temperature = 0;
		this.nbFloors = 1;

		this.roofAngle = roofAngle;
		this.roofType = roofType;

		this.nbWay = nbWay;
		this.maxSpeed = maxSpeed;
	}

	// ajoute une node à l'ensemble 
	public void AddNode(Node n) {
		this.nodes.Add (n);
	}

	//supprime la n ieme valeur de node 
	public void RemoveNode(int n) {
		this.nodes.RemoveAt(n);
	}

	public int NodeCount() {
		return nodes.Count;
	}

	// retourne la node demandée 
	public Node GetNode(int i) {
		return (Node)this.nodes [i];
	}

	// ajoute un tag au NodeGroup
	public void AddTag(string key, string value) {
		this.tags.Add (key, value);
	}

	// retourne vrai si le nodeGroup correspond à un batiment
	public bool IsBuilding() {
		return this.tags.ContainsKey ("building");
	}

	// retourne vrai si le nodeGroup correspond à une voie d'eau
	public bool IsWaterway() {
		return this.tags.ContainsKey ("waterway");
	}

	// retourne vrai si le NodeGroup correspond à un arbre
	public bool IsTree() {
		return this.tags.ContainsValue("tree");
	}

	// retourne vrai si le NodeGroup correspond à un feu tricolore
	public bool IsTrafficLight() {
		return this.tags.ContainsValue("traffic_signals");
	}

	// retourne vrai si le nodeGroup correspond à une route
	public bool IsHighway() {
		return this.tags.ContainsKey ("highway");
	}

	// retourne vrai si le nodeGroup correspond à une voie de bus
	public bool IsBusWayLane() {
		return (this.tags.ContainsKey ("bus") && this.tags.ContainsValue ("yes"));
	}

	// vrai si on a une route de type "primaire"
	public bool IsPrimary() {
		return this.tags.ContainsValue ("primary");
	}

	// vrai si on a une route de type "secondaire"
	public bool IsSecondary() {
		return this.tags.ContainsValue ("secondary");
	}

	// vrai si on a une route de type "tertiaire"
	public bool IsTertiary() {
		return this.tags.ContainsValue ("tertiary");
	}

	// vrai si on a une route de type "non classifiée"
	public bool IsUnclassified() {
		return this.tags.ContainsValue ("unclassified");
	}

	// vrai si on a une route de type "résidentielle"
	public bool IsResidential() {
		return this.tags.ContainsValue ("residential");
	}

	// vrai si on a une route de type "service"
	public bool IsService() {
		return this.tags.ContainsValue ("service");
	}

	// vrai si on a une route de type "chemin piétons"
	public bool IsFootway() {
		return this.tags.ContainsValue ("footway");
	}

	// vrai si on a une voie cyclable
	public bool IsCycleWay() {
		return this.tags.ContainsValue ("cycleway");
	}

	// teste l'egalité de deux NodeGroup
	public bool Equals (NodeGroup ng) {
		return this.id == ng.id;
	}

	// renvoie la valeur du tag si la clé existe
	public string GetTagValue(string key) {
		if (this.tags.ContainsKey(key))
			return this.tags [key].ToString ();
		else
			return key + "_unknown";
	}

	// teste si un point est à gauche ou a droite d'un vecteur
	public bool IsAtRight( Node nodeA, Node nodeB, Node nodeTest) {
		double determinant, abx, aby, dx, dy;

		abx = nodeB.Latitude - nodeA.Latitude;
		aby = nodeB.Longitude - nodeA.Longitude;

		dx = nodeTest.Latitude - nodeA.Latitude;
		dy = nodeTest.Longitude - nodeA.Longitude;

		determinant = abx * dy - aby * dx;

		return (determinant >= 0);
	}

	// Décompose le polygone en triangles
	public bool DecomposeRight() {
		LinkedList<Node> nodesTemp = new LinkedList<Node> ();
		LinkedListNode<Node> cur = new LinkedListNode<Node> (this.GetNode (0));
		int i;

		//on ajoute le premier élément
		nodesTemp.AddFirst (cur);

		for (i = 1; i < this.nodes.Count - 1; i++) {
			nodesTemp.AddAfter(cur,new LinkedListNode<Node> (this.GetNode (i)));
			cur = cur.Next;
		}

		LinkedListNode<Node> nodeA, nodeB, nodeTest;

		nodeA = nodesTemp.First;
		nodeB = nodeA.Next;
		nodeTest = nodeB.Next;
		NodeGroup ng = null;
		bool sens = true;

		i = 0;
		while (nodesTemp.Count > 3 && sens ) {
			// si l'angle est ok on ajoute le triangle et on supprime le point de la liste
			if(IsAtRight(nodeA.Value, nodeB.Value, nodeTest.Value)){
				// on créé le sous triangle
				ng = new NodeGroup(0);
				ng.AddNode(nodeA.Value);
				ng.AddNode(nodeB.Value);
				ng.AddNode(nodeTest.Value);
				this.decomposition.Add(ng);

				// on supprime le point
				nodesTemp.Remove(nodeB.Value);

				// on avance en cercle
				nodeA = nodeTest;
				if(nodeA.Next == null)
					nodeB = nodesTemp.First;
				else
					nodeB = nodeA.Next;

				if(nodeB.Next == null)
					nodeTest = nodesTemp.First;
				else
					nodeTest = nodeB.Next;
			} else {
				nodeA = nodeB;
				if(nodeA.Next == null)
					nodeB = nodesTemp.First;
				else
					nodeB = nodeA.Next;

				if(nodeB.Next == null)
					nodeTest = nodesTemp.First;
				else
					nodeTest = nodeB.Next;
			}

			i++;
			if (i > 150) {
				sens = false;
			}
		}

		// on ajoute le dernier triangle
		ng = new NodeGroup(0);
		ng.AddNode(nodeA.Value);
		ng.AddNode(nodeB.Value);
		ng.AddNode(nodeTest.Value);
		this.decomposition.Add(ng);

		return sens;
	}

	// Décompose le polygone en triangles
	public bool DecomposeLeft() {
		LinkedList<Node> nodesTemp = new LinkedList<Node> ();

		nodesTemp.Clear ();
		LinkedListNode<Node> cur = new LinkedListNode<Node> (this.GetNode (0));
		int i;

		//on ajoute le premier élément
		nodesTemp.AddFirst (cur);

		for (i = 1; i < this.nodes.Count - 1; i++) {
			nodesTemp.AddAfter(cur,new LinkedListNode<Node> (this.GetNode (i)));
			cur = cur.Next;
		}

		LinkedListNode<Node> nodeA, nodeB, nodeTest;

		nodeA = nodesTemp.First;
		nodeB = nodeA.Next;
		nodeTest = nodeB.Next;
		NodeGroup ng = null;
		bool sens = true;

		i = 0;
		while (nodesTemp.Count > 3 && sens ) {
			// si l'angle est ok on ajoute le triangle et on supprime le point de la liste
			if(!IsAtRight(nodeA.Value,nodeB.Value,nodeTest.Value)){
				// on créé le sous triangle
				ng = new NodeGroup(1);
				ng.AddNode(nodeA.Value);
				ng.AddNode(nodeB.Value);
				ng.AddNode(nodeTest.Value);
				this.decomposition.Add(ng);

				// on supprime le point
				nodesTemp.Remove(nodeB.Value);

				// on avance en cercle
				nodeA = nodeTest;
				if(nodeA.Next == null)
					nodeB = nodesTemp.First;
				else
					nodeB = nodeA.Next;

				if(nodeB.Next == null)
					nodeTest = nodesTemp.First;
				else
					nodeTest = nodeB.Next;
			} else {
				nodeA = nodeB;
				if(nodeA.Next == null)
					nodeB = nodesTemp.First;
				else
					nodeB = nodeA.Next;
				
				if(nodeB.Next == null)
					nodeTest = nodesTemp.First;
				else
					nodeTest = nodeB.Next;
			}

			i++;
			if(i > 150) {
				sens = false;
			}
		}

		// on ajoute le dernier triangle
		ng = new NodeGroup(1);
		ng.AddNode(nodeA.Value);
		ng.AddNode(nodeB.Value);
		ng.AddNode(nodeTest.Value);
		this.decomposition.Add(ng);

		return sens;
	}

	// teste l'appartenance d'un point à un nodeGroup
	public bool BelongsTo(Node test) {
		bool resultat = false;
		bool btest = false;

		foreach (NodeGroup ng in this.decomposition) {
			// on teste pour savoir dans quel sens on doit vérifier les points ( droite ou gauche )
			if(ng.id == 0 ) {
				btest = (this.IsAtRight(ng.GetNode(0),ng.GetNode(1),test) && this.IsAtRight(ng.GetNode(1),ng.GetNode(2),test) && this.IsAtRight(ng.GetNode(2),ng.GetNode(0),test));
				resultat = resultat || btest;
			} else {
				btest = (!this.IsAtRight(ng.GetNode(0),ng.GetNode(1),test) && !this.IsAtRight(ng.GetNode(1),ng.GetNode(2),test) && !this.IsAtRight(ng.GetNode(2),ng.GetNode(0),test));
				resultat = resultat || btest;
			}
		}

		return resultat;
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

	public double Id {
		get { return id; }
		set { id = value; }
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public ArrayList Nodes {
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

	public double Temperature {
		get { return temperature; }
		set { temperature = value; }
	}

	public int NbFloor {
		get { return nbFloors; }
		set { nbFloors = value; }
	}

	public string RoofType {
		get { return roofType; }
		set { roofType = value; }
	}

	public int RoofAngle {
		get { return roofAngle; }
		set { roofAngle = value; }
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