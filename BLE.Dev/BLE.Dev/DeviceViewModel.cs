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
			_device.ServiceDiscovered += DeviceOnServiceDiscovered;
		}

		private void DeviceOnServiceDiscovered(object sender, ServiceDiscoveredEventArgs serviceDiscoveredEventArgs) {
			RaisePropertyChanged(() => ServiceCount);
		}
	}
}
