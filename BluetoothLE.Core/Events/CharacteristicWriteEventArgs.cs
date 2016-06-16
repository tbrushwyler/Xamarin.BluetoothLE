using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
	public class CharacteristicWriteEventArgs : EventArgs{
		public bool Success { get; }
		public ICharacteristic Characteristic { get; }

		public CharacteristicWriteEventArgs(bool success, ICharacteristic characteristic) {
			Success = success;
			Characteristic = characteristic;
		}
	}
}
