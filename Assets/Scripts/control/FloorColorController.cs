using UnityEngine;
using System;
using System.Collections;

public class FloorColorController : MonoBehaviour {
	private static float INACTIVE_MIN_BRIGHTNESS = 0.3F;
	private static float INACTIVE_MAX_BRIGHTNESS = 0.5F;

	private static float HOVERED_BRIGHTNESS = 0.65F;

	private static float PRESSED_BRIGHTNESS = 0.85F;

	public enum FloorStates { INACTIVE, INACTIVE_TO_HOVERED, HOVERED, HOVERED_TO_INACTIVE,
		HOVERED_TO_PRESSED, PRESSED, PRESSED_TO_HOVERED }

	private FloorStates floorState;

	private Material targetMaterial;
	private int colorHue;

	public void Start() {
		this.floorState = FloorStates.INACTIVE;
	}

	public IEnumerator Animate() {
		float cursor = 0F;
		float brightness = INACTIVE_MIN_BRIGHTNESS;

		while (true) {
			switch (floorState) {
			case FloorStates.INACTIVE:
				brightness = (float) (INACTIVE_MIN_BRIGHTNESS + ((Math.Cos(cursor) + 1) / 2F) * (INACTIVE_MAX_BRIGHTNESS - INACTIVE_MIN_BRIGHTNESS));

				cursor += 0.05F;
				if (cursor >= Mathf.PI * 2)
					cursor -= Mathf.PI * 2;

				break;

			case FloorStates.INACTIVE_TO_HOVERED:
				if (brightness < HOVERED_BRIGHTNESS) {
					brightness += Math.Min(0.05F, HOVERED_BRIGHTNESS - brightness);
				} else {
					floorState = FloorStates.HOVERED;
				}
				break;

			case FloorStates.HOVERED:
				brightness = HOVERED_BRIGHTNESS;
				break;

			case FloorStates.HOVERED_TO_INACTIVE:
				if (brightness > INACTIVE_MAX_BRIGHTNESS) {
					brightness -= Math.Min(0.05F, brightness - INACTIVE_MAX_BRIGHTNESS);
				} else {
					cursor = 0F;
					floorState = FloorStates.INACTIVE;
				}
				break;

			case FloorStates.HOVERED_TO_PRESSED:
				if (brightness < PRESSED_BRIGHTNESS) {
					brightness += Math.Min(0.05F, PRESSED_BRIGHTNESS - brightness);
				} else {
					floorState = FloorStates.PRESSED;
				}
				break;

			case FloorStates.PRESSED:
				brightness = PRESSED_BRIGHTNESS;
				break;

			case FloorStates.PRESSED_TO_HOVERED:
				if (brightness > HOVERED_BRIGHTNESS) {
					brightness -= Math.Min(0.05F, brightness - HOVERED_BRIGHTNESS);
				} else {
					floorState = FloorStates.HOVERED;
				}
				break;
			}

			targetMaterial.SetColor("_EmissionColor", Color.HSVToRGB(colorHue / 360F, 0.603F, brightness));
			yield return new WaitForSeconds(0.01F);
		}
	}

	public void SetInactive() {
		if (floorState == FloorStates.HOVERED || floorState == FloorStates.INACTIVE_TO_HOVERED)
			floorState = FloorStates.HOVERED_TO_INACTIVE;
	}

	public void SetHovered() {
		if (floorState == FloorStates.INACTIVE || floorState == FloorStates.HOVERED_TO_INACTIVE)
			floorState = FloorStates.INACTIVE_TO_HOVERED;
		else if (floorState == FloorStates.PRESSED || floorState == FloorStates.HOVERED_TO_PRESSED)
			floorState = FloorStates.PRESSED_TO_HOVERED;
	}

	public void SetPressed() {
		if (floorState == FloorStates.HOVERED || floorState == FloorStates.PRESSED_TO_HOVERED)
			floorState = FloorStates.HOVERED_TO_PRESSED;
	}


	public Material TargetMaterial {
		get { return targetMaterial; }
		set { targetMaterial = value; }
	}

	public int ColorHue {
		get { return colorHue; }
		set { colorHue = value; }
	}
}