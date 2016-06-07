using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
    public enum PeripheralState {
        NotReady,
        Ready
    }

    public class PeripheralEventArgs : EventArgs{
        // Device state

        public PeripheralEventArgs(PeripheralState state) {
            State = state;
        }

        public PeripheralState State { get; }
    }
}
