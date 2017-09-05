public class ActuatorController : RoomDevice {
	private double minValue;
	private double maxValue;

	private double defaultStep;

	private string actuatorButtonsPathPattern;

	public ActuatorController(uint index, string buildingName, string roomName, string xmlIdentifier, string value, string unit, string actuatorButtonsPathPattern) :
			base(index, buildingName, roomName, xmlIdentifier, value, unit) {

		this.minValue = 0;
		this.maxValue = 100;

		this.defaultStep = 0.5;

		this.actuatorButtonsPathPattern = actuatorButtonsPathPattern;
	}

	public ActuatorController(uint index, string buildingName, string roomName, string actuatorIdentifier, string value, string unit, string actuatorButtonsPathPattern, double minValue, double maxValue, double defaultStep) :
			base(index, buildingName, roomName, actuatorIdentifier, value, unit) {

		this.minValue = minValue;
		this.maxValue = maxValue;

		this.defaultStep = defaultStep;

		this.actuatorButtonsPathPattern = actuatorButtonsPathPattern;
	}

	public double MinValue {
		get { return minValue; }
		set { minValue = value; }
	}

	public double MaxValue {
		get { return maxValue; }
		set { maxValue = value; }
	}

	public double DefaultStep {
		get { return defaultStep; }
		set { defaultStep = value; }
	}

	public string ActuatorButtonsPathPattern {
		get { return actuatorButtonsPathPattern; }
		set { actuatorButtonsPathPattern = value; }
	}

	public override string ToString() {
		return xmlIdentifier + " : " + "[" + minValue + ":" + defaultStep + ":" + maxValue + "]";
	}
}