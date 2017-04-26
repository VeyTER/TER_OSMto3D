using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandlerControler : MonoBehaviour, IBeginDragHandler, IDragHandler {
	private Vector2 initialPosition;
	private Vector2 initialOffset;

	private GameObject building;

	public void OnBeginDrag (PointerEventData eventData) {
		initialPosition = transform.localPosition;
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

		initialOffset = mousePosition - initialPosition;
	}

	public void OnDrag (PointerEventData eventData) {
		Vector2 mousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
		transform.localPosition = mousePosition - initialOffset;
	}

	public GameObject Building {
		get { return building; }
		set { building = value; }
	}
}

