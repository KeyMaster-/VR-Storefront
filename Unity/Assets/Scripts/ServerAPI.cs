using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerAPI {

	// Set this property with an original customer id
	public static string customerId = "89600710";
	
	public static bool makePaymentWithPrice(string price, string description) {
		WWWForm form = new WWWForm ();
		form.AddField ("customerId", customerId);
		form.AddField ("price", price);
		form.AddField ("description", description);
		byte[] rawData = form.data;
		string url = "http://vr-storefront.herokuapp.com/pay";

		WWW www = new WWW (url, rawData, form.headers);

		if (www.error == null)
			return true;
		else
			return false;
	}
}
