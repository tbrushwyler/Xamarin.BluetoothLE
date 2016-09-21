using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Core {
    public enum AdapterState
    {
        /// <summary>
        ///  State unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Resetting
        /// </summary>
        Resetting,
        /// <summary>
        /// BLE is not supported
        /// </summary>
        Unsupported,
        /// <summary>
        /// BLE usage is not authorized
        /// </summary>
        Unauthorized,
        /// <summary>
        /// BLE is off
        /// </summary>
        PoweredOff,
        /// <summary>
        /// BLE is on
        /// </summary>
        PoweredOn
    }

	/// <summary>
	/// Adapter interface that handles device discovery and connection.
	/// </summary>
	public interface IAdapter {
		/// <summary>
		/// Occurs when device discovered.
		/// </summary>
		event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;

		/// <summary>
		/// Occurs when device connected.
		/// </summary>
		event EventHandler<DeviceConnectionEventArgs> DeviceConnected;

		/// <summary>
		/// Occurs when device disconnected.
		/// </summary>
		event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected;

		/// <summary>
		/// Occurs when a device failed to connect.
		/// </summary>
		event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect;

		/// <summary>
		/// Occurs when advertising start fails
		/// </summary>
		event EventHandler<AdvertiseStartEventArgs> AdvertiseStartFailed;

		/// <summary>
		/// Occurs when advertising start succeeds
		/// </summary>
		event EventHandler<AdvertiseStartEventArgs> AdvertiseStartSuccess;

	    /// <summary>
	    /// Occurs when scan timeout elapsed.
	    /// </summary>
	    event EventHandler<DevicesDiscoveredEventArgs> ScanTimeoutElapsed;

        /// <summary>
        /// Gets a value indicating whether this instance is scanning.
        /// </summary>
        /// <value><c>true</c> if this instance is scanning; otherwise, <c>false</c>.</value>
        bool IsScanning { get; }

		/// <summary>
		/// Gets or sets the amount of time to scan for devices.
		/// </summary>
		/// <value>The scan timeout.</value>
		TimeSpan ScanTimeout { get; set; }

		/// <summary>
		/// Gets or sets the amount of time to attempt to connect to a device.
		/// </summary>
		/// <value>The connection timeout.</value>
		TimeSpan ConnectionTimeout { get; set; }

		///// <summary>
		///// Gets the discovered devices.
		///// </summary>
		///// <value>The discovered devices.</value>
		//IList<IDevice> DiscoveredDevices { get; }

		///// <summary>
		///// Gets the connected devices.
		///// </summary>
		///// <value>The connected devices.</value>
		//IList<IDevice> ConnectedDevices { get; }

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		void StartScanningForDevices();

		/// <summary>
		/// Start scanning for devices.
		/// </summary>
		/// <param name="serviceUuids">White-listed service UUIDs</param>
		void StartScanningForDevices(params string[] serviceUuids);

	    void StartContinuosScan();

		/// <summary>
		/// Stop scanning for devices.
		/// </summary>
		void StopScanningForDevices();
		
		/// <summary>
		/// Connect to a device.
		/// </summary>
		/// <param name="device">The device.</param>
		void ConnectToDevice(IDevice device);

		/// <summary>
		/// Discconnect from a device.
		/// </summary>
		/// <param name="device">The device.</param>
		void DisconnectDevice(IDevice device);

		Task StartAdvertising(string localName, List<IService> services);

		/// <summary>
		/// Stop advertising
		/// </summary>
		void StopAdvertising();

		/// <summary>
		/// Checks if the device supports advertising and peripheral mode, iOS versions above 6.0 and Android devices that pass tests
		/// </summary>
		/// <returns></returns>
		bool SupportsAdvertising();
	}
}

