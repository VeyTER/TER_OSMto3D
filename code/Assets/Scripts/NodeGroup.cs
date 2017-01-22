using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeGroup {

	protected double id;
    public ArrayList nodes;
    protected ArrayList decomposition;
    protected LinkedList <Node> list;
    public Hashtable tags;
    protected double minLon, maxLon, minLat, maxLat;
    protected string country;
    protected string region;
    protected string town;
    protected string district;
    protected double temperature;
    protected int nbFloors;
    protected string name;
    protected string typeRoof;
    protected int angleRoof;
    protected int nbNode;
    protected int nbVoie;
    protected int vitMax;
    
    //constructeur
	public NodeGroup(double id){
		this.id = id;
		this.nodes = new ArrayList ();
		this.decomposition = new ArrayList ();
		this.tags = new Hashtable ();
		this.list = new LinkedList<Node> ();
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
        this.name = "unknown";
        this.typeRoof = "unknown";
        this.angleRoof = 0;
        this.nbNode = 0;
        this.nbVoie = 1;
        this.vitMax = 50;
    }

    //Surcharge du constructeur
    public NodeGroup(double id,string country, string region, string town, string district, string name, string type, int angle, int nbVoie, int vitMax)
    {
        this.id = id;
        this.nodes = new ArrayList();
        this.decomposition = new ArrayList();
        this.tags = new Hashtable();
        this.list = new LinkedList<Node>();
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
        this.name = name;
        this.angleRoof = angle;
        this.typeRoof = type;
        this.nbVoie = nbVoie;
        this.vitMax = vitMax;
    }

    // ajoute une node à l'ensemble 
    public void addNode(Node n){
		this.nodes.Add (n);
        this.nbNode++;
	}

    //supprime la n ieme valeur de node 
    public void removeNode(int n)
    {
        this.nodes.RemoveAt(n);
        this.nbNode--;
    }

    // retourne la node demandée 
    public Node getNode(int i){
		return (Node)this.nodes [i];
	}

	// ajoute un tag au NodeGroup
	public void addTag(string key, string value){
		this.tags.Add (key, value);
	}

	// retourne vrai si le nodeGroup correspond à un batiment
	public bool isBuilding()
    {
		return this.tags.ContainsKey ("building");
	}

    // retourne vrai si le NodeGroup correspond à un arbre
    public bool isTree()
    {
        return this.tags.ContainsValue("tree");
    }

    // retourne vrai si le NodeGroup correspond à un feu tricolore
    public bool isFeuTri()
    {
        return this.tags.ContainsValue("traffic_signals");
    }

    // retourne vrai si le nodeGroup correspond à une route
    public bool isHighway()
    {
		return this.tags.ContainsKey ("highway");
	}

	// vrai si on a une route de type "primaire"
	public bool isPrimary(){
		return this.tags.ContainsValue ("primary");
	}

	// vrai si on a une route de type "secondaire"
	public bool isSecondary(){
		return this.tags.ContainsValue ("secondary");
	}

	// vrai si on a une route de type "tertiaire"
	public bool isTertiary(){
		return this.tags.ContainsValue ("tertiary");
	}

	// vrai si on a une route de type "non classifiée"
	public bool isUnclassified(){
		return this.tags.ContainsValue ("unclassified");
	}

	// vrai si on a une route de type "résidentielle"
	public bool isResidential(){
		return this.tags.ContainsValue ("residential");
	}

	// vrai si on a une route de type "service"
	public bool isService(){
		return this.tags.ContainsValue ("service");
	}

	// vrai si on a une route de type "chemin piétons"
	public bool isFootway(){
		return this.tags.ContainsValue ("footway");
	}

	// teste l'egalité de deux NodeGroup
	public bool equals (NodeGroup ng){
		return this.id == ng.id;
	}

	// renvoie la valeur du tag si la clé existe
	public string GetTagValue(string key){
		if (this.tags.ContainsKey(key)) {
			return this.tags [key].ToString ();
		} else {
			return key + "_unknown";
		}
	}
	

	// teste si un point est à gauche ou a droite d'un vecteur
	public bool isAtRight( Node nodeA, Node nodeB, Node nodeTest){

		double determinant, abx, aby, dx, dy;

		abx = nodeB.getLatitude() - nodeA.getLatitude();
		aby = nodeB.getLongitude() - nodeA.getLongitude();
		dx = nodeTest.getLatitude() - nodeA.getLatitude();
		dy = nodeTest.getLongitude() - nodeA.getLongitude();

		determinant = abx * dy - aby * dx;

		return (determinant >= 0);
	}


	// Décompose le polygone en triangles
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

    // Décompose le polygone en triangles
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

	// met a jour la longitude et getLatitude() min et max
	public void setBoundaries(){

		this.minLon = this.getNode(0).getLongitude();
		this.maxLon = this.getNode(0).getLongitude();
		this.minLat = this.getNode(0).getLatitude();
		this.maxLat = this.getNode(0).getLatitude();


		foreach (Node n in this.nodes) {
			if(n.getLatitude() > this.maxLat) maxLat = n.getLatitude();
			if(n.getLatitude() < this.minLat) minLat = n.getLatitude();
			if(n.getLongitude() > this.maxLon) maxLon = n.getLongitude();
			if(n.getLongitude() < this.minLon) minLon = n.getLongitude();
		}
	
	}

    // Accesseurs de l'attribut de country
    public void setCountry(string country)
    {
        this.country = country;
    }
    public string getCountry()
    {
        return this.country;
    }

    // Accesseurs de l'attribut de region
    public void setRegion(string region)
    {
        this.region = region;
    }
    public string getRegion()
    {
        return this.region;
    }

    // Accesseurs de l'attribut de town
    public void setTown(string town)
    {
        this.town = town;
    }
    public string getTown()
    {
        return this.town;
    }

    // Accesseurs de l'attribut de district
    public void setDistrict(string district)
    {
        this.district = district;
    }
    public string getDistrict()
    {
        return this.district;

    }

    // Accesseurs de l'attribut de temperature
    public void setTemperature(double temperature)
    {
        this.temperature = temperature;
    }
    public double getTemperature()
    {
        return this.temperature;
    }

    // Accesseurs de l'attribut de nbFloors
    public void setNbFloors(int nbFloors)
    {
        this.nbFloors = nbFloors;
    }
    public int getNbFloors()
    {
        return this.nbFloors;
    }

    // Accesseurs de l'attribut de name
    public void setName(string name)
    {
        this.name = name;
    }
    public string getName()
    {
        return this.name;

    }

    //Accesseurs de l'attribut de typeRoof
    public void setType(string type)
    {
        this.typeRoof = type;
    }
    public string getType()
    {
        return this.typeRoof;
    }

    //Accesseurs de l'attribut d'angleRoof
    public void setAngle(int angle)
    {
        this.angleRoof = angle;
    }
    public int getAngle()
    {
        return this.angleRoof;
    }

    // Accesseurs de l'attribut de id
    public void setID(double ident)
    {
        this.id = ident;
    }
    public double getID()
    {
        return this.id;
    }

    // Accesseurs de l'attribut de nbNode
    public void setNbNode(int num)
    {
        this.nbNode = num;
    }
    public int getNbNode()
    {
        return this.nbNode;
    }

    // Accesseurs de l'attribut de nbVoie
    public void setNbVoie(int num)
    {
        this.nbVoie = num;
    }
    public int getNbVoie()
    {
        return this.nbVoie;
    }

    // Accesseurs de l'attribut de vitMax
    public void setVitMax(int num)
    {
        this.vitMax = num;
    }
    public int getVitMax()
    {
        return this.vitMax;
    }
}

