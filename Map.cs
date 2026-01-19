using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using CesiumForUnity;

public class NewBehaviorScript : MonoBehaviour
{
	public TMP_InputField inputField;
	public CesiumGeoreference cesiumGeoreference;
	public CesiumCameraController cesiumCameraController;
	//INSERT YOUR GOOGLE MAPS PLATFORM KEY IN THE apiKey quotes
	private string apiKey = "";
	private string urlLocation = "";
	private string urlElevation = "";
	private double lat;
	private double lon;
	private double elevation;
	private string latLon;
	// Plus signs used because Google maps can't use spaces
	private string address = "2031+Kings+Highway+Shreveport+LA+71103";
	
	// Start is called before the first frame update
	void Start()
	{
		cesiumGeoreference.originAuthority = CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight;
		inputField.text = "2031 Kings Highway Shreveport LA 71103";
		StartCoroutine(GetGoogleMapLocation());
	}
	
	// Update is called once per frame
	void Update()
	
	
	{
	}

	IEnumerator GetGoogleMapLocation()
	{
		urlLocation = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&key=" + apiKey;
		UnityWebRequest webRequest = UnityWebRequest.Get(urlLocation);
		yield return webRequest.SendWebRequest();
		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("WWW Error: " + webRequest.error);
		}
		else
		{
			string requestText = webRequest.downloadHandler.text;
			int locationIndex = requestText.IndexOf("\"location\"");
			if (locationIndex != -1 && requestText.IndexOf("\"lat\"") > -1 && requestText.IndexOf("\"lng\"") > -1); 
			{
				int indexOfLat = requestText.IndexOf("\"lat\"");
				int startIndexLat = requestText.IndexOf(":", indexOfLat) + 1;
				int endIndexLat = requestText.IndexOf(",", startIndexLat);
				string latString = requestText.Substring(startIndexLat, endIndexLat - startIndexLat);
				if (double.TryParse(latString,out lat)) { }
	
				int indexOfLng = requestText.IndexOf("\"lng\"");
				int startIndexLng = requestText.IndexOf(":", indexOfLng) + 1;
				int endIndexLng = requestText.IndexOf("}", startIndexLng);
				string lngString = requestText.Substring(startIndexLng, endIndexLng - startIndexLng);
				if (double.TryParse(lngString,out lon)) { }
				
				latLon = latString + "," + lngString;
				StartCoroutine(GetGoogleMapElevation());
			}
		}
	}
	
	//Return elevation from lat/long using Google Maps Platform api
	IEnumerator GetGoogleMapElevation()
	{
		urlElevation = "https://maps.googleapis.com/maps/api/elevation/json?locations=" + latLon + "&key=" + apiKey;
		UnityWebRequest webRequest = UnityWebRequest.Get(urlElevation);
		yield return webRequest.SendWebRequest();
		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("WWW Error: " + webRequest.error);
		}
		else
		{
			string requestText = webRequest.downloadHandler.text;
			if (requestText.IndexOf("\"elevation\"") > -1)
			{
				int elevationIndex = requestText.IndexOf("\"elevation\"");
				int colonIndex = requestText.IndexOf(":", elevationIndex);
				int commaIndex = requestText.IndexOf(",", colonIndex);
				string elevationSubstring = requestText.Substring(colonIndex + 1, commaIndex - colonIndex - 1);
				elevationSubstring = elevationSubstring.Trim();
				if (double.TryParse(elevationSubstring,out elevation)) { }

				// 400 meters above the georeference to prevent from starting in a building
				cesiumGeoreference.SetOriginLongitudeLatitudeHeight(lon, lat, elevation + 400);
				cesiumCameraController.transform.position = Vector3.zero;
				cesiumCameraController.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
			}

		}
	}
	
	//Call everytime address input changes
	public void OnTextChange()
	{
		address = inputField.text;
		address = address.Replace(" ", "+");
		StartCoroutine(GetGoogleMapLocation());
	}
}

