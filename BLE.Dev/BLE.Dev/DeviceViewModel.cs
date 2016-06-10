using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using Xamarin.Forms;
using GalaSoft.MvvmLight;

namespace BLE.Dev {
	public class DeviceViewModel : ViewModelBase{
		private IDevice _device;
		private IAdapter _adapter;

		public IDevice Device {
			get { return _device; }
			set { _device = value; }
		}

		public string Name {
			get { return Device.Name; }
		}

		public string ServiceCount {
			get { return (Device.Services.Count == 0) ? $"No Services" : $"{Device.Services.Count} Services"; }
		}

		public DeviceViewModel(IDevice device) {
			_device = device;
		}

		public void DeviceConnected() {
			_device.DiscoverServices();
			_device.ServicesDiscovered += DeviceOnServicesDiscovered;
		}

		private void DeviceOnServicesDiscovered(object sender, ServicesDiscoveredEventArgs serviceDiscoveredEventArgs) {
			RaisePropertyChanged(() => ServiceCount);
		}

		public void Connect() {
			if (_device.State == DeviceState.Disconnected) {
				_adapter.ConnectToDevice(_device);
				_adapter.DeviceConnected += AdapterOnDeviceConnected;
			}
			ProcessConnection();
		}

		private void ProcessConnection() {
			if (_device.State == DeviceState.Disconnected)
			if (_device.State == DeviceState.Connected && _device.Services == null) {
				_device.DiscoverServices();
			} else if (_de)
		}

		public void Disconnect() {
			_device.Disconnect();
		}

		private void AdapterOnDeviceConnected(object sender, DeviceConnectionEventArgs deviceConnectionEventArgs) {
			 ProcessConnection();
		}
	}
}
