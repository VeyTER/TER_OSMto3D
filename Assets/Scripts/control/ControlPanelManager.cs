using System.Collections.Generic;
using UnityEngine;

public class ControlPanelManager {
	private static ControlPanelManager instance;

	public enum ControlStates { NONE, BUILDING_CREATION, VISIBILITY_TOGGLELING }
	private ControlStates controlState;

	private Dictionary<GameObject, PanelController> panelsBehaviours;

	private ControlPanelManager() {
		this.panelsBehaviours = new Dictionary<GameObject, PanelController>();
	}

	public static ControlPanelManager GetInstance() {
		if (instance == null)
			instance = new ControlPanelManager();
		return instance;
	}

	public void AddPanel(GameObject panel) {
		panelsBehaviours[panel] = panel.GetComponent<PanelController>();
	}

	public PanelController GetPanelController(string panelName) {
		GameObject matchingPanel = null;
		foreach (KeyValuePair<GameObject, PanelController> currentBehaviourEntry in panelsBehaviours) {
			if (currentBehaviourEntry.Key.name.Equals(panelName)) {
				matchingPanel = currentBehaviourEntry.Key;
			}
		}
		return panelsBehaviours[matchingPanel];
	}

	public PanelController GetPanelController(GameObject panel) {
		return panelsBehaviours[panel];
	}

	public bool AllPanelsClosed() {
		return controlState == ControlStates.NONE;
	}

	public ControlStates ControlState {
		get { return controlState; }
		set { controlState = value; }
	}
}
