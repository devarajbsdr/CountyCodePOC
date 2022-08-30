using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Globalization;
using UnityEngine.Networking;
using MiniJSON;
public class LocationManager : MonoBehaviour {

	private float lat,lng;
	public Text latutudeText;
	public Text longitudeText;
	public Text countryText;

	// Use this for initialization
	void Start () 
	{
		#if UNITY_ANDROID || UNITY_IOS
		StartCoroutine (UpdateLocation ());
		#endif

		#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX 
		StartCoroutine (GetLocation (Network.player.ipAddress));
		#endif
	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator GetLocation(string ip)
	{
		
			// Start a download of position information
			var www = new WWW("freegeoip.net/json");

			// Wait for download to complete
			while (!www.isDone)
				yield return new WaitForSeconds(2);

		//	print(www.text);

		IDictionary locInfo = (IDictionary) Json.Deserialize(www.text);

		latutudeText.text = locInfo ["latitude"].ToString ();
		longitudeText.text= locInfo ["longitude"].ToString ();
		countryText.text= locInfo ["country_name"].ToString ();
	}

	IEnumerator UpdateLocation()
	{
		if (!Input.location.isEnabledByUser)
			yield break;

		// Start service before querying location
		Input.location.Start();

		// Wait until service initializes
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			print("Timed out");
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			lat = Input.location.lastData.latitude;
			lng = Input.location.lastData.longitude;
			StartCoroutine (CountryFinder ());
			latutudeText.text = "Latitude " + lat;
			longitudeText.text= "Longitude " + lng;
			// Access granted and location value could be retrieved
			//print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		}

		// Stop service if there is no need to query location updates continuously
		Input.location.Stop();

	}

	IEnumerator CountryFinder()
	{
		WWW www = new WWW ("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lng + "&sensor=true");
		yield return www;
		if (www.error != null)
			yield break;
		XmlDocument reverseGeocodeResult = new XmlDocument ();
		reverseGeocodeResult.LoadXml (www.text);
		if (reverseGeocodeResult.GetElementsByTagName ("status").Item (0).ChildNodes.Item (0).Value != "OK")
			yield break;
		string countryCode = null;
		bool countryFound = false;
		foreach (XmlNode eachAdressComponent in reverseGeocodeResult.GetElementsByTagName("result").Item(0).ChildNodes) {
			if (eachAdressComponent.Name == "address_component") {
				foreach (XmlNode eachAddressAttribute in eachAdressComponent.ChildNodes) {
					if (eachAddressAttribute.Name == "long_name")
						countryCode = eachAddressAttribute.FirstChild.Value;
					if (eachAddressAttribute.Name == "type" && eachAddressAttribute.FirstChild.Value == "country")
						countryFound = true;
				}
				if (countryFound)
					break;
			}
			countryText.text = countryCode;
		}
	}
		

}
