public class GradientCell {
	private int rowNumber;
	private int lineNumber;

	private float value;
	private bool isAnchor;

	public GradientCell(int rowNumber, int lineNumber) {
		this.rowNumber = rowNumber;
		this.lineNumber = lineNumber;

		this.value = 0;
		this.isAnchor = false;
	}

	public GradientCell(int rowNumber, int lineNumber, float value) {
		this.rowNumber = rowNumber;
		this.lineNumber = lineNumber;

		this.value = value;
		this.isAnchor = false;
	}

	public float Value {
		get { return value; }
		set { this.value = value; }
	}

	public bool IsAnchor {
		get { return isAnchor; }
		set { this.isAnchor = value; }
	}

	public int RowNumber {
		get { return rowNumber; }
		set { rowNumber = value; }
	}

	public int LineNumber {
		get { return lineNumber; }
		set { lineNumber = value; }
	}
}