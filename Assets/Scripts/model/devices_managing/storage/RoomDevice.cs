public class RoomDevice {
	protected uint index;

	protected string buildingName;
	protected string roomName;
	protected string xmlIdentifier;

	protected string value;
	protected string unit;

	public RoomDevice(uint index, string buildingName, string roomName, string xmlIdentifier, string value, string unit) {
		this.index = index;

		this.buildingName = buildingName;
		this.roomName = roomName;
		this.xmlIdentifier = xmlIdentifier;

		this.value = value;
		this.unit = unit;
	}

	public string ToPath() {
		return buildingName + "/" + roomName + "/" + xmlIdentifier;
	}

	public uint Index {
		get { return index; }
	}

	public string BuildingName {
		get { return buildingName; }
		set { buildingName = value; }
	}

	public string RoomName {
		get { return roomName; }
		set { roomName = value; }
	}

	public string XmlIdentifier {
		get { return xmlIdentifier; }
		set { xmlIdentifier = value; }
	}

	public string Value {
		get { return value; }
		set { this.value = value; }
	}

	public string Unit {
		get { return unit; }
		set { unit = value; }
	}
}