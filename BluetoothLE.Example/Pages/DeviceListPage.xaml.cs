using System;
using System.Collections.Generic;

using Xamarin.Forms;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using System.Collections.ObjectModel;

namespace BluetoothLE.Example.Pages
{
	public partial class DeviceListPage : ContentPage
	{
		public ObservableCollection<IDevice> DiscoveredDevices { get; private set; }

		public DeviceListPage()
		{
			DiscoveredDevices = new ObservableCollection<IDevice>();

			InitializeComponent();

			deviceListView.ItemsSource = DiscoveredDevices;
			deviceListView.ItemSelected += DeviceSelected;

			App.BluetoothAdapter.DeviceDiscovered += DeviceDiscovered;
			App.BluetoothAdapter.DeviceConnected += DeviceConnected;
			App.BluetoothAdapter.StartScanningForDevices();
		}

		void DeviceSelected (object sender, SelectedItemChangedEventArgs e)
		{
			var device = e.SelectedItem as IDevice;
			if (device != null) {
				App.BluetoothAdapter.ConnectToDevice(device);
			}
		}

		#region BluetoothAdapter callbacks

		void DeviceDiscovered (object sender, DeviceDiscoveredEventArgs e)
		{
			DiscoveredDevices.Add(e.Device);
		}

		void DeviceConnected (object sender, DeviceConnectionEventArgs e)
		{
			Navigation.PushAsync(new DevicePage(e.Device));
		}

		#endregion
	}
}

