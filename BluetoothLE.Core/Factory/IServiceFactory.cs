using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Factory {
	public interface IServiceFactory {
		IService CreateService(Guid uuid);
	}
}
