using UnityEngine;
using System.Xml;
using System.Net;
using System.IO;
using System;

public class SensorDataLoader {
	private WebRequest webRequest;
	private string lastLoadedData;

	private string buildingIdentifier;

	public SensorDataLoader(string buildingIdentifier) {
		this.webRequest = null;
		this.lastLoadedData = null;

		this.buildingIdentifier = buildingIdentifier;
	}

	public void LaunchDataLoading(AsyncCallback callBack) {
		this.lastLoadedData = null;

		string url = "http://neocampus.univ-tlse3.fr:8004/api/" + buildingIdentifier + "/*/*?xml&pp";

		WebRequest webRequest = WebRequest.Create(url);

		string[] credentials = this.GetLoginId();
		webRequest.Credentials = new NetworkCredential(credentials[0], credentials[1]);

		RequestState myRequestState = new RequestState() {
			Request = webRequest
		};
		webRequest.BeginGetResponse(callBack, myRequestState);
	}

	private string[] GetLoginId() {
		StreamReader apiLoginFile = new StreamReader(File.Open(FilePaths.API_LOGIN_FILE, FileMode.Open));
		string user = apiLoginFile.ReadLine();
		string password = apiLoginFile.ReadLine();
		apiLoginFile.Close();

		return new string[] { user, password };
	}

	public void StopDataLoading() {
		if (webRequest != null)
			webRequest.Abort();
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