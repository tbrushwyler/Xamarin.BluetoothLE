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
		private IAdapter _adapter;
		private string _advertiseStatus = "Not advertising";
		public ObservableCollection<DeviceViewModel> Devices { get; private set;}

		public string AdvertiseStatus {
			get { return _advertiseStatus; }
			set {
				_advertiseStatus = value;
				RaisePropertyChanged(() => AdvertiseStatus);
			}
		}

		public DevicePageViewModel() {
			Devices= new ObservableCollection<DeviceViewModel>();
			
			_adapter = DependencyService.Get<IAdapter>();
			
			_adapter.AdvertiseStartFailed += AdapterOnAdvertiseStartFailed;
			_adapter.AdvertiseStartSuccess += AdapterOnAdvertiseStartSuccess;

			_adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
			_adapter.DeviceConnected += AdapterOnDeviceConnected;

			_adapter.StartScanningForDevices(true);

			var serviceFactory = DependencyService.Get<IServiceFactory>();
			var service = serviceFactory.CreateService("BEEF".ToGuid(), true);
			
			// Voltage
			AddCharacteristic(ref service, "BEF0".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] {0x12, 0x34});
			// Temperature
			AddCharacteristic(ref service, "BEF1".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34 });
			// ELT
			AddCharacteristic(ref service, "BEF2".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34, 0x56, 0x78 });
			// Voltage/Temperature Zone Accumulator 
			AddCharacteristic(ref service, "BEF3".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34 });
			// Time
			AddCharacteristic(ref service, "BEF5".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34, 0x56, 0x78 });
			// Configuration
			AddCharacteristic(ref service, "BEF6".ToGuid(), CharacterisiticPermissionType.Write, CharacteristicPropertyType.Write, null);
			// Event
			AddCharacteristic(ref service, "BEF7".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34 });
			// Mode
			AddCharacteristic(ref service, "BEF8".ToGuid(), CharacterisiticPermissionType.Read, CharacteristicPropertyType.Read, new byte[] { 0x12, 0x34 });

			_adapter.StartAdvertising("BattByteTest", new List<IService>() { service });

		}

		private void AddCharacteristic(ref IService service, Guid guid, CharacterisiticPermissionType permissions, CharacteristicPropertyType properties, byte[] value) {
			var charFactory = DependencyService.Get<ICharacteristicsFactory>();
			var characteristic = charFactory.Create(guid, permissions, properties);
			characteristic.Value = value;
			service.Characteristics.Add(characteristic);		}

		private void AdapterOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs) {
			if (Devices.All(x => x.Device != deviceDiscoveredEventArgs.Device)) {
				Devices.Add(new DeviceViewModel(deviceDiscoveredEventArgs.Device));
				_adapter.ConnectToDevice(deviceDiscoveredEventArgs.Device);
				RaisePropertyChanged(() => Devices);
			}
		}

		private void AdapterOnDeviceConnected(object sender, DeviceConnectionEventArgs deviceConnectionEventArgs) {
			var device = Devices.FirstOrDefault(x => x.Device.Id == deviceConnectionEventArgs.Device.Id);
			device.Device = deviceConnectionEventArgs.Device;
			device?.DeviceConnected();
		}

		private void AdapterOnAdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			AdvertiseStatus = "Advertising";
		}

		private void AdapterOnAdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			throw new Exception("Avertise failed");
			AdvertiseStatus = "Avertise failed";
		}
	}
}
