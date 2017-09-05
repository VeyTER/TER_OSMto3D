using UnityEngine;

public class CommandModule {
	private string sensorPath;
	private string actuatorPath;

	private Gradient gradient;
	private ActuatorTrigger actuatorTrigger;

	public CommandModule(string sensorPath, string actuatorPath, Gradient.GradientsTypes gradientType, int gradientSize) {
		this.sensorPath = sensorPath;
		this.actuatorPath = actuatorPath;

		string gradientFilePath = FilePaths.GRADIENTS_FOLDER + this.sensorPath + "|" + this.actuatorPath + ".csv";
		this.gradient = new SingleRangeGradient(gradientType, gradientFilePath, gradientSize);
	}

	public CommandModule(string sensorPath, string actuatorPath, Gradient.GradientsTypes gradientType, Vector2 gradientSize) {
		this.sensorPath = sensorPath;
		this.actuatorPath = actuatorPath;

		string gradientFilePath = FilePaths.GRADIENTS_FOLDER + this.sensorPath + "|" + this.actuatorPath + ".csv";
		this.gradient = new DualRangeGradient(gradientType, gradientFilePath, (int) gradientSize.x, (int) gradientSize.y);
	}

	public void AddUserInteraction(float[] userInteraction) {
		if (gradient is SingleRangeGradient && userInteraction.Length == 2 || gradient is DualRangeGradient && userInteraction.Length == 3) {
			GradientCell cell = gradient.GetCell(userInteraction);

			// Ajout d'un point d'ancrage au gradient puis réajustement de l'interpolation en prenant en compte ce nouveau point.

			gradient.SaveData();
		}
	}

	public void CommandActuator() {

	}

	public string ToFilePath() {
		return FilePaths.GRADIENTS_FOLDER + sensorPath + "|" + actuatorPath + ".csv";
	}

	public string SensorPath {
		get { return sensorPath; }
		set { sensorPath = value; }
	}

	public string ActuatorPath {
		get { return actuatorPath; }
		set { actuatorPath = value; }
	}

	public override string ToString() {
		return sensorPath + " | " + actuatorPath;
	}
}