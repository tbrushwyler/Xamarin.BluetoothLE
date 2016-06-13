using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using Xamarin.Forms;
using GalaSoft.MvvmLight;

namespace BLE.Dev {
	public class ServiceViewModel : ViewModelBase {
		private string _id;

		public string Id {
			get { return _id; }
			set {
				_id = value; 
				RaisePropertyChanged(() => Id);
			} 
		}
	}

	public class DeviceViewModel : ViewModelBase {
		private IDevice _device;
		private IAdapter _adapter;
		private String _name;
		private string _id;
		private bool _isDiscoveringServices;

		public IDevice Device {
			get { return _device; }
			set { _device = value; }
		}

		public string Name {
			get { return _name; }
			set {
				_name = value;
				RaisePropertyChanged(() => Name);
			}
		}

		public String DeviceId {
			get { return _id; }
			set {
				_id = value;
				RaisePropertyChanged(() => DeviceId);
			}
		}

		public ObservableCollection<ServiceViewModel> Services { get; }

		public string ServiceCount {
			get { return (Device.Services.Count == 0) ? $"No Services" : $"{Device.Services.Count} Services"; }
		}

		public DeviceViewModel(IDevice device) {
			_device = device;
			_adapter = DependencyService.Get<IAdapter>();
			Services = new ObservableCollection<ServiceViewModel>();
			Name = device.Name;
			DeviceId = device.Id.ToString();
			_isDiscoveringServices = false;
		}

		public void Connect() {
			ProcessConnection();
		}

		private void ProcessConnection() {
			if (_device.State == DeviceState.Disconnected) {
				_adapter.ConnectToDevice(_device);
				_adapter.DeviceConnected += AdapterOnDeviceConnected;
			} else if (_device.State == DeviceState.Connected && Services.Count == 0 && !_isDiscoveringServices) {
				_device.ServicesDiscovered += DeviceOnServicesDiscovered;
				_device.DiscoverServices();
				_isDiscoveringServices = true;
			} else if (_device.State == DeviceState.Connected && _isDiscoveringServices && _device.Services != null && _device.Services.Count > 0) {
				RaisePropertyChanged(() => ServiceCount);
				Services.Clear();

				foreach (var service in _device.Services) {
					Services.Add(new ServiceViewModel() {
						Id = service.Uuid
					});
				}
			}
		}

		private void DeviceOnServicesDiscovered(object sender, ServicesDiscoveredEventArgs servicesDiscoveredEventArgs) {
			ProcessConnection();
		}

		public void Disconnect() {
			_device.Disconnect();
		}

		private void AdapterOnDeviceConnected(object sender, DeviceConnectionEventArgs deviceConnectionEventArgs) {
			_device = deviceConnectionEventArgs.Device;
			ProcessConnection();
		}
	}
}
