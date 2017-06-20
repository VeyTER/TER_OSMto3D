using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

public class UiBuilder {

	private UiBuilder() {}

	public static UiBuilder GetInstance() {
		return UiBuilderHolder.instance;
	}

	public void BuildBuildingDataDisplay(GameObject building) {
		const float LINK_WIDTH = 0.0035F;
		const float LINK_HEIGHT = 1;

		const float ICON_SIZE = 0.1F;

		GameObject linkPanel = this.BuildBuildingDataLink(building, LINK_WIDTH, LINK_HEIGHT);
		this.BuildBuildingDataIcon(linkPanel, LINK_HEIGHT, ICON_SIZE);
	}

	private GameObject BuildBuildingDataLink(GameObject building, float linkWidth, float linkHeight) {
		Transform buildingFirstWallTransform = building.transform.GetChild(0);
		Vector3 buildingPosition = building.transform.position;
		float buildingHeight = buildingFirstWallTransform.localScale.y;

		GameObject link = new GameObject("Link", typeof(MeshFilter), typeof(MeshRenderer));
		link.transform.SetParent(building.transform, false);

		link.transform.position = new Vector3(buildingPosition.x, buildingHeight, buildingPosition.z);

		MeshFilter linkMeshRenderer = link.GetComponent<MeshFilter>();
		linkMeshRenderer.mesh.vertices = new Vector3[] {
			new Vector3(-linkWidth / 2F, 0, 0),
			new Vector3(-linkWidth / 2F, linkHeight, 0),
			new Vector3(linkWidth, linkHeight, 0),
			new Vector3(linkWidth, 0, 0),
		};
		linkMeshRenderer.mesh.triangles = new int[]{
			1, 0, 2,
			2, 0, 3
		};

		MeshRenderer linkRenderer = link.GetComponent<MeshRenderer>();
		linkRenderer.material = new Material(Shader.Find("Sprites/Default")) {
			color = ThemeColors.BLUE
		};

		link.transform.parent = building.transform.parent.parent;

		return link;
	}

	private GameObject BuildBuildingDataIcon(GameObject link, float linkHeight, float iconSize) {
		Vector3 linkPanelPosition = link.transform.position;

		GameObject iconBackground = new GameObject("IconBackground", typeof(MeshFilter), typeof(MeshRenderer));
		GameObject icon = new GameObject("Icon", typeof(MeshFilter), typeof(MeshRenderer));

		iconBackground.transform.SetParent(icon.transform, false);
		icon.transform.SetParent(link.transform, false);
		icon.transform.position = new Vector3(linkPanelPosition.x, linkPanelPosition.y + linkHeight + iconSize / 2F, linkPanelPosition.z);
		icon.transform.rotation = Quaternion.Euler(0, 0, 90);

		MeshFilter iconBackgroundShape = iconBackground.GetComponent<MeshFilter>();
		iconBackgroundShape.mesh.vertices = new Vector3[] {
			new Vector3(-iconSize / 2F, -iconSize / 2F, 0),
			new Vector3(-iconSize / 2F, iconSize / 2F, 0),
			new Vector3(iconSize / 2F, iconSize / 2F, 0),
			new Vector3(iconSize / 2F, -iconSize / 2F, 0),
		};
		iconBackgroundShape.mesh.triangles = new int[]{
			1, 0, 2,
			2, 0, 3
		};
		iconBackgroundShape.mesh.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (1, 1),
			new Vector2 (0, 1)
		};

		MeshFilter iconShape = icon.GetComponent<MeshFilter>();
		iconShape.mesh.vertices = new Vector3[] {
			new Vector3(-iconSize / 2F, -iconSize / 2F, 0),
			new Vector3(-iconSize / 2F, iconSize / 2F, 0),
			new Vector3(iconSize / 2F, iconSize / 2F, 0),
			new Vector3(iconSize / 2F, -iconSize / 2F, 0),
		};
		iconShape.mesh.triangles = new int[]{
			1, 0, 2,
			2, 0, 3
		};
		iconShape.mesh.uv = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (1, 1),
			new Vector2 (0, 1)
		};

		MeshRenderer iconBackgroundRenderer = iconBackground.GetComponent<MeshRenderer>();
		Texture buildingDataIconBackgroundTexture = Resources.Load<Texture>(IconsAndTextures.BUILDING_DATA_ICON_BACKGROUND);
		iconBackgroundRenderer.material = new Material(Shader.Find("Sprites/Default")) {
			mainTexture = buildingDataIconBackgroundTexture
		};

		MeshRenderer iconRenderer = icon.GetComponent<MeshRenderer>();
		Texture buildingDataIconTexture = Resources.Load<Texture>(IconsAndTextures.BUILDING_DATA_ICON);
		iconRenderer.material = new Material(Shader.Find("Sprites/Default")) {
			mainTexture = buildingDataIconTexture
		};

		return iconBackground;
	}

	private class UiBuilderHolder {
		internal static UiBuilder instance = new UiBuilder();
	}
}
