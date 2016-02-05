using System;
using BluetoothLE.Core;
using System.Collections.Generic;
using CoreBluetooth;
using Foundation;
using System.Linq;
using BluetoothLE.Core.Events;

namespace BluetoothLE.iOS
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.IDevice" /> interface
	/// </summary>
	public class Device : IDevice, IDisposable
	{
		private readonly CBPeripheral _peripheral;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Device"/> class.
		/// </summary>
		/// <param name="peripheral">Native peripheral.</param>
		public Device(CBPeripheral peripheral)
		{
			_peripheral = peripheral;
			_id = DeviceIdentifierToGuid(_peripheral.Identifier);

			_peripheral.DiscoveredService += DiscoveredService;

			Services = new List<IService>();
		}

		/// <summary>
		/// Gets the device identifier.
		/// </summary>
		/// <returns>The device identifier as a Guid.</returns>
		/// <param name="id">The device identifier as a NSUuid.</param>
		public static Guid DeviceIdentifierToGuid(NSUuid id)
		{
			return Guid.ParseExact(id.AsString(), "d");
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
			_peripheral.DiscoverServices();
		}

		/// <summary>
		/// Discconnect from the device.
		/// </summary>
		public void Disconnect()
		{
			Adapter.Current.DisconnectDevice(this);
			_peripheral.Dispose();
		}

		private Guid _id;
		/// <summary>
		/// Gets the unique identifier for the device
		/// </summary>
		/// <value>The device identifier</value>
		public Guid Id { get { return _id; } }

		/// <summary>
		/// Gets the device name
		/// </summary>
		/// <value>The device name</value>
		public string Name { get { return _peripheral.Name; } }

		/// <summary>
		/// Gets the Received Signal Strength Indicator
		/// </summary>
		/// <value>The RSSI in decibels</value>
		public int Rssi { get { return _peripheral.RSSI.Int32Value; } }

		/// <summary>
		/// Gets the native device object reference. Should be cast to the appropriate type.
		/// </summary>
		/// <value>The native device</value>
		public object NativeDevice { get { return _peripheral; } }

		/// <summary>
		/// Gets the state of the device
		/// </summary>
		/// <value>The device's state</value>
		public DeviceState State
		{
			get
			{
				switch (_peripheral.State)
				{
					case CBPeripheralState.Connected:
						return DeviceState.Connected;
					case CBPeripheralState.Connecting:
						return DeviceState.Connecting;
					case CBPeripheralState.Disconnected:
						return DeviceState.Disconnected;
					default:
						return DeviceState.Disconnected;
				}
			}
		}

		/// <summary>
		/// Gets the discovered services for the device
		/// </summary>
		/// <value>The device's services</value>
		public IList<IService> Services { get; set; }

		#endregion

		#region CBPeripheral delegate methods

		private void DiscoveredService(object sender, NSErrorEventArgs args)
		{
			if (_peripheral.Services != null) 
			{
				foreach (var s in _peripheral.Services)
				{
					var serviceId = s.UUID.ToString().ToGuid();
					if (Services.All(x => x.Id != serviceId))
					{
						var service = new Service(_peripheral, s);
						Services.Add(service);
						ServiceDiscovered(this, new ServiceDiscoveredEventArgs(service));
					}
				}
			}
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="BluetoothLE.iOS.Device"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="BluetoothLE.iOS.Device"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="BluetoothLE.iOS.Device"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="BluetoothLE.iOS.Device"/> so the garbage
		/// collector can reclaim the memory that the <see cref="BluetoothLE.iOS.Device"/> was occupying.</remarks>
		public void Dispose()
		{
			_peripheral.DiscoveredService -= DiscoveredService;
		}

		#endregion
	}
}

