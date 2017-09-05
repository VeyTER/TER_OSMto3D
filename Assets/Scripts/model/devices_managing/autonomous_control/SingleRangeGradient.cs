using UnityEngine;
using System.IO;

public class SingleRangeGradient : Gradient {
	private GradientCell[] cellsArray;
	private int rowCount;

	private DataAxis axis;

	public SingleRangeGradient(GradientsTypes gradientType, string gradientFilePath, int rowCount) : base(gradientType, gradientFilePath) {
		this.rowCount = rowCount;
		this.cellsArray = new GradientCell[this.rowCount];
		for(int x = 0; x < cellsArray.Length; x++)
			this.cellsArray[x] = new GradientCell(x, 1);

		this.axis = new DataAxis(0, 100, 1);
	}

	public SingleRangeGradient(GradientsTypes gradientType, string gradientFilePath) : base(gradientType, gradientFilePath) {
		this.LoadData();
	}

	public override void LoadData() {
		if (!File.Exists(gradientFilePath)) {
			FileStream gradientFile = File.Open(gradientFilePath, FileMode.Open);
			StreamReader gradientFileContent = new StreamReader(gradientFile);

			string gradientText = gradientFileContent.ReadToEnd();
			string[] gradientLines = gradientText.Split('\n');

			rowCount = gradientLines.Length;
			cellsArray = new GradientCell[rowCount];

			if (rowCount >= 1) {
				float axisMinValue = 0;
				float axisMaxValue = 100;

				string firstInterval = gradientLines[0].Split('\t')[0];
				string firstGraduation = firstInterval.Split('/')[0];

				string lastInterval = gradientLines[gradientLines.Length - 1].Split('\t')[0];
				string lastGraduation = lastInterval.Split('/')[1];

				bool minValueParsingOutcome = float.TryParse(firstGraduation, out axisMinValue);
				bool maxValueParsingOutcome = float.TryParse(lastGraduation, out axisMaxValue);

				if (minValueParsingOutcome && minValueParsingOutcome) {
					axis.MinValue = axisMinValue;
					axis.MaxValue = axisMaxValue;
					axis.Step = (axis.MaxValue - axis.MinValue) / (rowCount * 1F);
				}

				for(int i = 0; i < gradientLines.Length; i++) {
					string line = gradientLines[i];
					string term = line.Split('\t')[1];

					bool isCellAnchor = false;
					if (term.Contains("'")) {
						isCellAnchor = true;
						term = term.Replace("'", "");
					}

					float cellValue = 0;
					bool cellValueParsingOutcome = float.TryParse(term, out cellValue);

					if (cellValueParsingOutcome) {
						GradientCell newCell = new GradientCell(i, 1, cellValue) {
							IsAnchor = isCellAnchor
						};
						cellsArray[i] = newCell;
					}
				}
			} else {
				gradientFile.Close();
				Debug.LogError("Attention, gradient vide.");
			}
		} else {
			throw new FileNotFoundException("Le fichier de gradient \"" + gradientFilePath + "\" n'a pas été trouvé. Impossible de charger le gradient.\n");
		}
	}

	public override void SaveData() {
		if (!File.Exists(gradientFilePath))
			File.Create(gradientFilePath);

		FileStream gradientFile = File.Open(gradientFilePath, FileMode.Create);
		StreamWriter gradientFileWriter = new StreamWriter(gradientFile);

		for (int x = 0; x < rowCount; x++) {
			float interValueInf = axis.MinValue + x / (rowCount * 1F) * axis.Range();
			float interValueSup = interValueInf + axis.Step;

			GradientCell matchingCell = this.GetCell(new float[] { interValueInf });
			string cellExpression = matchingCell.Value + (matchingCell.IsAnchor ? "'" : "");

			gradientFileWriter.WriteLine(interValueInf + "/" + interValueSup + "\t" + cellExpression);
		}
		gradientFile.Close();
	}

	public void SetupAxis(float minValue, float maxValue, float step) {
		axis.MinValue = minValue;
		axis.MaxValue = maxValue;
		axis.Step = step;
	}

	public override GradientCell GetCell(float[] cellData) {
		float axisValue = cellData[0];
		if (axisValue >= axis.MinValue && axisValue <= axis.MaxValue) {
			int x = 0;

			bool celluleTrouvee = false;
			for (; x < rowCount && !celluleTrouvee; x++) {
				float interValue = axis.MinValue + x / (rowCount * 1F) * axis.Range();
				if (axisValue >= interValue && axisValue <= interValue + axis.Step)
					celluleTrouvee = true;
			}

			if (x < rowCount)
				return cellsArray[x];
			else
				return null;
		} else {
			return null;
		}
	}

	public int RowCount {
		get { return rowCount; }
		set { rowCount = value; }
	}
}