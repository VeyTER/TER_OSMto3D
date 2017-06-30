using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class SensorThreshold {
	private ThresholdConditions condition;

	private double minValue;
	private double maxValue;

	private List<string> fixedValues;

	public SensorThreshold() {
		this.condition = ThresholdConditions.EQUALS;

		this.minValue = double.MinValue;
		this.maxValue = double.MaxValue;

		this.fixedValues = new List<string>();
	}

	public SensorThreshold(ThresholdConditions condition, string fixedValue) {
		this.condition = condition;

		this.minValue = double.MinValue;
		this.maxValue = double.MaxValue;

		this.fixedValues = new List<string> {
			fixedValue
		};
	}

	public SensorThreshold(ThresholdConditions condition, double minValue, double maxValue) {
		this.condition = condition;

		this.minValue = minValue;
		this.maxValue = maxValue;

		this.fixedValues = new List<string>();
	}

	public void AddFixedValue(string fixedValue) {
		fixedValues.Add(fixedValue);
	}

	public void RemoveFixedValue(string fixedValue) {
		fixedValues.Remove(fixedValue);
	}

	public bool ValueOutOfThreshold(string value) {
		double sensorNumericalValue = double.MinValue;
		bool sensorValueParsingOutcome = false;

		switch (condition) {
		case ThresholdConditions.EQUALS:
			int i = 0;
			for (; i < fixedValues.Count && !value.Equals(fixedValues[i]); i++) ;
			return i == fixedValues.Count;

		case ThresholdConditions.NOT_EQUALS:
			int j = 0;
			for (; j < fixedValues.Count && !value.Equals(fixedValues[j]); j++) ;
			return j < fixedValues.Count;

		case ThresholdConditions.IN:
			sensorValueParsingOutcome = double.TryParse(value, out sensorNumericalValue);
			if (sensorValueParsingOutcome) {
				return !(sensorNumericalValue >= minValue && sensorNumericalValue <= maxValue);
			} else {
				throw new InvalidCastException("La valeur du capteur doit être un nombre pour être compris entre deux valeurs.");
			}

		case ThresholdConditions.OUT:
			sensorValueParsingOutcome = double.TryParse(value, out sensorNumericalValue);
			if (sensorValueParsingOutcome) {
				return !(sensorNumericalValue < minValue && sensorNumericalValue > maxValue);
			} else {
				throw new InvalidCastException("La valeur du capteur doit être un nombre pour être hors de deux valeurs.");
			}
		default:
			return false;
		}
	}

	public ThresholdConditions Condition {
		get { return condition; }
		set { condition = value; }
	}

	public double MinValue {
		get { return minValue; }
		set { minValue = value; }
	}

	public double MaxValue {
		get { return maxValue; }
		set { maxValue = value; }
	}

	public List<string> FixedValues {
		get { return fixedValues; }
	}
}