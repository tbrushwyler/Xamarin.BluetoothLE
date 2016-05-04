using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BluetoothLE.Droid {
	class ScanCallback : Android.Bluetooth.LE.ScanCallback{
		public override void OnScanResult(ScanCallbackType callbackType, ScanResult result) {
			base.OnScanResult(callbackType, result);
		}

		public override void OnBatchScanResults(IList<ScanResult> results) {
			base.OnBatchScanResults(results);
		}

		public override void OnScanFailed(ScanFailure errorCode) {
			base.OnScanFailed(errorCode);
		}
	}
}