using System;
using System.Collections.Generic;

using Xamarin.Forms;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using System.Collections.ObjectModel;
using System.Linq;
using BluetoothLE.Example.Models;

namespace BluetoothLE.Example.Pages
{
	public partial class DevicePage : ContentPage
	{
		private readonly IDevice _device;
		public ObservableCollection<Grouping<IService, ICharacteristic>> DiscoveredServices { get; private set; }

		public DevicePage(IDevice device)
		{
			_device = device;
			_device.ServicesDiscovered += ServicesDiscovered;

			DiscoveredServices = new ObservableCollection<Grouping<IService, ICharacteristic>>();

			InitializeComponent();

			BindingContext = _device;
			//serviceListView.ItemsSource = DiscoveredServices;
			//serviceListView.ItemSelected += ServiceSelected;

			App.BluetoothAdapter.DeviceDisconnected += DeviceDisconnected;

			//_device.DiscoverServices();
		}

		void ServiceSelected (object sender, SelectedItemChangedEventArgs e)
		{
			var characteristic = e.SelectedItem as ICharacteristic;
			if (characteristic != null) {
				// todo
			}
		}

		void DeviceDisconnected (object sender, DeviceConnectionEventArgs e)
		{
			// todo: stale
		}

		protected override bool OnBackButtonPressed()
		{
			App.BluetoothAdapter.DeviceDisconnected -= DeviceDisconnected;
			App.BluetoothAdapter.DisconnectDevice(_device);

			return base.OnBackButtonPressed();
		}

		#region IDevice callbacks

		void ServicesDiscovered (object sender, ServicesDiscoveredEventArgs e)
		{
			foreach (var service in e.Services) {
				var grouping = new Grouping<IService, ICharacteristic>(service);

				service.CharacteristicDiscovered += (s, evt) => CharacteristicDiscovered(s, evt, grouping);
				service.DiscoverCharacteristics();

				DiscoveredServices.Add(grouping);
			}
		}

		#endregion

		#region IService callbacks

		void CharacteristicDiscovered (object sender, CharacteristicsDiscoveredEventArgs e, Grouping<IService, ICharacteristic> grouping)
		{
			foreach (var characteristic in e.Characteristics) {
				grouping.Add(characteristic);
			}
			
		}

		#endregion
	}
}

