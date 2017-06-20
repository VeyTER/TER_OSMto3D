using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

// Attaché au bâtiment
public class BuildingDataDisplayController : MonoBehaviour {

	private void Start() {
		UiBuilder uiBuilder = UiBuilder.GetInstance();
		uiBuilder.BuildBuildingDataDisplay(gameObject);
	}
}