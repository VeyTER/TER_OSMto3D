public abstract class Gradient {
	public enum GradientsTypes { STATIC, TEMPORAL }
	protected GradientsTypes gradientType;

	protected string gradientFilePath;

	public Gradient(GradientsTypes gradientType, string gradientFilePath) {
		this.gradientType = gradientType;
		this.gradientFilePath = gradientFilePath;
	}

	public abstract void LoadData();
	public abstract void SaveData();

	public abstract GradientCell GetCell(float[] cellData);
}