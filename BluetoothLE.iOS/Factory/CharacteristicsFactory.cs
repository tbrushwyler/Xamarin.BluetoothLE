using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluetoothLE.Core;
using BluetoothLE.Core.Factory;

namespace BluetoothLE.iOS.Factory {
	public class CharacteristicsFactory : ICharacteristicsFactory {
		public ICharacteristic Create(Guid uuid, CharacterisiticPermissionType permissions, CharacteristicPropertyType propeties) {
			return new Characteristic(uuid, permissions, propeties);
		}
	}
}