using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Drm;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid {
	class ScanCallback : Android.Bluetooth.LE.ScanCallback{
		/// <summary>
		/// Occurs when device discovered.
		/// </summary>
		public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;

		public override void OnScanResult(ScanCallbackType callbackType, ScanResult result) {
			base.OnScanResult(callbackType, result);
			var device=  new Device(result.Device, null, null, result.Rssi);
			if (result.ScanRecord != null) {
				device.AdvertismentData = ProcessData(result.ScanRecord);
				if (result.ScanRecord.ServiceUuids != null) {
					device.AdvertisedServiceUuids = result.ScanRecord.ServiceUuids.Select(x => Guid.Parse(x.Uuid.ToString())).ToList();
				}
			}
			var eventArgs = new DeviceDiscoveredEventArgs(device);

			DeviceDiscovered?.Invoke(this, eventArgs);
		}

		private Dictionary<Guid, byte[]> ProcessData(ScanRecord scanRecord) {
			var dict = new Dictionary<Guid, byte[]>();
			foreach (var serviceData in scanRecord.ServiceData) {
				var guid = Guid.ParseExact(serviceData.Key.ToString(), "d");
				var data = serviceData.Value;
				dict[guid] = data;
			}
			return dict;
		}

		public override void OnBatchScanResults(IList<ScanResult> results) {
			base.OnBatchScanResults(results);
		}

		public override void OnScanFailed(ScanFailure errorCode) {
			base.OnScanFailed(errorCode);
		}
	}
}