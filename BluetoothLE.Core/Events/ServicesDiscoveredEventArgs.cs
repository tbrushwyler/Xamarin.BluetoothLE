using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
	public class ServicesDiscoveredEventArgs : EventArgs{
		public IList<IService> Services { get; }

		public ServicesDiscoveredEventArgs(IList<IService> services) {
			Services = services;
		}
	}
}
