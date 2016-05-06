using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;

namespace BLE.Dev {
	partial class DevicePage : ContentPage {
		private IAdapter _adapter;
		private ObservableCollection<DeviceViewModel> _devices;

		public DevicePage() {
			InitializeComponent();

			_adapter = DependencyService.Get<IAdapter>();
			_devices = new ObservableCollection<DeviceViewModel>();
			var packet = new AdvertisePacket(2, 654321);
			
			_adapter.AdvertiseStartFailed += AdapterOnAdvertiseStartFailed;
			_adapter.AdvertiseStartSuccess += AdapterOnAdvertiseStartSuccess;
			// Limit to northstar services
			_adapter.StartScanningForDevices(true);
			_adapter.StartAdvertising("NSTest", "9900".ToGuid(), packet.ToBytes());
			_adapter.DeviceDiscovered += AdapterOnDeviceDiscovered;
			_adapter.DeviceConnected += AdapterOnDeviceConnected;
			ListView.ItemsSource = _devices;
			ListView.ItemSelected += ListViewOnItemSelected;
		}

		private void AdapterOnAdvertiseStartSuccess(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			
		}

		private void AdapterOnAdvertiseStartFailed(object sender, AdvertiseStartEventArgs advertiseStartEventArgs) {
			throw new Exception("Avertise failed");
		}

		private void ListViewOnItemSelected(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs) {
			var deviceViewModel = (DeviceViewModel) selectedItemChangedEventArgs.SelectedItem;
			
			var detailPage = new DeviceDetailPage() {
				BindingContext = deviceViewModel
			};
			Navigation.PushAsync(detailPage);
		}

		private void AdapterOnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs deviceDiscoveredEventArgs) {
			if (_devices.All(x => x.Device != deviceDiscoveredEventArgs.Device)) {
				_devices.Add(new DeviceViewModel(deviceDiscoveredEventArgs.Device));
				_adapter.ConnectToDevice(deviceDiscoveredEventArgs.Device);
			}
		}

		private void AdapterOnDeviceConnected(object sender, DeviceConnectionEventArgs deviceConnectionEventArgs) {
			var device = _devices.FirstOrDefault(x => x.Device.Id == deviceConnectionEventArgs.Device.Id);
			device.Device = deviceConnectionEventArgs.Device;
			device?.DeviceConnected();
		}
	}
}
