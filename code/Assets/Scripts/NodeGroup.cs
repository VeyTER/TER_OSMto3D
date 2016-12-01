using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeGroup {

	public long id;
	public ArrayList nodes;
	public ArrayList decomposition;
	public LinkedList <Node> list;
	public Hashtable tags;
	public float minLon, maxLon, minLat, maxLat;

	public NodeGroup(long id){
		this.id = id;
		this.nodes = new ArrayList ();
		this.decomposition = new ArrayList ();
		this.tags = new Hashtable ();
		this.list = new LinkedList<Node> ();
		this.minLon = 0;
		this.maxLon = 0;
		this.minLat = 0;
		this.maxLat = 0;
	}

	// ajoute une node à l'ensemble 
	public void addNode(Node n){
		this.nodes.Add (n);
	}

	// retourne la node demandée 
	public Node getNode(int i){
		return (Node)this.nodes [i];
	}

	// ajoute un tag au NodeGroup
	public void addTag(string key, string value){
		this.tags.Add (key, value);
	}

	// 
	public bool isBuilding(){
		return this.tags.ContainsKey ("building");
	}

	//teste l'egalité de deux NodeGroup
	public bool equals (NodeGroup ng){
		return this.id == ng.id;
	}

	// renvoie la valeur du tag si la clé exitste
	public string GetTagValue(string key){
		if (this.tags.ContainsKey (key)) {
			return this.tags [key].ToString ();
		} else {
			return key + "_unknown";
		}
	}
	

	// teste si un point est à gauche ou a droite d'un vecteur
	public bool isAtRight( Node nodeA, Node nodeB, Node nodeTest){

		float determinant, abx, aby, dx, dy;

		abx = nodeB.latitude - nodeA.latitude;
		aby = nodeB.longitude - nodeA.longitude;
		dx = nodeTest.latitude - nodeA.latitude;
		dy = nodeTest.longitude - nodeA.longitude;

		determinant = abx * dy - aby * dx;

		return (determinant >= 0);
	
	}


	//décompose le polygone en triangles
	public bool decomposerRight(){
	
		LinkedListNode<Node> cur = new LinkedListNode<Node> (this.getNode (0));
		int i;

		//on ajoute le premier élément
		this.list.AddFirst (cur);

		for (i=1; i<this.nodes.Count-1; i++) {
			this.list.AddAfter(cur,new LinkedListNode<Node> (this.getNode (i)));
			cur = cur.Next;
		}

		LinkedListNode<Node> nodeA, nodeB, nodeTest;

		nodeA = this.list.First;
		nodeB = nodeA.Next;
		nodeTest = nodeB.Next;
		NodeGroup ng = null;
		bool sens = true;

		i = 0;
		while (this.list.Count > 3 && sens ) {
			// si l'angle est ok on ajoute le triangle et on supprime le point de la liste
			if(isAtRight(nodeA.Value,nodeB.Value,nodeTest.Value)){
				// on créé le sous triangle
				ng = new NodeGroup(0);
				ng.addNode(nodeA.Value);
				ng.addNode(nodeB.Value);
				ng.addNode(nodeTest.Value);
				decomposition.Add(ng);

				// on supprime le point
				this.list.Remove(nodeB.Value);

				// on avance en cercle
				nodeA = nodeTest;
				if(nodeA.Next == null){
					nodeB = this.list.First;
				}
				else{
					nodeB = nodeA.Next;
				}

				if(nodeB.Next == null){
					nodeTest = this.list.First;
				}
				else{
					nodeTest = nodeB.Next;
				}
			}
			else{
				nodeA = nodeB;
				if(nodeA.Next == null){
					nodeB = this.list.First;
				}
				else{
					nodeB = nodeA.Next;
				}
				
				if(nodeB.Next == null){
					nodeTest = this.list.First;
				}
				else{
					nodeTest = nodeB.Next;
				}
			}

			i++;
			if(i > 150){
				sens = false;
			}
		}

		// on ajoute le dernier triangle
		ng = new NodeGroup(0);
		ng.addNode(nodeA.Value);
		ng.addNode(nodeB.Value);
		ng.addNode(nodeTest.Value);
		decomposition.Add(ng);

		return sens;
	}

	public bool decomposerLeft(){

		this.list.Clear ();
		LinkedListNode<Node> cur = new LinkedListNode<Node> (this.getNode (0));
		int i;
		
		//on ajoute le premier élément
		this.list.AddFirst (cur);
		
		for (i=1; i<this.nodes.Count-1; i++) {
			this.list.AddAfter(cur,new LinkedListNode<Node> (this.getNode (i)));
			cur = cur.Next;
		}
		
		LinkedListNode<Node> nodeA, nodeB, nodeTest;
		
		nodeA = this.list.First;
		nodeB = nodeA.Next;
		nodeTest = nodeB.Next;
		NodeGroup ng = null;
		bool sens = true;
		
		i = 0;
		while (this.list.Count > 3 && sens ) {
			// si l'angle est ok on ajoute le triangle et on supprime le point de la liste
			if(!isAtRight(nodeA.Value,nodeB.Value,nodeTest.Value)){
				// on créé le sous triangle
				ng = new NodeGroup(1);
				ng.addNode(nodeA.Value);
				ng.addNode(nodeB.Value);
				ng.addNode(nodeTest.Value);
				decomposition.Add(ng);
				
				// on supprime le point
				this.list.Remove(nodeB.Value);
				
				// on avance en cercle
				nodeA = nodeTest;
				if(nodeA.Next == null){
					nodeB = this.list.First;
				}
				else{
					nodeB = nodeA.Next;
				}
				
				if(nodeB.Next == null){
					nodeTest = this.list.First;
				}
				else{
					nodeTest = nodeB.Next;
				}
			}
			else{
				nodeA = nodeB;
				if(nodeA.Next == null){
					nodeB = this.list.First;
				}
				else{
					nodeB = nodeA.Next;
				}
				
				if(nodeB.Next == null){
					nodeTest = this.list.First;
				}
				else{
					nodeTest = nodeB.Next;
				}
			}
			
			i++;
			if(i > 150){
				sens = false;
			}
		}
		
		// on ajoute le dernier triangle
		ng = new NodeGroup(1);
		ng.addNode(nodeA.Value);
		ng.addNode(nodeB.Value);
		ng.addNode(nodeTest.Value);
		decomposition.Add(ng);
		
		return sens;
	}





	// teste l'appartenance d'un point à un nodeGroup
	public bool appartient( Node test ){

		bool resultat = false;
		bool btest = false;

		foreach (NodeGroup ng in this.decomposition) {
			// on teste pour savoir dans quel sens on doit vérifier les points ( droite ou gauche )
			if(ng.id == 0 ){
				btest = (this.isAtRight(ng.getNode(0),ng.getNode(1),test) && this.isAtRight(ng.getNode(1),ng.getNode(2),test) && this.isAtRight(ng.getNode(2),ng.getNode(0),test));
				resultat = resultat || btest;
			}
			else{
				btest = (!this.isAtRight(ng.getNode(0),ng.getNode(1),test) && !this.isAtRight(ng.getNode(1),ng.getNode(2),test) && !this.isAtRight(ng.getNode(2),ng.getNode(0),test));
				resultat = resultat || btest;
			}

		}

		return resultat;
	}


	// met a jour la longitude et latitude min et max
	public void setBoundaries(){

		this.minLon = this.getNode(0).longitude;
		this.maxLon = this.getNode(0).longitude;
		this.minLat = this.getNode(0).latitude;
		this.maxLat = this.getNode(0).latitude;


		foreach (Node n in this.nodes) {
			if(n.latitude > this.maxLat) maxLat = n.latitude;
			if(n.latitude < this.minLat) minLat = n.latitude;
			if(n.longitude > this.maxLon) maxLon = n.longitude;
			if(n.longitude < this.minLon) minLon = n.longitude;
		}
	
	}











}
