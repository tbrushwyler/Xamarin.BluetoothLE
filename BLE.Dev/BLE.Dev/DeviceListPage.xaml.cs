using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;

namespace BLE.Dev {
	partial class DeviceListPage : ContentPage {
		public DeviceListPage() {
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

		protected override void OnAppearing() {
			base.OnAppearing();
			var model = (DevicePageViewModel)BindingContext;
			model.Refresh();
		}

		protected override void OnDisappearing() {
			base.OnDisappearing();
			var model = (DevicePageViewModel) BindingContext;
			model.Clear();
		}
	}
}
