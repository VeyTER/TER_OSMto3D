using System.Collections.Generic;
using UnityEngine;

public class ControlPanelManager {
	public enum ControlStates { NONE, BUILDING_CREATION, VISIBILITY_TOGGLELING }
	private ControlStates controlState;

	private Dictionary<GameObject, PanelController> panelsBehaviours;

	private ControlPanelManager() {
		this.panelsBehaviours = new Dictionary<GameObject, PanelController>();
	}

	public static ControlPanelManager GetInstance() {
		return ControlPanelManagerHolder.instance;
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

	private class ControlPanelManagerHolder {
		internal static ControlPanelManager instance = new ControlPanelManager();
	}

	public bool AllPanelsClosed() {
		return controlState == ControlStates.NONE;
	}

	public ControlStates ControlState {
		get { return controlState; }
		set { controlState = value; }
	}
}
