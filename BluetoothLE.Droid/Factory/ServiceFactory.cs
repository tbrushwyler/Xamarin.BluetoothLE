using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothLE.Core;
using BluetoothLE.Core.Factory;

namespace BluetoothLE.Droid.Factory {
	public class ServiceFactory : IServiceFactory{
		public IService CreateService(Guid uuid, bool isPrimary) {
			return new Service(uuid, isPrimary);
		}
	}
}