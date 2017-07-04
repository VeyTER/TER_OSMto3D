public class RoomComponent {
	protected uint index;

	protected string roomName;
	protected string xmlIdentifier;

	protected string value;
	protected string unit;

	public RoomComponent(uint index, string roomName, string xmlIdentifier, string value, string unit) {
		this.index = index;

		this.roomName = roomName;
		this.xmlIdentifier = xmlIdentifier;

		this.value = value;
		this.unit = unit;
	}

	public uint Index {
		get { return index; }
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