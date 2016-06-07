﻿using System;
using BluetoothLE.Core;
using CoreBluetooth;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BluetoothLE.Core.Events;
using CoreFoundation;
using Foundation;
using UIKit;

namespace BluetoothLE.iOS {
	/// <summary>
	/// Concrete implementation of <see cref="BluetoothLE.Core.IAdapter"/> interface.
	/// </summary>
	public class Adapter : CBPeripheralManagerDelegate, IAdapter {
		private readonly CBCentralManager _central;
		private readonly CBPeripheralManager _peripheralManager;
		private readonly AutoResetEvent _stateChanged;

		private static Adapter _current;
		private Task _startAdvertise;

		/// <summary>
		/// Gets the current Adpater instance
		/// </summary>
		/// <value>The current Adapter instance</value>
		public static Adapter Current {
			get { return _current; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Adapter"/> class.
		/// </summary>
		public Adapter() {
			_central = new CBCentralManager();

			_central.DiscoveredPeripheral += DiscoveredPeripheral;
			_central.UpdatedState += UpdatedState;
			_central.ConnectedPeripheral += ConnectedPeripheral;
			_central.DisconnectedPeripheral += DisconnectedPeripheral;
			_central.FailedToConnectPeripheral += FailedToConnectPeripheral;

			ConnectedDevices = new List<IDevice>();
			_stateChanged = new AutoResetEvent(false);

			_current = this;


			_peripheralManager = new CBPeripheralManager(this, null);
		}

		#region ICBPeripheralManagerDelegate implementation

		public override async void StateUpdated(CBPeripheralManager peripheral) {
			switch (peripheral.State) {
				case CBPeripheralManagerState.Unknown:
				case CBPeripheralManagerState.Resetting:
				case CBPeripheralManagerState.Unsupported:
				case CBPeripheralManagerState.Unauthorized:
				case CBPeripheralManagerState.PoweredOff:
					break;
				case CBPeripheralManagerState.PoweredOn:
					if (_startAdvertise != null) {
						_startAdvertise.Start();
						await _startAdvertise;
						_startAdvertise = null;
					}
					break;
			}
		}

		public override void AdvertisingStarted(CBPeripheralManager peripheral, NSError error) {
			if (error == null) {
				AdvertiseStartSuccess?.Invoke(this, new AdvertiseStartEventArgs(AdvertiseStatus.None));
			} else {
				AdvertiseStartFailed?.Invoke(this, new AdvertiseStartEventArgs(AdvertiseStatus.InternalError));
			}
		}

		#endregion

		private async Task WaitForState(CBCentralManagerState state) {
			while (_central.State != state) {
				await Task.Run(() => _stateChanged.WaitOne());
			}
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
		/// Occurs when advertising start fails
		/// </summary>
		public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartFailed = delegate { };

		/// <summary>
		/// Occurs when advertising start succeeds
		/// </summary>
		public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartSuccess = delegate { };

		/// <summary>
		/// Occurs when scan timeout elapsed.
		/// </summary>
		public event EventHandler ScanTimeoutElapsed = delegate { };

		/// <summary>
		/// Occurs when a device failed to connect.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate { };

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
		/// Start scanning for devices.
		/// </summary>
		public void StartScanningForDevices() {
			StartScanningForDevices(false, new string[0]);
		}

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		/// <param name="continuousScanning">Continuous scanning without timeout</param>
		/// <param name="serviceUuids">White-listed service UUIDs</param>
		public async void StartScanningForDevices(bool continuousScanning = false, params string[] serviceUuids) {
			await WaitForState(CBCentralManagerState.PoweredOn);

			var uuids = new List<CBUUID>();
			foreach (var guid in serviceUuids) {
				uuids.Add(CBUUID.FromString(guid));
			}

			DiscoveredDevices = new List<IDevice>();
			IsScanning = true;

			var options = new PeripheralScanningOptions() { AllowDuplicatesKey = continuousScanning };

			_central.ScanForPeripherals(uuids.ToArray(), options);

			if (continuousScanning == false) {
				// Wait for the timeout
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
			_central.StopScan();
		}

		/// <summary>
		/// Connect to a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public async void ConnectToDevice(IDevice device) {
			var peripheral = device.NativeDevice as CBPeripheral;
			_central.ConnectPeripheral(peripheral);

			await Task.Delay(ConnectionTimeout);

			if (ConnectedDevices.All(x => x.Id != device.Id)) {
				_central.CancelPeripheralConnection(peripheral);
				var args = new DeviceConnectionEventArgs(device) {
					ErrorMessage = "The device connection timed out."
				};

				DeviceFailedToConnect(this, args);
			}
		}

		/// <summary>
		/// Discconnect from a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void DisconnectDevice(IDevice device) {
			var peripheral = device.NativeDevice as CBPeripheral;
			_central.CancelPeripheralConnection(peripheral);
		}

		public async void StartAdvertising(string localName, List<IService> services) {
			_startAdvertise = new Task(() => {
				var cbuuIdArray = new NSMutableArray();
				foreach (Service service in services) {
					cbuuIdArray.Add(CBUUID.FromString(service.Uuid));
					_peripheralManager.AddService((CBMutableService)service.NativeService);
				}


				var optionsDict = new NSMutableDictionary();
				optionsDict[CBAdvertisement.DataLocalNameKey] = new NSString(localName);
				optionsDict[CBAdvertisement.DataServiceUUIDsKey] = cbuuIdArray;

				_peripheralManager.StartAdvertising(optionsDict);
			});

			if (_peripheralManager.State == CBPeripheralManagerState.PoweredOn) {
				_startAdvertise.Start();
				await _startAdvertise;
				_startAdvertise = null;
			}
		}

		public void StopAdvertising() {
			_startAdvertise = null;
			_peripheralManager.StopAdvertising();
		}

		public bool SupportsAdvertising() {
			// CBPeripheralManager seems to be widely supported by Apple, in the limited fassion that it is implemented
			return UIDevice.CurrentDevice.CheckSystemVersion(6, 0);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is scanning.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsScanning { get; set; }

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

		#region CBCentralManager delegate methods

		private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e) {
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);

			//System.Diagnostics.Debug.WriteLine($"Discovered BT Device: {deviceId}");

			var addedDevice = this.DiscoveredDevices.FirstOrDefault(d => d.Id == deviceId);

			if (addedDevice == null) {
				// New Device
				var device = new Device(e.Peripheral, e.RSSI);
				DiscoveredDevices.Add(device);
				device.AdvertismentData = ProcessData(e.AdvertisementData);
				device.AdvertisedServiceUuids = ProcessUuids(e.AdvertisementData);
				DeviceDiscovered(this, new DeviceDiscoveredEventArgs(device));
			} else {
				var device = (Device)addedDevice;

				device.UpdateRssi(e.RSSI);
				DeviceDiscovered(this, new DeviceDiscoveredEventArgs(device));
			}
		}

		private static List<Guid> ProcessUuids(NSDictionary advertisementData) {
			List<Guid> guids = new List<Guid>();
			if (advertisementData.ContainsKey(CBAdvertisement.DataServiceUUIDsKey)) {
				var cbUuids = advertisementData[CBAdvertisement.DataServiceUUIDsKey] as NSArray;
				if (cbUuids == null) {
					return guids;
				}

				for (nuint i = 0; i < cbUuids.Count; ++i) {
					var cbUuid = cbUuids.GetItem<CBUUID>(i);
					if (cbUuid == null) {
						continue;
					}

				}
			}
			return guids;
		}

		private static Dictionary<Guid, byte[]> ProcessData(NSDictionary advertisementData) {
			var resultData = new Dictionary<Guid, byte[]>();

			if (!advertisementData.ContainsKey(CBAdvertisement.DataServiceDataKey)) {
				return resultData;
			}

			var dataDictionary = advertisementData[CBAdvertisement.DataServiceDataKey] as NSDictionary;
			if (dataDictionary == null) {
				return resultData;
			}

			foreach (var dataPair in dataDictionary) {
				var uuid = dataPair.Key as CBUUID;
				var data = dataPair.Value as NSData;

				if (uuid == null || data == null) {
					continue;
				}

				var guid = uuid.ToString().ToGuid();
				var dataBytes = new byte[data.Length];
				System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));

				resultData[guid] = dataBytes;
			}
			return resultData;
		}

		private void UpdatedState(object sender, EventArgs e) {
			_stateChanged.Set();
		}

		private void ConnectedPeripheral(object sender, CBPeripheralEventArgs e) {
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);
			if (ConnectedDevices.All(x => x.Id != deviceId)) {
				var device = new Device(e.Peripheral);
				ConnectedDevices.Add(device);
				DeviceConnected(this, new DeviceConnectionEventArgs(device));
			}
		}

		private void DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e) {
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);
			var connectedDevice = ConnectedDevices.FirstOrDefault(x => x.Id == deviceId);

			if (connectedDevice != null) {
				ConnectedDevices.Remove(connectedDevice);
				DeviceDisconnected(this, new DeviceConnectionEventArgs(connectedDevice));
			}
		}

		private void FailedToConnectPeripheral(object sender, CBPeripheralErrorEventArgs e) {
			var device = new Device(e.Peripheral);
			var args = new DeviceConnectionEventArgs(device) {
				ErrorMessage = e.Error.Description
			};

			DeviceFailedToConnect(this, args);
		}

		#endregion
	}
}

