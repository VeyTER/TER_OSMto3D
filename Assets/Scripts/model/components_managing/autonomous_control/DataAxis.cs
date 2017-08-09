using UnityEngine;
using UnityEditor;

public class DataAxis {
	private float minValue;
	private float maxValue;

	private float step;

	public DataAxis(float minValue, float maxValue, float step) {
		this.minValue = minValue;
		this.maxValue = maxValue;

		this.step = step;
	}

	public float Range() {
		return maxValue - maxValue;
	}

	public float MinValue {
		get { return minValue; }
		set { minValue = value; }
	}

	public float MaxValue {
		get { return maxValue; }
		set { maxValue = value; }
	}

	public float Step {
		get { return step; }
		set { step = value; }
	}
}