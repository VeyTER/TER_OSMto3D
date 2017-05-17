using System;
using UnityEngine;
using System.Collections;

public class ObjectEditor {
	/// <summary>Mur courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedWall;

	/// <summary>Bâtiment courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedBuilding;


	/// <summary>Nodes 3D correspondant au mur courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedWallNodes;

	/// <summary>Nodes 3D correspondant au bâtiment courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedBuildingNodes;


	/// <summary>Témoin de transformation d'un mur.</summary>
	protected bool wallEdited;

	/// <summary>Témoin de transformation d'un bâtiment.</summary>
	protected bool buildingEdited;


	public ObjectEditor () {
		this.selectedWall = null;
		this.selectedBuilding = null;

		this.selectedWallNodes = null;
		this.selectedBuildingNodes = null;

		this.wallEdited = false;
		this.buildingEdited = false;
	}


	/// <summary>
	/// 	Prépare la transformation en initialisant les attributs.
	/// </summary>
	/// <param name="selectedWall">Selected wall.</param>
	/// <param name="selectedBuilding">Selected building.</param>
	public void Initialize(GameObject selectedWall, GameObject selectedBuilding) {
		// Initialisation des objets sélectionnés
		this.SelectedWall = selectedWall;
		this.SelectedBuilding = selectedBuilding;

		// Initialisation des nodes correspondant au bâtiment sélectionné (les mus n'étant pas gérés)
		BuildingsTools buildingsTools = BuildingsTools.GetInstance ();
		SelectedBuildingNodes = buildingsTools.BuildingToBuildingNodeGroup (selectedBuilding);

		// Initialisation des témoins de transformation
		WallTransformed = false;
		BuildingTransformed = false;
	}

	public GameObject SelectedWall {
		get { return selectedWall; }
		set { selectedWall = value; }
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public GameObject SelectedWallNodes {
		get { return selectedWallNodes; }
		set { selectedWallNodes = value; }
	}
	
	public GameObject SelectedBuildingNodes {
		get { return selectedBuildingNodes; }
		set { selectedBuildingNodes = value; }
	}

	public bool WallTransformed {
		get { return wallEdited; }
		set { wallEdited = value; }
	}

	public bool BuildingTransformed {
		get { return buildingEdited; }
		set { buildingEdited = value; }
	}
}