using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using GalaSoft.MvvmLight;
using Xamarin.Forms;

namespace BLE.Dev {
	class DevicePageViewModel : ViewModelBase{
		private IAdapter _adapter;
		private string _advertiseStatus = "Not advertising";
		public ObservableCollection<DeviceViewModel> Devices;

		public string AdvertiseStatus {
			get { return _advertiseStatus; }
		}

		public DevicePageViewModel() {
			_adapter = DependencyService.Get<IAdapter>();
			Devices = new ObservableCollection<DeviceViewModel>();
			var packet = new AdvertisePacket(2, 654321);

			_adapter.AdvertiseStartFailed += AdapterOnAdvertiseStartFailed;
			_adapter.AdvertiseStartSuccess += AdapterOnAdvertiseStartSuccess;
			
			_adapter.StartScanningForDevices(true);
			//_adapter.StartAdvertising("NSTest", "9900".ToGuid(), packet.ToBytes());
			_adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
			_adapter.DeviceConnected += AdapterOnDeviceConnected;
		}

		private void AdapterOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs) {
			if (Devices.All(x => x.Device != deviceDiscoveredEventArgs.Device)) {
				Devices.Add(new DeviceViewModel(deviceDiscoveredEventArgs.Device));
				_adapter.ConnectToDevice(deviceDiscoveredEventArgs.Device);
			}
		}

		private void AdapterOnDeviceConnected(object sender, DeviceConnectionEventArgs deviceConnectionEventArgs) {
			var device = Devices.FirstOrDefault(x => x.Device.Id == deviceConnectionEventArgs.Device.Id);
			device.Device = deviceConnectionEventArgs.Device;
			device?.DeviceConnected();
		}

		private void AdapterOnAdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			_advertiseStatus = "Advertising";
			RaisePropertyChanged(() => AdvertiseStatus);
		}

		private void AdapterOnAdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			throw new Exception("Avertise failed");
			_advertiseStatus = "Avertise failed";
		}
	}
}
