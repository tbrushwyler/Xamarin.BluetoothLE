﻿using System;
using BluetoothLE.Core;
using System.Collections.Generic;
using System.IO;
using Android.Bluetooth;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Java.Util;
using System.Threading.Tasks;
using Android.Bluetooth.LE;
using Android.Content;
using Java.Nio;
using Android.OS;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid {
	/// <summary>
	/// Concrete implementation of <see cref="BluetoothLE.Core.IAdapter"/> interface.
	/// </summary>
	public class Adapter : Java.Lang.Object, IAdapter {
		private readonly BluetoothAdapter _adapter;
		private readonly BluetoothGattServer _gattServer;

		private readonly GattCallback _callback;
		private readonly AdvertiseCallback _advertiseCallback;
		private readonly ScanCallback _scanCallback;

		private BluetoothGatt _gatt;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Adapter"/> class.
		/// </summary>
		public Adapter() {
			var appContext = Android.App.Application.Context;
			var manager = (BluetoothManager)appContext.GetSystemService(Context.BluetoothService);
			_adapter = manager.Adapter;

			_callback = new GattCallback();
			_callback.DeviceConnected += BluetoothGatt_DeviceConnected;
			_callback.DeviceDisconnected += BluetoothGatt_DeviceDisconnected;

			ConnectedDevices = new List<IDevice>();
			_scanCallback = new ScanCallback();
			_scanCallback.DeviceDiscovered += ScanCallbackOnDeviceDiscovered;

			_advertiseCallback = new AdvertiseCallback();
			_advertiseCallback.AdvertiseStartFailed += BluetoothGatt_AdvertiseStartFailed;
			_advertiseCallback.AdvertiseStartSuccess += Bluetooth_AdvertiseStartSuccess;

			var callback = new GattServerCallback();
			_gattServer = manager.OpenGattServer(appContext, callback);
			callback.Server = _gattServer;
		}

		#region IAdapter implementation

		/// <summary>
		/// Occurs when device discovered.
		/// </summary>
		public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate { };

		/// <summary>
		/// Occurs when device connected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };

		/// <summary>
		/// Occurs when device disconnected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };

		/// <summary>
		/// Occurs when a device failed to connect.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate { };

		/// <summary>
		/// Occurs when scan timeout elapsed.
		/// </summary>
		public event EventHandler ScanTimeoutElapsed = delegate { };

		/// <summary>
		/// Occurs when advertising start fails
		/// </summary>
		public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartFailed = delegate { };

		/// <summary>
		/// Occurs when advertising start succeeds
		/// </summary>
		public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartSuccess = delegate { };

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		public void StartScanningForDevices() {
			StartScanningForDevices(false, new string[0]);
		}

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		/// <param name="serviceUuids">White-listed service UUIDs</param>
		public async void StartScanningForDevices(bool continuousScanning = false, params string[] serviceUuids) {
			DiscoveredDevices = new List<IDevice>();
			IsScanning = true;

			var uuids = new List<UUID>();
			foreach (var id in serviceUuids) {
				var guid = id.ToGuid();
				uuids.Add(UUID.FromString(guid.ToString("D")));
			}

			//TODO: Power options, whitelist UUIDS
			
			_adapter.BluetoothLeScanner.StartScan(_scanCallback);

			//_adapter.StartLeScan(uuids.ToArray(), this);


			if (continuousScanning == false) {
				await Task.Delay(ScanTimeout);

				if (IsScanning) {
					StopScanningForDevices();
					ScanTimeoutElapsed(this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Stop scanning for devices.
		/// </summary>
		public void StopScanningForDevices() {
			IsScanning = false;
			_adapter.BluetoothLeScanner.StopScan(_scanCallback);
		}

		/// <summary>
		/// Connect to a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void ConnectToDevice(IDevice device) {
			Action action = () => PerformConnectToDevice(device);

			if (Configuration.ConnectOnMainThread) {
				var handler = new Handler(Looper.MainLooper);
				handler.PostAtFrontOfQueue(action);
			} else {
				action.Invoke();
			}
		}

		/// <summary>
		/// Discconnect from a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void DisconnectDevice(IDevice device) {
			if (_gatt == null)
				return;

			Action action = () => PerformDisconnect(device);

			if (Configuration.DisconnectOnMainThread) {
				var handler = new Handler(Looper.MainLooper);
				handler.PostAtFrontOfQueue(action);
			} else {
				action.Invoke();
			}
		}

		public void StartAdvertising(string localName, List<IService> services) {
			var settings = new AdvertiseSettings.Builder()
					.SetAdvertiseMode(AdvertiseMode.Balanced)
					.SetTxPowerLevel(AdvertiseTx.PowerHigh)
					.SetConnectable(true)
					.Build();


			_adapter.SetName(localName);
			
			var advertiseDataBuilder = new AdvertiseData.Builder()
				.SetIncludeDeviceName(true);

			foreach (Service service in services) {
				var parcelUuid = new ParcelUuid(UUID.FromString(service.Uuid));
				advertiseDataBuilder.AddServiceUuid(parcelUuid);
				if (_gattServer == null) {
					continue;
				}
				_gattServer.AddService(service.NativeService);
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

		public bool SupportsAdvertising() {
			var multi = _adapter.IsMultipleAdvertisementSupported;
			
			var batching =  _adapter.IsOffloadedScanBatchingSupported;
			var filtering = _adapter.IsOffloadedFilteringSupported;

			String missingFeatures = "";
			if (!multi) {
				missingFeatures += $"MultipleAdvertisment";
			}
			if (!batching) {
				missingFeatures += $" OffloadedScanBatching";
			}
			if (!filtering) {
				missingFeatures += $" OffloadedFiltering";
			}
			var support = multi && batching && filtering;
			if (!support) {
				throw new Exception($"Missing features: {missingFeatures}");
			}
			return support;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is scanning.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsScanning { get; set; }

		/// <summary>
		/// Gets or sets the amount of time to scan for devices.
		/// </summary>
		/// <value>The scan timeout.</value>
		public TimeSpan ScanTimeout { get; set; }

		/// <summary>
		/// Gets or sets the amount of time to attempt to connect to a device.
		/// </summary>
		/// <value>The connection timeout.</value>
		public TimeSpan ConnectionTimeout { get; set; }

		/// <summary>
		/// Gets the discovered devices.
		/// </summary>
		/// <value>The discovered devices.</value>
		public IList<IDevice> DiscoveredDevices { get; set; }

		/// <summary>
		/// Gets the connected devices.
		/// </summary>
		/// <value>The connected devices.</value>
		public IList<IDevice> ConnectedDevices { get; set; }

		#endregion

		#region GattCallback delegate methods

		private void BluetoothGatt_DeviceConnected(object sender, DeviceConnectionEventArgs e) {
			ConnectedDevices.Add(e.Device);
			DeviceConnected(this, e);
		}

		private void BluetoothGatt_DeviceDisconnected(object sender, DeviceConnectionEventArgs e) {
			var id = e.Device.Id;
			DisconnectDevice(id);
			DeviceDisconnected(this, e);
		}

		private void DisconnectDevice(Guid id) {
			var connDevice = ConnectedDevices.FirstOrDefault(x => x.Id == id);
			if (connDevice != null) {
				ConnectedDevices.Remove(connDevice);
			}
		}

		#endregion

		private void PerformConnectToDevice(IDevice device) {
			var bleDevice = device.NativeDevice as BluetoothDevice;
			if (bleDevice == null)
				return;

			var remoteDevice = _adapter.GetRemoteDevice(bleDevice.Address);
			if (remoteDevice == null)
				return;

			_gatt = remoteDevice.ConnectGatt(Android.App.Application.Context, false, _callback);
			_gatt.Connect();
		}

		private void PerformDisconnect(IDevice device) {
			try {
				_gatt.Close(); // this will not trigger the OnConnectionStateChange.OnConnectionStateChange callback
				_gatt = null;
				DisconnectDevice(device.Id);
				DeviceDisconnected(this, new DeviceConnectionEventArgs(device));
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
		}

		private void ScanCallbackOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs) {
			if (DiscoveredDevices.All(x => x.Id != deviceDiscoveredEventArgs.Device.Id)) {
				DiscoveredDevices.Add(deviceDiscoveredEventArgs.Device);
				DeviceDiscovered(this, new DeviceDiscoveredEventArgs(deviceDiscoveredEventArgs.Device));
			}
		}

		private void Bluetooth_AdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			AdvertiseStartSuccess?.Invoke(this, advertiseStartEventArgs);
		}

		private void BluetoothGatt_AdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			AdvertiseStartFailed?.Invoke(this, advertiseStartEventArgs);
		}
	}

	public class GattServerCallback : BluetoothGattServerCallback {
		public BluetoothGattServer Server { get; set; }

		public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic) {
			var value = characteristic.GetValue();
			Server.SendResponse(device, requestId, GattStatus.Success, offset, value);
		}

		public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value) {
			characteristic.SetValue(value);
			if (responseNeeded) {
				Server.SendResponse(device, requestId, GattStatus.Success, offset, value);
			}
		}
	}
}

