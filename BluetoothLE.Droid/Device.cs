using System;
using BluetoothLE.Core;
using Android.Bluetooth;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid
{

	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.IDevice" /> interface
	/// </summary>
	public class Device : IDevice
	{
		private readonly BluetoothDevice _nativeDevice;
		private readonly BluetoothGatt _gatt;
		private readonly GattCallback _callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Device"/> class.
		/// </summary>
		/// <param name="nativeDevice">Native device.</param>
		/// <param name="gatt">Native Gatt.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="rssi">Rssi.</param>
		public Device(BluetoothDevice nativeDevice, BluetoothGatt gatt, GattCallback callback, int rssi)
		{
			_nativeDevice = nativeDevice;
			_gatt = gatt;
			_callback = callback;

			_rssi = rssi;
			_id = DeviceIdFromAddress(_nativeDevice.Address);

			if (_callback != null)
			{
				_callback.ServicesDiscovered += ServicesDiscovered;
			}

			Services = new List<IService>();
		}

		/// <summary>
		/// Gets a device identifier from a MAC address
		/// </summary>
		/// <returns>The device identifier.</returns>
		/// <param name="address">The MAC address.</param>
		public static Guid DeviceIdFromAddress(string address)
		{
			var deviceGuid = new Byte[16];
			var macWithoutColons = address.Replace(":", "");
			var macBytes = Enumerable.Range(0, macWithoutColons.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
				.ToArray();
			macBytes.CopyTo(deviceGuid, 10);

			return new Guid(deviceGuid);
		}

		#region IDevice implementation

		/// <summary>
		/// Occurs when services discovered.
		/// </summary>
		public event EventHandler<ServiceDiscoveredEventArgs> ServiceDiscovered = delegate {};

		/// <summary>
		/// Initiate a service discovery on the device
		/// </summary>
		public void DiscoverServices()
		{
			Action action = () => _gatt.DiscoverServices();

			if (Configuration.DiscoverServicesOnMainThread)
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
		/// Discconnect from the device.
		/// </summary>
		public void Disconnect()
		{
			if (_gatt == null)
				return;

			try
			{
				_gatt.Disconnect();
				_gatt.Close();

				State = DeviceState.Disconnected;
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		private readonly Guid _id;
		/// <summary>
		/// Gets the unique identifier for the device
		/// </summary>
		/// <value>The device identifier</value>
		public Guid Id { get { return _id; } }

		/// <summary>
		/// Gets the device name
		/// </summary>
		/// <value>The device name</value>
		public string Name { get { return _nativeDevice.Name; } }

		private readonly int _rssi;
		/// <summary>
		/// Gets the Received Signal Strength Indicator
		/// </summary>
		/// <value>The RSSI in decibels</value>
		public int Rssi { get { return _rssi; } }

		/// <summary>
		/// Gets the native device object reference. Should be cast to the appropriate type.
		/// </summary>
		/// <value>The native device</value>
		public object NativeDevice { get { return _nativeDevice; } }

		/// <summary>
		/// Gets the state of the device
		/// </summary>
		/// <value>The device's state</value>
		public DeviceState State { get; set; }

		/// <summary>
		/// Gets the discovered services for the device
		/// </summary>
		/// <value>The device's services</value>
		public IList<IService> Services { get; set; }

		#endregion

		#region GattCallback delegate methods

		private void ServicesDiscovered(object sender, EventArgs e)
		{
			foreach (var s in _gatt.Services)
			{
				var service = new Service(s, _gatt, _callback);
				Services.Add(service);

				ServiceDiscovered(this, new ServiceDiscoveredEventArgs(service));
			}
		}

		#endregion
	}
}

