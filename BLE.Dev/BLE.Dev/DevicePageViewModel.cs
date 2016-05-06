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
			var packet = new AdvertisePacket(2, 654321);

			_adapter.AdvertiseStartFailed += AdapterOnAdvertiseStartFailed;
			_adapter.AdvertiseStartSuccess += AdapterOnAdvertiseStartSuccess;

			_adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
			_adapter.DeviceConnected += AdapterOnDeviceConnected;

			_adapter.StartScanningForDevices(true);
			_adapter.StartAdvertising("NSTest", "9900".ToGuid(), packet.ToBytes());

		}

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
