using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;

namespace BLE.Dev.Droid {
	[Activity(Label = "BLE.Dev", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity {
		private BattByteTestServer _server;

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);


			global::Xamarin.Forms.Forms.Init(this, bundle);

			DependencyService.Register<BluetoothLE.Core.IAdapter, BluetoothLE.Droid.Adapter>();
			DependencyService.Register<BluetoothLE.Core.Factory.ICharacteristicsFactory, BluetoothLE.Droid.Factory.CharacteristicsFactory>();
			DependencyService.Register<BluetoothLE.Core.Factory.IServiceFactory, BluetoothLE.Droid.Factory.ServiceFactory>();


			_server = new BattByteTestServer();

			LoadApplication(new App());
		}
	}
}

