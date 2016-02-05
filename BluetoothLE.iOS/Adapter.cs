using System;
using BluetoothLE.Core;
using CoreBluetooth;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using BluetoothLE.Core.Events;

namespace BluetoothLE.iOS
{
	/// <summary>
	/// Concrete implementation of <see cref="BluetoothLE.Core.IAdapter"/> interface.
	/// </summary>
	public class Adapter : IAdapter
	{
		private readonly CBCentralManager _central;
		private readonly AutoResetEvent _stateChanged;

		private static Adapter _current;

		/// <summary>
		/// Gets the current Adpater instance
		/// </summary>
		/// <value>The current Adapter instance</value>
		public static Adapter Current
		{
			get { return _current; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Adapter"/> class.
		/// </summary>
		public Adapter()
		{
			_central = new CBCentralManager();

			_central.DiscoveredPeripheral += DiscoveredPeripheral;
			_central.UpdatedState += UpdatedState;
			_central.ConnectedPeripheral += ConnectedPeripheral;
			_central.DisconnectedPeripheral += DisconnectedPeripheral;
			_central.FailedToConnectPeripheral += FailedToConnectPeripheral;

			ConnectedDevices = new List<IDevice>();
			_stateChanged = new AutoResetEvent(false);

			_current = this;
		}

		private async Task WaitForState(CBCentralManagerState state)
		{
			while (_central.State != state)
			{
				await Task.Run(() => _stateChanged.WaitOne());
			}
		}

		#region IAdapter implementation

		/// <summary>
		/// Occurs when device discovered.
		/// </summary>
		public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate {};

		/// <summary>
		/// Occurs when device connected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate {};

		/// <summary>
		/// Occurs when device disconnected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate {};

		/// <summary>
		/// Occurs when scan timeout elapsed.
		/// </summary>
		public event EventHandler ScanTimeoutElapsed = delegate {};

		/// <summary>
		/// Occurs when a device failed to connect.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate {};

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
		public void StartScanningForDevices()
		{
			StartScanningForDevices(new string[0]);
		}

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		/// <param name="serviceUuids">White-listed service UUIDs</param>
		public async void StartScanningForDevices(params string[] serviceUuids)
		{
			await WaitForState(CBCentralManagerState.PoweredOn);

			var uuids = new List<CBUUID>();
			foreach (var guid in serviceUuids)
			{
				uuids.Add(CBUUID.FromString(guid));
			}

			DiscoveredDevices = new List<IDevice>();
			IsScanning = true;

			_central.ScanForPeripherals(uuids.ToArray());

			await Task.Delay(ScanTimeout);

			if (IsScanning)
			{
				StopScanningForDevices();
				ScanTimeoutElapsed(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Stop scanning for devices.
		/// </summary>
		public void StopScanningForDevices()
		{
			IsScanning = false;
			_central.StopScan();
		}

		/// <summary>
		/// Connect to a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public async void ConnectToDevice(IDevice device)
		{
			var peripheral = device.NativeDevice as CBPeripheral;
			_central.ConnectPeripheral(peripheral);

			await Task.Delay(ConnectionTimeout);

			if (ConnectedDevices.All(x => x.Id != device.Id))
			{
				_central.CancelPeripheralConnection(peripheral);
				var args = new DeviceConnectionEventArgs(device)
				{
					ErrorMessage = "The device connection timed out."
				};

				DeviceFailedToConnect(this, args);
			}
		}

		/// <summary>
		/// Discconnect from a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void DisconnectDevice(IDevice device)
		{
			var peripheral = device.NativeDevice as CBPeripheral;
			_central.CancelPeripheralConnection(peripheral);
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

		private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
		{
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);
			if (DiscoveredDevices.All(x => x.Id != deviceId))
			{
				var device = new Device(e.Peripheral);
				DiscoveredDevices.Add(device);
				DeviceDiscovered(this, new DeviceDiscoveredEventArgs(device));
			}
		}

		private void UpdatedState(object sender, EventArgs e)
		{
			_stateChanged.Set();
		}

		private void ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
		{
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);
			if (ConnectedDevices.All(x => x.Id != deviceId))
			{
				var device = new Device(e.Peripheral);
				ConnectedDevices.Add(device);
				DeviceConnected(this, new DeviceConnectionEventArgs(device));
			}
		}

		private void DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
		{
			var deviceId = Device.DeviceIdentifierToGuid(e.Peripheral.Identifier);
			var connectedDevice = ConnectedDevices.FirstOrDefault(x => x.Id == deviceId);

			if (connectedDevice != null)
			{
				ConnectedDevices.Remove(connectedDevice);
				DeviceDisconnected(this, new DeviceConnectionEventArgs(connectedDevice));
			}
		}

		private void FailedToConnectPeripheral(object sender, CBPeripheralErrorEventArgs e)
		{
			var device = new Device(e.Peripheral);
			var args = new DeviceConnectionEventArgs(device) {
				ErrorMessage = e.Error.Description
			};

			DeviceFailedToConnect(this, args);
		}

		#endregion
	}
}

