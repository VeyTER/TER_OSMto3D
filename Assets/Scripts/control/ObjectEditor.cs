using UnityEngine;
using System.Collections.Generic;

public abstract class ObjectEditor {
	/// <summary>Bâtiment courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedBuilding;

	/// <summary>Nodes 3D correspondant au bâtiment courant sélectionné par l'utilsiateur.</summary>
	protected GameObject selectedBuildingNodes;

	/// <summary>Objets transformés durant la période de modification.</summary>
	protected List<GameObject> transformedObjects;

	/// <summary>Témoin de transformation d'un bâtiment.</summary>
	protected bool isBuildingTransformed;

	protected BuildingsTools buildingsTools;
	protected CityBuilder cityBuilder;

	public ObjectEditor () {
		this.selectedBuilding = null;
		this.selectedBuildingNodes = null;

		this.transformedObjects = new List<GameObject>();
		this.isBuildingTransformed = false;

		this.buildingsTools = BuildingsTools.GetInstance();
		this.cityBuilder = CityBuilder.GetInstance();
	}


	/// <summary>
	/// 	Prépare la transformation en initialisant les attributs.
	/// </summary>
	/// <param name="selectedWall">Selected wall.</param>
	/// <param name="selectedBuilding">Selected building.</param>
	public void InitializeBasics(GameObject selectedBuilding) {
		// Initialisation des objets sélectionnés
		this.selectedBuilding = selectedBuilding;

		// Initialisation des nodes correspondant au bâtiment sélectionné (les mus n'étant pas gérés)
		selectedBuildingNodes = buildingsTools.BuildingToBuildingNodeGroup (selectedBuilding);

		// Initialisation des témoins de transformation
		isBuildingTransformed = false;
	}

	public abstract void ValidateTransform();
	public abstract void CancelTransform();

	public void ClearHistory() {
		transformedObjects.Clear();
	}

	public GameObject SelectedBuilding {
		get { return selectedBuilding; }
		set { selectedBuilding = value; }
	}

	public GameObject SelectedBuildingNodes {
		get { return selectedBuildingNodes; }
		set { selectedBuildingNodes = value; }
	}

	public bool IsBuildingTransformed {
		get { return isBuildingTransformed; }
		set { isBuildingTransformed = value; }
	}
}