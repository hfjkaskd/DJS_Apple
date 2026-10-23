using System;
using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public class LoginOut2 : MonoBehaviour
	{
		public Dictionary<string, object> GetAutoTrackEventProperties(int type, Dictionary<string, object> properties)
		{
			return new Dictionary<string, object>
			{
				{ "AutoTrackEventProperty", DateTime.Today }
			};
		}
	}
}
