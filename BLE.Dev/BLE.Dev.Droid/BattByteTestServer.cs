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
	class AdvertiseCallback : Android.Bluetooth.LE.AdvertiseCallback {
		public override void OnStartFailure(AdvertiseFailure errorCode) {
			base.OnStartFailure(errorCode);
			
		}

		public override void OnStartSuccess(AdvertiseSettings settingsInEffect) {
			base.OnStartSuccess(settingsInEffect);
		}
	}

	public class ServerCallback : BluetoothGattServerCallback {
		private BluetoothGattServer _gattServer;

		public BluetoothGattServer GattServer {
			get { return _gattServer; }
			set { _gattServer = value; }
		}

		public override void OnServiceAdded(ProfileState status, BluetoothGattService service) {
			base.OnServiceAdded(status, service);
		}

		public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic) {
			_gattServer.SendResponse(device, requestId, GattStatus.Success, offset, characteristic.GetValue());
		}
	}

	public class BattByteTestServer {
		private BluetoothGattServer _server;
		private ServerCallback _serverCallback;
		private readonly AdvertiseCallback _advertiseCallback;
		private readonly BluetoothAdapter _adapter;

		public BattByteTestServer() {
			_serverCallback = new ServerCallback();
			_advertiseCallback = new AdvertiseCallback();

			var appContext = Android.App.Application.Context;
			var manager = (BluetoothManager)appContext.GetSystemService(Application.BluetoothService);
			var server = manager.OpenGattServer(appContext, _serverCallback);
			_serverCallback.GattServer = server;

			_adapter = manager.Adapter;

			var battByteService = new BluetoothGattService(UUID.FromString("BEEF".ToGuid().ToString()), GattServiceType.Primary);

			var voltageCharacteristic = new BluetoothGattCharacteristic(UUID.FromString("BEF0".ToGuid().ToString()), GattProperty.Read, GattPermission.Read);
			voltageCharacteristic.SetValue(new byte[] {0xB0, 0x0B});
			battByteService.AddCharacteristic(voltageCharacteristic);

			var tempCharacteristic = new BluetoothGattCharacteristic(UUID.FromString("BEF1".ToGuid().ToString()), GattProperty.Read, GattPermission.Read);
			tempCharacteristic.SetValue(9876, GattFormat.Sint16, 0);
			battByteService.AddCharacteristic(tempCharacteristic);

			server.AddService(battByteService);
			_server = server;
			StartAdvertising("Blah!");
		}

		public void StartAdvertising(string localName) {
			var settings = new AdvertiseSettings.Builder()
					.SetAdvertiseMode(AdvertiseMode.Balanced)
					.SetTxPowerLevel(AdvertiseTx.PowerHigh)
					.SetConnectable(true)
					.Build();


			_adapter.SetName(localName);

			var advertiseDataBuilder = new AdvertiseData.Builder()
				.SetIncludeDeviceName(true);

			foreach (var service in _server.Services) {
				var parcelUuid = new ParcelUuid(UUID.FromString("1234".ToGuid().ToString()));
				advertiseDataBuilder.AddServiceUuid(parcelUuid);
			}

			
			var advertiseData = advertiseDataBuilder.Build();

			_adapter.BluetoothLeAdvertiser.StartAdvertising(settings, advertiseData, _advertiseCallback);
		}

		/// <summary>
		/// Stops peripheral advertising
		/// </summary>
		public void StopAdvertising() {
			_adapter.BluetoothLeAdvertiser.StopAdvertising(_advertiseCallback);
		}
	}
}