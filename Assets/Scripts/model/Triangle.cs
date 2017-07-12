using System;


public class Triangle {
	private Node nodeA;
	private Node nodeB;
	private Node nodeC;

	public Triangle(Node nodeA, Node nodeB, Node nodeC) {
		this.nodeA = nodeA;
		this.nodeB = nodeB;
		this.nodeC = nodeC;
	}

	public Node NodeA {
		get { return nodeA; }
	}

	public Node NodeB {
		get { return nodeB; }
	}

	public Node NodeC {
		get { return nodeC; }
	}
}
