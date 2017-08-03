using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>
/// 	Suite d'outils destinés au chargement et au stockage des différents objects d'une carte OSM.
/// </summary>
public class MapLoader {
	private static MapLoader instance;

	private NodeGroupBase nodeGroupBase;
	private OsmTools osmTools;

	private MapFileManager mapFileManager;
	private SettingsFileManager settingsFileManager;
	private ResumeFileManager resumeFileManager;
	private CustomFileManager customFileManager;

	private MapLoader() {
		this.nodeGroupBase = NodeGroupBase.GetInstance();
		this.osmTools = OsmTools.GetInstance();

		this.mapFileManager = new MapFileManager();
		this.settingsFileManager = new SettingsFileManager();
		this.resumeFileManager = new ResumeFileManager();
		this.customFileManager = new CustomFileManager();
	}

	public static MapLoader GetInstance() {
		if (instance == null)
			instance = new MapLoader();
		return instance;
	}

	public void LoadMap(string mapName) {
		mapFileManager.LoadOsmData(mapName);

		settingsFileManager.LoadSettingsData();

		resumeFileManager.GenerateResumeFile();

		customFileManager.LoadCustomData();

		nodeGroupBase.NodeGroups.Clear();

		resumeFileManager.LoadResumedData();
	}
}