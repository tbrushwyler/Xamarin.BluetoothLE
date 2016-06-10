using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;

namespace BLE.Dev {
	partial class DevicePage : ContentPage {
		public DevicePage() {
			BindingContext = new DevicePageViewModel();

			InitializeComponent();
			ListView.ItemSelected += ListViewOnItemSelected;
		}

		private void ListViewOnItemSelected(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs) {
			var deviceViewModel = (DeviceViewModel) selectedItemChangedEventArgs.SelectedItem;
			
			var detailPage = new DeviceDetailPage() {
				BindingContext = deviceViewModel
			};
			Navigation.PushAsync(detailPage);
		}
	}
}
