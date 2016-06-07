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
	public class CharacteristicsFactory : ICharacteristicsFactory {
		public ICharacteristic Create(Guid uuid, CharacterisiticPermissionType permissions, CharacteristicPropertyType propeties) {
			return new Characteristic(uuid, permissions, propeties);
		}
	}
}