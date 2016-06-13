using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BLE.Dev {
	public partial class DeviceDetailPage : ContentPage {
		public DeviceDetailPage() {
			InitializeComponent();
			this.ServiceListView.ItemTemplate = new DataTemplate(typeof(ServiceViewCell));
		}

		protected override void OnAppearing() {
			base.OnAppearing();
			var viewModel = (DeviceViewModel) BindingContext;
			viewModel.Connect();
		}

		protected override void OnDisappearing() {
			base.OnDisappearing();
			var viewModel = (DeviceViewModel)BindingContext;
			viewModel.Disconnect();
		}
	}
}
