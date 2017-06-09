public class MapBackground {
	private string backgroundName;

	private double minLat;
	private double minLon;

	private double maxLat;
	private double maxLon;

	public MapBackground(string[] backgroundData) {
		if (backgroundData.Length == 3) {
			this.backgroundName = backgroundData[0];

			string[] startLocation = backgroundData[1].Split(';');
			string[] endLocation = backgroundData[2].Split(';');

			this.minLat = double.Parse(startLocation[1]);
			this.minLon = double.Parse(startLocation[0]);

			this.maxLat = double.Parse(endLocation[1]);
			this.maxLon = double.Parse(endLocation[0]);
		}
	}

	public MapBackground(string backgroundName, double minLat, double minLon, double maxLat, double maxLon) {
		this.backgroundName = backgroundName;

		this.minLat = minLat;
		this.minLon = minLon;

		this.minLat = maxLat;
		this.minLon = maxLon;
	}

	public string BackgroundName {
		get { return backgroundName; }
		set { backgroundName = value; }
	}

	public double MinLat {
		get { return minLat; }
		set { minLat = value; }
	}

	public double MinLon {
		get { return minLon; }
		set { minLon = value; }
	}

	public double MaxLat {
		get { return maxLat; }
		set { maxLat = value; }
	}

	public double MaxLon {
		get { return maxLon; }
		set { maxLon = value; }
	}
}