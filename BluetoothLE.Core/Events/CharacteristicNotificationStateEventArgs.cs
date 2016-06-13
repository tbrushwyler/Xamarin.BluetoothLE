﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
	/// <summary>
	/// Called when nofication state changed
	/// </summary>
	public class CharacteristicNotificationStateEventArgs : EventArgs{
		public ICharacteristic Characteristic { get; }

		public CharacteristicNotificationStateEventArgs(ICharacteristic characteristic) {
			Characteristic = characteristic;
		}
	}
}
