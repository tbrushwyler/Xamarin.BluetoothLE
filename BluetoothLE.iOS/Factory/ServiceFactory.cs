using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BluetoothLE.Core;
using BluetoothLE.Core.Factory;
using BluetoothLE.iOS;

namespace BluetoothLE.iOS.Factory {
	public class ServiceFactory : IServiceFactory{
		public IService CreateService(Guid uuid, bool isPrimary) {
			return new Service(uuid, isPrimary);
		}
	}
}