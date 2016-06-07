using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
	public class RssiUpdateEventArgs : EventArgs{
		public int Rssi { get; private set; }

		public RssiUpdateEventArgs(int rssi) {
			Rssi = rssi;
		}
	}
}
