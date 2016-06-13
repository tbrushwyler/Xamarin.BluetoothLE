using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using BluetoothLE.Core.Factory;
using GalaSoft.MvvmLight;
using Xamarin.Forms;

namespace BLE.Dev {
	class DevicePageViewModel : ViewModelBase{
		private static readonly Guid BattByteServiceGuid = "BEEF".ToGuid();
		private static readonly Guid VoltageGuid = "BEF0".ToGuid();
		private static readonly Guid TemperatureGuid = "BEF1".ToGuid();
		private static readonly Guid EltGuid = "BEF2".ToGuid();
		private static readonly Guid VoltageTemperatureZoneAccGuid = "BEF3".ToGuid();
		private static readonly Guid TimeGuid = "BEF5".ToGuid();
		private static readonly Guid ConfigurationGuid = "BEF6".ToGuid();
		
		private readonly IAdapter _adapter;
		private string _advertiseStatus = "Not advertising";
		private string _serialNumber;
		private ushort _voltage;
		private short _temperature;
		private uint _elt;
		private uint[] _voltageTemperatureZoneAcc;
		private uint _time;
		private int _configuration;
		private int _signalStrength;
		public ObservableCollection<DeviceViewModel> Devices { get; private set;}

		public string AdvertiseStatus {
			get { return _advertiseStatus; }
			set {
				_advertiseStatus = value;
				RaisePropertyChanged(() => AdvertiseStatus);
			}
		}

		public string SerialNumber {
			get { return _serialNumber; }
			set {
				_serialNumber = value;
				RaisePropertyChanged(() => SerialNumber);
			}
		}

		public UInt16 Voltage {
			get { return _voltage; }
			set {
				_voltage = value;
				RaisePropertyChanged(() => Voltage);
			}
		}

		public Int16 Temperature {
			get { return _temperature; }
			set {
				_temperature = value;
				RaisePropertyChanged(() => Temperature);
			}
		}

		public UInt32 Elt {
			get { return _elt; }
			set {
				_elt = value;
				RaisePropertyChanged(() => Elt);
			}
		}

		public UInt32[] VoltageTemperatureZoneAcc {
			get { return _voltageTemperatureZoneAcc; }
			set {
				_voltageTemperatureZoneAcc = value;
				RaisePropertyChanged(() => VoltageTemperatureZoneAcc);
			}
		}

		public UInt32 Time {
			get { return _time; }
			set {
				_time = value;
				RaisePropertyChanged(() => Time);
			}
		}

		public int Configuration {
			get { return _configuration; }
			set {
				_configuration = value; 
				RaisePropertyChanged(() => Configuration);
			}
		}

		public int SignalStrength {
			get { return _signalStrength; }
			set {
				_signalStrength = value; 
				RaisePropertyChanged(() => SignalStrength);
			}
		}

		public DevicePageViewModel() {
			Devices= new ObservableCollection<DeviceViewModel>();
			
			_adapter = DependencyService.Get<IAdapter>();
			_adapter.SupportsAdvertising();
			_adapter.AdvertiseStartFailed += AdapterOnAdvertiseStartFailed;
			_adapter.AdvertiseStartSuccess += AdapterOnAdvertiseStartSuccess;

			_adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;

			var serviceFactory = DependencyService.Get<IServiceFactory>();
			var service = serviceFactory.CreateService(BattByteServiceGuid, true);
			
			// Voltage
			AddCharacteristic(ref service, VoltageGuid, CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, BitConverter.GetBytes(Voltage));
			// Temperature
			AddCharacteristic(ref service, TemperatureGuid, CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, BitConverter.GetBytes(Temperature));
			// ELT
			AddCharacteristic(ref service, EltGuid, CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, BitConverter.GetBytes(Temperature));
			// Voltage/Temperature Zone Accumulator 
			AddCharacteristic(ref service, VoltageTemperatureZoneAccGuid, CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34 });
			// Time
			AddCharacteristic(ref service, TimeGuid, CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, BitConverter.GetBytes(Time));
			// Configuration
			AddCharacteristic(ref service, ConfigurationGuid, CharacterisiticPermissionType.Write | CharacterisiticPermissionType.Read, CharacteristicPropertyType.Write | CharacteristicPropertyType.Read, BitConverter.GetBytes(Configuration));

			_adapter.StartAdvertising("BattByteTest", new List<IService>() { service });

		}

		private void AddCharacteristic(ref IService service, Guid guid, CharacterisiticPermissionType permissions, CharacteristicPropertyType properties, byte[] value) {
			var charFactory = DependencyService.Get<ICharacteristicsFactory>();
			var characteristic = charFactory.Create(guid, permissions, properties);
			characteristic.ValueUpdated += CharacteristicOnValueUpdated;
			characteristic.Value = value;
			service.Characteristics.Add(characteristic);		}

		private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdateEventArgs characteristicReadEventArgs) {
			var characteristic = characteristicReadEventArgs.Characteristic;

			if (characteristic.Id == VoltageGuid) {
				Voltage = BitConverter.ToUInt16(characteristic.Value, 0);
			}
			if (characteristic.Id == TemperatureGuid) {
				Temperature = BitConverter.ToInt16(characteristic.Value, 0);
			}
			if (characteristic.Id == EltGuid) {
				Elt = BitConverter.ToUInt32(characteristic.Value, 0);
			}
			if (characteristic.Id == VoltageTemperatureZoneAccGuid) {
				// Keh?
			}
			if (characteristic.Id == TimeGuid) {
				Time = BitConverter.ToUInt32(characteristic.Value, 0);
			}

			if (characteristic.Id == ConfigurationGuid) {
				Configuration = BitConverter.ToInt32(characteristic.Value, 0);
			}
		}

		private void AdapterOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs) {
			if (Devices.All(x => x.Device != deviceDiscoveredEventArgs.Device)) {
				Devices.Add(new DeviceViewModel(deviceDiscoveredEventArgs.Device));
				_adapter.ConnectToDevice(deviceDiscoveredEventArgs.Device);
				RaisePropertyChanged(() => Devices);
			}
		}

		private void AdapterOnAdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			AdvertiseStatus = "Advertising";
		}

		private void AdapterOnAdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			throw new Exception("Avertise failed");
			AdvertiseStatus = "Avertise failed";
		}

		public void Refresh() {
			_adapter.StartScanningForDevices(true);
		}

		public void Clear() {
			Devices.Clear();
		}
	}
}
