using UnityEngine;
using System.Xml;
using System.Net;
using System.IO;
using System;

public class SensorDataLoader {
	private string lastLoadedData;

	private string buildingIdentifier;

	public SensorDataLoader(string buildingIdentifier) {
		this.buildingIdentifier = buildingIdentifier;
	}

	public void LaunchDataLoading(AsyncCallback callBack) {
		this.lastLoadedData = null;

		string url = "http://neocampus.univ-tlse3.fr:8004/api/" + buildingIdentifier + "/*/*?xml&pp";

		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Credentials = new NetworkCredential("reader", "readerpassword"); // <-- Dans un fichier

		RequestState myRequestState = new RequestState() {
			Request = webRequest
		};
		webRequest.BeginGetResponse(callBack, myRequestState);
	}

	public string LastLoadedData {
		get { return lastLoadedData; }
		set { lastLoadedData = value; }
	}

	public string BuildingIdentifier {
		get { return buildingIdentifier; }
		set { buildingIdentifier = value; }
	}

	public class RequestState {
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