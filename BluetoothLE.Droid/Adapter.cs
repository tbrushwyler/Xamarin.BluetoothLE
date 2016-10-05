using System;
using BluetoothLE.Core;
using System.Collections.Generic;
using System.IO;
using Android.Bluetooth;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Java.Util;
using System.Threading.Tasks;
using Android.Bluetooth.LE;
using Android.Content;
using Java.Nio;
using Android.OS;
using BluetoothLE.Core.Events;
using BluetoothLE.Core.Factory;
using BluetoothLE.Droid.Factory;
using ReactiveBluetooth.Android.Common;
using ReactiveBluetooth.Core;

namespace BluetoothLE.Droid
{
    /// <summary>
    /// Concrete implementation of <see cref="BluetoothLE.Core.IAdapter"/> interface.
    /// </summary>
    public class Adapter : Java.Lang.Object, IAdapter
    {
        private readonly BluetoothAdapter _adapter;
        private readonly BluetoothGattServer _gattServer;

        private readonly GattCallback _callback;
        private readonly AdvertiseCallback _advertiseCallback;
        private ScanCallback _scanCallback;

        private List<IDevice> _devices = new List<IDevice>();
        private List<IDevice> _discoveringDevices = new List<IDevice>();

        private CancellationTokenSource _scanCancellationToken;
        private BluetoothManager _manager;
        private BroadcastListener _broadcastListener;

        private ManagerState _peripheralState;
        private ManagerState _centralState;
        private IDisposable _peripheralStateDisposable;
        private IDisposable _centralStateDisposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothLE.Droid.Adapter"/> class.
        /// </summary>
        public Adapter()
        {
            var appContext = Android.App.Application.Context;
            _manager = (BluetoothManager) appContext.GetSystemService(Context.BluetoothService);
            _adapter = _manager.Adapter;

            _callback = new GattCallback();
            _callback.DeviceConnected += BluetoothGatt_DeviceConnected;
            _callback.DeviceDisconnected += BluetoothGatt_DeviceDisconnected;

            _advertiseCallback = new AdvertiseCallback();
            _advertiseCallback.AdvertiseStartFailed += BluetoothGatt_AdvertiseStartFailed;
            _advertiseCallback.AdvertiseStartSuccess += Bluetooth_AdvertiseStartSuccess;

            var callback = new GattServerCallback();
            _gattServer = _manager.OpenGattServer(appContext, callback);
            callback.Server = _gattServer;
            _broadcastListener = new BroadcastListener();

            CharacteristicsFactory = new CharacteristicsFactory();
            ServiceFactory = new ServiceFactory();

            PeripheralStateChanged = _broadcastListener.StateUpdatedSubject.Select(state =>
            {
                if (state == ManagerState.PoweredOn)
                {
                    if (_adapter.BluetoothLeAdvertiser == null)
                    {
                        return ManagerState.Unsupported;
                    }
                    if (!_adapter.IsMultipleAdvertisementSupported)
                    {
                        return ManagerState.PartialSupport;
                    }
                }
                return state;
            }).AsObservable();

            CentralStateChanged = _broadcastListener.StateUpdatedSubject.Select(state =>
            {
                if (state == ManagerState.PoweredOn)
                {
                    if (_adapter.BluetoothLeScanner == null)
                    {
                        return ManagerState.Unsupported;
                    }
                    if (!(_adapter.IsOffloadedFilteringSupported && _adapter.IsOffloadedScanBatchingSupported))
                    {
                        return ManagerState.PartialSupport;
                    }
                }

                return state;
            }).AsObservable();

            _peripheralStateDisposable = PeripheralStateChanged.Subscribe(state => _peripheralState = state);
            _centralStateDisposable = CentralStateChanged.Subscribe(state => _centralState = state);
        }

        public ManagerState CentralState
        {
            get
            {
                return ManagerState.PoweredOn;
            }
        } 
        public ManagerState PeripheralState => _peripheralState;

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
        public event EventHandler<DevicesDiscoveredEventArgs> ScanTimeoutElapsed = delegate { };

        /// <summary>
        /// Occurs when advertising start fails
        /// </summary>
        public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartFailed = delegate { };

        /// <summary>
        /// Occurs when advertising start succeeds
        /// </summary>
        public event EventHandler<AdvertiseStartEventArgs> AdvertiseStartSuccess = delegate { };

        public IObservable<ManagerState> CentralStateChanged { get; }
        public IObservable<ManagerState> PeripheralStateChanged { get; }

        public ICharacteristicsFactory CharacteristicsFactory { get; }
        public IServiceFactory ServiceFactory { get; }

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
            if (IsScanning)
            {
                System.Diagnostics.Debug.WriteLine("Already scanning");
                return;
            }

            IsScanning = true;

            var uuids = new List<UUID>();
            if (serviceUuids != null)
            {
                foreach (var id in serviceUuids)
                {
                    var guid = id.ToGuid();
                    uuids.Add(UUID.FromString(guid.ToString("D")));
                }
            }

            // Clear discover list
            _discoveringDevices = new List<IDevice>();

            CreateScanCallback();

            _adapter.BluetoothLeScanner.StartScan(_scanCallback);

            _scanCancellationToken = new CancellationTokenSource();
            try
            {
                await Task.Delay(ScanTimeout, _scanCancellationToken.Token);
                _scanCancellationToken = null;
            }
            catch (Exception)
            {
                // ignored
            }

            if (IsScanning)
            {
                StopScanningForDevices();
                var currentDevices = _devices.Select(x => x.Id);
                var newDevices = _discoveringDevices.Select(x => x.Id);
                var removeList = currentDevices.Except(newDevices);
                _devices.RemoveAll(x => removeList.Any(g => g == x.Id));
                ScanTimeoutElapsed(this, new DevicesDiscoveredEventArgs(_discoveringDevices));
            }
        }

        private void CreateScanCallback()
        {
            if (_scanCallback != null)
            {
                _scanCallback.DeviceDiscovered -= ScanCallbackOnDeviceDiscovered;
            }
            _discoveringDevices = new List<IDevice>();

            _scanCallback = new ScanCallback();
            _scanCallback.DeviceDiscovered += ScanCallbackOnDeviceDiscovered;
        }

        public void StartContinuosScan()
        {
            if (IsScanning)
            {
                System.Diagnostics.Debug.WriteLine("Already scanning");
                return;
            }

            IsScanning = true;
            CreateScanCallback();

            _adapter.BluetoothLeScanner?.StartScan(_scanCallback);
        }

        /// <summary>
        /// Stop scanning for devices.
        /// </summary>
        public void StopScanningForDevices()
        {
            if (IsScanning && _scanCancellationToken != null)
            {
                try
                {
                    _scanCancellationToken.Cancel();
                }
                catch (TaskCanceledException e)
                {
                    // ignored
                }
            }
            IsScanning = false;
            _adapter.BluetoothLeScanner?.StopScan(_scanCallback);
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

        public Task StartAdvertising(string localName, List<IService> services)
        {
            var settings = new AdvertiseSettings.Builder()
                .SetAdvertiseMode(AdvertiseMode.LowLatency)
                .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                .SetTimeout(0)
                .SetConnectable(true)
                .Build();

            if (localName != null)
            {
                _adapter.SetName(localName);
            }

            var advertiseDataBuilder = new AdvertiseData.Builder()
                .SetIncludeTxPowerLevel(false)
                .SetIncludeDeviceName(false);

            if (services != null)
            {
                foreach (Service service in services)
                {
                    var serviceUuid = service.Uuid;
                    var uuid = UUID.FromString(serviceUuid);
                    var parcelUuid = new ParcelUuid(uuid);
                    advertiseDataBuilder.AddServiceUuid(parcelUuid);
                }
            }
            var advertiseData = advertiseDataBuilder.Build();

            _adapter.BluetoothLeAdvertiser.StartAdvertising(settings, advertiseData, _advertiseCallback);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Stops peripheral advertising
        /// </summary>
        public void StopAdvertising()
        {
            _adapter.BluetoothLeAdvertiser.StopAdvertising(_advertiseCallback);
        }

        public bool SupportsAdvertising()
        {
            var multi = _adapter.IsMultipleAdvertisementSupported;

            var batching = _adapter.IsOffloadedScanBatchingSupported;
            var filtering = _adapter.IsOffloadedFilteringSupported;

            String missingFeatures = "";
            if (!multi)
            {
                missingFeatures += $"MultipleAdvertisment";
            }
            if (!batching)
            {
                missingFeatures += $" OffloadedScanBatching";
            }
            if (!filtering)
            {
                missingFeatures += $" OffloadedFiltering";
            }
            var support = multi && batching && filtering;
            return support;
        }

        public List<IDevice> ConnectedDevices()
        {
            return _manager?.GetConnectedDevices(ProfileType.Gatt).Select(x => new Device(x, null, _callback, 0)).Cast<IDevice>().ToList();
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
        //public IList<IDevice> DiscoveredDevices => _devices.ToList();
        /// <summary>
        /// Gets the connected devices.
        /// </summary>
        /// <value>The connected devices.</value>
        //public IList<IDevice> ConnectedDevices {
        //    get {
        //        return _devices.Where(x => x.State == DeviceState.Connected).ToList();
        //    }
        //}

        #endregion

        #region GattCallback delegate methods
        private void BluetoothGatt_DeviceConnected(object sender, DeviceConnectionEventArgs e)
        {
            DeviceConnected(this, e);
        }

        private void BluetoothGatt_DeviceDisconnected(object sender, DeviceConnectionEventArgs e)
        {
            DeviceDisconnected(this, e);
        }

        #endregion

        private void PerformConnectToDevice(IDevice device)
        {
            var androidDevice = (Device) device;
            var bleDevice = device.NativeDevice as BluetoothDevice;
            if (bleDevice == null)
                return;

            var remoteDevice = _adapter.GetRemoteDevice(bleDevice.Address);
            if (remoteDevice == null)
                return;

            var gatt = remoteDevice.ConnectGatt(Android.App.Application.Context, false, _callback);
            androidDevice.Gatt = gatt;
            gatt.Connect();
        }

        private void PerformDisconnect(IDevice device)
        {
            try
            {
                device.Disconnect();


                DeviceDisconnected(this, new DeviceConnectionEventArgs(device));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private void ScanCallbackOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs)
        {
            DeviceDiscovered(this, new DeviceDiscoveredEventArgs(deviceDiscoveredEventArgs.Device));
        }

        private void Bluetooth_AdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs)
        {
            AdvertiseStartSuccess?.Invoke(this, advertiseStartEventArgs);
        }

        private void BluetoothGatt_AdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs)
        {
            AdvertiseStartFailed?.Invoke(this, advertiseStartEventArgs);
        }
    }

    public class GattServerCallback : BluetoothGattServerCallback
    {
        public BluetoothGattServer Server { get; set; }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            var value = characteristic.GetValue();
            Server.SendResponse(device, requestId, GattStatus.Success, offset, value);
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            characteristic.SetValue(value);
            if (responseNeeded)
            {
                Server.SendResponse(device, requestId, GattStatus.Success, offset, value);
            }
        }
    }
}