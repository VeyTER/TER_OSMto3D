using UnityEngine;
using System;
using System.Reflection;

public class ToggleGrid
{
	private static Type m_annotationUtility;
	private static PropertyInfo m_showGridProperty;

	private static PropertyInfo ShowGridProperty
	{
		get {
			if (m_showGridProperty == null) {
				m_annotationUtility = Type.GetType ("UnityEditor.AnnotationUtility,UnityEditor.dll");
				m_showGridProperty = m_annotationUtility.GetProperty ("showGrid", BindingFlags.Static | BindingFlags.NonPublic);
			}
			return m_showGridProperty;
		}
	}

	public static bool ShowGrid
	{
		get {
			return(bool)ShowGridProperty.GetValue (null, null);
		}
		set {
			ShowGridProperty.SetValue (null, value, null);
		}
	}

	public class TestShowGrid : MonoBehaviour
	{
		[ContextMenu("Toggle Grid")]
		public void Toggle()
		{
			ToggleGrid.ShowGrid = !ToggleGrid.ShowGrid;
		}
	}
}