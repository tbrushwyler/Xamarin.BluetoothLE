using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
    public class DevicesDiscoveredEventArgs : EventArgs {
        public List<IDevice> DisoveredDevices { get; }

        public DevicesDiscoveredEventArgs(List<IDevice> disoveredDevices) {
            DisoveredDevices = disoveredDevices;
        }
    }
}
