using System;
using System.Collections.Generic;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Core
{
	/// <summary>
	/// The device interface.
	/// </summary>
	public interface IDevice
	{
		/// <summary>
		/// Occurs when services discovered.
		/// </summary>
		event EventHandler<ServiceDiscoveredEventArgs> ServiceDiscovered;

		/// <summary>
		/// Gets the unique identifier for the device
		/// </summary>
		/// <value>The device identifier</value>
		Guid Id { get; }

		/// <summary>
		/// Gets the device name
		/// </summary>
		/// <value>The device name</value>
		string Name { get; }

		/// <summary>
		/// Gets the Received Signal Strength Indicator
		/// </summary>
		/// <value>The RSSI in decibels</value>
		int Rssi { get; }

		/// <summary>
		/// Gets the native device object reference. Should be cast to the appropriate type.
		/// </summary>
		/// <value>The native device</value>
		object NativeDevice { get; }

		/// <summary>
		/// Gets the state of the device
		/// </summary>
		/// <value>The device's state</value>
		DeviceState State { get; }

		/// <summary>
		/// Gets the discovered services for the device
		/// </summary>
		/// <value>The device's services</value>
		IList<IService> Services { get; }

		/// <summary>
		/// Initiate a service discovery on the device
		/// </summary>
		void DiscoverServices();

		/// <summary>
		/// Discconnect from the device.
		/// </summary>
		void Disconnect();
	}
}

