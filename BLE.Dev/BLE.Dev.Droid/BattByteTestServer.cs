using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothLE.Core;
using Java.Util;

namespace BLE.Dev.Droid {
	public class ServerCallback : BluetoothGattServerCallback {
		public override void OnServiceAdded(ProfileState status, BluetoothGattService service) {
			base.OnServiceAdded(status, service);
		}
	}

	public class BattByteTestServer {
		private BluetoothGattServer _server;
		private ServerCallback _serverCallback;
		
		public BattByteTestServer() {
			_serverCallback = new ServerCallback();

			var appContext = Android.App.Application.Context;
			var manager = (BluetoothManager)appContext.GetSystemService(Application.BluetoothService);
			var server = manager.OpenGattServer(appContext, _serverCallback);

			var battByteService = new BluetoothGattService(UUID.FromString("BEEF".ToGuid().ToString()), GattServiceType.Primary);
			var voltageCharacteristic = new BluetoothGattCharacteristic(UUID.FromString("BEF0".ToGuid().ToString()), GattProperty.Read, GattPermission.Read);
			voltageCharacteristic.SetValue(1234, GattFormat.Sint16, 0);
			battByteService.AddCharacteristic(voltageCharacteristic);

			server.AddService(battByteService);
		}
	}
}