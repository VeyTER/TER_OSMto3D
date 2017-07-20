using UnityEngine;
using System;
using System.Collections;

public class StageColorController : MonoBehaviour {
	private static float INACTIVE_MIN_BRIGHTNESS = 0.3F;
	private static float INACTIVE_MAX_BRIGHTNESS = 0.5F;

	private static float HOVERED_BRIGHTNESS = 0.65F;

	private static float PRESSED_BRIGHTNESS = 0.85F;

	public enum StageStates {
		INACTIVE,
		INACTIVE_TO_HOVERED,
		HOVERED,
		HOVERED_TO_INACTIVE,
		HOVERED_TO_PRESSED,
		PRESSED,
		PRESSED_TO_HOVERED
	}

	private StageStates stageState;

	private Material targetMaterial;
	private int colorHue;

	public void Start() {
		this.stageState = StageStates.INACTIVE;
	}

	public IEnumerator Animate() {
		float cursor = 0F;
		float brightness = INACTIVE_MIN_BRIGHTNESS;

		while (true) {
			switch (stageState) {
			case StageStates.INACTIVE:
				brightness = (float) (INACTIVE_MIN_BRIGHTNESS + ((Math.Cos(cursor) + 1) / 2F) * (INACTIVE_MAX_BRIGHTNESS - INACTIVE_MIN_BRIGHTNESS));

				cursor += 0.05F;
				if (cursor >= Mathf.PI * 2)
					cursor -= Mathf.PI * 2;
				break;

			case StageStates.INACTIVE_TO_HOVERED:
				if (brightness < HOVERED_BRIGHTNESS)
					brightness += Math.Min(0.05F, HOVERED_BRIGHTNESS - brightness);
				else
					stageState = StageStates.HOVERED;
				break;

			case StageStates.HOVERED:
				brightness = HOVERED_BRIGHTNESS;
				break;

			case StageStates.HOVERED_TO_INACTIVE:
				if (brightness > INACTIVE_MAX_BRIGHTNESS)
					brightness -= Math.Min(0.05F, brightness - INACTIVE_MAX_BRIGHTNESS);
				else
					cursor = 0F;
					stageState = StageStates.INACTIVE;
				break;

			case StageStates.HOVERED_TO_PRESSED:
				if (brightness < PRESSED_BRIGHTNESS)
					brightness += Math.Min(0.05F, PRESSED_BRIGHTNESS - brightness);
				else
					stageState = StageStates.PRESSED;
				break;

			case StageStates.PRESSED:
				brightness = PRESSED_BRIGHTNESS;
				break;

			case StageStates.PRESSED_TO_HOVERED:
				if (brightness > HOVERED_BRIGHTNESS)
					brightness -= Math.Min(0.05F, brightness - HOVERED_BRIGHTNESS);
				else
					stageState = StageStates.HOVERED;
				break;
			}

			targetMaterial.SetColor("_EmissionColor", Color.HSVToRGB(colorHue / 360F, 0.603F, brightness));
			yield return new WaitForSeconds(0.01F);
		}
	}

	public void SetInactive() {
		if (stageState == StageStates.HOVERED || stageState == StageStates.INACTIVE_TO_HOVERED)
			stageState = StageStates.HOVERED_TO_INACTIVE;
	}

	public void SetHovered() {
		if (stageState == StageStates.INACTIVE || stageState == StageStates.HOVERED_TO_INACTIVE)
			stageState = StageStates.INACTIVE_TO_HOVERED;
		else if (stageState == StageStates.PRESSED || stageState == StageStates.HOVERED_TO_PRESSED)
			stageState = StageStates.PRESSED_TO_HOVERED;
	}

	public void SetPressed() {
		if (stageState == StageStates.HOVERED || stageState == StageStates.PRESSED_TO_HOVERED)
			stageState = StageStates.HOVERED_TO_PRESSED;
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