using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using BluetoothLE.Core.Factory;
using ReactiveBluetooth.Core;

namespace BluetoothLE.Core
{
    /// <summary>
    /// Adapter interface that handles device discovery and connection.
    /// </summary>
    public interface IAdapter
    {
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

        IObservable<ManagerState> CentralStateChanged { get; }
        IObservable<ManagerState> PeripheralStateChanged { get; }

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

        List<IDevice> ConnectedDevices();

        ICharacteristicsFactory CharacteristicsFactory { get; }

        IServiceFactory ServiceFactory { get; }

        ManagerState CentralState { get; }

        ManagerState PeripheralState { get; }
    }
}