using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Events {
	public class DescriptorWriteEventArgs : EventArgs{
		public bool Success { get; }
		public Guid CharacteristicId { get; }
		public Guid DescriptorId { get; }

		public DescriptorWriteEventArgs(bool success, Guid characteristicId, Guid descriptorId) {
			Success = success;
			CharacteristicId = characteristicId;
			DescriptorId = descriptorId;
		}
	}
}
