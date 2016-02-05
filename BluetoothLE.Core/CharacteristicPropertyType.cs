using System;

namespace BluetoothLE.Core
{
	/// <summary>
	/// Characteristic property type.
	/// </summary>
	[Flags]
	public enum CharacteristicPropertyType
	{
		/// <summary>
		/// The characteristic allows a broadcast
		/// </summary>
		Broadcast 					= 1 << 0, //0x001

		/// <summary>
		/// The characteristic allows a read.
		/// </summary>
		Read 						= 1 << 1, //0x002

		/// <summary>
		/// The characteristic allows a write without response (Apple-specific)
		/// </summary>
		AppleWriteWithoutResponse 	= 1 << 2, //0x004

		/// <summary>
		/// The characterisitic allows a write without response
		/// </summary>
		WriteWithoutResponse 		= 1 << 3, //0x008

		/// <summary>
		/// The characteristic allows notify
		/// </summary>
		Notify 						= 1 << 4, //0x010

		/// <summary>
		/// The characterisitc allows indicate
		/// </summary>
		Indicate 					= 1 << 5, //0x020

		/// <summary>
		/// The characteristic allows authenticated signed writes
		/// </summary>
		AuthenticatedSignedWrites 	= 1 << 6, //0x040

		/// <summary>
		/// The characteristic has extended properties
		/// </summary>
		ExtendedProperties 			= 1 << 7, //0x080

		/// <summary>
		/// The characterisitic requires notify subscription to be encrypted
		/// </summary>
		NotifyEncryptionRequired 	= 1 << 8, //0x100

		/// <summary>
		/// The characteristic requires indicate subscription to be encrypted
		/// </summary>
		IndicateEncryptionRequired 	= 1 << 9, //0x200
	}
}

