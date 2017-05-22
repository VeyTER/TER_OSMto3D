using System;

public class HeightChangingEditor : ObjectEditor {
	private ObjectBuilder objectBuilder;

	public HeightChangingEditor () {
		this.objectBuilder = ObjectBuilder.GetInstance();
	}

	public void InitializeTurningMode(EditionController.SelectionRanges selectionRange) {

	}

	private void ChangeObjectHeight(EditionController.SelectionRanges selectionRange) {
		objectBuilder.RebuildBuilding(selectedBuilding, -100);
	}
}
