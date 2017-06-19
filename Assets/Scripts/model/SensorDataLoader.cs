using UnityEngine;
using System.Xml;
using System.Net;
using System.IO;
using System;

public class SensorDataLoader {
	public enum Sensors { TEMPERATURE, HUMIDITY }

	private string lastLoadedData;
	private bool receptionCompleted;
	private string url;

	private SensorDataLoader(string url) {
		this.Initialize();
		this.url = url;
	}

	public void Initialize() {
		this.lastLoadedData = null;
		this.receptionCompleted = false;
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
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Credentials = new NetworkCredential("reader", "readerpassword");

		RequestState myRequestState = new RequestState() {
			Request = webRequest
		};
		webRequest.BeginGetResponse(new AsyncCallback(ProcessReceivedData), myRequestState);
	}

	private void ProcessReceivedData(IAsyncResult asynchronousResult) {
		while (!asynchronousResult.IsCompleted) ;
		RequestState myRequestState = (RequestState) asynchronousResult.AsyncState;
		string requestResult = myRequestState.RequestResult();

		lastLoadedData = requestResult;

		lastLoadedData = lastLoadedData.Replace("<pre>", "");
		lastLoadedData = lastLoadedData.Replace("</pre>", "");

		lastLoadedData = lastLoadedData.Replace("&lt;", "<");
		lastLoadedData = lastLoadedData.Replace("&gt;", ">");

		receptionCompleted = true;
	}

	public bool ReceptionCompleted {
		get { return receptionCompleted; }
	}

	public string LastLoadedData {
		get { return lastLoadedData; }
		set { lastLoadedData = value; }
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
		internal static SensorDataLoader temperatureLoader = new SensorDataLoader("http://neocampus.univ-tlse3.fr:8004/api/u4/*/temperature?csv&pp");
		internal static SensorDataLoader humidityLoader = new SensorDataLoader("http://neocampus.univ-tlse3.fr:8004/api/u4/*/humidity?csv&pp");
	}
}