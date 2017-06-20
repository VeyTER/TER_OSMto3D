using UnityEngine;
using System.Xml;
using System.Net;
using System.IO;
using System;

public class SensorDataLoader {
	public enum Sensors { TEMPERATURE, HUMIDITY }

	private string lastLoadedData;
	private bool receptionCompleted;

	private string buildingIdentifier;
	private string sensorIdentifier;

	private SensorDataLoader(string sensorIdentifier) {
		this.buildingIdentifier = null;
		this.sensorIdentifier = sensorIdentifier;
	}

	public static SensorDataLoader GetInstance(Sensors sensor) {
		switch (sensor) {
		case Sensors.TEMPERATURE:
			return SensorDataLoaderHolder.temperatureLoader;
		case Sensors.HUMIDITY:
			return SensorDataLoaderHolder.humidityLoader;
		default:
			return null;
		}
	}

	public void LoadData() {
		this.lastLoadedData = null;
		this.receptionCompleted = false;

		string url = "http://neocampus.univ-tlse3.fr:8004/api/" + buildingIdentifier + "/*/" + sensorIdentifier + "?csv&pp";

		url = "http://neocampus.univ-tlse3.fr:8004/api/" + buildingIdentifier + "/*/*?csv&pp";

		WebRequest webRequest = WebRequest.Create(url);

		webRequest.Credentials = new NetworkCredential("reader", "readerpassword"); // <-- Dans un fichier

		RequestState myRequestState = new RequestState() {
			Request = webRequest
		};
		webRequest.BeginGetResponse(new AsyncCallback(ProcessReceivedData), myRequestState);
	}

	private void ProcessReceivedData(IAsyncResult asynchronousResult) {
		while (!asynchronousResult.IsCompleted);
		RequestState requestState = (RequestState) asynchronousResult.AsyncState;
		string requestResult = requestState.RequestResult();

		lastLoadedData = requestResult;

		receptionCompleted = true;
	}

	public bool ReceptionCompleted {
		get { return receptionCompleted; }
	}

	public string LastLoadedData {
		get { return lastLoadedData; }
		set { lastLoadedData = value; }
	}

	public string BuildingIdentifier {
		get { return buildingIdentifier; }
		set { buildingIdentifier = value; }
	}

	private class RequestState {
		private WebRequest request;

		public string RequestResult() {
			WebResponse response = request.GetResponse();
			Stream streamResponse = response.GetResponseStream();
			return new StreamReader(streamResponse).ReadToEnd();
		}

		public WebRequest Request {
			get { return request; }
			set { request = value; }
		}
	}

	private class SensorDataLoaderHolder {
		internal static SensorDataLoader temperatureLoader = new SensorDataLoader("temperature");
		internal static SensorDataLoader humidityLoader = new SensorDataLoader("humidity");
		internal static SensorDataLoader co2Loader = new SensorDataLoader("co2");
	}
}