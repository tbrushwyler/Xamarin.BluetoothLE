using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core.Exceptions {
	

	public class CharacteristicException : Exception{
		public enum Code {
			WriteNotSupported,
			ReadNotSupported,
			WriteFailed,
			ReadFailed
		}

		public Code ErrorCode { get; private set; }

		public CharacteristicException(string message, Code errorCode) : base(message) {
			ErrorCode = errorCode;
		}
	}
}
