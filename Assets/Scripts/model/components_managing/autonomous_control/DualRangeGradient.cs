using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class DualRangeGradient : Gradient {
	private GradientCell[][] cellsArray;
	private int rowCount;
	private int lineCount;

	private DataAxis horizontalAxis;
	private DataAxis verticalAxis;

	public DualRangeGradient(GradientsTypes gradientType, string gradientFilePath, int rowCount, int lineCount) : base(gradientType, gradientFilePath) {
		this.rowCount = rowCount;
		this.lineCount = lineCount;
		this.cellsArray = new GradientCell[this.rowCount][];

		for (int x = 0; x < this.rowCount; x++) {
			this.cellsArray[x] = new GradientCell[this.lineCount];
			for (int y = 0; y < this.cellsArray[x].Length; y++)
				this.cellsArray[x][y] = new GradientCell(x, y);
		}

		this.horizontalAxis = new DataAxis(0, 100, 1);
		this.verticalAxis = new DataAxis(0, 100, 1);
	}

	public DualRangeGradient(GradientsTypes gradientType, string gradientFilePath) : base(gradientType, gradientFilePath) {
		this.LoadData();
	}

	public void SetupHorizontalAxis(float minValue, float maxValue, float step) {
		horizontalAxis.MinValue = minValue;
		horizontalAxis.MaxValue = maxValue;
		horizontalAxis.Step = step;
	}

	public void SetupVerticalAxis(float minValue, float maxValue, float step) {
		verticalAxis.MinValue = minValue;
		verticalAxis.MaxValue = maxValue;
		verticalAxis.Step = step;
	}

	public override void LoadData() {
		if (!File.Exists(gradientFilePath)) {
			FileStream gradientFile = File.Open(gradientFilePath, FileMode.Open);
			StreamReader gradientFileContent = new StreamReader(gradientFile);

			string gradientText = gradientFileContent.ReadToEnd();
			string[] gradientLines = gradientText.Split('\n');
			int lineTermsCount = gradientLines[0].Split('\t').Length;

			rowCount = gradientLines.Length;
			lineCount = lineTermsCount - 1;

			cellsArray = new GradientCell[rowCount][];
			for (int x = 0; x < rowCount; x++)
				cellsArray[x] = new GradientCell[lineCount];

			if (rowCount >= 1 && lineCount >= 1) {
				float hAxisMinValue = 0;
				float hAxisMaxValue = 100;

				float vAxisMinValue = 0;
				float vAxisMaxValue = 100;


				string firstHorizontalInterval = gradientLines[0].Split('\t')[1];
				string firstHorizontalGraduation = firstHorizontalInterval.Split('/')[0];

				string lastHorizontalInterval = gradientLines[0].Split('\t')[lineTermsCount - 1];
				string lastHorizontalGraduation = lastHorizontalInterval.Split('/')[1];

				bool minHorizontalValueParsingOutcome = float.TryParse(firstHorizontalGraduation, out hAxisMinValue);
				bool maxHorizontalValueParsingOutcome = float.TryParse(lastHorizontalGraduation, out hAxisMaxValue);


				string firstVerticalInterval = gradientLines[1].Split('\t')[0];
				string firstVerticalGraduation = firstVerticalInterval.Split('/')[0];

				string lastVerticalInterval = gradientLines[gradientLines.Length - 1].Split('\t')[0];
				string lastVerticalGraduation = lastVerticalInterval.Split('/')[1];

				bool minVerticalValueParsingOutcome = float.TryParse(firstVerticalGraduation, out vAxisMinValue);
				bool maxVerticalValueParsingOutcome = float.TryParse(lastVerticalGraduation, out vAxisMaxValue);


				if (minHorizontalValueParsingOutcome && maxHorizontalValueParsingOutcome) {
					horizontalAxis.MinValue = hAxisMinValue;
					horizontalAxis.MaxValue = hAxisMaxValue;
					horizontalAxis.Step = (horizontalAxis.MaxValue - horizontalAxis.MinValue) / (rowCount * 1F);
				}

				if (minVerticalValueParsingOutcome && maxVerticalValueParsingOutcome) {
					verticalAxis.MinValue = vAxisMinValue;
					verticalAxis.MaxValue = vAxisMaxValue;
					verticalAxis.Step = (verticalAxis.MaxValue - verticalAxis.MinValue) / (lineCount * 1F);
				}

				for (int i = 1; i < gradientLines.Length; i++) {
					string gradientLine = gradientLines[i];
					string[] gradientTerms = gradientLine.Split('\t');
					for (int j = 1; j < gradientTerms.Length; j++) {
						string term = gradientTerms[j];

						bool isCellAnchor = false;
						if (term.Contains("'")) {
							isCellAnchor = true;
							term = term.Replace("'", "");
						}

						float cellValue = 0;
						bool cellValueParsingOutcome = float.TryParse(term, out cellValue);

						if (cellValueParsingOutcome) {
							GradientCell newCell = new GradientCell(j, i, cellValue) {
								IsAnchor = isCellAnchor
							};
							cellsArray[j][i] = newCell;
						}
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

		string abscissaLine = "";
		for (int x = 0; x < rowCount; x++) {
			float hInterValueInf = horizontalAxis.MinValue + x / (rowCount * 1F) * horizontalAxis.Range();
			float hInterValueSup = hInterValueInf + horizontalAxis.Step;
			abscissaLine += "\t" + hInterValueInf + "/" + hInterValueSup;
		}
		gradientFileWriter.WriteLine(abscissaLine);

		for (int y = 0; y < lineCount; y++) {
			float vInterValueInf = verticalAxis.MinValue + y / (lineCount * 1F) * verticalAxis.Range();
			float vInterValueSup = vInterValueInf + verticalAxis.Step;

			string newLine = vInterValueInf + "/" + vInterValueSup;
			for (int x = 0; x < rowCount; x++) {
				float hInterValueInf = horizontalAxis.MinValue + x / (rowCount * 1F) * horizontalAxis.Range();
				float hInterValueSup = hInterValueInf + horizontalAxis.Step;

				GradientCell matchingCell = this.GetCell(new float[] { hInterValueInf, vInterValueInf });
				string cellExpression = matchingCell.Value + (matchingCell.IsAnchor ? "'" : "");

				newLine += "\t" + cellExpression;
			}
			gradientFileWriter.WriteLine(newLine);
		}
		gradientFile.Close();
	}

	public override GradientCell GetCell(float[] cellData) {
		float hAxisValue = cellData[0];
		float vAxisValue = cellData[1];

		if (hAxisValue >= horizontalAxis.MinValue && hAxisValue <= horizontalAxis.MaxValue
		 && vAxisValue >= verticalAxis.MinValue && vAxisValue <= verticalAxis.MaxValue) {
			int x = 0;
			int y = 0;

			bool celluleTrouvee = false;
			for (; x < rowCount && !celluleTrouvee; x++) {
				for (; y < lineCount && !celluleTrouvee; y++) {
					float hInterValue = horizontalAxis.MinValue + x / (rowCount * 1F) * horizontalAxis.Range();
					float vInterValue = verticalAxis.MinValue + y / (lineCount * 1F) * verticalAxis.Range();

					if (hAxisValue >= hInterValue && hAxisValue <= hInterValue + horizontalAxis.Step
					 && vAxisValue >= vInterValue && vAxisValue <= vInterValue + verticalAxis.Step)
						celluleTrouvee = true;
				}
			}

			if (x < rowCount && y < lineCount)
				return cellsArray[x][y];
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

	public int LineCount {
		get { return lineCount; }
		set { lineCount = value; }
	}
}