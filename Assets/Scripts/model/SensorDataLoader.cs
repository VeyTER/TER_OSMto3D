using UnityEngine;
using System.Xml;
using System.Net;
using System.IO;
using System;

public class SensorDataLoader {
	private string lastLoadedData;
	private bool receptionCompleted;

	private string buildingIdentifier;

	public SensorDataLoader(string buildingIdentifier) {
		this.buildingIdentifier = buildingIdentifier;
	}

	public void LaunchDataLoading() {
		this.lastLoadedData = null;
		this.receptionCompleted = false;

		string url = "http://neocampus.univ-tlse3.fr:8004/api/" + buildingIdentifier + "/*/*?xml&pp";

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

		requestResult = requestResult.Replace("<pre>", "");
		requestResult = requestResult.Replace("</pre>", "");

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
}