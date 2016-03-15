using System;
using BluetoothLE.Core;
using System.Collections.Generic;
using Android.Bluetooth;
using System.Linq;
using Java.Util;
using System.Threading.Tasks;
using Java.Nio;
using Android.OS;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid
{
	/// <summary>
	/// Concrete implementation of <see cref="BluetoothLE.Core.IAdapter"/> interface.
	/// </summary>
	public class Adapter : Java.Lang.Object, BluetoothAdapter.ILeScanCallback, IAdapter
	{
		private readonly BluetoothManager _manager;
		private readonly BluetoothAdapter _adapter;
		private readonly GattCallback _callback;

		private BluetoothGatt _gatt;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Adapter"/> class.
		/// </summary>
		public Adapter()
		{
			var appContext = Android.App.Application.Context;
			_manager = (BluetoothManager)appContext.GetSystemService("bluetooth");
			_adapter = _manager.Adapter;

			_callback = new GattCallback();
			_callback.DeviceConnected += BluetoothGatt_DeviceConnected;
			_callback.DeviceDisconnected += BluetoothGatt_DeviceDisconnected;

			ConnectedDevices = new List<IDevice>();
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
		/// Occurs when a device failed to connect.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate {};

		/// <summary>
		/// Occurs when scan timeout elapsed.
		/// </summary>
		public event EventHandler ScanTimeoutElapsed = delegate {};

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
			DiscoveredDevices = new List<IDevice>();
			IsScanning = true;

			var uuids = new List<UUID>();
			foreach (var id in serviceUuids)
			{
				var guid = id.ToGuid();
				uuids.Add(UUID.FromString(guid.ToString("D")));
			}

			_adapter.StartLeScan(uuids.ToArray(), this);

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
			_adapter.StopLeScan(this);
		}

		/// <summary>
		/// Connect to a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void ConnectToDevice(IDevice device)
		{
			Action action = () => PerformConnectToDevice(device);

			if (Configuration.ConnectOnMainThread)
			{
				var handler = new Handler(Looper.MainLooper);
				handler.PostAtFrontOfQueue(action);
			}
			else
			{
				action.Invoke();
			}
		}

		/// <summary>
		/// Discconnect from a device.
		/// </summary>
		/// <param name="device">The device.</param>
		public void DisconnectDevice(IDevice device)
		{
			if (_gatt == null)
				return;

			Action action = () => PerformDisconnect(device);

			if (Configuration.DisconnectOnMainThread)
			{
				var handler = new Handler(Looper.MainLooper);
				handler.PostAtFrontOfQueue(action);
			}
			else
			{
				action.Invoke();
			}
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

		#region ILeScanCallback implementation

		/// <summary>
		/// Raises the le scan event.
		/// </summary>
		/// <param name="bleDevice">The BLE device that was discovered.</param>
		/// <param name="rssi">Rssi.</param>
		/// <param name="scanRecord">Scan record.</param>
		public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
		{
			var deviceId = Device.DeviceIdFromAddress(bleDevice.Address);
			if (DiscoveredDevices.All(x => x.Id != deviceId))
			{
				var device = new Device(bleDevice, null, null, rssi);
				DiscoveredDevices.Add(device);
				DeviceDiscovered(this, new DeviceDiscoveredEventArgs(device));
			}
		}

		#endregion

		#region GattCallback delegate methods

		private void BluetoothGatt_DeviceConnected(object sender, DeviceConnectionEventArgs e)
		{
			ConnectedDevices.Add(e.Device);
			DeviceConnected(this, e);
		}

		private void BluetoothGatt_DeviceDisconnected(object sender, DeviceConnectionEventArgs e)
		{
			var connDevice = ConnectedDevices.FirstOrDefault(x => x.Id == e.Device.Id);
			if (connDevice != null)
				ConnectedDevices.Remove(connDevice);
			
			DeviceDisconnected(this, e);
		}

		#endregion

		private void PerformConnectToDevice(IDevice device)
		{
			var bleDevice = device.NativeDevice as BluetoothDevice;
			if (bleDevice == null)
				return;

			var remoteDevice = _adapter.GetRemoteDevice(bleDevice.Address);
			if (remoteDevice == null)
				return;

			_gatt = remoteDevice.ConnectGatt(Android.App.Application.Context, false, _callback);
			_gatt.Connect();
		}

		private void PerformDisconnect(IDevice device)
		{
			try
			{
                _gatt.Close(); // this will not trigger the OnConnectionStateChange.OnConnectionStateChange callback
                _gatt = null;

                DeviceDisconnected(this, new DeviceConnectionEventArgs(device));
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
		}
	}
}

