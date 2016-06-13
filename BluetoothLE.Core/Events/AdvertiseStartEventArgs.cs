using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
    public enum AdvertiseStatus {
        None,
        InternalError, 
        Unsupported
    }

    public class AdvertiseStartEventArgs : EventArgs {
        public AdvertiseStartEventArgs(AdvertiseStatus advertiseStatus) {
            Status = advertiseStatus;
        }

        /// <summary>
        /// The Status of the advertising, <see cref="AdvertiseStatus.None"/> if no errors
        /// </summary>
        public AdvertiseStatus Status { get; }
    }
}
