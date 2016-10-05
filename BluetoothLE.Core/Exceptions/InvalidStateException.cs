using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveBluetooth.Core;

namespace BluetoothLE.Core.Exceptions
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException(ManagerState state)
        {
            State = state;
        }

        public ManagerState State { get; }


    }
}
