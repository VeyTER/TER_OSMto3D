using System.Collections.Generic;

public class ControlPanelManager {
	private enum ControlStates { NONE, BUILDING_CREATION, VISIBILITY_TOGGLELING }
	private ControlStates controlState;

	private List<PanelController> panelControllers;
}
