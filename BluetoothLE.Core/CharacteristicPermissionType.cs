using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Core {
	[Flags]
	public enum CharacterisiticPermissionType {
		Read,
		Write,
		ReadEncrypted,
		WriteEncrypted,
		ReadEncryptedMitm,
		WriteEncryptedMitm,
		WriteSigned,
		WriteSignedMitm
	}
}
