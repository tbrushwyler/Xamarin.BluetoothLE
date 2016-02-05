using System;
using Android.OS;
using System.Text.RegularExpressions;
using Android.Views.InputMethods;

namespace BluetoothLE.Droid
{
	internal static class Configuration
	{
		private static bool IsSamsung
		{
			get
			{
				return Build.Manufacturer.ToLower() == "samsung";
			}
		}

		// todo: ensure that this works...
		private const string SSeriesRegex = @"^[sS]\d+$";
		private static bool IsSSeries
		{
			get
			{
				return Regex.IsMatch(Build.Model, SSeriesRegex);
			}
		}

		public static bool ConnectOnMainThread { get { return IsSamsung; } }
		public static bool DisconnectOnMainThread { get { return IsSamsung; } }
		public static bool DiscoverServicesOnMainThread { get { return IsSamsung; } }
	}
}

